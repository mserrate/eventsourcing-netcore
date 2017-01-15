using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.Caching.Memory;

namespace WebApp.ViewModels
{

    public class ProductsCache
    {
        private readonly IMemoryCache _cache;
        private readonly IList<dynamic> _products;

        public ProductsCache(IMemoryCache cache)
        {
            _cache = cache;
            _products = new List<dynamic>();

            _products.Add(new { Id = Guid.Parse("4704cd76-036d-4333-bb75-8a9173784491"), Name = "Product1", Description = "P1 Description", Price = 1.95m });
            _products.Add(new { Id = Guid.Parse("4704cd76-036d-4333-bb75-8a9173784492"), Name = "Product2", Description = "P2 Description", Price = 2.60m });
        }

        public dynamic GetPoduct(Guid itemId)
        {
            return _cache.GetOrCreate(itemId, entry =>
            {
                entry.Priority = CacheItemPriority.NeverRemove;
                return _products.Where(x => x.Id == itemId).Single();
            });
        }

        public IEnumerable<dynamic> GetProductList()
        {
            return _products.Select(p => new { p.Id, p.Name }).ToList();
        }
    }
}