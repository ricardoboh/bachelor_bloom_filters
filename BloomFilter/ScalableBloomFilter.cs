using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BloomFilter.BloomFilter
{
    /// <summary>
    /// Implements a Scalable Bloom filter (SBF) that automatically grows as items are added.
    /// This filter is thread-safe.
    /// </summary>
    /// <typeparam name="T">The type of elements to be stored.</typeparam>
    public class ScalableBloomFilter<T> : IBloomFilter<T>
    {
        // A list to hold the stack of filters.
        private readonly List<IBloomFilter<T>> _filters;

        // A builder to create new filters on the fly.
        private readonly BloomFilterBuilder<T> _builder;

        // Configuration for how the filter scales
        private readonly int _scalingFactor;
        private readonly double _tighteningRatio;

        // Parameters for the *next* filter to be created
        private long _currentCapacity;
        private double _currentFalsePositiveRate;

        private readonly object _lock = new object();

        /// <summary>
        /// Gets the total number of elements added across all filters in the stack.
        /// </summary>
        public long Count
        {
            get
            {
                lock (_lock)
                {
                    // Sum the counts of all individual filters
                    return _filters.Sum(f => f.Count);
                }
            }
        }

        /// <summary>
        /// Gets the total capacity of all filters in the stack.
        /// </summary>
        public long Capacity
        {
            get
            {
                lock (_lock)
                {
                    // Sum the capacities of all individual filters
                    return _filters.Sum(f => f.Capacity);
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScalableBloomFilter{T}"/> class.
        /// </summary>
        /// <param name="initialCapacity">The capacity of the very first filter.</param>
        /// <param name="falsePositiveRate">The target false positive rate for the *entire* structure.</param>
        /// <param name="scalingFactor">How much to multiply the capacity by for each new filter (default 2).</param>
        /// <param name="tighteningRatio">How much to multiply the false positive rate by for each new filter (default 0.85).</param>
        public ScalableBloomFilter(long initialCapacity, double falsePositiveRate, int scalingFactor = 2, double tighteningRatio = 0.85)
        {
            if (initialCapacity <= 0)
                throw new ArgumentOutOfRangeException(nameof(initialCapacity), "Initial capacity must be positive.");
            if (falsePositiveRate <= 0 || falsePositiveRate >= 1)
                throw new ArgumentOutOfRangeException(nameof(falsePositiveRate), "False positive rate must be between 0 and 1.");
            if (scalingFactor < 2)
                throw new ArgumentOutOfRangeException(nameof(scalingFactor), "Scaling factor must be 2 or greater.");
            if (tighteningRatio <= 0 || tighteningRatio >= 1)
                throw new ArgumentOutOfRangeException(nameof(tighteningRatio), "Tightening ratio must be between 0 and 1.");

            _filters = new List<IBloomFilter<T>>();
            _builder = BloomFilterBuilder<T>.Create();

            _scalingFactor = scalingFactor;
            _tighteningRatio = tighteningRatio;

            _currentCapacity = initialCapacity;
            _currentFalsePositiveRate = falsePositiveRate; // The first filter gets the base rate

            // Create the first filter
            AddNewFilter();
        }

        /// <summary>
        /// Private constructor for serialization.
        /// Creates an "empty" scalable filter that can be rehydrated.
        /// </summary>
        private ScalableBloomFilter()
        {
            _filters = new List<IBloomFilter<T>>();
            _builder = BloomFilterBuilder<T>.Create();
        }

        /// <summary>
        /// Adds an element to the Scalable Bloom filter.
        /// </summary>
        /// <param name="element">The element to add.</param>
        public void Add(T element)
        {
            lock (_lock)
            {
                // Get the newest filter
                var currentFilter = _filters[_filters.Count - 1];

                // Check if it's full
                if (currentFilter.Count >= currentFilter.Capacity)
                {
                    // It's full. Create a new, larger filter and add it.
                    AddNewFilter();
                    currentFilter = _filters[_filters.Count - 1];
                }

                // Add the element to the newest filter
                currentFilter.Add(element);
            }
        }

        /// <summary>
        /// Checks if an element is possibly in the set.
        /// Checks all filters in the stack.
        /// </summary>
        /// <param name="element">The element to check.</param>
        /// <returns>False if the element is definitely not in any filter, True otherwise.</returns>
        public bool Contains(T element)
        {
            lock (_lock)
            {
                // We must check every filter.
                // If any filter says "true", the element might be present.
                foreach (var filter in _filters)
                {
                    if (filter.Contains(element))
                    {
                        return true;
                    }
                }
            }

            // All filters returned "false", so it's definitely not present.
            return false;
        }

        /// <summary>
        /// Creates a new filter with scaled parameters and adds it to the list.
        /// </summary>
        private void AddNewFilter()
        {
            // Create the new filter using the builder and current parameters
            IBloomFilter<T> newFilter = _builder
                .WithCapacity(_currentCapacity)
                .WithFalsePositiveRate(_currentFalsePositiveRate)
                .Build();

            _filters.Add(newFilter);

            // Prepare the parameters for the *next* filter
            _currentCapacity *= _scalingFactor;
            _currentFalsePositiveRate *= _tighteningRatio;
        }
    }
}