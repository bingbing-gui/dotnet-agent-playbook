using Microsoft.AspNetCore.Mvc;
namespace AspNetCore.ModelBinding.Advanced.Models
{
    public class FullHeader
    {
        [FromHeader]
        public string Accept { get; set; }
        [FromHeader(Name = "Accept-Encoding")]
        public string AcceptEncoding { get; set; }
        [FromHeader(Name = "Accept-Language")]
        public string AcceptLanguage { get; set; }
        [FromHeader(Name = "Cache-Control")]
        public string CacheControl { get; set; }
        [FromHeader(Name = "Connection")]
        public string Connection { get; set; }
        [FromHeader(Name = "Host")]
        public string Host { get; set; }
        [FromHeader(Name = "Upgrade-Insecure-Requests")]
        public string UpgradeInsecureRequests { get; set; }
        [FromHeader(Name = "User-Agent")]
        public string UserAgent { get; set; }
    }
}
