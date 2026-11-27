/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SlimDX;

namespace OmegaEngine;

/// <summary>
/// Process-wide setup that must happen before any other code in this assembly runs.
/// </summary>
internal static class ModuleInitializer
{
    [ModuleInitializer]
    internal static void Initialize()
    {
        TryLoadLibrary(Environment.GetEnvironmentVariable("D3DX9"));
        TryLoadLibrary(Environment.GetEnvironmentVariable("D3DCOMPILER"));

        // Record stack traces of SlimDX object creations
        Configuration.EnableObjectTracking =
#if DEBUG
            true;
#else
            Environment.GetEnvironmentVariable(RenderHost.EnvVarNameTrackObjects) == "1";
#endif
    }

    private static void TryLoadLibrary(string? path)
    {
        if (!string.IsNullOrEmpty(path) && LoadLibrary(path) == 0)
            Log.Warn($"Unable to load {path}: error {Marshal.GetLastWin32Error()}");
    }

    [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern nint LoadLibrary(string lpFileName);
}
