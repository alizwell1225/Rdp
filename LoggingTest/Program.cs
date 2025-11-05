using LIB_RPC;

namespace LoggingTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Log Architecture Test ===\n");

            var testLogDir = Path.Combine(AppContext.BaseDirectory, "test_logs");
            Directory.CreateDirectory(testLogDir);

            var config = new GrpcConfig
            {
                LogFilePath = Path.Combine(testLogDir, "test_{date}.log"),
                MaxLogEntriesPerFile = 100, // Small number for testing rotation
                EnableConsoleLog = true,
                ForceAbandonLogOnException = false
            };

            Console.WriteLine($"Log directory: {testLogDir}");
            Console.WriteLine($"Max entries per file: {config.MaxLogEntriesPerFile}");
            Console.WriteLine($"Testing file rotation...\n");

            using (var logger = new GrpcLogger(config))
            {
                // Test different log levels
                logger.Debug("This is a debug message");
                logger.Info("Application started");
                logger.Warn("This is a warning");
                logger.Error("This is an error");

                // Test file rotation by writing more than MaxLogEntriesPerFile
                Console.WriteLine("\nGenerating logs to test file rotation...");
                for (int i = 0; i < 250; i++)
                {
                    logger.Info($"Log entry {i + 1}");
                    if ((i + 1) % 50 == 0)
                    {
                        logger.Warn($"Checkpoint reached: {i + 1} entries");
                    }
                }

                Console.WriteLine("\nWaiting for logs to flush...");
                Thread.Sleep(2000); // Give time for async writes
            }

            // Check created files
            Console.WriteLine("\n=== Log Files Created ===");
            var logFiles = Directory.GetFiles(testLogDir, "*.log");
            Console.WriteLine($"Total files created: {logFiles.Length}");
            foreach (var file in logFiles.OrderBy(f => f))
            {
                var fileInfo = new FileInfo(file);
                var lineCount = File.ReadLines(file).Count();
                Console.WriteLine($"  {Path.GetFileName(file)}: {lineCount} lines, {fileInfo.Length} bytes");
            }

            Console.WriteLine("\n=== Test Complete ===");
            Console.WriteLine($"Check {testLogDir} for log files");
        }
    }
}
