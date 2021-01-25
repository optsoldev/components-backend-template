using Components.Clean.Clientes;
using Components.Clean.Models;
using Components.Clean.Services;
using Microsoft.Extensions.DependencyInjection;
using Refit;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Rendering;
using System.Configuration;

namespace Compoentes.Clean
{
    class Program
    {

        private static ServiceProvider Provider;
        private static ConsoleRenderer ConsoleRenderer;
        private static InvocationContext InvocationContext;

        static void Main(InvocationContext invocationContext, string[] args = null)
        {
            GithubSettings githubSetting;

            var services = new ServiceCollection();
            services.AddSingleton(githubSetting = GetGitHubSettings());
            services.AddRefitClient<IGithubClient>()
                .ConfigureHttpClient(github => github.BaseAddress = new Uri(githubSetting.Api));
            services.AddScoped<IGithubService, GithubService>();

            Provider = services.BuildServiceProvider();

            Program.InvocationContext = invocationContext;

            Program.ConsoleRenderer = new ConsoleRenderer(
                    invocationContext.Console,
                    mode: invocationContext.BindingContext.OutputMode(),
                    resetAfterRender: true
                );

            var githubService = Provider.GetRequiredService<IGithubService>();
            var projects = githubService.GetAllProjects();

            var root = new RootCommand();
            root.AddCommand(NewProject(projects));
            root.Invoke(args);
        }

        private static GithubSettings GetGitHubSettings()
        {
            var github = ConfigurationManager.GetSection("github") as NameValueCollection;

            return new GithubSettings
            {
                Api = github.Get("api"),
                Users = github.Get("users")
            };
        }

        private static Command NewProject(IEnumerable<GithubProject> projects)
        {
            var cmd = new Command("new", "Criar novo template componentes arquitetura clean");



            return cmd;
        }
    }
}
