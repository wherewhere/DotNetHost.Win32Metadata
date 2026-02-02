# DotNetHost.Win32Metadata
This project contains code to build and publish the [DotNetHost.Win32Metadata](https://www.nuget.org/packages/DotNetHost.Win32Metadata) nuget package. The package wraps the [.NET Host library](https://github.com/dotnet/runtime/blob/main/src/native/corehost) into a winmd (Windows metadata) file. If you combine it with [Microsoft.Windows.CsWin32](https://www.nuget.org/packages/Microsoft.Windows.CsWin32), it will allow you to generate signatures (PInvokes) to easily use .NET Host functions in your executable/library.

[![Issues](https://img.shields.io/github/issues/wherewhere/DotNetHost.Win32Metadata.svg?label=Issues&style=flat-square)](https://github.com/wherewhere/DotNetHost.Win32Metadata/issues "Issues")
[![Stargazers](https://img.shields.io/github/stars/wherewhere/DotNetHost.Win32Metadata.svg?label=Stars&style=flat-square)](https://github.com/wherewhere/DotNetHost.Win32Metadata/stargazers "Stargazers")
[![NuGet](https://img.shields.io/nuget/dt/DotNetHost.Win32Metadata.svg?logo=NuGet&style=flat-square)](https://www.nuget.org/packages/DotNetHost.Win32Metadata "NuGet")

## How to use it
See "[Write a custom .NET host to control the .NET runtime from your native code](https://learn.microsoft.com/en-us/dotnet/core/tutorials/netcore-hosting)"

### Step 1 - Load `hostfxr` and get exported hosting functions
The `nethost` library provides the `get_hostfxr_path` function for locating the `hostfxr` library. The `hostfxr` library exposes functions for hosting the .NET runtime. The full list of functions can be found in [hostfxr.h](https://github.com/dotnet/runtime/blob/main/src/native/corehost/hostfxr.h) and [the native hosting design document](https://github.com/dotnet/runtime/blob/main/docs/design/features/native-hosting.md). The sample and this tutorial use the following:

- `hostfxr_initialize_for_runtime_config`: Initializes a host context and prepares for initialization of the .NET runtime using the specified runtime configuration.
- `hostfxr_get_runtime_delegate`: Gets a delegate for runtime functionality.
- `hostfxr_close`: Closes a host context.

The `hostfxr` library is found using `get_hostfxr_path` API from `nethost` library. It is then loaded and its exports are retrieved.

```cs
unsafe static bool LoadHostFXR()
{
    // Pre-allocate a large buffer for the path to hostfxr
    const int PATH_MAX = 260;
    char* buffer = stackalloc char[PATH_MAX];
    nuint buffer_size = PATH_MAX;
    int rc = DotNetHost.get_hostfxr_path(buffer, &buffer_size);
    if (rc != 0)
    { return false; }

    // Load hostfxr and get desired exports
    using FreeLibrarySafeHandle lib = new(PInvoke.LoadLibrary(new PCWSTR(buffer)), true);
    init_fptr = PInvoke.GetProcAddress(lib, "hostfxr_initialize_for_runtime_config");
    get_delegate_fptr = PInvoke.GetProcAddress(lib, "hostfxr_get_runtime_delegate");
    close_fptr = PInvoke.GetProcAddress(lib, "hostfxr_close");

    return (!init_fptr.IsNull && !get_delegate_fptr.IsNull && !close_fptr.IsNull);
}
```

The sample uses the following includes:

```cs
using Windows.Win32;
using Windows.Win32.Foundation;
using DotNetHost = System.Runtime.Hosting.Native.PInvoke;
using PInvoke = Windows.Win32.PInvoke;
```

### Step 2 - Initialize and start the .NET runtime
The `hostfxr_initialize_for_runtime_config` and `hostfxr_get_runtime_delegate` functions initialize and start the .NET runtime using the runtime configuration for the managed component that will be loaded. The `hostfxr_get_runtime_delegate` function is used to get a runtime delegate that allows loading a managed assembly and getting a function pointer to a static method in that assembly.

```cs
// Load and initialize .NET Core and get desired function pointer for scenario
unsafe static load_assembly_and_get_function_pointer_fn? GetDotNetLoadAssembly(ReadOnlySpan<char> config_path)
{
    // Load .NET Core App
    FARPROC load_assembly_and_get_function_pointer = default;
    hostfxr_handle cxt = default;
    rc = init_fptr((char*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(config_path)), null, &cxt);
    if (rc != 0 || cxt.IsNull)
    {
        Console.WriteLine($"Init failed: ${rc:X}");
        return null;
    }

    // Get the load assembly function pointer
    rc = get_delegate_fptr(
        cxt,
        hostfxr_delegate_type.hdt_load_assembly_and_get_function_pointer,
        &load_assembly_and_get_function_pointer);
    if (rc != 0 || load_assembly_and_get_function_pointer.IsNull)
    { Console.WriteLine($"Get delegate failed: ${rc:X}"); }

    close_fptr(cxt);
    return load_assembly_and_get_function_pointer.CreateDelegate<load_assembly_and_get_function_pointer_fn>();
}
```

### Step 3 - Load managed assembly and get function pointer to a managed method
The runtime delegate is called to load the managed assembly and get a function pointer to a managed method. The delegate requires the assembly path, type name, and method name as inputs and returns a function pointer that can be used to invoke the managed method.

```cs
// Function pointer to managed delegate
FARPROC hello = default;
int rc = load_assembly_and_get_function_pointer(
    (char*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(dotnetlib_path)),
    (char*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(dotnet_type)),
    (char*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(dotnet_type_method)),
    null /*delegate_type_name*/,
    null,
    &hello);
```

By passing `null` as the delegate type name when calling the runtime delegate, the sample uses a default signature for the managed method:

```cs
public delegate int ComponentEntryPoint(IntPtr args, int sizeBytes);
```

### Step 4 - Run managed code!
The native host can now call the managed method and pass it the desired parameters.

```cs
nint[] args =
[
    Marshal.StringToHGlobalAnsi("from host!"),
    i
];

hello(&args, args.Length);
```

## Contributors
[![Contributors](https://contrib.rocks/image?repo=wherewhere/DotNetHost.Win32Metadata)](https://github.com/wherewhere/DotNetHost.Win32Metadata/graphs/contributors "Contributors")