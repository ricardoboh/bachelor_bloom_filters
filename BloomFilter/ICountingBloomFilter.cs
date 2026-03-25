using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BloomFilter.BloomFilter
{
    /// <summary>
    /// Extends the IBloomFilter interface to add support for removing elements.
    /// </summary>
    /// <typeparam name="T">The type of elements in the filter.</typeparam>
    public interface ICountingBloomFilter<T> : IBloomFilter<T>
    {
        /// <summary>
        /// Removes an element from the Counting Bloom filter.
        /// </summary>
        /// <param name="element">The element to remove.</param>
        /// <remarks>
        /// Due to potential false positives, this might decrement counters for an element
        /// that was never truly added. Counters are protected from underflowing below zero.
        /// </remarks>
        void Remove(T element);
    }
}
