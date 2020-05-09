using System;
using System.Collections.Generic;
using MvcPodium.ConsoleApp.Models.Config;
using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using MvcPodium.ConsoleApp.Services;
using MvcPodium.ConsoleApp.Constants.CSharpGrammar;
using MvcPodium.ConsoleApp.Models.CSharpCommon;
using MvcPodium.ConsoleApp.Models.ServiceCommand;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Text.RegularExpressions;

namespace MvcPodium.ConsoleApp.Controllers
{
    public class ServiceCommandController
    {
        private readonly ILogger<MvcPodiumController> _logger;
        private readonly IOptions<ProjectEnvironment> _projectEnvironment;
        private readonly IOptions<UserSettings> _userSettings;
        private readonly IIoUtilService _ioUtilService;
        private readonly IServiceCommandService _serviceCommandService;
        private readonly ICSharpCommonStgService _cSharpCommonStgService;
        private readonly IServiceCommandStgService _serviceCommandStgService;
        private readonly IServiceCommandParserService _serviceCommandParserService;
        
        public ServiceCommandController(
            ILogger<MvcPodiumController> logger,
            IOptions<ProjectEnvironment> projectEnvironment,
            IOptions<UserSettings> userSettings,
            IIoUtilService ioUtilService,
            IServiceCommandService serviceCommandService,
            ICSharpCommonStgService cSharpCommonStgService,
            IServiceCommandStgService serviceCommandStgService,
            IServiceCommandParserService serviceCommandParserService)
        {
            _logger = logger;
            _projectEnvironment = projectEnvironment;
            _userSettings = userSettings;
            _ioUtilService = ioUtilService;
            _serviceCommandService = serviceCommandService;
            _cSharpCommonStgService = cSharpCommonStgService;
            _serviceCommandStgService = serviceCommandStgService;
            _serviceCommandParserService = serviceCommandParserService;
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
                || serviceCommand.Area.Trim() == string.Empty
                ? string.Empty : Path.Combine("Areas", serviceCommand.Area);
            var sevicesDirectory = Path.Combine(_projectEnvironment.Value.RootDirectory, serviceAreaDirectory, "Services");
            var serviceSubDirectory = Path.Combine(
                sevicesDirectory,
                serviceCommand.Subdirectory is null ? string.Empty : serviceCommand.Subdirectory.Trim());
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
                + (serviceCommand.Area is null || serviceCommand.Area.TrimEnd(@"/\ ".ToCharArray()) == string.Empty
                    ? string.Empty : $".Areas.{serviceCommand.Area.TrimEnd(@"/\ ".ToCharArray())}")
                + ".Services";
            if (serviceCommand.Subdirectory != null)
            {
                serviceNamespace += "." + serviceCommand.Subdirectory
                    .TrimEnd(@"/\ ".ToCharArray()).Replace("/", ".").Replace(@"\", ".");

            }

            var className = $"{serviceCommand.ServiceRootName}Service";
            var interfaceName = $"I{serviceCommand.ServiceRootName}Service";

            ConsolidateServiceClassAndInterface(
                sourceOfTruth: serviceCommand.SourceOfTruth ?? SourceOfTruth.Class,
                serviceClassName: className,
                serviceInterfaceName: interfaceName,
                typeParameters: serviceCommand.TypeParameters,
                injectedServices: serviceCommand.InjectedServices,
                serviceClassFilePath: serviceClassFilePath,
                serviceInterfaceFilePath: serviceInterfaceFilePath,
                serviceNamespace: serviceNamespace,
                outServiceClassFilePath: testOutServiceClassFilePath,
                outServiceInterfaceFilePath: testOutServiceInterfaceFilePath);

            RegisterServiceInStartup(
                serviceRootName: serviceCommand.ServiceRootName,
                typeParameters: serviceCommand.TypeParameters,
                serviceLifespan: serviceCommand?.ServiceLifespan ?? ServiceLifetime.Scoped,
                serviceNamespace: serviceNamespace);

            //Inject Service into Controllers
            //  For each Controller in Service.Controllers:
            foreach (var controller in serviceCommand.InjectInto)
            {
                InjectServiceIntoClassConstructors(
                    injectIntoClass: controller,
                    serviceClassName: className,
                    serviceInterfaceName: interfaceName,
                    serviceNamespace: serviceNamespace);
            }

            return Task.CompletedTask;
        }


        private void ConsolidateServiceClassAndInterface(
            SourceOfTruth sourceOfTruth,
            string serviceClassName,
            string serviceInterfaceName,
            List<TypeParameter> typeParameters,
            List<InjectedService> injectedServices,
            string serviceClassFilePath,
            string serviceInterfaceFilePath,
            string serviceNamespace,
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

                classScraperResults = _serviceCommandService.ScrapeServiceClass(
                    serviceClassParser,
                    serviceClassName,
                    serviceNamespace,
                    typeParameters);
            }

            //Check if <service> interface file exists:
            if (File.Exists(serviceInterfaceFilePath))
            {
                //  Else:
                //      Parse <service> interface file
                //          Extract list of existing method signatures

                serviceInterfaceParser = new CSharpParserWrapper(serviceInterfaceFilePath);

                interfaceScraperResults =  _serviceCommandService.ScrapeServiceInterface(
                    serviceInterfaceParser,
                    serviceInterfaceName,
                    serviceNamespace,
                    typeParameters);
            }

            var usingDirectives = new HashSet<string>();

            var fieldDeclarations = new List<FieldDeclaration>();

            var ctorParameters = new FormalParameterList();
            var fixedParams = ctorParameters.FixedParameters;

            var ctorBody = new ConstructorBody();
            var statements = ctorBody.Statements;

            var appendBody = new ClassInterfaceBody()
            {
                FieldDeclarations = fieldDeclarations,
                ConstructorDeclaration = new ConstructorDeclaration()
                {
                    FormalParameterList = ctorParameters,
                    Body = ctorBody
                }
            };

            if (injectedServices != null && injectedServices.Count > 0)
            {
                foreach (var injectedService in injectedServices)
                {
                    var injectedServiceIdentifier = injectedService.ServiceIdentifier
                        ?? Regex.Replace(
                            Regex.Replace(
                                injectedService.Type,
                                @"^I?([A-Z])",
                                "$1"),
                            @"^[A-Z]",
                            m => m.ToString().ToLower());

                    if (!Regex.Match(serviceNamespace, "^" + Regex.Escape(injectedService.Namespace)).Success)
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
            }

            if (classScraperResults is null && interfaceScraperResults is null)
            {
                //      Create <service> class file
                //      Create serviceClass StringTemplate with empty class body
                var classDeclaration = new ClassInterfaceDeclaration()
                {
                    IsInterface = false,
                    Modifiers = new List<string>() { Keywords.Public },
                    Identifier = serviceClassName,
                    TypeParameters = typeParameters.Copy(),
                    Base = new ClassInterfaceBase()
                    {
                        InterfaceTypeList = new List<string>()
                        {
                            serviceInterfaceName + _cSharpCommonStgService.RenderTypeParamList(typeParameters.Copy())
                        }
                    },
                    Body = new ClassInterfaceBody()
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

                _ioUtilService.WriteStringToFile(
                    _serviceCommandStgService.RenderServiceFile(
                        usingDirectives: usingDirectives.ToList(),
                        serviceNamespace: serviceNamespace,
                        service: classDeclaration),
                    outServiceClassFilePath);

                //      Create <service> interface file
                //      Create serviceInterface StringTemplate with empty interface body
                var interfaceDeclaration = new ClassInterfaceDeclaration()
                {
                    IsInterface = true,
                    Modifiers = new List<string>() { Keywords.Public },
                    Identifier = serviceInterfaceName,
                    TypeParameters = typeParameters.Copy()
                };

                _ioUtilService.WriteStringToFile(
                    _serviceCommandStgService.RenderServiceFile(
                        serviceNamespace: serviceNamespace,
                        service: interfaceDeclaration),
                    outServiceInterfaceFilePath);
            }
            else if (classScraperResults is null
                || sourceOfTruth == SourceOfTruth.Interface && interfaceScraperResults != null)
            {
                //      Create <service> class file
                //      Create serviceClass StringTemplate using interfaceScraperResults

                if (interfaceScraperResults.ServiceDeclaration is null)
                {
                    interfaceScraperResults.ServiceDeclaration = new ClassInterfaceDeclaration()
                    {
                        IsInterface = true,
                        Modifiers = new List<string>() { Keywords.Public },
                        Identifier = serviceInterfaceName,
                        TypeParameters = typeParameters.Copy(),
                        Base = new ClassInterfaceBase()
                    };

                    var serviceInterfaceRewrite = _serviceCommandService.InjectDataIntoServiceInterface(
                        interfaceScraperResults,
                        serviceInterfaceParser,
                        serviceInterfaceName,
                        _userSettings.Value.TabString);

                    if (serviceInterfaceRewrite != null)
                    {
                        _ioUtilService.WriteStringToFile(
                            serviceInterfaceRewrite,
                            outServiceInterfaceFilePath);
                    }

                }

                _ioUtilService.WriteStringToFile(
                    _serviceCommandService.GetClassServiceFileFromInterface(
                        interfaceScraperResults,
                        serviceClassName,
                        serviceNamespace,
                        usingDirectives.ToList(),
                        appendBody),
                    outServiceClassFilePath);
            }
            else if (interfaceScraperResults is null
                || sourceOfTruth == SourceOfTruth.Class && classScraperResults != null)
            {
                //      Create <service> interface file
                //      Create serviceInterface StringTemplate using classScraperResults
                if (classScraperResults.ServiceDeclaration is null)
                {
                    classScraperResults.ServiceDeclaration = new ClassInterfaceDeclaration()
                    {
                        IsInterface = false,
                        Modifiers = new List<string>() { Keywords.Public },
                        Identifier = serviceClassName,
                        TypeParameters = typeParameters.Copy(),
                        Base = new ClassInterfaceBase()
                        {
                            InterfaceTypeList = new List<string>()
                            {
                                serviceInterfaceName + _cSharpCommonStgService.RenderTypeParamList(typeParameters)
                            }
                        }
                    };
                }

                classScraperResults.ServiceDeclaration.Body.FieldDeclarations = fieldDeclarations;
                classScraperResults.ServiceDeclaration.Body.ConstructorDeclaration = new ConstructorDeclaration()
                {
                    Modifiers = new List<string>() { Keywords.Public },
                    Identifier = serviceClassName,
                    FormalParameterList = ctorParameters,
                    Body = ctorBody
                };


                var serviceClassRewrite = _serviceCommandService.InjectDataIntoServiceClass(
                    classScraperResults,
                    serviceClassParser,
                    serviceClassName,
                    _userSettings.Value.TabString);

                if (serviceClassRewrite != null)
                {
                    _ioUtilService.WriteStringToFile(
                        serviceClassRewrite,
                        outServiceClassFilePath);
                }

                _ioUtilService.WriteStringToFile(
                    _serviceCommandService.GetInterfaceServiceFileFromClass(
                        classScraperResults,
                        serviceInterfaceName,
                        serviceNamespace),
                    outServiceInterfaceFilePath);
            }
            else
            {
                //Compare lists of method signatures between interface and class
                (var classMissingResults, var interfaceMissingResults) =
                    _serviceCommandParserService.CompareScraperResults(
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
                        {
                            InterfaceTypeList = new List<string>()
                            {
                                serviceInterfaceName + _cSharpCommonStgService.RenderTypeParamList(typeParameters)
                            }
                        }
                    };

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
                    classMissingResults.ServiceDeclaration = _serviceCommandParserService.GetClassFromInterface(
                        interfaceScraperResults.ServiceDeclaration, serviceClassName);
                    interfaceMissingResults.ServiceDeclaration = 
                        interfaceScraperResults.ServiceDeclaration.CopyHeader();
                }
                else if (interfaceScraperResults.ServiceDeclaration is null)
                {
                    interfaceMissingResults.ServiceDeclaration = _serviceCommandParserService.GetInterfaceFromClass(
                        classScraperResults.ServiceDeclaration, serviceInterfaceName);
                    classMissingResults.ServiceDeclaration = classScraperResults.ServiceDeclaration.CopyHeader();
                }

                classMissingResults.ServiceDeclaration.Body.FieldDeclarations = fieldDeclarations;
                classMissingResults.ServiceDeclaration.Body.ConstructorDeclaration = new ConstructorDeclaration()
                {
                    Modifiers = new List<string>() { Keywords.Public },
                    Identifier = serviceClassName,
                    FormalParameterList = ctorParameters,
                    Body = ctorBody
                };

                //  If methods in interface that aren't in class:
                if (classScraperResults.ServiceDeclaration is null
                    | classMissingResults.ServiceDeclaration.Body.MethodDeclarations.Count > 0
                    | classMissingResults.ServiceDeclaration.Body.PropertyDeclarations.Count > 0
                    | (injectedServices != null && injectedServices.Count > 0))
                {
                    //  Create list of methods in interface that aren't in class
                    //  Add missing methods to class
                    //      Create StringTemplate for each method
                    //      Parse class and use rewriter to insert methods into parse tree
                    //      Write parse tree as string to class file
                    var serviceClassRewrite = _serviceCommandService.InjectDataIntoServiceClass(
                        classMissingResults,
                        serviceClassParser,
                        serviceClassName,
                        _userSettings.Value.TabString);

                    if (serviceClassRewrite != null)
                    {
                        _ioUtilService.WriteStringToFile(
                            serviceClassRewrite,
                            outServiceClassFilePath);
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
                    var serviceInterfaceRewrite = _serviceCommandService.InjectDataIntoServiceInterface(
                        interfaceMissingResults,
                        serviceInterfaceParser,
                        serviceInterfaceName,
                        _userSettings.Value.TabString);

                    if (serviceInterfaceRewrite != null)
                    {
                        _ioUtilService.WriteStringToFile(
                            serviceInterfaceRewrite,
                            outServiceInterfaceFilePath);
                    }
                }
            }
        }


        private void RegisterServiceInStartup(
            string serviceRootName,
            List<TypeParameter> typeParameters,
            ServiceLifetime serviceLifespan,
            string serviceNamespace)
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
                
                var startupFileRewrite = _serviceCommandService.RegisterServiceInStartup(
                    startupParser: startupParser,
                    rootNamespace: _projectEnvironment.Value.RootNamespace,
                    serviceNamespace: serviceNamespace,
                    serviceName: serviceRootName,
                    hasTypeParameters: typeParameters != null && typeParameters.Count > 0,
                    serviceLifespan: serviceLifespan,
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


        private void InjectServiceIntoClassConstructors(
            InjectIntoClass injectIntoClass,
            string serviceClassName,
            string serviceInterfaceName,
            string serviceNamespace)
        {
            var injectIntoAreaDirectory = injectIntoClass.Area is null
                    || injectIntoClass.Area.TrimEnd(@"/\ ".ToCharArray()) == string.Empty
                ? string.Empty : Path.Combine("Areas", injectIntoClass.Area);
            var injectIntoDirectory = Path.Combine(
                _projectEnvironment.Value.RootDirectory, injectIntoAreaDirectory);
            var injectIntoSubDirectory = Path.Combine(
                injectIntoDirectory,
                injectIntoClass.Subdirectory is null ? string.Empty : injectIntoClass.Subdirectory.Trim());

            string controllerFilePath = Path.Combine(injectIntoSubDirectory, $"{injectIntoClass.ClassName}.cs");
            string testControllerFilePath = Path.Combine(injectIntoSubDirectory, $"X{injectIntoClass.ClassName}.cs");

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
                var serviceIdentifier = injectIntoClass.ServiceIdentifier
                    ?? (string.IsNullOrEmpty(serviceClassName) || char.IsLower(serviceClassName, 0)
                        ? serviceClassName 
                        : char.ToLowerInvariant(serviceClassName[0]) + serviceClassName.Substring(1));

                var injectIntoClassNamespace = _projectEnvironment.Value.RootNamespace
                    + (injectIntoClass.Area is null || injectIntoClass.Area.TrimEnd(@"/\ ".ToCharArray()) == string.Empty
                        ? string.Empty : $".Areas.{injectIntoClass.Area}");

                if (injectIntoClass.Subdirectory != null)
                {
                    injectIntoClassNamespace += "." + injectIntoClass.Subdirectory
                        .TrimEnd(@"/\ ".ToCharArray()).Replace("/", ".").Replace(@"\", ".");

                }

                var fieldDeclaration = new FieldDeclaration()
                {
                    Modifiers = new List<string>() { Keywords.Private, Keywords.Readonly },
                    Type = serviceInterfaceName,
                    VariableDeclarator = new VariableDeclarator() { Identifier = $"_{serviceIdentifier}" }
                };

                var constructorParameter = new FixedParameter()
                {
                    Type = serviceInterfaceName,
                    Identifier = serviceIdentifier
                };

                var constructorAssignment = new SimpleAssignment()
                {
                    LeftHandSide = $"_{serviceIdentifier}",
                    RightHandSide = serviceIdentifier
                };

                var constructorDeclaration = new ConstructorDeclaration()
                {
                    Modifiers = new List<string>() { Keywords.Public },
                    Identifier = injectIntoClass.ClassName,
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

                var controllerInjectorParser = new CSharpParserWrapper(controllerFilePath);
                var controllerRewrite = _serviceCommandService.InjectServiceIntoController(
                            controllerInjectorParser: controllerInjectorParser,
                            constructorClassName: injectIntoClass.ClassName,
                            constructorClassNamespace: injectIntoClassNamespace,
                            serviceIdentifier: serviceIdentifier,
                            serviceNamespace: serviceNamespace,
                            serviceInterfaceType: serviceInterfaceName,
                            fieldDeclaration: fieldDeclaration,
                            constructorParameter: constructorParameter,
                            constructorAssignment: constructorAssignment,
                            constructorDeclaration: constructorDeclaration,
                            tabString: _userSettings.Value.TabString);

                if (controllerRewrite != null)
                {
                    _ioUtilService.WriteStringToFile(
                        controllerRewrite,
                        // startupFilepath);
                        testControllerFilePath);
                }
            }
        }

    }
}
