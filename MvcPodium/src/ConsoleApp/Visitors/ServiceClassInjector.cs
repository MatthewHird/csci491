using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using Antlr4.Runtime;
using Antlr4.Runtime.Misc;

using MvcPodium.ConsoleApp.Models.CSharpCommon;
using MvcPodium.ConsoleApp.Models.ServiceCommand;
using MvcPodium.ConsoleApp.Services;

namespace MvcPodium.ConsoleApp.Visitors
{
    public class ServiceClassInjector : CSharpParserBaseVisitor<object>
    {
        private readonly IStringUtilService _stringUtilService;
        private readonly ICSharpParserService _cSharpParserService;
        private readonly IServiceCommandParserService _serviceCommandParserService;

        private readonly string _serviceClassInterfaceName;
        private readonly ServiceFile _serviceFile;
        private readonly string _tabString;

        private bool _hasServiceNamespace;
        private bool _hasServiceClass;
        private bool _hasServiceConstructor;

        private readonly Stack<string> _currentNamespace;
        private readonly Stack<string> _currentClass;

        private readonly Stack<bool> _isCorrectClass;

        private readonly Dictionary<string, FieldDeclaration> _fieldDict;
        private readonly Dictionary<string, FixedParameter> _ctorParamDict;
        private readonly Dictionary<string, SimpleAssignment> _ctorAssignmentDict;
        private readonly HashSet<string> _usingSet;

        public TokenStreamRewriter Rewriter { get; }
        public BufferedTokenStream Tokens { get; }
        public bool IsModified { get; private set; }

        public ServiceClassInjector(
            IStringUtilService stringUtilService,
            ICSharpParserService cSharpParserService,
            IServiceCommandParserService serviceCommandParserService,
            BufferedTokenStream tokenStream,
            string serviceClassInterfaceName,
            ServiceFile serviceFile,
            string tabString = null)
        {
            _stringUtilService = stringUtilService;
            _cSharpParserService = cSharpParserService;
            _serviceCommandParserService = serviceCommandParserService;
            Tokens = tokenStream;
            Rewriter = new TokenStreamRewriter(tokenStream);
            _serviceClassInterfaceName = serviceClassInterfaceName;
            _serviceFile = serviceFile;
            _tabString = tabString;
            _currentNamespace = new Stack<string>();
            _currentClass = new Stack<string>();
            _isCorrectClass = new Stack<bool>();
            _isCorrectClass.Push(false);
            _hasServiceNamespace = false;
            _hasServiceClass = false;
            _hasServiceConstructor = false;
            IsModified = false;

            _usingSet = _serviceFile.UsingDirectives.ToHashSet();

            _ctorParamDict = new Dictionary<string, FixedParameter>();
            foreach (var fixedParam in
                _serviceFile.ServiceDeclaration.Body.ConstructorDeclaration.FormalParameterList.FixedParameters)
            {
                _ctorParamDict.Add($"{fixedParam.Type} {fixedParam.Identifier}", fixedParam);
            }

            _fieldDict = new Dictionary<string, FieldDeclaration>();
            foreach (var fieldDec in _serviceFile.ServiceDeclaration.Body.FieldDeclarations)
            {
                _fieldDict.Add($"{fieldDec.Type} {fieldDec?.VariableDeclarator?.Identifier}", fieldDec);
            }

            _ctorAssignmentDict = new Dictionary<string, SimpleAssignment>();
            var statements = _serviceFile.ServiceDeclaration?.Body?.ConstructorDeclaration?.Body?.Statements;
            if (statements != null)
            {
                foreach (var statement in statements)
                {
                    if (statement.SimpleAssignment != null)
                    {
                        var sa = statement.SimpleAssignment;
                        _ctorAssignmentDict.Add($"{sa.LeftHandSide}={sa.RightHandSide};", sa);
                    }
                }

            }
        }

        public override object VisitCompilation_unit([NotNull] CSharpParser.Compilation_unitContext context)
        {
            var usingDirs = context?.using_directive();
            if (usingDirs != null)
            {
                foreach (var usingDir in usingDirs)
                {
                    _usingSet.Remove(usingDir.using_directive_inner().GetText());
                }
            }

            VisitChildren(context);

            if (_usingSet.Count > 0)
            {
                var usingStopIndex = _cSharpParserService.GetUsingStopIndex(context);

                var usingDirectivesStr = _cSharpParserService.GenerateUsingDirectives(
                    _usingSet.ToList(),
                    usingStopIndex.Equals(context.Start));

                IsModified = true;
                Rewriter.InsertAfter(usingStopIndex, usingDirectivesStr);
            }

            if (!_hasServiceNamespace)
            {
                var namespaceStopIndex = _cSharpParserService.GetNamespaceStopIndex(context);
                var serviceNamespaceDeclaration = _serviceCommandParserService.GenerateServiceNamespaceDeclaration(
                    _serviceFile.ServiceNamespace,
                    _serviceFile.ServiceDeclaration);
                IsModified = true;
                Rewriter.InsertAfter(namespaceStopIndex, serviceNamespaceDeclaration);
            }

            return null;
        }

        public override object VisitNamespace_declaration([NotNull] CSharpParser.Namespace_declarationContext context)
        {
            _currentNamespace.Push(context.qualified_identifier().GetText());
            var currentNamespace = GetCurrentNamespace();
            _usingSet.Remove(currentNamespace);

            VisitChildren(context);
            _ = _currentNamespace.Pop();
            return null;
        }


        public override object VisitNamespace_body([NotNull] CSharpParser.Namespace_bodyContext context)
        {
            var isServiceNamespace = GetCurrentNamespace() == _serviceFile.ServiceNamespace;
            if (isServiceNamespace)
            {
                _hasServiceNamespace = true;
            }

            VisitChildren(context);

            if (!_hasServiceClass && isServiceNamespace)
            {
                var classInterfaceStopIndex = _cSharpParserService.GetClassInterfaceStopIndex(context);

                var preclassWhitespace = Tokens.GetHiddenTokensToLeft(context.Start.TokenIndex, Lexer.Hidden);
                int tabLevels = 1 + ((preclassWhitespace?.Count ?? 0) > 0 ?
                    _stringUtilService.CalculateTabLevels(preclassWhitespace[0]?.Text ?? string.Empty, _tabString) : 0);

                var classDeclaration = _cSharpParserService.GenerateClassInterfaceDeclaration(
                    _serviceFile.ServiceDeclaration,
                    tabLevels,
                    _tabString);
                IsModified = true;
                Rewriter.InsertAfter(classInterfaceStopIndex, classDeclaration);
            }

            return null;
        }


        public override object VisitClass_declaration([NotNull] CSharpParser.Class_declarationContext context)
        {
            _currentClass.Push(context.identifier().GetText());

            var typeParameters = _serviceFile.ServiceDeclaration.TypeParameters;
            var currentNamespace = GetCurrentNamespace();

            bool isTypeParamsMatch = true;
            var typeParams = context?.type_parameter_list()?.type_parameter();
            if (!(typeParams is null && (typeParameters is null || typeParameters.Count == 0)))
            {
                if ((typeParams?.Length ?? 0) != (typeParameters?.Count ?? 0))
                {
                    isTypeParamsMatch = false;
                }
                else
                {
                    for (int i = 0; i < typeParams.Length; ++i)
                    {
                        if (typeParams[i].identifier().GetText() != typeParameters[i].TypeParam)
                        {
                            isTypeParamsMatch = false;
                            break;
                        }
                    }
                }
            }

            if (GetCurrentClass() == _serviceClassInterfaceName
                && currentNamespace == _serviceFile.ServiceNamespace
                && isTypeParamsMatch)
            {
                _isCorrectClass.Push(true);
                _hasServiceClass = true;
            }
            else
            {
                _isCorrectClass.Push(false);
            }

            VisitChildren(context);

            if (_isCorrectClass.Peek())
            {
                _hasServiceClass = true;
                var preclassWhitespace = Tokens.GetHiddenTokensToLeft(context.Start.TokenIndex, Lexer.Hidden);

                int tabLevels = 1 + ((preclassWhitespace?.Count ?? 0) > 0 ?
                    _stringUtilService.CalculateTabLevels(preclassWhitespace[0]?.Text ?? string.Empty, _tabString) : 0);

                int? finalProperty = null;
                int? finalMethod = null;
                int? finalField = null;
                int? finalConstantOrField = null;
                int? finalConstructorOrDestructor = null;

                var members = context?.class_body()?.class_member_declarations()?.class_member_declaration();
                if (members != null)
                {
                    foreach (var member in members)
                    {
                        if (member.method_declaration() != null)
                        {
                            finalMethod = member.method_declaration().Stop.TokenIndex;
                        }
                        else if (member.property_declaration() != null)
                        {
                            finalProperty = member.property_declaration().Stop.TokenIndex;
                        }
                        else if (member.constant_declaration() != null)
                        {
                            finalConstantOrField = member.constant_declaration().Stop.TokenIndex;
                        }
                        else if (member.field_declaration() != null)
                        {
                            finalConstantOrField = member.field_declaration().Stop.TokenIndex;
                            finalField = member.field_declaration().Stop.TokenIndex;
                        }
                        else if (member.constructor_declaration() != null)
                        {
                            finalConstructorOrDestructor = member.constructor_declaration().Stop.TokenIndex;
                        }
                        else if (member.static_constructor_declaration() != null)
                        {
                            finalConstructorOrDestructor = member.static_constructor_declaration().Stop.TokenIndex;
                        }
                        else if (member.destructor_declaration() != null)
                        {
                            finalConstructorOrDestructor = member.destructor_declaration().Stop.TokenIndex;
                        }
                    }

                }

                int fieldStopIndex = finalField
                    ?? finalConstantOrField
                    ?? context.class_body().OPEN_BRACE().Symbol.TokenIndex;

                int? constructorStopIndex = null;
                int? propertyStopIndex = null;
                int? methodStopIndex = null;

                var fieldStringBuilder = new StringBuilder();
                StringBuilder constructorStringBuilder = null;
                StringBuilder propertyStringBuilder = null;
                StringBuilder methodStringBuilder = null;

                if (_fieldDict.Keys.Count > 0)
                {
                    // add fields
                    foreach (var field in _fieldDict.Values)
                    {
                        fieldStringBuilder.Append(_cSharpParserService.GenerateFieldDeclaration(
                            field,
                            tabLevels,
                            _tabString));
                    }
                }

                if (!_hasServiceConstructor)
                {
                    // add ctor
                    constructorStopIndex = finalProperty
                        ?? finalConstantOrField
                        ?? fieldStopIndex;

                    constructorStringBuilder = constructorStopIndex == fieldStopIndex
                        ? fieldStringBuilder : new StringBuilder();

                    constructorStringBuilder.Append(_cSharpParserService.GenerateConstructorDeclaration(
                        _serviceFile.ServiceDeclaration.Body.ConstructorDeclaration,
                        tabLevels,
                        _tabString));
                }

                if (_serviceFile.ServiceDeclaration.Body?.PropertyDeclarations != null 
                    && _serviceFile.ServiceDeclaration.Body.PropertyDeclarations.Count > 0)
                {
                    // add properties
                    propertyStopIndex = finalProperty
                        ?? finalConstructorOrDestructor
                        ?? constructorStopIndex
                        ?? fieldStopIndex;

                    propertyStringBuilder = propertyStopIndex == fieldStopIndex
                        ? fieldStringBuilder : (propertyStopIndex == constructorStopIndex
                            ? constructorStringBuilder : new StringBuilder());

                    propertyStringBuilder.Append(
                        _cSharpParserService.GeneratePropertyDeclarations(
                            _serviceFile.ServiceDeclaration.Body.PropertyDeclarations,
                            tabLevels,
                            _tabString));
                }

                if (_serviceFile.ServiceDeclaration.Body?.MethodDeclarations != null
                    && _serviceFile.ServiceDeclaration.Body.MethodDeclarations.Count > 0)
                {
                    // add methods
                    methodStopIndex = finalMethod
                        ?? finalProperty
                        ?? finalConstructorOrDestructor
                        ?? constructorStopIndex
                        ?? fieldStopIndex;

                    methodStringBuilder = methodStopIndex == fieldStopIndex
                        ? fieldStringBuilder : (methodStopIndex == constructorStopIndex
                            ? constructorStringBuilder : (methodStopIndex == propertyStopIndex
                                ? propertyStringBuilder : new StringBuilder()));

                    methodStringBuilder.Append(
                        _cSharpParserService.GenerateMethodDeclarations(
                            _serviceFile.ServiceDeclaration.Body.MethodDeclarations,
                            tabLevels,
                            _tabString));
                }

                var fieldString = fieldStringBuilder.ToString();
                if (fieldString.Length > 0)
                {
                    IsModified = true;
                    Rewriter.InsertAfter(fieldStopIndex, fieldString);
                }

                if (constructorStringBuilder != null
                    && constructorStopIndex != fieldStopIndex)
                {
                    var constructorString = constructorStringBuilder.ToString();
                    if (constructorString.Length > 0)
                    {
                        IsModified = true;
                        Rewriter.InsertAfter(constructorStopIndex ?? -1, constructorString);
                    }
                }

                if (propertyStringBuilder != null
                    && propertyStopIndex != constructorStopIndex
                    && propertyStopIndex != fieldStopIndex)
                {
                    var propertyString = propertyStringBuilder.ToString();
                    if (propertyString.Length > 0)
                    {
                        IsModified = true;
                        Rewriter.InsertAfter(propertyStopIndex ?? -1, propertyString);
                    }
                }

                if (methodStringBuilder != null
                    && methodStopIndex != constructorStopIndex
                    && methodStopIndex != fieldStopIndex
                    && methodStopIndex != propertyStopIndex)
                {
                    var methodString = methodStringBuilder.ToString();
                    if (methodString.Length > 0)
                    {
                        IsModified = true;
                        Rewriter.InsertAfter(methodStopIndex ?? -1, methodString);
                    }
                }

            }
            _ = _isCorrectClass.Pop();
            _ = _currentClass.Pop();
            return null;
        }

        public override object VisitField_declaration([NotNull] CSharpParser.Field_declarationContext context)
        {
            if (_isCorrectClass.Peek())
            {
                var fieldType = context.type_().GetText();
                var varDecs = context.variable_declarators().variable_declarator();
                foreach (var varDec in varDecs)
                {
                    var fieldName = $"{fieldType} {varDec.identifier().GetText()}";
                    if (_fieldDict.ContainsKey(fieldName))
                    {
                        _fieldDict.Remove(fieldName);
                    }
                }
            }

            VisitChildren(context);
            return null;
        }

        public override object VisitConstructor_declaration(
            [NotNull] CSharpParser.Constructor_declarationContext context)
        {
            if (_isCorrectClass.Peek())
            {
                _hasServiceConstructor = true;

                var preCtorWhitespace = Tokens.GetHiddenTokensToLeft(context.Start.TokenIndex, Lexer.Hidden);

                int tabLevels = 1 + ((preCtorWhitespace?.Count ?? 0) > 0 ?
                    _stringUtilService.CalculateTabLevels(preCtorWhitespace[0]?.Text ?? string.Empty, _tabString) : 0);

                var fixedParams = context?.constructor_declarator()?.formal_parameter_list()?.fixed_parameters()
                    ?.fixed_parameter();

                if (fixedParams != null)
                {
                    foreach (var fixedParam in fixedParams)
                    {
                        var paramName = $"{fixedParam.type_().GetText()} {fixedParam.identifier().GetText()}";
                        if (_ctorParamDict.ContainsKey(paramName))
                        {
                            _ctorParamDict.Remove(paramName);
                        }
                    }
                }

                var ctorStatements = context?.constructor_body()?.block()?.statement_list();
                if (ctorStatements != null)
                {
                    foreach (var statement in ctorStatements.statement())
                    {
                        var minStatement = _cSharpParserService.GetTextWithWhitespaceMinified(Tokens, statement);
                        if (_ctorAssignmentDict.ContainsKey(minStatement))
                        {
                            _ctorAssignmentDict.Remove(minStatement);
                        }
                    }
                }

                if (_ctorParamDict.Keys.Count > 0)
                {
                    // add params
                    var ctorParams = _ctorParamDict.Values.ToList();

                    var formalParamList = context?.constructor_declarator()?.formal_parameter_list();

                    var finalFixedParam = context?.constructor_declarator()
                        ?.formal_parameter_list()
                        ?.fixed_parameters()
                        ?.fixed_parameter()
                        ?.Last();

                    int fixedParamStopIndex = finalFixedParam?.Stop?.TokenIndex
                        ?? context.constructor_declarator().OPEN_PARENS().Symbol.TokenIndex;

                    var paramStringBuilder = new StringBuilder();
                    if (finalFixedParam != null)
                    {
                        var preFinalParamWhitespace = Tokens.GetHiddenTokensToLeft(
                            finalFixedParam?.Start?.TokenIndex ?? -1, Lexer.Hidden);

                        int finalParamtabs = (preFinalParamWhitespace?.Count ?? 0) > 0 ?
                            _stringUtilService.CalculateTabLevels(preFinalParamWhitespace[0]?.Text ?? string.Empty, _tabString) : 0;

                        if (finalParamtabs > 0)
                        {
                            paramStringBuilder.Append(_cSharpParserService.GenerateFixedParameters(
                                ctorParams,
                                tabLevels,
                                _tabString));

                            if (formalParamList?.parameter_array() != null)
                            {
                                var preParamArrayWhitespaceArray = Tokens.GetHiddenTokensToLeft(
                                     formalParamList.parameter_array().Start.TokenIndex, Lexer.Hidden);

                                string preParamArrayWhitespace =
                                    preParamArrayWhitespaceArray.Count > 0 ?
                                    preParamArrayWhitespaceArray[0].Text : string.Empty;

                                if (!Regex.IsMatch(preParamArrayWhitespace, @"\r?\n"))
                                {
                                    IsModified = true;
                                    Rewriter.InsertBefore(
                                        formalParamList.parameter_array().Start.TokenIndex,
                                        "\r\n" +
                                            _stringUtilService.TabString(string.Empty, finalParamtabs, _tabString));
                                }
                            }
                        }
                        else
                        {
                            paramStringBuilder.Append(_cSharpParserService.GenerateFixedParameters(
                                ctorParams,
                                tabLevels,
                                _tabString,
                                false,
                                true));
                        }
                    }
                    else
                    {
                        paramStringBuilder.Append(_cSharpParserService.GenerateFixedParameters(
                                ctorParams,
                                tabLevels,
                                _tabString,
                                true));

                        if (formalParamList?.parameter_array() != null)
                        {
                            var preParamArrayWhitespaceArray = Tokens.GetHiddenTokensToLeft(
                                     formalParamList.parameter_array().Start.TokenIndex, Lexer.Hidden);

                            string preParamArrayWhitespace =
                                preParamArrayWhitespaceArray.Count > 0 ?
                                preParamArrayWhitespaceArray[0].Text : string.Empty;

                            if (!Regex.IsMatch(preParamArrayWhitespace, $"\r?\n"))
                            {
                                IsModified = true;
                                Rewriter.InsertBefore(
                                    formalParamList.parameter_array().Start.TokenIndex,
                                    "\r\n" + _stringUtilService.TabString(string.Empty, tabLevels, _tabString));
                            }
                        }
                    }

                    var paramString = paramStringBuilder.ToString();
                    if (paramString.Length > 0)
                    {
                        IsModified = true;
                        Rewriter.InsertAfter(fixedParamStopIndex, paramString);
                    }
                }

                if (_ctorAssignmentDict.Keys.Count > 0)
                {
                    var ctorAssignments = _ctorAssignmentDict.Values.ToList();

                    var assignmentsString = _cSharpParserService.GenerateSimpleAssignments(
                        ctorAssignments,
                        tabLevels,
                        _tabString);

                    var constructorBody = context?.constructor_body();
                    if (constructorBody.SEMICOLON() != null)
                    {
                        var conBodyBuilder = new StringBuilder();

                        conBodyBuilder.Append("\r\n");
                        conBodyBuilder.Append(_stringUtilService.TabString("{", tabLevels - 1, _tabString));
                        conBodyBuilder.Append(assignmentsString);
                        conBodyBuilder.Append("\r\n");
                        conBodyBuilder.Append(_stringUtilService.TabString("}", tabLevels - 1, _tabString));
                        IsModified = true;
                        Rewriter.Replace(constructorBody.SEMICOLON().Symbol.TokenIndex, conBodyBuilder.ToString());
                    }
                    else
                    {

                        var block = constructorBody.block();

                        int? finalAssignment = block.statement_list()?.statement()?.Last()?.Stop?.TokenIndex;
                        int assignmentStopIndex = finalAssignment
                            ?? block.OPEN_BRACE().Symbol.TokenIndex;

                        var assignmentStringBuilder = new StringBuilder();
                        assignmentStringBuilder.Append(assignmentsString);

                        var postAssignmentStopWhitespaceArray = Tokens.GetHiddenTokensToRight(
                            assignmentStopIndex, Lexer.Hidden);

                        string postAssignmentStopWhitespace =
                            postAssignmentStopWhitespaceArray.Count > 0 ?
                            postAssignmentStopWhitespaceArray[0].Text : string.Empty;

                        if (!Regex.IsMatch(postAssignmentStopWhitespace, @"\r?\n"))
                        {
                            assignmentStringBuilder.Append("\r\n");
                        }

                        var assignmentString = assignmentStringBuilder.ToString();
                        if (assignmentString.Length > 0)
                        {
                            IsModified = true;
                            Rewriter.InsertAfter(assignmentStopIndex, assignmentString);
                        }
                    }
                }
            }

            VisitChildren(context);
            return null;
        }

        private string GetCurrentNamespace()
        {
            return string.Join(".", _currentNamespace.ToArray().Reverse());
        }

        private string GetCurrentClass()
        {
            return string.Join(".", _currentClass.ToArray().Reverse());
        }

    }
}
