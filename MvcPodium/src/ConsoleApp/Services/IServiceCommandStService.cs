using System;
using System.Collections.Generic;
using System.Text;
using MvcPodium.ConsoleApp.Model;

namespace MvcPodium.ConsoleApp.Services
{
    public interface IServiceCommandStService
    {
        string RenderServiceFile(
            string serviceNamespace = null,
            List<string> usingDirectives = null,
            ClassInterfaceDeclaration service = null);

        string RenderTypeParamList(List<TypeParameter> typeParamList = null);

        string RenderClassMethodDeclaration(MethodDeclaration method = null);

        string RenderClassPropertyDeclaration(PropertyDeclaration property = null);

        string RenderInterfaceMethodDeclaration(MethodDeclaration method = null);

        string RenderInterfacePropertyDeclaration(PropertyDeclaration property = null);

        string RenderServiceStartupRegistrationCall(ServiceRegistrationInfo serviceRegistrationInfo = null);

        string RenderFieldDeclaration(FieldDeclaration field = null);

        string RenderFixedParameter(FixedParameter fixedParam = null);

        string RenderSimpleAssignment(SimpleAssignment simpleAssignment = null);
        
        string RenderConstructorDeclaration(ConstructorDeclaration constructor = null);

    }
}
