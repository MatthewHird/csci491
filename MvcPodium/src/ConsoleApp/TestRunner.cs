using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using McMaster.Extensions.CommandLineUtils;
using System.Text.Json;
using System.Text.Json.Serialization;

using MvcPodium.ConsoleApp.Models.Config;
using MvcPodium.ConsoleApp.Controllers;

namespace MvcPodium.ConsoleApp
{
    class TestRunner
    {

        private enum Test
        {
            T1,
            T2,
            T3,
        }

        static void Main(string[] args)
        {
            var app = new CommandLineApplication() {
                Name = "MvcPodium",
                Description = "A scaffolding tool for .NET Core 3.1",
                ResponseFileHandling = ResponseFileHandling.ParseArgsAsLineSeparated,
                ThrowOnUnexpectedArgument = false,
            };

            app.HelpOption();

            //var basicOption = app.Option("-f|--file=<optionvalue>",
            //        "Some option value",
            //        CommandOptionType.SingleValue)
            //    .IsRequired()
            //    .Accepts(a => a.ExistingFile());

            app.OnExecute(() =>
            {
                var options = new JsonSerializerOptions
                {
                    ReadCommentHandling = JsonCommentHandling.Skip,
                    AllowTrailingCommas = true,
                };

                options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));

                var environment = JsonSerializer.Deserialize<ProjectEnvironment>(File.ReadAllText("D:\\Files HDD\\Workspace\\csci491\\MvcPodium\\Resources\\CommandFiles\\project_environment.json"), options);
                
                //var serviceCommand = JsonSerializer.Deserialize<ServiceCommand>(File.ReadAllText(environment.CommandFilesDirectory + "/service.json"), options);

                //Directory.SetCurrentDirectory(environment.RootDirectory);

                //var serviceCommandController = new ServiceCommandController(environment);
                //serviceCommandController.Test(serviceCommand);


                //var controller = new MvcPodiumController() { 
                    
                //};
                //controller.Run();

                //if (basicOption.HasValue())
                //{
                //    Console.WriteLine("Option was selected, value: {0}", basicOption.Value());


                //}
                //else
                //{
                //    app.ShowHint();
                //}

                return 0;
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
                //Console.WriteLine($"Failed to execute application: {0}", ex.Message);
                Console.WriteLine($"Failed to execute application: {ex.Message}");
            }

        }


    }
}
