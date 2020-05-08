using System.Collections.Generic;
using MvcPodium.ConsoleApp.Models.BreadcrumbCommand;

namespace MvcPodium.ConsoleApp.Services
{
    public interface IBreadcrumbCommandStgService
    {
        string RenderBreadcrumbServiceFile(
            List<string> usingDirectives,
            string breadcrumbNamespace,
            BreadcrumbServiceDeclaration breadcrumbDeclaration);

        string RenderBreadcrumbAssignment(
            string controllerRoot,
            string action,
            bool? hasId);

        string RenderBreadcrumbNamespaceDeclaration(
            string breadcrumbNamespace,
            BreadcrumbServiceDeclaration breadcrumbDeclaration);

        string RenderBreadcrumbClassDeclaration(BreadcrumbServiceDeclaration breadcrumbDeclaration);

        string RenderBreadcrumbInterfaceDeclaration(BreadcrumbServiceDeclaration breadcrumbDeclaration);

        string RenderBreadcrumbClassMethodDeclaration(BreadcrumbMethodDeclaration breadcrumbMethodDeclaration);

        string RenderBreadcrumbInterfaceMethodDeclaration(BreadcrumbMethodDeclaration breadcrumbMethodDeclaration);
    }
}
