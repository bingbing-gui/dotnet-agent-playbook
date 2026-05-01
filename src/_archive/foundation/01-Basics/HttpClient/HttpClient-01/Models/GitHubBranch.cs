using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AspNetCore.HttpClient.Models
{
    /// <summary>
    /// A partial representation of a branch object from the GitHub API
    /// </summary>
    public class GitHubBranch
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
    }

    public class BasicUsageModel
    {
        public IEnumerable<GitHubBranch> Branches { get; set; }

        public bool GetBranchesError { get; set; }
    }
}