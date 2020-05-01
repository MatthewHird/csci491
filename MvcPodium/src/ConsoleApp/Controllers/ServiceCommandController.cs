using System;
using System.Collections.Generic;
using System.Text;
using MvcPodium.ConsoleApp.Model.Config;
using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using MvcPodium.ConsoleApp.Services;
using MvcPodium.ConsoleApp.Model;
using MvcPodium.ConsoleApp.Constants.CSharpGrammar;
using MvcPodium.ConsoleApp.Visitors.Factories;


namespace MvcPodium.ConsoleApp.Controller
{
    public class ServiceCommandController
    {
        private readonly ILogger<MvcPodiumController> _logger;
        private readonly IOptions<CommandLineArgs> _commandLineArgs;
        private readonly IOptions<ProjectEnvironment> _projectEnvironment;
        private readonly IOptions<AppSettings> _appSettings;
        private readonly IOptions<UserSettings> _userSettings;
        private readonly IServiceCommandStService _stringTemplateService;
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
            IOptions<AppSettings> appSettings,
            IOptions<UserSettings> userSettings,
            IServiceCommandStService stringTemplateService,
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
            _appSettings = appSettings;
            _userSettings = userSettings;
            _stringTemplateService = stringTemplateService;
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

            CSharpParserWrapper serviceClassParser = null;
            CSharpParserWrapper serviceInterfaceParser = null;

            ServiceCommandScraperResults classScraperResults = null;
            ServiceCommandScraperResults interfaceScraperResults = null;

            //Check if <service> class file exists: 
            if (File.Exists(serviceClassFilePath))
            {
                //  Else:
                //      Parse <service> class file
                //          Extract list of existing public method signatures

                serviceClassParser = new CSharpParserWrapper(serviceClassFilePath);
                var tree = serviceClassParser.GetParseTree();

                var visitor = _serviceClassScraperFactory.Create(serviceClassParser.Tokens, 
                                                                 serviceCommand.ServiceRootName);
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

                var visitor = _serviceInterfaceScraperFactory.Create(serviceInterfaceParser.Tokens, 
                                                                     serviceCommand.ServiceRootName);
                visitor.Visit(tree);
                interfaceScraperResults = visitor.Results;
            }

            var className = $"{serviceCommand.ServiceRootName}Service";
            var interfaceName = $"I{serviceCommand.ServiceRootName}Service";

            if (classScraperResults is null && interfaceScraperResults is null)
            {
                //      Create <service> class file
                //      Create serviceClass StringTemplate with empty class body
                var classDeclaration = new ClassInterfaceDeclaration()
                {
                    IsInterface = false,
                    Identifier = className,
                    TypeParameters = serviceCommand.TypeParameters.Copy(),
                    Base = new ClassInterfaceBase()
                };
                classDeclaration.Modifiers.Add(Keywords.Public);
                
                classDeclaration.Base.InterfaceTypeList.Add(
                    interfaceName + _stringTemplateService.RenderTypeParamList(serviceCommand.TypeParameters.Copy())
                );

                using (var outStream = File.Create(serviceClassFilePath))
                {
                    outStream.Write(Encoding.UTF8.GetBytes(
                        _stringTemplateService.RenderServiceFile(
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
                    Identifier = interfaceName,
                    TypeParameters = serviceCommand.TypeParameters.Copy()
                };
                interfaceDeclaration.Modifiers.Add(Keywords.Public);

                using (var outStream = File.Create(serviceInterfaceFilePath))
                {
                    outStream.Write(Encoding.UTF8.GetBytes(
                        _stringTemplateService.RenderServiceFile(
                            serviceNamespace: serviceNamespace,
                            service: classDeclaration
                        )
                    ));
                    outStream.Flush();
                }
            }
            else if (classScraperResults is null)
            {
                //      Create <service> class file
                //      Create serviceClass StringTemplate using interfaceScraperResults

                var classDeclaration = _serviceCommandService.GetClassFromInterface(
                    interfaceScraperResults.ClassInterfaceDeclaration,
                    className
                );

                using (var outStream = File.Create(serviceClassFilePath))
                {
                    outStream.Write(Encoding.UTF8.GetBytes(
                        _stringTemplateService.RenderServiceFile(
                            serviceNamespace: serviceNamespace,
                            usingDirectives: interfaceScraperResults.UsingDirectives,
                            service: classDeclaration
                        )
                    ));
                    outStream.Flush();
                }

                Console.WriteLine(
                    
                );
            }
            else if (interfaceScraperResults is null)
            {
                //      Create <service> interface file
                //      Create serviceInterface StringTemplate using classScraperResults

                var interfaceDeclaration = _serviceCommandService.GetInterfaceFromClass(
                    classScraperResults.ClassInterfaceDeclaration,
                    interfaceName
                );

                using (var outStream = File.Create(serviceInterfaceFilePath))
                {
                    outStream.Write(Encoding.UTF8.GetBytes(
                        _stringTemplateService.RenderServiceFile(
                            serviceNamespace: serviceNamespace,
                            usingDirectives: classScraperResults.UsingDirectives,
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
                    _serviceCommandService.CompareScraperResults(classScraperResults, interfaceScraperResults);
                
                //  If methods in interface that aren't in class:
                if (classMissingResults.ClassInterfaceDeclaration.Body.MethodDeclarations.Count > 0
                    | classMissingResults.ClassInterfaceDeclaration.Body.PropertyDeclarations.Count > 0)
                {
                    //  Create list of methods in interface that aren't in class
                    //  Add missing methods to class
                    //      Create StringTemplate for each method
                    //      Parse class and use rewriter to insert methods into parse tree
                    //      Write parse tree as string to class file

                    var properties = classMissingResults.ClassInterfaceDeclaration.Body.PropertyDeclarations;
                    var methods = classMissingResults.ClassInterfaceDeclaration.Body.MethodDeclarations;

                    var propertyDeclarations = new List<string>();
                    var methodDeclarations = new List<string>();

                    foreach (var property in properties)
                    {
                        propertyDeclarations.Add(_stringTemplateService.RenderClassPropertyDeclaration(property));
                    }

                    foreach (var method in methods)
                    {
                        methodDeclarations.Add(_stringTemplateService.RenderClassMethodDeclaration(method));
                    }

                    var classInjectorArgs = new ServiceClassInterfaceInjectorArguments()
                    {
                        ServiceClassInterfaceName = $"{serviceCommand.ServiceRootName}Service",
                        ServiceNamespace = classScraperResults.Namespace,
                        UsingDirectives = classMissingResults.UsingDirectives,
                        PropertyDeclarations = propertyDeclarations,
                        MethodDeclarations = methodDeclarations
                    };

                    var tree = serviceClassParser.GetParseTree();
                    var visitor = _serviceClassInjectorFactory.Create(
                        serviceClassParser.Tokens,
                        classInjectorArgs
                    );
                    visitor.Visit(tree);
                    var siiSuccess = visitor.Success;

                    //using (var outStream = File.Create(outServiceClassFilePath))
                    using (var outStream = File.Create(testOutServiceClassFilePath))
                    {
                        outStream.Write(Encoding.UTF8.GetBytes(visitor.Rewriter.GetText()));
                        outStream.Flush();
                    }

                }
                //  If methods in class that are not in interface:
                if (interfaceMissingResults.ClassInterfaceDeclaration.Body.MethodDeclarations.Count > 0
                    | interfaceMissingResults.ClassInterfaceDeclaration.Body.PropertyDeclarations.Count > 0)
                {
                    //  Create list of methods in class that are not in interface
                    //  Add missing methods to interface
                    //      Create StringTemplate for each method
                    //      Parse interface and use rewriter to insert methods into parse tree
                    //      Write parse tree as string to interface file
                    var properties = interfaceMissingResults.ClassInterfaceDeclaration.Body.PropertyDeclarations;
                    var methods = interfaceMissingResults.ClassInterfaceDeclaration.Body.MethodDeclarations;

                    var propertyDeclarations = new List<string>();
                    var methodDeclarations = new List<string>();

                    foreach (var property in properties)
                    {
                        propertyDeclarations.Add(_stringTemplateService.RenderInterfacePropertyDeclaration(property));
                    }

                    foreach (var method in methods)
                    {
                        methodDeclarations.Add(_stringTemplateService.RenderInterfaceMethodDeclaration(method));
                    }

                    var interfaceInjectorArgs = new ServiceClassInterfaceInjectorArguments()
                    {
                        ServiceClassInterfaceName = $"I{serviceCommand.ServiceRootName}Service",
                        ServiceNamespace = interfaceScraperResults.Namespace,
                        UsingDirectives = interfaceMissingResults.UsingDirectives,
                        PropertyDeclarations = propertyDeclarations,
                        MethodDeclarations = methodDeclarations
                    };

                    var tree = serviceInterfaceParser.GetParseTree();
                    var visitor = _serviceInterfaceInjectorFactory.Create(
                        serviceInterfaceParser.Tokens,
                        interfaceInjectorArgs
                    );
                    visitor.Visit(tree);
                    var siiSuccess = visitor.Success;

                    //using (var outStream = File.Create(outServiceInterfaceFilePath))
                    using (var outStream = File.Create(testOutServiceInterfaceFilePath))
                    {
                        outStream.Write(Encoding.UTF8.GetBytes(visitor.Rewriter.GetText()));
                        outStream.Flush();
                    }

                }
            }

            var serviceRegistrationInfo = new ServiceRegistrationInfo()
            {
                ServiceName = serviceCommand.ServiceRootName,
                HasTypeParameters = serviceCommand.TypeParameters != null && serviceCommand.TypeParameters.Count > 0,
                Scope = serviceCommand.Lifespan.ToString()
            };
            var startupRegistrationCall = _stringTemplateService.RenderServiceStartupRegistrationCall(
                serviceRegistrationInfo);

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

                var serviceRegistrationArgs = new ServiceStartupRegistrationArguments()
                {
                    RootNamespace = _projectEnvironment.Value.RootNamespace,
                    ServiceNamespace = serviceNamespace,
                    StartupRegistrationCall = startupRegistrationCall,
                    ServiceRegistrationInfo = serviceRegistrationInfo
                };

                var serviceStartupRegistration = _serviceStartupRegistrationFactory.Create(
                    startupParser.Tokens,
                    serviceRegistrationArgs);
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

            //Inject Service into Controllers
            //  For each Controller in Service.Controllers:
            foreach (var controller in serviceCommand.Controllers)
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

                    var fieldDeclaration = _stringTemplateService.RenderFieldDeclaration(
                        new FieldDeclaration()
                        {
                            Modifiers = new List<string>() { Keywords.Private, Keywords.Readonly },
                            Type = interfaceName,
                            VariableDeclarators = new List<VariableDeclarator>()
                            {
                                new VariableDeclarator() { Identifier = $"_{controllerServiceIdentifier}" }
                            }
                        }
                    );

                    var ctorParam = new FixedParameter()
                    {
                        Type = interfaceName,
                        Identifier = controllerServiceIdentifier
                    };

                    var constructorParameter = _stringTemplateService.RenderFixedParameter(ctorParam);

                    var ctorAssignment = new SimpleAssignment()
                    {
                        LeftHandSide = $"_{controllerServiceIdentifier}",
                        RightHandSide = controllerServiceIdentifier
                    };

                    var constructorAssignment = _stringTemplateService.RenderSimpleAssignment(ctorAssignment);

                    var constructorDeclaration = _stringTemplateService.RenderConstructorDeclaration(
                        new ConstructorDeclaration()
                        {
                            Modifiers = new List<string>() { Keywords.Public },
                            Identifier = controller.Name,
                            FormalParameterList = new FormalParameterList()
                            {
                                FixedParameters = new List<FixedParameter>() { ctorParam }
                            },
                            Body = new ConstructorBody()
                            {
                                Statements = new List<Statement>()
                                {
                                    new Statement() { SimpleAssignment = ctorAssignment }
                                }
                            }
                        }
                    );

                    var constructionInjectionArgs = new ServiceConstructionInjectorArguments()
                    {
                        ConstructorClassName = controller.Name,
                        ConstructorClassNamespace = controllerNamespace,
                        ServiceIdentifier = controllerServiceIdentifier,
                        ServiceNamespace = serviceNamespace,
                        ServiceInterfaceType = interfaceName,
                        FieldDeclaration = fieldDeclaration,
                        ConstructorParameter = constructorParameter,
                        ConstructorAssignment = constructorAssignment,
                        ConstructorDeclaration = constructorDeclaration
                    };

                    var serviceControllerInjector = _serviceConstructorInjectorFactory.Create(
                        controllerInjectorParser.Tokens,
                        constructionInjectionArgs);
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

            return Task.CompletedTask;
        }
    }
}
