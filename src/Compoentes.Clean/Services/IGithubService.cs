using Components.Clean.Models;
using System.Collections.Generic;

namespace Components.Clean.Services
{
    public interface IGithubService
    {
        IEnumerable<GithubProject> GetAllProjects();
    }
}