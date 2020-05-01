using System.Collections.Generic;
using Antlr4.Runtime;
using MvcPodium.ConsoleApp.Model;
using MvcPodium.ConsoleApp.Services;

namespace MvcPodium.ConsoleApp.Visitors.Factories
{
    public interface IServiceConstructorInjectorFactory
    {
        public ServiceConstructorInjector Create(
            BufferedTokenStream tokenStream,
            ServiceConstructionInjectorArguments constructionInjectionArgs);
    }

    public class ServiceConstructorInjectorFactory : IServiceConstructorInjectorFactory
    {
        private readonly IStringUtilService _stringUtilService;
        public ServiceConstructorInjectorFactory(
            IStringUtilService stringUtilService)
        {
            _stringUtilService = stringUtilService;
        }

        public ServiceConstructorInjector Create(
            BufferedTokenStream tokenStream,
            ServiceConstructionInjectorArguments constructionInjectionArgs)
        {
            return new ServiceConstructorInjector(
                tokenStream,
                constructionInjectionArgs,
                _stringUtilService);
        }
    }
}
