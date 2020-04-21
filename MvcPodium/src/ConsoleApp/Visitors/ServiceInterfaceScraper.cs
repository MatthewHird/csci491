using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using Microsoft.Extensions.Options;
using MvcPodium.ConsoleApp.Model;
using MvcPodium.ConsoleApp.Model.Config;
using MvcPodium.ConsoleApp.Services;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using IToken = Antlr4.Runtime.IToken;
using ParserRuleContext = Antlr4.Runtime.ParserRuleContext;

namespace MvcPodium.ConsoleApp.Visitors
{
    public class ServiceInterfaceScraper : CSharpParserBaseVisitor<object>
    {
        private readonly ICSharpParserService _cSharpParserService;

        private Stack<string> _currentClassInterface;
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
            _currentClassInterface = new Stack<string>();
        }

        public override object VisitCompilation_unit([NotNull] CSharpParser.Compilation_unitContext context)
        {
            foreach (var usingDirective in context.using_directive())
            {
                Results.UsingDirectives.Add(usingDirective.GetText());
            }
            VisitChildren(context);
            return null;
        }

        public override object VisitInterface_declaration([NotNull] CSharpParser.Interface_declarationContext context)
        {
            Match idMatch = Regex.Match(context.identifier().GetText(), $"^I?{Regex.Escape(ServiceRootName)}Service$");
            if (idMatch.Success)
            {
                var interfaceName = idMatch.Value;
                var interfaceDeclaration = new ClassInterfaceDeclaration()
                {
                    IsInterface = true,
                    Attributes = context?.attributes()?.GetText(),
                    Identifier = interfaceName
                };

                Results.ClassInterfaceDeclarations.Add(interfaceName, interfaceDeclaration);

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
                    _cSharpParserService.ParseVariantTypeParameterList(variantTypeParameters, constraintsClauses);

                var interfaceTypes = context?.interface_base()?.interface_type_list()?.interface_type();
                if (interfaceTypes != null)
                {
                    interfaceDeclaration.Base = new ClassInterfaceBase();
                    foreach (var interfaceType in interfaceTypes)
                    {
                        interfaceDeclaration.Base.InterfaceTypeList.Add(interfaceType.GetText());
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
                                Attributes = interfaceMethodDeclaration?.attributes()?.GetText(),
                                ReturnType = interfaceMethodDeclaration.return_type().GetText(),
                                Identifier = interfaceMethodDeclaration.identifier().GetText()
                            };

                            if (interfaceMethodDeclaration?.NEW()?.GetText() != null)
                            {
                                methodDeclaration.Modifiers.Add(interfaceMethodDeclaration.NEW().GetText());
                            }

                            var formalParameterList = interfaceMethodDeclaration?.formal_parameter_list();
                            if (formalParameterList != null)
                            {
                                var fixedParameters = formalParameterList?.fixed_parameters()?.fixed_parameter();
                                if (fixedParameters != null)
                                {
                                    methodDeclaration.FormalParameterList = new FormalParameterList();
                                    foreach (var fixedParameter in fixedParameters)
                                    {
                                        methodDeclaration.FormalParameterList.FixedParameters.Add(
                                            new FixedParameter()
                                            {
                                                Attributes = fixedParameter?.attributes()?.GetText(),
                                                ParameterModifier = fixedParameter?.parameter_modifier()?.GetText(),
                                                Type = fixedParameter.type_().GetText(),
                                                Identifier = fixedParameter.identifier().GetText(),
                                                DefaultArgument = fixedParameter?.default_argument()
                                                                                ?.expression()
                                                                                ?.GetText()
                                            }
                                        );
                                    }
                                }
                                var parameterArray = formalParameterList?.parameter_array();
                                if (parameterArray != null)
                                {
                                    methodDeclaration.FormalParameterList.ParameterArray = new ParameterArray()
                                    {
                                        Attributes = parameterArray?.attributes()?.GetText(),
                                        Type = parameterArray.array_type().GetText(),
                                        Identifier = parameterArray.identifier().GetText()
                                    };
                                }
                            }

                            var methodTypeParameters = 
                                interfaceMethodDeclaration?.type_parameter_list()?.type_parameter();
                            var methodConstraintsClauses = interfaceMethodDeclaration
                                ?.type_parameter_constraints_clauses()
                                ?.type_parameter_constraints_clause();

                            methodDeclaration.TypeParameters = _cSharpParserService
                                .ParseTypeParameterList(methodTypeParameters, methodConstraintsClauses);

                            interfaceDeclaration.Body.MethodDeclarations.Add(methodDeclaration);
                        }
                        else if (interfacePropertyDeclaration != null)
                        {
                            var propertyDeclaration = new PropertyDeclaration()
                            {
                                Attributes = interfacePropertyDeclaration?.attributes()?.GetText(),
                                Type = interfacePropertyDeclaration.type_().GetText(),
                                Identifier = interfacePropertyDeclaration.identifier().GetText(),
                                PropertyBody = interfacePropertyDeclaration.interface_accessors().GetText()
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
