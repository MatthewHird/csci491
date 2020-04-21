using System;
using System.Collections.Generic;
using System.Text;

namespace MvcPodium.ConsoleApp.Model
{
    public class ServiceCommandScraperResults
    {
        public string Filepath { get; set; }
        public List<string> UsingDirectives { get; set; } = new List<string>();

        public Dictionary<string, ClassInterfaceDeclaration> ClassInterfaceDeclarations { get; set; } = new Dictionary<string, ClassInterfaceDeclaration>();
    }


}
