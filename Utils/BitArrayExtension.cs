using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace BloomFilter.Utils
{
    /// <summary>
    /// Provides extension methods for the BitArray class.
    /// </summary>
    public static class BitArrayExtension
    {
        /// <summary>
        /// Converts a BitArray into a byte array.
        /// </summary>
        /// <param name="bits">The BitArray to convert.</param>
        /// <returns>A byte array representing the BitArray.</returns>
        public static byte[] ToByteArray(this BitArray bits)
        {
            // Ensure the byte array is the correct size.
            // BitArray.Length is the number of bits. We need (bits + 7) / 8 bytes.
            int byteCount = (bits.Length + 7) / 8;
            byte[] bytes = new byte[byteCount];

            // Copy the bits into the byte array.
            bits.CopyTo(bytes, 0);

            return bytes;
        }

        /// <summary>
        /// Creates a BitArray from a byte array.
        /// </summary>
        /// <param name="bytes">The byte array to convert.</param>
        /// <param name="bitLength">The original number of bits (m).</param>
        /// <returns>A BitArray.</returns>
        public static BitArray FromByteArray(byte[] bytes, int bitLength)
        {
            var bitArray = new BitArray(bytes);

            // The BitArray(byte[]) constructor creates an array with
            // length = bytes.Length * 8. This might be longer than our
            // original 'm' due to padding. We must truncate it.
            bitArray.Length = bitLength;

            return bitArray;
        }
    }
}