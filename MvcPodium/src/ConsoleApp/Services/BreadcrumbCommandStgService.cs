using System.Collections.Generic;
using System.IO;
using Antlr4.StringTemplate;
using Microsoft.Extensions.Options;
using MvcPodium.ConsoleApp.Constants.StringTemplateGroups;
using MvcPodium.ConsoleApp.Models.BreadcrumbCommand;
using MvcPodium.ConsoleApp.Models.Config;
using StgBreadcrumbCommand = MvcPodium.ConsoleApp.Constants.StringTemplateGroups.BreadcrumbCommand;

namespace MvcPodium.ConsoleApp.Services
{
    public class BreadcrumbCommandStgService : IBreadcrumbCommandStgService
    {
        private readonly TemplateGroupFile _serviceCommandGroupFile;
        private readonly IOptions<AppSettings> _appSettings;

        public BreadcrumbCommandStgService(
            IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings;
            _serviceCommandGroupFile = new TemplateGroupFile(
                Path.Combine(
                    _appSettings.Value.AssemblyDirectory,
                    _appSettings.Value.StringTemplatesDirectory,
                    StgFileNames.BreadcrumbCommand
                )
            );
        }

        public string RenderBreadcrumbServiceFile(
            List<string> usingDirectives,
            string breadcrumbNamespace,
            BreadcrumbServiceDeclaration breadcrumbDeclaration)
        {
            var stringTemplate = _serviceCommandGroupFile.GetInstanceOf(StgBreadcrumbCommand.BreadcrumbServiceFile.Name);
            stringTemplate.Add(StgBreadcrumbCommand.BreadcrumbServiceFile.Params.UsingDirectives, usingDirectives);
            stringTemplate.Add(
                StgBreadcrumbCommand.BreadcrumbServiceFile.Params.BreadcrumbNamespace, breadcrumbNamespace);
            stringTemplate.Add(
                StgBreadcrumbCommand.BreadcrumbServiceFile.Params.BreadcrumbDeclaration, breadcrumbDeclaration);
            return stringTemplate.Render();
        }

        public string RenderBreadcrumbAssignment(
            string controllerRoot,
            string action,
            bool? hasId)
        {
            var stringTemplate = _serviceCommandGroupFile.GetInstanceOf(StgBreadcrumbCommand.BreadcrumbAssignment.Name);
            stringTemplate.Add(StgBreadcrumbCommand.BreadcrumbAssignment.Params.ControllerRoot, controllerRoot);
            stringTemplate.Add(StgBreadcrumbCommand.BreadcrumbAssignment.Params.Action, action);
            stringTemplate.Add(StgBreadcrumbCommand.BreadcrumbAssignment.Params.HasId, hasId);
            return stringTemplate.Render();
        }

        public string RenderBreadcrumbNamespaceDeclaration(
            string breadcrumbNamespace,
            BreadcrumbServiceDeclaration breadcrumbDeclaration)
        {
            var stringTemplate = _serviceCommandGroupFile.GetInstanceOf(
                StgBreadcrumbCommand.BreadcrumbNamespaceDeclaration.Name);
            stringTemplate.Add(
                StgBreadcrumbCommand.BreadcrumbNamespaceDeclaration.Params.BreadcrumbNamespace,
                breadcrumbNamespace);
            stringTemplate.Add(
                StgBreadcrumbCommand.BreadcrumbNamespaceDeclaration.Params.BreadcrumbDeclaration,
                breadcrumbDeclaration);
            return stringTemplate.Render();
        }

        public string RenderBreadcrumbClassDeclaration(BreadcrumbServiceDeclaration breadcrumbDeclaration)
        {
            var stringTemplate = _serviceCommandGroupFile.GetInstanceOf(
                StgBreadcrumbCommand.BreadcrumbClassDeclaration.Name);
            stringTemplate.Add(
                StgBreadcrumbCommand.BreadcrumbClassDeclaration.Params.Attributes, breadcrumbDeclaration);
            stringTemplate.Add(
                StgBreadcrumbCommand.BreadcrumbClassDeclaration.Params.Modifiers, breadcrumbDeclaration);
            stringTemplate.Add(
                StgBreadcrumbCommand.BreadcrumbClassDeclaration.Params.Identifier, breadcrumbDeclaration);
            stringTemplate.Add(
                StgBreadcrumbCommand.BreadcrumbClassDeclaration.Params.TypeParameters, breadcrumbDeclaration);
            stringTemplate.Add(StgBreadcrumbCommand.BreadcrumbClassDeclaration.Params.Base, breadcrumbDeclaration);
            stringTemplate.Add(StgBreadcrumbCommand.BreadcrumbClassDeclaration.Params.Body, breadcrumbDeclaration);
            return stringTemplate.Render();
        }

        public string RenderBreadcrumbInterfaceDeclaration(BreadcrumbServiceDeclaration breadcrumbDeclaration)
        {
            var stringTemplate = _serviceCommandGroupFile.GetInstanceOf(
                StgBreadcrumbCommand.BreadcrumbInterfaceDeclaration.Name);
            stringTemplate.Add(
                StgBreadcrumbCommand.BreadcrumbInterfaceDeclaration.Params.Attributes, breadcrumbDeclaration);
            stringTemplate.Add(
                StgBreadcrumbCommand.BreadcrumbInterfaceDeclaration.Params.Modifiers, breadcrumbDeclaration);
            stringTemplate.Add(
                StgBreadcrumbCommand.BreadcrumbInterfaceDeclaration.Params.Identifier, breadcrumbDeclaration);
            stringTemplate.Add(
                StgBreadcrumbCommand.BreadcrumbInterfaceDeclaration.Params.TypeParameters, breadcrumbDeclaration);
            stringTemplate.Add(StgBreadcrumbCommand.BreadcrumbInterfaceDeclaration.Params.Base, breadcrumbDeclaration);
            stringTemplate.Add(StgBreadcrumbCommand.BreadcrumbInterfaceDeclaration.Params.Body, breadcrumbDeclaration);
            return stringTemplate.Render();
        }

        public string RenderBreadcrumbClassMethodDeclaration(BreadcrumbMethodDeclaration breadcrumbMethodDeclaration)
        {
            var stringTemplate = _serviceCommandGroupFile.GetInstanceOf(
                StgBreadcrumbCommand.BreadcrumbClassMethodDeclaration.Name);
            stringTemplate.Add(
                StgBreadcrumbCommand.BreadcrumbClassMethodDeclaration.Params.ControllerRoot,
                breadcrumbMethodDeclaration.ControllerRoot);
            stringTemplate.Add(
                StgBreadcrumbCommand.BreadcrumbClassMethodDeclaration.Params.Action,
                breadcrumbMethodDeclaration.Action);
            stringTemplate.Add(
                StgBreadcrumbCommand.BreadcrumbClassMethodDeclaration.Params.HasId,
                breadcrumbMethodDeclaration.HasId);
            stringTemplate.Add(
                StgBreadcrumbCommand.BreadcrumbClassMethodDeclaration.Params.Controller,
                breadcrumbMethodDeclaration.Controller);
            stringTemplate.Add(
                StgBreadcrumbCommand.BreadcrumbClassMethodDeclaration.Params.ControllerNamePattern,
                breadcrumbMethodDeclaration.ControllerNamePattern);
            return stringTemplate.Render();
        }

        public string RenderBreadcrumbInterfaceMethodDeclaration(BreadcrumbMethodDeclaration breadcrumbMethodDeclaration)
        {
            var stringTemplate = _serviceCommandGroupFile.GetInstanceOf(
                StgBreadcrumbCommand.BreadcrumbInterfaceMethodDeclaration.Name);
            stringTemplate.Add(
                StgBreadcrumbCommand.BreadcrumbInterfaceMethodDeclaration.Params.ControllerRoot,
                breadcrumbMethodDeclaration.ControllerRoot);
            stringTemplate.Add(
                StgBreadcrumbCommand.BreadcrumbInterfaceMethodDeclaration.Params.Action,
                breadcrumbMethodDeclaration.Action);
            stringTemplate.Add(
                StgBreadcrumbCommand.BreadcrumbInterfaceMethodDeclaration.Params.HasId,
                breadcrumbMethodDeclaration.HasId);
            stringTemplate.Add(
                StgBreadcrumbCommand.BreadcrumbInterfaceMethodDeclaration.Params.Controller,
                breadcrumbMethodDeclaration.Controller);
            stringTemplate.Add(
                StgBreadcrumbCommand.BreadcrumbInterfaceMethodDeclaration.Params.ControllerNamePattern,
                breadcrumbMethodDeclaration.ControllerNamePattern);
            return stringTemplate.Render();
        }
    }
}
