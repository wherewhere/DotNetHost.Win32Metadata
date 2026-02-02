using MessagePack;
using Microsoft.Windows.SDK.Win32Docs;

using CancellationTokenSource cts = new();
Console.CancelKeyPress += (s, e) =>
{
    Console.WriteLine("Canceling...");
    cts.Cancel();
    e.Cancel = true;
};

if (args.Length < 2)
{
    Console.Error.WriteLine("USAGE: {0} <path-to-output-pack> <path-to-rsp>");
    return 1;
}

string outputPath = args[0];
string documentationMappingsRsp = args[1];

try
{
    Console.WriteLine("Parsing documents...");

    Dictionary<string, ApiDetails> results = new()
    {
        #region nethost.h
        {
            "get_hostfxr_parameters",
            new ApiDetails
            {
                HelpLink = new Uri("https://github.com/dotnet/runtime/blob/release/10.0/src/native/corehost/nethost/nethost.h#L44-L64"),
                Description = "Parameters for <c>get_hostfxr_path</c>",
                Fields = new Dictionary<string, string>
                {
                    {
                        "size",
                        "Size of the struct. This is used for versioning."
                    },
                    {
                        "assembly_path",
                        """
                        Path to the component's assembly.
                        If specified, <c>hostfxr</c> is located as if the <see cref="assembly_path"/> is the <c>apphost</c>
                        """
                    },
                    {
                        "dotnet_root",
                        """
                        Path to directory containing the dotnet executable.
                        If specified, <c>hostfxr</c> is located as if an application is started using
                        '<c>dotnet app.dll</c>', which means it will be searched for under the <c>dotnet_root</c>
                        path and the <see cref="assembly_path"/> is ignored.
                        """
                    }
                }
            }
        },
        {
            "get_hostfxr_path",
            new ApiDetails
            {
                HelpLink = new Uri("https://github.com/dotnet/runtime/blob/release/10.0/src/native/corehost/nethost/nethost.h#L66-L94"),
                Description = "Get the path to the <c>hostfxr</c> library",
                Parameters = new Dictionary<string, string>
                {
                    {
                        "buffer",
                        "Buffer that will be populated with the <c>hostfxr</c> path, including a null terminator."
                    },
                    {
                        "buffer_size",
                        """
                        [<see langword="in"/>] Size of <paramref name="buffer"/> in <see langword="char_t"/> units.<br/>
                        [<see langword="out"/>] Size of <paramref name="buffer"/> used in <see langword="char_t"/> units. If the input value is too small
                        or <paramref name="buffer"/> is <see langword="nullptr"/>, this is populated with the minimum required size
                        in <see langword="char_t"/> units for a buffer to hold the <c>hostfxr</c> path
                        """
                    },
                    {
                        "get_hostfxr_parameters",
                        """
                        Optional. Parameters that modify the behaviour for locating the <c>hostfxr</c> library.
                        If <see langword="nullptr"/>, <c>hostfxr</c> is located using the environment variable or global registration
                        """
                    }
                },
                ReturnValue =
                    """
                    <c>0</c> on success, otherwise failure<br/>
                    <c>0x80008098</c> - <paramref name="buffer"/> is too small (<c>HostApiBufferTooSmall</c>)
                    """,
                Remarks =
                    """
                    The full search for the <c>hostfxr</c> library is done on every call. To minimize the need
                    to call this function multiple times, pass a large buffer (e.g. <c>PATH_MAX</c>).
                    """
            }
        },
        #endregion
        #region coreclr_delegate.h
        {
            "load_assembly_and_get_function_pointer_fn",
            new ApiDetails
            {
                HelpLink = new Uri("https://github.com/dotnet/runtime/blob/release/10.0/src/native/corehost/coreclr_delegates.h#L29-L38"),
                Description = "Signature of delegate returned by <see cref=\"coreclr_delegate_type::hdt_load_assembly_and_get_function_pointer\"/>",
                Parameters = new Dictionary<string, string>
                {
                    {
                        "assembly_path",
                        "Fully qualified path to assembly"
                    },
                    {
                        "type_name",
                        "Assembly qualified type name"
                    },
                    {
                        "method_name",
                        "Public static method name compatible with delegateType"
                    },
                    {
                        "delegate_type_name",
                        """
                        Assembly qualified delegate type name or <see langword="null"/>
                        or <c>UNMANAGEDCALLERSONLY_METHOD</c> if the method is marked with
                        the <see cref="System.Runtime.InteropServices.UnmanagedCallersOnlyAttribute"/>.
                        """
                    },
                    {
                        "reserved",
                        "Extensibility parameter (currently unused and must be <c>0</c>)"
                    },
                    {
                        "delegate",
                        "Pointer where to store the function pointer result"
                    }
                }
            }
        },
        {
            "component_entry_point_fn",
            new ApiDetails
            {
                HelpLink = new Uri("https://github.com/dotnet/runtime/blob/release/10.0/src/native/corehost/coreclr_delegates.h#L40-L41"),
                Description = "Signature of delegate returned by <see cref=\"load_assembly_and_get_function_pointer_fn\"/> when <c>delegate_type_name == null</c> (<see langword=\"default\"/>)"
            }
        },
        {
            "get_function_pointer_fn",
            new ApiDetails
            {
                HelpLink = new Uri("https://github.com/dotnet/runtime/blob/release/10.0/src/native/corehost/coreclr_delegates.h#L43-L51"),
                Parameters = new Dictionary<string, string>
                {
                    {
                        "type_name",
                        "Assembly qualified type name"
                    },
                    {
                        "method_name",
                        "Public static method name compatible with delegateType"
                    },
                    {
                        "delegate_type_name",
                        """
                        Assembly qualified delegate type name or <see langword="null"/>
                        or <c>UNMANAGEDCALLERSONLY_METHOD</c> if the method is marked with
                        the <see cref="System.Runtime.InteropServices.UnmanagedCallersOnlyAttribute"/>.
                        """
                    },
                    {
                        "load_context",
                        "Extensibility parameter (currently unused and must be <c>0</c>)"
                    },
                    {
                        "reserved",
                        "Extensibility parameter (currently unused and must be <c>0</c>)"
                    },
                    {
                        "delegate",
                        "Pointer where to store the function pointer result"
                    }
                }
            }
        },
        {
            "load_assembly_fn",
            new ApiDetails
            {
                HelpLink = new Uri("https://github.com/dotnet/runtime/blob/release/10.0/src/native/corehost/coreclr_delegates.h#L53-L56"),
                Parameters = new Dictionary<string, string>
                {
                    {
                        "assembly_path",
                        "Fully qualified path to assembly"
                    },
                    {
                        "load_context",
                        "Extensibility parameter (currently unused and must be <c>0</c>)"
                    },
                    {
                        "reserved",
                        "Extensibility parameter (currently unused and must be <c>0</c>)"
                    }
                }
            }
        },
        {
            "load_assembly_bytes_fn",
            new ApiDetails
            {
                HelpLink = new Uri("https://github.com/dotnet/runtime/blob/release/10.0/src/native/corehost/coreclr_delegates.h#L58-L64"),
                Parameters = new Dictionary<string, string>
                {
                    {
                        "assembly_bytes",
                        "Bytes of the assembly to load"
                    },
                    {
                        "assembly_bytes_len",
                        "Byte length of the assembly to load"
                    },
                    {
                        "symbols_bytes",
                        "Optional. Bytes of the symbols for the assembly"
                    },
                    {
                        "symbols_bytes_len",
                        "Optional. Byte length of the symbols for the assembly"
                    },
                    {
                        "load_context",
                        "Extensibility parameter (currently unused and must be <c>0</c>)"
                    },
                    {
                        "reserved",
                        "Extensibility parameter (currently unused and must be <c>0</c>)"
                    }
                }
            }
        },
        #endregion
        #region hostfxr.h
        {
            "hostfxr_set_error_writer_fn",
            new ApiDetails
            {
                HelpLink = new Uri("https://github.com/dotnet/runtime/blob/release/10.0/src/native/corehost/hostfxr.h#L57-L80"),
                Description = "Sets a callback which is to be used to write errors to.",
                Parameters = new Dictionary<string, string>
                {
                    {
                        "error_writer",
                        """
                        A callback function which will be invoked every time an error is to be reported.
                        Or <see langword="nullptr"/> to unregister previously registered callback and return to the default behavior.
                        """
                    }
                },
                ReturnValue =
                    """
                    The previously registered callback (which is now unregistered), or <see langword="nullptr"/> if no previous callback
                    was registered
                    """,
                Remarks =
                    """
                    <para>The error writer is registered per-thread, so the registration is thread-local. On each thread
                    only one callback can be registered. Subsequent registrations overwrite the previous ones.</para>
                    <para>By default no callback is registered in which case the errors are written to stderr.</para>
                    <para>Each call to the error writer is sort of like writing a single line (the EOL character is omitted).
                    Multiple calls to the error writer may occur for one failure.</para>
                    <para>If the <c>hostfxr</c> invokes functions in <c>hostpolicy</c> as part of its operation, the error writer
                    will be propagated to hostpolicy for the duration of the call. This means that errors from
                    both <c>hostfxr</c> and <c>hostpolicy</c> will be reporter through the same error writer.</para>
                    """
            }
        },
        {
            "hostfxr_initialize_for_dotnet_command_line_fn",
            new ApiDetails
            {
                HelpLink = new Uri("https://github.com/dotnet/runtime/blob/release/10.0/src/native/corehost/hostfxr.h#L90-L121"),
                Description = "Initializes the hosting components for a dotnet command line running an application",
                Parameters = new Dictionary<string, string>
                {
                    {
                        "argc",
                        "Number of argv arguments"
                    },
                    {
                        "argv",
                        """
                        Command-line arguments for running an application (as if through the dotnet executable).
                        Only command-line arguments which are accepted by runtime installation are supported, SDK/CLI commands are not supported.
                        For example <c>app.dll app_argument_1 app_argument_2</c>.
                        """
                    },
                    {
                        "parameters",
                        "Optional. Additional parameters for initialization"
                    },
                    {
                        "host_context_handle",
                        "On success, this will be populated with an opaque value representing the initialized host context"
                    }
                },
                ReturnValue =
                    """
                    <c>Success</c>          - Hosting components were successfully initialized<br/>
                    <c>HostInvalidState</c> - Hosting components are already initialized
                    """,
                Remarks =
                    """
                    <para>This function parses the specified command-line arguments to determine the application to run. It will
                    then find the corresponding .runtimeconfig.json and .deps.json with which to resolve frameworks and
                    dependencies and prepare everything needed to load the runtime.</para>
                    <para>This function only supports arguments for running an application. It does not support SDK commands.</para>
                    <para>This function does not load the runtime.</para>
                    """
            }
        },
        {
            "hostfxr_initialize_for_runtime_config_fn",
            new ApiDetails
            {
                HelpLink = new Uri("https://github.com/dotnet/runtime/blob/release/10.0/src/native/corehost/hostfxr.h#L123-L156"),
                Description = "Initializes the hosting components using a <c>.runtimeconfig.json</c> file",
                Parameters = new Dictionary<string, string>
                {
                    {
                        "runtime_config_path",
                        "Path to the <c>.runtimeconfig.json</c> file"
                    },
                    {
                        "parameters",
                        "Optional. Additional parameters for initialization"
                    },
                    {
                        "host_context_handle",
                        "On success, this will be populated with an opaque value representing the initialized host context"
                    }
                },
                ReturnValue =
                    """
                    <c>Success</c>                            - Hosting components were successfully initialized<br/>
                    <c>Success_HostAlreadyInitialized</c>     - Config is compatible with already initialized hosting components<br/>
                    <c>Success_DifferentRuntimeProperties</c> - Config has runtime properties that differ from already initialized hosting components<br/>
                    <c>HostIncompatibleConfig</c>             - Config is incompatible with already initialized hosting components
                    """,
                Remarks =
                    """
                    <para>This function will process the <c>.runtimeconfig.json</c> to resolve frameworks and prepare everything needed
                    to load the runtime. It will only process the <c>.deps.json</c> from frameworks (not any app/component that
                    may be next to the <c>.runtimeconfig.json</c>).</para>
                    <para>This function does not load the runtime.</para>
                    <para>If called when the runtime has already been loaded, this function will check if the specified runtime
                    config is compatible with the existing runtime.</para>
                    <para>Both <c>Success_HostAlreadyInitialized</c> and <c>Success_DifferentRuntimeProperties</c> codes are considered successful
                    initializations. In the case of <c>Success_DifferentRuntimeProperties</c>, it is left to the consumer to verify that
                    the difference in properties is acceptable.</para>
                    """
            }
        },
        {
            "hostfxr_get_runtime_property_value_fn",
            new ApiDetails
            {
                HelpLink = new Uri("https://github.com/dotnet/runtime/blob/release/10.0/src/native/corehost/hostfxr.h#L158-L184"),
                Description = "Gets the runtime property value for an initialized host context",
                Parameters = new Dictionary<string, string>
                {
                    {
                        "host_context_handle",
                        "Handle to the initialized host context"
                    },
                    {
                        "name",
                        "Runtime property name"
                    },
                    {
                        "value",
                        "Out parameter. Pointer to a buffer with the property value."
                    }
                },
                ReturnValue = "The error code result.",
                Remarks =
                    """
                    <para>The buffer pointed to by value is owned by the host context. The lifetime of the buffer is only
                    guaranteed until any of the below occur:
                    <list type="bullet">
                    <item>a '<c>run</c>' method is called for the host context</item>
                    <item>properties are changed via <c>hostfxr_set_runtime_property_value</c></item>
                    <item>the host context is closed via '<c>hostfxr_close</c>'</item>
                    </list></para>
                    <para>If <paramref name="host_context_handle"/> is nullptr and an active host context exists, this function will get the
                    property value for the active host context.</para>
                    """
            }
        },
        {
            "hostfxr_set_runtime_property_value_fn",
            new ApiDetails
            {
                HelpLink = new Uri("https://github.com/dotnet/runtime/blob/release/10.0/src/native/corehost/hostfxr.h#L186-L208"),
                Description = "Sets the value of a runtime property for an initialized host context",
                Parameters = new Dictionary<string, string>
                {
                    {
                        "host_context_handle",
                        "Handle to the initialized host context"
                    },
                    {
                        "name",
                        "Runtime property name"
                    },
                    {
                        "value",
                        "Value to set"
                    }
                },
                ReturnValue = "The error code result.",
                Remarks =
                    """
                    <para>Setting properties is only supported for the first host context, before the runtime has been loaded.</para>
                    <para>If the property already exists in the host context, it will be overwritten. If value is nullptr, the
                    property will be removed.</para>
                    """
            }
        },
        {
            "hostfxr_get_runtime_properties_fn",
            new ApiDetails
            {
                HelpLink = new Uri("https://github.com/dotnet/runtime/blob/release/10.0/src/native/corehost/hostfxr.h#L210-L241"),
                Description = "Gets all the runtime properties for an initialized host context",
                Parameters = new Dictionary<string, string>
                {
                    {
                        "host_context_handle",
                        "Handle to the initialized host context"
                    },
                    {
                        "count",
                        """
                        [<see langword="in"/>] Size of the keys and values buffers<br/>
                        [<see langword="out"/>] Number of properties returned (size of <paramref name="keys"/>/<paramref name="values"/> buffers used). If the input value is too
                        small or keys/values is <see langword="nullptr"/>, this is populated with the number of available properties
                        """
                    },
                    {
                        "keys",
                        "Array of pointers to buffers with runtime property keys"
                    },
                    {
                        "values",
                        "Array of pointers to buffers with runtime property values"
                    }
                },
                ReturnValue = "The error code result.",
                Remarks =
                    """
                    <para>The buffer pointed to by value is owned by the host context. The lifetime of the buffer is only
                    guaranteed until any of the below occur:
                    <list type="bullet">
                    <item>a '<c>run</c>' method is called for the host context</item>
                    <item>properties are changed via <c>hostfxr_set_runtime_property_value</c></item>
                    <item>the host context is closed via '<c>hostfxr_close</c>'</item>
                    </list></para>
                    <para>If <paramref name="host_context_handle"/> is <see langword="nullptr"/> and an active host context exists, this function will get the
                    properties for the active host context.</para>
                    """
            }
        },
        {
            "hostfxr_run_app_fn",
            new ApiDetails
            {
                HelpLink = new Uri("https://github.com/dotnet/runtime/blob/release/10.0/src/native/corehost/hostfxr.h#L243-L257"),
                Description = "Load CoreCLR and run the application for an initialized host context",
                Parameters = new Dictionary<string, string>
                {
                    {
                        "host_context_handle",
                        "Handle to the initialized host context"
                    }
                },
                ReturnValue = "If the app was successfully run, the exit code of the application. Otherwise, the error code result.",
                Remarks =
                    """
                    <para>The <paramref name="host_context_handle"/> must have been initialized using <c>hostfxr_initialize_for_dotnet_command_line</c>.</para>
                    <para>This function will not return until the managed application exits.</para>
                    """
            }
        },
        {
            "hostfxr_get_runtime_delegate_fn",
            new ApiDetails
            {
                HelpLink = new Uri("https://github.com/dotnet/runtime/blob/release/10.0/src/native/corehost/hostfxr.h#L259-L283"),
                Description = "Gets a typed delegate from the currently loaded CoreCLR or from a newly created one.",
                Parameters = new Dictionary<string, string>
                {
                    {
                        "host_context_handle",
                        "Handle to the initialized host context"
                    },
                    {
                        "type",
                        "Type of runtime delegate requested"
                    },
                    {
                        "delegate",
                        "An out parameter that will be assigned the delegate."
                    }
                },
                ReturnValue = "The error code result.",
                Remarks =
                    """
                    If the <paramref name="host_context_handle"/> was initialized using <c>hostfxr_initialize_for_runtime_config</c>,
                    then all delegate types are supported.<br/>
                    If the <paramref name="host_context_handle"/> was initialized using <c>hostfxr_initialize_for_dotnet_command_line</c>,
                    then only the following delegate types are currently supported:<br/>
                        hdt_load_assembly_and_get_function_pointer<br/>
                        hdt_get_function_pointer
                    </list>
                    """
            }
        },
        {
            "hostfxr_close_fn",
            new ApiDetails
            {
                HelpLink = new Uri("https://github.com/dotnet/runtime/blob/release/10.0/src/native/corehost/hostfxr.h#L285-L295"),
                Description = "Closes an initialized host context",
                Parameters = new Dictionary<string, string>
                {
                    {
                        "host_context_handle",
                        "Handle to the initialized host context"
                    }
                },
                ReturnValue = "The error code result."
            }
        },
        {
            "hostfxr_get_dotnet_environment_info_fn",
            new ApiDetails
            {
                HelpLink = new Uri("https://github.com/dotnet/runtime/blob/release/10.0/src/native/corehost/hostfxr.h#L330-L369"),
                Description =
                    """
                    <para>Returns available SDKs and frameworks.</para>
                    <para>Resolves the existing SDKs and frameworks from a dotnet root directory (if
                    any), or the global default location. If multi-level lookup is enabled and
                    the dotnet root location is different than the global location, the SDKs and
                    frameworks will be enumerated from both locations.</para>
                    <para>The SDKs are sorted in ascending order by version, multi-level lookup
                    locations are put before private ones.</para>
                    <para>The frameworks are sorted in ascending order by name followed by version,
                    multi-level lookup locations are put before private ones.</para>
                    """,
                Parameters = new Dictionary<string, string>
                {
                    {
                        "dotnet_root",
                        "The path to a directory containing a dotnet executable."
                    },
                    {
                        "reserved",
                        "Reserved for future parameters."
                    },
                    {
                        "result",
                        """
                        Callback invoke to return the list of SDKs and frameworks.
                        Structs and their elements are valid for the duration of the call.
                        """
                    },
                    {
                        "result_context",
                        "Additional context passed to the result callback."
                    }
                },
                ReturnValue = "<c>0</c> on success, otherwise failure.",
                Remarks =
                    """
                    String encoding:<br/>
                    <c>Windows</c>     - UTF-16 (pal::char_t is 2 byte wchar_t)<br/>
                    <c>Non-Windows</c> - UTF-8  (pal::char_t is 1 byte char)
                    """
            }
        },
        {
            "hostfxr_resolve_frameworks_for_runtime_config_fn",
            new ApiDetails
            {
                HelpLink = new Uri("https://github.com/dotnet/runtime/blob/release/10.0/src/native/corehost/hostfxr.h#L395-L421"),
                Description = "Resolves frameworks for a runtime config",
                Parameters = new Dictionary<string, string>
                {
                    {
                        "runtime_config_path",
                        "Path to the <c>.runtimeconfig.json</c> file"
                    },
                    {
                        "parameters",
                        """
                        Optional. Additional parameters for initialization.
                        If <see langword="null"/> or <c>dotnet_root</c> is <see langword="null"/>, the root corresponding to the running <c>hostfx</c> is used.
                        """
                    },
                    {
                        "callback",
                        """
                        Optional. Result callback invoked with result of the resolution.
                        Structs and their elements are valid for the duration of the call.
                        """
                    },
                    {
                        "result_context",
                        "Optional. Additional context passed to the result callback."
                    }
                },
                ReturnValue = "<c>0</c> on success, otherwise failure.",
                Remarks =
                    """
                    String encoding:<br/>
                    <c>Windows</c>     - UTF-16 (pal::char_t is 2 byte wchar_t)<br/>
                    <c>Non-Windows</c> - UTF-8  (pal::char_t is 1 byte char)
                    """
            }
        }
        #endregion
    };

    Console.WriteLine($"Writing results to \"{outputPath}\" and \"{documentationMappingsRsp}\".");

    Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
    await using FileStream outputFileStream = File.OpenWrite(outputPath);
    await MessagePackSerializer.SerializeAsync(outputFileStream, results, MessagePackSerializerOptions.Standard, cts.Token).ConfigureAwait(false);

    List<string> documentationMappingsBuilder = new(results.Count + 1)
    {
        "--memberRemap"
    };

    foreach (KeyValuePair<string, ApiDetails> api in results)
    {
        documentationMappingsBuilder.Add($"{api.Key.Replace(".", "::")}=[Documentation(\"{api.Value.HelpLink}\")]");
    }

    Directory.CreateDirectory(Path.GetDirectoryName(documentationMappingsRsp)!);
    await File.WriteAllLinesAsync(documentationMappingsRsp, documentationMappingsBuilder, cts.Token).ConfigureAwait(false);
}
catch (OperationCanceledException ex) when (ex.CancellationToken == cts.Token)
{
    return 2;
}

return 0;