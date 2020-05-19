using System.Collections.Generic;
using System.IO;
using Antlr4.StringTemplate;
using Microsoft.Extensions.Options;
using MvcPodium.ConsoleApp.Models.Config;
using StgFileNames = MvcPodium.ConsoleApp.Constants.StringTemplateGroups.StgFileNames;
using StgServiceCommand = MvcPodium.ConsoleApp.Constants.StringTemplateGroups.ServiceCommand;
using MvcPodium.ConsoleApp.Models.CSharpCommon;
using MvcPodium.ConsoleApp.Models.ServiceCommand;

namespace MvcPodium.ConsoleApp.Services
{
    public class ServiceCommandStgService : IServiceCommandStgService
    {
        private readonly TemplateGroupFile _serviceCommandGroupFile;
        private readonly IOptions<AppSettings> _appSettings;

        public ServiceCommandStgService(
            IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings;
            _serviceCommandGroupFile = new TemplateGroupFile(
                Path.Combine(
                    _appSettings.Value.AssemblyDirectory,
                    _appSettings.Value.StringTemplatesDirectory,
                    StgFileNames.ServiceCommand
                )
            );
        }

        public string RenderServiceFile(
            List<string> usingDirectives,
            string serviceNamespace,
            ClassInterfaceDeclaration serviceDeclaration)
        {
            var stringTemplate = _serviceCommandGroupFile.GetInstanceOf(StgServiceCommand.ServiceFile.Name);
            stringTemplate.Add(StgServiceCommand.ServiceFile.Params.ServiceNamespace, serviceNamespace);
            stringTemplate.Add(StgServiceCommand.ServiceFile.Params.UsingDirectives, usingDirectives);
            stringTemplate.Add(StgServiceCommand.ServiceFile.Params.ServiceDeclaration, serviceDeclaration);
            return stringTemplate.Render();
        }


        public string RenderServiceFile(
            ServiceFile serviceFile = null)
        {
            return RenderServiceFile(
                serviceFile.UsingDirectives, serviceFile.ServiceNamespace, serviceFile.ServiceDeclaration);
        }


        public string RenderServiceNamespaceDeclaration(
            string serviceNamespace = null,
            ClassInterfaceDeclaration service = null)
        {
            var stringTemplate = _serviceCommandGroupFile.GetInstanceOf(
                StgServiceCommand.ServiceNamespaceDeclaration.Name);
            stringTemplate.Add(
                StgServiceCommand.ServiceNamespaceDeclaration.Params.ServiceNamespace, serviceNamespace);
            stringTemplate.Add(StgServiceCommand.ServiceNamespaceDeclaration.Params.ServiceDeclaration, service);
            return stringTemplate.Render();
        }


        public string RenderServiceStartupRegistrationCall(
            string serviceClassType = null,
            string serviceBaseType = null,
            bool? hasTypeParameters = null,
            string serviceLifespan = null)
        {
            var stringTemplate = _serviceCommandGroupFile.GetInstanceOf(
                StgServiceCommand.ServiceStartupRegistrationCall.Name);
            stringTemplate.Add(
                StgServiceCommand.ServiceStartupRegistrationCall.Params.ServiceLifespan, serviceLifespan);
            stringTemplate.Add(
                StgServiceCommand.ServiceStartupRegistrationCall.Params.HasTypeParameters, hasTypeParameters);
            stringTemplate.Add(
                StgServiceCommand.ServiceStartupRegistrationCall.Params.ServiceClassType, serviceClassType);
            stringTemplate.Add(
                StgServiceCommand.ServiceStartupRegistrationCall.Params.ServiceBaseType, serviceBaseType);
            return stringTemplate.Render();
        }
    }
}
