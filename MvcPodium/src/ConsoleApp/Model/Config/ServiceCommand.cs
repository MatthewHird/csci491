using System;
using System.Collections.Generic;
using System.Text;

namespace MvcPodium.ConsoleApp.Model.Config
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
        public List<GenericTypeParameter> GenericTypeParameters { get; set; }
        public List<Controller> Controllers { get; set; }

        public class GenericTypeParameter
        {
            public string TypeParameter { get; set; }
            public Variance Variance { get; set; }
            public List<string> Constraints { get; set; }
        }

        public class Controller
        {
            public string Name { get; set; }
            public string Area { get; set; }
        }

        public enum Variance
        {
            None,
            In,
            Out
        }

        public enum ServiceLifespan
        {
            Singleton,
            Scoped,
            Transient
        }
    }
}
