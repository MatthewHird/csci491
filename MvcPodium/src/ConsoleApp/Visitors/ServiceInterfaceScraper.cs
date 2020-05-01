using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using Microsoft.Extensions.Options;
using MvcPodium.ConsoleApp.Model;
using MvcPodium.ConsoleApp.Model.Config;
using MvcPodium.ConsoleApp.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using IToken = Antlr4.Runtime.IToken;
using ParserRuleContext = Antlr4.Runtime.ParserRuleContext;

namespace MvcPodium.ConsoleApp.Visitors
{
    public class ServiceInterfaceScraper : CSharpParserBaseVisitor<object>
    {
        private readonly ICSharpParserService _cSharpParserService;
        private readonly Stack<string> _currentNamespace;

        public string ServiceRootName { get; }

        public ServiceCommandScraperResults Results { get; } 
        public TokenStreamRewriter Rewriter { get; }
        public BufferedTokenStream Tokens { get; }

        public ServiceInterfaceScraper(
            BufferedTokenStream tokenStream,
            string serviceRootName,
            ICSharpParserService cSharpParserService)
        {
            _cSharpParserService = cSharpParserService;
            Tokens = tokenStream;
            ServiceRootName = serviceRootName;
            Rewriter = new TokenStreamRewriter(tokenStream);
            Results = new ServiceCommandScraperResults();
            _currentNamespace = new Stack<string>();
        }

        public override object VisitCompilation_unit([NotNull] CSharpParser.Compilation_unitContext context)
        {
            foreach (var usingDirective in context.using_directive())
            {
                Results.UsingDirectives.Add(
                    _cSharpParserService.GetTextWithWhitespaceMinifiedLite(
                        usingDirective.using_directive_inner(), Tokens));
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
            Match idMatch = Regex.Match(context.identifier().GetText(), $"^I?{Regex.Escape(ServiceRootName)}Service$");
            if (idMatch.Success)
            {
                Results.Namespace = string.Join(".", _currentNamespace.ToArray().Reverse());
                var interfaceName = idMatch.Value;
                var interfaceDeclaration = new ClassInterfaceDeclaration()
                {
                    IsInterface = true,
                    Attributes = _cSharpParserService.GetTextWithWhitespace(context?.attributes(), Tokens),
                    Identifier = interfaceName
                };

                //Results.ClassInterfaceDeclarations.Add(interfaceName, interfaceDeclaration);
                Results.ClassInterfaceDeclaration = interfaceDeclaration;


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

                var variantTypeParameters = context?.variant_type_parameter_list()?.variant_type_parameter();
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
                            _cSharpParserService.GetTextWithWhitespaceMinifiedLite(interfaceType, Tokens));
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
                                    interfaceMethodDeclaration?.attributes(), Tokens),
                                ReturnType = _cSharpParserService.GetTextWithWhitespaceMinifiedLite(
                                    interfaceMethodDeclaration.return_type(), Tokens),
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
                                    interfacePropertyDeclaration?.attributes(), Tokens),
                                Type = _cSharpParserService.GetTextWithWhitespaceMinifiedLite(
                                    interfacePropertyDeclaration.type_(), Tokens),
                                Identifier = interfacePropertyDeclaration.identifier().GetText(),
                                Body = new PropertyBody()
                                {
                                    Text = _cSharpParserService.GetTextWithWhitespaceUntab(
                                        interfacePropertyDeclaration.interface_accessors(), Tokens),
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
