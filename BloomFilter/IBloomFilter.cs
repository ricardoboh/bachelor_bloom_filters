using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BloomFilter.BloomFilter
{
    /// <summary>
    /// Defines the interface for a Bloom filter.
    /// </summary>
    /// <typeparam name="T">The type of elements to be stored in the filter.</typeparam>
    public interface IBloomFilter<T>
    {
        /// <summary>
        /// Adds an element to the Bloom filter.
        /// </summary>
        /// <param name="element">The element to be added.</param>
        void Add(T element);

        /// <summary>
        /// Checks if an element is possibly in the set.
        /// </summary>
        /// <param name="element">The element to check.</param>
        /// <returns>
        /// Returns <c>false</c> if the element is definitely not in the set.
        /// Returns <c>true</c> if the element is possibly in the set (with a certain false positive probability).
        /// </returns>
        bool Contains(T element);
    }
}
