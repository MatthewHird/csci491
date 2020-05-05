using Antlr4.Runtime;
using MvcPodium.ConsoleApp.Services;


namespace MvcPodium.ConsoleApp.Visitors.Factories
{
    public interface IServiceStartupRegistrationFactory
    {
        ServiceStartupRegistration Create(
            BufferedTokenStream tokenStream,
            string rootNamespace,
            string serviceNamespace,
            string serviceName,
            bool hasTypeParameters,
            string serviceLifespan,
            string tabString = null);
    }

    public class ServiceStartupRegistrationFactory : IServiceStartupRegistrationFactory
    {
        private readonly IStringUtilService _stringUtilService;
        private readonly ICSharpParserService _cSharpParserService;
        private readonly IServiceCommandStgService _serviceCommandStgService;

        public ServiceStartupRegistrationFactory(
            IStringUtilService stringUtilService,
            ICSharpParserService cSharpParserService,
            IServiceCommandStgService serviceCommandStgService)
        {
            _stringUtilService = stringUtilService;
            _cSharpParserService = cSharpParserService;
            _serviceCommandStgService = serviceCommandStgService;
        }

        public ServiceStartupRegistration Create(
            BufferedTokenStream tokenStream,
            string rootNamespace,
            string serviceNamespace,
            string serviceName,
            bool hasTypeParameters,
            string serviceLifespan,
            string tabString = null)
        {
            return new ServiceStartupRegistration(
                _stringUtilService,
                _cSharpParserService,
                _serviceCommandStgService,
                tokenStream,
                rootNamespace,
                serviceNamespace,
                serviceName,
                hasTypeParameters,
                serviceLifespan,
                tabString);
        }
    }
}
