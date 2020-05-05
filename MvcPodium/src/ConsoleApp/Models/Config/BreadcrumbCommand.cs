using System;
using System.Collections.Generic;
using System.Text;

namespace MvcPodium.ConsoleApp.Models.Config
{
    public partial class CommandSet
    {
        public List<BreadcrumbCommand> BreadcrumbCommands { get; set; }
    }

    public class BreadcrumbCommand
    {
        public string ControllerNamePattern { get; set; }
        public List<InjectedService> InjectedServices { get; set; }
    }

    public class InjectedService
    {
        public string Type { get; set; }
        public string ServiceIdentifier { get; set; }
        public string Namespace { get; set; }
    }
}
