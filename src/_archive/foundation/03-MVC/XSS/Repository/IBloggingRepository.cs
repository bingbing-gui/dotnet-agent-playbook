using AspNetCore.XSS.Models;
using System.Reflection.Metadata;

namespace AspNetCore.XSS.Repository
{
    public interface IBloggingRepository
    {
        Task<Blog> GetBlogByName(string name);
        Task<IEnumerable<Blog>> GetAllBlogs();
        Task AddBlog(Blog blog);
    }
}
