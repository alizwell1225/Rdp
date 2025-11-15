using System;
using System.IO;
using System.IO.Compression;

namespace LIB_RPC.Optimizations
{
    /// <summary>
    /// 壓縮輔助類 - 減少網路傳輸量
    /// Compression helper - Reduces network transfer size
    /// </summary>
    public static class CompressionHelper
    {
        /// <summary>
        /// 壓縮數據 / Compress data
        /// </summary>
        /// <param name="data">原始數據 / Original data</param>
        /// <param name="compressionLevel">壓縮等級 / Compression level</param>
        /// <returns>壓縮後的數據 / Compressed data</returns>
        public static byte[] Compress(byte[] data, CompressionLevel compressionLevel = CompressionLevel.Fastest)
        {
            if (data == null || data.Length == 0)
                return Array.Empty<byte>();

            using var output = new MemoryStream();
            using (var compressor = new GZipStream(output, compressionLevel, leaveOpen: true))
            {
                compressor.Write(data, 0, data.Length);
            }
            return output.ToArray();
        }

        /// <summary>
        /// 解壓縮數據 / Decompress data
        /// </summary>
        /// <param name="compressedData">壓縮的數據 / Compressed data</param>
        /// <returns>解壓縮後的數據 / Decompressed data</returns>
        public static byte[] Decompress(byte[] compressedData)
        {
            if (compressedData == null || compressedData.Length == 0)
                return Array.Empty<byte>();

            using var input = new MemoryStream(compressedData);
            using var decompressor = new GZipStream(input, CompressionMode.Decompress);
            using var output = new MemoryStream();
            
            decompressor.CopyTo(output);
            return output.ToArray();
        }

        /// <summary>
        /// 使用 RecyclableMemoryStream 壓縮數據 / Compress data using RecyclableMemoryStream
        /// </summary>
        public static byte[] CompressOptimized(ReadOnlySpan<byte> data, CompressionLevel compressionLevel = CompressionLevel.Fastest)
        {
            if (data.Length == 0)
                return Array.Empty<byte>();

            using var output = new RecyclableMemoryStream(data.Length / 2);
            using (var memStream = new MemoryStream())
            {
                using (var compressor = new GZipStream(memStream, compressionLevel, leaveOpen: true))
                {
                    compressor.Write(data.ToArray(), 0, data.Length);
                }
                output.Write(memStream.ToArray());
            }
            return output.ToArray();
        }

        /// <summary>
        /// 判斷是否值得壓縮 / Determine if compression is worthwhile
        /// </summary>
        /// <param name="dataSize">數據大小 / Data size</param>
        /// <param name="threshold">閾值（位元組）/ Threshold in bytes</param>
        /// <returns>是否建議壓縮 / Whether compression is recommended</returns>
        public static bool ShouldCompress(int dataSize, int threshold = 1024)
        {
            // 小於閾值的數據不建議壓縮 / Don't compress data smaller than threshold
            return dataSize > threshold;
        }
    }
}
