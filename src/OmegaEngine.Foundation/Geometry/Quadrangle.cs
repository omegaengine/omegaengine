/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.Globalization;
using NanoByte.Common;
using OmegaEngine.Foundation.Design;
using SlimDX;

namespace OmegaEngine.Foundation.Geometry;

/// <summary>
/// A 2D polygon consisting of four points.
/// </summary>
[TypeConverter(typeof(QuadrangleConverter))]
public readonly struct Quadrangle(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4) : IEquatable<Quadrangle>
{
    /// <summary>
    /// The coordinates of the first point; counter-clockwise ordering recommended.
    /// </summary>
    [Description("The coordinates of the first point; counter-clockwise ordering recommended.")]
    public Vector2 P1 { get; } = p1;

    /// <summary>
    /// The coordinates of the second point; counter-clockwise ordering recommended.
    /// </summary>
    [Description("The coordinates of the second point; counter-clockwise ordering recommended.")]
    public Vector2 P2 { get; } = p2;

    /// <summary>
    /// The coordinates of the third point; counter-clockwise ordering recommended.
    /// </summary>
    [Description("The coordinates of the third point; counter-clockwise ordering recommended.")]
    public Vector2 P3 { get; } = p3;

    /// <summary>
    /// The coordinates of the fourth point; counter-clockwise ordering recommended.
    /// </summary>
    [Description("The coordinates of the fourth point; counter-clockwise ordering recommended.")]
    public Vector2 P4 { get; } = p4;

    /// <summary>
    /// The edge from <see cref="P1"/> to <see cref="P2"/>.
    /// </summary>
    public Vector2Ray Edge1 => new(P1, P2 - P1);

    /// <summary>
    /// The edge from <see cref="P2"/> to <see cref="P3"/>.
    /// </summary>
    public Vector2Ray Edge2 => new(P2, P3 - P2);

    /// <summary>
    /// The edge from <see cref="P3"/> to <see cref="P4"/>.
    /// </summary>
    public Vector2Ray Edge3 => new(P3, P4 - P3);

    /// <summary>
    /// The edge from <see cref="P4"/> to <see cref="P1"/>.
    /// </summary>
    public Vector2Ray Edge4 => new(P4, P1 - P4);

    /// <summary>
    /// Creates a new quadrangle. Counter-clockwise ordering is recommended.
    /// </summary>
    public Quadrangle(float p1X, float p1Y, float p2X, float p2Y, float p3X, float p3Y, float p4X, float p4Y)
        : this(
            new(p1X, p1Y),
            new(p2X, p2Y),
            new(p3X, p3Y),
            new(p4X, p4Y))
    {}

    /// <summary>
    /// Creates a new quadrangle from a simple rectangle.
    /// </summary>
    public Quadrangle(RectangleF rectangle)
        : this(
            new(rectangle.Left, rectangle.Top),
            new(rectangle.Left, rectangle.Bottom),
            new(rectangle.Right, rectangle.Bottom),
            new(rectangle.Right, rectangle.Top))
    {}

    /// <summary>
    /// Returns a new <see cref="Quadrangle"/> shifted by <paramref name="distance"/>.
    /// </summary>
    /// <param name="distance">This value is added to each corner position.</param>
    /// <returns>The shifted <see cref="Quadrangle"/>.</returns>
    [Pure]
    public Quadrangle Offset(Vector2 distance) => new(
        P1 + distance, P2 + distance,
        P3 + distance, P4 + distance);

    /// <summary>
    /// Returns a new <see cref="Quadrangle"/> rotated by <paramref name="rotation"/> around the origin.
    /// </summary>
    /// <param name="rotation">The angle to rotate by in degrees.</param>
    /// <returns>The rotated <see cref="Quadrangle"/>.</returns>
    [Pure]
    public Quadrangle Rotate(float rotation)
    {
        var radians = rotation.DegreeToRadian();
        return new (
            P1.Rotate(radians), P2.Rotate(radians),
            P3.Rotate(radians), P4.Rotate(radians));
    }

    #region Intersect
    #region Point
    /// <summary>
    /// If the points are stored counter-clockwise and form a convex polygon, this will test if a point lies inside it.
    /// </summary>
    /// <param name="point">The point to test for intersection.</param>
    /// <returns><c>true</c> if <paramref name="point"/> lies within the quadrangle.</returns>
    public bool IntersectWith(Vector2 point)
    {
        // Check if the point lies on the outside of any of the lines
        if ((point.Y - P1.Y) * (P2.X - P1.X) - (point.X - P1.X) * (P2.Y - P1.Y) > 0) return false;
        if ((point.Y - P2.Y) * (P3.X - P2.X) - (point.X - P2.X) * (P3.Y - P2.Y) > 0) return false;
        if ((point.Y - P3.Y) * (P4.X - P3.X) - (point.X - P3.X) * (P4.Y - P3.Y) > 0) return false;
        if ((point.Y - P4.Y) * (P1.X - P4.X) - (point.X - P4.X) * (P1.Y - P4.Y) > 0) return false;

        // If not, it must be inside the quadrangle
        return true;
    }
    #endregion

    #region RectangleF
    /// <summary>
    /// If the points are stored counter-clockwise and form a convex polygon, this will test if a rectangle lies inside it.
    /// </summary>
    /// <param name="rectangle">The rectangle to test for intersection.</param>
    /// <returns><c>true</c> if <paramref name="rectangle"/> lies within the quadrangle.</returns>
    public bool IntersectWith(RectangleF rectangle)
    {
        // Check if any corner of the quadrangle lies within the rectangle
        if (rectangle.Contains(P1.X, P1.Y)) return true;
        if (rectangle.Contains(P2.X, P2.Y)) return true;
        if (rectangle.Contains(P3.X, P3.Y)) return true;
        if (rectangle.Contains(P4.X, P4.Y)) return true;

        // Check if the quadrangle has the size 0
        if (P1 == P2 && P2 == P3 && P3 == P4) return false;

        // Check if any corner of the rectangle lies within the quadrangle
        if (IntersectWith(new Vector2(rectangle.Left, rectangle.Top))) return true;
        if (IntersectWith(new Vector2(rectangle.Right, rectangle.Top))) return true;
        if (IntersectWith(new Vector2(rectangle.Right, rectangle.Bottom))) return true;
        if (IntersectWith(new Vector2(rectangle.Left, rectangle.Bottom))) return true;

        // If neither, it must be outside of the quadrangle
        return false;
    }
    #endregion

    #region Quadrangle
    /// <summary>
    /// This will test if two quadrangles intersect with each other. Only works if both quadrangles are counter-clockwise and form a convex polygon.
    /// </summary>
    /// <param name="quadrangle">The other quadrangle to test for intersection.</param>
    /// <returns><c>true</c> if <paramref name="quadrangle"/> intersects with this quadrangle.</returns>
    public bool IntersectWith(Quadrangle quadrangle)
        // ToDo: Optimize
        => IntersectWith(quadrangle.P1)
        || IntersectWith(quadrangle.P2)
        || IntersectWith(quadrangle.P3)
        || IntersectWith(quadrangle.P4)
        || quadrangle.IntersectWith(P1); // this fully inside quadrangle
    #endregion

    #region Circle
    /// <summary>
    /// If the points are stored counter-clockwise and form a convex polygon, this will test if a circle with the origin (0;0) lies inside it.
    /// </summary>
    /// <param name="radius">The rectangle to test for intersection.</param>
    /// <returns><c>true</c> if the circle lies within the quadrangle.</returns>
    public bool IntersectCircle(float radius)
    {
        // Check if a part of the circle lies within the quadrangle
        if (P1.Length() < radius || P2.Length() < radius || P3.Length() < radius || P4.Length() < radius) return true;

        // Check if the quadrangle has the size 0
        if (P1 == P2 && P2 == P3 && P3 == P4) return false;

        // Check if the circle lies within the quadrangle completely
        if (IntersectWith(new Vector2())) return true;

        // If neither, it must be outside of the quadrangle
        return false;
    }
    #endregion
    #endregion

    /// <inheritdoc/>
    public override string ToString() => string.Format(CultureInfo.InvariantCulture, "({0}, {1}, {2}, {3})", P1, P2, P3, P4);

    /// <inheritdoc/>
    public bool Equals(Quadrangle other) => other.P1 == P1 && other.P2 == P2 && other.P3 == P3 && other.P4 == P4;

    public static bool operator ==(Quadrangle left, Quadrangle right) => left.Equals(right);
    public static bool operator !=(Quadrangle left, Quadrangle right) => !left.Equals(right);

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        return obj is Quadrangle quadrangle && Equals(quadrangle);
    }

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(P1, P2, P3, P4);
}
