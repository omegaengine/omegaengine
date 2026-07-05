/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using NanoByte.Common.Storage;
using OmegaEngine.Foundation.Storage;
using Xunit;

namespace OmegaEngine;

/// <summary>
/// Base class for tests that need a live <see cref="OmegaEngine.Engine"/> with a Direct3D 9 device.
/// </summary>
/// <remarks>Skips gracefully (rather than failing) in headless environments where no device is available.</remarks>
public abstract class EngineTestBase : IDisposable
{
    static EngineTestBase()
    {
        ContentManager.BaseDir = new DirectoryInfo(GetContentDir());
    }

    private static string GetContentDir([CallerFilePath] string thisFile = "")
        => Path.Combine(Paths.Parent(thisFile), "..", "..", "content");

    private readonly Form _form;

    /// <summary>The engine under test, backed by a hidden window.</summary>
    protected Engine Engine { get; }

    protected EngineTestBase()
    {
        Form? form = null;
        try
        {
            form = new Form {Size = new(320, 240)};
            _ = form.Handle; // Force handle creation
            Engine = new Engine(form, new EngineConfig {TargetSize = form.ClientSize});
            _form = form;
        }
        catch (Exception ex)
        {
            form?.Dispose();
            Assert.Skip($"No Direct3D 9 device available: {ex.Message}");
            throw; // Unreachable (Assert.Skip throws), but proves Engine and _form are assigned on all completing paths
        }
    }

    public virtual void Dispose()
    {
        Engine.Cache.Clean();
        Engine.Dispose();
        _form.Dispose();
    }
}
