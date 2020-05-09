using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using MvcPodium.ConsoleApp.Models.CSharpCommon;
using MvcPodium.ConsoleApp.Models.ServiceCommand;
using MvcPodium.ConsoleApp.Visitors.Factories;

namespace MvcPodium.ConsoleApp.Services
{
    public class ServiceCommandService : IServiceCommandService
    {
        private readonly IServiceCommandStgService _serviceCommandStgService;
        private readonly IServiceCommandParserService _serviceCommandParserService;
        private readonly IServiceInterfaceScraperFactory _serviceInterfaceScraperFactory;
        private readonly IServiceClassScraperFactory _serviceClassScraperFactory;
        private readonly IServiceInterfaceInjectorFactory _serviceInterfaceInjectorFactory;
        private readonly IServiceClassInjectorFactory _serviceClassInjectorFactory;
        private readonly IServiceStartupRegistrationFactory _serviceStartupRegistrationFactory;
        private readonly IServiceConstructorInjectorFactory _serviceConstructorInjectorFactory;

        public ServiceCommandService(
            IServiceCommandStgService serviceCommandStgService,
            IServiceCommandParserService serviceCommandParserService,
            IServiceInterfaceScraperFactory serviceInterfaceScraperFactory,
            IServiceClassScraperFactory serviceClassScraperFactory,
            IServiceInterfaceInjectorFactory serviceInterfaceInjectorFactory,
            IServiceClassInjectorFactory serviceClassInjectorFactory,
            IServiceStartupRegistrationFactory serviceStartupRegistrationFactory,
            IServiceConstructorInjectorFactory serviceConstructorInjectorFactory)
        {
            _serviceCommandStgService = serviceCommandStgService;
            _serviceCommandParserService = serviceCommandParserService;
            _serviceInterfaceScraperFactory = serviceInterfaceScraperFactory;
            _serviceClassScraperFactory = serviceClassScraperFactory;
            _serviceInterfaceInjectorFactory = serviceInterfaceInjectorFactory;
            _serviceClassInjectorFactory = serviceClassInjectorFactory;
            _serviceStartupRegistrationFactory = serviceStartupRegistrationFactory;
            _serviceConstructorInjectorFactory = serviceConstructorInjectorFactory;
        }

        public ServiceFile ScrapeServiceInterface(
            CSharpParserWrapper serviceInterfaceParser,
            string serviceInterfaceName,
            string serviceNamespace,
            List<TypeParameter> serviceTypeParameters)
        {
            var tree = serviceInterfaceParser.GetParseTree();
            var visitor = _serviceInterfaceScraperFactory.Create(
                serviceInterfaceParser.Tokens,
                serviceInterfaceName,
                serviceNamespace,
                serviceTypeParameters);
            visitor.Visit(tree);
            return visitor.Results;
        }

        public ServiceFile ScrapeServiceClass(
            CSharpParserWrapper serviceClassParser,
            string serviceClassName,
            string serviceNamespace,
            List<TypeParameter> serviceTypeParameters)
        {
            var tree = serviceClassParser.GetParseTree();
            var visitor = _serviceClassScraperFactory.Create(
                serviceClassParser.Tokens,
                serviceClassName,
                serviceNamespace,
                serviceTypeParameters);
            visitor.Visit(tree);
            return visitor.Results;
        }

        public string GetInterfaceServiceFileFromClass(
            ServiceFile classServiceFile,
            string serviceInterfaceName,
            string serviceNamespace)
        {
            var interfaceDeclaration = _serviceCommandParserService.GetInterfaceFromClass(
                classServiceFile.ServiceDeclaration,
                serviceInterfaceName);

            return _serviceCommandStgService.RenderServiceFile(
                usingDirectives: classServiceFile.UsingDirectives,
                serviceNamespace: serviceNamespace,
                service: interfaceDeclaration);
        }

        public string GetClassServiceFileFromInterface(
            ServiceFile interfaceServiceFile,
            string serviceClassName,
            string serviceNamespace,
            List<string> appendUsingDirectives,
            ClassInterfaceBody appendClassBody)
        {
            var usingSet = interfaceServiceFile.UsingDirectives.ToHashSet();
            usingSet.UnionWith(appendUsingDirectives);
            usingSet.Remove(serviceNamespace);

            var classDeclaration = _serviceCommandParserService.GetClassFromInterface(
                interfaceServiceFile.ServiceDeclaration,
                serviceClassName);

            var fieldDeclarations = appendClassBody?.FieldDeclarations;

            classDeclaration.Body.ConstructorDeclaration = appendClassBody?.ConstructorDeclaration;
            if (fieldDeclarations != null)
            {
                classDeclaration.Body.FieldDeclarations = fieldDeclarations;
            }

            return _serviceCommandStgService.RenderServiceFile(
                usingDirectives: usingSet.ToList(),
                serviceNamespace: serviceNamespace,
                service: classDeclaration);
        }

        public string InjectDataIntoServiceInterface(
            ServiceFile interfaceServiceFile,
            CSharpParserWrapper serviceInterfaceParser,
            string serviceInterfaceName,
            string tabString = null)
        {
            var tree = serviceInterfaceParser.GetParseTree();
            var visitor = _serviceInterfaceInjectorFactory.Create(
                serviceInterfaceParser.Tokens,
                serviceClassInterfaceName: serviceInterfaceName,
                serviceFile: interfaceServiceFile,
                tabString: tabString
            );
            visitor.Visit(tree);

            return visitor.IsModified ? visitor.Rewriter.GetText() : null;
        }

        public string InjectDataIntoServiceClass(
            ServiceFile classServiceFile,
            CSharpParserWrapper serviceClassParser,
            string serviceClassName,
            string tabString = null)
        {
            var tree = serviceClassParser.GetParseTree();
            var visitor = _serviceClassInjectorFactory.Create(
                tokenStream: serviceClassParser.Tokens,
                serviceClassInterfaceName: serviceClassName,
                serviceFile: classServiceFile,
                tabString: tabString
            );
            visitor.Visit(tree);

            return visitor.IsModified ? visitor.Rewriter.GetText() : null;
        }

        public string RegisterServiceInStartup(
            CSharpParserWrapper startupParser,
            string rootNamespace,
            string serviceNamespace,
            string serviceName,
            bool hasTypeParameters,
            ServiceLifetime serviceLifespan,
            string tabString = null)
        {
            var startupTree = startupParser.GetParseTree();

            var serviceStartupRegistration = _serviceStartupRegistrationFactory.Create(
                tokenStream: startupParser.Tokens,
                rootNamespace: rootNamespace,
                startupRegInfo: new StartupRegistrationInfo()
                {
                    ServiceNamespace = serviceNamespace,
                    ServiceName = serviceName,
                    HasTypeParameters = hasTypeParameters,
                    ServiceLifespan = serviceLifespan
                },
                tabString: tabString);
            serviceStartupRegistration.Visit(startupTree);

            return serviceStartupRegistration.IsModified ? serviceStartupRegistration.Rewriter.GetText() : null;
        }

        public string RegisterServicesInStartup(
            CSharpParserWrapper startupParser,
            string rootNamespace,
            List<StartupRegistrationInfo> startupRegInfoList,
            string tabString = null)
        {
            var startupTree = startupParser.GetParseTree();

            var serviceStartupRegistration = _serviceStartupRegistrationFactory.Create(
                tokenStream: startupParser.Tokens,
                rootNamespace: rootNamespace,
                startupRegInfoList: startupRegInfoList,
                tabString: tabString);
            serviceStartupRegistration.Visit(startupTree);

            return serviceStartupRegistration.IsModified ? serviceStartupRegistration.Rewriter.GetText() : null;
        }

        public string InjectServiceIntoController(
            CSharpParserWrapper controllerInjectorParser,
            string constructorClassName,
            string constructorClassNamespace,
            string serviceIdentifier,
            string serviceNamespace,
            string serviceInterfaceType,
            FieldDeclaration fieldDeclaration,
            FixedParameter constructorParameter,
            SimpleAssignment constructorAssignment,
            ConstructorDeclaration constructorDeclaration,
            string tabString = null)
        {
            var controllerInjectorTree = controllerInjectorParser.GetParseTree();

            var serviceControllerInjector = _serviceConstructorInjectorFactory.Create(
                controllerInjectorParser.Tokens,
                constructorClassName: constructorClassName,
                constructorClassNamespace: constructorClassNamespace,
                serviceIdentifier: serviceIdentifier,
                serviceNamespace: serviceNamespace,
                serviceInterfaceType: serviceInterfaceType,
                fieldDeclaration: fieldDeclaration,
                constructorParameter: constructorParameter,
                constructorAssignment: constructorAssignment,
                constructorDeclaration: constructorDeclaration,
                tabString: tabString);
            serviceControllerInjector.Visit(controllerInjectorTree);

            return serviceControllerInjector.IsModified ? serviceControllerInjector.Rewriter.GetText() : null;
        }
    }
}
