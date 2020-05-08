using System.Collections.Generic;
using Antlr4.Runtime;
using MvcPodium.ConsoleApp.Models.CSharpCommon;
using MvcPodium.ConsoleApp.Models.ServiceCommand;
using MvcPodium.ConsoleApp.Services;

namespace MvcPodium.ConsoleApp.Visitors.Factories
{
    public interface IServiceClassInjectorFactory
    {
        public ServiceClassInjector Create(
            BufferedTokenStream tokenStream,
            string serviceClassInterfaceName,
            ServiceFile serviceFile,
            string tabString);
    }

    public class ServiceClassInjectorFactory : IServiceClassInjectorFactory
    {
        private readonly IStringUtilService _stringUtilService;
        private readonly ICSharpParserService _cSharpParserService;
        private readonly IServiceCommandParserService _serviceCommandParserService;
        public ServiceClassInjectorFactory(
            IStringUtilService stringUtilService,
            ICSharpParserService cSharpParserService,
            IServiceCommandParserService serviceCommandParserService)
        {
            _stringUtilService = stringUtilService;
            _cSharpParserService = cSharpParserService;
            _serviceCommandParserService = serviceCommandParserService;
        }

        public ServiceClassInjector Create(
            BufferedTokenStream tokenStream,
            string serviceClassInterfaceName,
            ServiceFile serviceFile,
            string tabString)
        {
            return new ServiceClassInjector(
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
