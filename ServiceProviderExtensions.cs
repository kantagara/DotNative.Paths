using System;
using Microsoft.Extensions.DependencyInjection;

namespace DotNative.Paths;

public static class PathsServiceProviderExtensions
{
#if NET10_0_OR_GREATER
    extension(IServiceProvider services)
    {
        /// <summary>Resolves the registered plugin using the provider's DI lifetime.</summary>
        public IPaths Paths => services.GetRequiredService<IPaths>();
    }
#else
    /// <summary>Resolves the registered plugin using the provider's DI lifetime.</summary>
    public static IPaths Paths(this IServiceProvider services) =>
        services.GetRequiredService<IPaths>();
#endif
}
