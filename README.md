# DotNative.Paths

App-scoped desktop paths for macOS, Windows and Linux. Android and iOS have the same public package/API shape, but this package currently throws `PlatformNotSupportedException` there; mobile app sandbox APIs are not wired yet.

```csharp
builder.Services.AddPaths("com.example.myapp");
var paths = provider.Paths;
var database = Path.Combine(paths.Get(PathKind.ApplicationData), "app.sqlite");
```

`ApplicationData` and `Cache` use Application Support/Caches on macOS, LocalApplicationData on Windows and XDG data/cache roots on Linux. Linux uses `$XDG_DATA_HOME` / `$XDG_CACHE_HOME` only when the value is absolute, otherwise it uses `~/.local/share` / `~/.cache`. `Temporary` is app-scoped under the OS temp root. `Documents` is the user's Documents folder. Directories are created by default; pass `create: false` for lookup without creation. Each path is absolute or throws if the OS cannot provide it.

The application ID permits ASCII letters, numbers, dot, dash and underscore, and is kept inside the OS-provided base folder. This service creates ordinary directories; do not use it to store secrets. On a remote preview, the managed host's OS can differ from the renderer, so the constructor rejects platform mismatches.

Build locally: `dotnet build -p:DotNativeSourceRoot=../dotNative` when this repository is adjacent to the DotNative checkout. No manual RID or platform source selection is needed. macOS, Windows 11 ARM64, and Linux ARM64 (Debian 12 container) runtime paths are tested.

## Service access

Import `DotNative.Paths` to access the plugin through `IServiceProvider`:

```csharp
using DotNative.Paths;

var plugin = services.Paths;
```

The getter calls `GetRequiredService<IPaths>()` on every access, preserving
DI lifetimes and the usual missing-registration error. Register the plugin with
`AddPaths(...)` before building the provider.

A `net10.0` application uses the property syntax with C# 14 or later. A
`net9.0` application uses only the method equivalent:

```csharp
var plugin = services.Paths();
```

The package contains separate `net9.0` and `net10.0` assemblies. NuGet selects
the assembly matching the application target framework. `NET10_0_OR_GREATER`
selects the property; the `#else` branch selects the method.

Build and pack both targets with .NET 10 SDK. A source build using .NET 9 SDK
builds only `net9.0`; it does not produce the .NET 10 assembly.
