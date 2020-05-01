using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using MvcPodium.ConsoleApp.Model;
using MvcPodium.ConsoleApp.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using IToken = Antlr4.Runtime.IToken;

namespace MvcPodium.ConsoleApp.Visitors
{
    public class ServiceConstructorInjector : CSharpParserBaseVisitor<object>
    {
        private readonly IStringUtilService _stringUtilService;
        private readonly Stack<string> _currentNamespace;

        private readonly ServiceConstructionInjectorArguments _constructionInjectionArgs;

        public bool IsServiceInjected { get; private set; }

        public TokenStreamRewriter Rewriter { get; }
        public BufferedTokenStream Tokens { get; }

        public ServiceConstructorInjector(
 BufferedTokenStream tokenStream,
            ServiceConstructionInjectorArguments constructionInjectionArgs,
            IStringUtilService stringUtilService)
        {
            _stringUtilService = stringUtilService;
            Tokens = tokenStream;
            Rewriter = new TokenStreamRewriter(tokenStream);
            _constructionInjectionArgs = constructionInjectionArgs;
            _currentNamespace = new Stack<string>();
        }

        public override object VisitCompilation_unit([NotNull] CSharpParser.Compilation_unitContext context)
        {
            IsServiceInjected = false;

            bool isUsingServiceNamespace = false;

            var usingDirectives = context?.using_directive();

            foreach (var usingDirective in usingDirectives)
            {
                if (_stringUtilService.MinifyString(usingDirective?.using_directive_inner().GetText()) 
                    == _constructionInjectionArgs.ServiceNamespace)
                {
                    isUsingServiceNamespace = true;
                    break;
                }
            }

            if (!isUsingServiceNamespace)
            {
                IToken usingStopIndex = context?.using_directive()?.LastOrDefault()?.Stop
                    ?? context?.extern_alias_directive()?.LastOrDefault()?.Stop
                    ?? context?.BYTE_ORDER_MARK()?.Symbol
                    ?? context.Start;

                var usingDirective = (usingStopIndex.Equals(context.Start) ? "" : "\r\n\r\n")
                    + $"using {_constructionInjectionArgs.ServiceNamespace};"
                    + (usingStopIndex.Equals(context.Start) ? "\r\n" : "");

                Rewriter.InsertAfter(usingStopIndex, usingDirective);
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

            if (context.identifier().GetText() == _constructionInjectionArgs.ConstructorClassName 
                && currentNamespace == _constructionInjectionArgs.ConstructorClassNamespace)
            {
                string tabString = "    ";
                var preclassWhitespace = Tokens.GetHiddenTokensToLeft(context.Start.TokenIndex, Lexer.Hidden);

                int classBodyTabLevels = 1 + (preclassWhitespace.Count > 0 ?
                    _stringUtilService.CalculateTabLevels(preclassWhitespace[0].Text, tabString) : 0);

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
                            if (fieldDec.type_().GetText() == _constructionInjectionArgs.ServiceInterfaceType)
                            {
                                foreach (var varDec in fieldDec.variable_declarators().variable_declarator())
                                {
                                    if (varDec.identifier().GetText() == 
                                        $"_{_constructionInjectionArgs.ServiceIdentifier}")
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
                            _constructionInjectionArgs.FieldDeclaration, classBodyTabLevels, tabString));
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
                            _constructionInjectionArgs.ConstructorDeclaration, classBodyTabLevels, tabString));
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
                                if (fixedParam.type_().GetText() == _constructionInjectionArgs.ServiceInterfaceType
                                    && fixedParam.identifier().GetText() == 
                                        _constructionInjectionArgs.ServiceIdentifier)
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
                        int fixedParamStopIndex = finalFixedParam
                            ?? constructorContext.constructor_declarator().OPEN_PARENS().Symbol.TokenIndex;
                        
                        var paramStringBuilder= new StringBuilder();
                        if (finalFixedParam != null)
                        {
                            var preFinalParamWhitespace = Tokens.GetHiddenTokensToLeft(
                                finalFixedParam ?? -1, Lexer.Hidden);

                            int finalParamtabs = preFinalParamWhitespace.Count > 0 ?
                                _stringUtilService.CalculateTabLevels(preFinalParamWhitespace[0].Text, tabString) : 0;

                            if (finalParamtabs > 0)
                            {
                                paramStringBuilder.Append(",\r\n");
                                paramStringBuilder.Append(_stringUtilService.TabString(
                                    _constructionInjectionArgs.ConstructorParameter, finalParamtabs, tabString));
                                if (formalParamList?.parameter_array() != null)
                                {
                                    var preParamArrayWhitespaceArray = Tokens.GetHiddenTokensToLeft(
                                         formalParamList.parameter_array().Start.TokenIndex, Lexer.Hidden);

                                    string preParamArrayWhitespace =
                                        preParamArrayWhitespaceArray.Count > 0 ?
                                        preParamArrayWhitespaceArray[0].Text : "";

                                    if (!Regex.IsMatch(preParamArrayWhitespace, $"\r?\n"))
                                    {
                                        Rewriter.InsertBefore(
                                            fieldStopIndex,
                                            $"\r\n{_stringUtilService.TabString("", finalParamtabs, tabString)}");
                                    }
                                }
                            }
                            else
                            {
                                paramStringBuilder.Append(", ");
                                paramStringBuilder.Append(_constructionInjectionArgs.ConstructorParameter);
                            }

                        }
                        else
                        {
                            paramStringBuilder.Append("\r\n");
                            paramStringBuilder.Append(
                                _stringUtilService.TabString(
                                    _constructionInjectionArgs.ConstructorParameter, ctorBodyTabLevels, tabString));
                            if (formalParamList?.parameter_array() != null)
                            {
                                var preParamArrayWhitespaceArray = Tokens.GetHiddenTokensToLeft(
                                         formalParamList.parameter_array().Start.TokenIndex, Lexer.Hidden);

                                string preParamArrayWhitespace =
                                    preParamArrayWhitespaceArray.Count > 0 ?
                                    preParamArrayWhitespaceArray[0].Text : "";

                                if (!Regex.IsMatch(preParamArrayWhitespace, $"\r?\n"))
                                {
                                    Rewriter.InsertBefore(
                                        fieldStopIndex,
                                        $"\r\n{_stringUtilService.TabString("", ctorBodyTabLevels, tabString)}");
                                }
                            }
                        }

                        var paramString = paramStringBuilder.ToString();
                        if (paramString.Length > 0)
                        {
                            Rewriter.InsertAfter(fixedParamStopIndex, paramString);
                        }

                    }
                    
                    var constructorBody = constructorContext?.constructor_body();
                    if (constructorBody.SEMICOLON() != null)
                    {
                        Rewriter.Replace(constructorBody.SEMICOLON().Symbol.TokenIndex, _stringUtilService.TabString(
                            $@"\r\n{{\r\n    {_constructionInjectionArgs.ConstructorAssignment}\r\n}}",
                            classBodyTabLevels,
                            tabString));
                    }
                    else
                    {
                        int? finalAssignment = null;

                        var block = constructorBody.block();
                        var statementList = block?.statement_list()?.GetText();
                        if (statementList != null)
                        {
                            var assignmentMatchString = $@"[\s;{{]_{_constructionInjectionArgs.ServiceIdentifier}" +
                                $@"\s*=\s*{_constructionInjectionArgs.ServiceIdentifier}\s*;";

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
                                _constructionInjectionArgs.ConstructorAssignment, ctorBodyTabLevels, tabString));

                            var postAssignmentStopWhitespaceArray = Tokens.GetHiddenTokensToRight(
                                assignmentStopIndex, Lexer.Hidden);

                            string postAssignmentStopWhitespace =
                                postAssignmentStopWhitespaceArray.Count > 0 ?
                                postAssignmentStopWhitespaceArray[0].Text : "";

                            if (!Regex.IsMatch(postAssignmentStopWhitespace, $"\r?\n"))
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
