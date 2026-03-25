using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BloomFilter.Hashing;
using BloomFilter.BloomFilter;
using System.Threading;

namespace BloomFilter.BloomFilter
{
    /// <summary>
    /// Implements a thread-safe Counting Bloom filter, which supports operation
    /// element deletion. It uses an array of 8-bit counters.
    /// </summary>
    /// <typeparam name="T">The type of elements to be stored.</typeparam>
    public class CountingBloomFilter<T> : ICountingBloomFilter<T>
    {
        private readonly byte[] _counters;
        private readonly int _m; // Size of the counter array
        private readonly int _k; // Number of hash functions
        private readonly DoubleHashWrapper _hashWrapper;

        private long _itemCount;
        private readonly long _capacity; // The 'n' variable how much items this filter was designed to hold

        /// <summary>
        /// Gets the number of items that have been added to the filter.
        /// </summary>
        public long Count => _itemCount;

        /// <summary>
        /// Gets the expected number of items the filter was designed to hold.
        /// </summary>
        public long Capacity => _capacity;

        private readonly object _lock = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="CountingBloomFilter{T}"/> class.
        /// </summary>
        /// <param name="m">The size of the bit array (calculated by BloomMath).</param>
        /// <param name="k">The number of hash functions (calculated by BloomMath).</param>
        /// <param name="capacity">The expected capacity (n) of the filter.</param>
        /// <param name="hashWrapper">The hashing strategy wrapper.</param>
        public CountingBloomFilter(int m, int k, long capacity, DoubleHashWrapper hashWrapper)
        {
            if (m <= 0)
                throw new ArgumentOutOfRangeException(nameof(m), "Counter array size 'm' must be positive.");
            if (k <= 0)
                throw new ArgumentOutOfRangeException(nameof(k), "Number of hash functions 'k' must be positive.");
            if (capacity <= 0)
                throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity must be positive.");

            _m = m;
            _k = k;
            _capacity = capacity;
            _itemCount = 0;
            _hashWrapper = hashWrapper ?? throw new ArgumentNullException(nameof(hashWrapper));
            _counters = new byte[m];
        }

        /// <summary>
        /// Adds an element to the Bloom filter by incrementing its corresponding counters.
        /// Counters will not overflow past 255.
        /// </summary>
        /// <param name="element">The element to add.</param>
        public void Add(T element)
        {
            byte[] data = ConvertToBytes(element);
            int[] positions = _hashWrapper.GetHashPositions(data, _k, _m);

            lock (_lock)
            {
                foreach (int position in positions)
                {
                    if (_counters[position] < byte.MaxValue)
                    {
                        _counters[position]++;
                    }
                }
                Interlocked.Increment(ref _itemCount);
            }
        }

        /// <summary>
        /// Checks if an element is possibly in the set.
        /// </summary>
        /// <param name="element">The element to check.</param>
        /// <returns>False if the element is definitely not in the set, True if it is possibly in the set.</returns>
        public bool Contains(T element)
        {
            byte[] data = ConvertToBytes(element);
            int[] positions = _hashWrapper.GetHashPositions(data, _k, _m);

            lock (_lock)
            {
                foreach (int position in positions)
                {
                    // If any counter is 0, the item is definitely not present
                    if (_counters[position] == 0)
                    {
                        return false;
                    }
                }
            }

            // All counters were > 0, so the element is *possibly* present
            return true;
        }

        /// <summary>
        /// Removes an element from the Bloom filter by decrementing its corresponding counters.
        /// Counters will not underflow below 0.
        /// </summary>
        /// <param name="element">The element to remove.</param>
        public void Remove(T element)
        {
            byte[] data = ConvertToBytes(element);
            int[] positions = _hashWrapper.GetHashPositions(data, _k, _m);

            lock (_lock)
            {
                foreach (int position in positions)
                {
                    // Protection against underflow
                    if (_counters[position] > 0)
                    {
                        _counters[position]--;
                    }
                }
            }
        }

        /// <summary>
        /// Converts the generic element to a byte array using its ToString() method.
        /// </summary>
        private byte[] ConvertToBytes(T element)
        {
            if (element == null)
            {
                return Array.Empty<byte>();
            }
            string s = element.ToString() ?? string.Empty;
            return Encoding.UTF8.GetBytes(s);
        }
    }
}
