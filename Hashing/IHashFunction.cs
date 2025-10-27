using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BloomFilter.Hashing
{
    /// <summary>
    /// Defines an interface for a hash function used by the Bloom filter.
    /// </summary>
    public interface IHashFunction
    {
        /// <summary>
        /// Computes the hash of the given data.
        /// </summary>
        /// <param name="data">The byte array to hash.</param>
        /// <returns>A 32-bit unsigned integer hash code.</returns>
        uint Hash(byte[] data);
    }
}
