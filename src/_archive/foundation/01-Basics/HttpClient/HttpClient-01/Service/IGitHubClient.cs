using AspNetCore.HttpClient.Models;
using Refit;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AspNetCore.HttpClient.Service
{
    public interface IGitHubClient
    {
        [Get("/repos/dotnet/AspNetCore.Docs/branches")]
        Task<IEnumerable<GitHubBranch>> GetAspNetCoreDocsBranchesAsync();
    }
}
