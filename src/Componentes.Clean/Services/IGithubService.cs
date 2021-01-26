using Optsol.Models;
using System.Collections.Generic;

namespace Optsol.Services
{
    public interface IGithubService
    {
        IEnumerable<GithubProject> GetAllProjects();
    }
}