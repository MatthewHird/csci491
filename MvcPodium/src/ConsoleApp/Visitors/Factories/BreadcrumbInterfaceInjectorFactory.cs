using System;
using System.Collections.Generic;
using System.Text;
using Antlr4.Runtime;
using MvcPodium.ConsoleApp.Services;

namespace MvcPodium.ConsoleApp.Visitors.Factories
{
    public interface IBreadcrumbInterfaceInjectorFactory
    {
        BreadcrumbInterfaceInjector Create(
            BufferedTokenStream tokenStream,
            string tabString);
    }

    public class BreadcrumbInterfaceInjectorFactory : IBreadcrumbInterfaceInjectorFactory
    {
        public BreadcrumbInterfaceInjectorFactory()
        {}

        public BreadcrumbInterfaceInjector Create(
            BufferedTokenStream tokenStream,
            string tabString)
        {
            return new BreadcrumbInterfaceInjector(
                tokenStream,
                tabString);
        }
    }
}
