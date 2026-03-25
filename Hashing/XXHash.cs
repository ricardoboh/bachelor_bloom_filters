using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace BloomFilter.Hashing
{
    /// <summary>
    /// Implements the 32-bit xxHash algorithm.
    /// A very fast non-cryptographic hash function.
    /// </summary>
    public sealed class XXHash : IHashFunction
    {
        private readonly uint _seed;

        // Constants for 32-bit xxHash
        private const uint PRIME32_1 = 2654435761U;
        private const uint PRIME32_2 = 2246822519U;
        private const uint PRIME32_3 = 3266489917U;
        private const uint PRIME32_4 = 668265263U;
        private const uint PRIME32_5 = 374761393U;

        /// <summary>
        /// Initializes a new instance of the XXHash function.
        /// </summary>
        /// <param name="seed">The seed value to use for the hash.</param>
        public XXHash(uint seed = 0)
        {
            _seed = seed;
        }

        /// <summary>
        /// Computes the 32-bit xxHash hash of the given data.
        /// </summary>
        /// <param name="data">The byte array to hash.</param>
        /// <returns>A 32-bit unsigned integer hash code.</returns>
        public uint Hash(byte[] data)
        {
            int length = data.Length;
            int index = 0;
            uint h32;

            if (length >= 16)
            {
                int limit = length - 16;
                uint v1 = _seed + PRIME32_1 + PRIME32_2;
                uint v2 = _seed + PRIME32_2;
                uint v3 = _seed + 0;
                uint v4 = _seed - PRIME32_1;

                do
                {
                    v1 = Round(v1, BitConverter.ToUInt32(data, index));
                    index += 4;
                    v2 = Round(v2, BitConverter.ToUInt32(data, index));
                    index += 4;
                    v3 = Round(v3, BitConverter.ToUInt32(data, index));
                    index += 4;
                    v4 = Round(v4, BitConverter.ToUInt32(data, index));
                    index += 4;
                } while (index <= limit);

                h32 = RotL32(v1, 1) + RotL32(v2, 7) + RotL32(v3, 12) + RotL32(v4, 18);
            }
            else
            {
                h32 = _seed + PRIME32_5;
            }

            h32 += (uint)length;

            // Process remaining bytes
            while (index <= length - 4)
            {
                h32 = RotL32(h32 + BitConverter.ToUInt32(data, index) * PRIME32_3, 17) * PRIME32_4;
                index += 4;
            }

            while (index < length)
            {
                h32 = RotL32(h32 + data[index] * PRIME32_5, 11) * PRIME32_1;
                index++;
            }

            // Final mix
            h32 ^= h32 >> 15;
            h32 *= PRIME32_2;
            h32 ^= h32 >> 13;
            h32 *= PRIME32_3;
            h32 ^= h32 >> 16;

            return h32;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint Round(uint acc, uint input)
        {
            return RotL32(acc + input * PRIME32_2, 13) * PRIME32_1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint RotL32(uint x, int r)
        {
            return (x << r) | (x >> (32 - r));
        }
    }
}