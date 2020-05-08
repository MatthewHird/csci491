using System;
using System.Collections.Generic;
using System.Text;
using Antlr4.Runtime;
using MvcPodium.ConsoleApp.Services;

namespace MvcPodium.ConsoleApp.Visitors.Factories
{
    public interface IBreadcrumbControllerScraperFactory
    {
        BreadcrumbControllerScraper Create(
            BufferedTokenStream tokenStream);
    }

    public class BreadcrumbControllerScraperFactory : IBreadcrumbControllerScraperFactory
    {
        public BreadcrumbControllerScraperFactory()
        {}

        public BreadcrumbControllerScraper Create(
            BufferedTokenStream tokenStream)
        {
            return new BreadcrumbControllerScraper(
                tokenStream);
        }
    }
}
