using System;
using System.Collections.Generic;
using System.Text;
using MvcPodium.ConsoleApp.Model;

namespace MvcPodium.ConsoleApp.Services
{
    public interface ICSharpParserService
    {
        List<TypeParameter> ParseVariantTypeParameterList(
            CSharpParser.Variant_type_parameterContext[] variantTypeParameters,
            CSharpParser.Type_parameter_constraints_clauseContext[] constraintsClauses);

        List<TypeParameter> ParseTypeParameterList(
            CSharpParser.Type_parameterContext[] typeParameters,
            CSharpParser.Type_parameter_constraints_clauseContext[] constraintsClauses);
    }
}
