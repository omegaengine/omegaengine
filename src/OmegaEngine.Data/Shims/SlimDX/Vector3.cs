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

public struct Vector3(float x, float y, float z) : IEquatable<Vector3>
{
  public float X = x;
  public float Y = y;
  public float Z = z;

  public float this[int index]
  {
    get
    {
      if (index == 0)
        return X;
      if (index == 1)
        return Y;
      if (index != 2)
        throw new ArgumentOutOfRangeException(nameof (index), "Indices for Vector3 run from 0 to 2, inclusive.");
      return Z;
    }
    set
    {
      if (index != 0)
      {
        if (index != 1)
        {
          if (index != 2)
            throw new ArgumentOutOfRangeException(nameof (index), "Indices for Vector3 run from 0 to 2, inclusive.");
          Z = value;
        }
        else
          Y = value;
      }
      else
        X = value;
    }
  }

  public static Vector3 Zero => new(0.0f, 0.0f, 0.0f);

  public static Vector3 UnitX => new(1f, 0.0f, 0.0f);

  public static Vector3 UnitY => new(0.0f, 1f, 0.0f);

  public static Vector3 UnitZ => new(0.0f, 0.0f, 1f);

  public static int SizeInBytes => Marshal.SizeOf(typeof (Vector3));

  public Vector3(Vector2 value, float z)
      : this(value.X, value.Y, z)
  {}

  public Vector3(float value)
      : this(value, value, value)
  {}

  public float Length()
  {
    double y = Y;
    double x = X;
    double z = Z;
    double num1 = x;
    double num2 = num1 * num1;
    double num3 = y;
    double num4 = num3 * num3;
    double num5 = num2 + num4;
    double num6 = z;
    double num7 = num6 * num6;
    return (float) Math.Sqrt(num5 + num7);
  }

  public float LengthSquared()
  {
    double y = Y;
    double x = X;
    double z = Z;
    double num1 = x;
    double num2 = num1 * num1;
    double num3 = y;
    double num4 = num3 * num3;
    double num5 = num2 + num4;
    double num6 = z;
    double num7 = num6 * num6;
    return (float) (num5 + num7);
  }

  public static void Normalize(ref Vector3 vector, out Vector3 result)
  {
    Vector3 vector3 = vector;
    result = vector3;
    result.Normalize();
  }

  public static Vector3 Normalize(Vector3 vector)
  {
    vector.Normalize();
    return vector;
  }

  public void Normalize()
  {
    float num1 = Length();
    if (num1 == 0.0)
      return;
    float num2 = 1f / num1;
    X *= num2;
    Y *= num2;
    Z *= num2;
  }

  public static void Add(ref Vector3 left, ref Vector3 right, out Vector3 result)
  {
    Vector3 vector3;
    vector3.X = left.X + right.X;
    vector3.Y = left.Y + right.Y;
    vector3.Z = left.Z + right.Z;
    result = vector3;
  }

  public static Vector3 Add(Vector3 left, Vector3 right)
  {
    Vector3 vector3;
    vector3.X = left.X + right.X;
    vector3.Y = left.Y + right.Y;
    vector3.Z = left.Z + right.Z;
    return vector3;
  }

  public static void Subtract(ref Vector3 left, ref Vector3 right, out Vector3 result)
  {
    Vector3 vector3;
    vector3.X = left.X - right.X;
    vector3.Y = left.Y - right.Y;
    vector3.Z = left.Z - right.Z;
    result = vector3;
  }

  public static Vector3 Subtract(Vector3 left, Vector3 right)
  {
    Vector3 vector3;
    vector3.X = left.X - right.X;
    vector3.Y = left.Y - right.Y;
    vector3.Z = left.Z - right.Z;
    return vector3;
  }

  public static void Multiply(ref Vector3 vector, float scale, out Vector3 result)
  {
    Vector3 vector3;
    vector3.X = vector.X * scale;
    vector3.Y = vector.Y * scale;
    vector3.Z = vector.Z * scale;
    result = vector3;
  }

  public static Vector3 Multiply(Vector3 value, float scale)
  {
    Vector3 vector3;
    vector3.X = value.X * scale;
    vector3.Y = value.Y * scale;
    vector3.Z = value.Z * scale;
    return vector3;
  }

  public static void Modulate(ref Vector3 left, ref Vector3 right, out Vector3 result)
  {
    Vector3 vector3;
    vector3.X = left.X * right.X;
    vector3.Y = left.Y * right.Y;
    vector3.Z = left.Z * right.Z;
    result = vector3;
  }

  public static Vector3 Modulate(Vector3 left, Vector3 right)
  {
    Vector3 vector3;
    vector3.X = left.X * right.X;
    vector3.Y = left.Y * right.Y;
    vector3.Z = left.Z * right.Z;
    return vector3;
  }

  public static void Divide(ref Vector3 vector, float scale, out Vector3 result)
  {
    Vector3 vector3;
    vector3.X = vector.X / scale;
    vector3.Y = vector.Y / scale;
    vector3.Z = vector.Z / scale;
    result = vector3;
  }

  public static Vector3 Divide(Vector3 value, float scale)
  {
    Vector3 vector3;
    vector3.X = value.X / scale;
    vector3.Y = value.Y / scale;
    vector3.Z = value.Z / scale;
    return vector3;
  }

  public static void Negate(ref Vector3 value, out Vector3 result)
  {
    float num1 = -value.X;
    float num2 = -value.Y;
    float num3 = -value.Z;
    Vector3 vector3;
    vector3.X = num1;
    vector3.Y = num2;
    vector3.Z = num3;
    result = vector3;
  }

  public static Vector3 Negate(Vector3 value)
  {
    float num1 = -value.X;
    float num2 = -value.Y;
    float num3 = -value.Z;
    Vector3 vector3;
    vector3.X = num1;
    vector3.Y = num2;
    vector3.Z = num3;
    return vector3;
  }

  public static void Barycentric(
    ref Vector3 value1,
    ref Vector3 value2,
    ref Vector3 value3,
    float amount1,
    float amount2,
    out Vector3 result)
  {
    Vector3 vector3;
    vector3.X = (float) ((value2.X - (double) value1.X) * amount1 + value1.X + (value3.X - (double) value1.X) * amount2);
    vector3.Y = (float) ((value2.Y - (double) value1.Y) * amount1 + value1.Y + (value3.Y - (double) value1.Y) * amount2);
    vector3.Z = (float) ((value2.Z - (double) value1.Z) * amount1 + value1.Z + (value3.Z - (double) value1.Z) * amount2);
    result = vector3;
  }

  public static Vector3 Barycentric(
    Vector3 value1,
    Vector3 value2,
    Vector3 value3,
    float amount1,
    float amount2)
  {
    return new()
    {
      X = (float) ((value2.X - (double) value1.X) * amount1 + value1.X + (value3.X - (double) value1.X) * amount2),
      Y = (float) ((value2.Y - (double) value1.Y) * amount1 + value1.Y + (value3.Y - (double) value1.Y) * amount2),
      Z = (float) ((value2.Z - (double) value1.Z) * amount1 + value1.Z + (value3.Z - (double) value1.Z) * amount2)
    };
  }

  public static void CatmullRom(
    ref Vector3 value1,
    ref Vector3 value2,
    ref Vector3 value3,
    ref Vector3 value4,
    float amount,
    out Vector3 result)
  {
    double num1 = amount;
    float num2 = (float) (num1 * num1);
    float num3 = num2 * amount;
    result = new()
    {
      X = (float) (((value1.X * 2.0 - value2.X * 5.0 + value3.X * 4.0 - value4.X) * num2 + ((value3.X - (double) value1.X) * amount + value2.X * 2.0) + (value2.X * 3.0 - value1.X - value3.X * 3.0 + value4.X) * num3) * 0.5),
      Y = (float) (((value1.Y * 2.0 - value2.Y * 5.0 + value3.Y * 4.0 - value4.Y) * num2 + ((value3.Y - (double) value1.Y) * amount + value2.Y * 2.0) + (value2.Y * 3.0 - value1.Y - value3.Y * 3.0 + value4.Y) * num3) * 0.5),
      Z = (float) (((value1.Z * 2.0 - value2.Z * 5.0 + value3.Z * 4.0 - value4.Z) * num2 + ((value3.Z - (double) value1.Z) * amount + value2.Z * 2.0) + (value2.Z * 3.0 - value1.Z - value3.Z * 3.0 + value4.Z) * num3) * 0.5)
    };
  }

  public static Vector3 CatmullRom(
    Vector3 value1,
    Vector3 value2,
    Vector3 value3,
    Vector3 value4,
    float amount)
  {
    Vector3 vector3 = new();
    double num1 = amount;
    float num2 = (float) (num1 * num1);
    float num3 = num2 * amount;
    vector3.X = (float) (((value1.X * 2.0 - value2.X * 5.0 + value3.X * 4.0 - value4.X) * num2 + ((value3.X - (double) value1.X) * amount + value2.X * 2.0) + (value2.X * 3.0 - value1.X - value3.X * 3.0 + value4.X) * num3) * 0.5);
    vector3.Y = (float) (((value1.Y * 2.0 - value2.Y * 5.0 + value3.Y * 4.0 - value4.Y) * num2 + ((value3.Y - (double) value1.Y) * amount + value2.Y * 2.0) + (value2.Y * 3.0 - value1.Y - value3.Y * 3.0 + value4.Y) * num3) * 0.5);
    vector3.Z = (float) (((value1.Z * 2.0 - value2.Z * 5.0 + value3.Z * 4.0 - value4.Z) * num2 + ((value3.Z - (double) value1.Z) * amount + value2.Z * 2.0) + (value2.Z * 3.0 - value1.Z - value3.Z * 3.0 + value4.Z) * num3) * 0.5);
    return vector3;
  }

  public static void Clamp(
    ref Vector3 value,
    ref Vector3 min,
    ref Vector3 max,
    out Vector3 result)
  {
    float x = value.X;
    float num1 = x <= (double) max.X ? x : max.X;
    float num2 = num1 >= (double) min.X ? num1 : min.X;
    float y = value.Y;
    float num3 = y <= (double) max.Y ? y : max.Y;
    float num4 = num3 >= (double) min.Y ? num3 : min.Y;
    float z = value.Z;
    float num5 = z <= (double) max.Z ? z : max.Z;
    float num6 = num5 >= (double) min.Z ? num5 : min.Z;
    Vector3 vector3;
    vector3.X = num2;
    vector3.Y = num4;
    vector3.Z = num6;
    result = vector3;
  }

  public static Vector3 Clamp(Vector3 value, Vector3 min, Vector3 max)
  {
    float x = value.X;
    float num1 = x <= (double) max.X ? x : max.X;
    float num2 = num1 >= (double) min.X ? num1 : min.X;
    float y = value.Y;
    float num3 = y <= (double) max.Y ? y : max.Y;
    float num4 = num3 >= (double) min.Y ? num3 : min.Y;
    float z = value.Z;
    float num5 = z <= (double) max.Z ? z : max.Z;
    float num6 = num5 >= (double) min.Z ? num5 : min.Z;
    Vector3 vector3;
    vector3.X = num2;
    vector3.Y = num4;
    vector3.Z = num6;
    return vector3;
  }

  public static void Hermite(
    ref Vector3 value1,
    ref Vector3 tangent1,
    ref Vector3 value2,
    ref Vector3 tangent2,
    float amount,
    out Vector3 result)
  {
    double num1 = amount;
    float num2 = (float) (num1 * num1);
    float num3 = num2 * amount;
    double num4 = num2 * 3.0;
    float num5 = (float) (num3 * 2.0 - num4 + 1.0);
    float num6 = (float) (num3 * -2.0 + num4);
    float num7 = num3 - num2 * 2f + amount;
    float num8 = num3 - num2;
    result.X = (float) (value2.X * (double) num6 + value1.X * (double) num5 + tangent1.X * (double) num7 + tangent2.X * (double) num8);
    result.Y = (float) (value2.Y * (double) num6 + value1.Y * (double) num5 + tangent1.Y * (double) num7 + tangent2.Y * (double) num8);
    result.Z = (float) (value2.Z * (double) num6 + value1.Z * (double) num5 + tangent1.Z * (double) num7 + tangent2.Z * (double) num8);
  }

  public static Vector3 Hermite(
    Vector3 value1,
    Vector3 tangent1,
    Vector3 value2,
    Vector3 tangent2,
    float amount)
  {
    Vector3 vector3 = new();
    double num1 = amount;
    float num2 = (float) (num1 * num1);
    float num3 = num2 * amount;
    double num4 = num2 * 3.0;
    float num5 = (float) (num3 * 2.0 - num4 + 1.0);
    float num6 = (float) (num3 * -2.0 + num4);
    float num7 = num3 - num2 * 2f + amount;
    float num8 = num3 - num2;
    vector3.X = (float) (value2.X * (double) num6 + value1.X * (double) num5 + tangent1.X * (double) num7 + tangent2.X * (double) num8);
    vector3.Y = (float) (value2.Y * (double) num6 + value1.Y * (double) num5 + tangent1.Y * (double) num7 + tangent2.Y * (double) num8);
    vector3.Z = (float) (value2.Z * (double) num6 + value1.Z * (double) num5 + tangent1.Z * (double) num7 + tangent2.Z * (double) num8);
    return vector3;
  }

  public static void Lerp(ref Vector3 start, ref Vector3 end, float amount, out Vector3 result)
  {
    result.X = (end.X - start.X) * amount + start.X;
    result.Y = (end.Y - start.Y) * amount + start.Y;
    result.Z = (end.Z - start.Z) * amount + start.Z;
  }

  public static Vector3 Lerp(Vector3 start, Vector3 end, float amount)
  {
    return new()
    {
      X = (end.X - start.X) * amount + start.X,
      Y = (end.Y - start.Y) * amount + start.Y,
      Z = (end.Z - start.Z) * amount + start.Z
    };
  }

  public static void SmoothStep(
    ref Vector3 start,
    ref Vector3 end,
    float amount,
    out Vector3 result)
  {
    float num1 = amount <= 1.0 ? (amount >= 0.0 ? amount : 0.0f) : 1f;
    double num2 = num1;
    double num3 = 3.0 - num1 * 2.0;
    double num4 = num2;
    double num5 = num4 * num4;
    amount = (float) (num3 * num5);
    result.X = (end.X - start.X) * amount + start.X;
    result.Y = (end.Y - start.Y) * amount + start.Y;
    result.Z = (end.Z - start.Z) * amount + start.Z;
  }

  public static Vector3 SmoothStep(Vector3 start, Vector3 end, float amount)
  {
    Vector3 vector3 = new();
    float num1 = amount <= 1.0 ? (amount >= 0.0 ? amount : 0.0f) : 1f;
    double num2 = num1;
    double num3 = 3.0 - num1 * 2.0;
    double num4 = num2;
    double num5 = num4 * num4;
    amount = (float) (num3 * num5);
    vector3.X = (end.X - start.X) * amount + start.X;
    vector3.Y = (end.Y - start.Y) * amount + start.Y;
    vector3.Z = (end.Z - start.Z) * amount + start.Z;
    return vector3;
  }

  public static float Distance(Vector3 value1, Vector3 value2)
  {
    float num1 = value1.X - value2.X;
    float num2 = value1.Y - value2.Y;
    float num3 = value1.Z - value2.Z;
    double num4 = num2;
    double num5 = num1;
    double num6 = num3;
    double num7 = num5;
    double num8 = num7 * num7;
    double num9 = num4;
    double num10 = num9 * num9;
    double num11 = num8 + num10;
    double num12 = num6;
    double num13 = num12 * num12;
    return (float) Math.Sqrt(num11 + num13);
  }

  public static float DistanceSquared(Vector3 value1, Vector3 value2)
  {
    float num1 = value1.X - value2.X;
    float num2 = value1.Y - value2.Y;
    float num3 = value1.Z - value2.Z;
    double num4 = num2;
    double num5 = num1;
    double num6 = num3;
    double num7 = num5;
    double num8 = num7 * num7;
    double num9 = num4;
    double num10 = num9 * num9;
    double num11 = num8 + num10;
    double num12 = num6;
    double num13 = num12 * num12;
    return (float) (num11 + num13);
  }

  public static float Dot(Vector3 left, Vector3 right)
  {
    return (float) (left.Y * (double) right.Y + left.X * (double) right.X + left.Z * (double) right.Z);
  }

  public static void Cross(ref Vector3 left, ref Vector3 right, out Vector3 result)
  {
    result = new()
    {
      X = (float) (left.Y * (double) right.Z - left.Z * (double) right.Y),
      Y = (float) (left.Z * (double) right.X - left.X * (double) right.Z),
      Z = (float) (left.X * (double) right.Y - left.Y * (double) right.X)
    };
  }

  public static Vector3 Cross(Vector3 left, Vector3 right)
  {
    return new()
    {
      X = (float) (right.Z * (double) left.Y - left.Z * (double) right.Y),
      Y = (float) (left.Z * (double) right.X - right.Z * (double) left.X),
      Z = (float) (right.Y * (double) left.X - left.Y * (double) right.X)
    };
  }

  public static void Reflect(ref Vector3 vector, ref Vector3 normal, out Vector3 result)
  {
    double num = (vector.Y * (double) normal.Y + vector.X * (double) normal.X + vector.Z * (double) normal.Z) * 2.0;
    result.X = vector.X - normal.X * (float) num;
    result.Y = vector.Y - normal.Y * (float) num;
    result.Z = vector.Z - normal.Z * (float) num;
  }

  public static Vector3 Reflect(Vector3 vector, Vector3 normal)
  {
    Vector3 vector3 = new();
    double num = (vector.Y * (double) normal.Y + vector.X * (double) normal.X + vector.Z * (double) normal.Z) * 2.0;
    vector3.X = vector.X - normal.X * (float) num;
    vector3.Y = vector.Y - normal.Y * (float) num;
    vector3.Z = vector.Z - normal.Z * (float) num;
    return vector3;
  }

  public static Vector4[] Transform(Vector3[] vectors, ref Quaternion rotation)
  {
    int length = vectors?.Length ?? throw new ArgumentNullException(nameof (vectors));
    Vector4[] vector4Array = new Vector4[length];
    double x = rotation.X;
    float num1 = (float) (x + x);
    double y = rotation.Y;
    float num2 = (float) (y + y);
    double z = rotation.Z;
    float num3 = (float) (z + z);
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
      double num20 = num9 - (double) num5;
      double num21 = num11 + (double) num4;
      double num22 = num16 - num10;
      do
      {
        vector4Array[index] = new()
        {
          X = (float) (vectors[index].Y * num14 + vectors[index].X * num13 + vectors[index].Z * num15),
          Y = (float) (vectors[index].X * num18 + vectors[index].Y * num17 + vectors[index].Z * num19),
          Z = (float) (vectors[index].Y * num21 + vectors[index].X * num20 + vectors[index].Z * num22),
          W = 1f
        };
        ++index;
      }
      while (index < length);
    }
    return vector4Array;
  }

  public static void Transform(ref Vector3 vector, ref Quaternion rotation, out Vector4 result)
  {
    double x = rotation.X;
    float num1 = (float) (x + x);
    double y = rotation.Y;
    float num2 = (float) (y + y);
    double z = rotation.Z;
    float num3 = (float) (z + z);
    float num4 = rotation.W * num1;
    float num5 = rotation.W * num2;
    float num6 = rotation.W * num3;
    float num7 = rotation.X * num1;
    float num8 = rotation.X * num2;
    float num9 = rotation.X * num3;
    float num10 = rotation.Y * num2;
    float num11 = rotation.Y * num3;
    float num12 = rotation.Z * num3;
    Vector4 vector4 = new();
    result = vector4;
    result.X = (float) (vector.X * (1.0 - num10 - num12) + vector.Y * (num8 - (double) num6) + vector.Z * (num9 + (double) num5));
    double num13 = 1.0 - num7;
    result.Y = (float) (vector.X * (num8 + (double) num6) + vector.Y * (num13 - num12) + vector.Z * (num11 - (double) num4));
    result.Z = (float) (vector.X * (num9 - (double) num5) + vector.Y * (num11 + (double) num4) + vector.Z * (num13 - num10));
    result.W = 1f;
  }

  public static Vector4 Transform(Vector3 vector, Quaternion rotation)
  {
    Vector4 vector4 = new();
    double x = rotation.X;
    float num1 = (float) (x + x);
    double y = rotation.Y;
    float num2 = (float) (y + y);
    double z = rotation.Z;
    float num3 = (float) (z + z);
    float num4 = rotation.W * num1;
    float num5 = rotation.W * num2;
    float num6 = rotation.W * num3;
    float num7 = rotation.X * num1;
    float num8 = rotation.X * num2;
    float num9 = rotation.X * num3;
    float num10 = rotation.Y * num2;
    float num11 = rotation.Y * num3;
    float num12 = rotation.Z * num3;
    vector4.X = (float) (vector.X * (1.0 - num10 - num12) + vector.Y * (num8 - (double) num6) + vector.Z * (num9 + (double) num5));
    double num13 = 1.0 - num7;
    vector4.Y = (float) (vector.X * (num8 + (double) num6) + vector.Y * (num13 - num12) + vector.Z * (num11 - (double) num4));
    vector4.Z = (float) (vector.X * (num9 - (double) num5) + vector.Y * (num11 + (double) num4) + vector.Z * (num13 - num10));
    vector4.W = 1f;
    return vector4;
  }

  public static void Transform(ref Vector3 vector, ref Matrix transformation, out Vector4 result)
  {
    Vector4 vector4 = new();
    result = vector4;
    result.X = (float) (vector.Y * (double) transformation.M21 + vector.X * (double) transformation.M11 + vector.Z * (double) transformation.M31) + transformation.M41;
    result.Y = (float) (vector.Y * (double) transformation.M22 + vector.X * (double) transformation.M12 + vector.Z * (double) transformation.M32) + transformation.M42;
    result.Z = (float) (vector.Y * (double) transformation.M23 + vector.X * (double) transformation.M13 + vector.Z * (double) transformation.M33) + transformation.M43;
    result.W = (float) ((double) transformation.M24 * vector.Y + (double) transformation.M14 * vector.X + vector.Z * (double) transformation.M34) + transformation.M44;
  }

  public static Vector4 Transform(Vector3 vector, Matrix transformation)
  {
    return new()
    {
      X = (float) ((double) transformation.M21 * vector.Y + (double) transformation.M11 * vector.X + (double) transformation.M31 * vector.Z) + transformation.M41,
      Y = (float) ((double) transformation.M22 * vector.Y + (double) transformation.M12 * vector.X + (double) transformation.M32 * vector.Z) + transformation.M42,
      Z = (float) ((double) transformation.M23 * vector.Y + (double) transformation.M13 * vector.X + (double) transformation.M33 * vector.Z) + transformation.M43,
      W = (float) ((double) transformation.M24 * vector.Y + (double) transformation.M14 * vector.X + (double) transformation.M34 * vector.Z) + transformation.M44
    };
  }

  public static void TransformCoordinate(
    ref Vector3 coordinate,
    ref Matrix transformation,
    out Vector3 result)
  {
    Vector4 vector4 = new();
    vector4.X = (float) (coordinate.Y * (double) transformation.M21 + coordinate.X * (double) transformation.M11 + coordinate.Z * (double) transformation.M31) + transformation.M41;
    vector4.Y = (float) (coordinate.Y * (double) transformation.M22 + coordinate.X * (double) transformation.M12 + coordinate.Z * (double) transformation.M32) + transformation.M42;
    vector4.Z = (float) (coordinate.Y * (double) transformation.M23 + coordinate.X * (double) transformation.M13 + coordinate.Z * (double) transformation.M33) + transformation.M43;
    float num = (float) (1.0 / ((double) transformation.M24 * coordinate.Y + (double) transformation.M14 * coordinate.X + coordinate.Z * (double) transformation.M34 + transformation.M44));
    vector4.W = num;
    Vector3 vector3;
    vector3.X = vector4.X * num;
    vector3.Y = vector4.Y * num;
    vector3.Z = vector4.Z * num;
    result = vector3;
  }

  public static Vector3 TransformCoordinate(Vector3 coordinate, Matrix transformation)
  {
    Vector4 vector4 = new();
    vector4.X = (float) ((double) transformation.M21 * coordinate.Y + (double) transformation.M11 * coordinate.X + (double) transformation.M31 * coordinate.Z) + transformation.M41;
    vector4.Y = (float) ((double) transformation.M22 * coordinate.Y + (double) transformation.M12 * coordinate.X + (double) transformation.M32 * coordinate.Z) + transformation.M42;
    vector4.Z = (float) ((double) transformation.M23 * coordinate.Y + (double) transformation.M13 * coordinate.X + (double) transformation.M33 * coordinate.Z) + transformation.M43;
    float num = (float) (1.0 / ((double) transformation.M24 * coordinate.Y + (double) transformation.M14 * coordinate.X + (double) transformation.M34 * coordinate.Z + transformation.M44));
    vector4.W = num;
    Vector3 vector3;
    vector3.X = vector4.X * num;
    vector3.Y = vector4.Y * num;
    vector3.Z = vector4.Z * num;
    return vector3;
  }

  public static void Project(
    ref Vector3 vector,
    float x,
    float y,
    float width,
    float height,
    float minZ,
    float maxZ,
    ref Matrix worldViewProjection,
    out Vector3 result)
  {
    TransformCoordinate(ref vector, ref worldViewProjection, out var result1);
    Vector3 vector3;
    vector3.X = (float) ((result1.X + 1.0) * 0.5) * width + x;
    vector3.Y = (float) ((1.0 - result1.Y) * 0.5) * height + y;
    vector3.Z = result1.Z * (maxZ - minZ) + minZ;
    result = vector3;
  }

  public static Vector3 Project(
    Vector3 vector,
    float x,
    float y,
    float width,
    float height,
    float minZ,
    float maxZ,
    Matrix worldViewProjection)
  {
    TransformCoordinate(ref vector, ref worldViewProjection, out vector);
    Vector3 vector3;
    vector3.X = (float) ((vector.X + 1.0) * 0.5) * width + x;
    vector3.Y = (float) ((1.0 - vector.Y) * 0.5) * height + y;
    vector3.Z = vector.Z * (maxZ - minZ) + minZ;
    return vector3;
  }

  public static void Minimize(ref Vector3 value1, ref Vector3 value2, out Vector3 result)
  {
    float num1 = value1.X >= (double) value2.X ? value2.X : value1.X;
    result.X = num1;
    float num2 = value1.Y >= (double) value2.Y ? value2.Y : value1.Y;
    result.Y = num2;
    float num3 = value1.Z >= (double) value2.Z ? value2.Z : value1.Z;
    result.Z = num3;
  }

  public static Vector3 Minimize(Vector3 value1, Vector3 value2)
  {
    return new()
    {
      X = value1.X >= (double) value2.X ? value2.X : value1.X,
      Y = value1.Y >= (double) value2.Y ? value2.Y : value1.Y,
      Z = value1.Z >= (double) value2.Z ? value2.Z : value1.Z
    };
  }

  public static void Maximize(ref Vector3 value1, ref Vector3 value2, out Vector3 result)
  {
    float num1 = value1.X <= (double) value2.X ? value2.X : value1.X;
    result.X = num1;
    float num2 = value1.Y <= (double) value2.Y ? value2.Y : value1.Y;
    result.Y = num2;
    float num3 = value1.Z <= (double) value2.Z ? value2.Z : value1.Z;
    result.Z = num3;
  }

  public static Vector3 Maximize(Vector3 value1, Vector3 value2)
  {
    return new()
    {
      X = value1.X <= (double) value2.X ? value2.X : value1.X,
      Y = value1.Y <= (double) value2.Y ? value2.Y : value1.Y,
      Z = value1.Z <= (double) value2.Z ? value2.Z : value1.Z
    };
  }

  public static Vector3 operator +(Vector3 left, Vector3 right)
  {
    Vector3 vector3;
    vector3.X = left.X + right.X;
    vector3.Y = left.Y + right.Y;
    vector3.Z = left.Z + right.Z;
    return vector3;
  }

  public static Vector3 operator -(Vector3 value)
  {
    float num1 = -value.X;
    float num2 = -value.Y;
    float num3 = -value.Z;
    Vector3 vector3;
    vector3.X = num1;
    vector3.Y = num2;
    vector3.Z = num3;
    return vector3;
  }

  public static Vector3 operator -(Vector3 left, Vector3 right)
  {
    Vector3 vector3;
    vector3.X = left.X - right.X;
    vector3.Y = left.Y - right.Y;
    vector3.Z = left.Z - right.Z;
    return vector3;
  }

  public static Vector3 operator *(float scale, Vector3 vector) => vector * scale;

  public static Vector3 operator *(Vector3 vector, float scale)
  {
    Vector3 vector3;
    vector3.X = vector.X * scale;
    vector3.Y = vector.Y * scale;
    vector3.Z = vector.Z * scale;
    return vector3;
  }

  public static Vector3 operator /(Vector3 vector, float scale)
  {
    Vector3 vector3;
    vector3.X = vector.X / scale;
    vector3.Y = vector.Y / scale;
    vector3.Z = vector.Z / scale;
    return vector3;
  }

  public static bool operator ==(Vector3 left, Vector3 right)
  {
    return Equals(ref left, ref right);
  }

  public static bool operator !=(Vector3 left, Vector3 right)
  {
    return !Equals(ref left, ref right);
  }

  public override string ToString()
  {
    object[] objArray = new object[3];
    float x = X;
    objArray[0] = x.ToString(CultureInfo.CurrentCulture);
    float y = Y;
    objArray[1] = y.ToString(CultureInfo.CurrentCulture);
    float z = Z;
    objArray[2] = z.ToString(CultureInfo.CurrentCulture);
    return string.Format(CultureInfo.CurrentCulture, "X:{0} Y:{1} Z:{2}", objArray);
  }

  public override int GetHashCode()
  {
    float x = X;
    float y = Y;
    float z = Z;
    int num = y.GetHashCode() + z.GetHashCode();
    return x.GetHashCode() + num;
  }

  public static bool Equals(ref Vector3 value1, ref Vector3 value2)
  {
    return value1.X == (double) value2.X && value1.Y == (double) value2.Y && value1.Z == (double) value2.Z;
  }

  public bool Equals(Vector3 other)
  {
    return X == (double) other.X && Y == (double) other.Y && Z == (double) other.Z;
  }

  public override bool Equals(object obj)
  {
    return obj != null && !(obj.GetType() != GetType()) && Equals((Vector3) obj);
  }
}
