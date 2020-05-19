using Antlr4.Runtime;
using Microsoft.Extensions.Logging;
using MvcPodium.ConsoleApp.Controllers;
using MvcPodium.ConsoleApp.Models.CSharpCommon;
using MvcPodium.ConsoleApp.Services;

namespace MvcPodium.ConsoleApp.Visitors.Factories
{
    public interface IServiceConstructorInjectorFactory
    {
        public ServiceConstructorInjector Create(
            BufferedTokenStream tokenStream,
            string constructorClassName,
            string constructorClassNamespace,
            string serviceIdentifier,
            string serviceNamespace,
            string serviceInterfaceType,
            FieldDeclaration fieldDeclaration,
            FixedParameter constructorParameter,
            SimpleAssignment constructorAssignment,
            ConstructorDeclaration constructorDeclaration,
            string tabString = null);
    }

    public class ServiceConstructorInjectorFactory : IServiceConstructorInjectorFactory
    {
        private readonly IStringUtilService _stringUtilService;
        private readonly ICSharpParserService _cSharpParserService;
        private readonly ICSharpCommonStgService _cSharpCommonStgService;
        private readonly ILogger<MvcPodiumController> _logger;
        public ServiceConstructorInjectorFactory(
            IStringUtilService stringUtilService,
            ICSharpParserService cSharpParserService,
            ICSharpCommonStgService cSharpCommonStgService,
            ILogger<MvcPodiumController> logger)
        {
            _stringUtilService = stringUtilService;
            _cSharpParserService = cSharpParserService;
            _cSharpCommonStgService = cSharpCommonStgService;
            _logger = logger;
        }

        public ServiceConstructorInjector Create(
            BufferedTokenStream tokenStream,
            string constructorClassName,
            string constructorClassNamespace,
            string serviceIdentifier,
            string serviceNamespace,
            string serviceInterfaceType,
            FieldDeclaration fieldDeclaration,
            FixedParameter constructorParameter,
            SimpleAssignment constructorAssignment,
            ConstructorDeclaration constructorDeclaration,
            string tabString = null)
        {
            return new ServiceConstructorInjector(
                _stringUtilService,
                _cSharpParserService,
                _cSharpCommonStgService,
                _logger,
                tokenStream,
                constructorClassName,
                constructorClassNamespace,
                serviceIdentifier,
                serviceNamespace,
                serviceInterfaceType,
                fieldDeclaration,
                constructorParameter,
                constructorAssignment,
                constructorDeclaration,
                tabString);
        }
    }
}
