using System.Collections.Generic;
using MvcPodium.ConsoleApp.Models;
using MvcPodium.ConsoleApp.Models.CSharpCommon;

namespace MvcPodium.ConsoleApp.Services
{
    public interface ICSharpCommonStgService
    {
        string RenderClassDeclaration(ClassInterfaceDeclaration classDeclaration = null);

        string RenderClassMethodDeclaration(MethodDeclaration method = null);

        string RenderClassPropertyDeclaration(PropertyDeclaration property = null);

        string RenderInterfaceDeclaration(ClassInterfaceDeclaration interfaceDeclaration = null);

        string RenderInterfaceMethodDeclaration(MethodDeclaration method = null);

        string RenderInterfacePropertyDeclaration(PropertyDeclaration property = null);

        string RenderFieldDeclaration(FieldDeclaration field = null);

        string RenderTypeParamList(List<TypeParameter> typeParamList = null);

        string RenderFixedParameter(FixedParameter fixedParam = null);

        string RenderSimpleAssignment(SimpleAssignment simpleAssignment = null);

        string RenderConstructorDeclaration(ConstructorDeclaration constructor = null);

        string RenderUsingDirectives(List<string> usingDirectives = null);
        
        string RenderUsingDirective(string usingDirective = null);
    }
}
