using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BloomFilter.BloomMath
{
    /// <summary>
    /// Provides static methods for Bloom filter-related mathematical calculations.
    /// </summary>
    public static class BloomMath
    {
        /// <summary>
        /// Calculates the optimal size (m) of the bit array and the optimal number of hash functions (k)
        /// for a Bloom filter, given the expected number of elements and the desired false positive probability.
        /// </summary>
        /// <param name="expectedCapacity">The expected number of elements to be added to the filter (n).</param>
        /// <param name="falsePositiveRate">The desired false positive probability (p), a value between 0 and 1.</param>
        /// <returns>A tuple containing the optimal size (m) and number of hash functions (k).</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if expectedCapacity is not positive or if falsePositiveRate is not between 0 and 1.
        /// </exception>
        public static (int m, int k) CalculateOptimalParameters(long expectedCapacity, double falsePositiveRate)
        {
            if (expectedCapacity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(expectedCapacity), "Expected capacity must be a positive number.");
            }

            if (falsePositiveRate <= 0 || falsePositiveRate >= 1)
            {
                throw new ArgumentOutOfRangeException(nameof(falsePositiveRate), "False positive rate must be between 0 and 1.");
            }

            // Calculate optimal size m
            // m = - (n * ln(p)) / (ln(2)^2)
            double m_double = -(expectedCapacity * Math.Log(falsePositiveRate)) / Math.Pow(Math.Log(2), 2);
            int m = (int)Math.Ceiling(m_double);

            // Calculate optimal number of hash functions k
            // k = (m / n) * ln(2)
            double k_double = (m / (double)expectedCapacity) * Math.Log(2);
            int k = (int)Math.Ceiling(k_double);

            // A Bloom filter must have at least one hash function.
            if (k < 1)
            {
                k = 1;
            }

            return (m, k);
        }
    }
}
