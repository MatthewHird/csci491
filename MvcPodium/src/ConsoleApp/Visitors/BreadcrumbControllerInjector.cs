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
    public class BreadcrumbControllerInjector : CSharpParserBaseVisitor<object>
    {
        private readonly IStringUtilService _stringUtilService;
        private readonly ICSharpParserService _cSharpParserService;
        private readonly IBreadcrumbCommandParserService _breadcrumbCommandParserService;
        private readonly ICSharpCommonStgService _cSharpCommonStgService;

        private readonly Stack<string> _currentNamespace;
        private readonly Stack<string> _currentClass;

        private readonly Stack<bool> _isControllerClass;
        private readonly Stack<bool> _isClassModified;

        private readonly string _breadcrumbServiceNamespace;
        private readonly string _controllerRootNamespace;
        private readonly string _defaultAreaBreadcrumbServiceRootName;

        public ControllerDictionary ControllerDict { get; private set; }

        public TokenStreamRewriter Rewriter { get; }
        public BufferedTokenStream Tokens { get; }
        public bool IsModified { get; private set; }

        private readonly string _tabString;

        public BreadcrumbControllerInjector(
            IStringUtilService stringUtilService,
            ICSharpParserService cSharpParserService,
            IBreadcrumbCommandParserService breadcrumbCommandParserService,
            ICSharpCommonStgService cSharpCommonStgService,
            BufferedTokenStream tokenStream,
            ControllerDictionary controllerDictionary,
            string breadcrumbServiceNamespace,
            string controllerRootNamespace,
            string defaultAreaBreadcrumbServiceRootName,
            string tabString)
        {
            _stringUtilService = stringUtilService;
            _cSharpParserService = cSharpParserService;
            _breadcrumbCommandParserService = breadcrumbCommandParserService;
            _cSharpCommonStgService = cSharpCommonStgService;
            Tokens = tokenStream;
            Rewriter = new TokenStreamRewriter(tokenStream);
            ControllerDict = controllerDictionary;
            _breadcrumbServiceNamespace = breadcrumbServiceNamespace;
            _controllerRootNamespace = controllerRootNamespace;
            _defaultAreaBreadcrumbServiceRootName = defaultAreaBreadcrumbServiceRootName;
            _tabString = tabString;
            _currentNamespace = new Stack<string>();
            _currentClass = new Stack<string>();
            _isControllerClass = new Stack<bool>();
            _isControllerClass.Push(false);
            _isClassModified = new Stack<bool>();
            _isClassModified.Push(false);
            IsModified = false;
        }

        public override object VisitCompilation_unit([NotNull] CSharpParser.Compilation_unitContext context)
        {
            VisitChildren(context);

            if (IsModified)
            {
                var missingUsingDirectives = _cSharpParserService.GetUsingDirectivesNotInContext(
                context, new List<string>() { _breadcrumbServiceNamespace });

                if (missingUsingDirectives.Count > 0)
                {
                    var usingStopIndex = _cSharpParserService.GetUsingStopIndex(context);

                    var usingDirectiveStr = _cSharpParserService.GenerateUsingDirectives(
                        missingUsingDirectives.ToList(),
                        usingStopIndex.Equals(context.Start));

                    Rewriter.InsertAfter(usingStopIndex, usingDirectiveStr);
                }
            }

            return null;
        }

        public override object VisitNamespace_declaration([NotNull] CSharpParser.Namespace_declarationContext context)
        {
            _currentNamespace.Push(context.qualified_identifier().GetText());
            VisitChildren(context);
            _ = _currentNamespace.Pop();
            return null;
        }

        public override object VisitClass_declaration([NotNull] CSharpParser.Class_declarationContext context)
        {
            _currentClass.Push(context.identifier().GetText());

            var classBaseType = context?.class_base()?.class_type()?.GetText();

            _isControllerClass.Push(classBaseType == "Controller");
            _isClassModified.Push(false);

            VisitChildren(context);

            if (_isClassModified.Peek() && _isControllerClass.Peek())
            {
                var currentNamespace = GetCurrentNamespace();
                var currentClass = GetCurrentClass();
                var controllerClass = ControllerDict.NamespaceDict[currentNamespace].ClassDict[currentClass];

                //public string Namespace { get; set; }
                //public string ControllerRoot { get; set; }
                //public string Controller { get; set; }
                var serviceInterfaceName = GetServiceInterfaceName();
                var controllerServiceIdentifier = "breadcrumbService";

                var fieldDeclaration = new FieldDeclaration()
                {
                    Modifiers = new List<string>() { Keywords.Private, Keywords.Readonly },
                    Type = serviceInterfaceName,
                    VariableDeclarator = new VariableDeclarator() { Identifier = $"_{controllerServiceIdentifier}" }
                };

                var constructorParameter = new FixedParameter()
                {
                    Type = serviceInterfaceName,
                    Identifier = controllerServiceIdentifier
                };

                var constructorAssignment = new SimpleAssignment()
                {
                    LeftHandSide = $"_{controllerServiceIdentifier}",
                    RightHandSide = controllerServiceIdentifier
                };

                var constructorDeclaration = new ConstructorDeclaration()
                {
                    Modifiers = new List<string>() { Keywords.Public },
                    Identifier = currentClass,
                    FormalParameterList = new FormalParameterList()
                    {
                        FixedParameters = new List<FixedParameter>() { constructorParameter }
                    },
                    Body = new ConstructorBody()
                    {
                        Statements = new List<Statement>()
                        {
                            new Statement() { SimpleAssignment = constructorAssignment }
                        }
                    }
                };

                var preclassWhitespace = Tokens.GetHiddenTokensToLeft(context.Start.TokenIndex, Lexer.Hidden);

                int classBodyTabLevels = 1 + ((preclassWhitespace?.Count ?? 0) > 0 ?
                    _stringUtilService.CalculateTabLevels(preclassWhitespace[0]?.Text ?? string.Empty, _tabString) : 0);

                int ctorBodyTabLevels = classBodyTabLevels + 1;

                int? finalConstantOrField = null;
                int? finalField = null;
                int? finalProperty = null;

                CSharpParser.Constructor_declarationContext constructorContext = null;
                bool hasServiceField = false;
                bool hasCtorParam = false;
                bool hasCtorAssignment = false;

                var members = context?.class_body()?.class_member_declarations()?.class_member_declaration();
                if (members != null)
                {
                    foreach (var member in members)
                    {
                        if (member.constant_declaration() != null)
                        {
                            finalConstantOrField = member.constant_declaration().Stop.TokenIndex;
                        }
                        else if (member.field_declaration() != null)
                        {
                            var fieldDec = member.field_declaration();
                            finalField = fieldDec.Stop.TokenIndex;
                            finalConstantOrField = fieldDec.Stop.TokenIndex;
                            if (fieldDec.type_().GetText() == serviceInterfaceName)
                            {
                                foreach (var varDec in fieldDec.variable_declarators().variable_declarator())
                                {
                                    if (varDec.identifier().GetText() ==
                                        $"_{controllerServiceIdentifier}")
                                    {
                                        hasServiceField = true;
                                        break;
                                    }
                                }
                            }
                        }
                        else if (member.property_declaration() != null)
                        {
                            finalProperty = member.property_declaration().Stop.TokenIndex;
                        }
                        else if (member.constructor_declaration() != null)
                        {
                            constructorContext ??= member.constructor_declaration();
                        }
                    }
                }

                int fieldStopIndex = finalField
                    ?? finalConstantOrField
                    ?? context.class_body().OPEN_BRACE().Symbol.TokenIndex;

                int? constructorStopIndex = null;

                var fieldStringBuilder = new StringBuilder();
                StringBuilder constructorStringBuilder = null;

                if (!hasServiceField)
                {
                    fieldStringBuilder.Append(_cSharpParserService.GenerateFieldDeclaration(
                        fieldDeclaration,
                        classBodyTabLevels,
                        _tabString));
                }

                if (constructorContext is null)
                {
                    constructorStopIndex = finalProperty
                        ?? finalConstantOrField
                        ?? fieldStopIndex;

                    constructorStringBuilder = constructorStopIndex == fieldStopIndex
                        ? fieldStringBuilder : new StringBuilder();

                    constructorStringBuilder.Append(
                        _cSharpParserService.GenerateConstructorDeclaration(
                            constructorDeclaration,
                            classBodyTabLevels,
                            _tabString));
                }
                else
                {
                    CSharpParser.Fixed_parameterContext finalFixedParam = null;

                    var formalParamList = constructorContext?.constructor_declarator()?.formal_parameter_list();
                    if (formalParamList != null)
                    {

                        var fixedParams = formalParamList.fixed_parameters();
                        if (fixedParams != null)
                        {
                            foreach (var fixedParam in fixedParams.fixed_parameter())
                            {
                                if (fixedParam.type_().GetText() == serviceInterfaceName
                                    && fixedParam.identifier().GetText() == controllerServiceIdentifier)
                                {
                                    hasCtorParam = true;
                                    break;
                                }
                            }
                            finalFixedParam = fixedParams.fixed_parameter().Last();
                        }
                    }
                    if (!hasCtorParam)
                    {
                        var ctorParam = _cSharpCommonStgService.RenderFixedParameter(constructorParameter);

                        int fixedParamStopIndex = finalFixedParam?.Stop?.TokenIndex
                            ?? constructorContext.constructor_declarator().OPEN_PARENS().Symbol.TokenIndex;

                        var paramStringBuilder = new StringBuilder();
                        if (finalFixedParam != null)
                        {
                            var preFinalParamWhitespace = Tokens.GetHiddenTokensToLeft(
                                finalFixedParam?.Start?.TokenIndex ?? -1, Lexer.Hidden);

                            int finalParamtabs = (preFinalParamWhitespace?.Count ?? 0) > 0 ?
                                _stringUtilService.CalculateTabLevels(
                                    preFinalParamWhitespace?[0]?.Text ?? string.Empty, _tabString) : 0;

                            if (finalParamtabs > 0)
                            {
                                paramStringBuilder.Append(",\r\n");
                                paramStringBuilder.Append(_stringUtilService.TabString(
                                    ctorParam,
                                    finalParamtabs,
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
                                paramStringBuilder.Append(", ");
                                paramStringBuilder.Append(ctorParam);
                            }
                        }
                        else
                        {
                            paramStringBuilder.Append("\r\n");
                            paramStringBuilder.Append(
                                _stringUtilService.TabString(ctorParam, ctorBodyTabLevels, _tabString));
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
                                        "\r\n" +
                                            _stringUtilService.TabString(string.Empty, ctorBodyTabLevels, _tabString));
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

                    string ctorAssignString = _cSharpCommonStgService.RenderSimpleAssignment(constructorAssignment);

                    var constructorBody = constructorContext?.constructor_body();
                    if (constructorBody.SEMICOLON() != null)
                    {
                        IsModified = true;
                        Rewriter.Replace(constructorBody.SEMICOLON().Symbol.TokenIndex, _stringUtilService.TabString(
                            $@"\r\n{{\r\n{_tabString}{ctorAssignString}\r\n}}",
                            classBodyTabLevels,
                            _tabString));
                    }
                    else
                    {
                        var block = constructorBody.block();
                        var statementList = block?.statement_list()?.GetText();
                        if (statementList != null)
                        {
                            var assignmentMatchString =
                                $@"[\s;{{]_{controllerServiceIdentifier}\s*=\s*{controllerServiceIdentifier}\s*;";

                            hasCtorAssignment = Regex.Match(statementList, assignmentMatchString).Success;
                        }

                        if (!hasCtorAssignment)
                        {
                            int? finalAssignment = block.statement_list().statement().Last().Stop.TokenIndex;
                            int assignmentStopIndex = finalAssignment
                                ?? block.OPEN_BRACE().Symbol.TokenIndex;

                            var assignmentStringBuilder = new StringBuilder();

                            assignmentStringBuilder.Append("\r\n");
                            assignmentStringBuilder.Append(_stringUtilService.TabString(
                                ctorAssignString, ctorBodyTabLevels, _tabString));

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
                        Rewriter.InsertAfter(Tokens.Get(constructorStopIndex ?? -1), constructorString);
                    }
                }

            }

            _ = _isClassModified.Pop();
            _ = _isControllerClass.Pop();
            _ = _currentClass.Pop();
            return null;
        }

        public override object VisitMethod_declaration([NotNull] CSharpParser.Method_declarationContext context)
        {
            if (_isControllerClass.Peek())
            {
                bool isNonAction = false;
                bool isPublic = false;

                var attributes = context.method_header()?.attributes()?.attribute_section();
                if (attributes != null)
                {
                    foreach (var attributeSection in attributes)
                    {
                        foreach (var attribute in attributeSection.attribute_list().attribute())
                        {
                            if (attribute.GetText() == "NonAction")
                            {
                                isNonAction = true;
                            }
                        }
                    }
                }

                var modifiers = context.method_header()?.method_modifier();
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

                if (!isNonAction && isPublic)
                {
                    // Add endpoint info to results
                    var currentNamespace = GetCurrentNamespace();
                    var currentClass = GetCurrentClass();
                    var controllerRootName = GetControllerRootName(currentClass);
                    var actionName = context.method_header().member_name().identifier().GetText();
                    bool? hasId = false;

                    var fixedParams = context.method_header()?.formal_parameter_list()
                        ?.fixed_parameters()?.fixed_parameter();
                    if (fixedParams != null)
                    {
                        foreach (var fixedParam in fixedParams)
                        {
                            if (Regex.Match(fixedParam?.type_()?.GetText(), @"^int\??$").Success
                                && fixedParam?.identifier()?.GetText() == "id")
                            {
                                hasId = true;
                            }
                        }
                    }

                    if (ControllerDict.NamespaceDict.ContainsKey(currentNamespace)
                        && ControllerDict.NamespaceDict[currentNamespace]
                            .ClassDict.ContainsKey(currentClass)
                        && ControllerDict.NamespaceDict[currentNamespace]
                            .ClassDict[currentClass].ActionDict.ContainsKey(actionName))
                    {
                        if (!Regex.Match(
                            context.method_body().GetText(), @"ViewData\[""BreadcrumbNode""\]\s*=").Success)
                        {
                            var preMethodWhitespace = Tokens.GetHiddenTokensToLeft(
                                context.Start.TokenIndex, Lexer.Hidden);

                            int tabLevels = 1 + ((preMethodWhitespace?.Count ?? 0) > 0
                                ? _stringUtilService.CalculateTabLevels(
                                    preMethodWhitespace[0]?.Text ?? string.Empty, _tabString)
                                : 0);

                            int openBraceIndex = context.method_body()?.block()?.OPEN_BRACE().Symbol.TokenIndex ?? -1;

                            if (openBraceIndex > -1)
                            {
                                var assignmentString = _breadcrumbCommandParserService.GenerateBreadcrumbAssignment(
                                    controllerRootName, actionName, hasId, tabLevels, _tabString);
                                SetIsClassModifiedToTrue();
                                IsModified = true;
                                Rewriter.InsertAfter(openBraceIndex, assignmentString);
                            }
                        }
                    }

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

        private string GetControllerRootName(string controllerClassName)
        {
            return controllerClassName.EndsWith("Controller")
                ? controllerClassName.Substring(0, controllerClassName.Length - "Controller".Length) 
                : controllerClassName;
        }

        private string GetServiceInterfaceName()
        {
            var namespaceSuffix = Regex.Replace(
                    GetCurrentNamespace(), "^" + Regex.Escape(_controllerRootNamespace + "."), string.Empty);
            var serviceNamePrefix = namespaceSuffix.Replace(".", string.Empty);
            var serviceClassName = serviceNamePrefix == string.Empty
                ? _defaultAreaBreadcrumbServiceRootName
                : serviceNamePrefix + "BreadcrumbService";
            return "I" + serviceClassName;
        }

        private void SetIsClassModifiedToTrue()
        {
            if (!_isClassModified.Peek())
            {
                _ = _isClassModified.Pop();
                _isClassModified.Push(true);
            }
        }
    }
}
