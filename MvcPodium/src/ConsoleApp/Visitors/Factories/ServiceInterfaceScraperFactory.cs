using System.Collections.Generic;
using Antlr4.Runtime;
using MvcPodium.ConsoleApp.Models.CSharpCommon;
using MvcPodium.ConsoleApp.Services;

namespace MvcPodium.ConsoleApp.Visitors.Factories
{
    public interface IServiceInterfaceScraperFactory
    {
        ServiceInterfaceScraper Create(BufferedTokenStream tokenStream,
            string serviceInterfaceName,
            string serviceNamespace,
            List<TypeParameter> typeParameters);
    }

    public class ServiceInterfaceScraperFactory : IServiceInterfaceScraperFactory
    {
        private readonly ICSharpParserService _cSharpParserService;
        private readonly IServiceCommandStgService _serviceCommandStgService;

        public ServiceInterfaceScraperFactory(
            ICSharpParserService cSharpParserService)
        {
            _cSharpParserService = cSharpParserService;
        }

        public ServiceInterfaceScraper Create(BufferedTokenStream tokenStream,
            string serviceInterfaceName,
            string serviceNamespace,
            List<TypeParameter> typeParameters)
        {
            return new ServiceInterfaceScraper(
                _cSharpParserService,
                tokenStream,
                serviceInterfaceName,
                serviceNamespace,
                typeParameters);
        }
    }
}
