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

// ReSharper disable once CheckNamespace
namespace SlimDX;

public struct Plane : IEquatable<Plane>
{
  public Vector3 Normal;
  public float D;

  public Plane(Vector4 value)
  {
    Normal = new(value.X, value.Y, value.Z);
    D = value.W;
  }

  public Plane(Vector3 point1, Vector3 point2, Vector3 point3)
  {
    float num1 = point2.X - point1.X;
    float num2 = point2.Y - point1.Y;
    float num3 = point2.Z - point1.Z;
    float num4 = point3.X - point1.X;
    float num5 = point3.Y - point1.Y;
    float num6 = point3.Z - point1.Z;
    float num7 = (float) (num6 * (double) num2 - num5 * (double) num3);
    float num8 = (float) (num4 * (double) num3 - num6 * (double) num1);
    float num9 = (float) (num5 * (double) num1 - num4 * (double) num2);
    double num10 = num8;
    double num11 = num7;
    double num12 = num9;
    double num13 = num11;
    double num14 = num13 * num13;
    double num15 = num10;
    double num16 = num15 * num15;
    double num17 = num14 + num16;
    double num18 = num12;
    double num19 = num18 * num18;
    float num20 = (float) (1.0 / Math.Sqrt(num17 + num19));
    float num21 = num20 * num7;
    Normal.X = num21;
    float num22 = num20 * num8;
    Normal.Y = num22;
    float num23 = num20 * num9;
    Normal.Z = num23;
    D = (float) -(point1.Y * (double) num22 + point1.X * (double) num21 + point1.Z * (double) num23);
  }

  public Plane(Vector3 point, Vector3 normal)
  {
    Normal = normal;
    D = -Vector3.Dot(normal, point);
  }

  public Plane(Vector3 normal, float d)
  {
    Normal = normal;
    D = d;
  }

  public Plane(float a, float b, float c, float d)
  {
    Normal = new(a, b, c);
    D = d;
  }

  public static float Dot(Plane plane, Vector4 point)
  {
    return (float) (plane.Normal.Y * (double) point.Y + plane.Normal.X * (double) point.X + plane.Normal.Z * (double) point.Z + point.W * (double) plane.D);
  }

  public static float DotCoordinate(Plane plane, Vector3 point)
  {
    return (float) (plane.Normal.Y * (double) point.Y + plane.Normal.X * (double) point.X + plane.Normal.Z * (double) point.Z) + plane.D;
  }

  public static float DotNormal(Plane plane, Vector3 point)
  {
    return (float) (plane.Normal.Y * (double) point.Y + plane.Normal.X * (double) point.X + plane.Normal.Z * (double) point.Z);
  }

  public static void Normalize(ref Plane plane, out Plane result)
  {
    double y = plane.Normal.Y;
    double x = plane.Normal.X;
    double z = plane.Normal.Z;
    double num1 = x;
    double num2 = num1 * num1;
    double num3 = y;
    double num4 = num3 * num3;
    double num5 = num2 + num4;
    double num6 = z;
    double num7 = num6 * num6;
    float num8 = (float) (1.0 / Math.Sqrt(num5 + num7));
    float num9 = plane.D * num8;
    Vector3 vector3 = new(plane.Normal.X * num8, plane.Normal.Y * num8, plane.Normal.Z * num8);
    Plane plane1;
    plane1.Normal = vector3;
    plane1.D = num9;
    result = plane1;
  }

  public static Plane Normalize(Plane plane)
  {
    double y = plane.Normal.Y;
    double x = plane.Normal.X;
    double z = plane.Normal.Z;
    double num1 = x;
    double num2 = num1 * num1;
    double num3 = y;
    double num4 = num3 * num3;
    double num5 = num2 + num4;
    double num6 = z;
    double num7 = num6 * num6;
    float num8 = (float) (1.0 / Math.Sqrt(num5 + num7));
    float num9 = plane.D * num8;
    Vector3 vector3 = new(plane.Normal.X * num8, plane.Normal.Y * num8, plane.Normal.Z * num8);
    Plane plane1;
    plane1.Normal = vector3;
    plane1.D = num9;
    return plane1;
  }

  public void Normalize()
  {
    double y = Normal.Y;
    double x = Normal.X;
    double z = Normal.Z;
    double num1 = x;
    double num2 = num1 * num1;
    double num3 = y;
    double num4 = num3 * num3;
    double num5 = num2 + num4;
    double num6 = z;
    double num7 = num6 * num6;
    float num8 = (float) (1.0 / Math.Sqrt(num5 + num7));
    Normal.X *= num8;
    Normal.Y *= num8;
    Normal.Z *= num8;
    D *= num8;
  }

  public static Plane[] Transform(Plane[] planes, ref Quaternion rotation)
  {
    int length = planes?.Length ?? throw new ArgumentNullException(nameof (planes));
    Plane[] planeArray = new Plane[length];
    double x1 = rotation.X;
    float num1 = (float) (x1 + x1);
    double y1 = rotation.Y;
    float num2 = (float) (y1 + y1);
    double z1 = rotation.Z;
    float num3 = (float) (z1 + z1);
    float num4 = rotation.W * num1;
    float num5 = rotation.W * num2;
    float num6 = rotation.W * num3;
    float num7 = rotation.X * num1;
    float num8 = rotation.X * num2;
    float num9 = rotation.X * num3;
    float num10 = rotation.Y * num2;
    float num11 = rotation.Y * num3;
    float num12 = rotation.Z * num3;
    int index = 0;
    if (0 < length)
    {
      double num13 = 1.0 - num10 - num12;
      double num14 = num8 - (double) num6;
      double num15 = num9 + (double) num5;
      double num16 = 1.0 - num7;
      double num17 = num16 - num12;
      double num18 = num8 + (double) num6;
      double num19 = num11 - (double) num4;
      double num20 = num11 + (double) num4;
      double num21 = num9 - (double) num5;
      double num22 = num16 - num10;
      do
      {
        float x2 = planes[index].Normal.X;
        float y2 = planes[index].Normal.Y;
        float z2 = planes[index].Normal.Z;
        planeArray[index] = new()
        {
          Normal = {
            X = (float) (y2 * num14 + x2 * num13 + z2 * num15),
            Y = (float) (x2 * num18 + y2 * num17 + z2 * num19),
            Z = (float) (x2 * num21 + y2 * num20 + z2 * num22)
          },
          D = planes[index].D
        };
        ++index;
      }
      while (index < length);
    }
    return planeArray;
  }

  public static void Transform(ref Plane plane, ref Quaternion rotation, out Plane result)
  {
    double x1 = rotation.X;
    float num1 = (float) (x1 + x1);
    double y1 = rotation.Y;
    float num2 = (float) (y1 + y1);
    double z1 = rotation.Z;
    float num3 = (float) (z1 + z1);
    float num4 = rotation.W * num1;
    float num5 = rotation.W * num2;
    float num6 = rotation.W * num3;
    float num7 = rotation.X * num1;
    float num8 = rotation.X * num2;
    float num9 = rotation.X * num3;
    float num10 = rotation.Y * num2;
    float num11 = rotation.Y * num3;
    float num12 = rotation.Z * num3;
    float x2 = plane.Normal.X;
    float y2 = plane.Normal.Y;
    float z2 = plane.Normal.Z;
    Plane plane1 = new();
    plane1.Normal.X = (float) ((1.0 - num10 - num12) * x2 + (num8 - (double) num6) * y2 + (num9 + (double) num5) * z2);
    double num13 = 1.0 - num7;
    plane1.Normal.Y = (float) ((num8 + (double) num6) * x2 + (num13 - num12) * y2 + (num11 - (double) num4) * z2);
    plane1.Normal.Z = (float) ((num11 + (double) num4) * y2 + (num9 - (double) num5) * x2 + (num13 - num10) * z2);
    plane1.D = plane.D;
    result = plane1;
  }

  public static Plane Transform(Plane plane, Quaternion rotation)
  {
    Plane plane1 = new();
    double x1 = rotation.X;
    float num1 = (float) (x1 + x1);
    double y1 = rotation.Y;
    float num2 = (float) (y1 + y1);
    double z1 = rotation.Z;
    float num3 = (float) (z1 + z1);
    float num4 = rotation.W * num1;
    float num5 = rotation.W * num2;
    float num6 = rotation.W * num3;
    float num7 = rotation.X * num1;
    float num8 = rotation.X * num2;
    float num9 = rotation.X * num3;
    float num10 = rotation.Y * num2;
    float num11 = rotation.Y * num3;
    float num12 = rotation.Z * num3;
    float x2 = plane.Normal.X;
    float y2 = plane.Normal.Y;
    float z2 = plane.Normal.Z;
    plane1.Normal.X = (float) ((1.0 - num10 - num12) * x2 + (num8 - (double) num6) * y2 + (num9 + (double) num5) * z2);
    double num13 = 1.0 - num7;
    plane1.Normal.Y = (float) ((num8 + (double) num6) * x2 + (num13 - num12) * y2 + (num11 - (double) num4) * z2);
    plane1.Normal.Z = (float) ((num11 + (double) num4) * y2 + (num9 - (double) num5) * x2 + (num13 - num10) * z2);
    plane1.D = plane.D;
    return plane1;
  }

  public static bool operator ==(Plane left, Plane right) => Equals(ref left, ref right);

  public static bool operator !=(Plane left, Plane right) => !Equals(ref left, ref right);

  public override string ToString()
  {
    object[] objArray = [Normal.ToString(), null];
    float d = D;
    objArray[1] = d.ToString(CultureInfo.CurrentCulture);
    return string.Format(CultureInfo.CurrentCulture, "Normal:{0} D:{1}", objArray);
  }

  public override int GetHashCode()
  {
    float d = D;
    return Normal.GetHashCode() + d.GetHashCode();
  }

  public static bool Equals(ref Plane value1, ref Plane value2)
  {
    return value1.Normal == value2.Normal && value1.D == (double) value2.D;
  }

  public bool Equals(Plane other)
  {
    return Normal == other.Normal && D == (double) other.D;
  }

  public override bool Equals(object obj)
  {
    return obj != null && !(obj.GetType() != GetType()) && Equals((Plane) obj);
  }
}
