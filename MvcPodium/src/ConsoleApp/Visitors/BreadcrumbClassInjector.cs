using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using Antlr4.Runtime;
using Antlr4.Runtime.Misc;

using MvcPodium.ConsoleApp.Constants.CSharpGrammar;
using MvcPodium.ConsoleApp.Models.BreadcrumbCommand;
using MvcPodium.ConsoleApp.Models.CSharpCommon;
using MvcPodium.ConsoleApp.Services;

namespace MvcPodium.ConsoleApp.Visitors
{
    public class BreadcrumbClassInjector : CSharpParserBaseVisitor<object>
    {
        private readonly IStringUtilService _stringUtilService;
        private readonly ICSharpParserService _cSharpParserService;
        private readonly IBreadcrumbCommandParserService _breadcrumbCommandParserService;

        private readonly Stack<string> _currentNamespace;
        private readonly Stack<string> _currentClass;

        private bool _hasBreadcrumbNamespace;
        private bool _hasBreadcrumbClass;
        private bool _hasBreadcrumbConstructor;

        //private readonly Stack<bool> _isControllerClass;

        private readonly string _breadcrumbNamespace;
        private readonly BreadcrumbServiceDeclaration _breadcrumbDeclaration;
        
        private readonly string _tabString;

        private readonly Dictionary<string, BreadcrumbMethodDeclaration> _methodDictionary;

        private readonly Dictionary<string, FieldDeclaration> _fieldDict;
        private readonly Dictionary<string, FixedParameter> _ctorParamDict;
        private readonly Dictionary<string, SimpleAssignment> _ctorAssignmentDict;
        private readonly HashSet<string> _usingSet;

        public TokenStreamRewriter Rewriter { get; }
        public BufferedTokenStream Tokens { get; }
        public bool IsModified { get; private set; }

        public BreadcrumbClassInjector(
            IStringUtilService stringUtilService,
            ICSharpParserService cSharpParserService,
            IBreadcrumbCommandParserService breadcrumbCommandParserService,
            BufferedTokenStream tokenStream,
            List<string> usingDirectives,
            string breadcrumbNamespace,
            BreadcrumbServiceDeclaration breadcrumbDeclaration,
            string tabString)
        {
            _stringUtilService = stringUtilService;
            _cSharpParserService = cSharpParserService;
            _breadcrumbCommandParserService = breadcrumbCommandParserService;
            Tokens = tokenStream;
            Rewriter = new TokenStreamRewriter(tokenStream);
            _breadcrumbNamespace = breadcrumbNamespace;
            _breadcrumbDeclaration = breadcrumbDeclaration;
            _tabString = tabString;
            _currentNamespace = new Stack<string>();
            _currentClass = new Stack<string>();
            _hasBreadcrumbNamespace = false;
            _hasBreadcrumbClass = false;
            _hasBreadcrumbConstructor = false;
            IsModified = false;

            _methodDictionary = new Dictionary<string, BreadcrumbMethodDeclaration>();
            foreach (var method in breadcrumbDeclaration.Body.MethodDeclarations)
            {
                _methodDictionary.Add(GetActionMethodName(method.ControllerRoot, method.Action), method);
            }

            _usingSet = usingDirectives.ToHashSet();

            _ctorParamDict = new Dictionary<string, FixedParameter>();
            foreach (var fixedParam in
                _breadcrumbDeclaration.Body.ConstructorDeclaration.FormalParameterList.FixedParameters)
            {
                _ctorParamDict.Add($"{fixedParam.Type} {fixedParam.Identifier}", fixedParam);
            }

            _fieldDict = new Dictionary<string, FieldDeclaration>();
            foreach (var fieldDec in _breadcrumbDeclaration.Body.FieldDeclarations)
            {
                _fieldDict.Add($"{fieldDec.Type} {fieldDec?.VariableDeclarator?.Identifier}", fieldDec);
            }

            _ctorAssignmentDict = new Dictionary<string, SimpleAssignment>();
            var statements = _breadcrumbDeclaration?.Body?.ConstructorDeclaration?.Body?.Statements;
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
                    var usingInner = usingDir.using_directive_inner().GetText();
                    if (_usingSet.Contains(usingInner))
                    {
                        _usingSet.Remove(usingInner);
                    }
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


            if (!_hasBreadcrumbNamespace)
            {
                var namespaceStopIndex = _cSharpParserService.GetNamespaceStopIndex(context);
                var breadcrumbNamespaceDeclaration = _breadcrumbCommandParserService.GenerateBreadcrumbNamespaceDeclaration(
                    _breadcrumbNamespace,
                    _breadcrumbDeclaration);
                IsModified = true;
                Rewriter.InsertAfter(namespaceStopIndex, breadcrumbNamespaceDeclaration);
            }

            return null;
        }

        public override object VisitNamespace_declaration([NotNull] CSharpParser.Namespace_declarationContext context)
        {
            _currentNamespace.Push(context.qualified_identifier().GetText());

            var isBreadcrumbNamespace = false;
            if (GetCurrentNamespace() == _breadcrumbNamespace)
            {
                _usingSet.Remove(GetCurrentNamespace());
                _hasBreadcrumbNamespace = true;
                isBreadcrumbNamespace = true;
            }

            VisitChildren(context);

            if (isBreadcrumbNamespace)
            {
                if (!_hasBreadcrumbClass)
                {
                    var classStopIndex = _cSharpParserService.GetClassInterfaceStopIndex(context.namespace_body());

                    var prenamespaceWhitespace = Tokens.GetHiddenTokensToLeft(context.Start.TokenIndex, Lexer.Hidden);

                    int tabLevels = 1 + ((prenamespaceWhitespace?.Count ?? 0) > 0 ?
                        _stringUtilService.CalculateTabLevels(prenamespaceWhitespace[0]?.Text ?? string.Empty, _tabString) : 0);

                    var breadcrumbClassString = _breadcrumbCommandParserService.GenerateBreadcrumbClassInterfaceDeclaration(
                        _breadcrumbDeclaration,
                        tabLevels,
                        _tabString);

                    IsModified = true;
                    Rewriter.InsertAfter(classStopIndex, breadcrumbClassString);
                }
            }

            _ = _currentNamespace.Pop();
            return null;
        }

        public override object VisitClass_declaration([NotNull] CSharpParser.Class_declarationContext context)
        {
            _currentClass.Push(context.identifier().GetText());

            var isBreadcrumbClass = false;
            if (GetCurrentNamespace() == _breadcrumbNamespace && GetCurrentClass() == _breadcrumbDeclaration.Identifier)
            {
                _hasBreadcrumbClass = true;
                isBreadcrumbClass = true;
            }

            VisitChildren(context);

            if (isBreadcrumbClass)
            {
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
                int? methodStopIndex = null;

                var fieldStringBuilder = new StringBuilder();
                StringBuilder constructorStringBuilder = null;
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

                if (!_hasBreadcrumbConstructor)
                {
                    // add ctor
                    constructorStopIndex = finalProperty
                        ?? finalConstantOrField
                        ?? fieldStopIndex;

                    constructorStringBuilder = constructorStopIndex == fieldStopIndex
                        ? fieldStringBuilder : new StringBuilder();

                    constructorStringBuilder.Append(_cSharpParserService.GenerateConstructorDeclaration(
                        _breadcrumbDeclaration.Body.ConstructorDeclaration,
                        tabLevels,
                        _tabString));
                }

                if (_methodDictionary.Keys.Count > 0)
                {
                    // add methods
                    methodStopIndex = finalMethod
                        ?? finalConstructorOrDestructor
                        ?? constructorStopIndex
                        ?? fieldStopIndex;

                    methodStringBuilder = methodStopIndex == fieldStopIndex
                        ? fieldStringBuilder : (methodStopIndex == constructorStopIndex
                            ? constructorStringBuilder : new StringBuilder());

                    methodStringBuilder.Append(_breadcrumbCommandParserService.GenerateBreadcrumbMethodDeclarations(
                        _methodDictionary.Values.ToList(),
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

                if (methodStringBuilder != null
                    && methodStopIndex != constructorStopIndex
                    && methodStopIndex != fieldStopIndex)
                {
                    var methodString = methodStringBuilder.ToString();
                    if (methodString.Length > 0)
                    {
                        IsModified = true;
                        Rewriter.InsertAfter(methodStopIndex ?? -1, methodString);
                    }
                }
            }

            _ = _currentClass.Pop();
            return null;
        }

        public override object VisitField_declaration([NotNull] CSharpParser.Field_declarationContext context)
        {
            if (GetCurrentNamespace() == _breadcrumbNamespace && GetCurrentClass() == _breadcrumbDeclaration.Identifier)
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
            if (GetCurrentNamespace() == _breadcrumbNamespace && GetCurrentClass() == _breadcrumbDeclaration.Identifier)
            {
                _hasBreadcrumbConstructor = true;

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

        public override object VisitMethod_header([NotNull] CSharpParser.Method_headerContext context)
        {
            bool isPublic = false;

            var modifiers = context?.method_modifier();
            if (modifiers != null)
            {
                foreach (var modifier in modifiers)
                {
                    if (modifier.GetText() == Keywords.Public)
                    {
                        isPublic = true;
                    }
                }
            }

            if (GetCurrentNamespace() == _breadcrumbNamespace
                && GetCurrentClass() == _breadcrumbDeclaration.Identifier
                && isPublic)
            {
                var methodName = context.member_name().identifier().GetText();
                if (_methodDictionary.ContainsKey(methodName))
                {
                    _methodDictionary.Remove(methodName);
                }
            }

            VisitChildren(context);

            return null;
        }

        private string GetCurrentClass()
        {
            return string.Join(".", _currentClass.ToArray().Reverse());
        }

        private string GetCurrentNamespace()
        {
            return string.Join(".", _currentNamespace.ToArray().Reverse());
        }

        private string GetActionMethodName(string controllerRoot, string action)
        {
            return $"{controllerRoot}{action}Breadcrumb";
        }

    }
}
