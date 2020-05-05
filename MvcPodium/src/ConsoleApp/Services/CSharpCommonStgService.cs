using System.Collections.Generic;
using System.IO;
using Antlr4.StringTemplate;
using Microsoft.Extensions.Options;
using MvcPodium.ConsoleApp.Constants.StringTemplateGroups;
using MvcPodium.ConsoleApp.Models.Config;
using MvcPodium.ConsoleApp.Models.CSharpCommon;
using StgCSharpCommon = MvcPodium.ConsoleApp.Constants.StringTemplateGroups.CSharpCommon;

namespace MvcPodium.ConsoleApp.Services
{
    public class CSharpCommonStgService : ICSharpCommonStgService
    {
        private readonly TemplateGroupFile _cSharpCommonGroupFile;
        private readonly IOptions<AppSettings> _appSettings;


        public CSharpCommonStgService(
            IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings;
            _cSharpCommonGroupFile = new TemplateGroupFile(
                Path.Combine(
                    _appSettings.Value.AssemblyDirectory,
                    _appSettings.Value.StringTemplatesDirectory,
                    StgFileNames.CSharpCommon
                )
            );
        }


        public string RenderClassDeclaration(ClassInterfaceDeclaration classDeclaration = null)
        {
            var stringTemplate = _cSharpCommonGroupFile.GetInstanceOf(StgCSharpCommon.ClassDeclaration.Name);
            stringTemplate.Add(StgCSharpCommon.ClassDeclaration.Params.Attributes, classDeclaration.Attributes);
            stringTemplate.Add(StgCSharpCommon.ClassDeclaration.Params.Modifiers, classDeclaration.Modifiers);
            stringTemplate.Add(StgCSharpCommon.ClassDeclaration.Params.Identifier, classDeclaration.Identifier);
            stringTemplate.Add(StgCSharpCommon.ClassDeclaration.Params.TypeParameters, classDeclaration.TypeParameters);
            stringTemplate.Add(StgCSharpCommon.ClassDeclaration.Params.Base, classDeclaration.Base);
            stringTemplate.Add(StgCSharpCommon.ClassDeclaration.Params.Body, classDeclaration.Body);
            return stringTemplate.Render();
        }


        public string RenderClassMethodDeclaration(MethodDeclaration method = null)
        {
            var stringTemplate = _cSharpCommonGroupFile.GetInstanceOf(StgCSharpCommon.ClassMethodDeclaration.Name);
            stringTemplate.Add(StgCSharpCommon.ClassMethodDeclaration.Params.Attributes, method.Attributes);
            stringTemplate.Add(StgCSharpCommon.ClassMethodDeclaration.Params.Modifiers, method.Modifiers);
            stringTemplate.Add(StgCSharpCommon.ClassMethodDeclaration.Params.ReturnType, method.ReturnType);
            stringTemplate.Add(StgCSharpCommon.ClassMethodDeclaration.Params.Identifier, method.Identifier);
            stringTemplate.Add(StgCSharpCommon.ClassMethodDeclaration.Params.TypeParameters, method.TypeParameters);
            stringTemplate.Add(
                StgCSharpCommon.ClassMethodDeclaration.Params.FormalParameterList, method.FormalParameterList);
            stringTemplate.Add(StgCSharpCommon.ClassMethodDeclaration.Params.Body, method.Body);
            return stringTemplate.Render();
        }


        public string RenderClassPropertyDeclaration(PropertyDeclaration property = null)
        {
            var stringTemplate = _cSharpCommonGroupFile.GetInstanceOf(StgCSharpCommon.ClassPropertyDeclaration.Name);
            stringTemplate.Add(StgCSharpCommon.ClassPropertyDeclaration.Params.Attributes, property.Attributes);
            stringTemplate.Add(StgCSharpCommon.ClassPropertyDeclaration.Params.Modifiers, property.Modifiers);
            stringTemplate.Add(StgCSharpCommon.ClassPropertyDeclaration.Params.Type, property.Type);
            stringTemplate.Add(StgCSharpCommon.ClassPropertyDeclaration.Params.Identifier, property.Identifier);
            stringTemplate.Add(StgCSharpCommon.ClassPropertyDeclaration.Params.Body, property.Body);
            return stringTemplate.Render();
        }


        public string RenderInterfaceDeclaration(ClassInterfaceDeclaration interfaceDeclaration = null)
        {
            var stringTemplate = _cSharpCommonGroupFile.GetInstanceOf(StgCSharpCommon.InterfaceDeclaration.Name);
            stringTemplate.Add(StgCSharpCommon.InterfaceDeclaration.Params.Attributes, interfaceDeclaration.Attributes);
            stringTemplate.Add(StgCSharpCommon.InterfaceDeclaration.Params.Modifiers, interfaceDeclaration.Modifiers);
            stringTemplate.Add(StgCSharpCommon.InterfaceDeclaration.Params.Identifier, interfaceDeclaration.Identifier);
            stringTemplate.Add(
                StgCSharpCommon.InterfaceDeclaration.Params.TypeParameters, interfaceDeclaration.TypeParameters);
            stringTemplate.Add(StgCSharpCommon.InterfaceDeclaration.Params.Base, interfaceDeclaration.Base);
            stringTemplate.Add(StgCSharpCommon.InterfaceDeclaration.Params.Body, interfaceDeclaration.Body);
            return stringTemplate.Render();
        }


        public string RenderInterfaceMethodDeclaration(MethodDeclaration method = null)
        {
            var stringTemplate = _cSharpCommonGroupFile.GetInstanceOf(StgCSharpCommon.InterfaceMethodDeclaration.Name);
            stringTemplate.Add(StgCSharpCommon.InterfaceMethodDeclaration.Params.Attributes, method.Attributes);
            stringTemplate.Add(StgCSharpCommon.InterfaceMethodDeclaration.Params.Modifiers, method.Modifiers);
            stringTemplate.Add(StgCSharpCommon.InterfaceMethodDeclaration.Params.ReturnType, method.ReturnType);
            stringTemplate.Add(StgCSharpCommon.InterfaceMethodDeclaration.Params.Identifier, method.Identifier);
            stringTemplate.Add(StgCSharpCommon.InterfaceMethodDeclaration.Params.TypeParameters, method.TypeParameters);
            stringTemplate.Add(
                StgCSharpCommon.InterfaceMethodDeclaration.Params.FormalParameterList, method.FormalParameterList);
            return stringTemplate.Render();
        }


        public string RenderInterfacePropertyDeclaration(PropertyDeclaration property = null)
        {
            var stringTemplate = _cSharpCommonGroupFile.GetInstanceOf(
                StgCSharpCommon.InterfacePropertyDeclaration.Name);
            stringTemplate.Add(StgCSharpCommon.InterfacePropertyDeclaration.Params.Attributes, property.Attributes);
            stringTemplate.Add(StgCSharpCommon.InterfacePropertyDeclaration.Params.Modifiers, property.Modifiers);
            stringTemplate.Add(StgCSharpCommon.InterfacePropertyDeclaration.Params.Type, property.Type);
            stringTemplate.Add(StgCSharpCommon.InterfacePropertyDeclaration.Params.Identifier, property.Identifier);
            stringTemplate.Add(StgCSharpCommon.InterfacePropertyDeclaration.Params.Body, property.Body);
            return stringTemplate.Render();
        }


        public string RenderFieldDeclaration(FieldDeclaration field = null)
        {
            var stringTemplate = _cSharpCommonGroupFile.GetInstanceOf(StgCSharpCommon.FieldDeclaration.Name);
            stringTemplate.Add(StgCSharpCommon.FieldDeclaration.Params.Attributes, field.Attributes);
            stringTemplate.Add(StgCSharpCommon.FieldDeclaration.Params.Modifiers, field.Modifiers);
            stringTemplate.Add(StgCSharpCommon.FieldDeclaration.Params.Type, field.Type);
            stringTemplate.Add(StgCSharpCommon.FieldDeclaration.Params.VariableDeclarators, field.VariableDeclarators);
            return stringTemplate.Render();
        }


        public string RenderTypeParamList(List<TypeParameter> typeParamList = null)
        {
            var stringTemplate = _cSharpCommonGroupFile.GetInstanceOf(StgCSharpCommon.TypeParamList.Name);
            stringTemplate.Add(StgCSharpCommon.TypeParamList.Params.TypeParamList, typeParamList);
            return stringTemplate.Render();
        }


        public string RenderFixedParameter(FixedParameter fixedParam = null)
        {
            var stringTemplate = _cSharpCommonGroupFile.GetInstanceOf(StgCSharpCommon.FixedParameter.Name);
            stringTemplate.Add(StgCSharpCommon.FixedParameter.Params.Attributes, fixedParam.Attributes);
            stringTemplate.Add(StgCSharpCommon.FixedParameter.Params.ParameterModifier, fixedParam.ParameterModifier);
            stringTemplate.Add(StgCSharpCommon.FixedParameter.Params.Type, fixedParam.Type);
            stringTemplate.Add(StgCSharpCommon.FixedParameter.Params.Identifier, fixedParam.Identifier);
            stringTemplate.Add(StgCSharpCommon.FixedParameter.Params.DefaultArgument, fixedParam.DefaultArgument);
            return stringTemplate.Render();
        }

        public string RenderSimpleAssignment(SimpleAssignment simpleAssignment = null)
        {
            var stringTemplate = _cSharpCommonGroupFile.GetInstanceOf(StgCSharpCommon.SimpleAssignmentStatement.Name);
            stringTemplate.Add(
                StgCSharpCommon.SimpleAssignmentStatement.Params.LeftHandSide, simpleAssignment.LeftHandSide);
            stringTemplate.Add(
                StgCSharpCommon.SimpleAssignmentStatement.Params.RightHandSide, simpleAssignment.RightHandSide);
            return stringTemplate.Render();
        }

        public string RenderConstructorDeclaration(ConstructorDeclaration constructor = null)
        {
            var stringTemplate = _cSharpCommonGroupFile.GetInstanceOf(StgCSharpCommon.ConstructorDeclaration.Name);
            stringTemplate.Add(StgCSharpCommon.ConstructorDeclaration.Params.Attributes, constructor.Attributes);
            stringTemplate.Add(StgCSharpCommon.ConstructorDeclaration.Params.Modifiers, constructor.Modifiers);
            stringTemplate.Add(StgCSharpCommon.ConstructorDeclaration.Params.Identifier, constructor.Identifier);
            stringTemplate.Add(
                StgCSharpCommon.ConstructorDeclaration.Params.FormalParameterList, constructor.FormalParameterList);
            stringTemplate.Add(
                StgCSharpCommon.ConstructorDeclaration.Params.ConstructorInitializer, 
                constructor.ConstructorInitializer);
            stringTemplate.Add(StgCSharpCommon.ConstructorDeclaration.Params.Body, constructor.Body);
            return stringTemplate.Render();
        }

        public string RenderUsingDirectives(List<string> usingDirectives = null)
        {
            var stringTemplate = _cSharpCommonGroupFile.GetInstanceOf(StgCSharpCommon.UsingDirectives.Name);
            stringTemplate.Add(StgCSharpCommon.UsingDirectives.Params.UsingDirectives, usingDirectives);
            return stringTemplate.Render();
        }

        public string RenderUsingDirective(string usingDirective = null)
        {
            var stringTemplate = _cSharpCommonGroupFile.GetInstanceOf(StgCSharpCommon.UsingDirective.Name);
            stringTemplate.Add(StgCSharpCommon.UsingDirective.Params.UsingDirective, usingDirective);
            return stringTemplate.Render();
        }
    }
}
