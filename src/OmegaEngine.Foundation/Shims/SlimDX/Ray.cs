// Decompiled from SlimDX
//
// Copyright (c) 2007-2011 SlimDX Group
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
#nullable disable

using System;
using System.Globalization;
using System.Runtime.InteropServices;

// ReSharper disable once CheckNamespace
namespace SlimDX;

public struct Ray(Vector3 position, Vector3 direction) : IEquatable<Ray>
{
  public Vector3 Position = position;
  public Vector3 Direction = direction;

  [return: MarshalAs(UnmanagedType.U1)]
  public static bool Intersects(Ray ray, Plane plane, out float distance)
  {
    ray.Direction.Normalize();
    float num1 = (float) (plane.Normal.Y * (double) ray.Direction.Y + plane.Normal.X * (double) ray.Direction.X + plane.Normal.Z * (double) ray.Direction.Z);
    if (Math.Abs(num1) < 9.999999974752427E-07)
    {
      distance = 0.0f;
      return false;
    }
    float num2 = (-plane.D - (float) (ray.Position.Y * (double) plane.Normal.Y + ray.Position.X * (double) plane.Normal.X + ray.Position.Z * (double) plane.Normal.Z)) / num1;
    if (num2 < 0.0)
    {
      if (num2 < -9.999999974752427E-07)
      {
        distance = 0.0f;
        return false;
      }
      num2 = 0.0f;
    }
    distance = num2;
    return true;
  }

  public static bool operator ==(Ray left, Ray right) => Equals(ref left, ref right);

  public static bool operator !=(Ray left, Ray right) => !Equals(ref left, ref right);

  public override string ToString()
  {
    return string.Format(CultureInfo.CurrentCulture, "Position:{0} Direction:{1}", [Position.ToString(), Direction.ToString()]);
  }

  public override int GetHashCode()
  {
    return Direction.GetHashCode() + Position.GetHashCode();
  }

  [return: MarshalAs(UnmanagedType.U1)]
  public static bool Equals(ref Ray value1, ref Ray value2)
  {
    return value1.Position == value2.Position && value1.Direction == value2.Direction;
  }

  [return: MarshalAs(UnmanagedType.U1)]
  public bool Equals(Ray other)
  {
    return Position == other.Position && Direction == other.Direction;
  }

  [return: MarshalAs(UnmanagedType.U1)]
  public override bool Equals(object obj)
  {
    return obj != null && !(obj.GetType() != GetType()) && Equals((Ray) obj);
  }
}
