namespace AspNetCore.XSS.Models
{
    public class Blog
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Body { get; set; }

        public DateTime CreateTime { get; set; }
    }
}
