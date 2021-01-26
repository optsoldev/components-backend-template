using Optsol.Models;
using Refit;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Optsol.Clientes
{
    public interface IGithubClient
    {
        [Get("/users/{userName}/repos")]
        [Headers("User-Agent: Optsol-Agent-Template")]
        Task<IEnumerable<GithubProject>> GetGithubProjects([Query]string userName);
    }
}
