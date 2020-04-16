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
    public class ServiceInterfaceScraper //: CSharpParserBaseVisitor<object>
    {
        private readonly IOptions<AppSettings> _appSettings;
        private readonly IOptions<UserSettings> _userSettings;
        private readonly IStringTemplateService _stringTemplateService;

        public HashSet<string> Imports { set; get; } = new HashSet<string>();
        public TokenStreamRewriter Rewriter { get; }
        public BufferedTokenStream Tokens { get; }

        public ServiceInterfaceScraper(
            BufferedTokenStream tokenStream,
            IOptions<AppSettings> appSettings,
            IOptions<UserSettings> userSettings,
            IStringTemplateService stringTemplateService)
        {
            _appSettings = appSettings;
            _userSettings = userSettings;
            _stringTemplateService = stringTemplateService;
            Tokens = tokenStream;
            Rewriter = new TokenStreamRewriter(tokenStream);
        }

    }
}
