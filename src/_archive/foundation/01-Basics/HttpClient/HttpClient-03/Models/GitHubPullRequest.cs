using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace AspNetCore.HttpClientHander.Models
{
    /// <summary>
    /// A partial representation of a pull request object from the GitHub API
    /// </summary>
    public class GitHubPullRequest
    {
        [JsonPropertyName("title")]
        public string Title { get; set; }
    }
    public class NamedClientModel
    {
        public IEnumerable<GitHubPullRequest> PullRequests { get; set; }

        public bool GetPullRequestsError { get; set; }

        public bool HasPullRequests => PullRequests.Any();

    }
}