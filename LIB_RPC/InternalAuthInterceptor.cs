using System.Security.Cryptography;
using System.Text;
using Grpc.Core;
using Grpc.Core.Interceptors;

namespace LIB_RPC
{
    internal sealed class InternalAuthInterceptor : Interceptor
    {
        private readonly GrpcConfig _config;
        private readonly GrpcLogger _logger;

        public const string MetadataKey = "auth";

        public InternalAuthInterceptor(GrpcConfig config, GrpcLogger logger)
        {
            _config = config;
            _logger = logger;
        }

        private void Validate(Metadata headers)
        {
            var provided = headers.FirstOrDefault(h => h.Key.Equals(MetadataKey, StringComparison.OrdinalIgnoreCase))?.Value;
            if (string.IsNullOrEmpty(provided)) throw new RpcException(new Status(StatusCode.Unauthenticated, "Missing auth metadata"));
            var expected = Convert.ToBase64String(Encoding.UTF8.GetBytes(_config.GetSecureCredentials().GetPassword()));
            if (!CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(provided), Encoding.UTF8.GetBytes(expected)))
            {
                _logger.Warn($"Auth failed: {provided}");
                throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid credentials"));
            }
        }

        public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(TRequest request, ServerCallContext context, UnaryServerMethod<TRequest, TResponse> continuation)
        {
            Validate(context.RequestHeaders);
            return await continuation(request, context);
        }

        public override async Task ServerStreamingServerHandler<TRequest, TResponse>(TRequest request, IServerStreamWriter<TResponse> responseStream, ServerCallContext context, ServerStreamingServerMethod<TRequest, TResponse> continuation)
        {
            Validate(context.RequestHeaders);
            await continuation(request, responseStream, context);
        }

        public override async Task<TResponse> ClientStreamingServerHandler<TRequest, TResponse>(IAsyncStreamReader<TRequest> requestStream, ServerCallContext context, ClientStreamingServerMethod<TRequest, TResponse> continuation)
        {
            Validate(context.RequestHeaders);
            return await continuation(requestStream, context);
        }

        public override async Task DuplexStreamingServerHandler<TRequest, TResponse>(IAsyncStreamReader<TRequest> requestStream, IServerStreamWriter<TResponse> responseStream, ServerCallContext context, DuplexStreamingServerMethod<TRequest, TResponse> continuation)
        {
            Validate(context.RequestHeaders);
            await continuation(requestStream, responseStream, context);
        }
    }
}
