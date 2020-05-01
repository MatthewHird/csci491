using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Antlr4.StringTemplate;
using Microsoft.Extensions.Options;
using MvcPodium.ConsoleApp.Constants.StringTemplateGroups;
using MvcPodium.ConsoleApp.Model;
using MvcPodium.ConsoleApp.Model.Config;
using MvcPodium.ConsoleApp.Constants.CSharpGrammar;
using StgFileNames = MvcPodium.ConsoleApp.Constants.StringTemplateGroups.StgFileNames;
using StgServiceCommand = MvcPodium.ConsoleApp.Constants.StringTemplateGroups.ServiceCommand;

namespace MvcPodium.ConsoleApp.Services
{
    public class ServiceCommandStService : IServiceCommandStService
    {
        private readonly TemplateGroupFile _serviceCommandGroupFile;
        private readonly IOptions<AppSettings> _appSettings;

        public ServiceCommandStService(
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
            string serviceNamespace = null,
            List<string> usingDirectives = null,
            ClassInterfaceDeclaration service = null)
        {
            var stringTemplate = _serviceCommandGroupFile.GetInstanceOf(StgServiceCommand.ServiceFile.Name);
            stringTemplate.Add(StgServiceCommand.ServiceFile.Params.ServiceNamespace, serviceNamespace);
            stringTemplate.Add(StgServiceCommand.ServiceFile.Params.UsingDirectives, usingDirectives);
            stringTemplate.Add(StgServiceCommand.ServiceFile.Params.Service, service);
            return stringTemplate.Render();
        }


        public string RenderTypeParamList(List<TypeParameter> typeParamList = null)
        {
            var stringTemplate = _serviceCommandGroupFile.GetInstanceOf(StgServiceCommand.TypeParamList.Name);
            stringTemplate.Add(StgServiceCommand.TypeParamList.Params.TypeParamList, typeParamList);
            return stringTemplate.Render();
        }


        public string RenderClassMethodDeclaration(MethodDeclaration method = null)
        {
            var stringTemplate = _serviceCommandGroupFile.GetInstanceOf(
                StgServiceCommand.ClassMethodDeclaration.Name);
            stringTemplate.Add(StgServiceCommand.ClassMethodDeclaration.Params.Method, method);
            return stringTemplate.Render();
        }


        public string RenderClassPropertyDeclaration(PropertyDeclaration property = null)
        {
            var stringTemplate = _serviceCommandGroupFile.GetInstanceOf(
                StgServiceCommand.ClassPropertyDeclaration.Name);
            stringTemplate.Add(StgServiceCommand.ClassPropertyDeclaration.Params.Property, property);
            return stringTemplate.Render();
        }


        public string RenderInterfaceMethodDeclaration(MethodDeclaration method = null)
        {
            var stringTemplate = _serviceCommandGroupFile.GetInstanceOf(
                StgServiceCommand.InterfaceMethodDeclaration.Name);
            stringTemplate.Add(StgServiceCommand.InterfaceMethodDeclaration.Params.Method, method);
            return stringTemplate.Render();
        }


        public string RenderInterfacePropertyDeclaration(PropertyDeclaration property = null)
        {
            var stringTemplate = _serviceCommandGroupFile.GetInstanceOf(
                StgServiceCommand.InterfacePropertyDeclaration.Name);
            stringTemplate.Add(StgServiceCommand.InterfacePropertyDeclaration.Params.Property, property);
            return stringTemplate.Render();
        }


        public string RenderServiceStartupRegistrationCall(ServiceRegistrationInfo serviceRegistrationInfo = null)
        {
            var stringTemplate = _serviceCommandGroupFile.GetInstanceOf(
                StgServiceCommand.ServiceStartupRegistrationCall.Name);
            stringTemplate.Add(
                StgServiceCommand.ServiceStartupRegistrationCall.Params.ServiceRegistrationInfo,
                serviceRegistrationInfo);
            return stringTemplate.Render();
        }

        public string RenderFieldDeclaration(FieldDeclaration field = null)
        {
            var stringTemplate = _serviceCommandGroupFile.GetInstanceOf(StgServiceCommand.FieldDeclaration.Name);
            stringTemplate.Add(StgServiceCommand.FieldDeclaration.Params.Field, field);
            return stringTemplate.Render();
        }

        public string RenderFixedParameter(FixedParameter fixedParam = null)
        {
            var stringTemplate = _serviceCommandGroupFile.GetInstanceOf(StgServiceCommand.FixedParameter.Name);
            stringTemplate.Add(StgServiceCommand.FixedParameter.Params.FixedParam, fixedParam);
            return stringTemplate.Render();
        }

        public string RenderSimpleAssignment(SimpleAssignment simpleAssignment = null)
        {
            var stringTemplate = _serviceCommandGroupFile.GetInstanceOf(StgServiceCommand.SimpleAssignmentStatement.Name);
            stringTemplate.Add(StgServiceCommand.SimpleAssignmentStatement.Params.SimpleAssignment, simpleAssignment);
            return stringTemplate.Render();
        }

        public string RenderConstructorDeclaration(ConstructorDeclaration constructor = null)
        {
            var stringTemplate = _serviceCommandGroupFile.GetInstanceOf(StgServiceCommand.ConstructorDeclaration.Name);
            stringTemplate.Add(StgServiceCommand.ConstructorDeclaration.Params.Constructor, constructor);
            return stringTemplate.Render();
        }
    }
}
