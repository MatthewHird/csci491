using System;
using System.Collections.Generic;
using System.Text;

namespace MvcPodium.ConsoleApp.Model
{
    public interface ITypeDeclaration
    {
        //string Identifier { get; set; }
    }


    public class ClassDeclaration : ITypeDeclaration
    {
        // Attributes [attributes]
        // Modifiers (ClassModifiers) MemberModifiers [all_member_modifiers] IsPartial
        // Identifier [identifier]
        // TypeParameterList (TypeParameterList) [type_parameter_list]
        // BaseTypes (ClassBase) [class_base -> class_type (COMMA namespace_or_type_name)*]
        // TypeParameterConstraintsClause (VariantTypeParameterList -> Constraints) [type_parameter_constraints_clause]

        // Body (ClassBody) ClassMemberDeclarations
        //  method_declaration
        //  property_declaration

        //  constructor_declaration

        //  field_declaration
        //  constant_declaration
        //  event_declaration
        //  indexer_declaration
        //  operator_declaration
        //  destructor_declaration
        //  static_constructor_declaration
        //  type_declaration
    }

    public class InterfaceDeclaration : ITypeDeclaration
    {
        // Attributes [attributes]
        // Modifiers (InterfaceModifiers) [all_member_modifiers] IsPartial
        // Identifier [identifier]
        // TypeParameterList (TypeParameterList) [type_parameter_list]
        // BaseTypes (InterfaceBase) [interface_base -> interface_tytpe_list -> namespace_or_type_name (COMMA namespace_or_type_name)*]
        // TypeParameterConstraintsClause (VariantTypeParameterList -> Constraints) [type_parameter_constraints_clause]

        // Body (InterfaceBody) InterfaceMemberDeclarations
        //  interface_method_declaration
        //  interface_property_declaration
        //  interface_event_declaration
        //  interface_indexer_declaration
    }

    public class MethodDeclaration
    {
        // [common_member_declaration -> VOID method_declaration]
        // [common_member_declaration -> typed_member_declaration -> type_ method_declaration]

        // Attributes attributes
        // Modifiers (MethodModifiers) [all_member_modifiers] IsPartial
        // ReturnType [typed_member_declaration -> type_]
        // Identifier **Interface** (Identifier) [identifier]
        // Identifier **Class** (MemberName/Identifier) [method_member_name -> identifier(LAST)]
        // TypeParameterList (TypeParameterList -> TypeParam) [type_parameter_list]
        // FormalParameterList (FormalParameterList) [formal_parameter_list]
        // TypeParameterConstraintsClause (VariantTypeParameterList -> Constraints) [type_parameter_constraints_clause]

        public string Attributes { get; set; }
        public List<string> Modifiers { get; set; }
        public string ReturnType { get; set; }
        public string Identifier { get; set; }
        public List<TypeParameter> TypeParameters { get; set; }
        public string FormalParameterList { get; set; }

        public string Body { get; set; }

        public List<string> GetTypeParameterList()
        {
            var list = new List<string>();
            foreach (var typeParameter in TypeParameters)
            {
                list.Add(typeParameter.TypeParam);
            }
            return list;
        }

    }

    public class PropertyDeclaration
    {
        // [common_member_declaration -> typed_member_declaration -> type_ property_declaration]

        // member_name (OPEN_BRACE accessor_declarations CLOSE_BRACE (ASSIGNMENT variable_initializer SEMICOLON)? | OP_ARROW expression SEMICOLON)
        // member_name -> namespace_or_type_name
    }

    public class ConstructorDeclaration
    {
        // [common_member_declaration -> constructor_declaration]
        //: identifier OPEN_PARENS formal_parameter_list? CLOSE_PARENS constructor_initializer? body



    }



    public class TypeParameter
    {
        public string TypeParam { get; set; }
        public List<string> Constraints { get; set; } = new List<string>();
    }

    public class VariantTypeParameter : TypeParameter
    {
        public Variance Variance { get; set; }
    }

    public enum Variance
    {
        None,
        In,
        Out
    }
}
