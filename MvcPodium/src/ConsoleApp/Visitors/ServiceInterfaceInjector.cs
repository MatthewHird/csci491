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
using System.Text;
using System.Text.RegularExpressions;
using IToken = Antlr4.Runtime.IToken;
using ParserRuleContext = Antlr4.Runtime.ParserRuleContext;

namespace MvcPodium.ConsoleApp.Visitors
{
    public class ServiceInterfaceInjector : CSharpParserBaseVisitor<object>
    {
        private readonly IStringUtilService _stringUtilService;
        private readonly Stack<string> _currentNamespace;

        private readonly ServiceClassInterfaceInjectorArguments _serviceInjectorArgs;

        public bool Success { get; private set; }

        public TokenStreamRewriter Rewriter { get; }
        public BufferedTokenStream Tokens { get; }

        public ServiceInterfaceInjector(
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

        public override object VisitInterface_declaration([NotNull] CSharpParser.Interface_declarationContext context)
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

                CSharpParser.Interface_method_declarationContext finalMethod = null;
                CSharpParser.Interface_property_declarationContext finalProperty = null;
                var members = context?.interface_body()?.interface_member_declaration();
                foreach (var member in members)
                {
                    if (member.interface_method_declaration() != null)
                    {
                        finalMethod = member.interface_method_declaration();
                    }
                    else if (member.interface_property_declaration() != null)
                    {
                        finalProperty = member.interface_property_declaration();
                    }
                }

                IToken propertyStopIndex = finalProperty?.Stop ?? context.interface_body().OPEN_BRACE().Symbol;

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

                IToken methodStopIndex = finalMethod?.Stop;
                var methodStringBuilder = finalMethod is null
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
                        Rewriter.InsertAfter(methodStopIndex, methodString);
                    }
                }

            }
            VisitChildren(context);
            return null;
        }
    }
}
