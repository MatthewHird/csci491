using System;
using System.Collections.Generic;
using System.Linq;
using MvcPodium.ConsoleApp.Model;
using MvcPodium.ConsoleApp.Constants.CSharpGrammar;

namespace MvcPodium.ConsoleApp.Services
{
    public class ServiceCommandService : IServiceCommandService
    {
        private readonly IStringUtilService _stringUtilService;
        private readonly IServiceCommandStService _stringTemplateService;

        public ServiceCommandService(
            IStringUtilService stringUtilService,
            IServiceCommandStService stringTemplateService)
        {
            _stringUtilService = stringUtilService;
            _stringTemplateService = stringTemplateService;
        }

        public (ServiceCommandScraperResults classMissingResults, ServiceCommandScraperResults interfaceMissingResults)
            CompareScraperResults(
                ServiceCommandScraperResults classResults,
                ServiceCommandScraperResults interfaceResults)
        {
            var classBody = classResults.ClassInterfaceDeclaration.Body;
            var interfaceBody = interfaceResults.ClassInterfaceDeclaration.Body;

            var classMissingResults = new ServiceCommandScraperResults()
            {
                UsingDirectives = GetMissingStrings(
                    classResults.UsingDirectives, interfaceResults.UsingDirectives).ToList(),
                ClassInterfaceDeclaration = classResults.ClassInterfaceDeclaration.CopyHeader()
            };

            classMissingResults.ClassInterfaceDeclaration.Body.MethodDeclarations =
                GetMissingMethodDeclarations(classBody.MethodDeclarations, interfaceBody.MethodDeclarations);

            classMissingResults.ClassInterfaceDeclaration.Body.PropertyDeclarations =
                GetMissingPropertyDeclarations(classBody.PropertyDeclarations, interfaceBody.PropertyDeclarations);

            var interfaceMissingResults = new ServiceCommandScraperResults()
            {
                UsingDirectives = 
                    GetMissingStrings(interfaceResults.UsingDirectives, classResults.UsingDirectives).ToList(),
                ClassInterfaceDeclaration = interfaceResults.ClassInterfaceDeclaration.CopyHeader()
            };

            interfaceMissingResults.ClassInterfaceDeclaration.Body.MethodDeclarations =
                GetMissingMethodDeclarations(interfaceBody.MethodDeclarations, classBody.MethodDeclarations);

            interfaceMissingResults.ClassInterfaceDeclaration.Body.PropertyDeclarations =
                GetMissingPropertyDeclarations(interfaceBody.PropertyDeclarations, classBody.PropertyDeclarations);

            return (classMissingResults, interfaceMissingResults);
        }


        private TCollection GetCollectionOfInSet<TCollection, TType>(
            TCollection items,
            HashSet<TType> matchSet) where TCollection : ICollection<TType>, new()
        {
            if (EqualityComparer<TCollection>.Default.Equals(items, default)) { return default; }
            
            var matching = new TCollection();
            foreach (var item in items)
            {
                if (matchSet.Contains(item))
                {
                    matching.Add(item.Copy());
                }
            }
            return matching;
        }


        public ClassInterfaceDeclaration GetInterfaceFromClass(
            ClassInterfaceDeclaration classDeclaration,
            string interfaceIdentifier)
        {
            var interfaceDeclaration = new ClassInterfaceDeclaration()
            {
                IsInterface = true,
                Modifiers = GetCollectionOfInSet(classDeclaration?.Modifiers, Modifiers.Interface),
                Identifier = interfaceIdentifier,
                TypeParameters = classDeclaration?.TypeParameters?.Copy()
            };

            var methods = classDeclaration?.Body?.MethodDeclarations;
            if (methods != null && methods.Count > 0)
            {
                foreach (var method in methods)
                {
                    var newMethod = new MethodDeclaration()
                    {
                        Modifiers = GetCollectionOfInSet(method?.Modifiers, Modifiers.InterfaceMethod),
                        ReturnType = method.ReturnType.Copy(),
                        Identifier = method.Identifier.Copy(),
                        TypeParameters = method?.TypeParameters?.Copy(),
                        FormalParameterList = method?.FormalParameterList?.Copy()
                    };
                    interfaceDeclaration.Body.MethodDeclarations.Add(newMethod);
                }
            }

            var properties = classDeclaration?.Body?.PropertyDeclarations;
            if (properties != null && properties.Count > 0)
            {
                foreach (var property in properties)
                {
                    var newProperty = new PropertyDeclaration()
                    {
                        Modifiers = GetCollectionOfInSet(property?.Modifiers, Modifiers.InterfaceProperty),
                        Type = property.Type.Copy(),
                        Identifier = property.Identifier.Copy(),
                        Body = property?.Body?.Copy()
                    };
                    interfaceDeclaration.Body.PropertyDeclarations.Add(newProperty);
                }
            }

            return interfaceDeclaration;
        }


        public ClassInterfaceDeclaration GetClassFromInterface(
            ClassInterfaceDeclaration interfaceDeclaration, 
            string classIdentifier)
        {
            var classDeclaration = new ClassInterfaceDeclaration()
            {
                IsInterface = false,
                Modifiers = GetCollectionOfInSet(interfaceDeclaration?.Modifiers, Modifiers.Class),
                Identifier = classIdentifier,
                TypeParameters = interfaceDeclaration?.TypeParameters?.Copy(),
                Base = new ClassInterfaceBase()
            };
            classDeclaration.Base.InterfaceTypeList.Add(
                interfaceDeclaration.Identifier
                    + _stringTemplateService.RenderTypeParamList(classDeclaration.TypeParameters)
            );
            foreach (var typeParam in classDeclaration.TypeParameters)
            {
                typeParam.VarianceAnnotation = null;
            }

            var methods = interfaceDeclaration?.Body?.MethodDeclarations;
            if (methods != null && methods.Count > 0)
            {
                foreach (var method in methods)
                {
                    var newMethod = new MethodDeclaration()
                    {
                        Modifiers = GetCollectionOfInSet(method?.Modifiers, Modifiers.Method) ?? new List<string>(),
                        ReturnType = method.ReturnType.Copy(),
                        Identifier = method.Identifier.Copy(),
                        TypeParameters = method?.TypeParameters?.Copy(),
                        FormalParameterList = method?.FormalParameterList?.Copy()
                    };
                    if (!newMethod.Modifiers.Contains(Keywords.Public))
                    {
                        newMethod.Modifiers.Insert(0, Keywords.Public);
                    }
                    classDeclaration.Body.MethodDeclarations.Add(newMethod);
                }
            }

            var properties = interfaceDeclaration?.Body?.PropertyDeclarations;
            if (properties != null && properties.Count > 0)
            {
                foreach (var property in properties)
                {
                    var newProperty = new PropertyDeclaration()
                    {
                        Modifiers = GetCollectionOfInSet(property?.Modifiers, Modifiers.Property) ?? new List<string>(),
                        Type = property.Type.Copy(),
                        Identifier = property.Identifier.Copy(),
                        Body = property?.Body?.Copy()
                    };
                    newProperty.Modifiers.Add(Keywords.Public);
                    classDeclaration.Body.PropertyDeclarations.Add(newProperty);
                }
            }

            return classDeclaration;
        }


        private HashSet<string> GetMissingStrings(IEnumerable<string> set1, IEnumerable<string> set2)
        {
            var missing = new HashSet<string>(set2);
            missing.ExceptWith(set1);
            return missing;
        }

        private List<MethodDeclaration> GetMissingMethodDeclarations(
            IEnumerable<MethodDeclaration> set1,
            IEnumerable<MethodDeclaration> set2)
        {

            var missing = new List<MethodDeclaration>();
            foreach (var item2 in set2)
            {
                bool isMissing = true;
                foreach (var item1 in set1)
                {
                    if (Equivalent(item1, item2))
                    {
                        isMissing = false;
                        break;
                    }
                }
                if (isMissing)
                {
                    missing.Add(item2.Copy());
                }
            }

            return missing;
        }

        private List<PropertyDeclaration> GetMissingPropertyDeclarations(
            IEnumerable<PropertyDeclaration> set1,
            IEnumerable<PropertyDeclaration> set2)
        {
            var missing = new List<PropertyDeclaration>();
            foreach (var item2 in set2)
            {
                bool isMissing = true;
                foreach (var item1 in set1)
                {
                    if (Equivalent(item1, item2))
                    {
                        isMissing = false;
                        break;
                    }
                }
                if (isMissing)
                {
                    missing.Add(item2.Copy());
                }
            }

            return missing;
        }


        private bool Equivalent(TypeParameter first, TypeParameter second)
        {
            if (first?.TypeParam != second?.TypeParam) { return false; }
            if (first?.Constraints.Count != second?.Constraints.Count) { return false; }

            var firstConstraints = StringEnumerableToMinifiedHashSet(first?.Constraints);
            var secondConstraints = StringEnumerableToMinifiedHashSet(second?.Constraints);

            if (!firstConstraints.SetEquals(secondConstraints)) { return false; }

            return true;
        }

        private bool Equivalent(FixedParameter first, FixedParameter second)
        {
            if (first?.ParameterModifier != second?.ParameterModifier) { return false; }
            if (_stringUtilService.MinifyString(first?.Type) != _stringUtilService.MinifyString(second?.Type))
            {
                return false;
            }
            return true;
        }

        private bool Equivalent(List<FixedParameter> first, List<FixedParameter> second)
        {
            if (first?.Count != second?.Count) { return false; }
            for (int i = 0; i < first?.Count; ++i)
            {
                if (!Equivalent(first[i], second[i])) { return false; }
            }
            return true;
        }

        private bool Equivalent(ParameterArray first, ParameterArray second)
        {
            if (_stringUtilService.MinifyString(first?.Type) != _stringUtilService.MinifyString(second?.Type))
            {
                return false;
            }
            return true;
        }

        private bool Equivalent(MethodDeclaration first, MethodDeclaration second)
        {
            if (_stringUtilService.MinifyString(first?.ReturnType) !=
                _stringUtilService.MinifyString(second?.ReturnType))
            {
                return false;
            }
            if (_stringUtilService.MinifyString(first?.Identifier) !=
                _stringUtilService.MinifyString(second?.Identifier))
            {
                return false;
            }
            if (!Equivalent(first?.TypeParameters, second?.TypeParameters)) { return false; }
            if (!Equivalent(first?.FormalParameterList, second?.FormalParameterList)) { return false; }

            return true;
        }

        private bool Equivalent(List<TypeParameter> first, List<TypeParameter> second)
        {
            if (first?.Count != second?.Count) { return false; }
            for (int i = 0; i < first?.Count; ++i)
            {
                if (!Equivalent(first[i], second[i])) { return false; }
            }
            return true;
        }

        private bool Equivalent(PropertyDeclaration first, PropertyDeclaration second)
        {
            if (_stringUtilService.MinifyString(first?.Type) != _stringUtilService.MinifyString(second?.Type))
            {
                return false;
            }
            if (first?.Identifier != second?.Identifier) { return false; }
            if (!Equivalent(first?.Body, second?.Body)) { return false; }

            return true;
        }

        private bool Equivalent(PropertyBody first, PropertyBody second)
        {
            if (first?.HasGetAccessor != second?.HasGetAccessor) { return false; }
            if (first?.HasSetAccessor != second?.HasSetAccessor) { return false; }
            return true;
        }

        private bool Equivalent(FormalParameterList first, FormalParameterList second)
        {
            if (!Equivalent(first?.ParameterArray, second?.ParameterArray)) { return false; }
            if (!Equivalent(first?.FixedParameters, second?.FixedParameters)) { return false; }
            return true;
        }

        private HashSet<string> StringEnumerableToMinifiedHashSet(IEnumerable<string> stringEnumerable)
        {
            var hashSet = new HashSet<string>();

            foreach (var constraint in stringEnumerable)
            {
                hashSet.Add(_stringUtilService.MinifyString(constraint));
            }
            return hashSet;
        }
    }
}
