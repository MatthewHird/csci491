using System.Collections.Generic;
using System.Linq;
using System.Text;
using Antlr4.Runtime;
using MvcPodium.ConsoleApp.Models.CSharpCommon;

namespace MvcPodium.ConsoleApp.Services
{
    public class CSharpParserService : ICSharpParserService
    {
        private readonly IStringUtilService _stringUtilService;
        private readonly ICSharpCommonStgService _cSharpCommonStgService;

        public CSharpParserService(
            IStringUtilService stringUtilService,
            ICSharpCommonStgService cSharpCommonStgService)
        {
            _stringUtilService = stringUtilService;
            _cSharpCommonStgService = cSharpCommonStgService;
        }


        public string GetTextWithWhitespace(BufferedTokenStream tokenStream, ParserRuleContext context)
        {
            if (context is null) { return null; }

            var startToken = context.Start.TokenIndex;
            var stopToken = context.Stop.TokenIndex;

            var tokens = tokenStream.Get(startToken, stopToken);
            var filteredText = new StringBuilder();
            foreach (var t in tokens)
            {
                if (t.Channel == Lexer.DefaultTokenChannel || t.Channel == Lexer.Hidden)
                {
                    filteredText.Append(t.Text);
                }
            }
            return filteredText.ToString();
        }


        public string GetTextWithWhitespaceMinifiedLite(BufferedTokenStream tokenStream, ParserRuleContext context)
        {
            if (context is null) { return null; }
            return _stringUtilService.MinifyStringLite(GetTextWithWhitespace(tokenStream, context));
        }


        public string GetTextWithWhitespaceMinified(BufferedTokenStream tokenStream, ParserRuleContext context)
        {
            if (context is null) { return null; }
            return _stringUtilService.MinifyString(GetTextWithWhitespace(tokenStream, context));
        }


        public string GetTextWithWhitespaceUntab(
            BufferedTokenStream tokenStream,
            ParserRuleContext context,
            int untabLevels=-1)
        {
            return _stringUtilService.UntabString(GetTextWithWhitespace(tokenStream, context), untabLevels);
        }


        public List<TypeParameter> ParseVariantTypeParameterList(
            BufferedTokenStream tokenStream,
            CSharpParser.Variant_type_parameterContext[] variantTypeParameters,
            CSharpParser.Type_parameter_constraints_clauseContext[] constraintsClauses)
        {
            var typeParamList = new List<TypeParameter>();

            var typeParamDict = new Dictionary<string, TypeParameter>();

            if (variantTypeParameters != null)
            {
                foreach (var variantTypeParameter in variantTypeParameters)
                {
                    var typeParam = variantTypeParameter.identifier().GetText();

                    var typeParameter = new TypeParameter()
                    {
                        TypeParam = typeParam,
                        VarianceAnnotation = variantTypeParameter?.variance_annotation()?.GetText()
                    };

                    typeParamDict.Add(typeParam, typeParameter);
                    typeParamList.Add(typeParameter);
                }

            }

            if (constraintsClauses != null)
            {
                foreach (var constraintClause in constraintsClauses)
                {
                    var identifier = constraintClause.identifier().GetText();
                    var typeParameter = typeParamDict[identifier];
                    var constraints = constraintClause.type_parameter_constraints();

                    var primaryConstraint = constraints?.primary_constraint()?.GetText();
                    if (primaryConstraint != null)
                    {
                        typeParameter.Constraints.Add(primaryConstraint);
                    }

                    var secondaryConstraints = constraints?.secondary_constraints()?.secondary_constraint();
                    if (secondaryConstraints != null)
                    {
                        foreach (var secondaryConstraint in secondaryConstraints)
                        {
                            typeParameter.Constraints.Add(secondaryConstraint.GetText());
                        }
                    }

                    var constructorConstraint = constraints?.constructor_constraint()?.GetText();
                    if (constructorConstraint != null)
                    {
                        typeParameter.Constraints.Add(constructorConstraint);
                    }

                }
            }

            return typeParamList;
        }


        public List<TypeParameter> ParseTypeParameterList(
            BufferedTokenStream tokenStream,
            CSharpParser.Type_parameterContext[] typeParameters,
            CSharpParser.Type_parameter_constraints_clauseContext[] constraintsClauses)
        {
            var typeParamList = new List<TypeParameter>();

            var typeParamDict = new Dictionary<string, TypeParameter>();

            if (typeParameters != null)
            {
                foreach (var tp in typeParameters)
                {
                    var typeParam = tp.identifier().GetText();

                    var typeParameter = new TypeParameter()
                    {
                        TypeParam = typeParam
                    };

                    typeParamDict.Add(typeParam, typeParameter);
                    typeParamList.Add(typeParameter);
                }

            }

            if (constraintsClauses != null)
            {
                foreach (var constraintClause in constraintsClauses)
                {
                    var identifier = constraintClause.identifier().GetText();
                    var typeParameter = typeParamDict[identifier];
                    var constraints = constraintClause.type_parameter_constraints();

                    var primaryConstraint = GetTextWithWhitespaceMinified(
                        tokenStream, constraints?.primary_constraint());
                    if (primaryConstraint != null)
                    {
                        typeParameter.Constraints.Add(primaryConstraint);
                    }

                    var secondaryConstraints = constraints?.secondary_constraints()?.secondary_constraint();
                    if (secondaryConstraints != null)
                    {
                        foreach (var secondaryConstraint in secondaryConstraints)
                        {
                            typeParameter.Constraints.Add(
                                GetTextWithWhitespaceMinified(tokenStream, secondaryConstraint));
                        }
                    }

                    var constructorConstraint = GetTextWithWhitespaceMinified(
                        tokenStream, constraints?.constructor_constraint());
                    if (constructorConstraint != null)
                    {
                        typeParameter.Constraints.Add(constructorConstraint);
                    }

                }
            }

            return typeParamList;
        }


        public FormalParameterList ParseFormalParameterList(
            BufferedTokenStream tokenStream,
            CSharpParser.Formal_parameter_listContext formalParameterList)
        {
            FormalParameterList formalParamList = null;
            if (formalParameterList != null)
            {
                formalParamList = new FormalParameterList();
                var fixedParameters = formalParameterList?.fixed_parameters()?.fixed_parameter();
                if (fixedParameters != null)
                {
                    foreach (var fixedParameter in fixedParameters)
                    {
                        formalParamList.FixedParameters.Add(
                            new FixedParameter()
                            {
                                Attributes = GetTextWithWhitespace(tokenStream, fixedParameter?.attributes()),
                                ParameterModifier = fixedParameter?.parameter_modifier()?.GetText(),
                                Type = GetTextWithWhitespaceMinifiedLite(tokenStream, fixedParameter.type_()),
                                Identifier = fixedParameter.identifier().GetText(),
                                DefaultArgument = GetTextWithWhitespace(
                                    tokenStream, fixedParameter?.default_argument()?.expression())
                            }
                        );
                    }
                }
                var parameterArray = formalParameterList?.parameter_array();
                if (parameterArray != null)
                {
                    formalParamList.ParameterArray = new ParameterArray()
                    {
                        Attributes = GetTextWithWhitespace(tokenStream, parameterArray?.attributes()),
                        Type = GetTextWithWhitespaceMinifiedLite(tokenStream, parameterArray.array_type()),
                        Identifier = parameterArray.identifier().GetText()
                    };
                }
            }
            return formalParamList;
        }


        public HashSet<string> GetUsingDirectivesNotInContext(
            CSharpParser.Compilation_unitContext context,
            List<string> usingDirectives)
        {
            var contextUsingDirectives = context?.using_directive();

            var contextUsingSet = new HashSet<string>();

            if (contextUsingDirectives != null)
            {
                foreach (var usingDir in contextUsingDirectives)
                {
                    contextUsingSet.Add(_stringUtilService.MinifyString(usingDir.using_directive_inner().GetText()));
                }
            }
            return _stringUtilService.GetMissingStrings(contextUsingSet, usingDirectives);
        }


        public IToken GetUsingStopIndex(CSharpParser.Compilation_unitContext context)
        {
            return context?.using_directive()?.LastOrDefault()?.Stop
                ?? context?.extern_alias_directive()?.LastOrDefault()?.Stop
                ?? context?.BYTE_ORDER_MARK()?.Symbol
                ?? context.Start;
        }


        public IToken GetNamespaceStopIndex(CSharpParser.Compilation_unitContext context)
        {
            return context?.namespace_member_declaration()?.LastOrDefault()?.Stop
                ?? context?.global_attributes()?.Stop
                ?? context?.using_directive()?.LastOrDefault()?.Stop
                ?? context?.extern_alias_directive()?.LastOrDefault()?.Stop
                ?? context?.BYTE_ORDER_MARK()?.Symbol
                ?? context.Start;
        }

        public IToken GetClassInterfaceStopIndex(CSharpParser.Namespace_bodyContext context)
        {
            return context?.namespace_member_declaration()?.LastOrDefault()?.Stop
                ?? context?.using_directive()?.LastOrDefault()?.Stop
                ?? context?.extern_alias_directive()?.LastOrDefault()?.Stop
                ?? context.OPEN_BRACE().Symbol;
        }



        public string GenerateUsingDirectives(
            List<string> usingDirectives,
            bool isStartOfFile)
        {
            if (usingDirectives is null || usingDirectives.Count == 0) { return string.Empty; }
            return (isStartOfFile ? string.Empty : "\r\n") +
                _cSharpCommonStgService.RenderUsingDirectives(usingDirectives) +
                (isStartOfFile ? "\r\n" : string.Empty);
        }


        public string GenerateUsingDirective(
            string usingDirective,
            bool isStartOfFile)
        {
            if (usingDirective is null || usingDirective == string.Empty) { return string.Empty; }
            return (isStartOfFile ? string.Empty : "\r\n") +
                _cSharpCommonStgService.RenderUsingDirective(usingDirective) +
                (isStartOfFile ? "\r\n" : string.Empty);
        }


        public string GeneratePropertyDeclarations(
            List<PropertyDeclaration> propertyDeclarations,
            int tabLevels = 0,
            string tabString = null,
            bool isInterface = false)
        {
            var propertyStringBuilder = new StringBuilder();
            if (propertyDeclarations != null && propertyDeclarations.Count > 0)
            {
                propertyStringBuilder.Append("\r\n");
                foreach (var property in propertyDeclarations)
                {
                    propertyStringBuilder.Append("\r\n");
                    propertyStringBuilder.Append(_stringUtilService.TabString(
                        isInterface
                            ? _cSharpCommonStgService.RenderInterfacePropertyDeclaration(property)
                            : _cSharpCommonStgService.RenderClassPropertyDeclaration(property),
                        tabLevels,
                        tabString));
                    propertyStringBuilder.Append("\r\n");
                }
            }
            return propertyStringBuilder.ToString();
        }

        public string GenerateMethodDeclarations(
            List<MethodDeclaration> methodDeclarations,
            int tabLevels = 0,
            string tabString = null,
            bool isInterface = false)
        {
            var methodStringBuilder = new StringBuilder();
            if (methodDeclarations != null && methodDeclarations.Count > 0)
            {
                methodStringBuilder.Append("\r\n");
                foreach (var method in methodDeclarations)
                {
                    methodStringBuilder.Append("\r\n");
                    methodStringBuilder.Append(_stringUtilService.TabString(
                        isInterface
                            ? _cSharpCommonStgService.RenderInterfaceMethodDeclaration(method)
                            : _cSharpCommonStgService.RenderClassMethodDeclaration(method),
                        tabLevels,
                        tabString));
                    methodStringBuilder.Append("\r\n");
                }
            }
            return methodStringBuilder.ToString();
        }

        public string GenerateClassInterfaceDeclaration(
            ClassInterfaceDeclaration classInterfaceDeclaration,
            int tabLevels = 0,
            string tabString = null)
        {
            if (classInterfaceDeclaration is null) { return string.Empty; }
            return "\r\n\r\n" +
                _stringUtilService.TabString(
                    classInterfaceDeclaration.IsInterface
                        ? _cSharpCommonStgService.RenderInterfaceDeclaration(classInterfaceDeclaration)
                        : _cSharpCommonStgService.RenderClassDeclaration(classInterfaceDeclaration),
                    tabLevels,
                    tabString);
        }

        public string GenerateFieldDeclaration(
            FieldDeclaration fieldDeclaration,
            int tabLevels = 0,
            string tabString = null)
        {
            if (fieldDeclaration is null) { return string.Empty; }
            return "\r\n" +
                _stringUtilService.TabString(
                    _cSharpCommonStgService.RenderFieldDeclaration(fieldDeclaration),
                    tabLevels,
                    tabString);
        }

        public string GenerateConstructorDeclaration(
            ConstructorDeclaration constructorDeclaration,
            int tabLevels = 0,
            string tabString = null)
        {
            if (constructorDeclaration is null) { return string.Empty; }
            return "\r\n\r\n" +
                _stringUtilService.TabString(
                    _cSharpCommonStgService.RenderConstructorDeclaration(constructorDeclaration),
                    tabLevels,
                    tabString) +
                "\r\n";
        }

        public string GenerateFixedParameters(
            List<FixedParameter> fixedParameters,
            int tabLevels = 0,
            string tabString = null,
            bool isFirstParam = false,
            bool isSingleLine = false)
        {
            if (fixedParameters is null) { return string.Empty; }
            var stringBuilder = new StringBuilder();
            bool firstParam = isFirstParam;

            foreach (var fixedParam in fixedParameters)
            {
                var ctorParam = _cSharpCommonStgService.RenderFixedParameter(fixedParam);

                if (firstParam)
                {
                    firstParam = false;
                }
                else
                {
                    stringBuilder.Append(
                        isSingleLine
                        ? ", "
                        : ",");
                }
                stringBuilder.Append(
                    isSingleLine
                    ? ctorParam
                    : "\r\n" + _stringUtilService.TabString(ctorParam, tabLevels, tabString));
            }

            return stringBuilder.ToString();
        }

        public string GenerateSimpleAssignments(
            List<SimpleAssignment> simpleAssignments,
            int tabLevels = 0,
            string tabString = null)
        {
            if (simpleAssignments is null) { return string.Empty; }
            var stringBuilder = new StringBuilder();
            foreach (var simpleAssignment in simpleAssignments)
            {
                stringBuilder.Append("\r\n");
                stringBuilder.Append(_stringUtilService.TabString(
                    _cSharpCommonStgService.RenderSimpleAssignment(simpleAssignment),
                    tabLevels,
                    tabString));
            }
            return stringBuilder.ToString();
        }
    }
}
