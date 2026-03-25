using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BloomFilter.BloomFilter;
using BloomFilter.Hashing;
using BloomFilter.Utils;
using System.IO;
using System.Collections;
using System.Reflection;

namespace BloomFilter.IO
{
    /// <summary>
    /// Handles saving and loading Bloom filter state to/from a stream.
    /// Supports Classic, Counting, and Scalable filter types.
    /// </summary>
    public class BloomFilterSerializer
    {
        // Define headers to identify filter types in the file
        private const string HEADER_CLASSIC = "BF_CLASSIC_v1";
        private const string HEADER_COUNTING = "BF_COUNTING_v1";
        private const string HEADER_SCALABLE = "BF_SCALABLE_v1";

        #region --- Save ---

        /// <summary>
        /// Saves the state of a Bloom filter to a stream.
        /// </summary>
        /// <param name="filter">The Bloom filter to save (Classic, Counting, or Scalable).</param>
        /// <param name="stream">The stream to write to (e.g., a FileStream).</param>
        public void Save(IBloomFilter<string> filter, Stream stream)
        {
            // Use a BinaryWriter to write data.
            // 'true' for leaveOpen ensures the stream isn't closed by the writer.
            using (var writer = new BinaryWriter(stream, System.Text.Encoding.UTF8, true))
            {
                // Classic Bloom Filter
                if (filter is BloomFilter<string> classicFilter)
                {
                    writer.Write(HEADER_CLASSIC);
                    var (m, k, capacity) = GetFilterParameters(classicFilter);
                    writer.Write(m);
                    writer.Write(k);
                    writer.Write(capacity);

                    var bits = GetBitArray(classicFilter);
                    byte[] bytes = bits.ToByteArray();
                    writer.Write(bytes.Length);
                    writer.Write(bytes);
                }
                // Counting Bloom Filter
                else if (filter is CountingBloomFilter<string> countingFilter)
                {
                    writer.Write(HEADER_COUNTING);
                    var (m, k, capacity) = GetFilterParameters(countingFilter);
                    writer.Write(m);
                    writer.Write(k);
                    writer.Write(capacity);

                    var counters = GetCounterArray(countingFilter);
                    writer.Write(counters.Length);
                    writer.Write(counters);
                }
                // Scalable Bloom Filter
                else if (filter is ScalableBloomFilter<string> scalableFilter)
                {
                    writer.Write(HEADER_SCALABLE);
                    var internalFilters = GetInternalFilters(scalableFilter);
                    writer.Write(internalFilters.Count);

                    foreach (var f in internalFilters)
                    {
                        Save(f, stream);
                    }
                }
                else
                {
                    throw new NotSupportedException($"Serialization for filter type {filter.GetType().Name} is not implemented.");
                }
            }
        }

        #endregion

        #region --- Load ---

        /// <summary>
        /// Loads a Bloom filter from a file path.
        /// </summary>
        /// <param name="filePath">The file to read from.</param>
        /// <returns>A new, rehydrated IBloomFilter instance.</returns>
        public IBloomFilter<string> Load(string filePath)
        {
            using (var stream = new FileStream(filePath, FileMode.Open))
            {
                return Load(stream);
            }
        }

        /// <summary>
        /// Loads a Bloom filter from a stream.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <returns>A new, rehydrated IBloomFilter instance.</returns>
        public IBloomFilter<string> Load(Stream stream)
        {
            // Use a BinaryReader to read data.
            // 'true' for leaveOpen ensures the stream isn't closed by the reader.
            using (var reader = new BinaryReader(stream, System.Text.Encoding.UTF8, true))
            {
                string header = reader.ReadString();
                var wrapper = CreateHashWrapper();

                switch (header)
                {
                    case HEADER_CLASSIC:
                        {
                            int m = reader.ReadInt32();
                            int k = reader.ReadInt32();
                            long capacity = reader.ReadInt64();

                            int byteCount = reader.ReadInt32();
                            byte[] bytes = reader.ReadBytes(byteCount);

                            var bits = BitArrayExtension.FromByteArray(bytes, m);

                            var filter = new BloomFilter<string>(m, k, capacity, wrapper);
                            SetBitArray(filter, bits);
                            return filter;
                        }
                    case HEADER_COUNTING:
                        {
                            int m = reader.ReadInt32();
                            int k = reader.ReadInt32();
                            long capacity = reader.ReadInt64();

                            int byteCount = reader.ReadInt32();
                            byte[] counters = reader.ReadBytes(byteCount);

                            var filter = new CountingBloomFilter<string>(m, k, capacity, wrapper);
                            SetCounterArray(filter, counters);
                            return filter;
                        }
                    case HEADER_SCALABLE:
                        {
                            var sbf = (ScalableBloomFilter<string>)Activator.CreateInstance(
                                typeof(ScalableBloomFilter<string>), true);

                            var internalFilters = GetInternalFilters(sbf);
                            int filterCount = reader.ReadInt32();

                            for (int i = 0; i < filterCount; i++)
                            {
                                internalFilters.Add(Load(stream));
                            }
                            return sbf;
                        }

                    default:
                        throw new InvalidDataException($"Unknown file header '{header}'. Cannot deserialize.");
                }
            }
        }

        #endregion

        #region --- Reflection Helpers ---
        private DoubleHashWrapper CreateHashWrapper()
        {
            return new DoubleHashWrapper(new MurmurHash3(0), new XXHash(1));
        }

        private (int m, int k, long capacity) GetFilterParameters(object filter)
        {
            var m = (int)GetPrivateField(filter, "_m");
            var k = (int)GetPrivateField(filter, "_k");
            var capacity = (long)GetPrivateField(filter, "_capacity");
            return (m, k, capacity);
        }

        private BitArray GetBitArray(BloomFilter<string> filter)
        {
            return (BitArray)GetPrivateField(filter, "_bits");
        }

        private byte[] GetCounterArray(CountingBloomFilter<string> filter)
        {
            return (byte[])GetPrivateField(filter, "_counters");
        }

        private List<IBloomFilter<string>> GetInternalFilters(ScalableBloomFilter<string> filter)
        {
            return (List<IBloomFilter<string>>)GetPrivateField(filter, "_filters");
        }

        private void SetBitArray(BloomFilter<string> filter, BitArray bits)
        {
            SetPrivateField(filter, "_bits", bits);
        }

        private void SetCounterArray(CountingBloomFilter<string> filter, byte[] counters)
        {
            SetPrivateField(filter, "_counters", counters);
        }

        private object GetPrivateField(object obj, string fieldName)
        {
            var field = obj.GetType().GetField(fieldName,
                BindingFlags.NonPublic | BindingFlags.Instance);

            if (field == null)
                throw new InvalidOperationException($"Field '{fieldName}' not found on type {obj.GetType().Name}.");
            return field.GetValue(obj);
        }

        private void SetPrivateField(object obj, string fieldName, object value)
        {
            var field = obj.GetType().GetField(fieldName,
                BindingFlags.NonPublic | BindingFlags.Instance);

            if (field == null)
                throw new InvalidOperationException($"Field '{fieldName}' not found on type {obj.GetType().Name}.");
            field.SetValue(obj, value);
        }

        #endregion
    }
}