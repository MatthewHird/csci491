using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using MvcPodium.ConsoleApp.Models.Config;

namespace MvcPodium.ConsoleApp.Controllers
{
    public class MvcPodiumController
    {
        private readonly ILogger<MvcPodiumController> _logger;
        private readonly IOptions<CommandLineArgs> _commandLineArgs;
        private readonly IOptions<ProjectEnvironment> _projectEnvironment;
        private readonly IOptions<AppSettings> _appSettings;
        private readonly ServiceCommandController _serviceCommandController;
        private readonly BreadcrumbCommandController _breadcrumbCommandController;

        public MvcPodiumController(
            ILogger<MvcPodiumController> logger,
            IOptions<CommandLineArgs> commandLineArgs,
            IOptions<ProjectEnvironment> projectEnvironment, 
            IOptions<AppSettings> appSettings,
            ServiceCommandController serviceCommandController,
            BreadcrumbCommandController breadcrumbCommandController)
        {
            _logger = logger;
            _commandLineArgs = commandLineArgs;
            _projectEnvironment = projectEnvironment;
            _appSettings = appSettings;
            _serviceCommandController = serviceCommandController;
            _breadcrumbCommandController = breadcrumbCommandController;
        }

        public int Run()
        {
            var options = new JsonSerializerOptions
            {
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true,
            };
            options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));

            _logger.LogInformation(
                $"Reading in command files: {_commandLineArgs.Value.CommandFiles.Count} files specified...");
            foreach (string commandFile in _commandLineArgs.Value.CommandFiles)
            {
                _logger.LogInformation($"Reading file {commandFile}...");
                var commandSet = JsonSerializer.Deserialize<CommandSet>(File.ReadAllText(commandFile), options);

                if (commandSet?.ServiceCommands != null)
                {
                    _logger.LogInformation($"{commandSet.ServiceCommands.Count} service commands found...");
                    int i = 0;
                    foreach (var serviceCommand in commandSet.ServiceCommands)
                    {
                        ++i;
                        _logger.LogInformation(
                            $"Executing service command {i} of {commandSet.ServiceCommands.Count}...");
                        _serviceCommandController.Execute(serviceCommand);
                    }
                }

                if (commandSet?.BreadcrumbCommands != null)
                {
                    _logger.LogInformation($"{commandSet.BreadcrumbCommands.Count} breadcrumb commands found...");
                    int i = 0;
                    foreach (var breadcrumbCommand in commandSet.BreadcrumbCommands)
                    {
                        ++i;
                        _logger.LogInformation(
                            $"Executing breadcrumb command {i} of {commandSet.BreadcrumbCommands.Count}...");
                        _breadcrumbCommandController.Execute(breadcrumbCommand);
                    }
                }
            }
            _logger.LogInformation("All commands have been completed. Exiting program...");

            return 0;
        }
    }
}
