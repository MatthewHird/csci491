using System.Collections.Generic;
using Antlr4.Runtime;
using MvcPodium.ConsoleApp.Models.ServiceCommand;
using MvcPodium.ConsoleApp.Services;
using static MvcPodium.ConsoleApp.Visitors.ServiceStartupRegistration;

namespace MvcPodium.ConsoleApp.Visitors.Factories
{
    public interface IServiceStartupRegistrationFactory
    {
        ServiceStartupRegistration Create(
            BufferedTokenStream tokenStream,
            string rootNamespace,
            List<StartupRegistrationInfo> startupRegInfoList,
            string tabString = null);

        ServiceStartupRegistration Create(
            BufferedTokenStream tokenStream,
            string rootNamespace,
            StartupRegistrationInfo startupRegInfo,
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
            List<StartupRegistrationInfo> startupRegInfoList,
            string tabString = null)
        {
            return new ServiceStartupRegistration(
                _stringUtilService,
                _cSharpParserService,
                _serviceCommandStgService,
                tokenStream,
                rootNamespace,
                startupRegInfoList,
                tabString);
        }

        public ServiceStartupRegistration Create(
            BufferedTokenStream tokenStream,
            string rootNamespace,
            StartupRegistrationInfo startupRegInfo,
            string tabString = null)
        {
            return Create(
                tokenStream,
                rootNamespace,
                new List<StartupRegistrationInfo>() { startupRegInfo },
                tabString);
        }
    }
}
