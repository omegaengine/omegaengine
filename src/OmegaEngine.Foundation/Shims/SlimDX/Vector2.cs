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

public struct Vector2 : IEquatable<Vector2>
{
  public float X;
  public float Y;

  public float this[int index]
  {
    get
    {
      if (index == 0)
        return X;
      if (index != 1)
        throw new ArgumentOutOfRangeException(nameof (index), "Indices for Vector2 run from 0 to 1, inclusive.");
      return Y;
    }
    set
    {
      if (index != 0)
      {
        if (index != 1)
          throw new ArgumentOutOfRangeException(nameof (index), "Indices for Vector2 run from 0 to 1, inclusive.");
        Y = value;
      }
      else
        X = value;
    }
  }

  public static Vector2 Zero => new(0.0f, 0.0f);

  public static Vector2 UnitX => new(1f, 0.0f);

  public static Vector2 UnitY => new(0.0f, 1f);

  public Vector2(float x, float y)
  {
    X = x;
    Y = y;
  }

  public Vector2(float value)
  {
    X = value;
    Y = value;
  }

  public float Length()
  {
    double y = Y;
    double x = X;
    double num1 = x * x;
    double num2 = y;
    double num3 = num2 * num2;
    return (float) Math.Sqrt(num1 + num3);
  }

  public float LengthSquared()
  {
    double y = Y;
    double x = X;
    double num1 = x * x;
    double num2 = y;
    double num3 = num2 * num2;
    return (float) (num1 + num3);
  }

  public static void Normalize(ref Vector2 vector, out Vector2 result)
  {
    Vector2 vector2_1 = vector;
    vector2_1.Normalize();
    Vector2 vector2_2 = vector2_1;
    result = vector2_2;
  }

  public static Vector2 Normalize(Vector2 vector)
  {
    vector.Normalize();
    return vector;
  }

  public void Normalize()
  {
    double y = Y;
    double x = X;
    double num1 = x * x;
    double num2 = y;
    double num3 = num2 * num2;
    float num4 = (float) Math.Sqrt(num1 + num3);
    if (num4 == 0.0)
      return;
    float num5 = 1f / num4;
    X *= num5;
    Y *= num5;
  }

  public static void Add(ref Vector2 left, ref Vector2 right, out Vector2 result)
  {
    Vector2 vector2;
    vector2.X = left.X + right.X;
    vector2.Y = left.Y + right.Y;
    result = vector2;
  }

  public static Vector2 Add(Vector2 left, Vector2 right)
  {
    Vector2 vector2;
    vector2.X = left.X + right.X;
    vector2.Y = left.Y + right.Y;
    return vector2;
  }

  public static void Subtract(ref Vector2 left, ref Vector2 right, out Vector2 result)
  {
    Vector2 vector2;
    vector2.X = left.X - right.X;
    vector2.Y = left.Y - right.Y;
    result = vector2;
  }

  public static Vector2 Subtract(Vector2 left, Vector2 right)
  {
    Vector2 vector2;
    vector2.X = left.X - right.X;
    vector2.Y = left.Y - right.Y;
    return vector2;
  }

  public static void Multiply(ref Vector2 vector, float scale, out Vector2 result)
  {
    Vector2 vector2;
    vector2.X = vector.X * scale;
    vector2.Y = vector.Y * scale;
    result = vector2;
  }

  public static Vector2 Multiply(Vector2 value, float scale)
  {
    Vector2 vector2;
    vector2.X = value.X * scale;
    vector2.Y = value.Y * scale;
    return vector2;
  }

  public static void Modulate(ref Vector2 left, ref Vector2 right, out Vector2 result)
  {
    Vector2 vector2;
    vector2.X = left.X * right.X;
    vector2.Y = left.Y * right.Y;
    result = vector2;
  }

  public static Vector2 Modulate(Vector2 left, Vector2 right)
  {
    Vector2 vector2;
    vector2.X = left.X * right.X;
    vector2.Y = left.Y * right.Y;
    return vector2;
  }

  public static void Divide(ref Vector2 vector, float scale, out Vector2 result)
  {
    Vector2 vector2;
    vector2.X = vector.X / scale;
    vector2.Y = vector.Y / scale;
    result = vector2;
  }

  public static Vector2 Divide(Vector2 value, float scale)
  {
    Vector2 vector2;
    vector2.X = value.X / scale;
    vector2.Y = value.Y / scale;
    return vector2;
  }

  public static void Negate(ref Vector2 value, out Vector2 result)
  {
    float num1 = -value.X;
    float num2 = -value.Y;
    Vector2 vector2;
    vector2.X = num1;
    vector2.Y = num2;
    result = vector2;
  }

  public static Vector2 Negate(Vector2 value)
  {
    float num1 = -value.X;
    float num2 = -value.Y;
    Vector2 vector2;
    vector2.X = num1;
    vector2.Y = num2;
    return vector2;
  }

  public static void Barycentric(
    ref Vector2 value1,
    ref Vector2 value2,
    ref Vector2 value3,
    float amount1,
    float amount2,
    out Vector2 result)
  {
    Vector2 vector2;
    vector2.X = (float) ((value2.X - (double) value1.X) * amount1 + value1.X + (value3.X - (double) value1.X) * amount2);
    vector2.Y = (float) ((value2.Y - (double) value1.Y) * amount1 + value1.Y + (value3.Y - (double) value1.Y) * amount2);
    result = vector2;
  }

  public static Vector2 Barycentric(
    Vector2 value1,
    Vector2 value2,
    Vector2 value3,
    float amount1,
    float amount2)
  {
    return new()
    {
      X = (float) ((value2.X - (double) value1.X) * amount1 + value1.X + (value3.X - (double) value1.X) * amount2),
      Y = (float) ((value2.Y - (double) value1.Y) * amount1 + value1.Y + (value3.Y - (double) value1.Y) * amount2)
    };
  }

  public static void CatmullRom(
    ref Vector2 value1,
    ref Vector2 value2,
    ref Vector2 value3,
    ref Vector2 value4,
    float amount,
    out Vector2 result)
  {
    double num1 = amount;
    float num2 = (float) (num1 * num1);
    float num3 = num2 * amount;
    result = new()
    {
      X = (float) (((value1.X * 2.0 - value2.X * 5.0 + value3.X * 4.0 - value4.X) * num2 + ((value3.X - (double) value1.X) * amount + value2.X * 2.0) + (value2.X * 3.0 - value1.X - value3.X * 3.0 + value4.X) * num3) * 0.5),
      Y = (float) (((value1.Y * 2.0 - value2.Y * 5.0 + value3.Y * 4.0 - value4.Y) * num2 + ((value3.Y - (double) value1.Y) * amount + value2.Y * 2.0) + (value2.Y * 3.0 - value1.Y - value3.Y * 3.0 + value4.Y) * num3) * 0.5)
    };
  }

  public static Vector2 CatmullRom(
    Vector2 value1,
    Vector2 value2,
    Vector2 value3,
    Vector2 value4,
    float amount)
  {
    Vector2 vector2 = new();
    double num1 = amount;
    float num2 = (float) (num1 * num1);
    float num3 = num2 * amount;
    vector2.X = (float) (((value1.X * 2.0 - value2.X * 5.0 + value3.X * 4.0 - value4.X) * num2 + ((value3.X - (double) value1.X) * amount + value2.X * 2.0) + (value2.X * 3.0 - value1.X - value3.X * 3.0 + value4.X) * num3) * 0.5);
    vector2.Y = (float) (((value1.Y * 2.0 - value2.Y * 5.0 + value3.Y * 4.0 - value4.Y) * num2 + ((value3.Y - (double) value1.Y) * amount + value2.Y * 2.0) + (value2.Y * 3.0 - value1.Y - value3.Y * 3.0 + value4.Y) * num3) * 0.5);
    return vector2;
  }

  public static void Clamp(
    ref Vector2 value,
    ref Vector2 min,
    ref Vector2 max,
    out Vector2 result)
  {
    float x = value.X;
    float num1 = x <= (double) max.X ? x : max.X;
    float num2 = num1 >= (double) min.X ? num1 : min.X;
    float y = value.Y;
    float num3 = y <= (double) max.Y ? y : max.Y;
    float num4 = num3 >= (double) min.Y ? num3 : min.Y;
    Vector2 vector2;
    vector2.X = num2;
    vector2.Y = num4;
    result = vector2;
  }

  public static Vector2 Clamp(Vector2 value, Vector2 min, Vector2 max)
  {
    float x = value.X;
    float num1 = x <= (double) max.X ? x : max.X;
    float num2 = num1 >= (double) min.X ? num1 : min.X;
    float y = value.Y;
    float num3 = y <= (double) max.Y ? y : max.Y;
    float num4 = num3 >= (double) min.Y ? num3 : min.Y;
    Vector2 vector2;
    vector2.X = num2;
    vector2.Y = num4;
    return vector2;
  }

  public static void Hermite(
    ref Vector2 value1,
    ref Vector2 tangent1,
    ref Vector2 value2,
    ref Vector2 tangent2,
    float amount,
    out Vector2 result)
  {
    double num1 = amount;
    float num2 = (float) (num1 * num1);
    float num3 = num2 * amount;
    double num4 = num2 * 3.0;
    float num5 = (float) (num3 * 2.0 - num4 + 1.0);
    float num6 = (float) (num3 * -2.0 + num4);
    float num7 = num3 - num2 * 2f + amount;
    float num8 = num3 - num2;
    result = new()
    {
      X = (float) (value2.X * (double) num6 + value1.X * (double) num5 + tangent1.X * (double) num7 + tangent2.X * (double) num8),
      Y = (float) (value2.Y * (double) num6 + value1.Y * (double) num5 + tangent1.Y * (double) num7 + tangent2.Y * (double) num8)
    };
  }

  public static Vector2 Hermite(
    Vector2 value1,
    Vector2 tangent1,
    Vector2 value2,
    Vector2 tangent2,
    float amount)
  {
    Vector2 vector2 = new();
    double num1 = amount;
    float num2 = (float) (num1 * num1);
    float num3 = num2 * amount;
    double num4 = num2 * 3.0;
    float num5 = (float) (num3 * 2.0 - num4 + 1.0);
    float num6 = (float) (num3 * -2.0 + num4);
    float num7 = num3 - num2 * 2f + amount;
    float num8 = num3 - num2;
    vector2.X = (float) (value2.X * (double) num6 + value1.X * (double) num5 + tangent1.X * (double) num7 + tangent2.X * (double) num8);
    vector2.Y = (float) (value2.Y * (double) num6 + value1.Y * (double) num5 + tangent1.Y * (double) num7 + tangent2.Y * (double) num8);
    return vector2;
  }

  public static void Lerp(ref Vector2 start, ref Vector2 end, float amount, out Vector2 result)
  {
    result = new()
    {
      X = (end.X - start.X) * amount + start.X,
      Y = (end.Y - start.Y) * amount + start.Y
    };
  }

  public static Vector2 Lerp(Vector2 start, Vector2 end, float amount)
  {
    return new()
    {
      X = (end.X - start.X) * amount + start.X,
      Y = (end.Y - start.Y) * amount + start.Y
    };
  }

  public static void SmoothStep(
    ref Vector2 start,
    ref Vector2 end,
    float amount,
    out Vector2 result)
  {
    float num1 = amount <= 1.0 ? (amount >= 0.0 ? amount : 0.0f) : 1f;
    double num2 = num1;
    double num3 = 3.0 - num1 * 2.0;
    double num4 = num2;
    double num5 = num4 * num4;
    amount = (float) (num3 * num5);
    result = new()
    {
      X = (end.X - start.X) * amount + start.X,
      Y = (end.Y - start.Y) * amount + start.Y
    };
  }

  public static Vector2 SmoothStep(Vector2 start, Vector2 end, float amount)
  {
    Vector2 vector2 = new();
    float num1 = amount <= 1.0 ? (amount >= 0.0 ? amount : 0.0f) : 1f;
    double num2 = num1;
    double num3 = 3.0 - num1 * 2.0;
    double num4 = num2;
    double num5 = num4 * num4;
    amount = (float) (num3 * num5);
    vector2.X = (end.X - start.X) * amount + start.X;
    vector2.Y = (end.Y - start.Y) * amount + start.Y;
    return vector2;
  }

  public static float Distance(Vector2 value1, Vector2 value2)
  {
    float num1 = value1.X - value2.X;
    double num2 = value1.Y - value2.Y;
    double num3 = num1;
    double num4 = num3 * num3;
    double num5 = num2;
    double num6 = num5 * num5;
    return (float) Math.Sqrt(num4 + num6);
  }

  public static float DistanceSquared(Vector2 value1, Vector2 value2)
  {
    float num1 = value1.X - value2.X;
    double num2 = value1.Y - value2.Y;
    double num3 = num1;
    double num4 = num3 * num3;
    double num5 = num2;
    double num6 = num5 * num5;
    return (float) (num4 + num6);
  }

  public static float Dot(Vector2 left, Vector2 right)
  {
    return (float) (left.Y * (double) right.Y + left.X * (double) right.X);
  }

  public static Vector4[] Transform(Vector2[] vectors, ref Quaternion rotation)
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
      double num15 = 1.0 - num7 - num12;
      double num16 = num8 + (double) num6;
      double num17 = num9 - (double) num5;
      double num18 = num11 + (double) num4;
      do
      {
        vector4Array[index] = new()
        {
          X = (float) (vectors[index].Y * num14 + vectors[index].X * num13),
          Y = (float) (vectors[index].X * num16 + vectors[index].Y * num15),
          Z = (float) (vectors[index].Y * num18 + vectors[index].X * num17),
          W = 1f
        };
        ++index;
      }
      while (index < length);
    }
    return vector4Array;
  }

  public static void Transform(ref Vector2 vector, ref Quaternion rotation, out Vector4 result)
  {
    double x = rotation.X;
    float num1 = (float) (x + x);
    double y = rotation.Y;
    float num2 = (float) (y + y);
    double z = rotation.Z;
    float num3 = (float) (z + z);
    float num4 = rotation.W * num3;
    float num5 = rotation.X * num2;
    float num6 = rotation.Z * num3;
    result = new()
    {
      X = (float) ((1.0 - rotation.Y * num2 - num6) * vector.X + vector.Y * (num5 - (double) num4)),
      Y = (float) ((1.0 - rotation.X * num1 - num6) * vector.Y + vector.X * (num5 + (double) num4)),
      Z = (float) ((rotation.X * num3 - (double) (rotation.W * num2)) * vector.X + (rotation.Y * num3 + (double) (rotation.W * num1)) * vector.Y),
      W = 1f
    };
  }

  public static Vector4 Transform(Vector2 vector, Quaternion rotation)
  {
    Vector4 vector4 = new();
    double x = rotation.X;
    float num1 = (float) (x + x);
    double y = rotation.Y;
    float num2 = (float) (y + y);
    double z = rotation.Z;
    float num3 = (float) (z + z);
    float num4 = rotation.W * num3;
    float num5 = rotation.X * num2;
    float num6 = rotation.Z * num3;
    vector4.X = (float) ((1.0 - rotation.Y * num2 - num6) * vector.X + vector.Y * (num5 - (double) num4));
    vector4.Y = (float) ((1.0 - rotation.X * num1 - num6) * vector.Y + vector.X * (num5 + (double) num4));
    vector4.Z = (float) ((rotation.X * num3 - (double) (rotation.W * num2)) * vector.X + (rotation.Y * num3 + (double) (rotation.W * num1)) * vector.Y);
    vector4.W = 1f;
    return vector4;
  }

  public static Vector4[] Transform(Vector2[] vectors, ref Matrix transformation)
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
          X = (float) (vectors[index].Y * (double) transformation.M21 + vectors[index].X * (double) transformation.M11) + transformation.M41,
          Y = (float) (vectors[index].Y * (double) transformation.M22 + vectors[index].X * (double) transformation.M12) + transformation.M42,
          Z = (float) (vectors[index].Y * (double) transformation.M23 + vectors[index].X * (double) transformation.M13) + transformation.M43,
          W = (float) (vectors[index].Y * (double) transformation.M24 + vectors[index].X * (double) transformation.M14) + transformation.M44
        };
        ++index;
      }
      while (index < length);
    }
    return vector4Array;
  }

  public static void Transform(ref Vector2 vector, ref Matrix transformation, out Vector4 result)
  {
    result = new()
    {
      X = (float) (vector.Y * (double) transformation.M21 + vector.X * (double) transformation.M11) + transformation.M41,
      Y = (float) (vector.Y * (double) transformation.M22 + vector.X * (double) transformation.M12) + transformation.M42,
      Z = (float) (vector.Y * (double) transformation.M23 + vector.X * (double) transformation.M13) + transformation.M43,
      W = (float) (vector.Y * (double) transformation.M24 + vector.X * (double) transformation.M14) + transformation.M44
    };
  }

  public static Vector4 Transform(Vector2 vector, Matrix transformation)
  {
    return new()
    {
      X = (float) (transformation.M21 * (double) vector.Y + transformation.M11 * (double) vector.X) + transformation.M41,
      Y = (float) (transformation.M22 * (double) vector.Y + transformation.M12 * (double) vector.X) + transformation.M42,
      Z = (float) (transformation.M23 * (double) vector.Y + transformation.M13 * (double) vector.X) + transformation.M43,
      W = (float) (transformation.M24 * (double) vector.Y + transformation.M14 * (double) vector.X) + transformation.M44
    };
  }

  public static Vector2[] TransformCoordinate(Vector2[] coordinates, ref Matrix transformation)
  {
    if (coordinates == null)
      throw new ArgumentNullException(nameof (coordinates));
    Vector4 vector4 = new();
    int length = coordinates.Length;
    Vector2[] vector2Array = new Vector2[length];
    int index = 0;
    if (0 < length)
    {
      do
      {
        vector4.X = (float) (coordinates[index].Y * (double) transformation.M21 + coordinates[index].X * (double) transformation.M11) + transformation.M41;
        vector4.Y = (float) (coordinates[index].Y * (double) transformation.M22 + coordinates[index].X * (double) transformation.M12) + transformation.M42;
        vector4.Z = (float) (coordinates[index].Y * (double) transformation.M23 + coordinates[index].X * (double) transformation.M13) + transformation.M43;
        float num = (float) (1.0 / (coordinates[index].Y * (double) transformation.M24 + coordinates[index].X * (double) transformation.M14 + transformation.M44));
        vector4.W = num;
        Vector2 vector2;
        vector2.X = vector4.X * num;
        vector2.Y = vector4.Y * num;
        vector2Array[index] = vector2;
        ++index;
      }
      while (index < length);
    }
    return vector2Array;
  }

  public static void TransformCoordinate(
    ref Vector2 coordinate,
    ref Matrix transformation,
    out Vector2 result)
  {
    Vector4 vector4 = new()
    {
        X = (float) (coordinate.Y * (double) transformation.M21 + coordinate.X * (double) transformation.M11) + transformation.M41,
        Y = (float) (coordinate.Y * (double) transformation.M22 + coordinate.X * (double) transformation.M12) + transformation.M42,
        Z = (float) (coordinate.Y * (double) transformation.M23 + coordinate.X * (double) transformation.M13) + transformation.M43
    };
    float num = (float) (1.0 / (coordinate.Y * (double) transformation.M24 + coordinate.X * (double) transformation.M14 + transformation.M44));
    vector4.W = num;
    Vector2 vector2;
    vector2.X = vector4.X * num;
    vector2.Y = vector4.Y * num;
    result = vector2;
  }

  public static Vector2 TransformCoordinate(Vector2 coordinate, Matrix transformation)
  {
    Vector4 vector4 = new();
    vector4.X = (float) (transformation.M21 * (double) coordinate.Y + transformation.M11 * (double) coordinate.X) + transformation.M41;
    vector4.Y = (float) (transformation.M22 * (double) coordinate.Y + transformation.M12 * (double) coordinate.X) + transformation.M42;
    vector4.Z = (float) (transformation.M23 * (double) coordinate.Y + transformation.M13 * (double) coordinate.X) + transformation.M43;
    float num = (float) (1.0 / (transformation.M24 * (double) coordinate.Y + transformation.M14 * (double) coordinate.X + transformation.M44));
    vector4.W = num;
    Vector2 vector2;
    vector2.X = vector4.X * num;
    vector2.Y = vector4.Y * num;
    return vector2;
  }

  public static Vector2[] TransformNormal(Vector2[] normals, ref Matrix transformation)
  {
    int length = normals?.Length ?? throw new ArgumentNullException(nameof (normals));
    Vector2[] vector2Array = new Vector2[length];
    int index = 0;
    if (0 < length)
    {
      do
      {
        vector2Array[index] = new()
        {
          X = (float) (normals[index].Y * (double) transformation.M21 + normals[index].X * (double) transformation.M11),
          Y = (float) (normals[index].Y * (double) transformation.M22 + normals[index].X * (double) transformation.M12)
        };
        ++index;
      }
      while (index < length);
    }
    return vector2Array;
  }

  public static void TransformNormal(
    ref Vector2 normal,
    ref Matrix transformation,
    out Vector2 result)
  {
    result = new()
    {
      X = (float) (normal.Y * (double) transformation.M21 + normal.X * (double) transformation.M11),
      Y = (float) (normal.Y * (double) transformation.M22 + normal.X * (double) transformation.M12)
    };
  }

  public static Vector2 TransformNormal(Vector2 normal, Matrix transformation)
  {
    return new()
    {
      X = (float) (transformation.M21 * (double) normal.Y + transformation.M11 * (double) normal.X),
      Y = (float) (transformation.M22 * (double) normal.Y + transformation.M12 * (double) normal.X)
    };
  }

  public static void Minimize(ref Vector2 value1, ref Vector2 value2, out Vector2 result)
  {
    result = new()
    {
      X = value1.X >= (double) value2.X ? value2.X : value1.X,
      Y = value1.Y >= (double) value2.Y ? value2.Y : value1.Y
    };
  }

  public static Vector2 Minimize(Vector2 value1, Vector2 value2)
  {
    return new()
    {
      X = value1.X >= (double) value2.X ? value2.X : value1.X,
      Y = value1.Y >= (double) value2.Y ? value2.Y : value1.Y
    };
  }

  public static void Maximize(ref Vector2 value1, ref Vector2 value2, out Vector2 result)
  {
    result = new()
    {
      X = value1.X <= (double) value2.X ? value2.X : value1.X,
      Y = value1.Y <= (double) value2.Y ? value2.Y : value1.Y
    };
  }

  public static Vector2 Maximize(Vector2 value1, Vector2 value2)
  {
    return new()
    {
      X = value1.X <= (double) value2.X ? value2.X : value1.X,
      Y = value1.Y <= (double) value2.Y ? value2.Y : value1.Y
    };
  }

  public static Vector2 operator +(Vector2 left, Vector2 right)
  {
    Vector2 vector2;
    vector2.X = left.X + right.X;
    vector2.Y = left.Y + right.Y;
    return vector2;
  }

  public static Vector2 operator -(Vector2 value)
  {
    float num1 = -value.X;
    float num2 = -value.Y;
    Vector2 vector2;
    vector2.X = num1;
    vector2.Y = num2;
    return vector2;
  }

  public static Vector2 operator -(Vector2 left, Vector2 right)
  {
    Vector2 vector2;
    vector2.X = left.X - right.X;
    vector2.Y = left.Y - right.Y;
    return vector2;
  }

  public static Vector2 operator *(float scale, Vector2 vector)
  {
    Vector2 vector2_1 = vector;
    Vector2 vector2_2;
    vector2_2.X = vector2_1.X * scale;
    vector2_2.Y = vector2_1.Y * scale;
    return vector2_2;
  }

  public static Vector2 operator *(Vector2 vector, float scale)
  {
    Vector2 vector2;
    vector2.X = vector.X * scale;
    vector2.Y = vector.Y * scale;
    return vector2;
  }

  public static Vector2 operator /(Vector2 vector, float scale)
  {
    Vector2 vector2;
    vector2.X = vector.X / scale;
    vector2.Y = vector.Y / scale;
    return vector2;
  }

  public static bool operator ==(Vector2 left, Vector2 right)
  {
    return left.X == (double) right.X && left.Y == (double) right.Y;
  }

  public static bool operator !=(Vector2 left, Vector2 right)
  {
    return (left.X != (double) right.X || left.Y != (double) right.Y ? 0 : 1) == 0;
  }

  public override string ToString()
  {
    object[] objArray = new object[2];
    float x = X;
    objArray[0] = x.ToString(CultureInfo.CurrentCulture);
    float y = Y;
    objArray[1] = y.ToString(CultureInfo.CurrentCulture);
    return string.Format(CultureInfo.CurrentCulture, "X:{0} Y:{1}", objArray);
  }

  public override int GetHashCode()
  {
    float x = X;
    return Y.GetHashCode() + x.GetHashCode();
  }

  public static bool Equals(ref Vector2 value1, ref Vector2 value2)
  {
    return value1.X == (double) value2.X && value1.Y == (double) value2.Y;
  }

  public bool Equals(Vector2 other)
  {
    return X == (double) other.X && Y == (double) other.Y;
  }

  public override bool Equals(object obj)
  {
    return obj != null && !(obj.GetType() != GetType()) && Equals((Vector2) obj);
  }
}
