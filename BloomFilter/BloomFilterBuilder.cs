using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BloomFilter.BloomMath;
using BloomFilter.Hashing;


namespace BloomFilter.BloomFilter
{
    /// <summary>
    /// A fluent builder for creating Bloom filter instances.
    /// </summary>
    public class BloomFilterBuilder<T>
    {
        private long _capacity;
        private double _falsePositiveRate;

        /// <summary>
        /// Creates a new BloomFilterBuilder.
        /// </summary>
        private BloomFilterBuilder() { }

        /// <summary>
        /// Starts the builder process for a specific type.
        /// </summary>
        /// <typeparam name="T">The type of elements the filter will hold.</typeparam>
        /// <returns>A new builder instance.</returns>
        public static BloomFilterBuilder<T> Create()
        {
            return new BloomFilterBuilder<T>();
        }

        /// <summary>
        /// Sets the expected capacity (n) for the filter.
        /// </summary>
        /// <param name="capacity">The expected number of elements.</param>
        public BloomFilterBuilder<T> WithCapacity(long capacity)
        {
            if (capacity <= 0)
                throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity must be positive.");
            _capacity = capacity;
            return this;
        }

        /// <summary>
        /// Sets the desired false positive probability (p) for the filter.
        /// </summary>
        /// <param name="rate">The probability, a value between 0 and 1.</param>
        public BloomFilterBuilder<T> WithFalsePositiveRate(double rate)
        {
            if (rate <= 0 || rate >= 1)
                throw new ArgumentOutOfRangeException(nameof(rate), "False positive rate must be between 0 and 1.");
            _falsePositiveRate = rate;
            return this;
        }

        /// <summary>
        /// Builds a classic BloomFilter instance based on the specified parameters.
        /// </summary>
        /// <returns>A new instance of BloomFilter&lt;T&gt;.</returns>
        public IBloomFilter<T> Build()
        {
            ValidateParameters();
            var (m, k) = BloomMath.BloomMath.CalculateOptimalParameters(_capacity, _falsePositiveRate);
            var hashWrapper = CreateHashWrapper();

            return new BloomFilter<T>(m, k, _capacity, hashWrapper);
        }

        /// <summary>
        /// Builds a CountingBloomFilter instance based on the specified parameters.
        /// </summary>
        /// <returns>A new instance of CountingBloomFilter&lt;T&gt;.</returns>
        public ICountingBloomFilter<T> BuildCounting()
        {
            ValidateParameters();
            var (m, k) = BloomMath.BloomMath.CalculateOptimalParameters(_capacity, _falsePositiveRate);
            var hashWrapper = CreateHashWrapper();

            return new CountingBloomFilter<T>(m, k, _capacity, hashWrapper);
        }

        /// <summary>
        /// Internal factory for creating the hashing strategy.
        /// </summary>
        private DoubleHashWrapper CreateHashWrapper()
        {
            // We use two different seeds to ensure two independent hash functions
            IHashFunction hash1 = new MurmurHash3(seed: 0);
            IHashFunction hash2 = new XXHash(seed: 1);
            return new DoubleHashWrapper(hash1, hash2);
        }

        private void ValidateParameters()
        {
            if (_capacity == 0)
                throw new InvalidOperationException("Capacity must be set.");
            if (_falsePositiveRate == 0)
                throw new InvalidOperationException("FalsePositiveRate must be set.");
        }
    }
}
