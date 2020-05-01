using System;
using System.Collections.Generic;
using System.Text;

namespace MvcPodium.ConsoleApp.Constants.StringTemplateGroups.ServiceCommand
{
    public class ServiceFile
    {
        public const string Name = "ServiceFile";

        public class Params
        {
            public const string ServiceNamespace = "serviceNamespace";
            public const string UsingDirectives = "usingDirectives";
            public const string Service = "service";
        }
    }

    public class TypeParamList
    {
        public const string Name = "TypeParamList";

        public class Params
        {
            public const string TypeParamList = "typeParamList";
        }
    }

    public class ClassMethodDeclaration
    {
        public const string Name = "ClassMethodDeclaration";

        public class Params
        {
            public const string Method = "method";
        }
    }

    public class ClassPropertyDeclaration
    {
        public const string Name = "ClassPropertyDeclaration";

        public class Params
        {
            public const string Property = "property";
        }
    }

    public class InterfaceMethodDeclaration
    {
        public const string Name = "InterfaceMethodDeclaration";

        public class Params
        {
            public const string Method = "method";
        }
    }

    public class InterfacePropertyDeclaration
    {
        public const string Name = "InterfacePropertyDeclaration";

        public class Params
        {
            public const string Property = "property";
        }
    }

    public class ServiceStartupRegistrationCall
    {
        public const string Name = "ServiceStartupRegistrationCall";

        public class Params
        {
            public const string ServiceRegistrationInfo = "serviceRegistrationInfo";
        }
    }

    public class FieldDeclaration
    {
        public const string Name = "FieldDeclaration";

        public class Params
        {
            public const string Field = "field";
        }
    }

    public class FixedParameter
    {
        public const string Name = "FixedParameter";

        public class Params
        {
            public const string FixedParam = "fixedParam";
        }
    }

    public class SimpleAssignmentStatement
    {
        public const string Name = "SimpleAssignmentStatement";

        public class Params
        {
            public const string SimpleAssignment = "simpleAssignment";
        }
    }

    public class ConstructorDeclaration
    {
        public const string Name = "ConstructorDeclaration";

        public class Params
        {
            public const string Constructor = "constructor";
        }
    }
}
