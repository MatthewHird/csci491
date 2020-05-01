using System.Collections.Generic;
using Antlr4.Runtime;
using MvcPodium.ConsoleApp.Model;
using MvcPodium.ConsoleApp.Services;

namespace MvcPodium.ConsoleApp.Visitors.Factories
{
    public interface IServiceInterfaceInjectorFactory
    {
        ServiceInterfaceInjector Create(
            BufferedTokenStream tokenStream,
            ServiceClassInterfaceInjectorArguments serviceInjectorArgs);
    }
    
    public class ServiceInterfaceInjectorFactory : IServiceInterfaceInjectorFactory
    {
        private readonly IStringUtilService _stringUtilService;
        public ServiceInterfaceInjectorFactory(
            IStringUtilService stringUtilService)
        {
            _stringUtilService = stringUtilService;
        }

        public ServiceInterfaceInjector Create(
            BufferedTokenStream tokenStream,
            ServiceClassInterfaceInjectorArguments serviceInjectorArgs)
        {
            return new ServiceInterfaceInjector(
                tokenStream,
                serviceInjectorArgs,
                _stringUtilService);
        }
    }
}
