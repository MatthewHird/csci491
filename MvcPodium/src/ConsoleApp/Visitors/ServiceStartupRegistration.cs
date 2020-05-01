using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using MvcPodium.ConsoleApp.Model;
using MvcPodium.ConsoleApp.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using IToken = Antlr4.Runtime.IToken;

namespace MvcPodium.ConsoleApp.Visitors
{
    public class ServiceStartupRegistration : CSharpParserBaseVisitor<object>
    {
        private readonly IStringUtilService _stringUtilService;
        private readonly ICSharpParserService _cSharpParserService;
        private readonly Stack<string> _currentNamespace;
        private readonly Stack<string> _currentClass;

        private readonly ServiceStartupRegistrationArguments _serviceRegistrationArgs;

        private bool _isFoundConfigureServices;

        public bool IsServiceRegistered { get; private set; }

        public TokenStreamRewriter Rewriter { get; }
        public BufferedTokenStream Tokens { get; }

        public ServiceStartupRegistration(
            BufferedTokenStream tokenStream,
            ServiceStartupRegistrationArguments serviceRegistrationArgs,
            IStringUtilService stringUtilService,
            ICSharpParserService cSharpParserService)
        {
            _stringUtilService = stringUtilService;
            _cSharpParserService = cSharpParserService;
            Tokens = tokenStream;
            Rewriter = new TokenStreamRewriter(tokenStream);
            _serviceRegistrationArgs = serviceRegistrationArgs;
            _currentNamespace = new Stack<string>();
            _currentClass = new Stack<string>();
        }

        public override object VisitCompilation_unit([NotNull] CSharpParser.Compilation_unitContext context)
        {
            _isFoundConfigureServices = false;
            IsServiceRegistered = false;

            bool isUsingServiceNamespace = false;

            var usingDirectives = context?.using_directive();

            foreach (var usingDirective in usingDirectives)
            {
                if (usingDirective?.using_directive_inner().GetText() == _serviceRegistrationArgs.ServiceNamespace)
                {
                    isUsingServiceNamespace = true;
                    break;
                }
            }

            if (!isUsingServiceNamespace)
            {
                IToken usingStopIndex = context?.using_directive()?.LastOrDefault()?.Stop
                    ?? context?.extern_alias_directive()?.LastOrDefault()?.Stop
                    ?? context?.BYTE_ORDER_MARK()?.Symbol
                    ?? context.Start;

                var usingDirective = (usingStopIndex.Equals(context.Start) ? "" : "\r\n\r\n")
                    + $"using {_serviceRegistrationArgs.ServiceNamespace};"
                    + (usingStopIndex.Equals(context.Start) ? "\r\n" : "");

                Rewriter.InsertAfter(usingStopIndex, usingDirective);
            }

            VisitChildren(context);
            return null;
        }

        public override object VisitNamespace_declaration([NotNull] CSharpParser.Namespace_declarationContext context)
        {
            _currentNamespace.Push(context.qualified_identifier().GetText());
            VisitChildren(context);
            _ = _currentNamespace.Pop();
            return null;
        }

        public override object VisitClass_declaration([NotNull] CSharpParser.Class_declarationContext context)
        {
            _currentClass.Push(context.identifier().GetText());
                
            if (string.Join(".", _currentClass.ToArray().Reverse()) == "Startup")
            {
                VisitChildren(context);
                if (!_isFoundConfigureServices)
                {
                    // throw
                }
            }
            else
            {
                VisitChildren(context);

            }
            _ = _currentClass.Pop();
            return null;
        }

        public override object VisitClass_member_declaration([NotNull] CSharpParser.Class_member_declarationContext context)
        {
            var method = context?.method_declaration();
            // Find method "public void ConfigureServices(IServiceCollection services)"
            if (method != null 
                && method.method_header().member_name().identifier().GetText() == "ConfigureServices"
                && string.Join(".", _currentClass.ToArray().Reverse()) == "Startup"
                && string.Join(".", _currentNamespace.ToArray().Reverse()) == _serviceRegistrationArgs.RootNamespace
                )
            {
                _isFoundConfigureServices = true;

                var paramList = method?.method_header()?.formal_parameter_list()?.fixed_parameters()?.fixed_parameter();
                if (paramList is null)
                {
                    // throw
                }

                // Get name of IServiceCollection parameter (services)
                string serviceCollectionName = null;
                foreach (var param in paramList)
                {
                    if (param.type_().GetText() == "IServiceCollection")
                    {
                        serviceCollectionName = param.identifier().GetText();
                        break;
                    }
                }

                if (serviceCollectionName is null)
                {
                    // throw
                }

                //  Check each services.Add[Lifespan] method in the form
                //          If TypeParameters:  services.AddScoped(typeof(IMyService<>), typeof(MyService<>));
                //          Else:               services.AddScoped<IMyService, MyService>();
                //      If found and Lifespan in services.Add[Lifespan] == Service.Lifespan:
                //          No action taken
                //      Else if found and Lifespan in services.Add[Lifespan] != Service.Lifespan:
                //          Replace Add[Lifespan] with Add[Service.Lifespan] in token stream
                //      Else:
                //          Create StringTemplate for services.Add[Lifespan] statement
                //              If TypeParameters:  services.AddScoped(typeof(IMyService<>), typeof(MyService<>));
                //              Else:               services.AddScoped<IMyService, MyService>();
                //          Use rewriter to insert services.Add[Lifespan] into token stream
                //          Write parse tree as string to Startup.cs


                var body = _cSharpParserService.GetTextWithWhitespace(method.method_body(), Tokens);

                var matchString = _serviceRegistrationArgs.ServiceRegistrationInfo.HasTypeParameters
                    ? $@"\.\s*Add{_serviceRegistrationArgs.ServiceRegistrationInfo.Scope}\s*" +
                        $@"<\s*I{_serviceRegistrationArgs.ServiceRegistrationInfo.ServiceName}Service\s*,\s*" +
                        $@"{_serviceRegistrationArgs.ServiceRegistrationInfo.ServiceName}Service\s*>\s*\(\s*\)"
                    : $@"\.\s*Add{_serviceRegistrationArgs.ServiceRegistrationInfo.Scope}\s*" +
                        $@"\(\s*typeof\s*\(\s*I{_serviceRegistrationArgs.ServiceRegistrationInfo.ServiceName}" +
                        $@"Service\s*<\s*>\s*\)\s*,\s*typeof\s*\(\s*" +
                        $@"{_serviceRegistrationArgs.ServiceRegistrationInfo.ServiceName}Service\s*<\s*>\s*\)\s*\)";

                var match = Regex.Match(body, matchString);

                if (match.Success)
                {
                    IsServiceRegistered = true;
                    return null;
                }

                if (method?.method_body()?.block() is null)
                {
                    // throw
                }
                else
                {
                    var closeBranceIndex = method.method_body().block().CLOSE_BRACE().Symbol.TokenIndex;

                    string tabString = "    ";
                    var preclassWhitespace = Tokens.GetHiddenTokensToLeft(closeBranceIndex, Lexer.Hidden);

                    int tabLevels = preclassWhitespace.Count > 0 ?
                        _stringUtilService.CalculateTabLevels(preclassWhitespace[0].Text, tabString) : 0;

                    var registrationStatement = 
                        _stringUtilService.TabString(
                            serviceCollectionName + _serviceRegistrationArgs.StartupRegistrationCall, 1, tabString)
                        + "\r\n"
                        + _stringUtilService.TabString("", tabLevels, tabString);

                    Rewriter.InsertBefore(closeBranceIndex, registrationStatement);
                    return null;
                }
            }

            VisitChildren(context);
            return null;
        }
    }
}
