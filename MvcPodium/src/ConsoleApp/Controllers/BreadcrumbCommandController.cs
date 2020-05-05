using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MvcPodium.ConsoleApp.Controller;
using MvcPodium.ConsoleApp.Models.Config;
using MvcPodium.ConsoleApp.Services;
using MvcPodium.ConsoleApp.Visitors.Factories;

namespace MvcPodium.ConsoleApp.Controllers
{
    public class BreadcrumbCommandController
    {
        private readonly ILogger<MvcPodiumController> _logger;
        private readonly IOptions<CommandLineArgs> _commandLineArgs;
        private readonly IOptions<ProjectEnvironment> _projectEnvironment;
        private readonly IOptions<UserSettings> _userSettings;
        private readonly ICSharpCommonStgService _cSharpCommonStgService;
        private readonly IServiceCommandStgService _serviceCommandStgService;
        private readonly IBreadcrumbCommandStgService _breadcrumbCommandStgService;
        private readonly IServiceInterfaceScraperFactory _serviceInterfaceScraperFactory;
        private readonly IServiceClassScraperFactory _serviceClassScraperFactory;
        private readonly IServiceInterfaceInjectorFactory _serviceInterfaceInjectorFactory;
        private readonly IServiceClassInjectorFactory _serviceClassInjectorFactory;
        private readonly IServiceCommandService _serviceCommandService;
        private readonly IServiceStartupRegistrationFactory _serviceStartupRegistrationFactory;
        private readonly IServiceConstructorInjectorFactory _serviceConstructorInjectorFactory;
        private readonly IBreadcrumbControllerScraperFactory _breadcrumbControllerScraperFactory;

        public BreadcrumbCommandController(
            ILogger<MvcPodiumController> logger,
            IOptions<CommandLineArgs> commandLineArgs,
            IOptions<ProjectEnvironment> projectEnvironment,
            IOptions<UserSettings> userSettings,
            ICSharpCommonStgService cSharpCommonStgService,
            IServiceCommandStgService serviceCommandStgService,
            IBreadcrumbCommandStgService breadcrumbCommandStgService,
            IServiceInterfaceScraperFactory serviceInterfaceScraperFactory,
            IServiceClassScraperFactory serviceClassScraperFactory,
            IServiceInterfaceInjectorFactory serviceInterfaceInjectorFactory,
            IServiceClassInjectorFactory serviceClassInjectorFactory,
            IServiceCommandService serviceCommandService,
            IServiceStartupRegistrationFactory serviceStartupRegistrationFactory,
            IServiceConstructorInjectorFactory serviceControllerInjectorFactory,
            IBreadcrumbControllerScraperFactory breadcrumbControllerScraperFactory)
        {
            _logger = logger;
            _commandLineArgs = commandLineArgs;
            _projectEnvironment = projectEnvironment;
            _userSettings = userSettings;
            _cSharpCommonStgService = cSharpCommonStgService;
            _serviceCommandStgService = serviceCommandStgService;
            _breadcrumbCommandStgService = breadcrumbCommandStgService;
            _serviceInterfaceScraperFactory = serviceInterfaceScraperFactory;
            _serviceClassScraperFactory = serviceClassScraperFactory;
            _serviceInterfaceInjectorFactory = serviceInterfaceInjectorFactory;
            _serviceClassInjectorFactory = serviceClassInjectorFactory;
            _serviceCommandService = serviceCommandService;
            _serviceStartupRegistrationFactory = serviceStartupRegistrationFactory;
            _serviceConstructorInjectorFactory = serviceControllerInjectorFactory;
            _breadcrumbControllerScraperFactory = breadcrumbControllerScraperFactory;
        }

        public Task Execute(ServiceCommand serviceCommand)
        {


            return Task.CompletedTask;
        }
    }
}
