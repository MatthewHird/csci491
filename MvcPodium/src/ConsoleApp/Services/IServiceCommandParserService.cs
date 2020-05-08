using MvcPodium.ConsoleApp.Models.CSharpCommon;
using MvcPodium.ConsoleApp.Models.ServiceCommand;

namespace MvcPodium.ConsoleApp.Services
{
    public interface IServiceCommandParserService
    {
        (ServiceFile classMissingResults, ServiceFile interfaceMissingResults)
            CompareScraperResults(ServiceFile classResults, ServiceFile interfaceResults);

        ClassInterfaceDeclaration GetInterfaceFromClass(
            ClassInterfaceDeclaration classDeclaration,
            string interfaceIdentifier);

        ClassInterfaceDeclaration GetClassFromInterface(
            ClassInterfaceDeclaration interfaceDeclaration,
            string classIdentifier);

        string GenerateServiceNamespaceDeclaration(
            string serviceNamespace,
            ClassInterfaceDeclaration serviceDeclaration);
    }
}
