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

        private readonly Dictionary<string, string> _writtenTo = new Dictionary<string, string>();
        
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
            var serviceArea = serviceCommand?.Area?.TrimEnd(@"/\ ".ToCharArray());
            var hasArea = serviceCommand.Area != null && serviceArea != string.Empty;
            var serviceAreaDirectory = Path.Combine(
                _projectEnvironment.Value.RootDirectory, 
                hasArea ? Path.Combine("Areas", serviceCommand.Area) : string.Empty);
            var serviceSubDirectory = Path.Combine(
                serviceAreaDirectory,
                serviceCommand.Subdirectory is null ? "Services" : serviceCommand.Subdirectory.Trim());
            Directory.CreateDirectory(serviceSubDirectory);

            string serviceClassFilePath = Path.Combine(
                serviceSubDirectory, $"{serviceCommand.ServiceName}.cs");
            string serviceInterfaceFilePath = Path.Combine(
                serviceSubDirectory, $"I{serviceCommand.ServiceName}.cs");

            string testOutServiceClassFilePath = Path.Combine(
                serviceSubDirectory, $"X{serviceCommand.ServiceName}.cs");
            string testOutServiceInterfaceFilePath = Path.Combine(
                serviceSubDirectory, $"XI{serviceCommand.ServiceName}.cs");

            var serviceNamespace = _projectEnvironment.Value.RootNamespace 
                + (hasArea ? $".Areas.{serviceCommand.Area.TrimEnd(@"/\ ".ToCharArray())}" : string.Empty)
                + ".Services";
            if (serviceCommand.Subdirectory != null)
            {
                serviceNamespace += "." + serviceCommand.Subdirectory
                    .TrimEnd(@"/\ ".ToCharArray()).Replace("/", ".").Replace(@"\", ".");

            }

            var className = serviceCommand.ServiceName;
            var interfaceName = $"I{serviceCommand.ServiceName}";
            var serviceBaseName = serviceCommand.IsClassOnly ?? false ? className : interfaceName;

            _logger.LogInformation($"Generating/updating service \"{className}\"" +
                (hasArea ? $" in area \"{serviceArea}\"." : "."));

            if (serviceCommand.IsClassOnly ?? false)
            {
                ValidateServiceClass(
                    serviceClassName: className,
                    typeParameters: serviceCommand.TypeParameters,
                    injectedServices: serviceCommand.InjectedServices,
                    serviceClassFilePath: serviceClassFilePath,
                    serviceNamespace: serviceNamespace,
                    outServiceClassFilePath: testOutServiceClassFilePath);
            }
            else
            {
                _logger.LogDebug($"Consolidating service class {className} and service interface {interfaceName}.");
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
            }

            _logger.LogDebug($"Registering {serviceBaseName} in Startup.cs ...");
            RegisterServiceInStartup(
                serviceClassType: className,
                serviceBaseType: serviceBaseName,
                typeParameters: serviceCommand.TypeParameters,
                serviceLifespan: serviceCommand?.ServiceLifespan ?? ServiceLifetime.Scoped,
                serviceNamespace: serviceNamespace);

            //Inject Service into Controllers
            //  For each Controller in Service.Controllers:

            if (serviceCommand.InjectInto != null && serviceCommand.InjectInto.Count > 0)
            {
                _logger.LogDebug($"Injecting {className} into class constructors...");
                InjectServiceIntoClassConstructors(
                    injectIntoClasses: serviceCommand.InjectInto,
                    serviceClassName: className,
                    serviceBaseType: serviceBaseName,
                    serviceNamespace: serviceNamespace);
            }

            return Task.CompletedTask;
        }


        private void ValidateServiceClass(
            string serviceClassName,
            List<TypeParameter> typeParameters,
            List<InjectedService> injectedServices,
            string serviceClassFilePath,
            string serviceNamespace,
            string outServiceClassFilePath)
        {
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

            var serviceClassFile = new ServiceFile()
            {
                UsingDirectives = usingDirectives.ToList(),
                ServiceNamespace = serviceNamespace,
                ServiceDeclaration = new ClassInterfaceDeclaration()
                {
                    IsInterface = false,
                    Modifiers = new List<string>() { Keywords.Public },
                    Identifier = serviceClassName,
                    TypeParameters = typeParameters.Copy(),
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
                }
            };

            if (!File.Exists(serviceClassFilePath))
            {
                //      Create <service> class file
                //      Create serviceClass StringTemplate with empty class body
                _logger.LogDebug("Creating new service class and writing it to file.");
                UpdateWrittenTo(serviceClassFilePath, outServiceClassFilePath);
                _ioUtilService.WriteStringToFile(
                    _serviceCommandStgService.RenderServiceFile(
                        usingDirectives: usingDirectives.ToList(),
                        serviceNamespace: serviceNamespace,
                        service: serviceClassFile.ServiceDeclaration),
                    outServiceClassFilePath);
            }
            else
            {
                var serviceClassParser = new CSharpParserWrapper(GetPathFromWrittenTo(serviceClassFilePath));

                var serviceClassRewrite = _serviceCommandService.InjectDataIntoServiceClass(
                    serviceClassFile,
                    serviceClassParser,
                    serviceClassName,
                    _userSettings.Value.TabString);

                if (serviceClassRewrite != null)
                {
                    _logger.LogDebug("Overwriting service class file.");
                    UpdateWrittenTo(serviceClassFilePath, outServiceClassFilePath);
                    _ioUtilService.WriteStringToFile(
                        serviceClassRewrite,
                        outServiceClassFilePath);
                }
                else
                {
                    _logger.LogDebug("Service class file was already up to date.");
                }
            }
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

            CSharpParserWrapper serviceClassParser = null;
            CSharpParserWrapper serviceInterfaceParser = null;

            ServiceFile classScraperResults = null;
            ServiceFile interfaceScraperResults = null;

            //Check if <service> class file exists: 
            if (File.Exists(serviceClassFilePath))
            {
                _logger.LogDebug("Service class file found. Pulling data from service class.");
                //  Else:
                //      Parse <service> class file
                //          Extract list of existing public method signatures

                serviceClassParser = new CSharpParserWrapper(GetPathFromWrittenTo(serviceClassFilePath));

                classScraperResults = _serviceCommandService.ScrapeServiceClass(
                    serviceClassParser,
                    serviceClassName,
                    serviceNamespace,
                    typeParameters);
            }
            else
            {
                _logger.LogDebug($"No service class file found at {serviceClassFilePath}.");

            }

            //Check if <service> interface file exists:
            if (File.Exists(serviceInterfaceFilePath))
            {
                _logger.LogDebug("Service interface file found. Pulling data from service interface.");
                //  Else:
                //      Parse <service> interface file
                //          Extract list of existing method signatures

                serviceInterfaceParser = new CSharpParserWrapper(GetPathFromWrittenTo(serviceInterfaceFilePath));

                interfaceScraperResults =  _serviceCommandService.ScrapeServiceInterface(
                    serviceInterfaceParser,
                    serviceInterfaceName,
                    serviceNamespace,
                    typeParameters);
            }
            else
            {
                _logger.LogDebug($"No service interface file found at {serviceInterfaceFilePath}.");
            }

            if (classScraperResults is null && interfaceScraperResults is null)
            {
                _logger.LogDebug("Creating new service class and interface and writing them to file.");

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
                UpdateWrittenTo(serviceClassFilePath, outServiceClassFilePath);
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

                UpdateWrittenTo(serviceInterfaceFilePath, outServiceInterfaceFilePath);
                _ioUtilService.WriteStringToFile(
                    _serviceCommandStgService.RenderServiceFile(
                        serviceNamespace: serviceNamespace,
                        service: interfaceDeclaration),
                    outServiceInterfaceFilePath);
            }
            else if (classScraperResults is null)
            {
                //      Create <service> class file
                //      Create serviceClass StringTemplate using interfaceScraperResults

                if (interfaceScraperResults.ServiceDeclaration is null)
                {
                    _logger.LogDebug("Service interface file is missing the interface declaration. " +
                        "Adding new interface declaration to existing file.");

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
                        UpdateWrittenTo(serviceInterfaceFilePath, outServiceInterfaceFilePath);
                        _ioUtilService.WriteStringToFile(
                            serviceInterfaceRewrite,
                            outServiceInterfaceFilePath);
                    }

                }
                _logger.LogDebug(
                    "Creating service class based on existing service interface and writing it to file.");

                UpdateWrittenTo(serviceClassFilePath, outServiceClassFilePath);
                _ioUtilService.WriteStringToFile(
                    _serviceCommandService.GetClassServiceFileFromInterface(
                        interfaceScraperResults,
                        serviceClassName,
                        serviceNamespace,
                        usingDirectives.ToList(),
                        appendBody),
                    outServiceClassFilePath);
            }
            else if (sourceOfTruth == SourceOfTruth.Interface 
                && classScraperResults != null
                && interfaceScraperResults != null)
            {
                if (interfaceScraperResults.ServiceDeclaration is null)
                {
                    _logger.LogDebug("Service interface file is missing the interface declaration. " +
                        "Adding new interface declaration to existing file.");

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
                        UpdateWrittenTo(serviceInterfaceFilePath, outServiceInterfaceFilePath);
                        _ioUtilService.WriteStringToFile(
                            serviceInterfaceRewrite,
                            outServiceInterfaceFilePath);
                    }

                }
                _logger.LogDebug("Updating service class to match existing service interface and writing it to file.");

                var newServiceClass = _serviceCommandParserService.GetClassFromInterface(
                        interfaceScraperResults.ServiceDeclaration, serviceClassName);

                newServiceClass.Body.FieldDeclarations = fieldDeclarations;
                newServiceClass.Body.ConstructorDeclaration = new ConstructorDeclaration()
                {
                    Modifiers = new List<string>() { Keywords.Public },
                    Identifier = serviceClassName,
                    FormalParameterList = ctorParameters,
                    Body = ctorBody
                };

                usingDirectives.UnionWith(interfaceScraperResults.UsingDirectives);

                var newClassFile = new ServiceFile()
                {
                    UsingDirectives = usingDirectives.ToList(),
                    ServiceNamespace = interfaceScraperResults.ServiceNamespace,
                    ServiceDeclaration = newServiceClass
                };


                var serviceClassRewrite = _serviceCommandService.InjectDataIntoServiceClass(
                    newClassFile,
                    serviceClassParser,
                    serviceClassName,
                    _userSettings.Value.TabString);

                if (serviceClassRewrite != null)
                {
                    _logger.LogDebug("Overwriting service class file.");
                    UpdateWrittenTo(serviceClassFilePath, outServiceClassFilePath);
                    _ioUtilService.WriteStringToFile(
                        serviceClassRewrite,
                        outServiceClassFilePath);
                }
                else
                {
                    _logger.LogDebug("Service class file was already up to date.");
                }
            }
            else if (interfaceScraperResults is null
                || sourceOfTruth == SourceOfTruth.Class && classScraperResults != null)
            {
                //      Create <service> interface file
                //      Create serviceInterface StringTemplate using classScraperResults
                if (classScraperResults.ServiceDeclaration is null)
                {
                    _logger.LogDebug("Service class file is missing the class declaration. " +
                        "Adding new class declaration to existing file.");
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
                    UpdateWrittenTo(serviceClassFilePath, outServiceClassFilePath);
                    _ioUtilService.WriteStringToFile(
                        serviceClassRewrite,
                        outServiceClassFilePath);
                }
                _logger.LogDebug(
                    "Creating service interface based on existing service class and writing it to file.");
                UpdateWrittenTo(serviceInterfaceFilePath, outServiceInterfaceFilePath);

                _ioUtilService.WriteStringToFile(
                    _serviceCommandService.GetInterfaceServiceFileFromClass(
                        classScraperResults,
                        serviceInterfaceName,
                        serviceNamespace),
                    outServiceInterfaceFilePath);
            }
            else
            {
                _logger.LogDebug("Comparing service class data with service interface data.");
                //Compare lists of method signatures between interface and class
                (var classMissingResults, var interfaceMissingResults) =
                    _serviceCommandParserService.CompareScraperResults(
                        classScraperResults,
                        interfaceScraperResults);

                if (classScraperResults.ServiceDeclaration is null
                    && interfaceScraperResults.ServiceDeclaration is null)
                {
                    _logger.LogDebug(
                        "Both the class file and the interface file are missing a class/interface declaration. " +
                        "Creating new service class declaration and service interface declaration.");

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
                    _logger.LogDebug("Service class file is missing a class declaration. " +
                        "Creating new service class declaration based on the existing service interface.");

                    classMissingResults.ServiceDeclaration = _serviceCommandParserService.GetClassFromInterface(
                        interfaceScraperResults.ServiceDeclaration, serviceClassName);
                    interfaceMissingResults.ServiceDeclaration = 
                        interfaceScraperResults.ServiceDeclaration.CopyHeader();
                }
                else if (interfaceScraperResults.ServiceDeclaration is null)
                {
                    _logger.LogDebug("Service interface file is missing an interface declaration. " +
                        "Creating new service interface declaration based on the existing service class.");
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

                usingDirectives.UnionWith(classMissingResults.UsingDirectives);
                classMissingResults.UsingDirectives = usingDirectives.ToList();

                //  If methods in interface that aren't in class:
                if (classScraperResults.ServiceDeclaration is null
                    | classMissingResults.ServiceDeclaration.Body.MethodDeclarations.Count > 0
                    | classMissingResults.ServiceDeclaration.Body.PropertyDeclarations.Count > 0
                    | (injectedServices != null && injectedServices.Count > 0))
                {
                    _logger.LogDebug("Injecting updated class declaration into parsed service class file.");
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
                        UpdateWrittenTo(serviceClassFilePath, outServiceClassFilePath);
                        _logger.LogDebug("Overwriting service class file.");
                        _ioUtilService.WriteStringToFile(
                            serviceClassRewrite,
                            outServiceClassFilePath);
                    }
                    else
                    {
                        _logger.LogDebug("Service class file was already up to date.");
                    }
                }
                //  If methods in class that are not in interface:
                if (interfaceScraperResults.ServiceDeclaration is null
                    | interfaceMissingResults.ServiceDeclaration.Body.MethodDeclarations.Count > 0
                    | interfaceMissingResults.ServiceDeclaration.Body.PropertyDeclarations.Count > 0)
                {
                    _logger.LogDebug("Injecting updated interface declaration into parsed interface file.");
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
                        UpdateWrittenTo(serviceInterfaceFilePath, outServiceInterfaceFilePath);
                        _logger.LogDebug("Overwriting service interface file.");
                        _ioUtilService.WriteStringToFile(
                            serviceInterfaceRewrite,
                            outServiceInterfaceFilePath);
                    }
                    else
                    {
                        _logger.LogDebug("Service interface file was already up to date.");
                    }
                }
            }
        }


        private void RegisterServiceInStartup(
            string serviceClassType,
            string serviceBaseType,
            List<TypeParameter> typeParameters,
            ServiceLifetime serviceLifespan,
            string serviceNamespace)
        {
            var startupFilepath = Path.Combine(_projectEnvironment.Value.RootDirectory, "Startup.cs");
            var testStartupFilepath = Path.Combine(_projectEnvironment.Value.RootDirectory, "XStartup.cs");
            //Check if Startup.cs exists
            if (!File.Exists(startupFilepath))
            {
                _logger.LogWarning($"Startup file does not exist at path {startupFilepath}");
                //  If !exists:
                //      Throw error
            }
            else
            {
                _logger.LogDebug("Attempting to register service in \"Startup.cs\".");
                //  Else:
                //      Parse Startup.cs
                var startupParser = new CSharpParserWrapper(GetPathFromWrittenTo(startupFilepath));
                
                var startupFileRewrite = _serviceCommandService.RegisterServiceInStartup(
                    startupParser: startupParser,
                    rootNamespace: _projectEnvironment.Value.RootNamespace,
                    serviceNamespace: serviceNamespace,
                    serviceClassType: serviceClassType,
                    serviceBaseType: serviceBaseType,
                    hasTypeParameters: typeParameters != null && typeParameters.Count > 0,
                    serviceLifespan: serviceLifespan,
                    tabString: _userSettings.Value.TabString);

                if (startupFileRewrite != null)
                {
                    UpdateWrittenTo(startupFilepath, testStartupFilepath);
                    _logger.LogDebug(
                        "Service registration has been added to \"Startup.cs\". Overwriting \"Startup.cs\"");
                    _ioUtilService.WriteStringToFile(
                        startupFileRewrite,
                        // startupFilepath);
                        testStartupFilepath);
                }
                else
                {
                    _logger.LogDebug("Service was already registration in \"Startup.cs\".");
                }
            }
        }


        private void InjectServiceIntoClassConstructors(
            List<InjectIntoClass> injectIntoClasses,
            string serviceClassName,
            string serviceBaseType,
            string serviceNamespace)
        {
            if (injectIntoClasses.Count > 0)
            {
                _logger.LogDebug($"Attempting to inject service into {injectIntoClasses.Count} classes.");
            }

            foreach (var injectIntoClass in injectIntoClasses)
            {
                var injectIntoAreaDirectory = injectIntoClass.Area is null
                        || injectIntoClass.Area.TrimEnd(@"/\ ".ToCharArray()) == string.Empty
                    ? string.Empty : Path.Combine("Areas", injectIntoClass.Area);
                var injectIntoDirectory = Path.Combine(
                    _projectEnvironment.Value.RootDirectory, injectIntoAreaDirectory);
                var injectIntoSubDirectory = Path.Combine(
                    injectIntoDirectory,
                    injectIntoClass.Subdirectory is null ? string.Empty : injectIntoClass.Subdirectory.Trim());

                string constructorFilePath = Path.Combine(injectIntoSubDirectory, $"{injectIntoClass.ClassName}.cs");
                string testConstructorFilePath = Path.Combine(injectIntoSubDirectory, $"X{injectIntoClass.ClassName}.cs");

                //  Check whether [Controller.Name].cs exists:
                //          If Controller.Area != null:
                //              ~/Areas/[Controller.Area]/Controllers/[Controller.Name].cs
                //          Else:
                //              ~/Controllers/[Controller.Name].cs
                //              ~/Areas/[Service.Area]/Controllers/[Controller.Name].cs
                if (!File.Exists(constructorFilePath))
                {
                    _logger.LogWarning($"Failed to inject service into {injectIntoClass.ClassName}. " +
                        $"{constructorFilePath} does not exist.");
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
                        + (injectIntoClass.Area is null 
                        || injectIntoClass.Area.TrimEnd(@"/\ ".ToCharArray()) == string.Empty
                            ? string.Empty : $".Areas.{injectIntoClass.Area}");

                    if (injectIntoClass.Subdirectory != null)
                    {
                        injectIntoClassNamespace += "." + injectIntoClass.Subdirectory
                            .TrimEnd(@"/\ ".ToCharArray()).Replace("/", ".").Replace(@"\", ".");

                    }

                    var fieldDeclaration = new FieldDeclaration()
                    {
                        Modifiers = new List<string>() { Keywords.Private, Keywords.Readonly },
                        Type = serviceBaseType,
                        VariableDeclarator = new VariableDeclarator() { Identifier = $"_{serviceIdentifier}" }
                    };

                    var constructorParameter = new FixedParameter()
                    {
                        Type = serviceBaseType,
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
                    _logger.LogDebug($"Attempting to inject service into {injectIntoClass.ClassName}.");

                    var constructorInjectorParser = new CSharpParserWrapper(GetPathFromWrittenTo(constructorFilePath));
                    var constructorRewrite = _serviceCommandService.InjectServiceIntoConstructor(
                                constructorInjectorParser: constructorInjectorParser,
                                constructorClassName: injectIntoClass.ClassName,
                                constructorClassNamespace: injectIntoClassNamespace,
                                serviceIdentifier: serviceIdentifier,
                                serviceNamespace: serviceNamespace,
                                serviceInterfaceType: serviceBaseType,
                                fieldDeclaration: fieldDeclaration,
                                constructorParameter: constructorParameter,
                                constructorAssignment: constructorAssignment,
                                constructorDeclaration: constructorDeclaration,
                                tabString: _userSettings.Value.TabString);

                    if (constructorRewrite != null)
                    {
                        UpdateWrittenTo(constructorFilePath, testConstructorFilePath);

                        _logger.LogDebug($"Overwriting {constructorFilePath}.");
                        _ioUtilService.WriteStringToFile(
                            constructorRewrite,
                            // constructorFilePath);
                            testConstructorFilePath);
                    }
                    else
                    {
                        _logger.LogDebug($"Service was already injected into {injectIntoClass.ClassName}.");
                    }
                }

            }
        }

        private string GetPathFromWrittenTo(string key)
        {
            return _writtenTo.ContainsKey(key) ? _writtenTo[key] : key;
        }

        private void UpdateWrittenTo(string key, string value)
        {
            _writtenTo[key] = value;
        }

    }
}
