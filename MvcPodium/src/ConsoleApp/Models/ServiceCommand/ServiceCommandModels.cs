using System.Collections.Generic;
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
}
