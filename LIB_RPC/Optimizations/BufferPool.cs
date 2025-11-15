using System;
using System.Buffers;

namespace LIB_RPC.Optimizations
{
    /// <summary>
    /// 記憶體緩衝區池 - 減少記憶體分配和 GC 壓力
    /// Memory buffer pool - Reduces memory allocations and GC pressure
    /// </summary>
    public static class BufferPool
    {
        // 使用 ArrayPool 來重用緩衝區 / Use ArrayPool to reuse buffers
        private static readonly ArrayPool<byte> _bytePool = ArrayPool<byte>.Shared;

        /// <summary>
        /// 從池中租用緩衝區 / Rent a buffer from the pool
        /// </summary>
        /// <param name="minimumLength">最小所需長度 / Minimum required length</param>
        /// <returns>緩衝區陣列 / Buffer array</returns>
        public static byte[] Rent(int minimumLength)
        {
            return _bytePool.Rent(minimumLength);
        }

        /// <summary>
        /// 歸還緩衝區到池中 / Return a buffer to the pool
        /// </summary>
        /// <param name="buffer">要歸還的緩衝區 / Buffer to return</param>
        /// <param name="clearBuffer">是否清除緩衝區內容 / Whether to clear buffer contents</param>
        public static void Return(byte[] buffer, bool clearBuffer = false)
        {
            if (buffer != null)
            {
                _bytePool.Return(buffer, clearBuffer);
            }
        }

        /// <summary>
        /// 租用並複製數據 / Rent and copy data
        /// </summary>
        public static byte[] RentAndCopy(ReadOnlySpan<byte> source)
        {
            var buffer = Rent(source.Length);
            source.CopyTo(buffer);
            return buffer;
        }
    }

    /// <summary>
    /// 可重用的 MemoryStream 包裝器 / Reusable MemoryStream wrapper
    /// </summary>
    public sealed class RecyclableMemoryStream : IDisposable
    {
        private byte[]? _buffer;
        private int _length;
        private int _position;
        private bool _disposed;

        public RecyclableMemoryStream(int capacity = 4096)
        {
            _buffer = BufferPool.Rent(capacity);
            _length = 0;
            _position = 0;
        }

        public int Length => _length;
        public int Position
        {
            get => _position;
            set
            {
                if (value < 0 || value > _length)
                    throw new ArgumentOutOfRangeException(nameof(value));
                _position = value;
            }
        }

        public void Write(ReadOnlySpan<byte> data)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(RecyclableMemoryStream));

            int requiredCapacity = _position + data.Length;
            if (_buffer == null || _buffer.Length < requiredCapacity)
            {
                // 需要擴展緩衝區 / Need to expand buffer
                int newCapacity = Math.Max(requiredCapacity, _buffer?.Length * 2 ?? 4096);
                var newBuffer = BufferPool.Rent(newCapacity);
                
                if (_buffer != null && _length > 0)
                {
                    _buffer.AsSpan(0, _length).CopyTo(newBuffer);
                    BufferPool.Return(_buffer);
                }
                
                _buffer = newBuffer;
            }

            data.CopyTo(_buffer.AsSpan(_position));
            _position += data.Length;
            _length = Math.Max(_length, _position);
        }

        public byte[] ToArray()
        {
            if (_disposed || _buffer == null)
                throw new ObjectDisposedException(nameof(RecyclableMemoryStream));

            var result = new byte[_length];
            _buffer.AsSpan(0, _length).CopyTo(result);
            return result;
        }

        public ReadOnlySpan<byte> GetBuffer()
        {
            if (_disposed || _buffer == null)
                throw new ObjectDisposedException(nameof(RecyclableMemoryStream));

            return _buffer.AsSpan(0, _length);
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            if (_buffer != null)
            {
                BufferPool.Return(_buffer, clearBuffer: true);
                _buffer = null;
            }
        }
    }
}
