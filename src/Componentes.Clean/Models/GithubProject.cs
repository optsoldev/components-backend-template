using Refit;

namespace Optsol.Models
{
    public class GithubProject
    {
        public long id { get; set; }

        public string name { get; set; }

        public string clone_url { get; set; }
    }
}
