// Eclipse Public License - v 1.0, http://www.eclipse.org/legal/epl-v10.html
// Copyright (c) 2013, Christian Wulf (chwchw@gmx.de)
// Copyright (c) 2016-2017, Ivan Kochurkin (kvanttt@gmail.com), Positive Technologies.

parser grammar CSharpParser;

options { tokenVocab=CSharpLexer; }

// ENTRY POINT
// https://github.com/dotnet/csharplang/blob/master/spec/namespaces.md#compilation-units

compilation_unit
    : BYTE_ORDER_MARK? extern_alias_directive* using_directive*
      global_attributes? namespace_member_declaration* EOF
    ;

// B.2 Syntactic grammar

// B.2.1 Basic concepts
// https://github.com/dotnet/csharplang/blob/master/spec/basic-concepts.md#namespace-and-type-names

namespace_name: namespace_or_type_name ;

type_name: namespace_or_type_name ;

namespace_or_type_name
    : identifier type_argument_list?
    | namespace_or_type_name DOT identifier type_argument_list?
    | qualified_alias_member
    ;

// B.2.2 Types
// https://github.com/dotnet/csharplang/blob/master/spec/types.md

type_: base_type (QUESTION | rank_specifier | STAR)* ;

base_type
    : value_type
    | reference_type
    | type_parameter
    ;

value_type
    : struct_type
    | enum_type
    ;

struct_type
    : type_name
    | simple_type
    | tuple_type
    ;

simple_type 
    : numeric_type
    | BOOL      //| 'System.Boolean'
    ;

numeric_type 
    : integral_type
    | floating_point_type
    | DECIMAL   //| 'System.Decimal'
    ;

integral_type 
    : SBYTE     //| 'System.SByte'
    | BYTE      //| 'System.Byte'
    | SHORT     //| 'System.Int16'
    | USHORT    //| 'System.UInt16'
    | INT       //| 'System.Int32'
    | UINT      //| 'System.UInt32'
    | LONG      //| 'System.Int64'
    | ULONG     //| 'System.UInt64'
    | CHAR      //| 'System.Char'
    ;

floating_point_type 
    : FLOAT     //| 'System.Single'
    | DOUBLE    //| 'System.Double'
    ;

string_type
    : STRING    //| 'System.String'
    ;

tuple_type: OPEN_PARENS tuple_type_element_list CLOSE_PARENS ;
    
tuple_type_element_list: tuple_type_element (COMMA tuple_type_element)+ ;
    
tuple_type_element: type_ identifier? ;

enum_type: type_name ;

reference_type
    : class_type
    | interface_type
    | delegate_type
    ;

class_type
    : type_name
    | OBJECT
    | DYNAMIC
    | string_type
    ;

interface_type: type_name ;

array_type: base_type ((STAR | QUESTION)* rank_specifier)+ ;

non_array_type: type_ ;

rank_specifier: OPEN_BRACKET dim_separator* CLOSE_BRACKET ;

dim_separator: COMMA ;

delegate_type: type_name ;

type_argument_list : OPEN_ANGLE type_ (COMMA type_)* CLOSE_ANGLE ;

// B.2.3 Variables
// https://github.com/dotnet/csharplang/blob/master/spec/variables.md#variable-references

variable_reference: expression ;

// B.2.4 Expressions
// https://github.com/dotnet/csharplang/blob/master/spec/expressions.md

expression
    : non_assignment_expression
    | assignment
    | declaration_expression
    ;

constant_expression: expression ;

boolean_expression: expression ;

argument_list: argument (COMMA argument)* ;

argument: argument_name? argument_value ;

argument_name: identifier COLON ;

argument_value
    : OUT type_ identifier
    | expression
    | REF variable_reference
    | OUT variable_reference
    ;

non_assignment_expression
    : lambda_expression
    | query_expression
    | conditional_expression
    ;

assignment: unary_expression assignment_operator expression ;

assignment_operator
    : ASSIGNMENT
    | OP_ADD_ASSIGNMENT
    | OP_SUB_ASSIGNMENT
    | OP_MULT_ASSIGNMENT
    | OP_DIV_ASSIGNMENT
    | OP_MOD_ASSIGNMENT
    | OP_AND_ASSIGNMENT
    | OP_OR_ASSIGNMENT
    | OP_XOR_ASSIGNMENT
    | OP_LEFT_SHIFT_ASSIGNMENT
    | OP_RIGHT_SHIFT_ASSIGNMENT
    | OP_COALESCING_ASSIGNMENT
    | ASSIGNMENT REF
    ;

conditional_expression: null_coalescing_expression (QUESTION expression COLON expression)? ;

null_coalescing_expression
    : conditional_or_expression (OP_COALESCING null_coalescing_expression)?
    | throw_expression
    ;

throw_expression: THROW null_coalescing_expression ;

conditional_or_expression: conditional_and_expression (OP_OR conditional_and_expression)* ;

conditional_and_expression: inclusive_or_expression (OP_AND inclusive_or_expression)* ;

inclusive_or_expression: exclusive_or_expression (PIPE exclusive_or_expression)* ;

exclusive_or_expression: and_expression (CARET and_expression)* ;

and_expression: equality_expression (AMP equality_expression)* ;

equality_expression: relational_expression ((OP_EQ | OP_NE)  relational_expression)* ;

relational_expression: shift_expression ((OPEN_ANGLE | CLOSE_ANGLE | OP_LE | OP_GE) shift_expression | (IS | AS) type_ | IS pattern)* ;

pattern
    : declaration_pattern
    | constant_pattern
    | var_pattern
    | positional_pattern
    | property_pattern
    | discard_pattern
    ;

declaration_pattern: type_ simple_designation ;

constant_pattern: constant_expression ;

var_pattern: VAR designation ;

designation
    : simple_designation
    | tuple_designation
    ;

positional_pattern: type_? OPEN_PARENS subpatterns? CLOSE_PARENS property_subpattern? simple_designation? ;

property_pattern: type_? property_subpattern simple_designation? ;

property_subpattern: OPEN_BRACE subpatterns? CLOSE_BRACE ;

subpatterns: subpattern (COMMA subpattern)* ;

subpattern: (identifier COLON)? pattern ;

simple_designation
    : single_variable_designation
    | discard_designation
    ;

discard_pattern: discard_designation ;

type_pattern: type_ simple_designation ;

declaration_expression: type_ variable_designation ;

variable_designation
    : single_variable_designation
    | parenthesized_variable_designation
    | discard_designation
    ;

single_variable_designation: identifier ;

parenthesized_variable_designation: OPEN_PARENS variable_designation (COMMA variable_designation)+ CLOSE_PARENS ;

discard_designation: UNDERSCORE ;

tuple_designation: OPEN_PARENS designations? CLOSE_PARENS ;

designations: designation (COMMA designation)* ;

foreach_variable_statement: FOREACH OPEN_PARENS declaration_expression IN expression CLOSE_PARENS embedded_statement ;

shift_expression: additive_expression ((OP_LEFT_SHIFT | OP_RIGHT_SHIFT)  additive_expression)* ;

additive_expression: multiplicative_expression ((PLUS | MINUS)  multiplicative_expression)* ;

multiplicative_expression
    : switch_expression
    | range_expression ((STAR | SLASH | PERCENT) range_expression)*
    ;

switch_expression: range_expression SWITCH OPEN_BRACE (switch_expression_arms COMMA?)? CLOSE_BRACE ;

switch_expression_arms: switch_expression_arm (COMMA switch_expression_arm)* ;

switch_expression_arm: pattern case_guard? OP_ARROW expression ;

range_expression
    : unary_expression
    | OP_RANGE
    | range_expression OP_RANGE
    | OP_RANGE range_expression
    | range_expression OP_RANGE range_expression
    ;

unary_expression
    : primary_expression
    | null_conditional_expression
    | PLUS unary_expression
    | MINUS unary_expression
    | BANG unary_expression
    | TILDE unary_expression
    | pre_increment_expression
    | pre_decrement_expression
    | cast_expression
    | await_expression
    | unary_expression_unsafe
    ;

null_conditional_expression: primary_expression null_conditional_operations ;

null_conditional_operations
    : (QUESTION DOT identifier type_argument_list? | QUESTION OPEN_BRACKET argument_list CLOSE_BRACKET)
       null_conditional_operation*
    ;

null_conditional_operation
    : QUESTION DOT identifier type_argument_list?
    | QUESTION OPEN_BRACKET argument_list CLOSE_BRACKET
    | DOT identifier type_argument_list?
    | OPEN_BRACKET argument_list CLOSE_BRACKET
    | OPEN_PARENS argument_list? CLOSE_PARENS
    ;

null_conditional_member_access
    : primary_expression null_conditional_operations? QUESTION DOT identifier type_argument_list?
    | primary_expression null_conditional_operations DOT identifier type_argument_list?
    ;

null_conditional_invocation_expression: primary_expression null_conditional_operations OPEN_PARENS argument_list? CLOSE_PARENS ;

pre_increment_expression: OP_INC unary_expression ;

pre_decrement_expression: OP_DEC unary_expression ;

cast_expression: OPEN_PARENS type_ CLOSE_PARENS unary_expression ;

await_expression: AWAIT unary_expression ;

primary_expression  // Null-conditional operators C# 6: https://msdn.microsoft.com/en-us/library/dn986595.aspx
    : pe=primary_expression_start bracket_expression*
      ((member_access | invocation_expression | OP_INC | OP_DEC | OP_PTR identifier | BANG) bracket_expression*)*
    | array_creation_expression
    ;

primary_expression_start
    : literal                                                                   #literal_pes
    | interpolated_string_expression                                            #interpolatedStringExpression_pes
    | simple_name                                                               #simpleName_pes
    | parenthesized_expression                                                  #predefinedName_pes
    | predefined_type                                                           #memberAccessExpression
    | qualified_alias_member                                                    #memberAccessExpression
    | LITERAL_ACCESS                                                            #literalAccess_pes
    | this_access                                                               #thisAccess_pes
    | base_access                                                               #baseAccess_pes
    | object_creation_expression                                                #objectCreationExpression_pes
    | typeof_expression                                                         #typeofExpression_pes
    | checked_expression                                                        #checkedExpression_pes
    | unchecked_expression                                                      #uncheckedExpression_pes
    | default_value_expression                                                  #defaultValueExpression_pes
    | anonymous_method_expression                                               #anonymousMethodExpression_pes
    | sizeof_expression                                                         #sizeofExpression_pes
    | nameof_expression                                                         #nameofExpression_pes
    | delegate_creation_expression                                              #delegateCreationExpression_pes
    | anonymous_object_creation_expression                                      #anonymousObjectCreationExpression_pes
    | stackalloc_initializer                                                    #stackallocInitializer_pes
    ;

member_access: QUESTION? DOT identifier type_argument_list? ;

bracket_expression: QUESTION? OPEN_BRACKET expression_list CLOSE_BRACKET ;

indexer_argument: (identifier COLON)? expression ;

nameof_expression: NAMEOF OPEN_PARENS named_entity CLOSE_PARENS ;

parenthesized_expression: OPEN_PARENS expression CLOSE_PARENS ;

sizeof_expression: SIZEOF OPEN_PARENS unmanaged_type CLOSE_PARENS ;

anonymous_method_expression: DELEGATE explicit_anonymous_function_signature? block ;

default_value_expression: DEFAULT (OPEN_PARENS type_ CLOSE_PARENS)? ;

checked_expression: CHECKED OPEN_PARENS expression CLOSE_PARENS ;

unchecked_expression: UNCHECKED OPEN_PARENS expression CLOSE_PARENS ;

typeof_expression: TYPEOF OPEN_PARENS (unbound_type_name | type_ | VOID) CLOSE_PARENS ;

this_access: THIS ;

base_access: BASE (DOT identifier type_argument_list? | OPEN_BRACKET expression_list CLOSE_BRACKET) ;

simple_name: identifier type_argument_list? ;

invocation_expression: OPEN_PARENS argument_list? CLOSE_PARENS ;

post_increment_expression: primary_expression OP_INC ;

post_decrement_expression: primary_expression OP_DEC ;

object_creation_expression
    : NEW type_ OPEN_PARENS argument_list? CLOSE_PARENS object_or_collection_initializer?
    | NEW type_ object_or_collection_initializer
    ;

object_or_collection_initializer
    : object_initializer
    | collection_initializer
    ;

object_initializer: OPEN_BRACE (member_initializer_list COMMA?)? CLOSE_BRACE ;

member_initializer_list: member_initializer (COMMA member_initializer)* ;

member_initializer: initializer_target ASSIGNMENT initializer_value ;

initializer_target
    : identifier
    | OPEN_BRACKET argument_list CLOSE_BRACKET
    ;

initializer_value
    : expression
    | object_or_collection_initializer
    ;

collection_initializer: OPEN_BRACE element_initializer_list COMMA? CLOSE_BRACE ;

element_initializer_list: element_initializer (COMMA element_initializer)* ;

element_initializer
    : non_assignment_expression
    | OPEN_BRACE expression_list CLOSE_BRACE
    ;

expression_list: expression (COMMA expression)* ;

array_creation_expression
    : NEW non_array_type OPEN_BRACKET expression_list CLOSE_BRACKET rank_specifier* array_initializer?
    | NEW array_type array_initializer
    | NEW rank_specifier array_initializer
    ;

delegate_creation_expression: NEW delegate_type OPEN_PARENS expression CLOSE_PARENS ;

anonymous_object_creation_expression: NEW anonymous_object_initializer ;

anonymous_object_initializer: OPEN_BRACE (member_declarator_list COMMA?)? CLOSE_BRACE ;

member_declarator_list: member_declarator (COMMA member_declarator)* ;

member_declarator
    : simple_name
    | member_access
    | base_access
    | null_conditional_member_access
    | identifier ASSIGNMENT expression
    ;

unbound_type_name
    : identifier generic_dimension_specifier?
    | identifier DOUBLE_COLON identifier generic_dimension_specifier?
    | unbound_type_name DOT identifier generic_dimension_specifier?
    ;

generic_dimension_specifier: OPEN_ANGLE COMMA* CLOSE_ANGLE ;

named_entity
    : simple_name
    | named_entity_target (DOT identifier type_argument_list?)+
    ;

named_entity_target
    : THIS
    | BASE
    | simple_name
    | predefined_type 
    | qualified_alias_member
    ;

// lambda_expression: ASYNC? anonymous_function_signature OP_ARROW anonymous_function_body ;
lambda_expression: anonymous_function_signature OP_ARROW anonymous_function_body ;

anonymous_function_signature
    : explicit_anonymous_function_signature
    | implicit_anonymous_function_signature
    ;

explicit_anonymous_function_signature: OPEN_PARENS explicit_anonymous_function_parameter_list? CLOSE_PARENS ;

explicit_anonymous_function_parameter_list: explicit_anonymous_function_parameter (COMMA explicit_anonymous_function_parameter)* ;

explicit_anonymous_function_parameter: anonymous_function_parameter_modifier? type_ identifier ;

anonymous_function_parameter_modifier
    : REF
    | OUT
    ;

implicit_anonymous_function_signature
    : OPEN_PARENS implicit_anonymous_function_parameter_list? CLOSE_PARENS
    | implicit_anonymous_function_parameter
    ;

implicit_anonymous_function_parameter_list: implicit_anonymous_function_parameter (COMMA implicit_anonymous_function_parameter)* ;

implicit_anonymous_function_parameter: identifier ;

anonymous_function_body
    : expression
    | block
    ;

predefined_type
    : integral_type
    | string_type
    | OBJECT 
    ;

query_expression
    : from_clause query_body
    ;

from_clause: FROM type_? identifier IN expression ;

query_body: query_body_clauses select_or_group_clause query_continuation? ;

query_body_clauses: query_body_clause+ ;

query_body_clause
    : from_clause
    | let_clause
    | where_clause
    | join_clause
    | join_into_clause
    | orderby_clause
    ;

let_clause: LET identifier ASSIGNMENT expression ;

where_clause: WHERE expression ;

join_clause: JOIN type_? identifier IN expression ON expression EQUALS expression ;

join_into_clause: JOIN type_? identifier IN expression ON expression EQUALS expression INTO identifier ;

orderby_clause: ORDERBY orderings ;

orderings: ordering (COMMA ordering)* ;

ordering: expression ordering_direction? ;

ordering_direction
    : ASCENDING
    | DESCENDING
    ;

select_or_group_clause
    : select_clause
    | group_clause
    ;

select_clause: SELECT expression ;

group_clause: GROUP expression BY expression ;

query_continuation: INTO identifier query_body ;

// B.2.5 Statements
// https://github.com/dotnet/csharplang/blob/master/spec/statements.md

statement
    : labeled_statement
    | declaration_statement
    | embedded_statement
    ;

embedded_statement
    : block
    | empty_statement
    | expression_statement
    | selection_statement
    | iteration_statement
    | jump_statement
    | try_statement
    | checked_statement
    | unchecked_statement
    | lock_statement
    | using_statement
    | yield_statement
    | embedded_statement_unsafe
    ;

labeled_statement: identifier COLON statement ;

declaration_statement
    : local_variable_declaration SEMICOLON
    | local_constant_declaration SEMICOLON 
    | local_function_declaration
    ;

local_function_declaration
    : local_function_header local_function_body
    ;

local_function_header
    : local_function_modifiers? return_type identifier type_parameter_list?
      (formal_parameter_list?) type_parameter_constraints_clauses
    ;

local_function_modifiers
    : (ASYNC | UNSAFE)
    ;

 local_function_body
    : block
    | OP_ARROW expression SEMICOLON
    ;

local_variable_declaration: local_variable_type local_variable_declarators ;

local_variable_type 
    : VAR
    | type_
    ;

local_variable_declarators: local_variable_declarator (COMMA local_variable_declarator)* ;

local_variable_declarator: identifier (ASSIGNMENT local_variable_initializer)? ;

local_variable_initializer
    : expression
    | array_initializer
    ;

local_constant_declaration: CONST type_ constant_declarators ;

block: OPEN_BRACE statement_list? CLOSE_BRACE ;

statement_list: statement+ ;

empty_statement: SEMICOLON ;

expression_statement: statement_expression SEMICOLON ;

statement_expression
    : invocation_expression
    | null_conditional_invocation_expression
    | object_creation_expression
    | assignment
    | post_increment_expression
    | post_decrement_expression
    | pre_increment_expression
    | pre_decrement_expression
    | await_expression
    ;

selection_statement
    : if_statement
    | switch_statement
    ;

if_statement: IF OPEN_PARENS boolean_expression CLOSE_PARENS embedded_statement (ELSE embedded_statement)? ;

switch_statement: SWITCH OPEN_PARENS expression CLOSE_PARENS switch_block ;

switch_block: OPEN_BRACE switch_section* CLOSE_BRACE ;

switch_section: switch_label+ statement_list ;

switch_label: (CASE (pattern | constant_expression) case_guard? | DEFAULT) COLON ;

case_guard: WHEN expression ;

iteration_statement
    : while_statement
    | do_statement
    | for_statement
    | foreach_statement
    | foreach_variable_statement
    ;

while_statement: WHILE OPEN_PARENS boolean_expression CLOSE_PARENS embedded_statement ;

do_statement: DO embedded_statement WHILE OPEN_PARENS boolean_expression CLOSE_PARENS SEMICOLON  ;

for_statement: FOR OPEN_PARENS for_initializer? SEMICOLON for_condition? SEMICOLON for_iterator? CLOSE_PARENS embedded_statement ;

for_initializer
    : local_variable_declaration
    | statement_expression_list
    ;

for_condition: boolean_expression ;

for_iterator: statement_expression_list ;

statement_expression_list: statement_expression (COMMA statement_expression)* ;

foreach_statement: FOREACH OPEN_PARENS local_variable_type identifier IN expression CLOSE_PARENS embedded_statement ;

jump_statement
    : break_statement
    | continue_statement
    | goto_statement
    | return_statement
    | throw_statement
    ;

break_statement: BREAK SEMICOLON ;

continue_statement: CONTINUE SEMICOLON ;

goto_statement: GOTO (identifier | CASE constant_expression | DEFAULT) SEMICOLON ;

return_statement: RETURN expression? SEMICOLON ;

throw_statement: THROW expression? SEMICOLON ;

try_statement: TRY block (catch_clause+ | (catch_clause* finally_clause)) ;

catch_clause: CATCH exception_specifier? exception_filter?  block ;

exception_specifier: OPEN_PARENS type_ identifier? CLOSE_PARENS ;

exception_filter: WHEN OPEN_PARENS expression CLOSE_PARENS ;

finally_clause: FINALLY block ;

checked_statement: CHECKED block ;

unchecked_statement: UNCHECKED block ;

lock_statement: LOCK OPEN_PARENS expression CLOSE_PARENS embedded_statement ;

using_statement: USING OPEN_PARENS resource_acquisition CLOSE_PARENS embedded_statement ;

resource_acquisition
    : local_variable_declaration
    | expression
    ;

yield_statement: YIELD (RETURN expression | BREAK) SEMICOLON ;

// B.2.6 Namespaces
// https://github.com/dotnet/csharplang/blob/master/spec/namespaces.md

namespace_declaration: NAMESPACE qi=qualified_identifier namespace_body SEMICOLON? ;

qualified_identifier: identifier (DOT  identifier)* ;

namespace_body: OPEN_BRACE extern_alias_directive* using_directive* namespace_member_declaration* CLOSE_BRACE ;

extern_alias_directive: EXTERN ALIAS identifier SEMICOLON ;

using_directive
    : using_alias_directive
    | using_namespace_directive
    | using_static_directive
    ;

using_alias_directive: USING identifier ASSIGNMENT namespace_or_type_name SEMICOLON ;

using_namespace_directive: USING namespace_or_type_name SEMICOLON ;

using_static_directive: USING STATIC namespace_or_type_name SEMICOLON ;

namespace_member_declaration
    : namespace_declaration
    | type_declaration
    ;

type_declaration
    : class_declaration 
    | struct_declaration 
    | interface_declaration 
    | enum_declaration 
    | delegate_declaration
    ;

qualified_alias_member: identifier DOUBLE_COLON identifier type_argument_list? ;

// B.2.7 Classes
// https://github.com/dotnet/csharplang/blob/master/spec/classes.md

class_declaration
    : attributes? class_modifier* PARTIAL? CLASS identifier type_parameter_list?
      class_base? type_parameter_constraints_clauses? class_body SEMICOLON?
    ;

class_modifier
    : NEW
    | PUBLIC
    | PROTECTED
    | INTERNAL
    | PRIVATE
    | ABSTRACT
    | SEALED
    | STATIC
    | class_modifier_unsafe
    ;

type_parameter_list: OPEN_ANGLE type_parameter (COMMA type_parameter)* CLOSE_ANGLE ;

type_parameter: attributes? identifier ;

class_base
    : COLON class_type
    | COLON (class_type COMMA)? interface_type_list
    ;

interface_type_list: interface_type (COMMA interface_type)* ;

type_parameter_constraints_clauses: type_parameter_constraints_clause+ ;

type_parameter_constraints_clause: WHERE identifier COLON type_parameter_constraints ;

type_parameter_constraints
    : primary_constraint (COMMA secondary_constraints)? (COMMA constructor_constraint)?
    | secondary_constraints (COMMA constructor_constraint)?
    | constructor_constraint
    ;

primary_constraint
    : class_type
    | CLASS
    | STRUCT
    | CLASS QUESTION
    ;

secondary_constraints: secondary_constraint (COMMA secondary_constraint)* ;

secondary_constraint
    : interface_type
    | type_parameter
    ;

constructor_constraint: NEW OPEN_PARENS CLOSE_PARENS ;

class_body
    : OPEN_BRACE class_member_declarations? CLOSE_BRACE
    ;

class_member_declarations: class_member_declaration+ ;

class_member_declaration
    : constant_declaration
    | field_declaration
    | method_declaration
    | property_declaration
    | event_declaration
    | indexer_declaration
    | operator_declaration
    | constructor_declaration
    | destructor_declaration
    | static_constructor_declaration
    | type_declaration
    ;

constant_declaration: attributes? constant_modifier* CONST type_ constant_declarators SEMICOLON ;

constant_modifier
    : NEW
    | PUBLIC
    | PROTECTED
    | INTERNAL
    | PRIVATE
    ;

constant_declarators: constant_declarator (COMMA  constant_declarator)* ;

constant_declarator: identifier ASSIGNMENT constant_expression ;

field_declaration: attributes? field_modifier* type_ variable_declarators SEMICOLON;

field_modifier
    : NEW
    | PUBLIC
    | PROTECTED
    | INTERNAL
    | PRIVATE
    | STATIC
    | READONLY
    | VOLATILE
    | field_modifier_unsafe
    ;

variable_declarators: variable_declarator (COMMA  variable_declarator)* ;

variable_declarator: identifier (ASSIGNMENT variable_initializer)? ;

variable_initializer
    : expression
    | array_initializer
    ;

method_declaration: method_header method_body ;

method_header
    : attributes? method_modifier* PARTIAL? return_type member_name type_parameter_list?
      OPEN_PARENS formal_parameter_list? CLOSE_PARENS type_parameter_constraints_clauses?
    ;

method_modifier
    : NEW
    | PUBLIC
    | PROTECTED
    | INTERNAL
    | PRIVATE
    | STATIC
    | VIRTUAL
    | SEALED
    | OVERRIDE
    | ABSTRACT
    | EXTERN
    | ASYNC
    | method_modifier_unsafe
    ;

return_type
    : type_
    | VOID
    ;

member_name
    : identifier
    | interface_type DOT identifier
    ;

method_body
    : block
    | OP_ARROW expression SEMICOLON
    | SEMICOLON
    ;

formal_parameter_list
    : fixed_parameters (COMMA parameter_array)?
    | parameter_array
    ;

fixed_parameters: fixed_parameter ( COMMA fixed_parameter )* ;

fixed_parameter: attributes? parameter_modifier? type_ identifier default_argument? ;

default_argument: ASSIGNMENT expression ;

parameter_modifier
    : REF
    | OUT
    | THIS
    ;

parameter_array: attributes? PARAMS array_type identifier ;

property_declaration: attributes? property_modifier* type_ member_name property_body ;

property_modifier
    : NEW
    | PUBLIC
    | PROTECTED
    | INTERNAL
    | PRIVATE
    | STATIC
    | VIRTUAL
    | SEALED
    | OVERRIDE
    | ABSTRACT
    | EXTERN
    | property_modifier_unsafe
    ;

property_body
    : OPEN_BRACE accessor_declarations CLOSE_BRACE property_initializer?
    | OP_ARROW expression SEMICOLON
    ;

property_initializer: ASSIGNMENT variable_initializer SEMICOLON ;

accessor_declarations
    : get_accessor_declaration set_accessor_declaration?
    | set_accessor_declaration get_accessor_declaration?
    ;

get_accessor_declaration: attributes? accessor_modifier? GET accessor_body ;

set_accessor_declaration: attributes? accessor_modifier? SET accessor_body ;

accessor_modifier
    : PROTECTED
    | INTERNAL
    | PRIVATE
    | PROTECTED INTERNAL
    | INTERNAL PROTECTED
    ;

accessor_body
    : block
    | SEMICOLON
    ;

event_declaration
    : attributes? event_modifier* EVENT type_ 
      (variable_declarators SEMICOLON | member_name OPEN_BRACE event_accessor_declarations CLOSE_BRACE)
    ;

event_modifier
    : NEW
    | PUBLIC
    | PROTECTED
    | INTERNAL
    | PRIVATE
    | STATIC
    | VIRTUAL
    | SEALED
    | OVERRIDE
    | ABSTRACT
    | EXTERN
    | event_modifier_unsafe
    ;

event_accessor_declarations
    : add_accessor_declaration remove_accessor_declaration
    | remove_accessor_declaration add_accessor_declaration
    ;

add_accessor_declaration: attributes? ADD block ;

remove_accessor_declaration: attributes? REMOVE block ;

indexer_declaration: attributes? indexer_modifier* indexer_declarator indexer_body ;

indexer_modifier
    : NEW
    | PUBLIC
    | PROTECTED
    | INTERNAL
    | PRIVATE
    | VIRTUAL
    | SEALED
    | OVERRIDE
    | ABSTRACT
    | EXTERN
    | indexer_modifier_unsafe
    ;

indexer_declarator
    : type_ THIS OPEN_BRACKET formal_parameter_list CLOSE_BRACKET
    | type_ interface_type DOT THIS OPEN_BRACKET formal_parameter_list CLOSE_BRACKET
    ;

indexer_body
    : OPEN_BRACE accessor_declarations CLOSE_BRACE
    | OP_ARROW expression SEMICOLON
    ;

operator_declaration: attributes? operator_modifier+ operator_declarator operator_body ;

operator_modifier
    : PUBLIC
    | STATIC
    | EXTERN
    | operator_modifier_unsafe
    ;

operator_declarator
    : unary_operator_declarator
    | binary_operator_declarator
    | conversion_operator_declarator
    ;

unary_operator_declarator: type_ OPERATOR overloadable_unary_operator OPEN_PARENS type_ identifier CLOSE_PARENS ;

overloadable_unary_operator
    : PLUS
    | MINUS
    | BANG
    | TILDE
    | OP_INC
    | OP_DEC
    | TRUE
    | FALSE
    ;

binary_operator_declarator: type_ OPERATOR overloadable_binary_operator OPEN_PARENS type_ identifier COMMA type_ identifier CLOSE_PARENS ;

overloadable_binary_operator
    : PLUS
    | MINUS
    | STAR
    | SLASH
    | PERCENT
    | AMP
    | PIPE
    | CARET
    | OP_LEFT_SHIFT
    | OP_RIGHT_SHIFT
    | OP_EQ
    | OP_NE
    | CLOSE_ANGLE
    | OPEN_ANGLE
    | OP_GE
    | OP_LE
    ;

conversion_operator_declarator: (IMPLICIT | EXPLICIT) OPERATOR type_ OPEN_PARENS type_ identifier CLOSE_PARENS ;

operator_body
    : block
    | OP_ARROW expression SEMICOLON
    | SEMICOLON
    ;

constructor_declaration
    : attributes? constructor_modifier* constructor_declarator constructor_body
    ;

constructor_modifier
    : PUBLIC
    | PROTECTED
    | INTERNAL
    | PRIVATE
    | EXTERN
    | constructor_modifier_unsafe
    ;

constructor_declarator: identifier OPEN_PARENS formal_parameter_list? CLOSE_PARENS constructor_initializer? ;

constructor_initializer: COLON (BASE | THIS) OPEN_PARENS argument_list? CLOSE_PARENS ;

constructor_body
    : block
    | SEMICOLON
    ;

static_constructor_declaration: attributes? static_constructor_modifiers identifier OPEN_PARENS CLOSE_PARENS static_constructor_body ;

static_constructor_modifiers
    : STATIC EXTERN?
    | EXTERN STATIC
    | static_constructor_modifiers_unsafe
    ;

static_constructor_body
    : block
    | SEMICOLON
    ;

destructor_declaration
    : attributes? EXTERN? TILDE identifier OPEN_PARENS CLOSE_PARENS destructor_body
    | destructor_declaration_unsafe
    ;

destructor_body
    : block
    | SEMICOLON
    ;

all_member_modifiers
    : all_member_modifier+
    ;

all_member_modifier
    : NEW
    | PUBLIC
    | PROTECTED
    | INTERNAL
    | PRIVATE
    | READONLY
    | VOLATILE
    | VIRTUAL
    | SEALED
    | OVERRIDE
    | ABSTRACT
    | STATIC
    | UNSAFE
    | EXTERN
    | PARTIAL
    | ASYNC  
    ;

// B.2.8 Structs
// https://github.com/dotnet/csharplang/blob/master/spec/structs.md

struct_declaration
    : attributes? struct_modifier* PARTIAL? STRUCT identifier type_parameter_list?
      struct_interfaces? type_parameter_constraints_clauses? struct_body SEMICOLON?
    ;

struct_modifier
    : NEW
    | PUBLIC
    | PROTECTED
    | INTERNAL
    | PRIVATE
    | struct_modifier_unsafe
    ;

struct_interfaces: COLON interface_type_list ;

struct_body: OPEN_BRACE struct_member_declaration* CLOSE_BRACE ;

struct_member_declaration
    : constant_declaration
    | field_declaration
    | method_declaration
    | property_declaration
    | event_declaration
    | indexer_declaration
    | operator_declaration
    | constructor_declaration
    | static_constructor_declaration
    | type_declaration
    | struct_member_declaration_unsafe
    ;

// B.2.9 Arrays
// https://github.com/dotnet/csharplang/blob/master/spec/arrays.md

array_initializer: OPEN_BRACE (variable_initializer_list? | variable_initializer_list COMMA) CLOSE_BRACE ;

variable_initializer_list: variable_initializer (COMMA variable_initializer)* ;

// B.2.10 Interfaces
// https://github.com/dotnet/csharplang/blob/master/spec/interfaces.md

interface_declaration
    : attributes? interface_modifier* PARTIAL? INTERFACE identifier variant_type_parameter_list?
      interface_base? type_parameter_constraints_clauses? interface_body SEMICOLON?
    ;

interface_modifier
    : NEW
    | PUBLIC
    | PROTECTED
    | INTERNAL
    | PRIVATE
    | interface_modifier_unsafe
    ;

variant_type_parameter_list: OPEN_ANGLE variant_type_parameter (COMMA variant_type_parameter)* CLOSE_ANGLE ;

variant_type_parameter: attributes? variance_annotation? identifier ;

variance_annotation
    : IN
    | OUT
    ;

interface_base: COLON interface_type_list ;

interface_body: OPEN_BRACE interface_member_declaration* CLOSE_BRACE ;

interface_member_declaration
    : interface_method_declaration
    | interface_property_declaration
    | interface_event_declaration
    | interface_indexer_declaration
    ;

interface_method_declaration
    : attributes? NEW? return_type identifier type_parameter_list?
      OPEN_PARENS formal_parameter_list? CLOSE_PARENS type_parameter_constraints_clauses? SEMICOLON
    ;

interface_property_declaration: attributes? NEW? type_ identifier OPEN_BRACE interface_accessors CLOSE_BRACE ;

interface_accessors
    : interface_get_accessor interface_set_accessor? 
    | interface_set_accessor interface_get_accessor? 
    ;

interface_get_accessor: attributes? GET SEMICOLON ;

interface_set_accessor: attributes? SET SEMICOLON ;

interface_event_declaration: attributes? NEW? EVENT type_ identifier SEMICOLON ;

interface_indexer_declaration
    : attributes? NEW? type_ THIS OPEN_BRACKET formal_parameter_list CLOSE_BRACKET
      OPEN_BRACE interface_accessors CLOSE_BRACE
    ;

// B.2.11 Enums
// https://github.com/dotnet/csharplang/blob/master/spec/enums.md

enum_declaration: attributes? enum_modifier* ENUM identifier enum_base? enum_body SEMICOLON? ;

enum_modifier
    : NEW
    | PUBLIC
    | PROTECTED
    | INTERNAL
    | PRIVATE
    ;

enum_base: COLON integral_type ;

enum_body: OPEN_BRACE (enum_member_declarations? | enum_member_declarations COMMA) CLOSE_BRACE ;

enum_member_declarations: enum_member_declaration (COMMA enum_member_declaration)* ;

enum_member_declaration: attributes? identifier (ASSIGNMENT constant_expression)? ;


// B.2.12 Delegates
// https://github.com/dotnet/csharplang/blob/master/spec/delegates.md

delegate_declaration
    : attributes? delegate_modifier* DELEGATE return_type identifier variant_type_parameter_list?
      OPEN_PARENS formal_parameter_list? CLOSE_PARENS type_parameter_constraints_clauses? SEMICOLON
    ;

delegate_modifier
    : NEW
    | PUBLIC
    | PROTECTED
    | INTERNAL
    | PRIVATE
    | delegate_modifier_unsafe
    ;


// B.2.13 Attributes
// https://github.com/dotnet/csharplang/blob/master/spec/attributes.md

global_attributes: global_attribute_section+ ;

global_attribute_section: OPEN_BRACKET global_attribute_target_specifier attribute_list COMMA? CLOSE_BRACKET ;

global_attribute_target_specifier: global_attribute_target COLON ;

global_attribute_target
    : ASSEMBLY
    | MODULE
    ;

attributes: attribute_section+ ;

attribute_section: OPEN_BRACKET attribute_target_specifier? attribute_list COMMA? CLOSE_BRACKET ;

attribute_target_specifier: attribute_target COLON ;

attribute_target
    : FIELD
    | EVENT
    | METHOD
    | PARAM
    | PROPERTY
    | RETURN
    | TYPE
    ;

attribute_list: attribute (COMMA  attribute)* ;

attribute: attribute_name attribute_arguments? ;

attribute_name: type_name ;

attribute_arguments
    : OPEN_PARENS positional_argument_list? CLOSE_PARENS
    | OPEN_PARENS positional_argument_list COMMA named_argument_list CLOSE_PARENS
    | OPEN_PARENS named_argument_list CLOSE_PARENS
    ;

positional_argument_list: positional_argument (COMMA positional_argument)* ;

positional_argument: attribute_argument_expression ;

named_argument_list: named_argument (COMMA named_argument)* ;

named_argument: identifier ASSIGNMENT attribute_argument_expression ;

attribute_argument_expression: expression ;

// B.3 Grammar extensions for unsafe code
// https://github.com/dotnet/csharplang/blob/master/spec/unsafe-code.md

class_modifier_unsafe: UNSAFE ;

struct_modifier_unsafe: UNSAFE ;

interface_modifier_unsafe: UNSAFE ;

delegate_modifier_unsafe: UNSAFE ;

field_modifier_unsafe: UNSAFE ;

method_modifier_unsafe: UNSAFE ;

property_modifier_unsafe: UNSAFE ;

event_modifier_unsafe: UNSAFE ;

indexer_modifier_unsafe: UNSAFE ;

operator_modifier_unsafe: UNSAFE ;

constructor_modifier_unsafe: UNSAFE ;

destructor_declaration_unsafe: attributes? (EXTERN? UNSAFE | UNSAFE EXTERN) TILDE identifier OPEN_PARENS CLOSE_PARENS destructor_body ;

static_constructor_modifiers_unsafe
    : EXTERN? UNSAFE? STATIC
    | UNSAFE? EXTERN? STATIC
    | EXTERN? STATIC UNSAFE?
    | UNSAFE? STATIC EXTERN?
    | STATIC EXTERN? UNSAFE?
    | STATIC UNSAFE? EXTERN?
    ;

embedded_statement_unsafe
    : unsafe_statement
    | fixed_statement
    ;

unsafe_statement: UNSAFE block ;

pointer_type
    : base_type (rank_specifier | QUESTION)* STAR
    | VOID STAR
    ;

unmanaged_type: type_ ;

unary_expression_unsafe
    : pointer_indirection_expression
    | addressof_expression
    ;

pointer_indirection_expression: STAR unary_expression ;

addressof_expression: AMP unary_expression ;

fixed_statement: FIXED OPEN_PARENS pointer_type fixed_pointer_declarators CLOSE_PARENS embedded_statement ;

fixed_pointer_declarators: fixed_pointer_declarator (COMMA fixed_pointer_declarator)* ;

fixed_pointer_declarator: identifier ASSIGNMENT fixed_pointer_initializer ;

fixed_pointer_initializer
    : AMP variable_reference
    | expression
    ;

struct_member_declaration_unsafe: fixed_size_buffer_declaration ;

fixed_size_buffer_declaration: attributes? fixed_size_buffer_modifier* FIXED buffer_element_type fixed_size_buffer_declarator+ SEMICOLON ;

fixed_size_buffer_modifier
    : NEW
    | PUBLIC
    | PROTECTED
    | INTERNAL
    | PRIVATE
    | fixed_size_buffer_modifier_unsafe
    ;

fixed_size_buffer_modifier_unsafe: UNSAFE ;

buffer_element_type: type_ ;

fixed_size_buffer_declarator: identifier OPEN_BRACKET constant_expression CLOSE_BRACKET ;

stackalloc_initializer
    : STACKALLOC unmanaged_type OPEN_BRACKET expression? CLOSE_BRACKET array_initializer?
    | STACKALLOC OPEN_BRACKET expression? CLOSE_BRACKET array_initializer
    ;

// B.1.7+ Tokens
// https://github.com/dotnet/csharplang/blob/master/spec/lexical-structure.md#tokens

// B.1.7 Identifiers

identifier
    : IDENTIFIER
    | ADD
    | ALIAS
    | ARGLIST
    | ASCENDING
    | ASYNC
    | AWAIT
    | BY
    | DESCENDING
    | DYNAMIC
    | EQUALS
    | FROM
    | GET
    | GROUP
    | INTO
    | JOIN
    | LET
    | NAMEOF
    | ON
    | ORDERBY
    | PARTIAL
    | REMOVE
    | SELECT
    | SET
    | VAR
    | WHEN
    | WHERE
    | YIELD
    ;

// B.1.8 Literals

literal
    : boolean_literal
    | string_literal
    | INTEGER_LITERAL
    | REAL_LITERAL
    | CHARACTER_LITERAL
    | NULL
    | tuple_literal
    ;

boolean_literal
    : TRUE
    | FALSE
    ;

string_literal
    : REGULAR_STRING
    | VERBATIM_STRING
    ;

interpolated_string_expression
    : interpolated_regular_string
    | interpolated_verbatim_string
    ;

interpolated_regular_string: INTERPOLATED_REGULAR_STRING_START interpolated_regular_string_part* DOUBLE_QUOTE_INSIDE ;

interpolated_verbatim_string: INTERPOLATED_VERBATIM_STRING_START interpolated_verbatim_string_part* DOUBLE_QUOTE_INSIDE ;

interpolated_regular_string_part
    : interpolated_string_inner_expression
    | DOUBLE_CURLY_INSIDE
    | REGULAR_CHAR_INSIDE
    | REGULAR_STRING_INSIDE
    ;

interpolated_verbatim_string_part
    : interpolated_string_inner_expression
    | DOUBLE_CURLY_INSIDE
    | VERBATIM_DOUBLE_QUOTE_INSIDE
    | VERBATIM_INSIDE_STRING
    ;

interpolated_string_inner_expression: expression (COMMA expression)* (COLON FORMAT_STRING+)? ;

tuple_literal: OPEN_PARENS tuple_literal_element_list CLOSE_PARENS ;

tuple_literal_element_list: tuple_literal_element (COMMA tuple_literal_element)+ ;

tuple_literal_element: ( identifier COLON )? expression ;

// B.1.9 Keywords

keyword
    : ABSTRACT
    | AS
    | BASE
    | BOOL
    | BREAK
    | BYTE
    | CASE
    | CATCH
    | CHAR
    | CHECKED
    | CLASS
    | CONST
    | CONTINUE
    | DECIMAL
    | DEFAULT
    | DELEGATE
    | DO
    | DOUBLE
    | ELSE
    | ENUM
    | EVENT
    | EXPLICIT
    | EXTERN
    | FALSE
    | FINALLY
    | FIXED
    | FLOAT
    | FOR
    | FOREACH
    | GOTO
    | IF
    | IMPLICIT
    | IN
    | INT
    | INTERFACE
    | INTERNAL
    | IS
    | LOCK
    | LONG
    | NAMESPACE
    | NEW
    | NULL
    | OBJECT
    | OPERATOR
    | OUT
    | OVERRIDE
    | PARAMS
    | PRIVATE
    | PROTECTED
    | PUBLIC
    | READONLY
    | REF
    | RETURN
    | SBYTE
    | SEALED
    | SHORT
    | SIZEOF
    | STACKALLOC
    | STATIC
    | STRING
    | STRUCT
    | SWITCH
    | THIS
    | THROW
    | TRUE
    | TRY
    | TYPEOF
    | UINT
    | ULONG
    | UNCHECKED
    | UNSAFE
    | USHORT
    | USING
    | VIRTUAL
    | VOID
    | VOLATILE
    | WHILE
    ;
