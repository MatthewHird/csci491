using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using MvcPodium.ConsoleApp.Models.CSharpCommon;

namespace MvcPodium.ConsoleApp.Models.ServiceCommand
{
    public class ServiceFile
    {
        public string Filepath { get; set; }
        public List<string> UsingDirectives { get; set; } = new List<string>();
        public string ServiceNamespace { get; set; }

        //public Dictionary<string, ClassInterfaceDeclaration> ClassInterfaceDeclarations { get; set; } = new Dictionary<string, ClassInterfaceDeclaration>();
        public ClassInterfaceDeclaration ServiceDeclaration { get; set; }
    }

    public class StartupRegistrationInfo
    {
        public string ServiceNamespace { get; set; }
        public string ServiceClassType { get; set; }
        public string ServiceBaseType { get; set; }
        public bool HasTypeParameters { get; set; }
        public ServiceLifetime ServiceLifespan { get; set; }
    }

    public enum SourceOfTruth
    {
        Class,
        Interface,
        Match
    }
}
