using Microsoft.Extensions.DependencyInjection;
using Optsol.Clientes;
using Optsol.Models;
using Optsol.Services;
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
using System.Management.Automation;

namespace Compoentes.Clean
{
    class Program
    {
        private static ServiceProvider Provider;
        private static ConsoleRenderer ConsoleRenderer;
        private static InvocationContext InvocationContext;

        static void Main(InvocationContext invocationContext, string[] args = null)
        {
            ReposSettings reposSettings;
            GithubSettings githubSetting;

            var services = new ServiceCollection();
            services.AddSingleton(reposSettings = GetReposSettings());
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
            root.AddCommand(NewProject(reposSettings, githubSetting, projects));
            root.Invoke(args);
        }

        private static ReposSettings GetReposSettings()
        {
            var repos = ConfigurationManager.GetSection("repos") as NameValueCollection;

            return new ReposSettings
            {
                Dir = repos.Get("dir")
            };
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

        private static Command NewProject(ReposSettings reposSettings, GithubSettings githubSettings, IEnumerable<GithubProject> projects)
        {
            var cmd = new Command("new", "Create new template of componentes basic DDD layers");

            foreach (var project in projects)
            {
                var repoCommand = new Command(project.name, $"Create new project from template { project.name }");
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

                    CreateProject(reposSettings, project, name);
                });

                cmd.AddCommand(repoCommand);
            }

            cmd.Handler = CommandHandler.Create(() =>
            {
                var table = new TableView<GithubProject> { Items = projects.ToList() };

                NewLineConsole();
                WriteConsole("Usage: new [template]");
                NewLineConsole();

                table.AddColumn(template => template.name, "Template");

                var screen = new ScreenView(ConsoleRenderer, InvocationContext.Console) { Child = table };
                screen.Render();

                WriteConsole("----");
                NewLineConsole();
                WriteConsole("Examples:");
                WriteConsole($"ConsoleApp1 new { projects.First().name } --name NameYourSolution");
            });

            return cmd;
        }

        private static void CreateProject(ReposSettings reposSettings, GithubProject githubProject, string name)
        {
            using (var powershell = PowerShell.Create())
            {
                powershell.AddScript($"cd { reposSettings.Dir }");
                powershell.AddScript($"mkdir { name }");
                powershell.AddScript($"cd { name }");
                powershell.AddScript($"git clone { githubProject.clone_url }");
                powershell.AddScript("start .");

                foreach (PSObject o in powershell.Invoke())
                {
                    WriteConsole($"{ o }");
                }

                PSDataCollection<ErrorRecord> errors = powershell.Streams.Error;
                if (errors != null && errors.Count > 0)
                {
                    foreach (ErrorRecord err in errors)
                    {
                        WriteConsole($"    error: { err }");
                    }
                }
            }

            WriteConsole($"Create { name } successfully created!");
        }

        static Action NewLineConsole = () => Console.WriteLine("\n");
        static Action<string> WriteConsole = (string writer) => Console.WriteLine(writer);
    }
}
