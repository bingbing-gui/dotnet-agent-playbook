using System.ComponentModel.DataAnnotations;

namespace AspNetCore.DependencyInjection.Models
{
    public class Storage : IStorage
    {
        private Dictionary<string, Product> items = new Dictionary<string, Product>();
        public Product this[string key]
        {
            get { return items[key]; }
            set { items[key] = value; }
        }
        public IEnumerable<Product> Items => items.Values;
        public bool ContainsKey(string key)
        {
            return items.ContainsKey(key);
        }
        public void RemoveItem(string key)
        {
            items.Remove(key);
        }
    }
}
