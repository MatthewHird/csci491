﻿using System;
using System.Collections.Generic;
using System.Text;

namespace MvcPodium.ConsoleApp.Model.Config
{
    public class CommandLineArgs
    {
        public string ProjectRoot { get; set; }
        public string CommandFileDirectory { get; set; }
        public List<string> CommandFiles { get; set; } = new List<string>();
    }
}
