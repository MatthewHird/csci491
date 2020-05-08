using System;
using System.Collections.Generic;
using System.Text;
using MvcPodium.ConsoleApp.Models.BreadcrumbCommand;

namespace MvcPodium.ConsoleApp.Services
{
    public interface IBreadcrumbCommandParserService
    {
        string GenerateBreadcrumbNamespaceDeclaration(
            string breadcrumbNamespace,
            BreadcrumbServiceDeclaration breadcrumbDeclaration);

        string GenerateBreadcrumbClassInterfaceDeclaration(
            BreadcrumbServiceDeclaration breadcrumbDeclaration,
            int tabLevels = 0,
            string tabString = null);

        string GenerateBreadcrumbMethodDeclarations(
            List<BreadcrumbMethodDeclaration> methodDeclarations,
            int tabLevels = 0,
            string tabString = null,
            bool isInterface = false);

        string GenerateBreadcrumbAssignment(
            string controllerRootName,
            string actionName,
            bool? hasId,
            int tabLevels = 0,
            string tabString = null);
    }
}
