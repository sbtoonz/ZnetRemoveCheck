using System;
using System.Collections.Generic;
using UnityEngine;

namespace ZnetRemovalChecker
{
    public class Cache<T>
    {
        private List<T> _cache;
        private readonly Func<List<T>> _source;
        private readonly int _checkInterval;
        private DateTime _lastUpdated;

        public Cache(Func<List<T>> source, int checkInterval)
        {
            _source = source;
            _checkInterval = checkInterval;
            _cache = new List<T>();
            _lastUpdated = DateTime.UtcNow;
        }

        public List<T> Get()
        {
            // Check if the cache is stale
            if (DateTime.UtcNow - _lastUpdated  > TimeSpan.FromSeconds(_checkInterval))
            {
                // Refresh the cache
                _cache = _source();

                // Check if any of the source list items are null
                foreach (T item in _source())
                {
                    if (item == null)
                    {
                        // Print the item from the cached list that is found to be null
                        Debug.LogWarning($"Null item found at index {_cache.IndexOf(item)}: {item}");
                    }
                }
            }

            return _cache;
        }
    }
}