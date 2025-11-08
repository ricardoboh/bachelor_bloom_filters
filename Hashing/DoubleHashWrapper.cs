using BloomFilter.Hashing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BloomFilter.Hashing
{
    /// <summary>
    /// Encapsulates the double hashing strategy.
    /// It uses two independent hash functions to generate k hash positions.
    /// </summary>
    public class DoubleHashWrapper
    {
        private readonly IHashFunction _hash1;
        private readonly IHashFunction _hash2;

        /// <summary>
        /// Initializes a new instance of the <see cref="DoubleHashWrapper"/> class.
        /// </summary>
        /// <param name="hash1">A primary hash function (e.g., MurmurHash3).</param>
        /// <param name="hash2">A secondary hash function (e.g., XXHash).</param>
        public DoubleHashWrapper(IHashFunction hash1, IHashFunction hash2)
        {
            _hash1 = hash1 ?? throw new ArgumentNullException(nameof(hash1));
            _hash2 = hash2 ?? throw new ArgumentNullException(nameof(hash2));
        }

        /// <summary>
        /// Generates k hash positions for the given data.
        /// </summary>
        /// <param name="data">The data to hash.</param>
        /// <param name="k">The number of hash positions to generate.</param>
        /// <param name="m">The size of the bit array (modulo).</param>
        /// <returns>An array of k hash positions.</returns>
        public int[] GetHashPositions(byte[] data, int k, int m)
        {
            int[] positions = new int[k];
            uint hashA = _hash1.Hash(data);
            uint hashB = _hash2.Hash(data);

            // Ensure hashB is non-zero
            if (hashB == 0)
            {
                hashB = 1; // Non-zero constant
            }

            for (int i = 0; i < k; i++)
            {
                // hash_i(x) = (hashA(x) + i * hashB(x)) % m
                uint hash = hashA + ((uint)i * hashB);
                positions[i] = (int)(hash % (uint)m);
            }

            return positions;
        }
    }
}
