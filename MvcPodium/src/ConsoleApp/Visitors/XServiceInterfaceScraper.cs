using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using Microsoft.Extensions.Options;
using MvcPodium.ConsoleApp.Model.Config;
using MvcPodium.ConsoleApp.Services;
using System;
using System.Collections.Generic;
using IToken = Antlr4.Runtime.IToken;
using ParserRuleContext = Antlr4.Runtime.ParserRuleContext;

namespace MvcPodium.ConsoleApp.Visitors
{
    public class XServiceInterfaceScraper : CSharpParserBaseVisitor<object>
    {
        private readonly IOptions<AppSettings> _appSettings;
        private readonly IOptions<UserSettings> _userSettings;
        private readonly IStringTemplateService _stringTemplateService;

        public string ServiceRootName { get; }

        public HashSet<string> Imports { set; get; } = new HashSet<string>();
        public TokenStreamRewriter Rewriter { get; }
        public BufferedTokenStream Tokens { get; }

        public XServiceInterfaceScraper(
            BufferedTokenStream tokenStream,
            string serviceRootName,
            IOptions<AppSettings> appSettings,
            IOptions<UserSettings> userSettings,
            IStringTemplateService stringTemplateService)
        {
            _appSettings = appSettings;
            _userSettings = userSettings;
            _stringTemplateService = stringTemplateService;
            Tokens = tokenStream;
            ServiceRootName = serviceRootName;
            Rewriter = new TokenStreamRewriter(tokenStream);
        }

        public override object VisitCompilation_unit([NotNull] CSharpParser.Compilation_unitContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitNamespace_name([NotNull] CSharpParser.Namespace_nameContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitType_name([NotNull] CSharpParser.Type_nameContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitNamespace_or_type_name([NotNull] CSharpParser.Namespace_or_type_nameContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitType_([NotNull] CSharpParser.Type_Context context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitBase_type([NotNull] CSharpParser.Base_typeContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitValue_type([NotNull] CSharpParser.Value_typeContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitStruct_type([NotNull] CSharpParser.Struct_typeContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitSimple_type([NotNull] CSharpParser.Simple_typeContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitNumeric_type([NotNull] CSharpParser.Numeric_typeContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitIntegral_type([NotNull] CSharpParser.Integral_typeContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitFloating_point_type([NotNull] CSharpParser.Floating_point_typeContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitString_type([NotNull] CSharpParser.String_typeContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitEnum_type([NotNull] CSharpParser.Enum_typeContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitReference_type([NotNull] CSharpParser.Reference_typeContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitClass_type([NotNull] CSharpParser.Class_typeContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitInterface_type([NotNull] CSharpParser.Interface_typeContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitArray_type([NotNull] CSharpParser.Array_typeContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitNon_array_type([NotNull] CSharpParser.Non_array_typeContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitRank_specifier([NotNull] CSharpParser.Rank_specifierContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitDim_separator([NotNull] CSharpParser.Dim_separatorContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitDelegate_type([NotNull] CSharpParser.Delegate_typeContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitType_argument_list([NotNull] CSharpParser.Type_argument_listContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitVariable_reference([NotNull] CSharpParser.Variable_referenceContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitExpression([NotNull] CSharpParser.ExpressionContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitConstant_expression([NotNull] CSharpParser.Constant_expressionContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitBoolean_expression([NotNull] CSharpParser.Boolean_expressionContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitArgument_list([NotNull] CSharpParser.Argument_listContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitArgument([NotNull] CSharpParser.ArgumentContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitArgument_name([NotNull] CSharpParser.Argument_nameContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitArgument_value([NotNull] CSharpParser.Argument_valueContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitNon_assignment_expression([NotNull] CSharpParser.Non_assignment_expressionContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitAssignment([NotNull] CSharpParser.AssignmentContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitAssignment_operator([NotNull] CSharpParser.Assignment_operatorContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitConditional_expression([NotNull] CSharpParser.Conditional_expressionContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitNull_coalescing_expression([NotNull] CSharpParser.Null_coalescing_expressionContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitThrow_expression([NotNull] CSharpParser.Throw_expressionContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitConditional_or_expression([NotNull] CSharpParser.Conditional_or_expressionContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitConditional_and_expression([NotNull] CSharpParser.Conditional_and_expressionContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitInclusive_or_expression([NotNull] CSharpParser.Inclusive_or_expressionContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitExclusive_or_expression([NotNull] CSharpParser.Exclusive_or_expressionContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitAnd_expression([NotNull] CSharpParser.And_expressionContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitEquality_expression([NotNull] CSharpParser.Equality_expressionContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitRelational_expression([NotNull] CSharpParser.Relational_expressionContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitShift_expression([NotNull] CSharpParser.Shift_expressionContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitAdditive_expression([NotNull] CSharpParser.Additive_expressionContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitMultiplicative_expression([NotNull] CSharpParser.Multiplicative_expressionContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitUnary_expression([NotNull] CSharpParser.Unary_expressionContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitNull_conditional_expression([NotNull] CSharpParser.Null_conditional_expressionContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitNull_conditional_operations([NotNull] CSharpParser.Null_conditional_operationsContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitNull_conditional_operation([NotNull] CSharpParser.Null_conditional_operationContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitNull_conditional_member_access([NotNull] CSharpParser.Null_conditional_member_accessContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitNull_conditional_invocation_expression([NotNull] CSharpParser.Null_conditional_invocation_expressionContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitPre_increment_expression([NotNull] CSharpParser.Pre_increment_expressionContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitPre_decrement_expression([NotNull] CSharpParser.Pre_decrement_expressionContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitCast_expression([NotNull] CSharpParser.Cast_expressionContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitAwait_expression([NotNull] CSharpParser.Await_expressionContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitPrimary_expression([NotNull] CSharpParser.Primary_expressionContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitLiteral_pes([NotNull] CSharpParser.Literal_pesContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitInterpolatedStringExpression_pes([NotNull] CSharpParser.InterpolatedStringExpression_pesContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitSimpleName_pes([NotNull] CSharpParser.SimpleName_pesContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitPredefinedName_pes([NotNull] CSharpParser.PredefinedName_pesContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitMemberAccessExpression([NotNull] CSharpParser.MemberAccessExpressionContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitLiteralAccess_pes([NotNull] CSharpParser.LiteralAccess_pesContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitThisAccess_pes([NotNull] CSharpParser.ThisAccess_pesContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitBaseAccess_pes([NotNull] CSharpParser.BaseAccess_pesContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitObjectCreationExpression_pes([NotNull] CSharpParser.ObjectCreationExpression_pesContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitTypeofExpression_pes([NotNull] CSharpParser.TypeofExpression_pesContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitCheckedExpression_pes([NotNull] CSharpParser.CheckedExpression_pesContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitUncheckedExpression_pes([NotNull] CSharpParser.UncheckedExpression_pesContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitDefaultValueExpression_pes([NotNull] CSharpParser.DefaultValueExpression_pesContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitAnonymousMethodExpression_pes([NotNull] CSharpParser.AnonymousMethodExpression_pesContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitSizeofExpression_pes([NotNull] CSharpParser.SizeofExpression_pesContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitNameofExpression_pes([NotNull] CSharpParser.NameofExpression_pesContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitDelegateCreationExpression_pes([NotNull] CSharpParser.DelegateCreationExpression_pesContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitAnonymousObjectCreationExpression_pes([NotNull] CSharpParser.AnonymousObjectCreationExpression_pesContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitMember_access([NotNull] CSharpParser.Member_accessContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitBracket_expression([NotNull] CSharpParser.Bracket_expressionContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitIndexer_argument([NotNull] CSharpParser.Indexer_argumentContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitNameof_expression([NotNull] CSharpParser.Nameof_expressionContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitParenthesized_expression([NotNull] CSharpParser.Parenthesized_expressionContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitSizeof_expression([NotNull] CSharpParser.Sizeof_expressionContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitAnonymous_method_expression([NotNull] CSharpParser.Anonymous_method_expressionContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitDefault_value_expression([NotNull] CSharpParser.Default_value_expressionContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitChecked_expression([NotNull] CSharpParser.Checked_expressionContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitUnchecked_expression([NotNull] CSharpParser.Unchecked_expressionContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitTypeof_expression([NotNull] CSharpParser.Typeof_expressionContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitThis_access([NotNull] CSharpParser.This_accessContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitBase_access([NotNull] CSharpParser.Base_accessContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitSimple_name([NotNull] CSharpParser.Simple_nameContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitInvocation_expression([NotNull] CSharpParser.Invocation_expressionContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitPost_increment_expression([NotNull] CSharpParser.Post_increment_expressionContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitPost_decrement_expression([NotNull] CSharpParser.Post_decrement_expressionContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitObject_creation_expression([NotNull] CSharpParser.Object_creation_expressionContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitObject_or_collection_initializer([NotNull] CSharpParser.Object_or_collection_initializerContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitObject_initializer([NotNull] CSharpParser.Object_initializerContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitMember_initializer_list([NotNull] CSharpParser.Member_initializer_listContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitMember_initializer([NotNull] CSharpParser.Member_initializerContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitInitializer_target([NotNull] CSharpParser.Initializer_targetContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitInitializer_value([NotNull] CSharpParser.Initializer_valueContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitCollection_initializer([NotNull] CSharpParser.Collection_initializerContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitElement_initializer_list([NotNull] CSharpParser.Element_initializer_listContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitElement_initializer([NotNull] CSharpParser.Element_initializerContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitExpression_list([NotNull] CSharpParser.Expression_listContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitArray_creation_expression([NotNull] CSharpParser.Array_creation_expressionContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitDelegate_creation_expression([NotNull] CSharpParser.Delegate_creation_expressionContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitAnonymous_object_creation_expression([NotNull] CSharpParser.Anonymous_object_creation_expressionContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitAnonymous_object_initializer([NotNull] CSharpParser.Anonymous_object_initializerContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitMember_declarator_list([NotNull] CSharpParser.Member_declarator_listContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitMember_declarator([NotNull] CSharpParser.Member_declaratorContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitUnbound_type_name([NotNull] CSharpParser.Unbound_type_nameContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitGeneric_dimension_specifier([NotNull] CSharpParser.Generic_dimension_specifierContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitNamed_entity([NotNull] CSharpParser.Named_entityContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitNamed_entity_target([NotNull] CSharpParser.Named_entity_targetContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitLambda_expression([NotNull] CSharpParser.Lambda_expressionContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitAnonymous_function_signature([NotNull] CSharpParser.Anonymous_function_signatureContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitExplicit_anonymous_function_signature([NotNull] CSharpParser.Explicit_anonymous_function_signatureContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitExplicit_anonymous_function_parameter_list([NotNull] CSharpParser.Explicit_anonymous_function_parameter_listContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitExplicit_anonymous_function_parameter([NotNull] CSharpParser.Explicit_anonymous_function_parameterContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitAnonymous_function_parameter_modifier([NotNull] CSharpParser.Anonymous_function_parameter_modifierContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitImplicit_anonymous_function_signature([NotNull] CSharpParser.Implicit_anonymous_function_signatureContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitImplicit_anonymous_function_parameter_list([NotNull] CSharpParser.Implicit_anonymous_function_parameter_listContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitImplicit_anonymous_function_parameter([NotNull] CSharpParser.Implicit_anonymous_function_parameterContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitAnonymous_function_body([NotNull] CSharpParser.Anonymous_function_bodyContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitPredefined_type([NotNull] CSharpParser.Predefined_typeContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitQuery_expression([NotNull] CSharpParser.Query_expressionContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitFrom_clause([NotNull] CSharpParser.From_clauseContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitQuery_body([NotNull] CSharpParser.Query_bodyContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitQuery_body_clauses([NotNull] CSharpParser.Query_body_clausesContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitQuery_body_clause([NotNull] CSharpParser.Query_body_clauseContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitLet_clause([NotNull] CSharpParser.Let_clauseContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitWhere_clause([NotNull] CSharpParser.Where_clauseContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitJoin_clause([NotNull] CSharpParser.Join_clauseContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitJoin_into_clause([NotNull] CSharpParser.Join_into_clauseContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitOrderby_clause([NotNull] CSharpParser.Orderby_clauseContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitOrderings([NotNull] CSharpParser.OrderingsContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitOrdering([NotNull] CSharpParser.OrderingContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitOrdering_direction([NotNull] CSharpParser.Ordering_directionContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitSelect_or_group_clause([NotNull] CSharpParser.Select_or_group_clauseContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitSelect_clause([NotNull] CSharpParser.Select_clauseContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitGroup_clause([NotNull] CSharpParser.Group_clauseContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitQuery_continuation([NotNull] CSharpParser.Query_continuationContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitStatement([NotNull] CSharpParser.StatementContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitEmbedded_statement([NotNull] CSharpParser.Embedded_statementContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitLabeled_statement([NotNull] CSharpParser.Labeled_statementContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitDeclaration_statement([NotNull] CSharpParser.Declaration_statementContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitLocal_variable_declaration([NotNull] CSharpParser.Local_variable_declarationContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitLocal_variable_type([NotNull] CSharpParser.Local_variable_typeContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitLocal_variable_declarators([NotNull] CSharpParser.Local_variable_declaratorsContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitLocal_variable_declarator([NotNull] CSharpParser.Local_variable_declaratorContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitLocal_variable_initializer([NotNull] CSharpParser.Local_variable_initializerContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitLocal_constant_declaration([NotNull] CSharpParser.Local_constant_declarationContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitBlock([NotNull] CSharpParser.BlockContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitStatement_list([NotNull] CSharpParser.Statement_listContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitEmpty_statement([NotNull] CSharpParser.Empty_statementContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitExpression_statement([NotNull] CSharpParser.Expression_statementContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitStatement_expression([NotNull] CSharpParser.Statement_expressionContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitSelection_statement([NotNull] CSharpParser.Selection_statementContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitIf_statement([NotNull] CSharpParser.If_statementContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitSwitch_statement([NotNull] CSharpParser.Switch_statementContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitSwitch_block([NotNull] CSharpParser.Switch_blockContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitSwitch_section([NotNull] CSharpParser.Switch_sectionContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitSwitch_label([NotNull] CSharpParser.Switch_labelContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitIteration_statement([NotNull] CSharpParser.Iteration_statementContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitWhile_statement([NotNull] CSharpParser.While_statementContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitDo_statement([NotNull] CSharpParser.Do_statementContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitFor_statement([NotNull] CSharpParser.For_statementContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitFor_initializer([NotNull] CSharpParser.For_initializerContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitFor_condition([NotNull] CSharpParser.For_conditionContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitFor_iterator([NotNull] CSharpParser.For_iteratorContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitStatement_expression_list([NotNull] CSharpParser.Statement_expression_listContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitForeach_statement([NotNull] CSharpParser.Foreach_statementContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitJump_statement([NotNull] CSharpParser.Jump_statementContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitBreak_statement([NotNull] CSharpParser.Break_statementContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitContinue_statement([NotNull] CSharpParser.Continue_statementContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitGoto_statement([NotNull] CSharpParser.Goto_statementContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitReturn_statement([NotNull] CSharpParser.Return_statementContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitThrow_statement([NotNull] CSharpParser.Throw_statementContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitTry_statement([NotNull] CSharpParser.Try_statementContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitCatch_clause([NotNull] CSharpParser.Catch_clauseContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitException_specifier([NotNull] CSharpParser.Exception_specifierContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitException_filter([NotNull] CSharpParser.Exception_filterContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitFinally_clause([NotNull] CSharpParser.Finally_clauseContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitChecked_statement([NotNull] CSharpParser.Checked_statementContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitUnchecked_statement([NotNull] CSharpParser.Unchecked_statementContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitLock_statement([NotNull] CSharpParser.Lock_statementContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitUsing_statement([NotNull] CSharpParser.Using_statementContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitResource_acquisition([NotNull] CSharpParser.Resource_acquisitionContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitYield_statement([NotNull] CSharpParser.Yield_statementContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitNamespace_declaration([NotNull] CSharpParser.Namespace_declarationContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitQualified_identifier([NotNull] CSharpParser.Qualified_identifierContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitNamespace_body([NotNull] CSharpParser.Namespace_bodyContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitExtern_alias_directive([NotNull] CSharpParser.Extern_alias_directiveContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitUsing_directive([NotNull] CSharpParser.Using_directiveContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitUsing_alias_directive([NotNull] CSharpParser.Using_alias_directiveContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitUsing_namespace_directive([NotNull] CSharpParser.Using_namespace_directiveContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitUsing_static_directive([NotNull] CSharpParser.Using_static_directiveContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitNamespace_member_declaration([NotNull] CSharpParser.Namespace_member_declarationContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitType_declaration([NotNull] CSharpParser.Type_declarationContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitQualified_alias_member([NotNull] CSharpParser.Qualified_alias_memberContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitClass_declaration([NotNull] CSharpParser.Class_declarationContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitClass_modifier([NotNull] CSharpParser.Class_modifierContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitType_parameter_list([NotNull] CSharpParser.Type_parameter_listContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitType_parameter([NotNull] CSharpParser.Type_parameterContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitClass_base([NotNull] CSharpParser.Class_baseContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitInterface_type_list([NotNull] CSharpParser.Interface_type_listContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitType_parameter_constraints_clauses([NotNull] CSharpParser.Type_parameter_constraints_clausesContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitType_parameter_constraints_clause([NotNull] CSharpParser.Type_parameter_constraints_clauseContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitType_parameter_constraints([NotNull] CSharpParser.Type_parameter_constraintsContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitPrimary_constraint([NotNull] CSharpParser.Primary_constraintContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitSecondary_constraints([NotNull] CSharpParser.Secondary_constraintsContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitSecondary_constraint([NotNull] CSharpParser.Secondary_constraintContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitConstructor_constraint([NotNull] CSharpParser.Constructor_constraintContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitClass_body([NotNull] CSharpParser.Class_bodyContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitClass_member_declarations([NotNull] CSharpParser.Class_member_declarationsContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitClass_member_declaration([NotNull] CSharpParser.Class_member_declarationContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitConstant_declaration([NotNull] CSharpParser.Constant_declarationContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitConstant_modifier([NotNull] CSharpParser.Constant_modifierContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitConstant_declarators([NotNull] CSharpParser.Constant_declaratorsContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitConstant_declarator([NotNull] CSharpParser.Constant_declaratorContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitField_declaration([NotNull] CSharpParser.Field_declarationContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitField_modifier([NotNull] CSharpParser.Field_modifierContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitVariable_declarators([NotNull] CSharpParser.Variable_declaratorsContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitVariable_declarator([NotNull] CSharpParser.Variable_declaratorContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitVariable_initializer([NotNull] CSharpParser.Variable_initializerContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitMethod_declaration([NotNull] CSharpParser.Method_declarationContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitMethod_header([NotNull] CSharpParser.Method_headerContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitMethod_modifier([NotNull] CSharpParser.Method_modifierContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitReturn_type([NotNull] CSharpParser.Return_typeContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitMember_name([NotNull] CSharpParser.Member_nameContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitMethod_body([NotNull] CSharpParser.Method_bodyContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitFormal_parameter_list([NotNull] CSharpParser.Formal_parameter_listContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitFixed_parameters([NotNull] CSharpParser.Fixed_parametersContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitFixed_parameter([NotNull] CSharpParser.Fixed_parameterContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitDefault_argument([NotNull] CSharpParser.Default_argumentContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitParameter_modifier([NotNull] CSharpParser.Parameter_modifierContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitParameter_array([NotNull] CSharpParser.Parameter_arrayContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitProperty_declaration([NotNull] CSharpParser.Property_declarationContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitProperty_modifier([NotNull] CSharpParser.Property_modifierContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitProperty_body([NotNull] CSharpParser.Property_bodyContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitProperty_initializer([NotNull] CSharpParser.Property_initializerContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitAccessor_declarations([NotNull] CSharpParser.Accessor_declarationsContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitGet_accessor_declaration([NotNull] CSharpParser.Get_accessor_declarationContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitSet_accessor_declaration([NotNull] CSharpParser.Set_accessor_declarationContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitAccessor_modifier([NotNull] CSharpParser.Accessor_modifierContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitAccessor_body([NotNull] CSharpParser.Accessor_bodyContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitEvent_declaration([NotNull] CSharpParser.Event_declarationContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitEvent_modifier([NotNull] CSharpParser.Event_modifierContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitEvent_accessor_declarations([NotNull] CSharpParser.Event_accessor_declarationsContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitAdd_accessor_declaration([NotNull] CSharpParser.Add_accessor_declarationContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitRemove_accessor_declaration([NotNull] CSharpParser.Remove_accessor_declarationContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitIndexer_declaration([NotNull] CSharpParser.Indexer_declarationContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitIndexer_modifier([NotNull] CSharpParser.Indexer_modifierContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitIndexer_declarator([NotNull] CSharpParser.Indexer_declaratorContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitIndexer_body([NotNull] CSharpParser.Indexer_bodyContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitOperator_declaration([NotNull] CSharpParser.Operator_declarationContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitOperator_modifier([NotNull] CSharpParser.Operator_modifierContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitOperator_declarator([NotNull] CSharpParser.Operator_declaratorContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitUnary_operator_declarator([NotNull] CSharpParser.Unary_operator_declaratorContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitOverloadable_unary_operator([NotNull] CSharpParser.Overloadable_unary_operatorContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitBinary_operator_declarator([NotNull] CSharpParser.Binary_operator_declaratorContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitOverloadable_binary_operator([NotNull] CSharpParser.Overloadable_binary_operatorContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitConversion_operator_declarator([NotNull] CSharpParser.Conversion_operator_declaratorContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitOperator_body([NotNull] CSharpParser.Operator_bodyContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitConstructor_declaration([NotNull] CSharpParser.Constructor_declarationContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitConstructor_modifier([NotNull] CSharpParser.Constructor_modifierContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitConstructor_declarator([NotNull] CSharpParser.Constructor_declaratorContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitConstructor_initializer([NotNull] CSharpParser.Constructor_initializerContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitConstructor_body([NotNull] CSharpParser.Constructor_bodyContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitStatic_constructor_declaration([NotNull] CSharpParser.Static_constructor_declarationContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitStatic_constructor_modifiers([NotNull] CSharpParser.Static_constructor_modifiersContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitStatic_constructor_body([NotNull] CSharpParser.Static_constructor_bodyContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitDestructor_declaration([NotNull] CSharpParser.Destructor_declarationContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitDestructor_body([NotNull] CSharpParser.Destructor_bodyContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitAll_member_modifiers([NotNull] CSharpParser.All_member_modifiersContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitAll_member_modifier([NotNull] CSharpParser.All_member_modifierContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitStruct_declaration([NotNull] CSharpParser.Struct_declarationContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitStruct_modifier([NotNull] CSharpParser.Struct_modifierContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitStruct_interfaces([NotNull] CSharpParser.Struct_interfacesContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitStruct_body([NotNull] CSharpParser.Struct_bodyContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitStruct_member_declaration([NotNull] CSharpParser.Struct_member_declarationContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitArray_initializer([NotNull] CSharpParser.Array_initializerContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitVariable_initializer_list([NotNull] CSharpParser.Variable_initializer_listContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitInterface_declaration([NotNull] CSharpParser.Interface_declarationContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitInterface_modifier([NotNull] CSharpParser.Interface_modifierContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitVariant_type_parameter_list([NotNull] CSharpParser.Variant_type_parameter_listContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitVariant_type_parameter([NotNull] CSharpParser.Variant_type_parameterContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitVariance_annotation([NotNull] CSharpParser.Variance_annotationContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitInterface_base([NotNull] CSharpParser.Interface_baseContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitInterface_body([NotNull] CSharpParser.Interface_bodyContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitInterface_member_declaration([NotNull] CSharpParser.Interface_member_declarationContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitInterface_method_declaration([NotNull] CSharpParser.Interface_method_declarationContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitInterface_property_declaration([NotNull] CSharpParser.Interface_property_declarationContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitInterface_accessors([NotNull] CSharpParser.Interface_accessorsContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitInterface_event_declaration([NotNull] CSharpParser.Interface_event_declarationContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitInterface_indexer_declaration([NotNull] CSharpParser.Interface_indexer_declarationContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitEnum_declaration([NotNull] CSharpParser.Enum_declarationContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitEnum_modifier([NotNull] CSharpParser.Enum_modifierContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitEnum_base([NotNull] CSharpParser.Enum_baseContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitEnum_body([NotNull] CSharpParser.Enum_bodyContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitEnum_member_declarations([NotNull] CSharpParser.Enum_member_declarationsContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitEnum_member_declaration([NotNull] CSharpParser.Enum_member_declarationContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitDelegate_declaration([NotNull] CSharpParser.Delegate_declarationContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitDelegate_modifier([NotNull] CSharpParser.Delegate_modifierContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitGlobal_attributes([NotNull] CSharpParser.Global_attributesContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitGlobal_attribute_section([NotNull] CSharpParser.Global_attribute_sectionContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitGlobal_attribute_target_specifier([NotNull] CSharpParser.Global_attribute_target_specifierContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitGlobal_attribute_target([NotNull] CSharpParser.Global_attribute_targetContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitAttributes([NotNull] CSharpParser.AttributesContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitAttribute_section([NotNull] CSharpParser.Attribute_sectionContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitAttribute_target_specifier([NotNull] CSharpParser.Attribute_target_specifierContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitAttribute_target([NotNull] CSharpParser.Attribute_targetContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitAttribute_list([NotNull] CSharpParser.Attribute_listContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitAttribute([NotNull] CSharpParser.AttributeContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitAttribute_name([NotNull] CSharpParser.Attribute_nameContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitAttribute_arguments([NotNull] CSharpParser.Attribute_argumentsContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitPositional_argument_list([NotNull] CSharpParser.Positional_argument_listContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitPositional_argument([NotNull] CSharpParser.Positional_argumentContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitNamed_argument_list([NotNull] CSharpParser.Named_argument_listContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitNamed_argument([NotNull] CSharpParser.Named_argumentContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitAttribute_argument_expression([NotNull] CSharpParser.Attribute_argument_expressionContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitClass_modifier_unsafe([NotNull] CSharpParser.Class_modifier_unsafeContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitStruct_modifier_unsafe([NotNull] CSharpParser.Struct_modifier_unsafeContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitInterface_modifier_unsafe([NotNull] CSharpParser.Interface_modifier_unsafeContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitDelegate_modifier_unsafe([NotNull] CSharpParser.Delegate_modifier_unsafeContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitField_modifier_unsafe([NotNull] CSharpParser.Field_modifier_unsafeContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitMethod_modifier_unsafe([NotNull] CSharpParser.Method_modifier_unsafeContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitProperty_modifier_unsafe([NotNull] CSharpParser.Property_modifier_unsafeContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitEvent_modifier_unsafe([NotNull] CSharpParser.Event_modifier_unsafeContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitIndexer_modifier_unsafe([NotNull] CSharpParser.Indexer_modifier_unsafeContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitOperator_modifier_unsafe([NotNull] CSharpParser.Operator_modifier_unsafeContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitConstructor_modifier_unsafe([NotNull] CSharpParser.Constructor_modifier_unsafeContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitDestructor_declaration_unsafe([NotNull] CSharpParser.Destructor_declaration_unsafeContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitStatic_constructor_modifiers_unsafe([NotNull] CSharpParser.Static_constructor_modifiers_unsafeContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitEmbedded_statement_unsafe([NotNull] CSharpParser.Embedded_statement_unsafeContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitUnsafe_statement([NotNull] CSharpParser.Unsafe_statementContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitPointer_type([NotNull] CSharpParser.Pointer_typeContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitUnmanaged_type([NotNull] CSharpParser.Unmanaged_typeContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitUnary_expression_unsafe([NotNull] CSharpParser.Unary_expression_unsafeContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitPointer_indirection_expression([NotNull] CSharpParser.Pointer_indirection_expressionContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitAddressof_expression([NotNull] CSharpParser.Addressof_expressionContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitFixed_statement([NotNull] CSharpParser.Fixed_statementContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitFixed_pointer_declarators([NotNull] CSharpParser.Fixed_pointer_declaratorsContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitFixed_pointer_declarator([NotNull] CSharpParser.Fixed_pointer_declaratorContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitFixed_pointer_initializer([NotNull] CSharpParser.Fixed_pointer_initializerContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitStruct_member_declaration_unsafe([NotNull] CSharpParser.Struct_member_declaration_unsafeContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitFixed_size_buffer_declaration([NotNull] CSharpParser.Fixed_size_buffer_declarationContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitFixed_size_buffer_modifier([NotNull] CSharpParser.Fixed_size_buffer_modifierContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitFixed_size_buffer_modifier_unsafe([NotNull] CSharpParser.Fixed_size_buffer_modifier_unsafeContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitBuffer_element_type([NotNull] CSharpParser.Buffer_element_typeContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitFixed_size_buffer_declarator([NotNull] CSharpParser.Fixed_size_buffer_declaratorContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitStackalloc_initializer([NotNull] CSharpParser.Stackalloc_initializerContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitIdentifier([NotNull] CSharpParser.IdentifierContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitLiteral([NotNull] CSharpParser.LiteralContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitBoolean_literal([NotNull] CSharpParser.Boolean_literalContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitString_literal([NotNull] CSharpParser.String_literalContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitInterpolated_string_expression([NotNull] CSharpParser.Interpolated_string_expressionContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitInterpolated_regular_string([NotNull] CSharpParser.Interpolated_regular_stringContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitInterpolated_verbatim_string([NotNull] CSharpParser.Interpolated_verbatim_stringContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitInterpolated_regular_string_part([NotNull] CSharpParser.Interpolated_regular_string_partContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitInterpolated_verbatim_string_part([NotNull] CSharpParser.Interpolated_verbatim_string_partContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitInterpolated_string_inner_expression([NotNull] CSharpParser.Interpolated_string_inner_expressionContext context)
        {
            VisitChildren(context);
            return null;
        }

        public override object VisitKeyword([NotNull] CSharpParser.KeywordContext context)
        {
            VisitChildren(context);
            return null;
        }


    }
}
