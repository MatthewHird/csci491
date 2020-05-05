using System;
using System.Collections.Generic;
using System.Text;
using MvcPodium.ConsoleApp.Models.CSharpCommon;

namespace MvcPodium.ConsoleApp.Models.Config
{
    public partial class CommandSet
    {
        public List<ServiceCommand> ServiceCommands { get; set; }
    }

    public class ServiceCommand
    {
        public string ServiceRootName { get; set; }
        public string Area { get; set; }
        public List<string> Subdirectories { get; set; }
        public ServiceLifespan Lifespan { get; set; }
        public List<TypeParameter> TypeParameters { get; set; }
        public List<Controller> Controllers { get; set; }

        public class Controller
        {
            public string Name { get; set; }
            public string Area { get; set; }
            public List<string> Subdirectories { get; set; }
            public string ServiceIdentifier { get; set; }
        }

        public enum ServiceLifespan
        {
            Singleton,
            Scoped,
            Transient
        }
    }
}
