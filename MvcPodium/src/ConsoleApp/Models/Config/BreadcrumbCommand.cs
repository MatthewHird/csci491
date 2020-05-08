using System.Collections.Generic;

namespace MvcPodium.ConsoleApp.Models.Config
{
    public partial class CommandSet
    {
        public List<BreadcrumbCommand> BreadcrumbCommands { get; set; }
    }

    public class BreadcrumbCommand
    {
        public string Area { get; set; }
        public string TargetDirectory { get; set; }
        public string TargetFile { get; set; }
        public string BreadcrumbServiceDirectory { get; set; }
        public bool? IsRecursive { get; set; }
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
