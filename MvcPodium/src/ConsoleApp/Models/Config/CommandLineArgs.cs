using System;
using System.Collections.Generic;
using System.Text;

namespace MvcPodium.ConsoleApp.Models.Config
{
    public class CommandLineArgs
    {
        public string ProjectEnvironment { get; set; }
        public string CommandFileDirectory { get; set; }
        public List<string> CommandFiles { get; set; } = new List<string>();
    }
}
