using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Depo.Models;
using Microsoft.Extensions.Caching.Memory;

namespace Depo.Components
{
    public class Cache
    {
        private readonly IMemoryCache store;

        public Cache(IMemoryCache _store)
        {
            store = _store ?? throw new ArgumentNullException(nameof(_store));

        }

        public async Task<List<DriveResult>> CreateCache(string id,List<DriveResult> data)
        {
           return store.Set<List<DriveResult>>(id, data,DateTime.Now.AddHours(6));
        }

        public async Task<List<DriveResult>> GetCache(string id)
        {
            var data = store.Get<List<DriveResult>>(id);
            if (data == null)
            {
                return null;
            }

            return data;
        }
    }
}
