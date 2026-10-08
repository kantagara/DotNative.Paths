using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DotNative.Paths;

public enum PathKind
{
    ApplicationData,
    Cache,
    Temporary,
    Documents,
}

public interface IPaths
{
    string Get(PathKind kind, bool create = true);
}

public sealed class ApplicationPaths : IPaths
{
    private readonly string appId;

    public ApplicationPaths(string applicationId, PresentationTarget? target = null)
    {
        PlatformGuard.Desktop(target ?? PresentationTarget.Local);
        appId = PlatformGuard.Namespace(applicationId);
    }

    public string Get(PathKind kind, bool create = true)
    {
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        if (string.IsNullOrEmpty(home))
            throw new IOException("The current user's home directory is unavailable.");
        var path = kind switch
        {
            PathKind.Temporary => Path.Combine(Path.GetTempPath(), appId),
            PathKind.Documents => Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            PathKind.ApplicationData when OperatingSystem.IsMacOS() => Path.Combine(
                home,
                "Library",
                "Application Support",
                appId
            ),
            PathKind.Cache when OperatingSystem.IsMacOS() => Path.Combine(
                home,
                "Library",
                "Caches",
                appId
            ),
            PathKind.ApplicationData when OperatingSystem.IsLinux() => Path.Combine(
                Xdg("XDG_DATA_HOME", Path.Combine(home, ".local", "share")),
                appId
            ),
            PathKind.Cache when OperatingSystem.IsLinux() => Path.Combine(
                Xdg("XDG_CACHE_HOME", Path.Combine(home, ".cache")),
                appId
            ),
            PathKind.ApplicationData => Path.Combine(LocalData(), appId),
            PathKind.Cache => Path.Combine(LocalData(), appId, "Cache"),
            _ => throw new ArgumentOutOfRangeException(nameof(kind)),
        };
        if (string.IsNullOrEmpty(path) || !Path.IsPathFullyQualified(path))
            throw new DirectoryNotFoundException(
                $"The OS did not provide an absolute {kind} directory."
            );
        if (create)
            Directory.CreateDirectory(path);
        return path;
    }

    private static string LocalData()
    {
        var path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        if (string.IsNullOrEmpty(path))
            throw new DirectoryNotFoundException("LocalApplicationData is unavailable.");
        return path;
    }

    private static string Xdg(string name, string fallback) =>
        Environment.GetEnvironmentVariable(name) is { Length: > 0 } value
        && Path.IsPathFullyQualified(value)
            ? value
            : fallback;
}

public static class PathsServices
{
    public static IServiceCollection AddPaths(
        this IServiceCollection services,
        string applicationId
    )
    {
        services.TryAddSingleton<IPaths>(p => new ApplicationPaths(
            applicationId,
            p.GetService<PresentationTarget>()
        ));
        return services;
    }
}
