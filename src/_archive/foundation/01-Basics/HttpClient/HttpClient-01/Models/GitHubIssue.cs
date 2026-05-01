using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace AspNetCore.HttpClient.Models
{
    /// <summary>
    /// A partial representation of an issue object from the GitHub API
    /// </summary>
    public class GitHubIssue
    {
        [JsonPropertyName("html_url")]
        public string Url { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("created_at")]
        public DateTime Created { get; set; }
    }

    public class TypeClientModel
    {
        public IEnumerable<GitHubIssue> LatestIssues { get; set; }
        public bool HasIssue => LatestIssues.Any();
        public bool GetIssuesError { get; set; }
    }
}