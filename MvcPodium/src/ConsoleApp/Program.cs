﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using MvcPodium.ConsoleApp.Controllers;
using MvcPodium.ConsoleApp.Models.Config;
using MvcPodium.ConsoleApp.Services;
using MvcPodium.ConsoleApp.Visitors.Factories;

namespace MvcPodium.ConsoleApp
{
    public class Program
    {
        static void Main(string[] args)
        {
            var app = new CommandLineApplication()
            {
                Name = "MvcPodium",
                Description = "A scaffolding tool for ASP.NET Core 3.1",
                ResponseFileHandling = ResponseFileHandling.ParseArgsAsLineSeparated,
                ThrowOnUnexpectedArgument = false,
            };

            app.HelpOption();

            var projectEnvironment = app.Option("-p|--project_environment=<PATH>",
                    "REQUIRED. Absolute path to the project environment JSON file.",
                    CommandOptionType.SingleValue)
                .IsRequired()
                .Accepts(a => a.ExistingFileOrDirectory());

            var commandFileDirectory = app.Option("-d|--command_file_directory=<PATH>",
                    "Absolute path to the directory containing command files.",
                    CommandOptionType.SingleValue)
                .Accepts(a => a.ExistingDirectory());

            var commandFiles = app.Option("-f|--command_file=<PATH>",
                    "REQUIRED.Option can be used multiple times to pass in multiple command files. Path can be " +
                    "absolute or relative to the command file directory.",
                    CommandOptionType.MultipleValue);

            var userSettingsPath = app.Option("-u|--user_settings=<PATH>",
                    "Absolute path to user settings JSON file.",
                    CommandOptionType.SingleValue)
                .Accepts(a => a.ExistingFile());

            var logLevelOption = app.Option("-g|--log_level=<LEVEL>",
                    "The minimum log level. values: { Debug, Information(default), Warning, Error, Critical, None}",
                    CommandOptionType.SingleValue)
                .Accepts(a => a.Enum<LogLevel>(true));

            app.OnExecute(() =>
            {
                var pe = projectEnvironment?.Value() ?? "";
                var cfd = commandFileDirectory?.Value() ?? "";

                if (commandFiles.Values.Count == 0)
                {
                    Console.Write(
                        "At least one command file is required.\r\n" +
                        "\r\n" +
                        "Usage:\r\n" +
                        "\t-f|--command_file=<optionvalue>\r\n" +
                        "\r\n" +
                        "Option can be specified multiple times.\r\n"
                    );
                    return 1;
                }

                var commandFilesFull = new List<string>();
                foreach (var commandFile in commandFiles.Values)
                {
                    try
                    {
                        string cf = "";
                        if (cfd != "" && File.Exists(Path.Combine(cfd, commandFile)))
                        {
                            cf = Path.Combine(cfd, commandFile);
                        }
                        else if (File.Exists(commandFile))
                        {
                            cf = commandFile;
                        }
                        else
                        {
                            Console.Write($"Command file {commandFile} could not be found.");
                            return 1;
                        }
                        commandFilesFull.Add(cf);
                    }
                    catch (Exception ex) when (ex is ArgumentException || ex is ArgumentNullException)
                    {
                        Console.Write($"Command file {commandFile} or command file directory {commandFileDirectory}" +
                            $" is null or contains invalid characters for a path.\r\n");
                        return 1;
                    }
                }

                string assemblyDir = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
                var inMemConfig = new Dictionary<string, string>() 
                { 
                    { "AppSettings:AssemblyDirectory", assemblyDir },
                    { "CommandLineArgs:ProjectEnvironment", pe },
                    { "CommandLineArgs:CommandFileDirectory", cfd}
                };
                for (int i = 0; i < commandFilesFull.Count; ++i)
                {
                    inMemConfig[$"CommandLineArgs:CommandFiles:{i}"] = commandFilesFull[i];
                }

                var logLevel = LogLevel.Information;
                if (logLevelOption?.Value() != null)
                {
                    logLevel = Enum.Parse<LogLevel>(logLevelOption.Value());
                    if (logLevel == LogLevel.Trace)
                    {
                        logLevel = LogLevel.Debug;
                    }
                }

                var config = CreateConfiguration(inMemConfig, pe, userSettingsPath?.Value());
                var serviceProvider = CreateServiceProvider(config, logLevel);

                var program = serviceProvider.GetRequiredService<MvcPodiumController>();
                return program.Run();
            });

            try
            {
                app.Execute(args);
            }
            catch (CommandParsingException ex)
            {
                Console.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to execute application: {0}", ex.Message);
            } 

        }

        public static IConfigurationRoot CreateConfiguration(
            Dictionary<string, string> inMemConfig,
            string projectEnvironment,
            string userSettingsPath = null)
        {
            IConfigurationBuilder configBuilder = new ConfigurationBuilder();
            configBuilder.AddInMemoryCollection(inMemConfig);
            configBuilder.AddJsonFile("appsettings.json", optional: true);
#if DEBUG
            configBuilder.AddJsonFile("appsettings.Debug.json", optional: true);
#endif
            configBuilder.AddJsonFile("usersettings.Default.json");
            if (userSettingsPath != null)
            {
                configBuilder.AddJsonFile(userSettingsPath);
            }
            configBuilder.AddJsonFile(projectEnvironment);

            return configBuilder.Build();
        }

        public static IServiceProvider CreateServiceProvider(IConfigurationRoot config, LogLevel logLevel)
        {
            IServiceCollection serviceCollection = new ServiceCollection();

            serviceCollection.AddSingleton<ILogger<MvcPodiumController>, Logger<MvcPodiumController>>();
            serviceCollection.AddLogging(builder =>
            {
                builder
                    .AddFilter("Microsoft", LogLevel.Warning)
                    .AddFilter("System", LogLevel.Warning)
                    .AddFilter("LoggingConsoleApp.Program", LogLevel.Debug)
                    .AddConsole()
                    .AddDebug()
                    .SetMinimumLevel(logLevel)
                    .AddEventLog();
            });

            serviceCollection.Configure<CommandLineArgs>(config.GetSection("CommandLineArgs"));
            serviceCollection.Configure<AppSettings>(config.GetSection("AppSettings"));
            serviceCollection.Configure<UserSettings>(config.GetSection("UserSettings"));
            serviceCollection.Configure<ProjectEnvironment>(config.GetSection("ProjectEnvironment"));

            serviceCollection.AddTransient<MvcPodiumController, MvcPodiumController>();
            serviceCollection.AddTransient<ServiceCommandController, ServiceCommandController>();
            serviceCollection.AddTransient<BreadcrumbCommandController, BreadcrumbCommandController>();
            
            serviceCollection.AddSingleton<IServiceCommandService, ServiceCommandService>();
            
            serviceCollection.AddSingleton<ICSharpParserService, CSharpParserService>();
            serviceCollection.AddSingleton<IServiceCommandParserService, ServiceCommandParserService>();
            serviceCollection.AddSingleton<IBreadcrumbCommandParserService, BreadcrumbCommandParserService>();
            
            serviceCollection.AddSingleton<ICSharpCommonStgService, CSharpCommonStgService>();
            serviceCollection.AddSingleton<IServiceCommandStgService, ServiceCommandStgService>();
            serviceCollection.AddSingleton<IBreadcrumbCommandStgService, BreadcrumbCommandStgService>();

            serviceCollection.AddSingleton<IStringUtilService, StringUtilService>();
            serviceCollection.AddSingleton<IIoUtilService, IoUtilService>();

            serviceCollection.AddSingleton<IServiceInterfaceScraperFactory, ServiceInterfaceScraperFactory>();
            serviceCollection.AddSingleton<IServiceClassScraperFactory, ServiceClassScraperFactory>();
            serviceCollection.AddSingleton<IServiceInterfaceInjectorFactory, ServiceInterfaceInjectorFactory>();
            serviceCollection.AddSingleton<IServiceClassInjectorFactory, ServiceClassInjectorFactory>();
            serviceCollection.AddSingleton<IServiceStartupRegistrationFactory, ServiceStartupRegistrationFactory>();
            serviceCollection.AddSingleton<IServiceConstructorInjectorFactory, ServiceConstructorInjectorFactory>();
            serviceCollection.AddSingleton<IBreadcrumbControllerScraperFactory, BreadcrumbControllerScraperFactory>();
            serviceCollection.AddSingleton<IBreadcrumbControllerInjectorFactory, BreadcrumbControllerInjectorFactory>();
            serviceCollection.AddSingleton<IBreadcrumbClassInjectorFactory, BreadcrumbClassInjectorFactory>();
            
            return serviceCollection.BuildServiceProvider();
        }
    }
}
