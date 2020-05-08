using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Microsoft.Extensions.DependencyInjection;
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
        private readonly List<StartupRegistrationInfo> _startupRegInfoList;
        private readonly string _tabString;


        private bool _isFoundConfigureServices;

        public bool IsModified { get; private set; }

        public TokenStreamRewriter Rewriter { get; }
        public BufferedTokenStream Tokens { get; }

        public ServiceStartupRegistration(
            IStringUtilService stringUtilService,
            ICSharpParserService cSharpParserService,
            IServiceCommandStgService serviceCommandStgService,
            BufferedTokenStream tokenStream,
            string rootNamespace,
            List<StartupRegistrationInfo> startupRegInfoList,
            string tabString = null)
        {
            _stringUtilService = stringUtilService;
            _cSharpParserService = cSharpParserService;
            _serviceCommandStgService = serviceCommandStgService;
            Tokens = tokenStream;
            Rewriter = new TokenStreamRewriter(tokenStream);
            _rootNamespace = rootNamespace;
            _startupRegInfoList = startupRegInfoList;
            _tabString = tabString;
            _currentNamespace = new Stack<string>();
            _currentClass = new Stack<string>();
            IsModified = false;
        }

        public override object VisitCompilation_unit([NotNull] CSharpParser.Compilation_unitContext context)
        {
            _isFoundConfigureServices = false;

            var usingNamespaceList = new List<string>();

            foreach (var regInfo in _startupRegInfoList)
            {
                usingNamespaceList.Add(regInfo.ServiceNamespace);
            }

            var missingUsingDirectives = _cSharpParserService.GetUsingDirectivesNotInContext(
                context, usingNamespaceList);

            if (missingUsingDirectives.Count > 0)
            {
                var usingStopIndex = _cSharpParserService.GetUsingStopIndex(context);

                var usingDirectiveStr = _cSharpParserService.GenerateUsingDirectives(
                    missingUsingDirectives.ToList(),
                    usingStopIndex.Equals(context.Start));

                IsModified = true;
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
                    if (param.type_().GetText() == nameof(IServiceCollection))
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
                var missingRegInfo = new List<StartupRegistrationInfo>();

                var body = _cSharpParserService.GetTextWithWhitespace(Tokens, method.method_body());

                foreach (var regInfo in _startupRegInfoList)
                {
                    if (!Regex.Match(body, ServiceRegistrationMatchString(regInfo)).Success)
                    {
                        missingRegInfo.Add(regInfo);
                    }
                }

                if (missingRegInfo.Count > 0)
                {
                    if (method?.method_body()?.block() is null)
                    {
                        // throw
                    }
                    else
                    {
                        var closeBranceIndex = method.method_body().block().CLOSE_BRACE().Symbol.TokenIndex;

                        var preclassWhitespace = Tokens.GetHiddenTokensToLeft(closeBranceIndex, Lexer.Hidden);

                        int tabLevels = (preclassWhitespace?.Count ?? 0) > 0 ?
                            _stringUtilService.CalculateTabLevels(preclassWhitespace[0]?.Text ?? string.Empty, _tabString) : 0;

                        var regCallStrings = new List<string>();

                        foreach (var regInfo in missingRegInfo)
                        {
                            var regCallString = _serviceCommandStgService.RenderServiceStartupRegistrationCall(
                                serviceName: regInfo.ServiceName,
                                hasTypeParameters: regInfo.HasTypeParameters,
                                serviceLifespan: regInfo.ServiceLifespan.ToString());

                            regCallStrings.Add(serviceCollectionName + regCallString + ";");
                        }
                        var registrationStatement = 
                            "\r\n" +
                            _stringUtilService.TabString(
                                string.Join("\r\n", regCallStrings), tabLevels + 1, _tabString) +
                            "\r\n" +
                            _stringUtilService.TabString(string.Empty, tabLevels, _tabString);

                        IsModified = true;
                        Rewriter.InsertBefore(closeBranceIndex, registrationStatement);
                        return null;
                    }
                }
            }

            VisitChildren(context);
            return null;
        }

        private string ServiceRegistrationMatchString(StartupRegistrationInfo regInfo)
        {
            return regInfo.HasTypeParameters
                ? $@"\.\s*Add{regInfo.ServiceLifespan}\s*\(\s*typeof\s*\(\s*I{regInfo.ServiceName}" +
                    $@"Service\s*<\s*>\s*\)\s*,\s*typeof\s*\(\s*{regInfo.ServiceName}Service\s*<\s*>\s*\)\s*\)"
                : $@"\.\s*Add{regInfo.ServiceLifespan}\s*<\s*I{regInfo.ServiceName}Service\s*,\s*" +
                    $@"{regInfo.ServiceName}Service\s*>\s*\(\s*\)";
        }

    }
}
