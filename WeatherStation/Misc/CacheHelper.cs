using System.Runtime.Caching;

internal static class CacheHelper {
    public static async Task<T?> GetItemMaybeFromCache<T> (string key, ObjectCache cache, bool useCache, TimeSpan cacheExpiration, Func<string, Task<T>> fetch) where T : class {
        if (useCache && cache.Contains (key)) {
            return cache[key] as T;
        }
        var result = await fetch (key);
        if (useCache && result != null) {
            var cacheItemPolicy = new CacheItemPolicy {
                AbsoluteExpiration = DateTimeOffset.UtcNow.Add (cacheExpiration)
            };
            cache.Add (key, result, cacheItemPolicy);
        };
        return result;
    }
}
