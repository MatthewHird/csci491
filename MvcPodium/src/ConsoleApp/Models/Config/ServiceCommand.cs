using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using MvcPodium.ConsoleApp.Models.CSharpCommon;
using MvcPodium.ConsoleApp.Models.ServiceCommand;

namespace MvcPodium.ConsoleApp.Models.Config
{
    public partial class CommandSet
    {
        public List<ServiceCommand> ServiceCommands { get; set; }
    }

    public class ServiceCommand
    {
        public string ServiceRootName { get; set; }
        public SourceOfTruth? SourceOfTruth { get; set; }
        public string Area { get; set; }
        public List<string> Subdirectories { get; set; }
        public ServiceLifetime? ServiceLifespan { get; set; }
        public List<TypeParameter> TypeParameters { get; set; }
        public List<Controller> Controllers { get; set; }

        public class Controller
        {
            public string Name { get; set; }
            public string Area { get; set; }
            public List<string> Subdirectories { get; set; }
            public string ServiceIdentifier { get; set; }
        }

    }
}
