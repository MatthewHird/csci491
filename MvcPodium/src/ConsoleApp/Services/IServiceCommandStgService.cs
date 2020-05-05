using System.Collections.Generic;
using MvcPodium.ConsoleApp.Models.CSharpCommon;
using MvcPodium.ConsoleApp.Models.ServiceCommand;

namespace MvcPodium.ConsoleApp.Services
{
    public interface IServiceCommandStgService
    {
        string RenderServiceFile(
            List<string> usingDirectives = null,
            string serviceNamespace = null,
            ClassInterfaceDeclaration service = null);

        string RenderServiceFile(
            ServiceFile serviceFile = null);

        string RenderServiceNamespaceDeclaration(
            string serviceNamespace = null,
            ClassInterfaceDeclaration service = null);

        string RenderServiceStartupRegistrationCall(
            string serviceName = null,
            bool? hasTypeParameters = null,
            string serviceLifespan = null);
    }
}
