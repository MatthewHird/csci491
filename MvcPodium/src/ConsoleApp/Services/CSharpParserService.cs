using System;
using System.Collections.Generic;
using System.Text;
using MvcPodium.ConsoleApp.Model;

namespace MvcPodium.ConsoleApp.Services
{
    public class CSharpParserService : ICSharpParserService
    {
        public List<TypeParameter> ParseVariantTypeParameterList(
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
    }
}
