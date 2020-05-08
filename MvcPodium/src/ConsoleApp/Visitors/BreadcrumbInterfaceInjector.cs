using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using MvcPodium.ConsoleApp.Constants.CSharpGrammar;
using MvcPodium.ConsoleApp.Models.BreadcrumbCommand;
using MvcPodium.ConsoleApp.Services;

namespace MvcPodium.ConsoleApp.Visitors
{
    public class BreadcrumbInterfaceInjector : CSharpParserBaseVisitor<object>
    {
        private readonly Stack<string> _currentNamespace;
        private readonly Stack<string> _currentClass;

        private readonly Stack<bool> _isControllerClass;

        public BufferedTokenStream Tokens { get; }
        public ControllerDictionary Results { get; set; }

        private readonly string _tabString;

        public BreadcrumbInterfaceInjector(
            BufferedTokenStream tokenStream,
            string tabString)
        {
            Tokens = tokenStream;
            _tabString = tabString;
            _currentNamespace = new Stack<string>();
            _currentClass = new Stack<string>();
            _isControllerClass = new Stack<bool>();
            _isControllerClass.Push(false);
            Results = new ControllerDictionary();
        }

        public override object VisitNamespace_declaration([NotNull] CSharpParser.Namespace_declarationContext context)
        {
            _currentNamespace.Push(context.qualified_identifier().GetText());
            VisitChildren(context);
            _ = _currentNamespace.Pop();
            return null;
        }

        public override object VisitClass_declaration([NotNull] CSharpParser.Class_declarationContext context)
        {
            _currentClass.Push(context.identifier().GetText());

            var classBaseType = context?.class_base()?.class_type()?.GetText();

            _isControllerClass.Push(classBaseType == "Controller");

            VisitChildren(context);

            _ = _isControllerClass.Pop();
            _ = _currentClass.Pop();
            return null;
        }

        public override object VisitMethod_header([NotNull] CSharpParser.Method_headerContext context)
        {
            if (_isControllerClass.Peek())
            {
                bool isNonAction = false;
                bool isPublic = false;

                var attributes = context?.attributes()?.attribute_section();
                if (attributes != null)
                {
                    foreach (var attributeSection in attributes)
                    {
                        foreach (var attribute in attributeSection.attribute_list().attribute())
                        {
                            if (attribute.GetText() == "NonAction")
                            {
                                isNonAction = true;
                            }
                        }
                    }
                }

                var modifiers = context?.method_modifier();
                if (modifiers != null)
                {
                    foreach (var modifier in modifiers)
                    {
                        if (modifier.GetText() == Keywords.Public)
                        {
                            isPublic = true;
                        }
                    }
                }

                if (!isNonAction && isPublic)
                {
                    // Add endpoint info to results
                    var currentNamespace = GetCurrentNamespace();
                    var currentClass = GetCurrentClass();
                    var actionName = context.member_name().identifier().GetText();
                    bool? hasId = false;

                    var fixedParams = context?.formal_parameter_list()?.fixed_parameters()?.fixed_parameter();
                    if (fixedParams != null)
                    {
                        foreach (var fixedParam in fixedParams)
                        {
                            if (Regex.Match(fixedParam?.type_()?.GetText(), @"^int\??$").Success
                                && fixedParam?.identifier()?.GetText() == "id")
                            {
                                hasId = true;
                            }
                        }
                    }
                    
                    if (!Results.NamespaceDict.ContainsKey(currentNamespace))
                    {
                        Results.AddControllerNamespace(currentNamespace);
                    }
                    if (!Results.NamespaceDict[currentNamespace].ClassDict.ContainsKey(currentClass))
                    {
                        Results.NamespaceDict[currentNamespace].AddControllerClass(
                            currentClass, GetControllerRootName(currentClass));
                    }
                    if (!Results.NamespaceDict[currentNamespace]
                            .ClassDict[currentClass]
                            .ActionDict
                            .ContainsKey(actionName))
                    {
                        Results.NamespaceDict[currentNamespace].ClassDict[currentClass].AddControllerAction(
                            actionName,
                            hasId);
                    }
                    else
                    {
                        Results.NamespaceDict[currentNamespace]
                            .ClassDict[currentClass]
                            .ActionDict[actionName]
                            .HasId |= hasId;
                    }
                }

            }

            VisitChildren(context);

            return null;
        }

        private string GetCurrentClass()
        {
            return string.Join(".", _currentClass.ToArray().Reverse());
        }

        private string GetCurrentNamespace()
        {
            return string.Join(".", _currentNamespace.ToArray().Reverse());
        }

        private string GetControllerRootName(string controllerClassName)
        {
            return controllerClassName.EndsWith("Controller")
                ? controllerClassName.Substring(0, controllerClassName.Length - "Controller".Length) 
                : controllerClassName;
        }
    }
}
