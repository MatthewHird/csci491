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
        public string ServiceName { get; set; }
        public bool? IsClassOnly { get; set; }
        public SourceOfTruth? SourceOfTruth { get; set; }
        public string Area { get; set; }
        public string Subdirectory { get; set; }
        public ServiceLifetime? ServiceLifespan { get; set; }
        public List<TypeParameter> TypeParameters { get; set; }
        public List<InjectedService> InjectedServices { get; set; }
        public List<InjectIntoClass> InjectInto { get; set; }


    }
    public class InjectIntoClass
    {
        public string ClassName { get; set; }
        public string Area { get; set; }
        public string Subdirectory { get; set; }
        public string ServiceIdentifier { get; set; }
    }
}
