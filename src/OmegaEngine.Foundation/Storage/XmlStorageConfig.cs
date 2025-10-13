/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Drawing;
using System.Xml.Serialization;
using NanoByte.Common.Storage;
using SlimDX;

namespace OmegaEngine.Foundation.Storage;

/// <summary>
/// Global configuration for <see cref="XmlStorage"/>.
/// </summary>
public static class XmlStorageConfig
{
    /// <summary>
    /// Applies the configuration to <see cref="XmlStorage"/>.
    /// </summary>
    /// <remarks>Must be called before <see cref="XmlStorage"/> is used.</remarks>
    public static void Apply()
    {
        XmlStorage.Overrides = GetOverrides();
    }

    private static XmlAttributeOverrides GetOverrides()
    {
        var overrides = new XmlAttributeOverrides();

        // .NET BCL types
        MembersAsAttributes<Point>(nameof(Point.X), nameof(Point.Y));
        MembersAsAttributes<Size>(nameof(Size.Width), nameof(Size.Height));
        MembersAsAttributes<Rectangle>(nameof(Rectangle.X), nameof(Rectangle.Y), nameof(Rectangle.Width), nameof(Rectangle.Height));
        IgnoreMembers<Rectangle>(nameof(Rectangle.Location), nameof(Rectangle.Size));
        IgnoreMembers<Exception>(nameof(Exception.Data));

        // SlimDX types
        MembersAsAttributes<Color3>(nameof(Color3.Red), nameof(Color3.Green), nameof(Color3.Blue));
        MembersAsAttributes<Color4>(nameof(Color4.Alpha), nameof(Color4.Red), nameof(Color4.Green), nameof(Color4.Blue));
        MembersAsAttributes<Vector2>(nameof(Vector2.X), nameof(Vector2.Y));
        MembersAsAttributes<Vector3>(nameof(Vector3.X), nameof(Vector3.Y), nameof(Vector3.Z));
        MembersAsAttributes<Vector4>(nameof(Vector4.X), nameof(Vector4.Y), nameof(Vector4.Z), nameof(Vector4.W));
        MembersAsAttributes<Quaternion>(nameof(Quaternion.X), nameof(Quaternion.Y), nameof(Quaternion.Z), nameof(Quaternion.W));

        return overrides;

        void MembersAsAttributes<T>(params string[] members)
        {
            var type = typeof(T);
            foreach (string member in members)
                overrides.Add(type, member, new() { XmlAttribute = new() });
        }

        void IgnoreMembers<T>(params string[] members)
        {
            var type = typeof(T);
            foreach (string member in members)
                overrides.Add(type, member, new() { XmlIgnore = true });
        }
    }
}
