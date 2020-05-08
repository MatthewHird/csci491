using System;
using System.Collections.Generic;
using System.Text;
using Antlr4.Runtime;
using MvcPodium.ConsoleApp.Models.BreadcrumbCommand;
using MvcPodium.ConsoleApp.Services;

namespace MvcPodium.ConsoleApp.Visitors.Factories
{
    public interface IBreadcrumbClassInjectorFactory
    {
        BreadcrumbClassInjector Create(
            BufferedTokenStream tokenStream,
            List<string> usingDirectives,
            string breadcrumbNamespace,
            BreadcrumbServiceDeclaration breadcrumbDeclaration,
            string tabString);
    }

    public class BreadcrumbClassInjectorFactory : IBreadcrumbClassInjectorFactory
    {
        private readonly IStringUtilService _stringUtilService;
        private readonly ICSharpParserService _cSharpParserService;
        private readonly IBreadcrumbCommandParserService _breadcrumbCommandParserService;
        
        public BreadcrumbClassInjectorFactory(
            IStringUtilService stringUtilService,
            ICSharpParserService cSharpParserService,
            IBreadcrumbCommandParserService breadcrumbCommandParserService)
        {
            _stringUtilService = stringUtilService;
            _cSharpParserService = cSharpParserService;
            _breadcrumbCommandParserService = breadcrumbCommandParserService;
        }

        public BreadcrumbClassInjector Create(
            BufferedTokenStream tokenStream,
            List<string> usingDirectives,
            string breadcrumbNamespace,
            BreadcrumbServiceDeclaration breadcrumbDeclaration,
            string tabString)
        {
            return new BreadcrumbClassInjector(
                _stringUtilService,
                _cSharpParserService,
                _breadcrumbCommandParserService,
                tokenStream,
                usingDirectives,
                breadcrumbNamespace,
                breadcrumbDeclaration,
                tabString);
        }
    }
}
