using Antlr4.Runtime;

namespace MvcPodium.ConsoleApp
{
    public class CSharpParserWrapper
    {
        public CommonTokenStream Tokens { get; }

        public CSharpParser Parser { get; }

        public CSharpParserWrapper(string filepath)
        {
            var charStream = CharStreams.fromPath(filepath);
            var lexer = new CSharpLexer(charStream);
            Tokens = new CommonTokenStream(lexer);
            Parser = new CSharpParser(Tokens);
            Parser.BuildParseTree = true;
        }

        public CSharpParser.Compilation_unitContext GetParseTree()
        {
            return Parser.compilation_unit();
        }
    }
}
