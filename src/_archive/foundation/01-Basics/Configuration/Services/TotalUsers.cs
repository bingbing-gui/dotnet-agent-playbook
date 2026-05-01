namespace AspNetCore.Configuration.Services
{
    public class TotalUsers
    {
        public long TUsers()
        {
            Random rnd = new Random();
            return rnd.Next(100, int.MaxValue);
        }
    }
}
