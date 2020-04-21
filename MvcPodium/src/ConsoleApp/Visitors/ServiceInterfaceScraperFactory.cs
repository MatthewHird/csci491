using Antlr4.Runtime;
using MvcPodium.ConsoleApp.Services;

namespace MvcPodium.ConsoleApp.Visitors
{
    public interface IServiceInterfaceScraperFactory
    {
        ServiceInterfaceScraper Create(BufferedTokenStream tokenStream, string serviceName);
    }

    public class ServiceInterfaceScraperFactory : IServiceInterfaceScraperFactory
    {
        private readonly ICSharpParserService _cSharpParserService;

        public ServiceInterfaceScraperFactory(ICSharpParserService cSharpParserService)
        {
            _cSharpParserService = cSharpParserService;
        }

        public ServiceInterfaceScraper Create(BufferedTokenStream tokenStream, string serviceRootName)
        {
            return new ServiceInterfaceScraper(
                tokenStream,
                serviceRootName,
                _cSharpParserService);
        }
    }
}
