using System;
using System.Collections.Generic;
using System.Text;
using Antlr4.Runtime;
using MvcPodium.ConsoleApp.Models.BreadcrumbCommand;
using MvcPodium.ConsoleApp.Services;

namespace MvcPodium.ConsoleApp.Visitors.Factories
{
    public interface IBreadcrumbControllerInjectorFactory
    {
        BreadcrumbControllerInjector Create(
            BufferedTokenStream tokenStream,
            ControllerDictionary controllerDictionary,
            string breadcrumbServiceNamespace,
            string controllerRootNamespace,
            string defaultAreaBreadcrumbServiceRootName,
            string tabString);
    }

    public class BreadcrumbControllerInjectorFactory : IBreadcrumbControllerInjectorFactory
    {
        private readonly IStringUtilService _stringUtilService;
        private readonly ICSharpParserService _cSharpParserService;
        private readonly IBreadcrumbCommandParserService _breadcrumbCommandParserService;
        private readonly ICSharpCommonStgService _cSharpCommonStgService;

        public BreadcrumbControllerInjectorFactory(
            IStringUtilService stringUtilService,
            ICSharpParserService cSharpParserService,
            IBreadcrumbCommandParserService breadcrumbCommandParserService,
            ICSharpCommonStgService cSharpCommonStgService)
        {
            _stringUtilService = stringUtilService;
            _cSharpParserService = cSharpParserService;
            _breadcrumbCommandParserService = breadcrumbCommandParserService;
            _cSharpCommonStgService = cSharpCommonStgService;
        }

        public BreadcrumbControllerInjector Create(
            BufferedTokenStream tokenStream,
            ControllerDictionary controllerDictionary,
            string breadcrumbServiceNamespace,
            string controllerRootNamespace,
            string defaultAreaBreadcrumbServiceRootName,
            string tabString)
        {
            return new BreadcrumbControllerInjector(
                _stringUtilService,
                _cSharpParserService,
                _breadcrumbCommandParserService,
                _cSharpCommonStgService,
                tokenStream,
                controllerDictionary,
                breadcrumbServiceNamespace,
                controllerRootNamespace,
                defaultAreaBreadcrumbServiceRootName,
                tabString);
        }
    }
}
