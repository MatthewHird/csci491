using System;
using System.Collections.Generic;
using System.Text;
using Antlr4.Runtime;
using MvcPodium.ConsoleApp.Model;
using MvcPodium.ConsoleApp.Services;


namespace MvcPodium.ConsoleApp.Visitors.Factories
{
    public interface IServiceStartupRegistrationFactory
    {
        ServiceStartupRegistration Create(
            BufferedTokenStream tokenStream,
            ServiceStartupRegistrationArguments serviceRegistrationArgs);
    }

    public class ServiceStartupRegistrationFactory : IServiceStartupRegistrationFactory
    {
        private readonly IStringUtilService _stringUtilService;
        private readonly ICSharpParserService _cSharpParserService;

        public ServiceStartupRegistrationFactory(
            IStringUtilService stringUtilService,
            ICSharpParserService cSharpParserService)
        {
            _stringUtilService = stringUtilService;
            _cSharpParserService = cSharpParserService;
        }

        public ServiceStartupRegistration Create(
            BufferedTokenStream tokenStream,
            ServiceStartupRegistrationArguments serviceRegistrationArgs)
        {
            return new ServiceStartupRegistration(
                tokenStream,
                serviceRegistrationArgs,
                _stringUtilService,
                _cSharpParserService);
        }
    }
}
