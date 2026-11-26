/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Runtime.CompilerServices;
using NanoByte.Common.Native;
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
        if (WindowsUtils.IsWine)
            RegistryUtils.SetSoftwareString(@"Wine\DllOverrides", "d3dx9_43", "native,builtin");

        Configuration.EnableObjectTracking =
#if DEBUG
            true;
#else
            System.Environment.GetEnvironmentVariable(RenderHost.EnvVarNameTrackObjects) == "1";
#endif
    }
}
