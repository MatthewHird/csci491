using System;
using System.Collections.Generic;
using System.Text;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using MvcPodium.ConsoleApp.Models;
using MvcPodium.ConsoleApp.Models.CSharpCommon;

namespace MvcPodium.ConsoleApp.Services
{
    public interface ICSharpParserService
    {
        string GetTextWithWhitespace(BufferedTokenStream tokenStream, ParserRuleContext context);

        string GetTextWithWhitespaceMinifiedLite(BufferedTokenStream tokenStream, ParserRuleContext context);

        string GetTextWithWhitespaceMinified(BufferedTokenStream tokenStream, ParserRuleContext context);

        string GetTextWithWhitespaceUntab(
            BufferedTokenStream tokenStream, 
            ParserRuleContext context, 
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

        HashSet<string> GetUsingDirectivesNotInContext(
            CSharpParser.Compilation_unitContext context,
            List<string> usingDirectives);

        IToken GetUsingStopIndex(CSharpParser.Compilation_unitContext context);

        IToken GetNamespaceStopIndex(CSharpParser.Compilation_unitContext context);

        IToken GetClassInterfaceStopIndex(CSharpParser.Namespace_bodyContext context);

        string GenerateUsingDirectives(
            List<string> usingDirectives,
            bool isStartOfFile);

        string GenerateUsingDirective(
            string usingDirective,
            bool isStartOfFile);

        string GeneratePropertyDeclarations(
            List<PropertyDeclaration> propertyDeclarations,
            int tabLevels = 0,
            string tabString = null,
            bool isInterface = false);

        string GenerateMethodDeclarations(
            List<MethodDeclaration> methodDeclarations,
            int tabLevels = 0,
            string tabString = null,
            bool isInterface = false);

        string GenerateClassInterfaceDeclaration(
            ClassInterfaceDeclaration classInterfaceDeclaration,
            int tabLevels = 0,
            string tabString = null);

        string GenerateFieldDeclaration(
            FieldDeclaration fieldDeclaration,
            int tabLevels = 0,
            string tabString = null);

        string GenerateConstructorDeclaration(
            ConstructorDeclaration constructorDeclaration,
            int tabLevels = 0,
            string tabString = null);

        string GenerateFixedParameters(
            List<FixedParameter> fixedParameters,
            int tabLevels = 0,
            string tabString = null,
            bool isFirstParam = false,
            bool isSingleLine = false);

        string GenerateSimpleAssignments(
            List<SimpleAssignment> simpleAssignments,
            int tabLevels = 0,
            string tabString = null);
    }
}
