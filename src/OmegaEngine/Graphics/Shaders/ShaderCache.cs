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
using System.Security.Cryptography;
using System.Text;
using SlimDX.Direct3D9;

namespace OmegaEngine.Graphics.Shaders;

/// <summary>
/// Caches compiled effect bytecode on disk, so that shaders compiled at runtime do not have to be recompiled after a restart.
/// </summary>
/// <param name="directory">The directory to store the bytecode in. Created on demand.</param>
/// <remarks>This class is thread-safe.</remarks>
/// <seealso cref="DynamicShader.Cache"/>
public sealed class ShaderCache(string directory)
{
    /// <summary>Change this whenever the key derivation or the file layout changes, to leave behind old entries.</summary>
    private const string FormatVersion = "1";

    /// <summary>Entries start with a hash of the bytecode, to detect truncated or corrupted files.</summary>
    private const int ChecksumLength = 32;

    /// <summary>
    /// The directory the bytecode is stored in.
    /// </summary>
    public string Directory { get; } = directory ?? throw new ArgumentNullException(nameof(directory));

    /// <summary>
    /// Returns cached bytecode for a shader, or compiles and caches it if there is none yet.
    /// </summary>
    /// <param name="source">The shader source code.</param>
    /// <param name="defines">The preprocessor defines the shader is compiled with.</param>
    /// <param name="compile">Compiles <paramref name="source"/> with <paramref name="defines"/>. Called only if there is no usable cache entry.</param>
    /// <returns>The effect bytecode.</returns>
    public byte[] GetOrAdd(string source, IReadOnlyDictionary<string, string> defines, Func<byte[]> compile)
    {
        #region Sanity checks
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (defines == null) throw new ArgumentNullException(nameof(defines));
        if (compile == null) throw new ArgumentNullException(nameof(compile));
        #endregion

        string path = Path.Combine(Directory, $"{GetKey(source, defines)}.fxo");
        if (TryRead(path) is {} cached) return cached;

        var bytecode = compile();
        TryWrite(path, bytecode);
        return bytecode;
    }

    private static string GetKey(string source, IReadOnlyDictionary<string, string> defines)
    {
        var builder = new StringBuilder();
        builder.Append(FormatVersion).Append('\0');
        builder.Append(typeof(EffectCompiler).Assembly.GetName().Version).Append('\0'); // Determines the D3DX compiler version
        foreach (var define in defines.OrderBy(x => x.Key, StringComparer.Ordinal))
            builder.Append(define.Key).Append('=').Append(define.Value).Append('\0');
        builder.Append(source);
        return ToHex(Hash(Encoding.UTF8.GetBytes(builder.ToString())));
    }

    private static byte[]? TryRead(string path)
    {
        try
        {
            if (!File.Exists(path)) return null;

            var data = File.ReadAllBytes(path);
            if (data.Length > ChecksumLength)
            {
                var bytecode = new byte[data.Length - ChecksumLength];
                Buffer.BlockCopy(data, ChecksumLength, bytecode, 0, bytecode.Length);
                if (Hash(bytecode).SequenceEqual(data.Take(ChecksumLength)))
                    return bytecode;
            }

            Log.Warn($"Ignoring corrupted shader cache entry: {path}");
            return null;
        }
        #region Error handling
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            Log.Warn($"Failed to read shader cache entry: {path}", ex);
            return null;
        }
        #endregion
    }

    private void TryWrite(string path, byte[] bytecode)
    {
        string tempPath = $"{path}.{Guid.NewGuid():N}.tmp";
        try
        {
            System.IO.Directory.CreateDirectory(Directory);

            // Write to a temporary file first, so that other threads and processes never see a partially written entry
            using (var stream = File.Create(tempPath))
            {
                stream.Write(Hash(bytecode), 0, ChecksumLength);
                stream.Write(bytecode, 0, bytecode.Length);
            }

            if (File.Exists(path)) File.Delete(path); // Replaces a corrupted entry
            File.Move(tempPath, path);
        }
        #region Error handling        
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // Another thread or process may have written the same entry concurrently
            Log.Info($"Failed to write shader cache entry: {path}", ex);
            try
            {
                File.Delete(tempPath);
            }
            catch (Exception ex2) when (ex2 is IOException or UnauthorizedAccessException)
            {}
        }
        #endregion
    }

    private static byte[] Hash(byte[] data)
    {
        using var sha = SHA256.Create();
        return sha.ComputeHash(data);
    }

    private static string ToHex(byte[] data)
        => BitConverter.ToString(data).Replace("-", "").ToLowerInvariant();
}
