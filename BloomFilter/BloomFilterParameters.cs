using BloomFilter.BloomFilter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BloomFilter.BloomFilter
{
    /// <summary>
    /// Represents the configuration parameters of a Bloom filter.
    /// This class is often used for serialization or building filters.
    /// </summary>
    public class BloomFilterParameters
    {
        /// <summary>
        /// The size of the bit array (m).
        /// </summary>
        public int M { get; set; }

        /// <summary>
        /// The number of hash functions (k).
        /// </summary>
        public int K { get; set; }

        /// <summary>
        /// The expected capacity (n).
        /// </summary>
        public long Capacity { get; set; }

        /// <summary>
        /// The target false positive probability (p).
        /// </summary>
        public double FalsePositiveRate { get; set; }
    }
}
