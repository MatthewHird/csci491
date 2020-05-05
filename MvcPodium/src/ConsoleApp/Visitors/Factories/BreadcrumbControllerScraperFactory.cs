using System;
using System.Collections.Generic;
using System.Text;
using Antlr4.Runtime;
using MvcPodium.ConsoleApp.Services;

namespace MvcPodium.ConsoleApp.Visitors.Factories
{
    public interface IBreadcrumbControllerScraperFactory
    {
        ServiceClassInjector Create(
            BufferedTokenStream tokenStream);
    }

    public class BreadcrumbControllerScraperFactory : IBreadcrumbControllerScraperFactory
    {
        private readonly IStringUtilService _stringUtilService;
        private readonly ICSharpParserService _cSharpParserService;

        public BreadcrumbControllerScraperFactory(
            IStringUtilService stringUtilService,
            ICSharpParserService cSharpParserService)
        {
            _stringUtilService = stringUtilService;
            _cSharpParserService = cSharpParserService;
        }

        public BreadcrumbControllerScraper Create(
            BufferedTokenStream tokenStream)
        {
            return new BreadcrumbControllerScraper(
                _stringUtilService,
                _cSharpParserService,
                tokenStream);
        }
    }
}
