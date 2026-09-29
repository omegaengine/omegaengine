/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NanoByte.Common.Streams;
using OmegaEngine.Foundation.Storage;
using SlimDX;
using SlimDX.Direct3D9;
using Resources = OmegaEngine.Properties.Resources;

namespace OmegaEngine.Graphics.Shaders;

/// <summary>
/// Helper class for compiling <see cref="Shader"/> code at runtime
/// </summary>
/// <remarks>Uses .fx files containing HLSL that generates different variants depending on preprocessor defines.</remarks>
public static class DynamicShader
{
    /// <summary>
    /// Loads a dynamic shader file via the <see cref="ContentManager"/> and compiles it.
    /// </summary>
    /// <param name="id">The ID of the shader to be loaded</param>
    /// <param name="defines">Preprocessor macros (name and definition) that select the variant of the shader to compile</param>
    /// <returns>The compiled effect bytecode</returns>
    /// <exception cref="ShaderCompileException">The shader code could not be compiled.</exception>
    public static byte[] FromContent(string id, IReadOnlyDictionary<string, string> defines)
    {
        #region Sanity checks
        if (string.IsNullOrEmpty(id)) throw new ArgumentNullException(nameof(id));
        if (defines == null) throw new ArgumentNullException(nameof(defines));
        #endregion

        return Compile(File.ReadAllText(ContentManager.GetFilePath("Graphics/Shaders", id)), defines);
    }

    private static byte[] Compile(string fxCode, IReadOnlyDictionary<string, string> defines)
    {
        var macros = defines.Select(x => new Macro {Name = x.Key, Definition = x.Value}).ToArray();
        try
        {
            using var compiler = EffectCompiler.FromStream(fxCode.ToStream(), macros, includeFile: null, ShaderFlags.None);
            using var stream = compiler.CompileEffect(ShaderFlags.None);
            stream.Position = 0;
            var bytecode = new byte[stream.Length];
            stream.Read(bytecode, 0, bytecode.Length); // Copy to managed array to avoid memory leak if not disposed
            return bytecode;
        }
        catch (Exception ex) when (ex is CompilationException or Direct3D9Exception)
        {
            string definesText = string.Join(", ", defines.Select(x => $"{x.Key}={x.Value}"));
            throw new ShaderCompileException($"{Resources.DynamicShaderCompileFail} ({definesText})", ex, fxCode);
        }
    }
}
