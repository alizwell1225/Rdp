using System.Collections.Concurrent;
using System.Text;

namespace LIB_RPC
{
    public sealed class GrpcLogger
    {
        private readonly GrpcConfig _config;
        private readonly BlockingCollection<string> _queue = new();
        private readonly CancellationTokenSource _cts = new();
        private readonly Task _worker;

        public event Action<string>? OnLine;

        public GrpcLogger(GrpcConfig config)
        {
            _config = config;
            _worker = Task.Run(ProcessAsync);
        }

        public void Info(string message) => Write("INFO", message);
        public void Error(string message) => Write("ERROR", message);
        public void Warn(string message) => Write("WARN", message);

        private void Write(string level, string message)
        {
            var line = $"{DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()} [{level}] {message}";
            _queue.Add(line);
            if (_config.EnableConsoleLog) Console.WriteLine(line);
            OnLine?.Invoke(line);
        }

        private async Task ProcessAsync()
        {
            try
            {
                using var fs = new FileStream(_config.LogFilePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                using var writer = new StreamWriter(fs, Encoding.UTF8);
                foreach (var line in _queue.GetConsumingEnumerable(_cts.Token))
                {
                    await writer.WriteLineAsync(line);
                    await writer.FlushAsync();
                }
            }
            catch (OperationCanceledException) { }
        }

        public void Dispose()
        {
            _queue.CompleteAdding();
            _cts.Cancel();
            try { _worker.Wait(2000); } catch { }
        }
    }
}
