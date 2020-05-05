using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using MvcPodium.ConsoleApp.Models.ServiceCommand;
using MvcPodium.ConsoleApp.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MvcPodium.ConsoleApp.Visitors
{
    public class ServiceInterfaceInjector : CSharpParserBaseVisitor<object>
    {
        private readonly IStringUtilService _stringUtilService;
        private readonly ICSharpParserService _cSharpParserService;
        private readonly IServiceCommandService _serviceCommandService;

        private readonly string _serviceClassInterfaceName;
        private readonly ServiceFile _serviceFile;
        private readonly string _tabString;

        private bool _hasServiceNamespace;
        private bool _hasServiceInterface;

        private readonly Stack<string> _currentNamespace;

        public bool Success { get; private set; }

        public TokenStreamRewriter Rewriter { get; }
        public BufferedTokenStream Tokens { get; }

        public ServiceInterfaceInjector(
            IStringUtilService stringUtilService,
            ICSharpParserService cSharpParserService,
            IServiceCommandService serviceCommandService,
            BufferedTokenStream tokenStream,
            string serviceClassInterfaceName,
            ServiceFile serviceFile,
            string tabString = null)
        {
            _stringUtilService = stringUtilService;
            _cSharpParserService = cSharpParserService;
            _serviceCommandService = serviceCommandService;
            Tokens = tokenStream;
            Rewriter = new TokenStreamRewriter(tokenStream);
            _serviceClassInterfaceName = serviceClassInterfaceName;
            _serviceFile = serviceFile;
            _tabString = tabString;
            _currentNamespace = new Stack<string>();
            _hasServiceNamespace = false;
            _hasServiceInterface = false;
    }

        public override object VisitCompilation_unit([NotNull] CSharpParser.Compilation_unitContext context)
        {
            Success = false;
            _hasServiceNamespace = false;
            _hasServiceInterface = false;

            var usingStopIndex = _cSharpParserService.GetUsingStopIndex(context);

            var usingDirectivesStr = _cSharpParserService.GenerateUsingDirectives(
                _serviceFile.UsingDirectives,
                usingStopIndex.Equals(context.Start));

            Rewriter.InsertAfter(usingStopIndex, usingDirectivesStr);

            VisitChildren(context);

            if (!_hasServiceNamespace)
            {
                var namespaceStopIndex = _cSharpParserService.GetNamespaceStopIndex(context);
                var serviceNamespaceDeclaration = _serviceCommandService.GenerateServiceNamespaceDeclaration(
                    _serviceFile.ServiceNamespace,
                    _serviceFile.ServiceDeclaration);
                Rewriter.InsertAfter(namespaceStopIndex, serviceNamespaceDeclaration);
            }

            Success = true;
            return null;
        }

        public override object VisitNamespace_declaration([NotNull] CSharpParser.Namespace_declarationContext context)
        {
            _currentNamespace.Push(context.qualified_identifier().GetText());
            VisitChildren(context);
            _ = _currentNamespace.Pop();
            return null;
        }

        public override object VisitNamespace_body([NotNull] CSharpParser.Namespace_bodyContext context)
        {
            var isServiceNamespace = GetCurrentNamespace() == _serviceFile.ServiceNamespace;
            if (isServiceNamespace)
            {
                _hasServiceNamespace = true;
            }

            VisitChildren(context);

            if (!_hasServiceInterface && isServiceNamespace)
            {
                var classInterfaceStopIndex = _cSharpParserService.GetClassInterfaceStopIndex(context);

                var preclassWhitespace = Tokens.GetHiddenTokensToLeft(context.Start.TokenIndex, Lexer.Hidden);
                int tabLevels = 1 + (preclassWhitespace.Count > 0 ?
                    _stringUtilService.CalculateTabLevels(preclassWhitespace[0].Text, _tabString) : 0);

                var interfaceDeclaration = _cSharpParserService.GenerateClassInterfaceDeclaration(
                    _serviceFile.ServiceDeclaration,
                    tabLevels,
                    _tabString);

                Rewriter.InsertAfter(classInterfaceStopIndex, interfaceDeclaration);
            }

            return null;
        }

        public override object VisitInterface_declaration([NotNull] CSharpParser.Interface_declarationContext context)
        {
            var typeParameters = _serviceFile.ServiceDeclaration.TypeParameters;

            bool isTypeParamsMatch = true;
            var variantTypeParameters = context?.variant_type_parameter_list()?.variant_type_parameter();
            if (!(variantTypeParameters is null && (typeParameters is null || typeParameters.Count == 0)))
            {
                if ((variantTypeParameters?.Length ?? 0) != (typeParameters?.Count ?? 0))
                {
                    isTypeParamsMatch = false;
                }
                else
                {
                    for (int i = 0; i < variantTypeParameters.Length; ++i)
                    {
                        if (variantTypeParameters[i].identifier().GetText() != typeParameters[i].TypeParam)
                        {
                            isTypeParamsMatch = false;
                            break;
                        }
                    }
                }
            }

            if (context.identifier().GetText() == _serviceClassInterfaceName
                && GetCurrentNamespace() == _serviceFile.ServiceNamespace
                && isTypeParamsMatch)
            {
                _hasServiceInterface = true;
                var preclassWhitespace = Tokens.GetHiddenTokensToLeft(context.Start.TokenIndex, Lexer.Hidden);

                int tabLevels = 1 + (preclassWhitespace.Count > 0 ? 
                    _stringUtilService.CalculateTabLevels(preclassWhitespace[0].Text, _tabString) : 0);

                int?finalMethod = null;
                int? finalProperty = null;
                var members = context?.interface_body()?.interface_member_declaration();
                foreach (var member in members)
                {
                    if (member.interface_method_declaration() != null)
                    {
                        finalMethod = member?.interface_method_declaration()?.Stop?.TokenIndex;
                    }
                    else if (member.interface_property_declaration() != null)
                    {
                        finalProperty = member?.interface_property_declaration()?.Stop?.TokenIndex;
                    }
                }

                int propertyStopIndex = finalProperty
                    ?? context.interface_body().OPEN_BRACE().Symbol.TokenIndex;

                var propertyString = _cSharpParserService.GeneratePropertyDeclarations(
                    _serviceFile.ServiceDeclaration.Body.PropertyDeclarations,
                    tabLevels,
                    _tabString,
                    true);

                int? methodStopIndex = finalMethod;

                var methodString = _cSharpParserService.GenerateMethodDeclarations(
                    _serviceFile.ServiceDeclaration.Body.MethodDeclarations,
                    tabLevels,
                    _tabString,
                    true);

                if (methodStopIndex is null)
                {
                    propertyString += methodString;
                }
                else
                {
                    if (methodString.Length > 0)
                    {
                        Rewriter.InsertAfter(Tokens.Get(methodStopIndex ?? -1), methodString);
                    }
                }

                if (propertyString.Length > 0)
                {
                    Rewriter.InsertAfter(propertyStopIndex, propertyString);
                }

            }
            VisitChildren(context);
            return null;
        }

        private string GetCurrentNamespace()
        {
            return string.Join(".", _currentNamespace.ToArray().Reverse());
        }
    }
}
