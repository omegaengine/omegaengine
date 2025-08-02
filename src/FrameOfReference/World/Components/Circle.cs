/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

using System.ComponentModel;
using System.Xml.Serialization;
using AlphaFramework.World.Components;
using OmegaEngine.Values;
using SlimDX;

namespace FrameOfReference.World.Components;

/// <summary>
/// Collision-detection using a simple uniform circle.
/// </summary>
public class Circle : Collision<Vector2>
{
    /// <summary>
    /// The radius of the circle.
    /// </summary>
    [DefaultValue(0f), Description("The radius of the circle.")]
    [XmlAttribute]
    public float Radius { get; set; }

    /// <summary>
    /// Determines whether a certain point lies within a circle.
    /// </summary>
    /// <param name="point">The point to check for collision in entity space.</param>
    /// <param name="rotation">This is ignored for circles.</param>
    /// <returns><c>true</c> if the <paramref name="point"/> does collide with the circle, <c>false</c>.</returns>
    public override bool CollisionTest(Vector2 point, float rotation)
    {
        // Empty or negative circles can never intersect
        if (Radius <= 0) return false;

        return point.Length() < Radius;
    }

    /// <summary>
    /// Determines whether a certain area lies within a circle.
    /// </summary>
    /// <param name="area">The area to check for collision in entity space.</param>
    /// <param name="rotation">This is ignored for circles.</param>
    /// <returns><c>true</c> if <paramref name="area"/> does collide with the circle, <c>false</c>.</returns>
    public override bool CollisionTest(Quadrangle area, float rotation)
    {
        // Empty or negative circles can never intersect
        if (Radius <= 0) return false;

        // Shift area to the circle center as the origin
        return area.IntersectCircle(Radius);
    }
}
