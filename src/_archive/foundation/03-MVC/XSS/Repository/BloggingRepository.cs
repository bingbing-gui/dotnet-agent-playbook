using AspNetCore.XSS.Models;
using Microsoft.EntityFrameworkCore;

namespace AspNetCore.XSS.Repository
{
    public class BloggingRepository : IBloggingRepository
    {
        private MemoryContext _memoryContext;
        public BloggingRepository(MemoryContext memoryContext)
        {
            _memoryContext = memoryContext;
        }
        public async Task AddBlog(Blog blog)
        {
            _memoryContext.Blogs.Add(blog);
            await _memoryContext.SaveChangesAsync();
        }
        public async Task<IEnumerable<Blog>> GetAllBlogs()
        {
            var blogs=_memoryContext.Blogs;
            return blogs;
        }

        public async Task<Blog> GetBlogByName(string name)
        {
            return _memoryContext.Blogs.Where(b => b.Title == name)
                .FirstOrDefault();
        }
    }
}
