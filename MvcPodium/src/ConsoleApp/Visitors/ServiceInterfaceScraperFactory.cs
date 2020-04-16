using System;
using System.Collections.Generic;
using System.Text;
using Antlr4.Runtime;
using Microsoft.Extensions.Options;
using MvcPodium.ConsoleApp.Model.Config;
using MvcPodium.ConsoleApp.Services;

namespace MvcPodium.ConsoleApp.Visitors
{
    public interface IServiceInterfaceScraperFactory
    {
        ServiceInterfaceScraper Create(BufferedTokenStream tokenStream);
    }

    public class ServiceInterfaceScraperFactory : IServiceInterfaceScraperFactory
    {
        private readonly IOptions<AppSettings> _appSettings;
        private readonly IOptions<UserSettings> _userSettings;
        private readonly IStringTemplateService _stringTemplateService;

        public ServiceInterfaceScraperFactory(
            IOptions<AppSettings> appSettings,
            IOptions<UserSettings> userSettings,
            IStringTemplateService stringTemplateService)
        {
            _appSettings = appSettings;
            _userSettings = userSettings;
            _stringTemplateService = stringTemplateService;
        }

        public ServiceInterfaceScraper Create(BufferedTokenStream tokenStream)
        {
            return new ServiceInterfaceScraper(
                tokenStream,
                _appSettings,
                _userSettings,
                _stringTemplateService);
        }
    }
}
