using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace LIB_RPC.Optimizations
{
    /// <summary>
    /// 物件池 - 重用頻繁創建的物件
    /// Object pool - Reuses frequently created objects
    /// </summary>
    public class ObjectPool<T> where T : class
    {
        private readonly ConcurrentBag<T> _objects = new();
        private readonly Func<T> _objectGenerator;
        private readonly Action<T>? _resetAction;
        private readonly int _maxSize;
        private int _currentSize;

        /// <summary>
        /// 建立物件池 / Create object pool
        /// </summary>
        /// <param name="objectGenerator">物件生成器 / Object generator</param>
        /// <param name="resetAction">重置動作（可選）/ Reset action (optional)</param>
        /// <param name="maxSize">最大池大小 / Maximum pool size</param>
        public ObjectPool(Func<T> objectGenerator, Action<T>? resetAction = null, int maxSize = 100)
        {
            _objectGenerator = objectGenerator ?? throw new ArgumentNullException(nameof(objectGenerator));
            _resetAction = resetAction;
            _maxSize = maxSize;
        }

        /// <summary>
        /// 從池中取得物件 / Get object from pool
        /// </summary>
        public T Rent()
        {
            if (_objects.TryTake(out var item))
            {
                Interlocked.Decrement(ref _currentSize);
                return item;
            }
            return _objectGenerator();
        }

        /// <summary>
        /// 歸還物件到池中 / Return object to pool
        /// </summary>
        public void Return(T item)
        {
            if (item == null)
                return;

            // 重置物件狀態 / Reset object state
            _resetAction?.Invoke(item);

            // 如果池未滿，則歸還 / Return if pool is not full
            if (_currentSize < _maxSize)
            {
                _objects.Add(item);
                Interlocked.Increment(ref _currentSize);
            }
        }

        /// <summary>
        /// 清空池 / Clear pool
        /// </summary>
        public void Clear()
        {
            while (_objects.TryTake(out _))
            {
                Interlocked.Decrement(ref _currentSize);
            }
        }

        /// <summary>
        /// 當前池大小 / Current pool size
        /// </summary>
        public int Count => _currentSize;
    }

    /// <summary>
    /// 可出租的物件包裝器 / Rentable object wrapper
    /// </summary>
    public struct PooledObject<T> : IDisposable where T : class
    {
        private readonly ObjectPool<T> _pool;
        private T? _object;

        internal PooledObject(ObjectPool<T> pool, T obj)
        {
            _pool = pool;
            _object = obj;
        }

        /// <summary>
        /// 取得物件 / Get object
        /// </summary>
        public T Object => _object ?? throw new ObjectDisposedException(nameof(PooledObject<T>));

        /// <summary>
        /// 歸還物件到池中 / Return object to pool
        /// </summary>
        public void Dispose()
        {
            if (_object != null)
            {
                _pool.Return(_object);
                _object = null;
            }
        }
    }

    /// <summary>
    /// 物件池擴展方法 / Object pool extension methods
    /// </summary>
    public static class ObjectPoolExtensions
    {
        /// <summary>
        /// 租用物件並自動歸還 / Rent object with automatic return
        /// </summary>
        public static PooledObject<T> RentScoped<T>(this ObjectPool<T> pool) where T : class
        {
            return new PooledObject<T>(pool, pool.Rent());
        }
    }
}
