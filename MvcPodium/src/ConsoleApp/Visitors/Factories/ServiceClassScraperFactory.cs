using System.Collections.Generic;
using Antlr4.Runtime;
using MvcPodium.ConsoleApp.Models.CSharpCommon;
using MvcPodium.ConsoleApp.Services;

namespace MvcPodium.ConsoleApp.Visitors.Factories
{
    public interface IServiceClassScraperFactory
    {
        ServiceClassScraper Create(BufferedTokenStream tokenStream,
            string serviceClassName,
            string serviceNamespace,
            List<TypeParameter> typeParameters);
    }

    public class ServiceClassScraperFactory : IServiceClassScraperFactory
    {
        private readonly ICSharpParserService _cSharpParserService;

        public ServiceClassScraperFactory(ICSharpParserService cSharpParserService)
        {
            _cSharpParserService = cSharpParserService;
        }

        public ServiceClassScraper Create(
            BufferedTokenStream tokenStream, 
            string serviceClassName,
            string serviceNamespace,
            List<TypeParameter> typeParameters)
        {
            return new ServiceClassScraper(
                _cSharpParserService,
                tokenStream,
                serviceClassName,
                serviceNamespace,
                typeParameters);
        }
    }
}
