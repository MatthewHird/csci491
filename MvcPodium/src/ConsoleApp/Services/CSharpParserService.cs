using System;
using System.Collections.Generic;
using System.Text;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using MvcPodium.ConsoleApp.Model;

namespace MvcPodium.ConsoleApp.Services
{
    public class CSharpParserService : ICSharpParserService
    {
        private readonly IStringUtilService _stringUtilService;

        public CSharpParserService(IStringUtilService stringUtilService)
        {
            _stringUtilService = stringUtilService;
        }


        public string GetTextWithWhitespace(ParserRuleContext context, BufferedTokenStream tokenStream)
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


        public string GetTextWithWhitespaceMinifiedLite(ParserRuleContext context, BufferedTokenStream tokenStream)
        {
            if (context is null) { return null; }
            return _stringUtilService.MinifyStringLite(GetTextWithWhitespace(context, tokenStream));
        }


        public string GetTextWithWhitespaceMinified(ParserRuleContext context, BufferedTokenStream tokenStream)
        {
            if (context is null) { return null; }
            return _stringUtilService.MinifyString(GetTextWithWhitespace(context, tokenStream));
        }


        public string GetTextWithWhitespaceUntab(
            ParserRuleContext context,
            BufferedTokenStream tokenStream,
            int untabLevels=-1)
        {
            return _stringUtilService.UntabString(GetTextWithWhitespace(context, tokenStream), untabLevels);
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
                        constraints?.primary_constraint(), tokenStream);
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
                                GetTextWithWhitespaceMinified(secondaryConstraint, tokenStream));
                        }
                    }

                    var constructorConstraint = GetTextWithWhitespaceMinified(
                        constraints?.constructor_constraint(), tokenStream);
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
                                Attributes = GetTextWithWhitespace(fixedParameter?.attributes(), tokenStream),
                                ParameterModifier = fixedParameter?.parameter_modifier()?.GetText(),
                                Type = GetTextWithWhitespaceMinifiedLite(fixedParameter.type_(), tokenStream),
                                Identifier = fixedParameter.identifier().GetText(),
                                DefaultArgument = GetTextWithWhitespace(
                                    fixedParameter?.default_argument()?.expression(),
                                    tokenStream)
                            }
                        );
                    }
                }
                var parameterArray = formalParameterList?.parameter_array();
                if (parameterArray != null)
                {
                    formalParamList.ParameterArray = new ParameterArray()
                    {
                        Attributes = GetTextWithWhitespace(parameterArray?.attributes(), tokenStream),
                        Type = GetTextWithWhitespaceMinifiedLite(parameterArray.array_type(), tokenStream),
                        Identifier = parameterArray.identifier().GetText()
                    };
                }
            }
            return formalParamList;
        }
    }
}
