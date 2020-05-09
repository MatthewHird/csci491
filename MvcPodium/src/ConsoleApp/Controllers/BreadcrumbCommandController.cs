using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using MvcPodium.ConsoleApp.Constants.CSharpGrammar;
using MvcPodium.ConsoleApp.Models.BreadcrumbCommand;
using MvcPodium.ConsoleApp.Models.Config;
using MvcPodium.ConsoleApp.Models.CSharpCommon;
using MvcPodium.ConsoleApp.Models.ServiceCommand;
using MvcPodium.ConsoleApp.Services;
using MvcPodium.ConsoleApp.Visitors.Factories;

namespace MvcPodium.ConsoleApp.Controllers
{
    public class BreadcrumbCommandController
    {
        private readonly ILogger<MvcPodiumController> _logger;
        private readonly IOptions<CommandLineArgs> _commandLineArgs;
        private readonly IOptions<ProjectEnvironment> _projectEnvironment;
        private readonly IOptions<UserSettings> _userSettings;
        private readonly IIoUtilService _ioUtilService;
        private readonly IBreadcrumbCommandStgService _breadcrumbCommandStgService;
        private readonly IServiceCommandService _serviceCommandService;
        private readonly IBreadcrumbControllerScraperFactory _breadcrumbControllerScraperFactory;
        private readonly IBreadcrumbControllerInjectorFactory _breadcrumbControllerInjectorFactory;
        private readonly IBreadcrumbClassInjectorFactory _breadcrumbClassInjectorFactory;
        
        public BreadcrumbCommandController(
            ILogger<MvcPodiumController> logger,
            IOptions<CommandLineArgs> commandLineArgs,
            IOptions<ProjectEnvironment> projectEnvironment,
            IOptions<UserSettings> userSettings,
            IIoUtilService ioUtilService,
            IBreadcrumbCommandStgService breadcrumbCommandStgService,
            IServiceCommandService serviceCommandService,
            IBreadcrumbControllerScraperFactory breadcrumbControllerScraperFactory,
            IBreadcrumbControllerInjectorFactory breadcrumbControllerInjectorFactory,
            IBreadcrumbClassInjectorFactory breadcrumbClassInjectorFactory)
        {
            _logger = logger;
            _commandLineArgs = commandLineArgs;
            _projectEnvironment = projectEnvironment;
            _userSettings = userSettings;
            _ioUtilService = ioUtilService;
            _breadcrumbCommandStgService = breadcrumbCommandStgService;
            _serviceCommandService = serviceCommandService;
            _breadcrumbControllerScraperFactory = breadcrumbControllerScraperFactory;
            _breadcrumbControllerInjectorFactory = breadcrumbControllerInjectorFactory;
            _breadcrumbClassInjectorFactory = breadcrumbClassInjectorFactory;
        }

        public Task Execute(BreadcrumbCommand breadcrumbCommand)
        {
            var areaRootNamespace = _projectEnvironment.Value.RootNamespace +
                (breadcrumbCommand.Area is null || breadcrumbCommand.Area.TrimEnd(@"/\ ".ToCharArray()) == string.Empty
                    ? string.Empty : $".Areas.{breadcrumbCommand.Area.TrimEnd(@"/\ ".ToCharArray())}");

            var controllerRootNamespace = areaRootNamespace + ".Controllers";
            
            var breadcrumbRootNamespace = areaRootNamespace + ".Services.Breadcrumbs";

            var defaultAreaBreadcrumbServiceRootName = _projectEnvironment.Value.RootNamespace +
                (breadcrumbCommand.Area is null  || breadcrumbCommand.Area.TrimEnd(@"/\ ".ToCharArray()) == string.Empty
                    ? string.Empty
                    : breadcrumbCommand.Area.TrimEnd(@"/\ ".ToCharArray())) +
                "Breadcrumb";

            var defaultAreaBreadcrumbServiceName = defaultAreaBreadcrumbServiceRootName + "Service";

            var commandRootDirectory = Path.Combine(
                _projectEnvironment.Value.RootDirectory, 
                (breadcrumbCommand.Area is null|| breadcrumbCommand.Area.TrimEnd(@"/\ ".ToCharArray()) == string.Empty
                    ? string.Empty : Path.Combine("Areas", breadcrumbCommand.Area)));

            var targetDirectory = Path.Combine(
                commandRootDirectory, breadcrumbCommand.TargetDirectory ?? "Controllers");

            var breadcrumbOutputDirectory = Path.Combine(
                commandRootDirectory, breadcrumbCommand.BreadcrumbServiceDirectory ?? "Services/Breadcrumbs");

            Directory.CreateDirectory(breadcrumbOutputDirectory);

            if (!Directory.Exists(targetDirectory))
            {
                _logger.LogError($"TargetDirectory does not exist at {targetDirectory}");
                // throw
                return Task.CompletedTask;
            }

            var filenames = new List<string>();

            if (breadcrumbCommand.TargetFile != null)
            {
                var targetFile = Path.Combine(targetDirectory, breadcrumbCommand.TargetFile);
                if (!File.Exists(targetFile))
                {
                    _logger.LogError($"TargetFile does not exist at {targetFile}");
                    // throw
                    return Task.CompletedTask;
                }
                filenames.Add(breadcrumbCommand.TargetFile);
            }
            else
            {
                filenames = GetCSharpFilesInDirectory(targetDirectory, breadcrumbCommand.IsRecursive ?? false);
            }

            var controllerDict = new ControllerDictionary();

            foreach (var filename in filenames)
            {
                var controllerParser = new CSharpParserWrapper(filename);
                var tree = controllerParser.GetParseTree();

                var visitor = _breadcrumbControllerScraperFactory.Create(
                    controllerParser.Tokens);
                visitor.Visit(tree);
                var scraperResults = visitor.Results;

                foreach (var nameKey in scraperResults.NamespaceDict.Keys)
                {
                    if (controllerDict.NamespaceDict.ContainsKey(nameKey))
                    {
                        foreach (var classKey in scraperResults.NamespaceDict[nameKey].ClassDict.Keys)
                        {
                            controllerDict.NamespaceDict[nameKey].ClassDict[classKey] =
                                scraperResults.NamespaceDict[nameKey].ClassDict[classKey];
                        }
                    }
                    else
                    {
                        controllerDict.NamespaceDict[nameKey] = scraperResults.NamespaceDict[nameKey];
                    }
                }

            }

            //public class InjectedService
            //{
            //    public string Type { get; set; }
            //    public string ServiceIdentifier { get; set; }
            //    public string Namespace { get; set; }
            //}
            var usingDirectives = new HashSet<string>() { "SmartBreadcrumbs.Nodes" };

            var fieldDeclarations = new List<FieldDeclaration>();

            var ctorParameters = new FormalParameterList();
            var fixedParams = ctorParameters.FixedParameters;

            var ctorBody = new ConstructorBody();
            var statements = ctorBody.Statements;

            foreach (var injectedService in breadcrumbCommand.InjectedServices)
            {
                var injectedServiceIdentifier = injectedService.ServiceIdentifier
                        ?? Regex.Replace(
                            Regex.Replace(
                                injectedService.Type,
                                @"^I?([A-Z])",
                                "$1"),
                            @"^[A-Z]",
                            m => m.ToString().ToLower());

                if (!Regex.Match(breadcrumbRootNamespace, "^" + Regex.Escape(injectedService.Namespace)).Success)
                {
                    usingDirectives.Add(injectedService.Namespace);

                }

                fieldDeclarations.Add(
                    new FieldDeclaration()
                    {
                        Modifiers = new List<string>()
                        {
                            Keywords.Private,
                            Keywords.Readonly
                        },
                        Type = injectedService.Type,
                        VariableDeclarator = new VariableDeclarator()
                        {
                            Identifier = "_" + injectedServiceIdentifier
                        }
                        
                    });
                fixedParams.Add(
                    new FixedParameter()
                    {
                        Type = injectedService.Type,
                        Identifier = injectedServiceIdentifier
                    });
                statements.Add(
                    new Statement()
                    {
                        SimpleAssignment = new SimpleAssignment()
                        {
                            LeftHandSide = "_" + injectedServiceIdentifier,
                            RightHandSide = injectedServiceIdentifier
                        }
                    });
            }

            var startupRegInfoList = new List<StartupRegistrationInfo>();

            foreach (var controllerNamespace in controllerDict.NamespaceDict.Values)
            {
                var namespaceSuffix = Regex.Replace(
                    controllerNamespace.Namespace, "^" + Regex.Escape(controllerRootNamespace + "."), string.Empty);

                var serviceNamePrefix = namespaceSuffix.Replace(".", "");

                var serviceRootName = serviceNamePrefix == string.Empty
                    ? defaultAreaBreadcrumbServiceRootName
                    : serviceNamePrefix + "Breadcrumb";

                var serviceClassName = serviceRootName + "Service";

                var serviceInterfaceName = $"I{serviceClassName}";

                var serviceClassFilename = $"{serviceClassName}.cs";
                var serviceInterfaceFilename = $"{serviceInterfaceName}.cs";

                startupRegInfoList.Add(new StartupRegistrationInfo()
                {
                    ServiceNamespace = breadcrumbRootNamespace,
                    ServiceName = serviceRootName,
                    HasTypeParameters = false,
                    ServiceLifespan = ServiceLifetime.Scoped
                });

                var classDeclaration = new BreadcrumbServiceDeclaration()
                {
                    IsInterface = false,
                    Modifiers = new List<string>() { Keywords.Public },
                    Identifier = serviceClassName,
                    Base = new ClassInterfaceBase()
                    {
                        InterfaceTypeList = new List<string>() { serviceInterfaceName }
                    },
                    Body = new BreadcrumbServiceBody()
                    {
                        FieldDeclarations = fieldDeclarations,
                        ConstructorDeclaration = new ConstructorDeclaration()
                        {
                            Modifiers = new List<string>() { Keywords.Public },
                            Identifier = serviceClassName,
                            FormalParameterList = ctorParameters,
                            Body = ctorBody
                        }
                    }
                };

                var interfaceDeclaration = new BreadcrumbServiceDeclaration()
                {
                    IsInterface = true,
                    Identifier = serviceInterfaceName
                };

                foreach (var controllerClass in controllerNamespace.ClassDict.Values)
                {
                    var controllerNamePattern = breadcrumbCommand.ControllerNamePattern.Replace(
                        "$ControllerType$", controllerClass.Controller);

                    foreach (var controllerAction in controllerClass.ActionDict.Values)
                    {
                        var methodDeclaration = new BreadcrumbMethodDeclaration()
                        {
                            ControllerRoot = controllerClass.ControllerRoot,
                            Action = controllerAction.Action,
                            HasId = controllerAction.HasId,
                            Controller = controllerClass.Controller,
                            ControllerNamePattern = controllerNamePattern
                        };

                        classDeclaration.Body.MethodDeclarations.Add(methodDeclaration);
                        interfaceDeclaration.Body.MethodDeclarations.Add(methodDeclaration);
                    }
                }

                if (!File.Exists(Path.Combine(breadcrumbOutputDirectory, serviceClassFilename)))
                {
                    var cos = _breadcrumbCommandStgService.RenderBreadcrumbServiceFile(
                        usingDirectives.ToList(),
                        breadcrumbRootNamespace,
                        classDeclaration);

                    _ioUtilService.WriteStringToFile(
                        cos,
                        Path.Combine(breadcrumbOutputDirectory, $"X{serviceClassFilename}"));

                    var ios = _breadcrumbCommandStgService.RenderBreadcrumbServiceFile(
                        usingDirectives.ToList(),
                        breadcrumbRootNamespace,
                        interfaceDeclaration);

                    _ioUtilService.WriteStringToFile(
                        ios,
                        Path.Combine(breadcrumbOutputDirectory, $"X{serviceInterfaceFilename}"));
                }
                else
                {
                    var breadcrumbClassParser = new CSharpParserWrapper(
                        Path.Combine(breadcrumbOutputDirectory, serviceClassFilename));
                    var tree = breadcrumbClassParser.GetParseTree();

                    var visitor = _breadcrumbClassInjectorFactory.Create(
                        breadcrumbClassParser.Tokens,
                        usingDirectives.ToList(),
                        breadcrumbRootNamespace,
                        classDeclaration,
                        _userSettings.Value.TabString);
                    visitor.Visit(tree);

                    var testServiceClassFilename = serviceClassFilename;
                    var testServiceInterfaceFilename = Path.Combine(
                        breadcrumbOutputDirectory, $"X{serviceInterfaceFilename}");

                    if (visitor.IsModified)
                    {
                        testServiceClassFilename = Path.Combine(breadcrumbOutputDirectory, $"X{serviceClassFilename}");

                        _ioUtilService.WriteStringToFile(
                            visitor.Rewriter.GetText(),
                            Path.Combine(breadcrumbOutputDirectory, $"X{serviceClassFilename}"));
                    }

                    var serviceClassParser = new CSharpParserWrapper(testServiceClassFilename);

                    var classScraperResults = _serviceCommandService.ScrapeServiceClass(
                        serviceClassParser,
                        serviceClassName,
                        breadcrumbRootNamespace,
                        null);

                    _ioUtilService.WriteStringToFile(
                        _serviceCommandService.GetInterfaceServiceFileFromClass(
                            classScraperResults,
                            serviceInterfaceName,
                            breadcrumbRootNamespace),
                        Path.Combine(breadcrumbOutputDirectory, $"X{serviceInterfaceFilename}"));

                }

                var x = 1;
            }

            RegisterServicesInStartup(startupRegInfoList);

            InjectBreadcrumbsIntoControllers(controllerFilepaths: filenames,
                controllerDictionary: controllerDict,
                breadcrumbServiceNamespace: breadcrumbRootNamespace,
                controllerRootNamespace: controllerRootNamespace,
                defaultAreaBreadcrumbServiceRootName: defaultAreaBreadcrumbServiceRootName,
                tabString: _userSettings.Value.TabString);

            return Task.CompletedTask;
        }

        private void RegisterServicesInStartup(List<StartupRegistrationInfo> startupRegInfoList)
        {
            var startupFilepath = Path.Combine(_projectEnvironment.Value.RootDirectory, "Startup.cs");
            var testStartupFilepath = Path.Combine(_projectEnvironment.Value.RootDirectory, "XStartup.cs");
            //Check if Startup.cs exists
            if (!File.Exists(startupFilepath))
            {
                _logger.LogError($"Startup file does not exist at path {startupFilepath}");
                //  If !exists:
                //      Throw error
            }
            else
            {
                //  Else:
                //      Parse Startup.cs
                var startupParser = new CSharpParserWrapper(startupFilepath);

                var startupFileRewrite = _serviceCommandService.RegisterServicesInStartup(
                    startupParser: startupParser,
                    rootNamespace: _projectEnvironment.Value.RootNamespace,
                    startupRegInfoList: startupRegInfoList,
                    tabString: _userSettings.Value.TabString);

                if (startupFileRewrite != null)
                {
                    _ioUtilService.WriteStringToFile(
                        startupFileRewrite,
                        // startupFilepath);
                        testStartupFilepath);
                }
            }
        }

        private void InjectBreadcrumbsIntoControllers(
            List<string> controllerFilepaths,
            ControllerDictionary controllerDictionary,
            string breadcrumbServiceNamespace,
            string controllerRootNamespace,
            string defaultAreaBreadcrumbServiceRootName,
            string tabString)
        {
            foreach (var filepath in controllerFilepaths)
            {


                var controllerParser = new CSharpParserWrapper(filepath);
                var tree = controllerParser.GetParseTree();

                var visitor = _breadcrumbControllerInjectorFactory.Create(
                    tokenStream: controllerParser.Tokens,
                    controllerDictionary: controllerDictionary,
                    breadcrumbServiceNamespace: breadcrumbServiceNamespace,
                    controllerRootNamespace: controllerRootNamespace,
                    defaultAreaBreadcrumbServiceRootName: defaultAreaBreadcrumbServiceRootName,
                    tabString: tabString);

                visitor.Visit(tree);
                if (visitor.IsModified)
                {
                    _ioUtilService.WriteStringToFile(
                        visitor.Rewriter.GetText(),
                        // filepath);
                        filepath + ".test.cs");
                }
                var g = 1;
            }

        }

        private List<string> GetCSharpFilesInDirectory(string directoryPath, bool isRecursive = false)
        {
            var cSharpFiles = new List<string>();

            if (isRecursive)
            {
                var dirs = Directory.GetDirectories(directoryPath);
                foreach (var dir in dirs)
                {
                    cSharpFiles.AddRange(GetCSharpFilesInDirectory(dir, isRecursive));
                }
            }
            var files = Directory.GetFiles(directoryPath);
            foreach (var file in files)
            {
                if (file.EndsWith(".cs", true, null))
                {
                    cSharpFiles.Add(file);
                }
            }

            return cSharpFiles;
        }
    }
}
