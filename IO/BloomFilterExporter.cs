using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BloomFilter.BloomFilter;
using BloomFilter.Utils;
using System.IO;
using System.Collections;
using System.Text.Json;
using System.Reflection;

namespace BloomFilter.IO
{
    /// <summary>
    /// Exports Bloom filter data to common, human-readable formats like JSON.
    /// Supports Classic, Counting, and Scalable filter types.
    /// </summary>
    public class BloomFilterExporter
    {
        /// <summary>
        /// Exports the filter's parameters and data to a JSON string.
        /// </summary>
        /// <param name="filter">The filter to export (can be Classic, Counting, or Scalable).</param>
        /// <returns>A JSON string.</returns>
        public string ExportToJson(IBloomFilter<string> filter)
        {
            // Call the recursive helper to build the data object
            var exportData = GetExportObject(filter);

            // Serialize the final object to a pretty-printed JSON string
            return JsonSerializer.Serialize(exportData, new JsonSerializerOptions { WriteIndented = true });
        }

        /// <summary>
        /// Private recursive helper to build an anonymous object
        /// representing the filter's state.
        /// </summary>
        private object GetExportObject(IBloomFilter<string> filter)
        {
            // Classic Bloom Filter
            if (filter is BloomFilter<string> classicFilter)
            {
                var (m, k, capacity) = GetFilterParameters(classicFilter);
                var bits = GetBitArray(classicFilter);

                // Convert BitArray to a simple string of 0s and 1s
                var bitString = new StringBuilder(bits.Length);
                for (int i = 0; i < bits.Length; i++)
                {
                    bitString.Append(bits[i] ? '1' : '0');
                }

                return new
                {
                    Type = "Classic",
                    Parameters = new BloomFilterParameters { M = m, K = k, Capacity = capacity },
                    Data = bitString.ToString()
                };
            }
            // Counting Bloom Filter
            else if (filter is CountingBloomFilter<string> countingFilter)
            {
                var (m, k, capacity) = GetFilterParameters(countingFilter);
                var counters = GetCounterArray(countingFilter);

                return new
                {
                    Type = "Counting",
                    Parameters = new BloomFilterParameters { M = m, K = k, Capacity = capacity },
                    Data = counters
                };
            }
            // Scalable Bloom Filter
            else if (filter is ScalableBloomFilter<string> scalableFilter)
            {
                var internalFilters = GetInternalFilters(scalableFilter);

                return new
                {
                    Type = "Scalable",
                    InternalFilters = internalFilters.Select(f => GetExportObject(f)).ToList()
                };
            }

            throw new NotSupportedException($"Export for filter type {filter.GetType().Name} is not implemented.");
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

        private object GetPrivateField(object obj, string fieldName)
        {
            var field = obj.GetType().GetField(fieldName,
                BindingFlags.NonPublic | BindingFlags.Instance);

            if (field == null)
            {
                throw new InvalidOperationException($"Field '{fieldName}' not found on type {obj.GetType().Name}.");
            }
            return field.GetValue(obj);
        }
    }
}