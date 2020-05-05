

namespace MvcPodium.ConsoleApp.Constants.StringTemplateGroups.ServiceCommand
{
    public class ServiceFile
    {
        public const string Name = "ServiceFile";

        public class Params
        {
            public const string UsingDirectives = "usingDirectives";
            public const string ServiceNamespace = "serviceNamespace";
            public const string ServiceDeclaration = "serviceDeclaration";
        }
    }

    public class ServiceNamespaceDeclaration
    {
        public const string Name = "ServiceNamespaceDeclaration";

        public class Params
        {
            public const string ServiceNamespace = "serviceNamespace";
            public const string ServiceDeclaration = "serviceDeclaration";
        }
    }

    public class ServiceStartupRegistrationCall
    {
        public const string Name = "ServiceStartupRegistrationCall";

        public class Params
        {
            public const string ServiceLifespan = "serviceLifespan";
            public const string HasTypeParameters = "hasTypeParameters";
            public const string ServiceName = "serviceName";
        }
    }
}
