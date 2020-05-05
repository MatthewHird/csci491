using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using MvcPodium.ConsoleApp.Models.CSharpCommon;
using MvcPodium.ConsoleApp.Models.ServiceCommand;
using MvcPodium.ConsoleApp.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MvcPodium.ConsoleApp.Visitors
{
    public class ServiceInterfaceScraper : CSharpParserBaseVisitor<object>
    {
        private readonly ICSharpParserService _cSharpParserService;
        private readonly Stack<string> _currentNamespace;
        private readonly string _serviceNamespace;

        public string ServiceInterfaceName { get; }
        public List<TypeParameter> TypeParameters { get; }

        public ServiceFile Results { get; } 
        public TokenStreamRewriter Rewriter { get; }
        public BufferedTokenStream Tokens { get; }

        public bool HasServiceInterface { get; private set; }

        public ServiceInterfaceScraper(
            ICSharpParserService cSharpParserService,
            BufferedTokenStream tokenStream,
            string serviceInterfaceName,
            string serviceNamespace,
            List<TypeParameter> typeParameters)
        {
            _cSharpParserService = cSharpParserService;
            Tokens = tokenStream;
            ServiceInterfaceName = serviceInterfaceName;
            TypeParameters = typeParameters;
            _serviceNamespace = serviceNamespace;
            Rewriter = new TokenStreamRewriter(tokenStream);
            Results = new ServiceFile();
            _currentNamespace = new Stack<string>();
            HasServiceInterface = false;
        }

        public override object VisitCompilation_unit([NotNull] CSharpParser.Compilation_unitContext context)
        {
            HasServiceInterface = false;
            Results.ServiceNamespace = _serviceNamespace;
            foreach (var usingDirective in context.using_directive())
            {
                Results.UsingDirectives.Add(
                    _cSharpParserService.GetTextWithWhitespaceMinifiedLite(
                        Tokens, usingDirective.using_directive_inner()));
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

        public override object VisitInterface_declaration([NotNull] CSharpParser.Interface_declarationContext context)
        {
            bool matchNames = context.identifier().GetText() == ServiceInterfaceName;
            bool isTypeParamsMatch = true;
            var variantTypeParameters = context?.variant_type_parameter_list()?.variant_type_parameter();
            if (!(variantTypeParameters is null && (TypeParameters is null || TypeParameters.Count == 0)))
            {
                if ((variantTypeParameters?.Length ?? 0) != (TypeParameters?.Count ?? 0))
                {
                    isTypeParamsMatch = false;
                }
                else
                {
                    for (int i = 0; i < variantTypeParameters.Length; ++i)
                    {
                        if (variantTypeParameters[i].identifier().GetText() != TypeParameters[i].TypeParam)
                        {
                            isTypeParamsMatch = false;
                            break;
                        }
                    }
                }
            }

            if (matchNames && isTypeParamsMatch)
            {
                HasServiceInterface = true;

                Results.ServiceNamespace = string.Join(".", _currentNamespace.ToArray().Reverse());
                var interfaceDeclaration = new ClassInterfaceDeclaration()
                {
                    IsInterface = true,
                    Attributes = _cSharpParserService.GetTextWithWhitespace(Tokens, context?.attributes()),
                    Identifier = ServiceInterfaceName
                };

                //Results.ClassInterfaceDeclarations.Add(interfaceName, interfaceDeclaration);
                Results.ServiceDeclaration = interfaceDeclaration;

                if (context?.PARTIAL()?.GetText() != null)
                {
                    interfaceDeclaration.Modifiers.Add(context.PARTIAL().GetText());
                }

                if (context?.interface_modifier() != null)
                {
                    foreach (var modifier in context.interface_modifier())
                    {
                        interfaceDeclaration.Modifiers.Add(modifier.GetText());
                    }
                }

                var constraintsClauses = 
                    context?.type_parameter_constraints_clauses()?.type_parameter_constraints_clause();

                interfaceDeclaration.TypeParameters = 
                    _cSharpParserService.ParseVariantTypeParameterList(
                        Tokens, variantTypeParameters, constraintsClauses);

                var interfaceTypes = context?.interface_base()?.interface_type_list()?.interface_type();
                if (interfaceTypes != null)
                {
                    interfaceDeclaration.Base = new ClassInterfaceBase();
                    foreach (var interfaceType in interfaceTypes)
                    {
                        interfaceDeclaration.Base.InterfaceTypeList.Add(
                            _cSharpParserService.GetTextWithWhitespaceMinifiedLite(Tokens, interfaceType));
                    }
                }

                var memberDeclarations = context?.interface_body()?.interface_member_declaration();
                if (memberDeclarations != null)
                {
                    foreach (var memberDeclaration in memberDeclarations)
                    {
                        var interfaceMethodDeclaration = memberDeclaration?.interface_method_declaration();
                        var interfacePropertyDeclaration = memberDeclaration?.interface_property_declaration();
                        if (interfaceMethodDeclaration != null)
                        {
                            var methodDeclaration = new MethodDeclaration()
                            {
                                Attributes = _cSharpParserService.GetTextWithWhitespace(
                                    Tokens, interfaceMethodDeclaration?.attributes()),
                                ReturnType = _cSharpParserService.GetTextWithWhitespaceMinifiedLite(
                                    Tokens, interfaceMethodDeclaration.return_type()),
                                Identifier = interfaceMethodDeclaration.identifier().GetText()
                            };

                            if (interfaceMethodDeclaration?.NEW()?.GetText() != null)
                            {
                                methodDeclaration.Modifiers.Add(interfaceMethodDeclaration.NEW().GetText());
                            }

                            var formalParameterList = interfaceMethodDeclaration?.formal_parameter_list();
                            methodDeclaration.FormalParameterList =
                                _cSharpParserService.ParseFormalParameterList(Tokens, formalParameterList);

                            var methodTypeParameters = 
                                interfaceMethodDeclaration?.type_parameter_list()?.type_parameter();
                            var methodConstraintsClauses = interfaceMethodDeclaration
                                ?.type_parameter_constraints_clauses()
                                ?.type_parameter_constraints_clause();

                            methodDeclaration.TypeParameters = _cSharpParserService
                                .ParseTypeParameterList(Tokens, methodTypeParameters, methodConstraintsClauses);

                            interfaceDeclaration.Body.MethodDeclarations.Add(methodDeclaration);
                        }
                        else if (interfacePropertyDeclaration != null)
                        {
                            var propertyDeclaration = new PropertyDeclaration()
                            {
                                Attributes = _cSharpParserService.GetTextWithWhitespace(
                                    Tokens, interfacePropertyDeclaration?.attributes()),
                                Type = _cSharpParserService.GetTextWithWhitespaceMinifiedLite(
                                    Tokens, interfacePropertyDeclaration.type_()),
                                Identifier = interfacePropertyDeclaration.identifier().GetText(),
                                Body = new PropertyBody()
                                {
                                    Text = _cSharpParserService.GetTextWithWhitespaceUntab(
                                        Tokens, interfacePropertyDeclaration.interface_accessors()),
                                    HasGetAccessor = interfacePropertyDeclaration.interface_accessors()
                                                                                 .interface_get_accessor() != null,
                                    HasSetAccessor = interfacePropertyDeclaration.interface_accessors()
                                                                                 .interface_set_accessor() != null
                                }
                            };

                            if (interfacePropertyDeclaration?.NEW() != null)
                            {
                                propertyDeclaration.Modifiers.Add(interfacePropertyDeclaration?.NEW()?.GetText());
                            }

                            interfaceDeclaration.Body.PropertyDeclarations.Add(propertyDeclaration);
                        }
                    }
                }
            }
            VisitChildren(context);
            return null;
        }
    }
}
