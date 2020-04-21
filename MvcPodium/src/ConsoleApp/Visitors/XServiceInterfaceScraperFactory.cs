using System;
using System.Collections.Generic;
using System.Text;
using Antlr4.Runtime;
using Microsoft.Extensions.Options;
using MvcPodium.ConsoleApp.Model.Config;
using MvcPodium.ConsoleApp.Services;

namespace MvcPodium.ConsoleApp.Visitors
{
    public interface IXServiceInterfaceScraperFactory
    {
        XServiceInterfaceScraper Create(BufferedTokenStream tokenStream, string serviceName);
    }

    public class XServiceInterfaceScraperFactory : IXServiceInterfaceScraperFactory
    {
        private readonly IOptions<AppSettings> _appSettings;
        private readonly IOptions<UserSettings> _userSettings;
        private readonly IStringTemplateService _stringTemplateService;
        private readonly ICSharpParserService _cSharpParserService;

        public XServiceInterfaceScraperFactory(
            IOptions<AppSettings> appSettings,
            IOptions<UserSettings> userSettings,
            IStringTemplateService stringTemplateService,
            ICSharpParserService cSharpParserService)
        {
            _appSettings = appSettings;
            _userSettings = userSettings;
            _stringTemplateService = stringTemplateService;
            _cSharpParserService = cSharpParserService;
        }

        public XServiceInterfaceScraper Create(BufferedTokenStream tokenStream, string serviceRootName)
        {
            return new XServiceInterfaceScraper(
                tokenStream,
                serviceRootName,
                _appSettings,
                _userSettings,
                _stringTemplateService);
        }
    }
}
