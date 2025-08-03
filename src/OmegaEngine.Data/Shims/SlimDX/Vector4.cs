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

public struct Vector4(float x, float y, float z, float w) : IEquatable<Vector4>
{
  public float X = x;
  public float Y = y;
  public float Z = z;
  public float W = w;

  public float this[int index]
  {
    get
    {
      switch (index)
      {
        case 0:
          return X;
        case 1:
          return Y;
        case 2:
          return Z;
        case 3:
          return W;
        default:
          throw new ArgumentOutOfRangeException(nameof (index), "Indices for Vector4 run from 0 to 3, inclusive.");
      }
    }
    set
    {
      switch (index)
      {
        case 0:
          X = value;
          break;
        case 1:
          Y = value;
          break;
        case 2:
          Z = value;
          break;
        case 3:
          W = value;
          break;
        default:
          throw new ArgumentOutOfRangeException(nameof (index), "Indices for Vector4 run from 0 to 3, inclusive.");
      }
    }
  }

  public static Vector4 Zero => new(0.0f, 0.0f, 0.0f, 0.0f);

  public static Vector4 UnitX => new(1f, 0.0f, 0.0f, 0.0f);

  public static Vector4 UnitY => new(0.0f, 1f, 0.0f, 0.0f);

  public static Vector4 UnitZ => new(0.0f, 0.0f, 1f, 0.0f);

  public static Vector4 UnitW => new(0.0f, 0.0f, 0.0f, 1f);

  public static int SizeInBytes => Marshal.SizeOf(typeof (Vector4));

  public Vector4(Vector3 value, float w)
      : this(value.X, value.Y, value.Z, w)
  {}

  public Vector4(Vector2 value, float z, float w)
      : this(value.X, value.Y, z, w)
  {}

  public Vector4(float value)
      : this(value, value, value, value)
  {}

  public float Length()
  {
    double y = Y;
    double x = X;
    double z = Z;
    double w = W;
    double num1 = x;
    double num2 = num1 * num1;
    double num3 = y;
    double num4 = num3 * num3;
    double num5 = num2 + num4;
    double num6 = z;
    double num7 = num6 * num6;
    double num8 = num5 + num7;
    double num9 = w;
    double num10 = num9 * num9;
    return (float) Math.Sqrt(num8 + num10);
  }

  public float LengthSquared()
  {
    double y = Y;
    double x = X;
    double z = Z;
    double w = W;
    double num1 = x;
    double num2 = num1 * num1;
    double num3 = y;
    double num4 = num3 * num3;
    double num5 = num2 + num4;
    double num6 = z;
    double num7 = num6 * num6;
    double num8 = num5 + num7;
    double num9 = w;
    double num10 = num9 * num9;
    return (float) (num8 + num10);
  }

  public static void Normalize(ref Vector4 vector, out Vector4 result)
  {
    Vector4 vector4 = vector;
    result = vector4;
    result.Normalize();
  }

  public static Vector4 Normalize(Vector4 vector)
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
    W *= num2;
  }

  public static void Add(ref Vector4 left, ref Vector4 right, out Vector4 result)
  {
    Vector4 vector4;
    vector4.X = left.X + right.X;
    vector4.Y = left.Y + right.Y;
    vector4.Z = left.Z + right.Z;
    vector4.W = left.W + right.W;
    result = vector4;
  }

  public static Vector4 Add(Vector4 left, Vector4 right)
  {
    Vector4 vector4;
    vector4.X = left.X + right.X;
    vector4.Y = left.Y + right.Y;
    vector4.Z = left.Z + right.Z;
    vector4.W = left.W + right.W;
    return vector4;
  }

  public static void Subtract(ref Vector4 left, ref Vector4 right, out Vector4 result)
  {
    Vector4 vector4;
    vector4.X = left.X - right.X;
    vector4.Y = left.Y - right.Y;
    vector4.Z = left.Z - right.Z;
    vector4.W = left.W - right.W;
    result = vector4;
  }

  public static Vector4 Subtract(Vector4 left, Vector4 right)
  {
    Vector4 vector4;
    vector4.X = left.X - right.X;
    vector4.Y = left.Y - right.Y;
    vector4.Z = left.Z - right.Z;
    vector4.W = left.W - right.W;
    return vector4;
  }

  public static void Multiply(ref Vector4 vector, float scale, out Vector4 result)
  {
    Vector4 vector4;
    vector4.X = vector.X * scale;
    vector4.Y = vector.Y * scale;
    vector4.Z = vector.Z * scale;
    vector4.W = vector.W * scale;
    result = vector4;
  }

  public static Vector4 Multiply(Vector4 value, float scale)
  {
    Vector4 vector4;
    vector4.X = value.X * scale;
    vector4.Y = value.Y * scale;
    vector4.Z = value.Z * scale;
    vector4.W = value.W * scale;
    return vector4;
  }

  public static void Modulate(ref Vector4 left, ref Vector4 right, out Vector4 result)
  {
    Vector4 vector4;
    vector4.X = left.X * right.X;
    vector4.Y = left.Y * right.Y;
    vector4.Z = left.Z * right.Z;
    vector4.W = left.W * right.W;
    result = vector4;
  }

  public static Vector4 Modulate(Vector4 left, Vector4 right)
  {
    Vector4 vector4;
    vector4.X = left.X * right.X;
    vector4.Y = left.Y * right.Y;
    vector4.Z = left.Z * right.Z;
    vector4.W = left.W * right.W;
    return vector4;
  }

  public static void Divide(ref Vector4 vector, float scale, out Vector4 result)
  {
    Vector4 vector4;
    vector4.X = vector.X / scale;
    vector4.Y = vector.Y / scale;
    vector4.Z = vector.Z / scale;
    vector4.W = vector.W / scale;
    result = vector4;
  }

  public static Vector4 Divide(Vector4 value, float scale)
  {
    Vector4 vector4;
    vector4.X = value.X / scale;
    vector4.Y = value.Y / scale;
    vector4.Z = value.Z / scale;
    vector4.W = value.W / scale;
    return vector4;
  }

  public static void Negate(ref Vector4 value, out Vector4 result)
  {
    float num1 = -value.X;
    float num2 = -value.Y;
    float num3 = -value.Z;
    float num4 = -value.W;
    Vector4 vector4;
    vector4.X = num1;
    vector4.Y = num2;
    vector4.Z = num3;
    vector4.W = num4;
    result = vector4;
  }

  public static Vector4 Negate(Vector4 value)
  {
    float num1 = -value.X;
    float num2 = -value.Y;
    float num3 = -value.Z;
    float num4 = -value.W;
    Vector4 vector4;
    vector4.X = num1;
    vector4.Y = num2;
    vector4.Z = num3;
    vector4.W = num4;
    return vector4;
  }

  public static void Barycentric(
    ref Vector4 value1,
    ref Vector4 value2,
    ref Vector4 value3,
    float amount1,
    float amount2,
    out Vector4 result)
  {
    Vector4 vector4;
    vector4.X = (float) ((value2.X - (double) value1.X) * amount1 + value1.X + (value3.X - (double) value1.X) * amount2);
    vector4.Y = (float) ((value2.Y - (double) value1.Y) * amount1 + value1.Y + (value3.Y - (double) value1.Y) * amount2);
    vector4.Z = (float) ((value2.Z - (double) value1.Z) * amount1 + value1.Z + (value3.Z - (double) value1.Z) * amount2);
    vector4.W = (float) ((value2.W - (double) value1.W) * amount1 + value1.W + (value3.W - (double) value1.W) * amount2);
    result = vector4;
  }

  public static Vector4 Barycentric(
    Vector4 value1,
    Vector4 value2,
    Vector4 value3,
    float amount1,
    float amount2)
  {
    return new()
    {
      X = (float) ((value2.X - (double) value1.X) * amount1 + value1.X + (value3.X - (double) value1.X) * amount2),
      Y = (float) ((value2.Y - (double) value1.Y) * amount1 + value1.Y + (value3.Y - (double) value1.Y) * amount2),
      Z = (float) ((value2.Z - (double) value1.Z) * amount1 + value1.Z + (value3.Z - (double) value1.Z) * amount2),
      W = (float) ((value2.W - (double) value1.W) * amount1 + value1.W + (value3.W - (double) value1.W) * amount2)
    };
  }

  public static void CatmullRom(
    ref Vector4 value1,
    ref Vector4 value2,
    ref Vector4 value3,
    ref Vector4 value4,
    float amount,
    out Vector4 result)
  {
    double num1 = amount;
    float num2 = (float) (num1 * num1);
    float num3 = num2 * amount;
    result = new()
    {
      X = (float) (((value1.X * 2.0 - value2.X * 5.0 + value3.X * 4.0 - value4.X) * num2 + ((value3.X - (double) value1.X) * amount + value2.X * 2.0) + (value2.X * 3.0 - value1.X - value3.X * 3.0 + value4.X) * num3) * 0.5),
      Y = (float) (((value1.Y * 2.0 - value2.Y * 5.0 + value3.Y * 4.0 - value4.Y) * num2 + ((value3.Y - (double) value1.Y) * amount + value2.Y * 2.0) + (value2.Y * 3.0 - value1.Y - value3.Y * 3.0 + value4.Y) * num3) * 0.5),
      Z = (float) (((value1.Z * 2.0 - value2.Z * 5.0 + value3.Z * 4.0 - value4.Z) * num2 + ((value3.Z - (double) value1.Z) * amount + value2.Z * 2.0) + (value2.Z * 3.0 - value1.Z - value3.Z * 3.0 + value4.Z) * num3) * 0.5),
      W = (float) (((value1.W * 2.0 - value2.W * 5.0 + value3.W * 4.0 - value4.W) * num2 + ((value3.W - (double) value1.W) * amount + value2.W * 2.0) + (value2.W * 3.0 - value1.W - value3.W * 3.0 + value4.W) * num3) * 0.5)
    };
  }

  public static Vector4 CatmullRom(
    Vector4 value1,
    Vector4 value2,
    Vector4 value3,
    Vector4 value4,
    float amount)
  {
    Vector4 vector4 = new();
    double num1 = amount;
    float num2 = (float) (num1 * num1);
    float num3 = num2 * amount;
    vector4.X = (float) (((value1.X * 2.0 - value2.X * 5.0 + value3.X * 4.0 - value4.X) * num2 + ((value3.X - (double) value1.X) * amount + value2.X * 2.0) + (value2.X * 3.0 - value1.X - value3.X * 3.0 + value4.X) * num3) * 0.5);
    vector4.Y = (float) (((value1.Y * 2.0 - value2.Y * 5.0 + value3.Y * 4.0 - value4.Y) * num2 + ((value3.Y - (double) value1.Y) * amount + value2.Y * 2.0) + (value2.Y * 3.0 - value1.Y - value3.Y * 3.0 + value4.Y) * num3) * 0.5);
    vector4.Z = (float) (((value1.Z * 2.0 - value2.Z * 5.0 + value3.Z * 4.0 - value4.Z) * num2 + ((value3.Z - (double) value1.Z) * amount + value2.Z * 2.0) + (value2.Z * 3.0 - value1.Z - value3.Z * 3.0 + value4.Z) * num3) * 0.5);
    vector4.W = (float) (((value1.W * 2.0 - value2.W * 5.0 + value3.W * 4.0 - value4.W) * num2 + ((value3.W - (double) value1.W) * amount + value2.W * 2.0) + (value2.W * 3.0 - value1.W - value3.W * 3.0 + value4.W) * num3) * 0.5);
    return vector4;
  }

  public static void Clamp(
    ref Vector4 value,
    ref Vector4 min,
    ref Vector4 max,
    out Vector4 result)
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
    float w = value.W;
    float num7 = w <= (double) max.W ? w : max.W;
    float num8 = num7 >= (double) min.W ? num7 : min.W;
    Vector4 vector4;
    vector4.X = num2;
    vector4.Y = num4;
    vector4.Z = num6;
    vector4.W = num8;
    result = vector4;
  }

  public static Vector4 Clamp(Vector4 value, Vector4 min, Vector4 max)
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
    float w = value.W;
    float num7 = w <= (double) max.W ? w : max.W;
    float num8 = num7 >= (double) min.W ? num7 : min.W;
    Vector4 vector4;
    vector4.X = num2;
    vector4.Y = num4;
    vector4.Z = num6;
    vector4.W = num8;
    return vector4;
  }

  public static void Hermite(
    ref Vector4 value1,
    ref Vector4 tangent1,
    ref Vector4 value2,
    ref Vector4 tangent2,
    float amount,
    out Vector4 result)
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
    result.W = (float) (value2.W * (double) num6 + value1.W * (double) num5 + tangent1.W * (double) num7 + tangent2.W * (double) num8);
  }

  public static Vector4 Hermite(
    Vector4 value1,
    Vector4 tangent1,
    Vector4 value2,
    Vector4 tangent2,
    float amount)
  {
    Vector4 vector4 = new();
    double num1 = amount;
    float num2 = (float) (num1 * num1);
    float num3 = num2 * amount;
    double num4 = num2 * 3.0;
    float num5 = (float) (num3 * 2.0 - num4 + 1.0);
    float num6 = (float) (num3 * -2.0 + num4);
    float num7 = num3 - num2 * 2f + amount;
    float num8 = num3 - num2;
    vector4.X = (float) (value2.X * (double) num6 + value1.X * (double) num5 + tangent1.X * (double) num7 + tangent2.X * (double) num8);
    vector4.Y = (float) (value2.Y * (double) num6 + value1.Y * (double) num5 + tangent1.Y * (double) num7 + tangent2.Y * (double) num8);
    vector4.Z = (float) (value2.Z * (double) num6 + value1.Z * (double) num5 + tangent1.Z * (double) num7 + tangent2.Z * (double) num8);
    vector4.W = (float) (value2.W * (double) num6 + value1.W * (double) num5 + tangent1.W * (double) num7 + tangent2.W * (double) num8);
    return vector4;
  }

  public static void Lerp(ref Vector4 start, ref Vector4 end, float amount, out Vector4 result)
  {
    result.X = (end.X - start.X) * amount + start.X;
    result.Y = (end.Y - start.Y) * amount + start.Y;
    result.Z = (end.Z - start.Z) * amount + start.Z;
    result.W = (end.W - start.W) * amount + start.W;
  }

  public static Vector4 Lerp(Vector4 start, Vector4 end, float amount)
  {
    return new()
    {
      X = (end.X - start.X) * amount + start.X,
      Y = (end.Y - start.Y) * amount + start.Y,
      Z = (end.Z - start.Z) * amount + start.Z,
      W = (end.W - start.W) * amount + start.W
    };
  }

  public static void SmoothStep(
    ref Vector4 start,
    ref Vector4 end,
    float amount,
    out Vector4 result)
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
    result.W = (end.W - start.W) * amount + start.W;
  }

  public static Vector4 SmoothStep(Vector4 start, Vector4 end, float amount)
  {
    Vector4 vector4 = new();
    float num1 = amount <= 1.0 ? (amount >= 0.0 ? amount : 0.0f) : 1f;
    double num2 = num1;
    double num3 = 3.0 - num1 * 2.0;
    double num4 = num2;
    double num5 = num4 * num4;
    amount = (float) (num3 * num5);
    vector4.X = (end.X - start.X) * amount + start.X;
    vector4.Y = (end.Y - start.Y) * amount + start.Y;
    vector4.Z = (end.Z - start.Z) * amount + start.Z;
    vector4.W = (end.W - start.W) * amount + start.W;
    return vector4;
  }

  public static float Distance(Vector4 value1, Vector4 value2)
  {
    float num1 = value1.X - value2.X;
    float num2 = value1.Y - value2.Y;
    float num3 = value1.Z - value2.Z;
    float num4 = value1.W - value2.W;
    double num5 = num2;
    double num6 = num1;
    double num7 = num3;
    double num8 = num4;
    double num9 = num6;
    double num10 = num9 * num9;
    double num11 = num5;
    double num12 = num11 * num11;
    double num13 = num10 + num12;
    double num14 = num7;
    double num15 = num14 * num14;
    double num16 = num13 + num15;
    double num17 = num8;
    double num18 = num17 * num17;
    return (float) Math.Sqrt(num16 + num18);
  }

  public static float DistanceSquared(Vector4 value1, Vector4 value2)
  {
    float num1 = value1.X - value2.X;
    float num2 = value1.Y - value2.Y;
    float num3 = value1.Z - value2.Z;
    float num4 = value1.W - value2.W;
    double num5 = num2;
    double num6 = num1;
    double num7 = num3;
    double num8 = num4;
    double num9 = num6;
    double num10 = num9 * num9;
    double num11 = num5;
    double num12 = num11 * num11;
    double num13 = num10 + num12;
    double num14 = num7;
    double num15 = num14 * num14;
    double num16 = num13 + num15;
    double num17 = num8;
    double num18 = num17 * num17;
    return (float) (num16 + num18);
  }

  public static float Dot(Vector4 left, Vector4 right)
  {
    return (float) (left.Y * (double) right.Y + left.X * (double) right.X + left.Z * (double) right.Z + left.W * (double) right.W);
  }

  public static Vector4[] Transform(Vector4[] vectors, ref Quaternion rotation)
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
      double num20 = num11 + (double) num4;
      double num21 = num9 - (double) num5;
      double num22 = num16 - num10;
      do
      {
        vector4Array[index] = new()
        {
          X = (float) (vectors[index].Y * num14 + vectors[index].X * num13 + vectors[index].Z * num15),
          Y = (float) (vectors[index].X * num18 + vectors[index].Y * num17 + vectors[index].Z * num19),
          Z = (float) (vectors[index].X * num21 + vectors[index].Y * num20 + vectors[index].Z * num22),
          W = vectors[index].W
        };
        ++index;
      }
      while (index < length);
    }
    return vector4Array;
  }

  public static void Transform(ref Vector4 vector, ref Quaternion rotation, out Vector4 result)
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
    Vector4 vector4 = new()
    {
        X = (float) (vector.X * (1.0 - num10 - num12) + vector.Y * (num8 - (double) num6) + vector.Z * (num9 + (double) num5))
    };
    double num13 = 1.0 - num7;
    vector4.Y = (float) (vector.X * (num8 + (double) num6) + vector.Y * (num13 - num12) + vector.Z * (num11 - (double) num4));
    vector4.Z = (float) (vector.X * (num9 - (double) num5) + vector.Y * (num11 + (double) num4) + vector.Z * (num13 - num10));
    vector4.W = vector.W;
    result = vector4;
  }

  public static Vector4 Transform(Vector4 vector, Quaternion rotation)
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
    vector4.W = vector.W;
    return vector4;
  }

  public static Vector4[] Transform(Vector4[] vectors, ref Matrix transformation)
  {
    int length = vectors?.Length ?? throw new ArgumentNullException(nameof (vectors));
    Vector4[] vector4Array = new Vector4[length];
    int index = 0;
    if (0 < length)
    {
      do
      {
        vector4Array[index] = new()
        {
          X = (float) (vectors[index].Y * (double) transformation.M21 + vectors[index].X * (double) transformation.M11 + vectors[index].Z * (double) transformation.M31 + vectors[index].W * (double) transformation.M41),
          Y = (float) (vectors[index].Y * (double) transformation.M22 + vectors[index].X * (double) transformation.M12 + vectors[index].Z * (double) transformation.M32 + vectors[index].W * (double) transformation.M42),
          Z = (float) (vectors[index].Y * (double) transformation.M23 + vectors[index].X * (double) transformation.M13 + vectors[index].Z * (double) transformation.M33 + vectors[index].W * (double) transformation.M43),
          W = (float) (vectors[index].Y * (double) transformation.M24 + vectors[index].X * (double) transformation.M14 + vectors[index].Z * (double) transformation.M34 + vectors[index].W * (double) transformation.M44)
        };
        ++index;
      }
      while (index < length);
    }
    return vector4Array;
  }

  public static void Transform(ref Vector4 vector, ref Matrix transformation, out Vector4 result)
  {
    result = new()
    {
      X = (float) (vector.Y * (double) transformation.M21 + vector.X * (double) transformation.M11 + vector.Z * (double) transformation.M31 + vector.W * (double) transformation.M41),
      Y = (float) (vector.Y * (double) transformation.M22 + vector.X * (double) transformation.M12 + vector.Z * (double) transformation.M32 + vector.W * (double) transformation.M42),
      Z = (float) (vector.Y * (double) transformation.M23 + vector.X * (double) transformation.M13 + vector.Z * (double) transformation.M33 + vector.W * (double) transformation.M43),
      W = (float) (vector.Y * (double) transformation.M24 + vector.X * (double) transformation.M14 + vector.Z * (double) transformation.M34 + vector.W * (double) transformation.M44)
    };
  }

  public static Vector4 Transform(Vector4 vector, Matrix transformation)
  {
    return new()
    {
      X = (float) ((double) transformation.M21 * vector.Y + (double) transformation.M11 * vector.X + (double) transformation.M31 * vector.Z + (double) transformation.M41 * vector.W),
      Y = (float) ((double) transformation.M22 * vector.Y + (double) transformation.M12 * vector.X + (double) transformation.M32 * vector.Z + (double) transformation.M42 * vector.W),
      Z = (float) ((double) transformation.M23 * vector.Y + (double) transformation.M13 * vector.X + (double) transformation.M33 * vector.Z + (double) transformation.M43 * vector.W),
      W = (float) ((double) transformation.M24 * vector.Y + (double) transformation.M14 * vector.X + (double) transformation.M34 * vector.Z + (double) transformation.M44 * vector.W)
    };
  }

  public static void Minimize(ref Vector4 value1, ref Vector4 value2, out Vector4 result)
  {
    float num1 = value1.X >= (double) value2.X ? value2.X : value1.X;
    result.X = num1;
    float num2 = value1.Y >= (double) value2.Y ? value2.Y : value1.Y;
    result.Y = num2;
    float num3 = value1.Z >= (double) value2.Z ? value2.Z : value1.Z;
    result.Z = num3;
    float num4 = value1.W >= (double) value2.W ? value2.W : value1.W;
    result.W = num4;
  }

  public static Vector4 Minimize(Vector4 value1, Vector4 value2)
  {
    return new()
    {
      X = value1.X >= (double) value2.X ? value2.X : value1.X,
      Y = value1.Y >= (double) value2.Y ? value2.Y : value1.Y,
      Z = value1.Z >= (double) value2.Z ? value2.Z : value1.Z,
      W = value1.W >= (double) value2.W ? value2.W : value1.W
    };
  }

  public static void Maximize(ref Vector4 value1, ref Vector4 value2, out Vector4 result)
  {
    float num1 = value1.X <= (double) value2.X ? value2.X : value1.X;
    result.X = num1;
    float num2 = value1.Y <= (double) value2.Y ? value2.Y : value1.Y;
    result.Y = num2;
    float num3 = value1.Z <= (double) value2.Z ? value2.Z : value1.Z;
    result.Z = num3;
    float num4 = value1.W <= (double) value2.W ? value2.W : value1.W;
    result.W = num4;
  }

  public static Vector4 Maximize(Vector4 value1, Vector4 value2)
  {
    return new()
    {
      X = value1.X <= (double) value2.X ? value2.X : value1.X,
      Y = value1.Y <= (double) value2.Y ? value2.Y : value1.Y,
      Z = value1.Z <= (double) value2.Z ? value2.Z : value1.Z,
      W = value1.W <= (double) value2.W ? value2.W : value1.W
    };
  }

  public static Vector4 operator +(Vector4 left, Vector4 right)
  {
    Vector4 vector4;
    vector4.X = left.X + right.X;
    vector4.Y = left.Y + right.Y;
    vector4.Z = left.Z + right.Z;
    vector4.W = left.W + right.W;
    return vector4;
  }

  public static Vector4 operator -(Vector4 value)
  {
    float num1 = -value.X;
    float num2 = -value.Y;
    float num3 = -value.Z;
    float num4 = -value.W;
    Vector4 vector4;
    vector4.X = num1;
    vector4.Y = num2;
    vector4.Z = num3;
    vector4.W = num4;
    return vector4;
  }

  public static Vector4 operator -(Vector4 left, Vector4 right)
  {
    Vector4 vector4;
    vector4.X = left.X - right.X;
    vector4.Y = left.Y - right.Y;
    vector4.Z = left.Z - right.Z;
    vector4.W = left.W - right.W;
    return vector4;
  }

  public static Vector4 operator *(float scale, Vector4 vector) => vector * scale;

  public static Vector4 operator *(Vector4 vector, float scale)
  {
    Vector4 vector4;
    vector4.X = vector.X * scale;
    vector4.Y = vector.Y * scale;
    vector4.Z = vector.Z * scale;
    vector4.W = vector.W * scale;
    return vector4;
  }

  public static Vector4 operator /(Vector4 vector, float scale)
  {
    Vector4 vector4;
    vector4.X = vector.X / scale;
    vector4.Y = vector.Y / scale;
    vector4.Z = vector.Z / scale;
    vector4.W = vector.W / scale;
    return vector4;
  }

  public static bool operator ==(Vector4 left, Vector4 right)
  {
    return Equals(ref left, ref right);
  }

  public static bool operator !=(Vector4 left, Vector4 right)
  {
    return !Equals(ref left, ref right);
  }

  public override string ToString()
  {
    object[] objArray = new object[4];
    float x = X;
    objArray[0] = x.ToString(CultureInfo.CurrentCulture);
    float y = Y;
    objArray[1] = y.ToString(CultureInfo.CurrentCulture);
    float z = Z;
    objArray[2] = z.ToString(CultureInfo.CurrentCulture);
    float w = W;
    objArray[3] = w.ToString(CultureInfo.CurrentCulture);
    return string.Format(CultureInfo.CurrentCulture, "X:{0} Y:{1} Z:{2} W:{3}", objArray);
  }

  public override int GetHashCode()
  {
    float x = X;
    float y = Y;
    float z = Z;
    float w = W;
    int num = z.GetHashCode() + w.GetHashCode() + y.GetHashCode();
    return x.GetHashCode() + num;
  }

  public static bool Equals(ref Vector4 value1, ref Vector4 value2)
  {
    return value1.X == (double) value2.X && value1.Y == (double) value2.Y && value1.Z == (double) value2.Z && value1.W == (double) value2.W;
  }

  public bool Equals(Vector4 other)
  {
    return X == (double) other.X && Y == (double) other.Y && Z == (double) other.Z && W == (double) other.W;
  }

  public override bool Equals(object obj)
  {
    return obj != null && !(obj.GetType() != GetType()) && Equals((Vector4) obj);
  }
}
