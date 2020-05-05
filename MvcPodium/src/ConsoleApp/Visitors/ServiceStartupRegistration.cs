using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using MvcPodium.ConsoleApp.Models.ServiceCommand;
using MvcPodium.ConsoleApp.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace MvcPodium.ConsoleApp.Visitors
{
    public class ServiceStartupRegistration : CSharpParserBaseVisitor<object>
    {
        private readonly IStringUtilService _stringUtilService;
        private readonly ICSharpParserService _cSharpParserService;
        private readonly IServiceCommandStgService _serviceCommandStgService;
        private readonly Stack<string> _currentNamespace;
        private readonly Stack<string> _currentClass;

        private readonly string _rootNamespace;
        private readonly string _serviceNamespace;
        private readonly string _serviceName;
        private readonly bool _hasTypeParameters;
        private readonly string _serviceLifespan;
        private readonly string _tabString;

        private bool _isFoundConfigureServices;

        public bool IsServiceRegistered { get; private set; }

        public TokenStreamRewriter Rewriter { get; }
        public BufferedTokenStream Tokens { get; }

        public ServiceStartupRegistration(
            IStringUtilService stringUtilService,
            ICSharpParserService cSharpParserService,
            IServiceCommandStgService serviceCommandStgService,
            BufferedTokenStream tokenStream,
            string rootNamespace,
            string serviceNamespace,
            string serviceName,
            bool hasTypeParameters,
            string serviceLifespan,
            string tabString = null)
        {
            _stringUtilService = stringUtilService;
            _cSharpParserService = cSharpParserService;
            _serviceCommandStgService = serviceCommandStgService;
            Tokens = tokenStream;
            Rewriter = new TokenStreamRewriter(tokenStream);
            _serviceNamespace = serviceNamespace;
            _serviceName = serviceName;
            _hasTypeParameters = hasTypeParameters;
            _rootNamespace = rootNamespace;
            _serviceLifespan = serviceLifespan;
            _tabString = tabString;
            _currentNamespace = new Stack<string>();
            _currentClass = new Stack<string>();
        }

        public override object VisitCompilation_unit([NotNull] CSharpParser.Compilation_unitContext context)
        {
            _isFoundConfigureServices = false;
            IsServiceRegistered = false;

            bool isUsingServiceNamespace = _cSharpParserService.IsUsingDirectiveInContext(
                context,
                _serviceNamespace);

            if (!isUsingServiceNamespace)
            {
                var usingStopIndex = _cSharpParserService.GetUsingStopIndex(context);

                var usingDirectiveStr = _cSharpParserService.GenerateUsingDirective(
                    _serviceNamespace,
                    usingStopIndex.Equals(context.Start));

                Rewriter.InsertAfter(usingStopIndex, usingDirectiveStr);
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
                && string.Join(".", _currentNamespace.ToArray().Reverse()) == _rootNamespace
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

                var body = _cSharpParserService.GetTextWithWhitespace(Tokens, method.method_body());

                var match = Regex.Match(body, ServiceRegistrationMatchString());

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

                    var preclassWhitespace = Tokens.GetHiddenTokensToLeft(closeBranceIndex, Lexer.Hidden);

                    int tabLevels = preclassWhitespace.Count > 0 ?
                        _stringUtilService.CalculateTabLevels(preclassWhitespace[0].Text, _tabString) : 0;

                    var startupRegistrationCall = _serviceCommandStgService.RenderServiceStartupRegistrationCall(
                        serviceName: _serviceName,
                        hasTypeParameters: _hasTypeParameters,
                        serviceLifespan: _serviceLifespan);

                    var registrationStatement = 
                        _stringUtilService.TabString(
                            serviceCollectionName + startupRegistrationCall, 1, _tabString)
                        + "\r\n"
                        + _stringUtilService.TabString(string.Empty, tabLevels, _tabString);

                    Rewriter.InsertBefore(closeBranceIndex, registrationStatement);
                    return null;
                }
            }

            VisitChildren(context);
            return null;
        }

        private string ServiceRegistrationMatchString()
        {
            return _hasTypeParameters
                ? $@"\.\s*Add{_serviceLifespan}\s*\(\s*typeof\s*\(\s*I{_serviceName}" +
                    $@"Service\s*<\s*>\s*\)\s*,\s*typeof\s*\(\s*{_serviceName}Service\s*<\s*>\s*\)\s*\)"
                : $@"\.\s*Add{_serviceLifespan}\s*<\s*I{_serviceName}Service\s*,\s*{_serviceName}Service\s*>\s*\(\s*\)";
        }

    }
}
