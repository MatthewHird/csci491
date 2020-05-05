using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using MvcPodium.ConsoleApp.Models.CSharpCommon;
using MvcPodium.ConsoleApp.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MvcPodium.ConsoleApp.Visitors
{
    public class ServiceConstructorInjector : CSharpParserBaseVisitor<object>
    {
        private readonly IStringUtilService _stringUtilService;
        private readonly ICSharpParserService _cSharpParserService;
        private readonly ICSharpCommonStgService _cSharpCommonStgService;
        private readonly Stack<string> _currentNamespace;

        private readonly string _constructorClassName;
        private readonly string _constructorClassNamespace;
        private readonly string _serviceIdentifier;
        private readonly string _serviceNamespace;
        private readonly string _serviceInterfaceType;

        private readonly FieldDeclaration _fieldDeclaration;
        private readonly FixedParameter _constructorParameter;
        private readonly SimpleAssignment _constructorAssignment;
        private readonly ConstructorDeclaration _constructorDeclaration;
        private readonly string _tabString;

        public bool IsServiceInjected { get; private set; }

        public TokenStreamRewriter Rewriter { get; }
        public BufferedTokenStream Tokens { get; }

        public ServiceConstructorInjector(
            IStringUtilService stringUtilService,
            ICSharpParserService cSharpParserService,
            ICSharpCommonStgService cSharpCommonStgService,
            BufferedTokenStream tokenStream,
            string constructorClassName,
            string constructorClassNamespace,
            string serviceIdentifier,
            string serviceNamespace,
            string serviceInterfaceType,
            FieldDeclaration fieldDeclaration,
            FixedParameter constructorParameter,
            SimpleAssignment constructorAssignment,
            ConstructorDeclaration constructorDeclaration,
            string tabString = null)
        {
            _stringUtilService = stringUtilService;
            _cSharpParserService = cSharpParserService;
            _cSharpCommonStgService = cSharpCommonStgService;
            Tokens = tokenStream;
            Rewriter = new TokenStreamRewriter(tokenStream);
            _constructorClassName = constructorClassName;
            _constructorClassNamespace = constructorClassNamespace;
            _serviceIdentifier = serviceIdentifier;
            _serviceNamespace = serviceNamespace;
            _serviceInterfaceType = serviceInterfaceType;
            _fieldDeclaration = fieldDeclaration;
            _constructorParameter = constructorParameter;
            _constructorAssignment = constructorAssignment;
            _constructorDeclaration = constructorDeclaration;
            _tabString = tabString;
            _currentNamespace = new Stack<string>();
        }

        public override object VisitCompilation_unit([NotNull] CSharpParser.Compilation_unitContext context)
        {
            IsServiceInjected = false;

            bool isUsingServiceNamespace = _cSharpParserService.IsUsingDirectiveInContext(
                context,
                _serviceNamespace);

            if (!isUsingServiceNamespace)
            {
                var usingStopIndex = _cSharpParserService.GetUsingStopIndex(context);

                var usingDirectiveStr = _cSharpParserService.GenerateUsingDirective(
                    _serviceNamespace,
                    usingStopIndex.Equals(context.Start));

                Rewriter.InsertAfter(usingStopIndex, usingDirectiveStr);
            }

            VisitChildren(context);
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
            var currentNamespace = string.Join(".", _currentNamespace.ToArray().Reverse());

            if (context.identifier().GetText() == _constructorClassName 
                && currentNamespace == _constructorClassNamespace)
            {
                var preclassWhitespace = Tokens.GetHiddenTokensToLeft(context.Start.TokenIndex, Lexer.Hidden);

                int classBodyTabLevels = 1 + (preclassWhitespace.Count > 0 ?
                    _stringUtilService.CalculateTabLevels(preclassWhitespace[0].Text, _tabString) : 0);

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
                            if (fieldDec.type_().GetText() == _serviceInterfaceType)
                            {
                                foreach (var varDec in fieldDec.variable_declarators().variable_declarator())
                                {
                                    if (varDec.identifier().GetText() == 
                                        $"_{_serviceIdentifier}")
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
                    fieldStringBuilder.Append("\r\n");
                    fieldStringBuilder.Append(
                        _stringUtilService.TabString(
                            _cSharpCommonStgService.RenderFieldDeclaration(_fieldDeclaration),
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

                    constructorStringBuilder.Append("\r\n");
                    constructorStringBuilder.Append("\r\n");
                    constructorStringBuilder.Append(
                        _stringUtilService.TabString(
                            _cSharpCommonStgService.RenderConstructorDeclaration(_constructorDeclaration),
                            classBodyTabLevels,
                            _tabString));
                    constructorStringBuilder.Append("\r\n");
                }
                else
                {
                    int? finalFixedParam = null;

                    var formalParamList = constructorContext?.constructor_declarator()?.formal_parameter_list();
                    if (formalParamList != null)
                    {

                        var fixedParams = formalParamList.fixed_parameters();
                        if (fixedParams != null)
                        {
                            foreach (var fixedParam in fixedParams.fixed_parameter())
                            {
                                if (fixedParam.type_().GetText() == _serviceInterfaceType
                                    && fixedParam.identifier().GetText() == _serviceIdentifier)
                                {
                                    hasCtorParam = true;
                                    break;
                                }
                            }
                            finalFixedParam = fixedParams.fixed_parameter().Last().Stop.TokenIndex;
                        }
                    }
                    if (!hasCtorParam)
                    {
                        var ctorParam = _cSharpCommonStgService.RenderFixedParameter(_constructorParameter);

                        int fixedParamStopIndex = finalFixedParam
                            ?? constructorContext.constructor_declarator().OPEN_PARENS().Symbol.TokenIndex;
                        
                        var paramStringBuilder= new StringBuilder();
                        if (finalFixedParam != null)
                        {
                            var preFinalParamWhitespace = Tokens.GetHiddenTokensToLeft(
                                finalFixedParam ?? -1, Lexer.Hidden);

                            int finalParamtabs = preFinalParamWhitespace.Count > 0 ?
                                _stringUtilService.CalculateTabLevels(preFinalParamWhitespace[0].Text, _tabString) : 0;
                            
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
                                        Rewriter.InsertBefore(
                                            fieldStopIndex,
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
                                    Rewriter.InsertBefore(
                                        fieldStopIndex,
                                        "\r\n" + 
                                            _stringUtilService.TabString(string.Empty, ctorBodyTabLevels, _tabString));
                                }
                            }
                        }

                        var paramString = paramStringBuilder.ToString();
                        if (paramString.Length > 0)
                        {
                            Rewriter.InsertAfter(fixedParamStopIndex, paramString);
                        }
                    }

                    string ctorAssignString = _cSharpCommonStgService.RenderSimpleAssignment(_constructorAssignment);

                    var constructorBody = constructorContext?.constructor_body();
                    if (constructorBody.SEMICOLON() != null)
                    {
                        Rewriter.Replace(constructorBody.SEMICOLON().Symbol.TokenIndex, _stringUtilService.TabString(
                            $@"\r\n{{\r\n    {ctorAssignString}\r\n}}",
                            classBodyTabLevels,
                            _tabString));
                    }
                    else
                    {
                        int? finalAssignment = null;

                        var block = constructorBody.block();
                        var statementList = block?.statement_list()?.GetText();
                        if (statementList != null)
                        {
                            var assignmentMatchString = 
                                $@"[\s;{{]_{_serviceIdentifier}\s*=\s*{_serviceIdentifier}\s*;";

                            hasCtorAssignment = Regex.Match(statementList, assignmentMatchString).Success;
                        }

                        if (!hasCtorAssignment)
                        {
                            finalAssignment = block.statement_list().statement().Last().Stop.TokenIndex;
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
                                Rewriter.InsertAfter(assignmentStopIndex, assignmentString);
                            }
                        }
                    }
                }

                var fieldString = fieldStringBuilder.ToString();
                if (fieldString.Length > 0)
                {
                    Rewriter.InsertAfter(fieldStopIndex, fieldString);
                }

                if (constructorStringBuilder != null
                    && constructorStopIndex != fieldStopIndex)
                {
                    var constructorString = constructorStringBuilder.ToString();
                    if (constructorString.Length > 0)
                    {
                        Rewriter.InsertAfter(Tokens.Get(constructorStopIndex ?? -1), constructorString);
                    }
                }

                IsServiceInjected = hasServiceField && hasCtorParam && hasCtorAssignment;
            }
            VisitChildren(context);
            return null;
        }
    }
}
