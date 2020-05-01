using System.Collections.Generic;
using Antlr4.Runtime;
using MvcPodium.ConsoleApp.Model;
using MvcPodium.ConsoleApp.Services;

namespace MvcPodium.ConsoleApp.Visitors.Factories
{
    public interface IServiceClassInjectorFactory
    {
        public ServiceClassInjector Create(
            BufferedTokenStream tokenStream,
            ServiceClassInterfaceInjectorArguments serviceInjectorArgs);
    }

    public class ServiceClassInjectorFactory : IServiceClassInjectorFactory
    {
        private readonly IStringUtilService _stringUtilService;
        public ServiceClassInjectorFactory(
            IStringUtilService stringUtilService)
        {
            _stringUtilService = stringUtilService;
        }

        public ServiceClassInjector Create(
            BufferedTokenStream tokenStream,
            ServiceClassInterfaceInjectorArguments serviceInjectorArgs)
        {
            return new ServiceClassInjector(
                tokenStream,
                serviceInjectorArgs,
                _stringUtilService);
        }
    }
}
