using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BloomFilter.Hashing
{
    /// <summary>
    /// Implements the 32-bit MurmurHash3 algorithm.
    /// A fast, non-cryptographic hash function with excellent distribution.
    /// </summary>
    public sealed class MurmurHash3 : IHashFunction
    {
        // A default seed value.
        private const uint Seed = 0;

        /// <summary>
        /// Computes the 32-bit MurmurHash3 hash of the given data.
        /// </summary>
        /// <param name="data">The byte array to hash.</param>
        /// <returns>A 32-bit unsigned integer hash code.</returns>
        public uint Hash(byte[] data)
        {
            const uint c1 = 0xcc9e2d51;
            const uint c2 = 0x1b873593;

            uint h1 = Seed;
            int nblocks = data.Length / 4;

            // Process 4-byte chunks
            for (int i = 0; i < nblocks; i++)
            {
                uint k1 = BitConverter.ToUInt32(data, i * 4);

                k1 *= c1;
                k1 = RotL32(k1, 15);
                k1 *= c2;

                h1 ^= k1;
                h1 = RotL32(h1, 13);
                h1 = h1 * 5 + 0xe6546b64;
            }

            // Process remaining bytes (tail)
            int offset = nblocks * 4;
            uint k2 = 0;
            switch (data.Length & 3)
            {
                case 3: k2 ^= (uint)data[offset + 2] << 16; goto case 2;
                case 2: k2 ^= (uint)data[offset + 1] << 8; goto case 1;
                case 1:
                    k2 ^= (uint)data[offset];
                    k2 *= c1; k2 = RotL32(k2, 15); k2 *= c2; h1 ^= k2;
                    break;
            }

            // Finalization mix
            h1 ^= (uint)data.Length;
            h1 = Fmix32(h1);

            return h1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint RotL32(uint x, byte r)
        {
            return (x << r) | (x >> (32 - r));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint Fmix32(uint h)
        {
            h ^= h >> 16;
            h *= 0x85ebca6b;
            h ^= h >> 13;
            h *= 0xc2b2ae35;
            h ^= h >> 16;
            return h;
        }
    }
}
