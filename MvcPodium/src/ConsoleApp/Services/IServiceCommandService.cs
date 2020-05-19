using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using MvcPodium.ConsoleApp.Models.CSharpCommon;
using MvcPodium.ConsoleApp.Models.ServiceCommand;

namespace MvcPodium.ConsoleApp.Services
{
    public interface IServiceCommandService
    {
        ServiceFile ScrapeServiceInterface(
            CSharpParserWrapper serviceInterfaceParser,
            string serviceInterfaceName,
            string serviceNamespace,
            List<TypeParameter> serviceTypeParameters);

        ServiceFile ScrapeServiceClass(
            CSharpParserWrapper serviceClassParser,
            string serviceClassName,
            string serviceNamespace,
            List<TypeParameter> serviceTypeParameters);

        string GetInterfaceServiceFileFromClass(
            ServiceFile classServiceFile,
            string serviceInterfaceName,
            string serviceNamespace);

        string GetClassServiceFileFromInterface(
            ServiceFile interfaceServiceFile,
            string serviceClassName,
            string serviceNamespace,
            List<string> appendUsingDirectives,
            ClassInterfaceBody appendClassBody);

        string InjectDataIntoServiceInterface(
            ServiceFile interfaceServiceFile,
            CSharpParserWrapper serviceInterfaceParser,
            string serviceInterfaceName,
            string tabString = null);

        string InjectDataIntoServiceClass(
            ServiceFile classServiceFile,
            CSharpParserWrapper serviceClassParser,
            string serviceClassName,
            string tabString = null);

        string RegisterServiceInStartup(
            CSharpParserWrapper startupParser,
            string rootNamespace,
            string serviceNamespace,
            string serviceClassType,
            string serviceBaseType,
            bool hasTypeParameters,
            ServiceLifetime serviceLifespan,
            string tabString = null);

        public string RegisterServicesInStartup(
            CSharpParserWrapper startupParser,
            string rootNamespace,
            List<StartupRegistrationInfo> startupRegInfoList,
            string tabString = null);

        string InjectServiceIntoConstructor(
            CSharpParserWrapper constructorInjectorParser,
            string constructorClassName,
            string constructorClassNamespace,
            string serviceIdentifier,
            string serviceNamespace,
            string serviceInterfaceType,
            FieldDeclaration fieldDeclaration,
            FixedParameter constructorParameter,
            SimpleAssignment constructorAssignment,
            ConstructorDeclaration constructorDeclaration,
            string tabString = null);
    }
}
