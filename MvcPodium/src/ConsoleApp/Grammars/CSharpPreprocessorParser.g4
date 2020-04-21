// Eclipse Public License - v 1.0, http://www.eclipse.org/legal/epl-v10.html
// Copyright (c) 2013, Christian Wulf (chwchw@gmx.de)
// Copyright (c) 2016-2017, Ivan Kochurkin (kvanttt@gmail.com), Positive Technologies.

parser grammar CSharpPreprocessorParser;

options { tokenVocab=CSharpLexer; }

@parser::header {}

@parser::members
{
public Stack<bool> Conditions { get; set; } = new Stack<bool>();
public HashSet<string> ConditionalSymbols { get; set; } = new HashSet<string>(); 

private bool AllConditions() {
    foreach(bool condition in Conditions) {
        if (!condition)
        {
            return false;
        }
    }
    return true;
}
}

pp_directive returns [bool value]
    : SHARP inner=pp_directive_inner (DIRECTIVE_NEWLINE | EOF) 
{
Conditions.Push(true);
ConditionalSymbols.Add("DEBUG");
$value = $inner.value;
};

pp_directive_inner returns [bool value]
    : DEFINE CONDITIONAL_SYMBOL 
{
ConditionalSymbols.Add($CONDITIONAL_SYMBOL.text);
$value = AllConditions();
} #ppDeclaration

    | UNDEF CONDITIONAL_SYMBOL 
{
ConditionalSymbols.Remove($CONDITIONAL_SYMBOL.text);
$value = AllConditions();
} #ppDeclaration

    | IF expr=pp_expression 
{
$value = $expr.value.Equals("true") && AllConditions();
Conditions.Push($expr.value.Equals("true"));
} #ppConditional

    | ELIF expr=pp_expression
{
if (!Conditions.Peek())
{
    Conditions.Pop(); 
    $value = $expr.value.Equals("true") && AllConditions();
    Conditions.Push($expr.value.Equals("true"));
} 
else 
{
$value = false;
}
} #ppConditional

    | ELSE
{
if (!Conditions.Peek())
{
Conditions.Pop();
$value = true && AllConditions();
Conditions.Push(true);
}
else 
{
$value = false;
}
} #ppConditional

    | ENDIF             
{
Conditions.Pop();
$value = Conditions.Peek();
} #ppConditional
    
    | LINE (DIGITS STRING? | DEFAULT | DIRECTIVE_HIDDEN)    { $value = AllConditions(); } #ppLine

    | ERROR TEXT                                            { $value = AllConditions(); } #ppDiagnostic

    | WARNING TEXT                                          { $value = AllConditions(); } #ppDiagnostic

    | REGION TEXT?                                          { $value = AllConditions(); } #ppRegion

    | ENDREGION TEXT?                                       { $value = AllConditions(); } #ppRegion

    | PRAGMA TEXT                                           { $value = AllConditions(); } #ppPragma

    | DIRECTIVE_NULLABLE DIRECTIVE_DISABLE       { $value = AllConditions(); } #ppNullable

    | DIRECTIVE_NULLABLE DIRECTIVE_ENABLE        { $value = AllConditions(); } #ppNullable

    | DIRECTIVE_NULLABLE DIRECTIVE_RESTORE       { $value = AllConditions(); } #ppNullable
    ;

pp_expression returns [String value]
    : TRUE
        { $value = "true"; } #ppPrimaryExpression
    | FALSE
        { $value = "false"; } #ppPrimaryExpression
    | CONDITIONAL_SYMBOL
        { $value = ConditionalSymbols.Contains($CONDITIONAL_SYMBOL.text) ? "true" : "false"; } #ppPrimaryExpression
    | OPEN_PARENS expr=pp_expression CLOSE_PARENS
        { $value = $expr.value; } #ppPrimaryExpression
    | BANG expr=pp_expression
        { $value = $expr.value.Equals("true") ? "false" : "true"; } #ppUnaryExpression
    | expr1=pp_expression OP_EQ expr2=pp_expression
        { $value = ($expr1.value == $expr2.value ? "true" : "false"); } #ppEqualityExpression
    | expr1=pp_expression OP_NE expr2=pp_expression
        { $value = ($expr1.value != $expr2.value ? "true" : "false"); } #ppEqualityExpression
    | expr1=pp_expression OP_AND expr2=pp_expression
        { $value = ($expr1.value.Equals("true") && $expr2.value.Equals("true") ? "true" : "false"); } #ppAndExpression
    | expr1=pp_expression OP_OR expr2=pp_expression
        { $value = ($expr1.value.Equals("true") || $expr2.value.Equals("true") ? "true" : "false"); } #ppOrExpression
    ;
