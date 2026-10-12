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
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using SharpGen.Runtime;
using Vortice.Direct3D9;
using Vortice.Mathematics;

namespace OmegaEngine.D3DX;

/// <summary>
/// Wraps an <c>ID3DXEffect</c>: a set of techniques, passes and parameters loaded from an <c>.fx</c>/<c>.fxo</c> file.
/// </summary>
public sealed unsafe class Effect : ComObject
{
    #region vtable
    // ID3DXBaseEffect (d3dx9effect.h)
    private const int
        VtGetDesc = 3,
        VtGetParameterDesc = 4,
        VtGetTechniqueDesc = 5,
        VtGetPassDesc = 6,
        VtGetParameter = 8,
        VtGetParameterByName = 9,
        VtGetTechnique = 12,
        VtGetTechniqueByName = 13,
        VtGetPass = 14,
        VtGetAnnotation = 18,
        VtSetValue = 20,
        VtGetValue = 21,
        VtSetBool = 22,
        VtGetBool = 23,
        VtSetBoolArray = 24,
        VtSetInt = 26,
        VtGetInt = 27,
        VtSetIntArray = 28,
        VtSetFloat = 30,
        VtGetFloat = 31,
        VtSetFloatArray = 32,
        VtGetFloatArray = 33,
        VtSetVector = 34,
        VtSetVectorArray = 36,
        VtSetMatrix = 38,
        VtSetMatrixArray = 40,
        VtGetString = 51,
        VtSetTexture = 52;

    // ID3DXEffect (d3dx9effect.h)
    private const int
        VtSetTechnique = 58,
        VtGetCurrentTechnique = 59,
        VtBegin = 63,
        VtBeginPass = 64,
        VtCommitChanges = 65,
        VtEndPass = 66,
        VtEnd = 67,
        VtOnLostDevice = 69,
        VtOnResetDevice = 70;
    #endregion

    /// <summary>
    /// Wraps an existing native <c>ID3DXEffect</c> pointer, taking ownership of one reference.
    /// </summary>
    public Effect(nint nativePointer) : base(nativePointer) {}

    #region Creation
    /// <summary>
    /// Loads an effect from an <c>.fx</c> or compiled <c>.fxo</c> file.
    /// </summary>
    /// <exception cref="SharpGenException">The effect could not be loaded or compiled.</exception>
    public static Effect FromFile(IDirect3DDevice9 device, string path, ShaderFlags flags = ShaderFlags.None)
    {
        #region Sanity checks
        if (device == null) throw new ArgumentNullException(nameof(device));
        if (path == null) throw new ArgumentNullException(nameof(path));
        #endregion

        nint effect, errors;
        int hr = D3DX9Native.D3DXCreateEffectFromFileW(device.NativePointer, path, null, 0, (uint)flags, 0, &effect, &errors);
        D3DX9Native.Check(hr, errors);
        return new(effect);
    }

    /// <summary>
    /// Loads an effect from <c>.fx</c> source code or compiled effect bytecode in memory.
    /// </summary>
    /// <exception cref="SharpGenException">The effect could not be loaded or compiled.</exception>
    public static Effect FromMemory(IDirect3DDevice9 device, byte[] data, ShaderFlags flags = ShaderFlags.None)
    {
        #region Sanity checks
        if (device == null) throw new ArgumentNullException(nameof(device));
        if (data == null) throw new ArgumentNullException(nameof(data));
        #endregion

        nint effect, errors;
        fixed (byte* pData = data)
        {
            int hr = D3DX9Native.D3DXCreateEffect(device.NativePointer, pData, (uint)data.Length, null, 0, (uint)flags, 0, &effect, &errors);
            D3DX9Native.Check(hr, errors);
        }
        return new(effect);
    }
    #endregion

    #region Descriptions
    /// <summary>
    /// Describes the effect as a whole.
    /// </summary>
    public EffectDescription Description
    {
        get
        {
            NativeEffectDescription desc;
            Call(VtGetDesc, &desc);
            return new(Marshal.PtrToStringAnsi(desc.Creator), (int)desc.Parameters, (int)desc.Techniques, (int)desc.Functions);
        }
    }

    /// <summary>
    /// Describes a parameter or an annotation.
    /// </summary>
    public ParameterDescription GetParameterDescription(EffectHandle parameter)
    {
        NativeParameterDescription desc;
        Call(VtGetParameterDesc, parameter, &desc);
        return new()
        {
            Name = Marshal.PtrToStringAnsi(desc.Name) ?? "",
            Semantic = Marshal.PtrToStringAnsi(desc.Semantic),
            Class = desc.Class,
            Type = desc.Type,
            Rows = (int)desc.Rows,
            Columns = (int)desc.Columns,
            Elements = (int)desc.Elements,
            Annotations = (int)desc.Annotations,
            StructMembers = (int)desc.StructMembers,
            Flags = (int)desc.Flags,
            Bytes = (int)desc.Bytes
        };
    }

    /// <summary>
    /// Describes a technique.
    /// </summary>
    public TechniqueDescription GetTechniqueDescription(EffectHandle technique)
    {
        NativeTechniqueDescription desc;
        Call(VtGetTechniqueDesc, technique, &desc);
        return new(Marshal.PtrToStringAnsi(desc.Name) ?? "", (int)desc.Passes, (int)desc.Annotations);
    }

    /// <summary>
    /// Describes a pass.
    /// </summary>
    public PassDescription GetPassDescription(EffectHandle pass)
    {
        NativePassDescription desc;
        Call(VtGetPassDesc, pass, &desc);
        return new(Marshal.PtrToStringAnsi(desc.Name) ?? "", (int)desc.Annotations);
    }
    #endregion

    #region Handles
    /// <summary>
    /// Gets a top-level parameter (<paramref name="parent"/> = <c>null</c>) or a struct member by index.
    /// </summary>
    /// <returns>The parameter; <c>null</c> if there is no such parameter.</returns>
    public EffectHandle? GetParameter(EffectHandle? parent, int index)
    {
        nint result = ((delegate* unmanaged[Stdcall]<nint, nint, uint, nint>)this[VtGetParameter])(NativePointer, Ptr(parent), (uint)index);
        GC.KeepAlive(parent);
        return EffectHandle.FromPointer(result);
    }

    /// <summary>
    /// Gets a top-level parameter (<paramref name="parent"/> = <c>null</c>) or a struct member by name.
    /// </summary>
    /// <returns>The parameter; <c>null</c> if there is no such parameter.</returns>
    public EffectHandle? GetParameter(EffectHandle? parent, string name)
    {
        nint pName = Marshal.StringToHGlobalAnsi(name);
        try
        {
            nint result = ((delegate* unmanaged[Stdcall]<nint, nint, nint, nint>)this[VtGetParameterByName])(NativePointer, Ptr(parent), pName);
            GC.KeepAlive(parent);
            return EffectHandle.FromPointer(result);
        }
        finally
        {
            Marshal.FreeHGlobal(pName);
        }
    }

    /// <summary>
    /// Gets a technique by index.
    /// </summary>
    /// <returns>The technique; <c>null</c> if there is no such technique.</returns>
    public EffectHandle? GetTechnique(int index)
        => EffectHandle.FromPointer(((delegate* unmanaged[Stdcall]<nint, uint, nint>)this[VtGetTechnique])(NativePointer, (uint)index));

    /// <summary>
    /// Gets a technique by name.
    /// </summary>
    /// <returns>The technique; <c>null</c> if there is no such technique.</returns>
    public EffectHandle? GetTechnique(string name)
    {
        nint pName = Marshal.StringToHGlobalAnsi(name);
        try
        {
            return EffectHandle.FromPointer(((delegate* unmanaged[Stdcall]<nint, nint, nint>)this[VtGetTechniqueByName])(NativePointer, pName));
        }
        finally
        {
            Marshal.FreeHGlobal(pName);
        }
    }

    /// <summary>
    /// Gets a pass of a technique by index.
    /// </summary>
    /// <returns>The pass; <c>null</c> if there is no such pass.</returns>
    public EffectHandle? GetPass(EffectHandle technique, int index)
    {
        nint result = ((delegate* unmanaged[Stdcall]<nint, nint, uint, nint>)this[VtGetPass])(NativePointer, Ptr(technique), (uint)index);
        GC.KeepAlive(technique);
        return EffectHandle.FromPointer(result);
    }

    /// <summary>
    /// Gets an annotation of a parameter, technique, pass or function by index.
    /// </summary>
    /// <returns>The annotation; <c>null</c> if there is no such annotation.</returns>
    public EffectHandle? GetAnnotation(EffectHandle handle, int index)
    {
        nint result = ((delegate* unmanaged[Stdcall]<nint, nint, uint, nint>)this[VtGetAnnotation])(NativePointer, Ptr(handle), (uint)index);
        GC.KeepAlive(handle);
        return EffectHandle.FromPointer(result);
    }
    #endregion

    #region Get values
    /// <summary>
    /// Gets the value of a parameter or annotation.
    /// </summary>
    public T GetValue<T>(EffectHandle parameter) where T : unmanaged
    {
        if (typeof(T) == typeof(bool))
        {
            int value;
            Call(VtGetBool, parameter, &value);
            bool result = value != 0;
            return Unsafe.As<bool, T>(ref result);
        }
        if (typeof(T) == typeof(int))
        {
            int value;
            Call(VtGetInt, parameter, &value);
            return Unsafe.As<int, T>(ref value);
        }
        if (typeof(T) == typeof(float))
        {
            float value;
            Call(VtGetFloat, parameter, &value);
            return Unsafe.As<float, T>(ref value);
        }

        T ret;
        Call(VtGetValue, parameter, &ret, (uint)sizeof(T));
        return ret;
    }

    /// <summary>
    /// Gets the value of an array parameter or annotation.
    /// </summary>
    public float[] GetValue<T>(EffectHandle parameter, int count) where T : unmanaged
    {
        if (typeof(T) != typeof(float)) throw new NotSupportedException("Only float arrays are supported.");

        var ret = new float[count];
        fixed (float* pRet = ret)
            Call(VtGetFloatArray, parameter, pRet, (uint)count);
        return ret;
    }

    /// <summary>
    /// Gets the value of a string parameter or annotation.
    /// </summary>
    public string GetString(EffectHandle parameter)
    {
        nint value;
        Call(VtGetString, parameter, &value);
        return Marshal.PtrToStringAnsi(value) ?? "";
    }
    #endregion

    #region Set values
    /// <summary>
    /// Sets the value of a parameter.
    /// </summary>
    /// <exception cref="SharpGenException">The value does not match the parameter.</exception>
    public Result SetValue<T>(EffectHandle parameter, T value) where T : unmanaged
    {
        if (typeof(T) == typeof(bool)) return Call(VtSetBool, parameter, Unsafe.As<T, bool>(ref value) ? 1 : 0);
        if (typeof(T) == typeof(int)) return Call(VtSetInt, parameter, Unsafe.As<T, int>(ref value));
        if (typeof(T) == typeof(float)) return Call(VtSetFloat, parameter, Unsafe.As<T, float>(ref value));
        if (typeof(T) == typeof(Matrix4x4)) return Call(VtSetMatrix, parameter, &value);
        if (typeof(T) == typeof(Vector4) || typeof(T) == typeof(Color4)) return Call(VtSetVector, parameter, &value);
        return Call(VtSetValue, parameter, &value, (uint)sizeof(T));
    }

    /// <summary>
    /// Sets the value of an array parameter.
    /// </summary>
    /// <exception cref="SharpGenException">The value does not match the parameter.</exception>
    public Result SetValue<T>(EffectHandle parameter, T[] values) where T : unmanaged
    {
        #region Sanity checks
        if (values == null) throw new ArgumentNullException(nameof(values));
        #endregion

        if (typeof(T) == typeof(bool))
        {
            int* pBools = stackalloc int[values.Length];
            for (int i = 0; i < values.Length; i++) pBools[i] = Unsafe.As<T, bool>(ref values[i]) ? 1 : 0;
            return Call(VtSetBoolArray, parameter, pBools, (uint)values.Length);
        }

        fixed (T* pValues = values)
        {
            if (typeof(T) == typeof(int)) return Call(VtSetIntArray, parameter, pValues, (uint)values.Length);
            if (typeof(T) == typeof(float)) return Call(VtSetFloatArray, parameter, pValues, (uint)values.Length);
            if (typeof(T) == typeof(Matrix4x4)) return Call(VtSetMatrixArray, parameter, pValues, (uint)values.Length);
            if (typeof(T) == typeof(Vector4) || typeof(T) == typeof(Color4)) return Call(VtSetVectorArray, parameter, pValues, (uint)values.Length);
            return Call(VtSetValue, parameter, pValues, (uint)(sizeof(T) * values.Length));
        }
    }

    /// <summary>
    /// Sets a texture parameter.
    /// </summary>
    /// <param name="parameter">The parameter to set.</param>
    /// <param name="texture">The texture to set; <c>null</c> to unset.</param>
    public Result SetTexture(EffectHandle parameter, IDirect3DBaseTexture9? texture)
    {
        var result = Call(VtSetTexture, parameter, texture?.NativePointer ?? 0);
        GC.KeepAlive(texture);
        return result;
    }
    #endregion

    #region Rendering
    /// <summary>
    /// The active technique.
    /// </summary>
    public EffectHandle Technique
    {
        get => EffectHandle.FromPointer(((delegate* unmanaged[Stdcall]<nint, nint>)this[VtGetCurrentTechnique])(NativePointer))!;
        set => Call(VtSetTechnique, value);
    }

    /// <summary>
    /// Starts an active technique.
    /// </summary>
    /// <returns>The number of passes needed to render the technique.</returns>
    public int Begin(FX flags = FX.None)
    {
        uint passes;
        Check(((delegate* unmanaged[Stdcall]<nint, uint*, uint, int>)this[VtBegin])(NativePointer, &passes, (uint)flags));
        return (int)passes;
    }

    /// <summary>
    /// Begins a pass within the active technique.
    /// </summary>
    public void BeginPass(int pass) => Check(((delegate* unmanaged[Stdcall]<nint, uint, int>)this[VtBeginPass])(NativePointer, (uint)pass));

    /// <summary>
    /// Propagates state changes made inside an active pass to the device.
    /// </summary>
    public void CommitChanges() => Check(((delegate* unmanaged[Stdcall]<nint, int>)this[VtCommitChanges])(NativePointer));

    /// <summary>
    /// Ends the active pass.
    /// </summary>
    public void EndPass() => Check(((delegate* unmanaged[Stdcall]<nint, int>)this[VtEndPass])(NativePointer));

    /// <summary>
    /// Ends the active technique.
    /// </summary>
    public void End() => Check(((delegate* unmanaged[Stdcall]<nint, int>)this[VtEnd])(NativePointer));
    #endregion

    #region Device events
    /// <summary>
    /// Releases video memory resources before the device is reset.
    /// </summary>
    public void OnLostDevice() => Check(((delegate* unmanaged[Stdcall]<nint, int>)this[VtOnLostDevice])(NativePointer));

    /// <summary>
    /// Restores video memory resources after the device was reset.
    /// </summary>
    public void OnResetDevice() => Check(((delegate* unmanaged[Stdcall]<nint, int>)this[VtOnResetDevice])(NativePointer));
    #endregion

    #region Helpers
    private static nint Ptr(EffectHandle? handle) => handle?.Pointer ?? 0;

    private static Result Check(int hr)
    {
        var result = new Result(hr);
        result.CheckError();
        return result;
    }

    private void Call(int slot, void* arg)
        => Check(((delegate* unmanaged[Stdcall]<nint, void*, int>)this[slot])(NativePointer, arg));

    private Result Call(int slot, EffectHandle? handle)
    {
        var result = Check(((delegate* unmanaged[Stdcall]<nint, nint, int>)this[slot])(NativePointer, Ptr(handle)));
        GC.KeepAlive(handle);
        return result;
    }

    private Result Call(int slot, EffectHandle? handle, void* arg)
    {
        var result = Check(((delegate* unmanaged[Stdcall]<nint, nint, void*, int>)this[slot])(NativePointer, Ptr(handle), arg));
        GC.KeepAlive(handle);
        return result;
    }

    private Result Call(int slot, EffectHandle? handle, void* arg, uint count)
    {
        var result = Check(((delegate* unmanaged[Stdcall]<nint, nint, void*, uint, int>)this[slot])(NativePointer, Ptr(handle), arg, count));
        GC.KeepAlive(handle);
        return result;
    }

    private Result Call(int slot, EffectHandle? handle, int value)
    {
        var result = Check(((delegate* unmanaged[Stdcall]<nint, nint, int, int>)this[slot])(NativePointer, Ptr(handle), value));
        GC.KeepAlive(handle);
        return result;
    }

    private Result Call(int slot, EffectHandle? handle, float value)
    {
        var result = Check(((delegate* unmanaged[Stdcall]<nint, nint, float, int>)this[slot])(NativePointer, Ptr(handle), value));
        GC.KeepAlive(handle);
        return result;
    }

    private Result Call(int slot, EffectHandle? handle, nint value)
    {
        var result = Check(((delegate* unmanaged[Stdcall]<nint, nint, nint, int>)this[slot])(NativePointer, Ptr(handle), value));
        GC.KeepAlive(handle);
        return result;
    }
    #endregion

    #region Native structs
    [StructLayout(LayoutKind.Sequential)]
    private struct NativeEffectDescription
    {
        public nint Creator;
        public uint Parameters, Techniques, Functions;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeParameterDescription
    {
        public nint Name, Semantic;
        public ParameterClass Class;
        public ParameterType Type;
        public uint Rows, Columns, Elements, Annotations, StructMembers, Flags, Bytes;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeTechniqueDescription
    {
        public nint Name;
        public uint Passes, Annotations;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NativePassDescription
    {
        public nint Name;
        public uint Annotations;
        public nint VertexShaderFunction, PixelShaderFunction;
    }
    #endregion
}

/// <summary>
/// Compiles <c>.fx</c> source code to effect bytecode. Wraps an <c>ID3DXEffectCompiler</c>.
/// </summary>
public sealed unsafe class EffectCompiler : ComObject
{
    // ID3DXEffectCompiler (d3dx9effect.h)
    private const int VtCompileEffect = 59;

    private EffectCompiler(nint nativePointer) : base(nativePointer) {}

    /// <summary>
    /// Parses <c>.fx</c> source code.
    /// </summary>
    /// <param name="source">The HLSL source code of the effect.</param>
    /// <param name="defines">Preprocessor macros.</param>
    /// <param name="flags">Compile options.</param>
    /// <exception cref="SharpGenException">The source code could not be parsed. The message contains the compiler output.</exception>
    public static EffectCompiler FromString(string source, IEnumerable<Macro>? defines = null, ShaderFlags flags = ShaderFlags.None)
    {
        #region Sanity checks
        if (source == null) throw new ArgumentNullException(nameof(source));
        #endregion

        var macros = defines?.ToArray() ?? [];
        var nativeMacros = new D3DXMacro[macros.Length + 1]; // Terminated by an empty entry
        try
        {
            for (int i = 0; i < macros.Length; i++)
            {
                nativeMacros[i].Name = Marshal.StringToHGlobalAnsi(macros[i].Name);
                nativeMacros[i].Definition = Marshal.StringToHGlobalAnsi(macros[i].Definition);
            }

            byte[] sourceBytes = Encoding.UTF8.GetBytes(source);
            nint compiler, errors;
            fixed (byte* pSource = sourceBytes)
            fixed (D3DXMacro* pMacros = nativeMacros)
            {
                int hr = D3DX9Native.D3DXCreateEffectCompiler(pSource, (uint)sourceBytes.Length, pMacros, 0, (uint)flags, &compiler, &errors);
                D3DX9Native.Check(hr, errors);
            }
            return new(compiler);
        }
        finally
        {
            foreach (var macro in nativeMacros)
            {
                if (macro.Name != 0) Marshal.FreeHGlobal(macro.Name);
                if (macro.Definition != 0) Marshal.FreeHGlobal(macro.Definition);
            }
        }
    }

    /// <summary>
    /// Compiles the effect to bytecode that can be loaded with <see cref="Effect.FromMemory"/>.
    /// </summary>
    /// <exception cref="SharpGenException">The effect could not be compiled. The message contains the compiler output.</exception>
    public byte[] CompileEffect(ShaderFlags flags = ShaderFlags.None)
    {
        nint effect = 0, errors = 0;
        int hr = ((delegate* unmanaged[Stdcall]<nint, uint, nint*, nint*, int>)this[VtCompileEffect])(NativePointer, (uint)flags, &effect, &errors);
        if (hr < 0) D3DXBuffer.ReleaseBuffer(effect);
        D3DX9Native.Check(hr, errors);
        return D3DXBuffer.ReadAndRelease(effect);
    }
}

/// <summary>
/// Identifies a parameter, technique, pass or annotation in an <see cref="Effect"/> (<c>D3DXHANDLE</c>).
/// </summary>
/// <remarks>D3DX also accepts the name of a parameter or technique in place of a handle. Use the implicit conversion from <see cref="string"/> for this.</remarks>
public sealed class EffectHandle : IEquatable<EffectHandle>
{
    internal nint Pointer { get; }

    private readonly bool _ownsString;

    private EffectHandle(nint pointer, bool ownsString)
    {
        Pointer = pointer;
        _ownsString = ownsString;
    }

    ~EffectHandle()
    {
        if (_ownsString) Marshal.FreeHGlobal(Pointer);
    }

    internal static EffectHandle? FromPointer(nint pointer) => pointer == 0 ? null : new(pointer, ownsString: false);

    /// <summary>
    /// Refers to a parameter or technique by name.
    /// </summary>
    public static implicit operator EffectHandle(string name) => new(Marshal.StringToHGlobalAnsi(name), ownsString: true);

    /// <inheritdoc/>
    public bool Equals(EffectHandle? other) => other is not null && Pointer == other.Pointer;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is EffectHandle other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => Pointer.GetHashCode();

    public static bool operator ==(EffectHandle? left, EffectHandle? right) => left?.Equals(right) ?? right is null;
    public static bool operator !=(EffectHandle? left, EffectHandle? right) => !(left == right);
}

/// <summary>
/// A preprocessor macro definition for <see cref="EffectCompiler"/>.
/// </summary>
public record struct Macro(string Name, string Definition);

/// <summary>
/// Describes an <see cref="Effect"/> (<c>D3DXEFFECT_DESC</c>).
/// </summary>
public readonly record struct EffectDescription(string? Creator, int Parameters, int Techniques, int Functions);

/// <summary>
/// Describes a technique in an <see cref="Effect"/> (<c>D3DXTECHNIQUE_DESC</c>).
/// </summary>
public readonly record struct TechniqueDescription(string Name, int Passes, int Annotations);

/// <summary>
/// Describes a pass in an <see cref="Effect"/> (<c>D3DXPASS_DESC</c>).
/// </summary>
public readonly record struct PassDescription(string Name, int Annotations);

/// <summary>
/// Describes a parameter or annotation in an <see cref="Effect"/> (<c>D3DXPARAMETER_DESC</c>).
/// </summary>
public readonly record struct ParameterDescription
{
    public string Name { get; init; }
    public string? Semantic { get; init; }
    public ParameterClass Class { get; init; }
    public ParameterType Type { get; init; }
    public int Rows { get; init; }
    public int Columns { get; init; }
    public int Elements { get; init; }
    public int Annotations { get; init; }
    public int StructMembers { get; init; }
    public int Flags { get; init; }
    public int Bytes { get; init; }
}

/// <summary>
/// The class of an effect parameter (<c>D3DXPARAMETER_CLASS</c>).
/// </summary>
public enum ParameterClass
{
    Scalar,
    Vector,
    MatrixRows,
    MatrixColumns,
    Object,
    Struct
}

/// <summary>
/// The type of an effect parameter (<c>D3DXPARAMETER_TYPE</c>).
/// </summary>
public enum ParameterType
{
    Void,
    Bool,
    Int,
    Float,
    String,
    Texture,
    Texture1D,
    Texture2D,
    Texture3D,
    TextureCube,
    Sampler,
    Sampler1D,
    Sampler2D,
    Sampler3D,
    SamplerCube,
    PixelShader,
    VertexShader,
    PixelFragment,
    VertexFragment,
    Unsupported
}

/// <summary>
/// Options for compiling shaders (<c>D3DXSHADER</c>).
/// </summary>
[Flags]
public enum ShaderFlags
{
    None = 0,
    Debug = 1 << 0,
    SkipValidation = 1 << 1,
    SkipOptimization = 1 << 2,
    PackMatrixRowMajor = 1 << 3,
    PackMatrixColumnMajor = 1 << 4,
    PartialPrecision = 1 << 5,
    AvoidFlowControl = 1 << 9,
    PreferFlowControl = 1 << 10,
    EnableBackwardsCompatibility = 1 << 12,
    IeeeStrictness = 1 << 13
}

/// <summary>
/// Options for <see cref="Effect.Begin"/> (<c>D3DXFX</c>).
/// </summary>
[Flags]
public enum FX
{
    None = 0,
    DoNotSaveState = 1 << 0,
    DoNotSaveShaderState = 1 << 1,
    DoNotSaveSamplerState = 1 << 2
}
