using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using MvcPodium.ConsoleApp.Model;
using MvcPodium.ConsoleApp.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IToken = Antlr4.Runtime.IToken;

namespace MvcPodium.ConsoleApp.Visitors
{
    public class ServiceClassInjector : CSharpParserBaseVisitor<object>
    {
        private readonly IStringUtilService _stringUtilService;
        private readonly Stack<string> _currentNamespace;

        private readonly ServiceClassInterfaceInjectorArguments _serviceInjectorArgs;

        public bool Success { get; private set; }

        public TokenStreamRewriter Rewriter { get; }
        public BufferedTokenStream Tokens { get; }

        public ServiceClassInjector(
            BufferedTokenStream tokenStream,
            ServiceClassInterfaceInjectorArguments serviceInjectorArgs,
            IStringUtilService stringUtilService)
        {
            _stringUtilService = stringUtilService;
            Tokens = tokenStream;
            Rewriter = new TokenStreamRewriter(tokenStream);
            _serviceInjectorArgs = serviceInjectorArgs;
            _currentNamespace = new Stack<string>();
        }

        public override object VisitCompilation_unit([NotNull] CSharpParser.Compilation_unitContext context)
        {
            Success = false;
            IToken usingStopIndex = context?.using_directive()?.LastOrDefault()?.Stop
                ?? context?.extern_alias_directive()?.LastOrDefault()?.Stop
                ?? context?.BYTE_ORDER_MARK()?.Symbol
                ?? context.Start;

            var usingString = usingStopIndex.Equals(context.Start) ? "" : "\r\n\r\n";
            usingString += _serviceInjectorArgs.UsingDirectives.Count > 0 
                ? string.Join("\r\n", $"using {_serviceInjectorArgs.UsingDirectives};") : "";

            Rewriter.InsertAfter(usingStopIndex, usingString);

            VisitChildren(context);
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

        public override object VisitClass_declaration([NotNull] CSharpParser.Class_declarationContext context)
        {
            //Rewriter.InsertAfter(x.Stop, whitespace + serviceClass);
            var currentNamespace = string.Join(".", _currentNamespace.ToArray().Reverse());

            if (context.identifier().GetText() == _serviceInjectorArgs.ServiceClassInterfaceName 
                && currentNamespace == _serviceInjectorArgs.ServiceNamespace)
            {
                string tabString = "    ";
                var preclassWhitespace = Tokens.GetHiddenTokensToLeft(context.Start.TokenIndex, Lexer.Hidden);

                int tabLevels = 1 + (preclassWhitespace.Count > 0 ?
                    _stringUtilService.CalculateTabLevels(preclassWhitespace[0].Text, tabString) : 0);

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

                var propertyStringBuilder = new StringBuilder();

                if (_serviceInjectorArgs.PropertyDeclarations != null 
                    && _serviceInjectorArgs.PropertyDeclarations.Count > 0)
                {
                    propertyStringBuilder.Append("\r\n");
                    foreach (var property in _serviceInjectorArgs.PropertyDeclarations)
                    {
                        propertyStringBuilder.Append("\r\n");
                        propertyStringBuilder.Append(_stringUtilService.TabString(property, tabLevels, tabString));
                        propertyStringBuilder.Append("\r\n");
                    }
                }

                int? methodStopIndex = finalMethod ?? finalConstructorOrDestructor;
                var methodStringBuilder = methodStopIndex is null
                    ? propertyStringBuilder : new StringBuilder();

                if (_serviceInjectorArgs.MethodDeclarations != null 
                    && _serviceInjectorArgs.MethodDeclarations.Count > 0)
                {
                    methodStringBuilder.Append("\r\n");
                    foreach (var method in _serviceInjectorArgs.MethodDeclarations)
                    {
                        methodStringBuilder.Append("\r\n");
                        methodStringBuilder.Append(_stringUtilService.TabString(method, tabLevels, tabString));
                        methodStringBuilder.Append("\r\n");
                    }
                }

                var propertyString = propertyStringBuilder.ToString();
                if (propertyString.Length > 0)
                {
                    Rewriter.InsertAfter(propertyStopIndex, propertyString);
                }

                if (methodStopIndex != null)
                {
                    var methodString = methodStringBuilder.ToString();
                    if (methodString.Length > 0)
                    {
                        Rewriter.InsertAfter(Tokens.Get(methodStopIndex ?? -1), methodString);
                    }
                }
            }
            VisitChildren(context);
            return null;
        }
    }
}
