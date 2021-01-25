using Components.Clean.Models;
using Refit;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Components.Clean.Clientes
{
    public interface IGithubClient
    {
        [Get("/users/{userName}/repos")]
        Task<IEnumerable<GithubProject>> GetGithubProjects([Query]string userName);
    }
}
