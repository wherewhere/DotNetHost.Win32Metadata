using System.Runtime.CompilerServices;
using System.Runtime.Hosting.Native;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using DotNetHost = System.Runtime.Hosting.Native.PInvoke;
using PInvoke = Windows.Win32.PInvoke;

Version a = Environment.Version;
Console.WriteLine($"Host Process .NET Version: {a}");
if (a.Major > 4) { return 0; }

unsafe
{
    // Pre-allocate a large buffer for the path to hostfxr
    const int PATH_MAX = 260;
    Span<ushort> buffer = stackalloc ushort[PATH_MAX];
    nuint buffer_size = PATH_MAX;
    int rc = DotNetHost.get_hostfxr_path(ref buffer[0], ref buffer_size, Unsafe.NullRef<get_hostfxr_parameters>());
    if (rc != 0)
    { Marshal.ThrowExceptionForHR(rc); }

    // Load hostfxr and get desired exports
    using FreeLibrarySafeHandle lib = new(PInvoke.LoadLibrary(new PCWSTR((char*)Unsafe.AsPointer(ref buffer[0]))), true);
    hostfxr_initialize_for_runtime_config_fn init_fptr = PInvoke.GetProcAddress(lib, "hostfxr_initialize_for_runtime_config").CreateDelegate<hostfxr_initialize_for_runtime_config_fn>();
    hostfxr_get_runtime_delegate_fn get_delegate_fptr = PInvoke.GetProcAddress(lib, "hostfxr_get_runtime_delegate").CreateDelegate<hostfxr_get_runtime_delegate_fn>();
    hostfxr_close_fn close_fptr = PInvoke.GetProcAddress(lib, "hostfxr_close").CreateDelegate<hostfxr_close_fn>();

    ReadOnlySpan<char> config_path = "DotNetHost.Win32Metadata.Test.runtimeconfig.json".AsSpan();
    // Load .NET Core App
    void* pointer = null;
    void* cxt = null;
    rc = init_fptr((ushort*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(config_path)), null, &cxt);
    if (rc != 0 || cxt == null)
    {
        Marshal.ThrowExceptionForHR(rc);
        close_fptr(cxt);
    }

    // Get the load assembly function pointer
    rc = get_delegate_fptr(
        cxt,
        hostfxr_delegate_type.hdt_load_assembly_and_get_function_pointer,
        &pointer);
    if (rc != 0 || pointer == null)
    { Marshal.ThrowExceptionForHR(rc); }

    load_assembly_and_get_function_pointer_fn load_assembly_and_get_function_pointer = Marshal.GetDelegateForFunctionPointer<load_assembly_and_get_function_pointer_fn>((nint)pointer);

    // Function pointer to managed delegate
    ReadOnlySpan<char> dotnetlib_path = "DotNetHost.Win32Metadata.Test.exe".AsSpan();
    ReadOnlySpan<char> dotnet_type = "Program, DotNetHost.Win32Metadata.Test".AsSpan();
    ReadOnlySpan<char> dotnet_type_method = "<Main>$".AsSpan();
    ReadOnlySpan<char> delegate_type_name = $"{nameof(MainDelegate)}, DotNetHost.Win32Metadata.Test".AsSpan();
    void* main = null;
    rc = load_assembly_and_get_function_pointer(
        (ushort*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(dotnetlib_path)),
        (ushort*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(dotnet_type)),
        (ushort*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(dotnet_type_method)),
        (ushort*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(delegate_type_name)),
        null,
        &main);
    if (rc != 0)
    { Marshal.ThrowExceptionForHR(rc); }

    MainDelegate entry_point = Marshal.GetDelegateForFunctionPointer<MainDelegate>((nint)main);
    rc = entry_point([]);

    _ = Console.Read();
    return rc;
}

public delegate int MainDelegate(string[] args);

namespace Windows.Win32 { }