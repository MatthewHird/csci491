using System;
using System.Collections.Generic;
using System.Text;

namespace MvcPodium.ConsoleApp.Model
{
    public class ClassInterfaceBody
    {
        public List<MethodDeclaration> MethodDeclarations { get; set; } = new List<MethodDeclaration>();

        public List<PropertyDeclaration> PropertyDeclarations { get; set; } = new List<PropertyDeclaration>();
    }


    // class_declaration
    //  : attributes? class_modifier* PARTIAL? CLASS identifier type_parameter_list?
    //    class_base? type_parameter_constraints_clauses? class_body SEMICOLON?
    // interface_declaration
    //  : attributes? interface_modifier* PARTIAL? INTERFACE identifier variant_type_parameter_list?
    //    interface_base? type_parameter_constraints_clauses? interface_body SEMICOLON?
    public class ClassInterfaceDeclaration
    {
        // Attributes [attributes?]
        // Modifiers (ClassModifiers) MemberModifiers [class_modifier*] IsPartial
        // Identifier [identifier]
        // TypeParameterList (TypeParameterList -> TypeParam) [type_parameter_list?]
        // BaseTypes (ClassBase) [class_base? -> class_type (COMMA interface_type_list)?]
        // TypeParameterConstraintsClauses (VariantTypeParameterList -> Constraints) [type_parameter_constraints_clauses?]

        public bool IsInterface { get; set; } = false;
        // Attributes [attributes?]
        public string Attributes { get; set; } = "";

        // Modifiers [class_modifier*] [interface_modifier*] IsPartial
        public List<string> Modifiers { get; set; } = new List<string>();

        // Identifier (Identifier) [identifier]
        public string Identifier { get; set; }

        // **class** TypeParameterList (TypeParameterList -> TypeParam) [type_parameter_list?]
        // **interface**TypeParameterList (TypeParameterList -> TypeParam) [varaint_type_parameter_list?]
        // TypeParameterConstraintsClauses (TypeParameterList -> Constraints) [type_parameter_constraints_clauses?]
        public List<TypeParameter> TypeParameters { get; set; } = new List<TypeParameter>();

        // **class** [class_base?]
        // **interface** [interface_base?]
        public ClassInterfaceBase Base { get; set; }

        // **class** [class_body]
        // **interface** [interface_body]
        public ClassInterfaceBody Body { get; set; } = new ClassInterfaceBody();

        // Body (ClassBody) [class_body] ClassMemberDeclarations
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

        // Body (InterfaceBody) [interface_body] InterfaceMemberDeclarations
        //  interface_method_declaration
        //  interface_property_declaration

        //  interface_event_declaration
        //  interface_indexer_declaration
    }

    public class MethodDeclaration
    {
        // interface_method_declaration
        //  : attributes? NEW? return_type identifier type_parameter_list
        //    OPEN_PARENS formal_parameter_list? CLOSE_PARENS type_parameter_constraints_clauses? SEMICOLON

        // method_declaration: method_header method_body ;

        // method_header
        //  : attributes? method_modifier* PARTIAL? return_type member_name type_parameter_list?
        //    OPEN_PARENS formal_parameter_list? CLOSE_PARENS type_parameter_constraints_clauses?



        // Attributes [attributes?]
        public string Attributes { get; set; } = "";

        // Modifiers (MethodModifiers) IsNew [method_modifier] IsPartial
        public List<string> Modifiers { get; set; } = new List<string>();
        
        // ReturnType [return_type]
        public string ReturnType { get; set; }
        
        // Identifier **Interface** (Identifier) [identifier]
        // Identifier **Class** (MemberName/Identifier) [member_name -> interface_type DOT identifier]
        public string Identifier { get; set; }

        // TypeParameterList (TypeParameterList -> TypeParam) [type_parameter_list?]
        // TypeParameterConstraintsClauses (TypeParameterList -> Constraints) [type_parameter_constraints_clauses?]
        public List<TypeParameter> TypeParameters { get; set; } = new List<TypeParameter>();

        // FormalParameterList (FormalParameterList) [formal_parameter_list?]
        public FormalParameterList FormalParameterList { get; set; }

        // method_body (string)
        public string MethodBody { get; set; }

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

    public class ClassInterfaceBase
    {
        public string ClassType { get; set; }
        public List<string> InterfaceTypeList { get; set; } = new List<string>();
    }

    // formal_parameter_list: fixed_parameters(COMMA parameter_array)? | parameter_array
    public class FormalParameterList
    {
        public List<FixedParameter> FixedParameters { get; set; } = new List<FixedParameter>();

        // parameter_array: attributes? PARAMS array_type identifier ;
        public ParameterArray ParameterArray { get; set; }
    }

    // fixed_parameter: attributes? parameter_modifier? type_ identifier default_argument? ;
    public class FixedParameter
    {
        public string Attributes { get; set; }
        
        public string ParameterModifier { get; set; }
        
        public string Type { get; set; }
        
        public string Identifier { get; set; }
        
        // default_argument?->expression
        public string DefaultArgument { get; set; }
    }

    //parameter_array: attributes? PARAMS array_type identifier;
    public class ParameterArray
    {
        public string Attributes { get; set; }
        
        // array_type
        public string Type { get; set; }
        
        public string Identifier { get; set; }
    }

    public class PropertyDeclaration
    {
        // interface_property_declaration: attributes? NEW? type_ identifier OPEN_BRACE interface_accessors CLOSE_BRACE ;

        // property_declaration: attributes? property_modifier* type_ member_name property_body ;

        // [attributes?]
        public string Attributes { get; set; }

        // **INTERFACE** [NEW?]
        // **Class** [property_modifier*]
        public List<string> Modifiers { get; set; } = new List<string>();
        
        // Type [type_]
        public string Type { get; set; }

        // Identifier **Interface** [identifier]
        // Identifier **Class** [member_name]
        public string Identifier { get; set; }
        
        public string PropertyBody { get; set; }
    }


    public class ConstructorDeclaration
    {
        // constructor_declaration: attributes? constructor_modifier* constructor_declarator constructor_body ;

        // constructor_declarator: identifier OPEN_PARENS formal_parameter_list? CLOSE_PARENS constructor_initializer? ;

        // constructor_initializer: COLON (BASE | THIS) OPEN_PARENS argument_list? CLOSE_PARENS ;

        // argument_list: argument (COMMA argument)* ;

        // constructor_body: block | SEMICOLON ;
    }



    public class TypeParameter
    {
        public string TypeParam { get; set; }
        public string VarianceAnnotation { get; set; }
        public List<string> Constraints { get; set; } = new List<string>();
    }

}
