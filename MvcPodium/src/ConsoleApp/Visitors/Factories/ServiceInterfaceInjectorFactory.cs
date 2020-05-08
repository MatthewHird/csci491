using Antlr4.Runtime;
using MvcPodium.ConsoleApp.Models.ServiceCommand;
using MvcPodium.ConsoleApp.Services;

namespace MvcPodium.ConsoleApp.Visitors.Factories
{
    public interface IServiceInterfaceInjectorFactory
    {
        ServiceInterfaceInjector Create(
            BufferedTokenStream tokenStream,
            string serviceClassInterfaceName,
            ServiceFile serviceFile,
            string tabString);
    }
    
    public class ServiceInterfaceInjectorFactory : IServiceInterfaceInjectorFactory
    {
        private readonly IStringUtilService _stringUtilService;
        private readonly ICSharpParserService _cSharpParserService;
        private readonly IServiceCommandParserService _serviceCommandParserService;

        public ServiceInterfaceInjectorFactory(
            IStringUtilService stringUtilService,
            ICSharpParserService cSharpParserService,
            IServiceCommandParserService serviceCommandParserService)
        {
            _stringUtilService = stringUtilService;
            _cSharpParserService = cSharpParserService;
            _serviceCommandParserService = serviceCommandParserService;
        }

        public ServiceInterfaceInjector Create(
            BufferedTokenStream tokenStream,
            string serviceClassInterfaceName,
            ServiceFile serviceFile,
            string tabString)
        {
            return new ServiceInterfaceInjector(
                _stringUtilService,
                _cSharpParserService,
                _serviceCommandParserService,
                tokenStream,
                serviceClassInterfaceName,
                serviceFile,
                tabString);
        }
    }
}
