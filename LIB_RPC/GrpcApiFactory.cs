using LIB_RPC.Abstractions;
using LIB_RPC.API;

namespace LIB_RPC
{
    /// <summary>
    /// Factory for creating gRPC API instances with proper dependency injection.
    /// </summary>
    public static class GrpcApiFactory
    {
        /// <summary>
        /// Creates a client API instance with the specified configuration.
        /// </summary>
        public static IClientApi CreateClient(GrpcConfig? config = null)
        {
            return new GrpcClientApi(config);
        }

        /// <summary>
        /// Creates a server API instance.
        /// </summary>
        public static IServerApi CreateServer()
        {
            return new GrpcServerApi();
        }
    }
}
