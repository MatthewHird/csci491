using Antlr4.Runtime;
using MvcPodium.ConsoleApp.Services;

namespace MvcPodium.ConsoleApp.Visitors.Factories
{
    public interface IServiceClassScraperFactory
    {
        ServiceClassScraper Create(BufferedTokenStream tokenStream, string serviceName);
    }

    public class ServiceClassScraperFactory : IServiceClassScraperFactory
    {
        private readonly ICSharpParserService _cSharpParserService;

        public ServiceClassScraperFactory(ICSharpParserService cSharpParserService)
        {
            _cSharpParserService = cSharpParserService;
        }

        public ServiceClassScraper Create(BufferedTokenStream tokenStream, string serviceRootName)
        {
            return new ServiceClassScraper(
                tokenStream,
                serviceRootName,
                _cSharpParserService);
        }
    }
}
