using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Channels;

namespace LIB_RPC.Optimizations
{
    /// <summary>
    /// 非同步批次處理器 - 批量處理提升效能
    /// Async batch processor - Improves performance through batching
    /// </summary>
    public class AsyncBatchProcessor<T> : IAsyncDisposable
    {
        private readonly Channel<T> _channel;
        private readonly Func<T[], Task> _batchProcessor;
        private readonly int _batchSize;
        private readonly TimeSpan _batchTimeout;
        private readonly CancellationTokenSource _cts;
        private readonly Task _processingTask;

        /// <summary>
        /// 建立非同步批次處理器 / Create async batch processor
        /// </summary>
        /// <param name="batchProcessor">批次處理函數 / Batch processing function</param>
        /// <param name="batchSize">批次大小 / Batch size</param>
        /// <param name="batchTimeout">批次超時時間 / Batch timeout</param>
        /// <param name="capacity">通道容量 / Channel capacity</param>
        public AsyncBatchProcessor(
            Func<T[], Task> batchProcessor,
            int batchSize = 10,
            TimeSpan? batchTimeout = null,
            int capacity = 1000)
        {
            _batchProcessor = batchProcessor ?? throw new ArgumentNullException(nameof(batchProcessor));
            _batchSize = batchSize;
            _batchTimeout = batchTimeout ?? TimeSpan.FromMilliseconds(100);
            _cts = new CancellationTokenSource();
            
            _channel = Channel.CreateBounded<T>(new BoundedChannelOptions(capacity)
            {
                FullMode = BoundedChannelFullMode.Wait
            });

            _processingTask = ProcessBatchesAsync(_cts.Token);
        }

        /// <summary>
        /// 加入項目到批次佇列 / Add item to batch queue
        /// </summary>
        public async ValueTask EnqueueAsync(T item, CancellationToken ct = default)
        {
            await _channel.Writer.WriteAsync(item, ct);
        }

        /// <summary>
        /// 嘗試加入項目 / Try to add item
        /// </summary>
        public bool TryEnqueue(T item)
        {
            return _channel.Writer.TryWrite(item);
        }

        private async Task ProcessBatchesAsync(CancellationToken ct)
        {
            var batch = new T[_batchSize];
            var reader = _channel.Reader;

            try
            {
                while (!ct.IsCancellationRequested)
                {
                    int count = 0;
                    var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                    timeoutCts.CancelAfter(_batchTimeout);

                    try
                    {
                        // 收集批次 / Collect batch
                        while (count < _batchSize && !timeoutCts.Token.IsCancellationRequested)
                        {
                            if (await reader.WaitToReadAsync(timeoutCts.Token))
                            {
                                if (reader.TryRead(out var item))
                                {
                                    batch[count++] = item;
                                }
                            }
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        // 超時，處理已收集的項目 / Timeout, process collected items
                    }
                    finally
                    {
                        timeoutCts.Dispose();
                    }

                    // 處理批次 / Process batch
                    if (count > 0)
                    {
                        try
                        {
                            var itemsToProcess = new T[count];
                            Array.Copy(batch, itemsToProcess, count);
                            await _batchProcessor(itemsToProcess);
                        }
                        catch (Exception ex)
                        {
                            // 記錄錯誤但繼續處理 / Log error but continue processing
                            Console.WriteLine($"Batch processing error: {ex.Message}");
                        }

                        // 清除批次 / Clear batch
                        Array.Clear(batch, 0, count);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // 正常取消 / Normal cancellation
            }
        }

        /// <summary>
        /// 完成並等待所有批次處理完畢 / Complete and wait for all batches to process
        /// </summary>
        public async Task CompleteAsync()
        {
            _channel.Writer.Complete();
            await _processingTask;
        }

        public async ValueTask DisposeAsync()
        {
            _cts.Cancel();
            _channel.Writer.Complete();
            
            try
            {
                await _processingTask;
            }
            catch (OperationCanceledException)
            {
                // 預期的取消 / Expected cancellation
            }
            
            _cts.Dispose();
        }
    }
}
