/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SharpGen.Runtime;
using SharpGen.Runtime.Diagnostics;

namespace OmegaEngine;

/// <summary>
/// Process-wide setup that must happen before any other code in this assembly runs.
/// </summary>
internal static class ModuleInitializer
{
    [ModuleInitializer]
    [SuppressMessage("Usage", "CA2255:The 'ModuleInitializer' attribute should not be used in libraries", Justification = "Native DLLs and object tracking must be set up before any Direct3D call, regardless of which type the application touches first")]
    internal static void Initialize()
    {
        TryLoadLibrary(Environment.GetEnvironmentVariable("D3DX9"));
        TryLoadLibrary(Environment.GetEnvironmentVariable("D3DCOMPILER"));

        try
        {
            // Track all COM objects, so that leaks can be reported on shutdown
            Configuration.EnableObjectTracking = true;
        }
        catch (SharpGenException ex)
        {
            Log.Warn("Could not enable Direct3D object tracking, because COM objects were already created", ex);
            return;
        }

#if !DEBUG
        if (Environment.GetEnvironmentVariable(RenderHost.EnvVarNameTrackObjects) == "1")
#endif
            ObjectTracker.Tracked += comObject => CreationSources.AddOrUpdate(comObject, Environment.StackTrace);
    }

    /// <summary>
    /// The stack traces of the creation of the tracked COM objects.
    /// </summary>
    public static readonly ConditionalWeakTable<CppObject, string> CreationSources = new();

    private static void TryLoadLibrary(string? path)
    {
        if (!string.IsNullOrEmpty(path) && LoadLibrary(path) == 0)
            Log.Warn($"Unable to load {path}: error {Marshal.GetLastWin32Error()}");
    }

    [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern nint LoadLibrary(string lpFileName);
}
