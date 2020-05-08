using System;
using System.Collections.Generic;
using System.Text;
using MvcPodium.ConsoleApp.Models.BreadcrumbCommand;

namespace MvcPodium.ConsoleApp.Services
{
    public class BreadcrumbCommandParserService : IBreadcrumbCommandParserService
    {
        private readonly IStringUtilService _stringUtilService;
        private readonly ICSharpCommonStgService _cSharpCommonStgService;
        private readonly IBreadcrumbCommandStgService _breadcrumbCommandStgService;

        public BreadcrumbCommandParserService(
            IStringUtilService stringUtilService,
            ICSharpCommonStgService cSharpCommonStgService,
            IBreadcrumbCommandStgService breadcrumbCommandStgService)
        {
            _stringUtilService = stringUtilService;
            _cSharpCommonStgService = cSharpCommonStgService;
            _breadcrumbCommandStgService = breadcrumbCommandStgService;
        }

        public string GenerateBreadcrumbNamespaceDeclaration(
            string breadcrumbNamespace,
            BreadcrumbServiceDeclaration breadcrumbDeclaration)
        {
            if (breadcrumbDeclaration is null) { return string.Empty; }
            return "\r\n\r\n" +
                _breadcrumbCommandStgService.RenderBreadcrumbNamespaceDeclaration(
                    breadcrumbNamespace,
                    breadcrumbDeclaration);
        }

        public string GenerateBreadcrumbClassInterfaceDeclaration(
            BreadcrumbServiceDeclaration breadcrumbDeclaration,
            int tabLevels = 0,
            string tabString = null)
        {
            if (breadcrumbDeclaration is null) { return string.Empty; }
            return "\r\n\r\n" +
                _stringUtilService.TabString(breadcrumbDeclaration.IsInterface
                    ? _breadcrumbCommandStgService.RenderBreadcrumbInterfaceDeclaration(breadcrumbDeclaration)
                    : _breadcrumbCommandStgService.RenderBreadcrumbClassDeclaration(breadcrumbDeclaration),
                tabLevels,
                tabString);
        }

        public string GenerateBreadcrumbMethodDeclarations(
            List<BreadcrumbMethodDeclaration> methodDeclarations,
            int tabLevels = 0,
            string tabString = null,
            bool isInterface = false)
        {
            var methodStringBuilder = new StringBuilder();
            if (methodDeclarations != null && methodDeclarations.Count > 0)
            {
                methodStringBuilder.Append("\r\n");
                foreach (var method in methodDeclarations)
                {
                    methodStringBuilder.Append("\r\n");
                    methodStringBuilder.Append(_stringUtilService.TabString(
                        isInterface
                            ? _breadcrumbCommandStgService.RenderBreadcrumbInterfaceMethodDeclaration(method)
                            : _breadcrumbCommandStgService.RenderBreadcrumbClassMethodDeclaration(method),
                        tabLevels,
                        tabString));
                    methodStringBuilder.Append("\r\n");
                }
            }
            return methodStringBuilder.ToString();
        }

        public string GenerateBreadcrumbAssignment(
            string controllerRootName,
            string actionName,
            bool? hasId,
            int tabLevels = 0,
            string tabString = null)
        {
            return "\r\n" +
                _stringUtilService.TabString(
                    _breadcrumbCommandStgService.RenderBreadcrumbAssignment(controllerRootName, actionName, hasId),
                    tabLevels,
                    tabString) +
                "\r\n";
        }
    }
}
