using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using MvcPodium.ConsoleApp.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using MvcPodium.ConsoleApp.Constants.CSharpGrammar;
using MvcPodium.ConsoleApp.Models.CSharpCommon;
using MvcPodium.ConsoleApp.Models.ServiceCommand;

namespace MvcPodium.ConsoleApp.Visitors
{
    public class ServiceClassScraper : CSharpParserBaseVisitor<object>
    {
        private readonly ICSharpParserService _cSharpParserService;
        private readonly Stack<string> _currentNamespace;
        private readonly string _serviceNamespace;

        public string ServiceClassName { get; }
        public List<TypeParameter> TypeParameters { get; }

        public ServiceFile Results { get; } 
        public TokenStreamRewriter Rewriter { get; }
        public BufferedTokenStream Tokens { get; }

        public bool HasServiceClass { get; private set; }

        public ServiceClassScraper(
            ICSharpParserService cSharpParserService,
            BufferedTokenStream tokenStream,
            string serviceClassName,
            string serviceNamespace,
            List<TypeParameter> typeParameters)
        {
            _cSharpParserService = cSharpParserService;
            Tokens = tokenStream;
            ServiceClassName = serviceClassName;
            TypeParameters = typeParameters;
            _serviceNamespace = serviceNamespace;
            Rewriter = new TokenStreamRewriter(tokenStream);
            Results = new ServiceFile();
            _currentNamespace = new Stack<string>();
            HasServiceClass = false;
        }

        public override object VisitCompilation_unit([NotNull] CSharpParser.Compilation_unitContext context)
        {
            HasServiceClass = false;
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

        public override object VisitClass_declaration([NotNull] CSharpParser.Class_declarationContext context)
        {
            bool matchNames = context.identifier().GetText() == ServiceClassName;
            bool isTypeParamsMatch = true;
            var typeParams = context?.type_parameter_list()?.type_parameter();
            if (!(typeParams is null && (TypeParameters is null || TypeParameters.Count == 0)))
            {
                if ((typeParams?.Length ?? 0) != (TypeParameters?.Count ?? 0))
                {
                    isTypeParamsMatch = false;
                }
                else
                {
                    for (int i = 0; i < typeParams.Length; ++i)
                    {
                        if (typeParams[i].identifier().GetText() != TypeParameters[i].TypeParam)
                        {
                            isTypeParamsMatch = false;
                            break;
                        }
                    }
                }
            }

            if (matchNames && isTypeParamsMatch)
            {
                HasServiceClass = true;
                Results.ServiceNamespace = string.Join(".", _currentNamespace.ToArray().Reverse());
                var classDeclaration = new ClassInterfaceDeclaration()
                {
                    IsInterface = false,
                    Attributes = _cSharpParserService.GetTextWithWhitespace(Tokens, context?.attributes()),
                    Identifier = ServiceClassName
                };

                //Results.ClassInterfaceDeclarations.Add(className, classDeclaration);
                Results.ServiceDeclaration = classDeclaration;

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
                    Tokens, context?.class_base()?.class_type());
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
                                _cSharpParserService.GetTextWithWhitespaceMinifiedLite(Tokens, interfaceType));
                        }

                    }
                }

                var memberDeclarations = context?.class_body()?.class_member_declarations()?.class_member_declaration();
                if (memberDeclarations != null)
                {
                    foreach (var memberDeclaration in memberDeclarations)
                    {
                        bool isPublic = false;

                        var classMethodDeclaration = memberDeclaration?.method_declaration();
                        var classPropertyDeclaration = memberDeclaration?.property_declaration();
                        if (classMethodDeclaration != null)
                        {
                            var methodDeclaration = new MethodDeclaration()
                            {
                                Attributes = _cSharpParserService.GetTextWithWhitespace(
                                    Tokens, classMethodDeclaration?.method_header()?.attributes()),
                                ReturnType = _cSharpParserService.GetTextWithWhitespaceMinifiedLite(
                                    Tokens, classMethodDeclaration.method_header().return_type()),
                                Identifier = classMethodDeclaration.method_header().member_name().GetText(),
                                Body = _cSharpParserService.GetTextWithWhitespaceUntab(
                                    Tokens, classMethodDeclaration.method_body())
                            };

                            var methodModifiers = classMethodDeclaration?.method_header()?.method_modifier();
                            if (methodModifiers != null)
                            {
                                foreach (var methodMod in methodModifiers)
                                {
                                    if (methodMod.GetText() == Keywords.Public)
                                    {
                                        isPublic = true;
                                    }
                                    methodDeclaration.Modifiers.Add(methodMod.GetText());
                                }
                            }

                            if (!isPublic) { continue; }

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
                                    Tokens, classPropertyDeclaration?.attributes()),
                                Type = _cSharpParserService.GetTextWithWhitespaceMinifiedLite(
                                    Tokens, classPropertyDeclaration.type_()),
                                Identifier = classPropertyDeclaration.member_name().GetText(),
                                Body = new PropertyBody()
                                {
                                    Text = _cSharpParserService.GetTextWithWhitespaceUntab(
                                        Tokens, classPropertyDeclaration.property_body()),
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
                                    if (propModifier.GetText() == Keywords.Public)
                                    {
                                        isPublic = true;
                                    }
                                    propertyDeclaration.Modifiers.Add(propModifier.GetText());
                                }
                            }

                            if (!isPublic) { continue; }

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
