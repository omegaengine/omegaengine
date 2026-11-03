/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using Vortice.Direct3D9;

namespace OmegaEngine.Graphics.Meshes;

/// <summary>
/// Helper methods for vertex declarations (<see cref="VertexElement"/> arrays).
/// </summary>
public static class VertexElements
{
    /// <summary>
    /// Gets the size of a single vertex element in bytes.
    /// </summary>
    public static int GetSize(this DeclarationType type) => type switch
    {
        DeclarationType.Float1 or DeclarationType.Color or DeclarationType.Ubyte4 or DeclarationType.Short2 or DeclarationType.UByte4N
            or DeclarationType.Short2N or DeclarationType.UShort2N or DeclarationType.UDec3 or DeclarationType.Dec3N or DeclarationType.HalfTwo => 4,
        DeclarationType.Float2 or DeclarationType.Short4 or DeclarationType.Short4N or DeclarationType.UShort4N or DeclarationType.HalfFour => 8,
        DeclarationType.Float3 => 12,
        DeclarationType.Float4 => 16,
        _ => 0
    };

    /// <summary>
    /// Gets the size of a single vertex in bytes.
    /// </summary>
    /// <param name="declaration">The vertex declaration, optionally including the <see cref="VertexElement.VertexDeclarationEnd"/> marker.</param>
    public static int GetVertexSize(this VertexElement[] declaration)
    {
        #region Sanity checks
        if (declaration == null) throw new ArgumentNullException(nameof(declaration));
        #endregion

        return declaration.Where(x => x.Stream == 0).Select(x => x.Offset + x.Type.GetSize()).DefaultIfEmpty(0).Max();
    }

    /// <summary>
    /// Finds an element in a vertex declaration.
    /// </summary>
    /// <returns>The element; <c>null</c> if there is none for the <paramref name="usage"/> and <paramref name="usageIndex"/>.</returns>
    public static VertexElement? Find(this VertexElement[] declaration, DeclarationUsage usage, int usageIndex = 0)
    {
        #region Sanity checks
        if (declaration == null) throw new ArgumentNullException(nameof(declaration));
        #endregion

        foreach (var element in declaration)
        {
            if (element.Stream == 0 && element.Usage == usage && element.UsageIndex == usageIndex)
                return element;
        }
        return null;
    }

    /// <summary>
    /// Builds the vertex declaration equivalent to a fixed-function vertex format.
    /// </summary>
    /// <returns>The declaration, including the <see cref="VertexElement.VertexDeclarationEnd"/> marker.</returns>
    /// <exception cref="NotSupportedException"><paramref name="format"/> uses vertex blending or texture coordinates that are not 2D.</exception>
    public static VertexElement[] FromFormat(VertexFormat format)
    {
        var elements = new List<VertexElement>();
        short offset = 0;

        void Add(DeclarationType type, DeclarationUsage usage, byte usageIndex = 0)
        {
            elements.Add(new(0, offset, type, DeclarationMethod.Default, usage, usageIndex));
            offset += (short)type.GetSize();
        }

        switch (format & VertexFormat.PositionMask)
        {
            case 0:
                break;
            case VertexFormat.Position:
                Add(DeclarationType.Float3, DeclarationUsage.Position);
                break;
            case VertexFormat.PositionRhw:
                Add(DeclarationType.Float4, DeclarationUsage.PositionTransformed);
                break;
            default:
                throw new NotSupportedException($"Unsupported position format in {format}");
        }

        if (format.HasFlag(VertexFormat.Normal)) Add(DeclarationType.Float3, DeclarationUsage.Normal);
        if (format.HasFlag(VertexFormat.PointSize)) Add(DeclarationType.Float1, DeclarationUsage.PointSize);
        if (format.HasFlag(VertexFormat.Diffuse)) Add(DeclarationType.Color, DeclarationUsage.Color);
        if (format.HasFlag(VertexFormat.Specular)) Add(DeclarationType.Color, DeclarationUsage.Color, 1);

        if (((int)format & ~0xFFFF) != 0) throw new NotSupportedException($"Only 2D texture coordinates are supported: {format}");
        int textureCount = ((int)format & (int)VertexFormat.TextureCountMask) >> (int)VertexFormat.TextureCountShift;
        for (int i = 0; i < textureCount; i++)
            Add(DeclarationType.Float2, DeclarationUsage.TextureCoordinate, (byte)i);

        elements.Add(VertexElement.VertexDeclarationEnd);
        return elements.ToArray();
    }

    /// <summary>
    /// Determines the fixed-function vertex format equivalent to a vertex declaration.
    /// </summary>
    /// <returns>The format; <see cref="VertexFormat.None"/> if there is no equivalent.</returns>
    public static VertexFormat ToFormat(this VertexElement[] declaration)
    {
        #region Sanity checks
        if (declaration == null) throw new ArgumentNullException(nameof(declaration));
        #endregion

        var format = VertexFormat.None;
        int textureCount = 0;
        foreach (var element in declaration)
        {
            if (element.Stream != 0) continue;
            switch (element.Usage, element.UsageIndex)
            {
                case (DeclarationUsage.Position, 0):
                    format |= VertexFormat.Position;
                    break;
                case (DeclarationUsage.PositionTransformed, 0):
                    format |= VertexFormat.PositionRhw;
                    break;
                case (DeclarationUsage.Normal, 0):
                    format |= VertexFormat.Normal;
                    break;
                case (DeclarationUsage.PointSize, 0):
                    format |= VertexFormat.PointSize;
                    break;
                case (DeclarationUsage.Color, 0):
                    format |= VertexFormat.Diffuse;
                    break;
                case (DeclarationUsage.Color, 1):
                    format |= VertexFormat.Specular;
                    break;
                case (DeclarationUsage.TextureCoordinate, _):
                    textureCount++;
                    break;
                default:
                    return VertexFormat.None;
            }
        }
        format |= (VertexFormat)(textureCount << (int)VertexFormat.TextureCountShift);

        // Only accept the format if it reproduces exactly the same layout
        try
        {
            return FromFormat(format).Where(x => x.Stream == 0).SequenceEqual(declaration.Where(x => x.Stream == 0))
                ? format
                : VertexFormat.None;
        }
        catch (NotSupportedException)
        {
            return VertexFormat.None;
        }
    }
}
