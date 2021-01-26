using Refit;

namespace Optsol.Models
{
    public class GithubSettings
    {
        public string Api { get; set; }
        public string Users { get; set; }
        public string[] WhiteList { get; set; }
    }
}
