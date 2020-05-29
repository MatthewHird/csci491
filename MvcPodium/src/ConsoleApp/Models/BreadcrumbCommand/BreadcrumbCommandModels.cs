using System;
using System.Collections.Generic;
using System.Text;
using MvcPodium.ConsoleApp.Models.CSharpCommon;

namespace MvcPodium.ConsoleApp.Models.BreadcrumbCommand
{
    public class ControllerDictionary
    {
        public Dictionary<string, ControllerNamespace> NamespaceDict { get; set; } =
            new Dictionary<string, ControllerNamespace>();

        public void  AddControllerNamespace(string namespace_)
        {
            NamespaceDict.Add(
                namespace_,
                new ControllerNamespace()
                {
                    Namespace = namespace_
                });
        }
    }

    public class ControllerNamespace
    {
        public string Namespace { get; set; }
        public Dictionary<string, ControllerClass> ClassDict { get; set; } =
            new Dictionary<string, ControllerClass>();

        public void AddControllerClass(string controller, string controllerRoot)
        {
            ClassDict.Add(
                controller,
                new ControllerClass()
                {
                    Namespace = Namespace,
                    ControllerRoot = controllerRoot,
                    Controller = controller
                });
        }
    }

    public class ControllerClass
    {
        public string Namespace { get; set; }
        public string ControllerRoot { get; set; }
        public string Controller { get; set; }
        public Dictionary<string, ControllerAction> ActionDict { get; set; } =
            new Dictionary<string, ControllerAction>();

        public void AddControllerAction(string action, bool? hasId)
        {
            ActionDict.Add(
                action,
                new ControllerAction()
                {
                    Namespace = Namespace,
                    ControllerRoot = ControllerRoot,
                    Controller = Controller,
                    Action = action,
                    HasId = hasId
                });
        }
    }

    public class ControllerAction
    {
        public string Namespace { get; set; }
        public string ControllerRoot { get; set; }
        public string Controller { get; set; }
        public string Action { get; set; }
        public bool? HasId { get; set; }

    }

    public class BreadcrumbServiceDeclaration
    {
        public bool IsInterface { get; set; } = false;

        public string Attributes { get; set; }

        public List<string> Modifiers { get; set; } = new List<string>();

        public string Identifier { get; set; }

        public List<TypeParameter> TypeParameters { get; set; } = new List<TypeParameter>();

        public ClassInterfaceBase Base { get; set; }

        public BreadcrumbServiceBody Body { get; set; } = new BreadcrumbServiceBody();
    }

    public class BreadcrumbServiceBody
    {
        public List<FieldDeclaration> FieldDeclarations { get; set; } = new List<FieldDeclaration>();

        public ConstructorDeclaration ConstructorDeclaration { get; set; }

        public List<BreadcrumbMethodDeclaration> MethodDeclarations { get; set; }
            = new List<BreadcrumbMethodDeclaration>();
    }

    public class BreadcrumbMethodDeclaration
    {
        public string ControllerRoot { get; set; }
        public string Action { get; set; }
        public bool? HasId { get; set; }
        public string Controller { get; set; }
        public string ControllerNamePattern { get; set; }
    }
}
