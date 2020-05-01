using System;
using System.Collections.Generic;
using System.Text;
using MvcPodium.ConsoleApp.Model;

namespace MvcPodium.ConsoleApp.Services
{
    public interface IServiceCommandService
    {
        (ServiceCommandScraperResults classMissingResults, ServiceCommandScraperResults interfaceMissingResults)
            CompareScraperResults(ServiceCommandScraperResults classResults, ServiceCommandScraperResults interfaceResults);

        ClassInterfaceDeclaration GetInterfaceFromClass(
            ClassInterfaceDeclaration classDeclaration,
            string interfaceIdentifier);

        ClassInterfaceDeclaration GetClassFromInterface(
            ClassInterfaceDeclaration interfaceDeclaration,
            string classIdentifier);
    }
}
