using System;
using System.Collections.Generic;
using System.Text;
using MvcPodium.ConsoleApp.Models.Config;
using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using MvcPodium.ConsoleApp.Services;
using MvcPodium.ConsoleApp.Constants.CSharpGrammar;
using MvcPodium.ConsoleApp.Visitors.Factories;
using MvcPodium.ConsoleApp.Models.CSharpCommon;
using MvcPodium.ConsoleApp.Models.ServiceCommand;

namespace MvcPodium.ConsoleApp.Controller
{
    public class ServiceCommandController
    {
        private readonly ILogger<MvcPodiumController> _logger;
        private readonly IOptions<CommandLineArgs> _commandLineArgs;
        private readonly IOptions<ProjectEnvironment> _projectEnvironment;
        private readonly IOptions<UserSettings> _userSettings;
        private readonly ICSharpCommonStgService _cSharpCommonStgService;
        private readonly IServiceCommandStgService _serviceCommandStgService;
        private readonly IServiceInterfaceScraperFactory _serviceInterfaceScraperFactory;
        private readonly IServiceClassScraperFactory _serviceClassScraperFactory;
        private readonly IServiceInterfaceInjectorFactory _serviceInterfaceInjectorFactory;
        private readonly IServiceClassInjectorFactory _serviceClassInjectorFactory;
        private readonly IServiceCommandService _serviceCommandService;
        private readonly IServiceStartupRegistrationFactory _serviceStartupRegistrationFactory;
        private readonly IServiceConstructorInjectorFactory _serviceConstructorInjectorFactory;

        public ServiceCommandController(
            ILogger<MvcPodiumController> logger,
            IOptions<CommandLineArgs> commandLineArgs,
            IOptions<ProjectEnvironment> projectEnvironment,
            IOptions<UserSettings> userSettings,
            ICSharpCommonStgService cSharpCommonStgService,
            IServiceCommandStgService serviceCommandStgService,
            IServiceInterfaceScraperFactory serviceInterfaceScraperFactory,
            IServiceClassScraperFactory serviceClassScraperFactory,
            IServiceInterfaceInjectorFactory serviceInterfaceInjectorFactory,
            IServiceClassInjectorFactory serviceClassInjectorFactory,
            IServiceCommandService serviceCommandService,
            IServiceStartupRegistrationFactory serviceStartupRegistrationFactory,
            IServiceConstructorInjectorFactory serviceControllerInjectorFactory)
        {
            _logger = logger;
            _commandLineArgs = commandLineArgs;
            _projectEnvironment = projectEnvironment;
            _userSettings = userSettings;
            _cSharpCommonStgService = cSharpCommonStgService;
            _serviceCommandStgService = serviceCommandStgService;
            _serviceInterfaceScraperFactory = serviceInterfaceScraperFactory;
            _serviceClassScraperFactory = serviceClassScraperFactory;
            _serviceInterfaceInjectorFactory = serviceInterfaceInjectorFactory;
            _serviceClassInjectorFactory = serviceClassInjectorFactory;
            _serviceCommandService = serviceCommandService;
            _serviceStartupRegistrationFactory = serviceStartupRegistrationFactory;
            _serviceConstructorInjectorFactory = serviceControllerInjectorFactory;
        }

        public Task Execute(ServiceCommand serviceCommand)
        {
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
            var serviceAreaDirectory = serviceCommand.Area is null 
                || serviceCommand.Area.TrimEnd(@"/\ ".ToCharArray()) == "" 
                ? "" : Path.Combine("Areas", serviceCommand.Area);
            var sevicesDirectory = Path.Combine(_commandLineArgs.Value.ProjectRoot, serviceAreaDirectory, "Services");
            var serviceSubDirectory = Path.Combine(sevicesDirectory, serviceCommand.Subdirectories is null 
                                                              || serviceCommand.Subdirectories.Count == 0 
                                                              ? "" : string.Join('/', serviceCommand.Subdirectories));
            Directory.CreateDirectory(serviceSubDirectory);

            string serviceClassFilePath = Path.Combine(
                serviceSubDirectory, $"{serviceCommand.ServiceRootName}Service.cs");
            string serviceInterfaceFilePath = Path.Combine(
                serviceSubDirectory, $"I{serviceCommand.ServiceRootName}Service.cs");

            string testOutServiceClassFilePath = Path.Combine(
                serviceSubDirectory, $"X{serviceCommand.ServiceRootName}Service.cs");
            string testOutServiceInterfaceFilePath = Path.Combine(
                serviceSubDirectory, $"XI{serviceCommand.ServiceRootName}Service.cs");

            var serviceNamespace = _projectEnvironment.Value.RootNamespace 
                + (serviceCommand.Area is null || serviceCommand.Area.TrimEnd(@"/\ ".ToCharArray()) == ""
                    ? "" : $".Areas.{serviceCommand.Area}")
                + ".Services";
            if (serviceCommand.Subdirectories != null)
            {
                foreach (var subDir in serviceCommand.Subdirectories)
                {
                    serviceNamespace += $".{subDir.TrimEnd(@"/\ ".ToCharArray())}";
                }

            }

            var className = $"{serviceCommand.ServiceRootName}Service";
            var interfaceName = $"I{serviceCommand.ServiceRootName}Service";

            ConsolidateServiceClassAndInterface(
                serviceClassName: className,
                serviceInterfaceName: interfaceName,
                typeParameters: serviceCommand.TypeParameters,
                serviceClassFilePath: serviceClassFilePath,
                serviceInterfaceFilePath: serviceInterfaceFilePath,
                serviceNamespace: serviceNamespace,
                serviceRootName: serviceCommand.ServiceRootName,
                outServiceClassFilePath: testOutServiceClassFilePath,
                outServiceInterfaceFilePath: testOutServiceInterfaceFilePath);

            RegisterServiceInStartup(
                serviceRootName: serviceCommand.ServiceRootName,
                typeParameters: serviceCommand.TypeParameters,
                serviceLifespan: serviceCommand.Lifespan.ToString(),
                serviceNamespace: serviceNamespace);

            //Inject Service into Controllers
            //  For each Controller in Service.Controllers:
            foreach (var controller in serviceCommand.Controllers)
            {
                InjectServiceIntoClassConstructors(
                    controller: controller,
                    className: className,
                    interfaceName: interfaceName,
                    serviceNamespace: serviceNamespace);
            }

            return Task.CompletedTask;
        }


        private void ConsolidateServiceClassAndInterface(
            string serviceClassName,
            string serviceInterfaceName,
            List<TypeParameter> typeParameters,
            string serviceClassFilePath,
            string serviceInterfaceFilePath,
            string serviceNamespace,
            string serviceRootName,
            string outServiceClassFilePath,
            string outServiceInterfaceFilePath)
        {
            CSharpParserWrapper serviceClassParser = null;
            CSharpParserWrapper serviceInterfaceParser = null;

            ServiceFile classScraperResults = null;
            ServiceFile interfaceScraperResults = null;

            //Check if <service> class file exists: 
            if (File.Exists(serviceClassFilePath))
            {
                //  Else:
                //      Parse <service> class file
                //          Extract list of existing public method signatures

                serviceClassParser = new CSharpParserWrapper(serviceClassFilePath);
                var tree = serviceClassParser.GetParseTree();

                var visitor = _serviceClassScraperFactory.Create(
                    serviceClassParser.Tokens,
                    serviceClassName,
                    serviceNamespace,
                    typeParameters);
                visitor.Visit(tree);
                classScraperResults = visitor.Results;
            }

            //Check if <service> interface file exists:
            if (File.Exists(serviceInterfaceFilePath))
            {
                //  Else:
                //      Parse <service> interface file
                //          Extract list of existing method signatures

                serviceInterfaceParser = new CSharpParserWrapper(serviceInterfaceFilePath);
                var tree = serviceInterfaceParser.GetParseTree();

                var visitor = _serviceInterfaceScraperFactory.Create(
                    serviceInterfaceParser.Tokens,
                    serviceInterfaceName,
                    serviceNamespace,
                    typeParameters);
                visitor.Visit(tree);
                interfaceScraperResults = visitor.Results;
            }

            if (classScraperResults is null && interfaceScraperResults is null)
            {
                //      Create <service> class file
                //      Create serviceClass StringTemplate with empty class body
                var classDeclaration = new ClassInterfaceDeclaration()
                {
                    IsInterface = false,
                    Identifier = serviceClassName,
                    TypeParameters = typeParameters.Copy(),
                    Base = new ClassInterfaceBase()
                };
                classDeclaration.Modifiers.Add(Keywords.Public);

                classDeclaration.Base.InterfaceTypeList.Add(
                    serviceInterfaceName + _cSharpCommonStgService.RenderTypeParamList(typeParameters.Copy())
                );

                using (var outStream = File.Create(outServiceClassFilePath))
                {
                    outStream.Write(Encoding.UTF8.GetBytes(
                        _serviceCommandStgService.RenderServiceFile(
                            serviceNamespace: serviceNamespace,
                            service: classDeclaration
                        )
                    ));
                    outStream.Flush();
                }

                //      Create <service> interface file
                //      Create serviceInterface StringTemplate with empty interface body
                var interfaceDeclaration = new ClassInterfaceDeclaration()
                {
                    IsInterface = true,
                    Identifier = serviceInterfaceName,
                    TypeParameters = typeParameters.Copy()
                };
                interfaceDeclaration.Modifiers.Add(Keywords.Public);

                using (var outStream = File.Create(outServiceInterfaceFilePath))
                {
                    outStream.Write(Encoding.UTF8.GetBytes(
                        _serviceCommandStgService.RenderServiceFile(
                            serviceNamespace: serviceNamespace,
                            service: classDeclaration
                        )
                    ));
                    outStream.Flush();
                }
            }
            else if (classScraperResults is null)
            {
                //var classDeclaration = new ClassInterfaceDeclaration()
                //{
                //    IsInterface = false,
                //    Identifier = serviceClassName,
                //    TypeParameters = typeParameters.Copy(),
                //    Base = new ClassInterfaceBase()
                //};

                //      Create <service> class file
                //      Create serviceClass StringTemplate using interfaceScraperResults

                if (interfaceScraperResults.ServiceDeclaration is null)
                {
                    interfaceScraperResults.ServiceDeclaration = new ClassInterfaceDeclaration()
                    {
                        IsInterface = true,
                        Identifier = serviceInterfaceName,
                        TypeParameters = typeParameters.Copy(),
                        Base = new ClassInterfaceBase()
                    };

                    var tree = serviceInterfaceParser.GetParseTree();
                    var visitor = _serviceInterfaceInjectorFactory.Create(
                        serviceInterfaceParser.Tokens,
                        serviceClassInterfaceName: serviceInterfaceName,
                        serviceFile: interfaceScraperResults,
                        tabString: _userSettings.Value.TabString
                    );
                    visitor.Visit(tree);
                    var siiSuccess = visitor.Success;

                    using (var outStream = File.Create(outServiceInterfaceFilePath))
                    {
                        outStream.Write(Encoding.UTF8.GetBytes(visitor.Rewriter.GetText()));
                        outStream.Flush();
                    }
                }

                var classDeclaration = _serviceCommandService.GetClassFromInterface(
                    interfaceScraperResults.ServiceDeclaration,
                    serviceClassName
                );

                using (var outStream = File.Create(outServiceClassFilePath))
                {
                    outStream.Write(Encoding.UTF8.GetBytes(
                        _serviceCommandStgService.RenderServiceFile(
                            usingDirectives: interfaceScraperResults.UsingDirectives,
                            serviceNamespace: serviceNamespace,
                            service: classDeclaration
                        )
                    ));
                    outStream.Flush();
                }

            }
            else if (interfaceScraperResults is null)
            {
                //      Create <service> interface file
                //      Create serviceInterface StringTemplate using classScraperResults
                if (classScraperResults.ServiceDeclaration is null)
                {
                    classScraperResults.ServiceDeclaration = new ClassInterfaceDeclaration()
                    {
                        IsInterface = false,
                        Identifier = serviceClassName,
                        TypeParameters = typeParameters.Copy(),
                        Base = new ClassInterfaceBase()
                    };
                    classScraperResults.ServiceDeclaration.Base.InterfaceTypeList.Add(
                        serviceInterfaceName + _cSharpCommonStgService.RenderTypeParamList(typeParameters));

                    var tree = serviceClassParser.GetParseTree();
                    var visitor = _serviceClassInjectorFactory.Create(
                        tokenStream: serviceClassParser.Tokens,
                        serviceClassInterfaceName: serviceClassName,
                        serviceFile: classScraperResults,
                        tabString: _userSettings.Value.TabString
                    );
                    visitor.Visit(tree);
                    var sciSuccess = visitor.Success;

                    using (var outStream = File.Create(outServiceClassFilePath))
                    {
                        outStream.Write(Encoding.UTF8.GetBytes(visitor.Rewriter.GetText()));
                        outStream.Flush();
                    }
                }

                var interfaceDeclaration = _serviceCommandService.GetInterfaceFromClass(
                    classScraperResults.ServiceDeclaration,
                    serviceInterfaceName
                );

                using (var outStream = File.Create(outServiceInterfaceFilePath))
                {
                    outStream.Write(Encoding.UTF8.GetBytes(
                        _serviceCommandStgService.RenderServiceFile(
                            usingDirectives: classScraperResults.UsingDirectives,
                            serviceNamespace: serviceNamespace,
                            service: interfaceDeclaration
                        )
                    ));
                    outStream.Flush();
                }
            }
            else
            {
                //Compare lists of method signatures between interface and class
                (var classMissingResults, var interfaceMissingResults) =
                    _serviceCommandService.CompareScraperResults(
                        classScraperResults,
                        interfaceScraperResults);

                if (classScraperResults.ServiceDeclaration is null 
                    && interfaceScraperResults.ServiceDeclaration is null)
                {
                    classMissingResults.ServiceDeclaration = new ClassInterfaceDeclaration()
                    {
                        IsInterface = false,
                        Modifiers = new List<string>() { Keywords.Public },
                        Identifier = serviceClassName,
                        TypeParameters = typeParameters.Copy(),
                        Base = new ClassInterfaceBase()
                    };
                    classMissingResults.ServiceDeclaration.Base.InterfaceTypeList.Add(
                        serviceInterfaceName + _cSharpCommonStgService.RenderTypeParamList(typeParameters));

                    interfaceMissingResults.ServiceDeclaration = new ClassInterfaceDeclaration()
                    {
                        IsInterface = true,
                        Modifiers = new List<string>() { Keywords.Public },
                        Identifier = serviceClassName,
                        TypeParameters = typeParameters.Copy()
                    };
                }
                else if (classScraperResults.ServiceDeclaration is null)
                {
                    classMissingResults.ServiceDeclaration = _serviceCommandService.GetClassFromInterface(
                        interfaceScraperResults.ServiceDeclaration, serviceClassName);
                    interfaceMissingResults.ServiceDeclaration = interfaceScraperResults.ServiceDeclaration.CopyHeader();
                }
                else if (interfaceScraperResults.ServiceDeclaration is null)
                {
                    interfaceMissingResults.ServiceDeclaration = _serviceCommandService.GetInterfaceFromClass(
                        classScraperResults.ServiceDeclaration, serviceInterfaceName);
                    classMissingResults.ServiceDeclaration = classScraperResults.ServiceDeclaration.CopyHeader();
                }

                //  If methods in interface that aren't in class:
                if (classScraperResults.ServiceDeclaration is null
                    | classMissingResults.ServiceDeclaration.Body.MethodDeclarations.Count > 0
                    | classMissingResults.ServiceDeclaration.Body.PropertyDeclarations.Count > 0)
                {
                    //  Create list of methods in interface that aren't in class
                    //  Add missing methods to class
                    //      Create StringTemplate for each method
                    //      Parse class and use rewriter to insert methods into parse tree
                    //      Write parse tree as string to class file

                    var tree = serviceClassParser.GetParseTree();
                    var visitor = _serviceClassInjectorFactory.Create(
                        tokenStream: serviceClassParser.Tokens,
                        serviceClassInterfaceName: serviceClassName,
                        serviceFile: classMissingResults,
                        tabString: _userSettings.Value.TabString
                    );
                    visitor.Visit(tree);
                    var sciSuccess = visitor.Success;

                    using (var outStream = File.Create(outServiceClassFilePath))
                    {
                        outStream.Write(Encoding.UTF8.GetBytes(visitor.Rewriter.GetText()));
                        outStream.Flush();
                    }

                }
                //  If methods in class that are not in interface:
                if (interfaceScraperResults.ServiceDeclaration is null
                    | interfaceMissingResults.ServiceDeclaration.Body.MethodDeclarations.Count > 0
                    | interfaceMissingResults.ServiceDeclaration.Body.PropertyDeclarations.Count > 0)
                {
                    //  Create list of methods in class that are not in interface
                    //  Add missing methods to interface
                    //      Create StringTemplate for each method
                    //      Parse interface and use rewriter to insert methods into parse tree
                    //      Write parse tree as string to interface file

                    var tree = serviceInterfaceParser.GetParseTree();
                    var visitor = _serviceInterfaceInjectorFactory.Create(
                        serviceInterfaceParser.Tokens,
                        serviceClassInterfaceName: serviceInterfaceName,
                        serviceFile: interfaceMissingResults,
                        tabString: _userSettings.Value.TabString
                    );
                    visitor.Visit(tree);
                    var siiSuccess = visitor.Success;

                    using (var outStream = File.Create(outServiceInterfaceFilePath))
                    {
                        outStream.Write(Encoding.UTF8.GetBytes(visitor.Rewriter.GetText()));
                        outStream.Flush();
                    }

                }
            }
        }


        private void RegisterServiceInStartup(
            string serviceRootName,
            List<TypeParameter> typeParameters,
            string serviceLifespan,
            string serviceNamespace)
        {
            var startupFilepath = Path.Combine(_commandLineArgs.Value.ProjectRoot, "Startup.cs");
            var testStartupFilepath = Path.Combine(_commandLineArgs.Value.ProjectRoot, "XStartup.cs");
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
                var startupTree = startupParser.GetParseTree();

                var serviceStartupRegistration = _serviceStartupRegistrationFactory.Create(
                    startupParser.Tokens,
                    rootNamespace: _projectEnvironment.Value.RootNamespace,
                    serviceNamespace: serviceNamespace,
                    serviceName: serviceRootName,
                    hasTypeParameters: typeParameters != null && typeParameters.Count > 0,
                    serviceLifespan: serviceLifespan,
                    tabString: _userSettings.Value.TabString);
                serviceStartupRegistration.Visit(startupTree);

                if (!serviceStartupRegistration.IsServiceRegistered)
                {
                    //using (var outStream = File.Create(startupFilepath))
                    using (var outStream = File.Create(testStartupFilepath))
                    {
                        outStream.Write(Encoding.UTF8.GetBytes(serviceStartupRegistration.Rewriter.GetText()));
                        outStream.Flush();
                    }
                }
            }
        }


        private void InjectServiceIntoClassConstructors(
            ServiceCommand.Controller controller,
            string className,
            string interfaceName,
            string serviceNamespace)
        {
            var controllerAreaDirectory = controller.Area is null
                    || controller.Area.TrimEnd(@"/\ ".ToCharArray()) == ""
                ? "" : Path.Combine("Areas", controller.Area);
            var controllerDirectory = Path.Combine(
                _commandLineArgs.Value.ProjectRoot, controllerAreaDirectory, "Controllers");
            var controllerSubDirectory = Path.Combine(
                controllerDirectory,
                controller.Subdirectories is null
                    || controller.Subdirectories.Count == 0
                    ? ""
                    : string.Join('/', controller.Subdirectories));

            string controllerFilePath = Path.Combine(controllerSubDirectory, $"{controller.Name}.cs");
            string testControllerFilePath = Path.Combine(controllerSubDirectory, $"X{controller.Name}.cs");

            //  Check whether [Controller.Name].cs exists:
            //          If Controller.Area != null:
            //              ~/Areas/[Controller.Area]/Controllers/[Controller.Name].cs
            //          Else:
            //              ~/Controllers/[Controller.Name].cs
            //              ~/Areas/[Service.Area]/Controllers/[Controller.Name].cs
            if (!File.Exists(controllerFilePath))
            {
                //  If !exists:
                //      Throw error
            }
            else
            {
                var controllerServiceIdentifier = controller.ServiceIdentifier
                    ?? (string.IsNullOrEmpty(className) || char.IsLower(className, 0)
                        ? className : char.ToLowerInvariant(className[0]) + className.Substring(1));

                var controllerInjectorParser = new CSharpParserWrapper(controllerFilePath);
                var controllerInjectorTree = controllerInjectorParser.GetParseTree();

                var controllerNamespace = _projectEnvironment.Value.RootNamespace
                    + (controller.Area is null || controller.Area.TrimEnd(@"/\ ".ToCharArray()) == ""
                        ? "" : $".Areas.{controller.Area}")
                    + ".Controllers";

                var fieldDeclaration = new FieldDeclaration()
                {
                    Modifiers = new List<string>() { Keywords.Private, Keywords.Readonly },
                    Type = interfaceName,
                    VariableDeclarators = new List<VariableDeclarator>()
                    {
                        new VariableDeclarator() { Identifier = $"_{controllerServiceIdentifier}" }
                    }
                };

                var constructorParameter = new FixedParameter()
                {
                    Type = interfaceName,
                    Identifier = controllerServiceIdentifier
                };

                var constructorAssignment = new SimpleAssignment()
                {
                    LeftHandSide = $"_{controllerServiceIdentifier}",
                    RightHandSide = controllerServiceIdentifier
                };

                var constructorDeclaration = new ConstructorDeclaration()
                {
                    Modifiers = new List<string>() { Keywords.Public },
                    Identifier = controller.Name,
                    FormalParameterList = new FormalParameterList()
                    {
                        FixedParameters = new List<FixedParameter>() { constructorParameter }
                    },
                    Body = new ConstructorBody()
                    {
                        Statements = new List<Statement>()
                        {
                            new Statement() { SimpleAssignment = constructorAssignment }
                        }
                    }
                };

                var serviceControllerInjector = _serviceConstructorInjectorFactory.Create(
                    controllerInjectorParser.Tokens,
                    constructorClassName: controller.Name,
                    constructorClassNamespace: controllerNamespace,
                    serviceIdentifier: controllerServiceIdentifier,
                    serviceNamespace: serviceNamespace,
                    serviceInterfaceType: interfaceName,
                    fieldDeclaration: fieldDeclaration,
                    constructorParameter: constructorParameter,
                    constructorAssignment: constructorAssignment,
                    constructorDeclaration: constructorDeclaration,
                    tabString: _userSettings.Value.TabString);
                serviceControllerInjector.Visit(controllerInjectorTree);

                if (!serviceControllerInjector.IsServiceInjected)
                {
                    //using (var outStream = File.Create(startupFilepath))
                    using (var outStream = File.Create(testControllerFilePath))
                    {
                        outStream.Write(Encoding.UTF8.GetBytes(serviceControllerInjector.Rewriter.GetText()));
                        outStream.Flush();
                    }
                }
            }
        }

    }
}
