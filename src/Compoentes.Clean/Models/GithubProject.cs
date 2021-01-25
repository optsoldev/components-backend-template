using Refit;

namespace Components.Clean.Models
{
    public class GithubProject
    {
        [AliasAs("id")]
        public long Id { get; set; }

        [AliasAs("name")]
        public string Description { get; set; }

        [AliasAs("git_url")]
        public string Url { get; set; }
    }
}
