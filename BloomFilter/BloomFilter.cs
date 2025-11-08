using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using BloomFilter.BloomFilter;
using BloomFilter.Hashing;
using System.Threading;

namespace BloomFilter.BloomFilter
{
    /// <summary>
    /// Implements a classic Bloom filter data structure.
    /// This implementation is thread-safe.
    /// </summary>
    /// <typeparam name="T">The type of elements to be stored. Note: The element's .ToString() method 
    /// will be used to generate its byte representation for hashing.</typeparam>
    public class BloomFilter<T> : IBloomFilter<T>
    {
        private readonly BitArray _bits;
        private readonly int _m; // Size of the bit array
        private readonly int _k; // Number of hash functions

        private long _itemCount;
        private readonly long _capacity; // The 'n' this filter was designed for

        /// <summary>
        /// Gets the number of items that have been added to the filter.
        /// </summary>
        public long Count => _itemCount;
        /// <summary>
        /// Gets the expected number of items the filter was designed to hold.
        /// </summary>
        public long Capacity => _capacity;

        // Use the wrapper for hashing logic
        private readonly DoubleHashWrapper _hashWrapper;
        private readonly object _lock = new object(); // For thread-safety

        /// <summary>
        /// Initializes a new instance of the <see cref="BloomFilter{T}"/> class.
        /// </summary>
        /// <param name="m">The size of the bit array (calculated by BloomMath).</param>
        /// <param name="k">The number of hash functions (calculated by BloomMath).</param>
        /// <param name="capacity">The expected capacity (n) of the filter.</param>
        /// <param name="hashWrapper">The hashing strategy wrapper.</param>
        public BloomFilter(int m, int k, long capacity, DoubleHashWrapper hashWrapper)
        {
            if (m <= 0)
                throw new ArgumentOutOfRangeException(nameof(m), "Bit array size 'm' must be positive.");
            if (k <= 0)
                throw new ArgumentOutOfRangeException(nameof(k), "Number of hash functions 'k' must be positive.");
            if (capacity <= 0)
                throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity must be positive.");

            _m = m;
            _k = k;
            _capacity = capacity;
            _itemCount = 0;
            _hashWrapper = hashWrapper ?? throw new ArgumentNullException(nameof(hashWrapper));
            _bits = new BitArray(m);
        }

        /// <summary>
        /// Adds an element to the Bloom filter.
        /// </summary>
        /// <param name="element">The element to add.</param>
        public void Add(T element)
        {
            byte[] data = ConvertToBytes(element);

            // Get positions from the wrapper
            int[] positions = _hashWrapper.GetHashPositions(data, _k, _m);

            lock (_lock)
            {
                foreach (int position in positions)
                {
                    _bits.Set(position, true);
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

            // Get positions from the wrapper
            int[] positions = _hashWrapper.GetHashPositions(data, _k, _m);

            lock (_lock)
            {
                foreach (int position in positions)
                {
                    if (_bits.Get(position) == false)
                    {
                        return false;
                    }
                }
            }

            return true;
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
