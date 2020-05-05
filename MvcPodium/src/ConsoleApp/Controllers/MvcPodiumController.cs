using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MvcPodium.ConsoleApp.Visitors;

using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

using Antlr4.StringTemplate;
using Microsoft.Extensions.Hosting;
using System.Threading;
using Microsoft.Extensions.Logging;
using MvcPodium.ConsoleApp.Models.Config;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MvcPodium.ConsoleApp.Controller
{
    public class MvcPodiumController
    {
        private readonly ILogger<MvcPodiumController> _logger;
        private readonly IOptions<CommandLineArgs> _commandLineArgs;
        private readonly IOptions<ProjectEnvironment> _projectEnvironment;
        private readonly IOptions<AppSettings> _appSettings;
        private readonly ServiceCommandController _serviceCommandController;

        public MvcPodiumController(
            ILogger<MvcPodiumController> logger,
            IOptions<CommandLineArgs> commandLineArgs,
            IOptions<ProjectEnvironment> projectEnvironment, 
            IOptions<AppSettings> appSettings,
            ServiceCommandController serviceCommandController)
        {
            _logger = logger;
            _commandLineArgs = commandLineArgs;
            _projectEnvironment = projectEnvironment;
            _appSettings = appSettings;
            _serviceCommandController = serviceCommandController;
        }

        public int Run()
        {
            var options = new JsonSerializerOptions
            {
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true,
            };
            options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));

            foreach (string commandFile in _commandLineArgs.Value.CommandFiles)
            {
                var commandSet = JsonSerializer.Deserialize<CommandSet>(File.ReadAllText(commandFile), options);

                if (commandSet?.ServiceCommands != null)
                {
                    foreach(var serviceCommand in commandSet.ServiceCommands)
                    {
                        _serviceCommandController.Execute(serviceCommand);
                    }
                }
            }

            return 0;
        }
    }
}
