using Optsol.Clientes;
using Optsol.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Optsol.Services
{
    public class GithubService : IGithubService
    {
        private readonly GithubSettings _githubSetting;
        private readonly IGithubClient _githubClient;

        public GithubService(GithubSettings githubSetting, IGithubClient githubClient)
        {
            _githubSetting = githubSetting ?? throw new ArgumentNullException(nameof(githubSetting));
            _githubClient = githubClient ?? throw new ArgumentNullException(nameof(githubClient));
        }

        public IEnumerable<GithubProject> GetAllProjects()
        {
            var projects = _githubClient
                .GetGithubProjects(_githubSetting.Users)
                .GetAwaiter()
                .GetResult();

            return projects.Where(project => _githubSetting.WhiteList.Contains(project.name));
        }
    }
}
