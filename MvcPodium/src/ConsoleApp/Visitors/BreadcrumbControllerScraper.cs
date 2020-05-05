using System;
using System.Collections.Generic;
using System.Text;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using MvcPodium.ConsoleApp.Services;

namespace MvcPodium.ConsoleApp.Visitors
{
    public class BreadcrumbControllerScraper : CSharpParserBaseVisitor<object>
    {
        private readonly IStringUtilService _stringUtilService;
        private readonly ICSharpParserService _cSharpParserService;

        public BufferedTokenStream Tokens { get; }

        public BreadcrumbControllerScraper(
            IStringUtilService stringUtilService,
            ICSharpParserService cSharpParserService,
            BufferedTokenStream tokenStream)
        {
            _stringUtilService = stringUtilService;
            _cSharpParserService = cSharpParserService;
            Tokens = tokenStream;
        }

        public override object VisitCompilation_unit([NotNull] CSharpParser.Compilation_unitContext context)
        {
            //HasServiceClass = false;
            //Results.ServiceNamespace = _serviceNamespace;
            //foreach (var usingDirective in context.using_directive())
            //{
            //    Results.UsingDirectives.Add(
            //        _cSharpParserService.GetTextWithWhitespaceMinifiedLite(
            //            Tokens, usingDirective.using_directive_inner()));
            //}
            VisitChildren(context);
            return null;
        }

    }
}
