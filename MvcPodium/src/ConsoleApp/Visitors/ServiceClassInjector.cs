using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using MvcPodium.ConsoleApp.Models.ServiceCommand;
using MvcPodium.ConsoleApp.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MvcPodium.ConsoleApp.Visitors
{
    public class ServiceClassInjector : CSharpParserBaseVisitor<object>
    {
        private readonly IStringUtilService _stringUtilService;
        private readonly ICSharpParserService _cSharpParserService;
        private readonly IServiceCommandParserService _serviceCommandParserService;

        private readonly string _serviceClassInterfaceName;
        private readonly ServiceFile _serviceFile;
        private readonly string _tabString;

        private bool _hasServiceNamespace;
        private bool _hasServiceClass;

        private readonly Stack<string> _currentNamespace;

        public bool IsModified { get; private set; }

        public TokenStreamRewriter Rewriter { get; }
        public BufferedTokenStream Tokens { get; }

        public ServiceClassInjector(
            IStringUtilService stringUtilService,
            ICSharpParserService cSharpParserService,
            IServiceCommandParserService serviceCommandParserService,
            BufferedTokenStream tokenStream,
            string serviceClassInterfaceName,
            ServiceFile serviceFile,
            string tabString = null)
        {
            _stringUtilService = stringUtilService;
            _cSharpParserService = cSharpParserService;
            _serviceCommandParserService = serviceCommandParserService;
            Tokens = tokenStream;
            Rewriter = new TokenStreamRewriter(tokenStream);
            _serviceClassInterfaceName = serviceClassInterfaceName;
            _serviceFile = serviceFile;
            _tabString = tabString;
            _currentNamespace = new Stack<string>();
            _hasServiceNamespace = false;
            _hasServiceClass = false;
            IsModified = false;
        }

        public override object VisitCompilation_unit([NotNull] CSharpParser.Compilation_unitContext context)
        {
            var usingStopIndex = _cSharpParserService.GetUsingStopIndex(context);

            var usingDirectivesStr = _cSharpParserService.GenerateUsingDirectives(
                _serviceFile.UsingDirectives,
                usingStopIndex.Equals(context.Start));

            if (usingDirectivesStr != null && usingDirectivesStr != string.Empty)
            {
                IsModified = true;
                Rewriter.InsertAfter(usingStopIndex, usingDirectivesStr);
            }

            VisitChildren(context);

            if (!_hasServiceNamespace)
            {
                var namespaceStopIndex = _cSharpParserService.GetNamespaceStopIndex(context);
                var serviceNamespaceDeclaration = _serviceCommandParserService.GenerateServiceNamespaceDeclaration(
                    _serviceFile.ServiceNamespace,
                    _serviceFile.ServiceDeclaration);
                IsModified = true;
                Rewriter.InsertAfter(namespaceStopIndex, serviceNamespaceDeclaration);
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


        public override object VisitNamespace_body([NotNull] CSharpParser.Namespace_bodyContext context)
        {
            var isServiceNamespace = GetCurrentNamespace() == _serviceFile.ServiceNamespace;
            if (isServiceNamespace)
            {
                _hasServiceNamespace = true;
            }

            VisitChildren(context);

            if (!_hasServiceClass && isServiceNamespace)
            {
                var classInterfaceStopIndex = _cSharpParserService.GetClassInterfaceStopIndex(context);

                var preclassWhitespace = Tokens.GetHiddenTokensToLeft(context.Start.TokenIndex, Lexer.Hidden);
                int tabLevels = 1 + ((preclassWhitespace?.Count ?? 0) > 0 ?
                    _stringUtilService.CalculateTabLevels(preclassWhitespace[0]?.Text ?? string.Empty, _tabString) : 0);

                var classDeclaration = _cSharpParserService.GenerateClassInterfaceDeclaration(
                    _serviceFile.ServiceDeclaration,
                    tabLevels,
                    _tabString);
                IsModified = true;
                Rewriter.InsertAfter(classInterfaceStopIndex, classDeclaration);
            }

            return null;
        }


        public override object VisitClass_declaration([NotNull] CSharpParser.Class_declarationContext context)
        {
            var typeParameters = _serviceFile.ServiceDeclaration.TypeParameters;
            var currentNamespace = string.Join(".", _currentNamespace.ToArray().Reverse());

            bool isTypeParamsMatch = true;
            var typeParams = context?.type_parameter_list()?.type_parameter();
            if (!(typeParams is null && (typeParameters is null || typeParameters.Count == 0)))
            {
                if ((typeParams?.Length ?? 0) != (typeParameters?.Count ?? 0))
                {
                    isTypeParamsMatch = false;
                }
                else
                {
                    for (int i = 0; i < typeParams.Length; ++i)
                    {
                        if (typeParams[i].identifier().GetText() != typeParameters[i].TypeParam)
                        {
                            isTypeParamsMatch = false;
                            break;
                        }
                    }
                }
            }

            if (context.identifier().GetText() == _serviceClassInterfaceName 
                && currentNamespace == _serviceFile.ServiceNamespace
                && isTypeParamsMatch)
            {
                _hasServiceClass = true;
                var preclassWhitespace = Tokens.GetHiddenTokensToLeft(context.Start.TokenIndex, Lexer.Hidden);

                int tabLevels = 1 + ((preclassWhitespace?.Count ?? 0) > 0 ?
                    _stringUtilService.CalculateTabLevels(preclassWhitespace[0]?.Text ?? string.Empty, _tabString) : 0);

                int? finalProperty = null;
                int? finalMethod = null;
                int? finalConstantOrField = null;
                int? finalConstructorOrDestructor = null;

                var members = context?.class_body()?.class_member_declarations()?.class_member_declaration();
                if (members != null)
                {
                    foreach (var member in members)
                    {
                        if (member.method_declaration() != null)
                        {
                            finalMethod = member.method_declaration().Stop.TokenIndex;
                        }
                        else if (member.property_declaration() != null)
                        {
                            finalProperty = member.property_declaration().Stop.TokenIndex;
                        }
                        else if (member.constant_declaration() != null)
                        {
                            finalConstantOrField = member.constant_declaration().Stop.TokenIndex;
                        }
                        else if (member.field_declaration() != null)
                        {
                            finalConstantOrField = member.field_declaration().Stop.TokenIndex;
                        }
                        else if (member.constructor_declaration() != null)
                        {
                            finalConstructorOrDestructor = member.constructor_declaration().Stop.TokenIndex;
                        }
                        else if (member.static_constructor_declaration() != null)
                        {
                            finalConstructorOrDestructor = member.static_constructor_declaration().Stop.TokenIndex;
                        }
                        else if (member.destructor_declaration() != null)
                        {
                            finalConstructorOrDestructor = member.destructor_declaration().Stop.TokenIndex;
                        }
                    }

                }

                int propertyStopIndex = finalProperty
                    ?? finalConstantOrField
                    ?? context.class_body().OPEN_BRACE().Symbol.TokenIndex;

                var propertyString = _cSharpParserService.GeneratePropertyDeclarations(
                    _serviceFile.ServiceDeclaration.Body.PropertyDeclarations,
                    tabLevels,
                    _tabString);

                int? methodStopIndex = finalMethod ?? finalConstructorOrDestructor;

                var methodString = _cSharpParserService.GenerateMethodDeclarations(
                    _serviceFile.ServiceDeclaration.Body.MethodDeclarations,
                    tabLevels,
                    _tabString);

                if (methodStopIndex is null)
                {
                    propertyString += methodString;
                }
                else
                {
                    if (methodString.Length > 0)
                    {
                        IsModified = true;
                        Rewriter.InsertAfter(methodStopIndex ?? -1, methodString);
                    }
                }

                if (propertyString.Length > 0)
                {
                    IsModified = true;
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
