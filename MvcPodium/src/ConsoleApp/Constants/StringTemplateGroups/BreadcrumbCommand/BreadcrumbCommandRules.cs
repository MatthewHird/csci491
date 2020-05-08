

namespace MvcPodium.ConsoleApp.Constants.StringTemplateGroups.BreadcrumbCommand
{
    public class BreadcrumbServiceFile
    {
        public const string Name = "BreadcrumbServiceFile";

        public class Params
        {
            public const string UsingDirectives = "usingDirectives";
            public const string BreadcrumbNamespace = "breadcrumbNamespace";
            public const string BreadcrumbDeclaration = "breadcrumbDeclaration";
        }
    }

    public class BreadcrumbAssignment
    {
        public const string Name = "BreadcrumbAssignment";

        public class Params
        {
            public const string ControllerRoot = "controllerRoot";
            public const string Action = "action";
            public const string HasId = "hasId";
        }
    }

    public class BreadcrumbNamespaceDeclaration
    {
        public const string Name = "BreadcrumbNamespaceDeclaration";

        public class Params
        {
            public const string BreadcrumbNamespace = "breadcrumbNamespace";
            public const string BreadcrumbDeclaration = "breadcrumbDeclaration";
        }
    }

    public class BreadcrumbClassDeclaration
    {
        public const string Name = "BreadcrumbClassDeclaration";
        public class Params
        {
            public const string Attributes = "attributes";
            public const string Modifiers = "modifiers";
            public const string Identifier = "identifier";
            public const string TypeParameters = "typeParameters";
            public const string Base = "base";
            public const string Body = "body";
        }
    }

    public class BreadcrumbInterfaceDeclaration
    {
        public const string Name = "BreadcrumbInterfaceDeclaration";
        public class Params
        {
            public const string Attributes = "attributes";
            public const string Modifiers = "modifiers";
            public const string Identifier = "identifier";
            public const string TypeParameters = "typeParameters";
            public const string Base = "base";
            public const string Body = "body";
        }
    }
    
    public class BreadcrumbClassMethodDeclaration
    {
        public const string Name = "BreadcrumbClassMethodDeclaration";
        public class Params
        {
            public const string ControllerRoot = "controllerRoot";
            public const string Action = "action";
            public const string HasId = "hasId";
            public const string Controller = "controller";
            public const string ControllerNamePattern = "controllerNamePattern";
        }
    }

    public class BreadcrumbInterfaceMethodDeclaration
    {
        public const string Name = "BreadcrumbInterfaceMethodDeclaration";
        public class Params
        {
            public const string ControllerRoot = "controllerRoot";
            public const string Action = "action";
            public const string HasId = "hasId";
            public const string Controller = "controller";
            public const string ControllerNamePattern = "controllerNamePattern";
        }
    }
}
