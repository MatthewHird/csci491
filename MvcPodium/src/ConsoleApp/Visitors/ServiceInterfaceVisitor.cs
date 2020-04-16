using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using Antlr4.StringTemplate;
using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using IToken = Antlr4.Runtime.IToken;
using ParserRuleContext = Antlr4.Runtime.ParserRuleContext;

namespace MvcPodium.ConsoleApp.Visitors
{
    class ServiceInterfaceVisitor //: CSharpParserBaseVisitor<object>
    {
        public HashSet<string> Imports { set; get; } = new HashSet<string>();

        public TokenStreamRewriter Rewriter { get; }

        public BufferedTokenStream Tokens { get; }

        public ServiceInterfaceVisitor(BufferedTokenStream tokenStream)
        {
            Rewriter = new TokenStreamRewriter(tokenStream);
            Tokens = tokenStream;
        }

        //public override object VisitCompilation_unit([NotNull] CSharpParser.Compilation_unitContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitNamespace_or_type_name([NotNull] CSharpParser.Namespace_or_type_nameContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitType_([NotNull] CSharpParser.Type_Context context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitBase_type([NotNull] CSharpParser.Base_typeContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitSimple_type([NotNull] CSharpParser.Simple_typeContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitNumeric_type([NotNull] CSharpParser.Numeric_typeContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitIntegral_type([NotNull] CSharpParser.Integral_typeContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitFloating_point_type([NotNull] CSharpParser.Floating_point_typeContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitClass_type([NotNull] CSharpParser.Class_typeContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitType_argument_list([NotNull] CSharpParser.Type_argument_listContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitArgument_list([NotNull] CSharpParser.Argument_listContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitArgument([NotNull] CSharpParser.ArgumentContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitExpression([NotNull] CSharpParser.ExpressionContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitNon_assignment_expression([NotNull] CSharpParser.Non_assignment_expressionContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitAssignment([NotNull] CSharpParser.AssignmentContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitAssignment_operator([NotNull] CSharpParser.Assignment_operatorContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitConditional_expression([NotNull] CSharpParser.Conditional_expressionContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitNull_coalescing_expression([NotNull] CSharpParser.Null_coalescing_expressionContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitConditional_or_expression([NotNull] CSharpParser.Conditional_or_expressionContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitConditional_and_expression([NotNull] CSharpParser.Conditional_and_expressionContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitInclusive_or_expression([NotNull] CSharpParser.Inclusive_or_expressionContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitExclusive_or_expression([NotNull] CSharpParser.Exclusive_or_expressionContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitAnd_expression([NotNull] CSharpParser.And_expressionContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitEquality_expression([NotNull] CSharpParser.Equality_expressionContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitRelational_expression([NotNull] CSharpParser.Relational_expressionContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitShift_expression([NotNull] CSharpParser.Shift_expressionContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitAdditive_expression([NotNull] CSharpParser.Additive_expressionContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitMultiplicative_expression([NotNull] CSharpParser.Multiplicative_expressionContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitUnary_expression([NotNull] CSharpParser.Unary_expressionContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitPrimary_expression([NotNull] CSharpParser.Primary_expressionContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitLiteralExpression([NotNull] CSharpParser.LiteralExpressionContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitSimpleNameExpression([NotNull] CSharpParser.SimpleNameExpressionContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitParenthesisExpressions([NotNull] CSharpParser.ParenthesisExpressionsContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitMemberAccessExpression([NotNull] CSharpParser.MemberAccessExpressionContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitLiteralAccessExpression([NotNull] CSharpParser.LiteralAccessExpressionContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitThisReferenceExpression([NotNull] CSharpParser.ThisReferenceExpressionContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitBaseAccessExpression([NotNull] CSharpParser.BaseAccessExpressionContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitObjectCreationExpression([NotNull] CSharpParser.ObjectCreationExpressionContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitTypeofExpression([NotNull] CSharpParser.TypeofExpressionContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitCheckedExpression([NotNull] CSharpParser.CheckedExpressionContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitUncheckedExpression([NotNull] CSharpParser.UncheckedExpressionContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitDefaultValueExpression([NotNull] CSharpParser.DefaultValueExpressionContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitAnonymousMethodExpression([NotNull] CSharpParser.AnonymousMethodExpressionContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitSizeofExpression([NotNull] CSharpParser.SizeofExpressionContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitNameofExpression([NotNull] CSharpParser.NameofExpressionContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitMember_access([NotNull] CSharpParser.Member_accessContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitBracket_expression([NotNull] CSharpParser.Bracket_expressionContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitIndexer_argument([NotNull] CSharpParser.Indexer_argumentContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitPredefined_type([NotNull] CSharpParser.Predefined_typeContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitExpression_list([NotNull] CSharpParser.Expression_listContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitObject_or_collection_initializer([NotNull] CSharpParser.Object_or_collection_initializerContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitObject_initializer([NotNull] CSharpParser.Object_initializerContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitMember_initializer_list([NotNull] CSharpParser.Member_initializer_listContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitMember_initializer([NotNull] CSharpParser.Member_initializerContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitInitializer_value([NotNull] CSharpParser.Initializer_valueContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitCollection_initializer([NotNull] CSharpParser.Collection_initializerContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitElement_initializer([NotNull] CSharpParser.Element_initializerContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitAnonymous_object_initializer([NotNull] CSharpParser.Anonymous_object_initializerContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitMember_declarator_list([NotNull] CSharpParser.Member_declarator_listContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitMember_declarator([NotNull] CSharpParser.Member_declaratorContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitUnbound_type_name([NotNull] CSharpParser.Unbound_type_nameContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitGeneric_dimension_specifier([NotNull] CSharpParser.Generic_dimension_specifierContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitIsType([NotNull] CSharpParser.IsTypeContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitLambda_expression([NotNull] CSharpParser.Lambda_expressionContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitAnonymous_function_signature([NotNull] CSharpParser.Anonymous_function_signatureContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitExplicit_anonymous_function_parameter_list([NotNull] CSharpParser.Explicit_anonymous_function_parameter_listContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitExplicit_anonymous_function_parameter([NotNull] CSharpParser.Explicit_anonymous_function_parameterContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitImplicit_anonymous_function_parameter_list([NotNull] CSharpParser.Implicit_anonymous_function_parameter_listContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitAnonymous_function_body([NotNull] CSharpParser.Anonymous_function_bodyContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitQuery_expression([NotNull] CSharpParser.Query_expressionContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitFrom_clause([NotNull] CSharpParser.From_clauseContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitQuery_body([NotNull] CSharpParser.Query_bodyContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitQuery_body_clause([NotNull] CSharpParser.Query_body_clauseContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitLet_clause([NotNull] CSharpParser.Let_clauseContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitWhere_clause([NotNull] CSharpParser.Where_clauseContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitCombined_join_clause([NotNull] CSharpParser.Combined_join_clauseContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitOrderby_clause([NotNull] CSharpParser.Orderby_clauseContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitOrdering([NotNull] CSharpParser.OrderingContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitSelect_or_group_clause([NotNull] CSharpParser.Select_or_group_clauseContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitQuery_continuation([NotNull] CSharpParser.Query_continuationContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitLabeledStatement([NotNull] CSharpParser.LabeledStatementContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitDeclarationStatement([NotNull] CSharpParser.DeclarationStatementContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitEmbeddedStatement([NotNull] CSharpParser.EmbeddedStatementContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitLabeled_Statement([NotNull] CSharpParser.Labeled_StatementContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitEmbedded_statement([NotNull] CSharpParser.Embedded_statementContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitTheEmptyStatement([NotNull] CSharpParser.TheEmptyStatementContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitExpressionStatement([NotNull] CSharpParser.ExpressionStatementContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitIfStatement([NotNull] CSharpParser.IfStatementContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitSwitchStatement([NotNull] CSharpParser.SwitchStatementContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitWhileStatement([NotNull] CSharpParser.WhileStatementContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitDoStatement([NotNull] CSharpParser.DoStatementContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitForStatement([NotNull] CSharpParser.ForStatementContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitForeachStatement([NotNull] CSharpParser.ForeachStatementContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitBreakStatement([NotNull] CSharpParser.BreakStatementContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitContinueStatement([NotNull] CSharpParser.ContinueStatementContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitGotoStatement([NotNull] CSharpParser.GotoStatementContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitReturnStatement([NotNull] CSharpParser.ReturnStatementContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitThrowStatement([NotNull] CSharpParser.ThrowStatementContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitTryStatement([NotNull] CSharpParser.TryStatementContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitCheckedStatement([NotNull] CSharpParser.CheckedStatementContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitUncheckedStatement([NotNull] CSharpParser.UncheckedStatementContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitLockStatement([NotNull] CSharpParser.LockStatementContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitUsingStatement([NotNull] CSharpParser.UsingStatementContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitYieldStatement([NotNull] CSharpParser.YieldStatementContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitUnsafeStatement([NotNull] CSharpParser.UnsafeStatementContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitFixedStatement([NotNull] CSharpParser.FixedStatementContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitBlock([NotNull] CSharpParser.BlockContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitLocal_variable_declaration([NotNull] CSharpParser.Local_variable_declarationContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitLocal_variable_type([NotNull] CSharpParser.Local_variable_typeContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitLocal_variable_declarator([NotNull] CSharpParser.Local_variable_declaratorContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitLocal_variable_initializer([NotNull] CSharpParser.Local_variable_initializerContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitLocal_constant_declaration([NotNull] CSharpParser.Local_constant_declarationContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitIf_body([NotNull] CSharpParser.If_bodyContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitSwitch_section([NotNull] CSharpParser.Switch_sectionContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitSwitch_label([NotNull] CSharpParser.Switch_labelContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitStatement_list([NotNull] CSharpParser.Statement_listContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitFor_initializer([NotNull] CSharpParser.For_initializerContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitFor_iterator([NotNull] CSharpParser.For_iteratorContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitCatch_clauses([NotNull] CSharpParser.Catch_clausesContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitSpecific_catch_clause([NotNull] CSharpParser.Specific_catch_clauseContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitGeneral_catch_clause([NotNull] CSharpParser.General_catch_clauseContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitException_filter([NotNull] CSharpParser.Exception_filterContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitFinally_clause([NotNull] CSharpParser.Finally_clauseContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitResource_acquisition([NotNull] CSharpParser.Resource_acquisitionContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitNamespace_declaration([NotNull] CSharpParser.Namespace_declarationContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitQualified_identifier([NotNull] CSharpParser.Qualified_identifierContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitNamespace_body([NotNull] CSharpParser.Namespace_bodyContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitExtern_alias_directives([NotNull] CSharpParser.Extern_alias_directivesContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitExtern_alias_directive([NotNull] CSharpParser.Extern_alias_directiveContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitUsing_directives([NotNull] CSharpParser.Using_directivesContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitUsingAliasDirective([NotNull] CSharpParser.UsingAliasDirectiveContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitUsingNamespaceDirective([NotNull] CSharpParser.UsingNamespaceDirectiveContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitUsingStaticDirective([NotNull] CSharpParser.UsingStaticDirectiveContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitNamespace_member_declarations([NotNull] CSharpParser.Namespace_member_declarationsContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitNamespace_member_declaration([NotNull] CSharpParser.Namespace_member_declarationContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitType_declaration([NotNull] CSharpParser.Type_declarationContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitQualified_alias_member([NotNull] CSharpParser.Qualified_alias_memberContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitType_parameter_list([NotNull] CSharpParser.Type_parameter_listContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitType_parameter([NotNull] CSharpParser.Type_parameterContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitClass_base([NotNull] CSharpParser.Class_baseContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitInterface_type_list([NotNull] CSharpParser.Interface_type_listContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitType_parameter_constraints_clauses([NotNull] CSharpParser.Type_parameter_constraints_clausesContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitType_parameter_constraints_clause([NotNull] CSharpParser.Type_parameter_constraints_clauseContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitType_parameter_constraints([NotNull] CSharpParser.Type_parameter_constraintsContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitPrimary_constraint([NotNull] CSharpParser.Primary_constraintContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitSecondary_constraints([NotNull] CSharpParser.Secondary_constraintsContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitConstructor_constraint([NotNull] CSharpParser.Constructor_constraintContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitClass_body([NotNull] CSharpParser.Class_bodyContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitClass_member_declarations([NotNull] CSharpParser.Class_member_declarationsContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitClass_member_declaration([NotNull] CSharpParser.Class_member_declarationContext context)
        //{
        //    var x = context?.common_member_declaration()?.typed_member_declaration()?.property_declaration();
        //    if (x != null && x.member_name().GetText() == "Photo")
        //    {
        //        var servicesGroupFile = new TemplateGroupFile(@"D:\Files HDD\Workspace\csci491\MvcPodium\Resources\StringTemplates\Services.stg");
        //        var serviceClassSt = servicesGroupFile.GetInstanceOf("ClassMethod");
        //        //identifier, modifiers, returnType, genericTypeParams, args
        //        serviceClassSt.Add("identifier", "MyMethodAsync");
        //        serviceClassSt.Add("modifiers", new List<string> { "public", "async" });
        //        serviceClassSt.Add("returnType", "Task<int>");
        //        serviceClassSt.Add("args", new List<object> { new { Type = "int", Identifier = "Arg1" }, new { Type = "string", Identifier = "Arg2" } });

        //        int i = context.Start.TokenIndex;
        //        IList<IToken> whitespaceChannel = Tokens.GetHiddenTokensToLeft(i, Lexer.Hidden);

        //        string whitespace = "";
        //        string newlines = "";
        //        string spaces = "";

        //        //foreach (var whiteToken in whitespaceChannel)
        //        //{
        //        //    whitespace += whiteToken.Text;
        //        //}

        //        if (whitespaceChannel.Count > 0)
        //        {
        //            whitespace = whitespaceChannel[0].Text;
        //            int lastNewlineIndex = whitespace.LastIndexOf("\n");
        //            if (lastNewlineIndex >= 0)
        //            {
        //                newlines = whitespace.Substring(0, lastNewlineIndex + 1);
        //                spaces = whitespace.Substring(lastNewlineIndex + 1);
        //            }
        //        }

        //        string serviceClass = serviceClassSt.Render();

        //        if (spaces != "" && newlines != "")
        //        {
        //            serviceClass = Regex.Replace(serviceClass, @"\r?\n", "\r\n" + spaces);
        //        }

        //        Rewriter.InsertAfter(x.Stop, whitespace + serviceClass);
        //    }

        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitAll_member_modifiers([NotNull] CSharpParser.All_member_modifiersContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitAll_member_modifier([NotNull] CSharpParser.All_member_modifierContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitCommon_member_declaration([NotNull] CSharpParser.Common_member_declarationContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitTyped_member_declaration([NotNull] CSharpParser.Typed_member_declarationContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitConstant_declarators([NotNull] CSharpParser.Constant_declaratorsContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitConstant_declarator([NotNull] CSharpParser.Constant_declaratorContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitVariable_declarators([NotNull] CSharpParser.Variable_declaratorsContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitVariable_declarator([NotNull] CSharpParser.Variable_declaratorContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitVariable_initializer([NotNull] CSharpParser.Variable_initializerContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitReturn_type([NotNull] CSharpParser.Return_typeContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitMember_name([NotNull] CSharpParser.Member_nameContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitMethod_body([NotNull] CSharpParser.Method_bodyContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitFormal_parameter_list([NotNull] CSharpParser.Formal_parameter_listContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitFixed_parameters([NotNull] CSharpParser.Fixed_parametersContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitFixed_parameter([NotNull] CSharpParser.Fixed_parameterContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitParameter_modifier([NotNull] CSharpParser.Parameter_modifierContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitParameter_array([NotNull] CSharpParser.Parameter_arrayContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitAccessor_declarations([NotNull] CSharpParser.Accessor_declarationsContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitGet_accessor_declaration([NotNull] CSharpParser.Get_accessor_declarationContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitSet_accessor_declaration([NotNull] CSharpParser.Set_accessor_declarationContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitAccessor_modifier([NotNull] CSharpParser.Accessor_modifierContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitAccessor_body([NotNull] CSharpParser.Accessor_bodyContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitEvent_accessor_declarations([NotNull] CSharpParser.Event_accessor_declarationsContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitAdd_accessor_declaration([NotNull] CSharpParser.Add_accessor_declarationContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitRemove_accessor_declaration([NotNull] CSharpParser.Remove_accessor_declarationContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitOverloadable_operator([NotNull] CSharpParser.Overloadable_operatorContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitConversion_operator_declarator([NotNull] CSharpParser.Conversion_operator_declaratorContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitConstructor_initializer([NotNull] CSharpParser.Constructor_initializerContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitBody([NotNull] CSharpParser.BodyContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitStruct_interfaces([NotNull] CSharpParser.Struct_interfacesContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitStruct_body([NotNull] CSharpParser.Struct_bodyContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitStruct_member_declaration([NotNull] CSharpParser.Struct_member_declarationContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitArray_type([NotNull] CSharpParser.Array_typeContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitRank_specifier([NotNull] CSharpParser.Rank_specifierContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitArray_initializer([NotNull] CSharpParser.Array_initializerContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitVariant_type_parameter_list([NotNull] CSharpParser.Variant_type_parameter_listContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitVariant_type_parameter([NotNull] CSharpParser.Variant_type_parameterContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitVariance_annotation([NotNull] CSharpParser.Variance_annotationContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitInterface_base([NotNull] CSharpParser.Interface_baseContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitInterface_body([NotNull] CSharpParser.Interface_bodyContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitInterface_member_declaration([NotNull] CSharpParser.Interface_member_declarationContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitInterface_accessors([NotNull] CSharpParser.Interface_accessorsContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitEnum_base([NotNull] CSharpParser.Enum_baseContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitEnum_body([NotNull] CSharpParser.Enum_bodyContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitEnum_member_declaration([NotNull] CSharpParser.Enum_member_declarationContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitGlobal_attribute_section([NotNull] CSharpParser.Global_attribute_sectionContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitGlobal_attribute_target([NotNull] CSharpParser.Global_attribute_targetContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitAttributes([NotNull] CSharpParser.AttributesContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitAttribute_section([NotNull] CSharpParser.Attribute_sectionContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitAttribute_target([NotNull] CSharpParser.Attribute_targetContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitAttribute_list([NotNull] CSharpParser.Attribute_listContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitAttribute([NotNull] CSharpParser.AttributeContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitAttribute_argument([NotNull] CSharpParser.Attribute_argumentContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitPointer_type([NotNull] CSharpParser.Pointer_typeContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitFixed_pointer_declarators([NotNull] CSharpParser.Fixed_pointer_declaratorsContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitFixed_pointer_declarator([NotNull] CSharpParser.Fixed_pointer_declaratorContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitFixed_pointer_initializer([NotNull] CSharpParser.Fixed_pointer_initializerContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitFixed_size_buffer_declarator([NotNull] CSharpParser.Fixed_size_buffer_declaratorContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitLocal_variable_initializer_unsafe([NotNull] CSharpParser.Local_variable_initializer_unsafeContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitLiteral([NotNull] CSharpParser.LiteralContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitBoolean_literal([NotNull] CSharpParser.Boolean_literalContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitString_literal([NotNull] CSharpParser.String_literalContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitInterpolated_regular_string([NotNull] CSharpParser.Interpolated_regular_stringContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitInterpolated_verbatim_string([NotNull] CSharpParser.Interpolated_verbatim_stringContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitInterpolated_regular_string_part([NotNull] CSharpParser.Interpolated_regular_string_partContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitInterpolated_verbatim_string_part([NotNull] CSharpParser.Interpolated_verbatim_string_partContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitInterpolated_string_expression([NotNull] CSharpParser.Interpolated_string_expressionContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitKeyword([NotNull] CSharpParser.KeywordContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitClass_definition([NotNull] CSharpParser.Class_definitionContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitStruct_definition([NotNull] CSharpParser.Struct_definitionContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitInterface_definition([NotNull] CSharpParser.Interface_definitionContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitEnum_definition([NotNull] CSharpParser.Enum_definitionContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitDelegate_definition([NotNull] CSharpParser.Delegate_definitionContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitEvent_declaration([NotNull] CSharpParser.Event_declarationContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitField_declaration([NotNull] CSharpParser.Field_declarationContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitProperty_declaration([NotNull] CSharpParser.Property_declarationContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitConstant_declaration([NotNull] CSharpParser.Constant_declarationContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitIndexer_declaration([NotNull] CSharpParser.Indexer_declarationContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitDestructor_definition([NotNull] CSharpParser.Destructor_definitionContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitConstructor_declaration([NotNull] CSharpParser.Constructor_declarationContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitMethod_declaration([NotNull] CSharpParser.Method_declarationContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitMethod_member_name([NotNull] CSharpParser.Method_member_nameContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitOperator_declaration([NotNull] CSharpParser.Operator_declarationContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitArg_declaration([NotNull] CSharpParser.Arg_declarationContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitMethod_invocation([NotNull] CSharpParser.Method_invocationContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitObject_creation_expression([NotNull] CSharpParser.Object_creation_expressionContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


        //public override object VisitIdentifier([NotNull] CSharpParser.IdentifierContext context)
        //{
        //    VisitChildren(context);
        //    return null;
        //}


    }
}
