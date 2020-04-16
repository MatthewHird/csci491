using System;
using System.Collections.Generic;
using System.Text;
using MvcPodium.ConsoleApp.Model.Config;
using System.IO;
using Antlr4.StringTemplate;
using MvcPodium.ConsoleApp.Visitors;
using Antlr4.Runtime;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using MvcPodium.ConsoleApp.Services;

namespace MvcPodium.ConsoleApp.Controller
{
    public class ServiceCommandController
    {
        private readonly ILogger<MvcPodiumController> _logger;
        private readonly IOptions<CommandLineArgs> _commandLineArgs;
        private readonly IOptions<ProjectEnvironment> _projectEnvironment;
        private readonly IOptions<AppSettings> _appSettings;
        private readonly IOptions<UserSettings> _userSettings;
        private readonly IStringTemplateService _stringTemplateService;

        public ServiceCommandController(
            ILogger<MvcPodiumController> logger,
            IOptions<CommandLineArgs> commandLineArgs,
            IOptions<ProjectEnvironment> projectEnvironment,
            IOptions<AppSettings> appSettings,
            IOptions<UserSettings> userSettings,
            IStringTemplateService stringTemplateService)
        {
            _logger = logger;
            _commandLineArgs = commandLineArgs;
            _projectEnvironment = projectEnvironment;
            _appSettings = appSettings;
            _userSettings = userSettings;
            _stringTemplateService = stringTemplateService;
        }

        public Task Execute(ServiceCommand serviceCommand)
        {

            //var servicesGroupFile = new TemplateGroupFile(_environment.StringTemplatesDirectory + "/Services.stg");
            //var serviceClassSt = servicesGroupFile.GetInstanceOf("ServiceClass");
            //serviceClassSt.Add("rootNamespace", _environment.RootNamespace);
            //serviceClassSt.Add("usingStatements", new List<string> { "UsingStatement1", "UsingStatement2.Test"});
            //serviceClassSt.Add("service", service);

            //Console.WriteLine(serviceClassSt.Render());




            var charStream = CharStreams.fromPath(@"D:\Files HDD\Workspace\csci491\MvcPodium\Resources\TestData\Input\FormPhoto.cs");
            //var charStream = CharStreams.fromString(serviceClassSt.Render());
            var lexer = new CSharpLexer(charStream);
            var tokenStream = new CommonTokenStream(lexer);
            var parser = new CSharpParser(tokenStream);
            parser.BuildParseTree = true;
            var tree = parser.compilation_unit();

            _logger.LogInformation(tree.ToStringTree());


            //var visitor = new ServiceInterfaceVisitor(tokenStream);
            //visitor.Visit(tree);

            //File.WriteAllText("TestData/Output/TestFormPhoto.cs", visitor.Rewriter.GetText());

            var x = 1;


            //using (var outStream = File.Create("TestData/Output/output.txt"))
            //{
            //    outStream.Write(Encoding.UTF8.GetBytes(mySt.Render()));
            //    outStream.Flush();
            //}



            //Check for Areas folder
            //  If !exist:
            //      Create Areas folder

            //Check for <area> folder
            //  If !exist:
            //      Create <area> folder

            //Check for Services folder
            //  If !exist:
            //      Create Services folder

            //Check for <subFolders> folders:
            //  Split <subFolder> path into folders (strings)
            //  For each folder in <subFolder> path:
            //      If !exist:
            //          Create folder
            var areaDirectory = serviceCommand.Area is null || serviceCommand.Area.TrimEnd(@"/\ ".ToCharArray()) == "" 
                ? "" : Path.Combine("Areas", serviceCommand.Area);
            var sevicesDirectory = Path.Combine(areaDirectory, "Services");
            var subDirectory = Path.Combine(sevicesDirectory, serviceCommand.Subdirectories is null 
                                                              || serviceCommand.Subdirectories.Count == 0 
                                                              ? "" : string.Join('/', serviceCommand.Subdirectories));
            Directory.CreateDirectory(subDirectory);



            //Check if <service> class file exists: 
            if(File.Exists(Path.Combine(subDirectory, $"{serviceCommand.ServiceRootName}Service.cs")))
            {
                //  Else:
                //      Parse <service> class file
                //          Extract list of existing public method signatures

            }
            else
            {
                //  If !exist:
                //      Create <service> class file
                //      Create serviceClass StringTemplate

            }

            //Check if <service> interface file exists:
            if (File.Exists(Path.Combine(subDirectory, $"{serviceCommand.ServiceRootName}Service.cs")))
            {
                //  Else:
                //      Parse <service> interface file
                //          Extract list of existing method signatures

            }
            else
            {
                //  If !exist:
                //      Create <service> interface file
                //      Create serviceInterface StringTemplate

            }


            //Compare lists of method signatures between interface and class
            //  If methods in class that are not in interface:
            //      Create list of methods in class that are not in interface
            //      Add missing methods to interface
            //          Create StringTemplate for each method
            //          Parse interface and use rewriter to insert methods into parse tree
            //          Write parse tree as string to interface file
            //  If methods in interface that aren't in class:
            //      Create list of methods in interface that aren't in class
            //      Add missing methods to class
            //          Create StringTemplate for each method
            //          Parse class and use rewriter to insert methods into parse tree
            //          Write parse tree as string to class file

            //Check if Startup.cs exists
            //  If !exists:
            //      Throw error
            //  Else:
            //      Parse Startup.cs
            //          Find method "public void ConfigureServices(IServiceCollection services)"
            //              Get name of IServiceCollection parameter (services)
            //          Check each services.Add[Lifespan] method int the form
            //                  If TypeParameters:  services.AddScoped(typeof(IMyService<>), typeof(MyService<>));
            //                  Else:               services.AddScoped<IMyService, MyService>();
            //              If found and Lifespan in services.Add[Lifespan] == Service.Lifespan:
            //                  No action taken
            //              Else if found and Lifespan in services.Add[Lifespan] != Service.Lifespan:
            //                  Replace Add[Lifespan] with Add[Service.Lifespan] in token stream
            //              Else:
            //                  Create StringTemplate for services.Add[Lifespan] statement
            //                      If TypeParameters:  services.AddScoped(typeof(IMyService<>), typeof(MyService<>));
            //                      Else:               services.AddScoped<IMyService, MyService>();
            //                  Use rewriter to insert services.Add[Lifespan] into token stream
            //                  Write parse tree as string to Startup.cs

            //Inject Service into Controllers
            //  For each Controller in Service.Controllers:
            //      Check whether [Controller.Name].cs exists:
            //              If Controller.Area != null:
            //                  ~/Areas/[Controller.Area]/Controllers/[Controller.Name].cs
            //              Else:
            //                  ~/Controllers/[Controller.Name].cs
            //                  ~/Areas/[Service.Area]/Controllers/[Controller.Name].cs
            //          If !exists:
            //              Throw error
            //          Else:
            //              Parse Controller file and check whether service has been injected
            //                  If injection exists:
            //                      No action
            //                  Else:
            //                      Create StringTemplates for private variable:
            //                          private IMyService _myService;
            //                      If no contructor for Controller:
            //                          Create StringTemplates for Controller with "IMyService myService" as a parameter 
            //                                  and assign parameter to private variable:
            //                              public MyController(IMyService myService)
            //                              {
            //                                  _myService = myService;
            //                              }
            //                      Else:
            //                          Add "IMyService myService" as a parameter to the constructor
            //                          Add line to assign parameter to private variable: "_myService = myService;"
            //                      Use rewriter to insert services.Add[Lifespan] into token stream
            //                      Write parse tree as string to Controller file

            return Task.CompletedTask;
        }
    }
}
