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

preprocessor_directive returns [bool value]
    : SHARP inner=preprocessor_directive_inner (DIRECTIVE_NEWLINE | EOF) 
{
Conditions.Push(true);
ConditionalSymbols.Add("DEBUG");
$value = $inner.value;
};

preprocessor_directive_inner returns [bool value]
    : DEFINE CONDITIONAL_SYMBOL 
{
ConditionalSymbols.Add($CONDITIONAL_SYMBOL.text);
$value = AllConditions();
} #preprocessorDeclaration

    | UNDEF CONDITIONAL_SYMBOL 
{
ConditionalSymbols.Remove($CONDITIONAL_SYMBOL.text);
$value = AllConditions();
} #preprocessorDeclaration

    | IF expr=preprocessor_expression 
{
$value = $expr.value.Equals("true") && AllConditions();
Conditions.Push($expr.value.Equals("true"));
} #preprocessorConditional

    | ELIF expr=preprocessor_expression
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
} #preprocessorConditional

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
} #preprocessorConditional

    | ENDIF             
{
Conditions.Pop();
$value = Conditions.Peek();
} #preprocessorConditional
    
    | LINE (DIGITS STRING? | DEFAULT | DIRECTIVE_HIDDEN)    { $value = AllConditions(); } #preprocessorLine

    | ERROR TEXT                                            { $value = AllConditions(); } #preprocessorDiagnostic

    | WARNING TEXT                                          { $value = AllConditions(); } #preprocessorDiagnostic

    | REGION TEXT?                                          { $value = AllConditions(); } #preprocessorRegion

    | ENDREGION TEXT?                                       { $value = AllConditions(); } #preprocessorRegion

    | PRAGMA TEXT                                           { $value = AllConditions(); } #preprocessorPragma
    ;

preprocessor_expression returns [String value]
    : TRUE
        { $value = "true"; }
    | FALSE
        { $value = "false"; }
    | CONDITIONAL_SYMBOL
        { $value = ConditionalSymbols.Contains($CONDITIONAL_SYMBOL.text) ? "true" : "false"; }
    | OPEN_PARENS expr=preprocessor_expression CLOSE_PARENS
        { $value = $expr.value; }
    | BANG expr=preprocessor_expression
        { $value = $expr.value.Equals("true") ? "false" : "true"; }
    | expr1=preprocessor_expression OP_EQ expr2=preprocessor_expression
        { $value = ($expr1.value == $expr2.value ? "true" : "false"); }
    | expr1=preprocessor_expression OP_NE expr2=preprocessor_expression
        { $value = ($expr1.value != $expr2.value ? "true" : "false"); }
    | expr1=preprocessor_expression OP_AND expr2=preprocessor_expression
        { $value = ($expr1.value.Equals("true") && $expr2.value.Equals("true") ? "true" : "false"); }
    | expr1=preprocessor_expression OP_OR expr2=preprocessor_expression
        { $value = ($expr1.value.Equals("true") || $expr2.value.Equals("true") ? "true" : "false"); }
    ;
