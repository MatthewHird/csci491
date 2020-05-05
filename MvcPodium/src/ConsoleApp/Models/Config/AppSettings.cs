using System;
using System.Collections.Generic;
using System.Text;

namespace MvcPodium.ConsoleApp.Models.Config
{
    public class AppSettings
    {
        public string AssemblyDirectory { get; set; }
        public string StringTemplatesDirectory { get; set; }
        public string CommandFilesDirectory { get; set; }
    }
}
