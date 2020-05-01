using System;
using System.Collections.Generic;
using System.Text;
using Antlr4.Runtime;
using MvcPodium.ConsoleApp.Model;

namespace MvcPodium.ConsoleApp.Services
{
    public interface ICSharpParserService
    {
        string GetTextWithWhitespace(ParserRuleContext context, BufferedTokenStream tokenStream);

        string GetTextWithWhitespaceMinifiedLite(ParserRuleContext context, BufferedTokenStream tokenStream);

        string GetTextWithWhitespaceMinified(ParserRuleContext context, BufferedTokenStream tokenStream);

        string GetTextWithWhitespaceUntab(
            ParserRuleContext context, 
            BufferedTokenStream tokenStream, 
            int untabLevels=-1);

        List<TypeParameter> ParseVariantTypeParameterList(
            BufferedTokenStream tokenStream,
            CSharpParser.Variant_type_parameterContext[] variantTypeParameters,
            CSharpParser.Type_parameter_constraints_clauseContext[] constraintsClauses);

        List<TypeParameter> ParseTypeParameterList(
            BufferedTokenStream tokenStream,
            CSharpParser.Type_parameterContext[] typeParameters,
            CSharpParser.Type_parameter_constraints_clauseContext[] constraintsClauses);

        FormalParameterList ParseFormalParameterList(
            BufferedTokenStream tokenStream,
            CSharpParser.Formal_parameter_listContext formalParameterList);
    }
}
