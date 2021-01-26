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
using System.CommandLine.Rendering.Views;
using System.Configuration;
using System.Linq;

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
                Users = github.Get("users"),
                WhiteList = github.Get("white_list").Split(",")
            };
        }

        private static Command NewProject(IEnumerable<GithubProject> projects)
        {
            var cmd = new Command("new", "Create new template of componentes basic DDD layers");

            foreach (var project in projects)
            {
                var repoCommand = new Command(project.Name, $"Create new project from template {project.Name}");
                repoCommand.AddOption(new Option(new[] { "--name", "-n" }, "Name Solution")
                {
                    Argument = new Argument<string>
                    {
                        Arity = ArgumentArity.ExactlyOne
                    }
                });


                repoCommand.Handler = CommandHandler.Create<string>((name) =>
                {
                    if (string.IsNullOrWhiteSpace(name))
                    {
                        WriteConsole("Usage: new [template] [options]");
                        NewLineConsole();
                        WriteConsole("Options:");
                        NewLineConsole();
                        WriteConsole("--name <NameYourSolution>");
                        return;
                    }

                    CreateProject(name, project.Url);
                });

                cmd.AddCommand(repoCommand);
            }

            cmd.Handler = CommandHandler.Create(() =>
            {
                var table = new TableView<GithubProject> { Items = projects.ToList() };

                NewLineConsole();
                WriteConsole("Usage: new [template]");
                NewLineConsole();

                table.AddColumn(template => template.Name, "Template");

                var screen = new ScreenView(ConsoleRenderer, InvocationContext.Console) { Child = table };
                screen.Render();

                WriteConsole("----");
                NewLineConsole();
                WriteConsole("Examples:");
                WriteConsole($"ConsoleApp1 new { projects.First().Name } --name NameYourSolution");
            });

            return cmd;
        }

        private static void CreateProject(string name, string url)
        {

        }

        static Action NewLineConsole = () => Console.WriteLine("\n");
        static Action<string> WriteConsole = (string writer) => Console.WriteLine(writer);
    }
}
