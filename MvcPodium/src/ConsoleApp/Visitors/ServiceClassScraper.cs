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
using MvcPodium.ConsoleApp.Constants.CSharpGrammar;

namespace MvcPodium.ConsoleApp.Visitors
{
    public class ServiceClassScraper : CSharpParserBaseVisitor<object>
    {
        private readonly ICSharpParserService _cSharpParserService;
        private readonly Stack<string> _currentNamespace;

        public string ServiceRootName { get; }

        public ServiceCommandScraperResults Results { get; } 
        public TokenStreamRewriter Rewriter { get; }
        public BufferedTokenStream Tokens { get; }

        public ServiceClassScraper(
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

        public override object VisitClass_declaration([NotNull] CSharpParser.Class_declarationContext context)
        {
            Match idMatch = Regex.Match(context.identifier().GetText(), $"^{Regex.Escape(ServiceRootName)}Service$");
            if (idMatch.Success)
            {
                Results.Namespace = string.Join(".", _currentNamespace.ToArray().Reverse());
                var className = idMatch.Value;
                var classDeclaration = new ClassInterfaceDeclaration()
                {
                    IsInterface = false,
                    Attributes = _cSharpParserService.GetTextWithWhitespace(context?.attributes(), Tokens),
                    Identifier = className
                };

                //Results.ClassInterfaceDeclarations.Add(className, classDeclaration);
                Results.ClassInterfaceDeclaration = classDeclaration;

                if (context?.PARTIAL()?.GetText() != null)
                {
                    classDeclaration.Modifiers.Add(context.PARTIAL().GetText());
                }

                if (context?.class_modifier() != null)
                {
                    foreach (var modifier in context.class_modifier())
                    {
                        classDeclaration.Modifiers.Add(modifier.GetText());
                    }
                }

                var typeParameters = context?.type_parameter_list()?.type_parameter();
                var constraintsClauses =
                    context?.type_parameter_constraints_clauses()?.type_parameter_constraints_clause();

                classDeclaration.TypeParameters =
                    _cSharpParserService.ParseTypeParameterList(Tokens, typeParameters, constraintsClauses);

                var baseClassType = _cSharpParserService.GetTextWithWhitespaceMinifiedLite(
                    context?.class_base()?.class_type(), Tokens);
                var interfaceTypes = context?.class_base()?.interface_type_list()?.interface_type();
                if (interfaceTypes != null || baseClassType != null)
                {
                    classDeclaration.Base = new ClassInterfaceBase();
                    if (baseClassType != null)
                    {
                        classDeclaration.Base.ClassType = baseClassType;
                    }

                    if (interfaceTypes!= null)
                    {
                        foreach (var interfaceType in interfaceTypes)
                        {
                            classDeclaration.Base.InterfaceTypeList.Add(
                                _cSharpParserService.GetTextWithWhitespaceMinifiedLite(interfaceType, Tokens));
                        }

                    }
                }

                var memberDeclarations = context?.class_body()?.class_member_declarations()?.class_member_declaration();
                if (memberDeclarations != null)
                {
                    foreach (var memberDeclaration in memberDeclarations)
                    {
                        bool isPublicOrInternal = false;

                        var classMethodDeclaration = memberDeclaration?.method_declaration();
                        var classPropertyDeclaration = memberDeclaration?.property_declaration();
                        if (classMethodDeclaration != null)
                        {
                            var methodDeclaration = new MethodDeclaration()
                            {
                                Attributes = _cSharpParserService.GetTextWithWhitespace(
                                    classMethodDeclaration?.method_header()?.attributes(), Tokens),
                                ReturnType = _cSharpParserService.GetTextWithWhitespaceMinifiedLite(
                                    classMethodDeclaration.method_header().return_type(), Tokens),
                                Identifier = classMethodDeclaration.method_header().member_name().GetText(),
                                Body = _cSharpParserService.GetTextWithWhitespaceUntab(
                                    classMethodDeclaration.method_body(), Tokens)
                            };

                            var methodModifiers = classMethodDeclaration?.method_header()?.method_modifier();
                            if (methodModifiers != null)
                            {
                                foreach (var methodMod in methodModifiers)
                                {
                                    if (methodMod.GetText() == Keywords.Public 
                                        || methodMod.GetText() == Keywords.Internal)
                                    {
                                        isPublicOrInternal = true;
                                    }
                                    methodDeclaration.Modifiers.Add(methodMod.GetText());
                                }
                            }

                            if (!isPublicOrInternal) { continue; }

                            var formalParameterList = classMethodDeclaration?.method_header()?.formal_parameter_list();
                            methodDeclaration.FormalParameterList = 
                                _cSharpParserService.ParseFormalParameterList(Tokens, formalParameterList);

                            var methodTypeParameters =
                                classMethodDeclaration?.method_header()?.type_parameter_list()?.type_parameter();
                            var methodConstraintsClauses = classMethodDeclaration
                                ?.method_header()
                                ?.type_parameter_constraints_clauses()
                                ?.type_parameter_constraints_clause();

                            methodDeclaration.TypeParameters = _cSharpParserService
                                .ParseTypeParameterList(Tokens, methodTypeParameters, methodConstraintsClauses);

                            classDeclaration.Body.MethodDeclarations.Add(methodDeclaration);
                        }
                        else if (classPropertyDeclaration != null)
                        {
                            var propertyDeclaration = new PropertyDeclaration()
                            {
                                Attributes = _cSharpParserService.GetTextWithWhitespace(
                                    classPropertyDeclaration?.attributes(), Tokens),
                                Type = _cSharpParserService.GetTextWithWhitespaceMinifiedLite(
                                    classPropertyDeclaration.type_(), Tokens),
                                Identifier = classPropertyDeclaration.member_name().GetText(),
                                Body = new PropertyBody()
                                {
                                    Text = _cSharpParserService.GetTextWithWhitespaceUntab(
                                        classPropertyDeclaration.property_body(), Tokens),
                                    HasGetAccessor = classPropertyDeclaration.property_body().expression() != null
                                                   | classPropertyDeclaration.property_body()
                                                                             ?.accessor_declarations()
                                                                             ?.get_accessor_declaration() != null,
                                    HasSetAccessor = classPropertyDeclaration.property_body()
                                                                             ?.accessor_declarations()
                                                                             ?.set_accessor_declaration() != null
                                }
                            };

                            var propertyModifiers = classPropertyDeclaration?.property_modifier();
                            if (propertyModifiers != null)
                            {
                                foreach (var propModifier in propertyModifiers)
                                {
                                    if (propModifier.GetText() == Keywords.Public
                                        || propModifier.GetText() == Keywords.Internal)
                                    {
                                        isPublicOrInternal = true;
                                    }
                                    propertyDeclaration.Modifiers.Add(propModifier.GetText());
                                }
                            }

                            if (!isPublicOrInternal) { continue; }

                            classDeclaration.Body.PropertyDeclarations.Add(propertyDeclaration);
                        }
                    }
                }
            }
            VisitChildren(context);
            return null;
        }
    }
}
