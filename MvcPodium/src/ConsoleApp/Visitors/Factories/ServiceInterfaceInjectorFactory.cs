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
        private readonly IServiceCommandService _serviceCommandService;

        public ServiceInterfaceInjectorFactory(
            IStringUtilService stringUtilService,
            ICSharpParserService cSharpParserService,
            IServiceCommandService serviceCommandService)
        {
            _stringUtilService = stringUtilService;
            _cSharpParserService = cSharpParserService;
            _serviceCommandService = serviceCommandService;
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
                _serviceCommandService,
                tokenStream,
                serviceClassInterfaceName,
                serviceFile,
                tabString);
        }
    }
}
