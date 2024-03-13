using AMS.Services.Services.IServices;
using System.Runtime.Caching;

namespace AMS.Services.Services
{
    public class CacheService : ICacheService
    {
        ObjectCache _memoryCache = MemoryCache.Default;
        private static object _lock = new object();
        public T GetData<T>(string key)
        {
            try
            {
                T item = (T)_memoryCache.Get(key);
                return item;
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public object RemoveData(string key)
        {
            try
            {
                if (!string.IsNullOrEmpty(key))
                {
                    return _memoryCache.Remove(key);
                }
            }
            catch (Exception e)
            {
                throw;
            }
            return false;
        }

        public bool SetData<T>(string key, T value, DateTimeOffset expirationTime)
        {
            lock (_lock)
            {
                bool res = true;
                try
                {
                    if (!string.IsNullOrEmpty(key))
                    {
                        _memoryCache.Set(key, value, expirationTime);
                    }
                }
                catch (Exception e)
                {
                    throw;
                }
                return res;
            }
        }
    }
}
