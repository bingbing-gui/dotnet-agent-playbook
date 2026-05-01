using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspNetCore.HttpClient.Models
{
    public class GitHubRepo
    {
        public IEnumerable<string> RepoRequests { get; set; }

        public bool GetRepoRequestsError { get; set; }

        public bool HasPullRequests => RepoRequests.Any();
    }
}
