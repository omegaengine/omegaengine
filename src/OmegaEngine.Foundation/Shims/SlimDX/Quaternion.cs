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
using System.ComponentModel;
using System.Globalization;

// ReSharper disable once CheckNamespace
namespace SlimDX;

public struct Quaternion(float x, float y, float z, float w) : IEquatable<Quaternion>
{
  public float X = x;
  public float Y = y;
  public float Z = z;
  public float W = w;

  public Quaternion(Vector3 value, float w)
      : this(value.X, value.Y, value.Z, w)
  {}

  public static Quaternion Identity =>
      new()
      {
          X = 0.0f,
          Y = 0.0f,
          Z = 0.0f,
          W = 1f
      };

  [Browsable(false)]
  public bool IsIdentity => X == 0.0 && Y == 0.0 && Z == 0.0 && W == 1.0;

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

  public static void Normalize(ref Quaternion quaternion, out Quaternion result)
  {
    float num = 1f / quaternion.Length();
    result.X = quaternion.X * num;
    result.Y = quaternion.Y * num;
    result.Z = quaternion.Z * num;
    result.W = quaternion.W * num;
  }

  public static Quaternion Normalize(Quaternion quaternion)
  {
    quaternion.Normalize();
    return quaternion;
  }

  public void Normalize()
  {
    float num = 1f / Length();
    X *= num;
    Y *= num;
    Z *= num;
    W *= num;
  }

  public static void Conjugate(ref Quaternion quaternion, out Quaternion result)
  {
    result.X = -quaternion.X;
    result.Y = -quaternion.Y;
    result.Z = -quaternion.Z;
    result.W = quaternion.W;
  }

  public static Quaternion Conjugate(Quaternion quaternion)
  {
    return new()
    {
      X = -quaternion.X,
      Y = -quaternion.Y,
      Z = -quaternion.Z,
      W = quaternion.W
    };
  }

  public void Conjugate()
  {
    ref Quaternion local1 = ref this;
    local1.X = -local1.X;
    ref Quaternion local2 = ref this;
    local2.Y = -local2.Y;
    ref Quaternion local3 = ref this;
    local3.Z = -local3.Z;
  }

  public static void Invert(ref Quaternion quaternion, out Quaternion result)
  {
    double y = quaternion.Y;
    double x = quaternion.X;
    double z = quaternion.Z;
    double w = quaternion.W;
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
    float num11 = (float) (1.0 / (num8 + num10));
    result.X = -quaternion.X * num11;
    result.Y = -quaternion.Y * num11;
    result.Z = -quaternion.Z * num11;
    result.W = quaternion.W * num11;
  }

  public static Quaternion Invert(Quaternion quaternion)
  {
    Quaternion quaternion1 = new();
    double y = quaternion.Y;
    double x = quaternion.X;
    double z = quaternion.Z;
    double w = quaternion.W;
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
    float num11 = (float) (1.0 / (num8 + num10));
    quaternion1.X = -quaternion.X * num11;
    quaternion1.Y = -quaternion.Y * num11;
    quaternion1.Z = -quaternion.Z * num11;
    quaternion1.W = quaternion.W * num11;
    return quaternion1;
  }

  public void Invert()
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
    float num11 = (float) (1.0 / (num8 + num10));
    ref Quaternion local1 = ref this;
    local1.X = -local1.X * num11;
    ref Quaternion local2 = ref this;
    local2.Y = -local2.Y * num11;
    ref Quaternion local3 = ref this;
    local3.Z = -local3.Z * num11;
    W *= num11;
  }

  public static void Add(ref Quaternion left, ref Quaternion right, out Quaternion result)
  {
    result = new()
    {
      X = left.X + right.X,
      Y = left.Y + right.Y,
      Z = left.Z + right.Z,
      W = left.W + right.W
    };
  }

  public static Quaternion Add(Quaternion left, Quaternion right)
  {
    return new()
    {
      X = left.X + right.X,
      Y = left.Y + right.Y,
      Z = left.Z + right.Z,
      W = left.W + right.W
    };
  }

  public static void Divide(ref Quaternion left, ref Quaternion right, out Quaternion result)
  {
    result.X = left.X / right.X;
    result.Y = left.Y / right.Y;
    result.Z = left.Z / right.Z;
    result.W = left.W / right.W;
  }

  public static Quaternion Divide(Quaternion left, Quaternion right)
  {
    return new()
    {
      X = left.X / right.X,
      Y = left.Y / right.Y,
      Z = left.Z / right.Z,
      W = left.W / right.W
    };
  }

  public static float Dot(Quaternion left, Quaternion right)
  {
    return (float) (left.Y * (double) right.Y + left.X * (double) right.X + left.Z * (double) right.Z + left.W * (double) right.W);
  }

  public static void Lerp(
    ref Quaternion start,
    ref Quaternion end,
    float amount,
    out Quaternion result)
  {
    float num1 = 1f - amount;
    if (start.Y * (double) end.Y + start.X * (double) end.X + start.Z * (double) end.Z + start.W * (double) end.W >= 0.0)
    {
      result.X = (float) (start.X * (double) num1 + end.X * (double) amount);
      result.Y = (float) (start.Y * (double) num1 + end.Y * (double) amount);
      result.Z = (float) (start.Z * (double) num1 + end.Z * (double) amount);
      result.W = (float) (start.W * (double) num1 + end.W * (double) amount);
    }
    else
    {
      result.X = (float) (start.X * (double) num1 - end.X * (double) amount);
      result.Y = (float) (start.Y * (double) num1 - end.Y * (double) amount);
      result.Z = (float) (start.Z * (double) num1 - end.Z * (double) amount);
      result.W = (float) (start.W * (double) num1 - end.W * (double) amount);
    }
    float num2 = 1f / result.Length();
    result.X *= num2;
    result.Y *= num2;
    result.Z *= num2;
    result.W *= num2;
  }

  public static Quaternion Lerp(Quaternion start, Quaternion end, float amount)
  {
    Quaternion quaternion = new();
    float num1 = 1f - amount;
    if (start.Y * (double) end.Y + start.X * (double) end.X + start.Z * (double) end.Z + start.W * (double) end.W >= 0.0)
    {
      quaternion.X = (float) (start.X * (double) num1 + end.X * (double) amount);
      quaternion.Y = (float) (start.Y * (double) num1 + end.Y * (double) amount);
      quaternion.Z = (float) (start.Z * (double) num1 + end.Z * (double) amount);
      quaternion.W = (float) (start.W * (double) num1 + end.W * (double) amount);
    }
    else
    {
      quaternion.X = (float) (start.X * (double) num1 - end.X * (double) amount);
      quaternion.Y = (float) (start.Y * (double) num1 - end.Y * (double) amount);
      quaternion.Z = (float) (start.Z * (double) num1 - end.Z * (double) amount);
      quaternion.W = (float) (start.W * (double) num1 - end.W * (double) amount);
    }
    float num2 = 1f / quaternion.Length();
    quaternion.X *= num2;
    quaternion.Y *= num2;
    quaternion.Z *= num2;
    quaternion.W *= num2;
    return quaternion;
  }

  public static void Multiply(ref Quaternion quaternion, float scale, out Quaternion result)
  {
    result.X = quaternion.X * scale;
    result.Y = quaternion.Y * scale;
    result.Z = quaternion.Z * scale;
    result.W = quaternion.W * scale;
  }

  public static Quaternion Multiply(Quaternion quaternion, float scale)
  {
    return new()
    {
      X = quaternion.X * scale,
      Y = quaternion.Y * scale,
      Z = quaternion.Z * scale,
      W = quaternion.W * scale
    };
  }

  public static void Multiply(ref Quaternion left, ref Quaternion right, out Quaternion result)
  {
    float x1 = left.X;
    float y1 = left.Y;
    float z1 = left.Z;
    float w1 = left.W;
    float x2 = right.X;
    float y2 = right.Y;
    float z2 = right.Z;
    float w2 = right.W;
    result.X = (float) (x2 * (double) w1 + w2 * (double) x1 + y2 * (double) z1 - z2 * (double) y1);
    result.Y = (float) (y2 * (double) w1 + w2 * (double) y1 + z2 * (double) x1 - x2 * (double) z1);
    result.Z = (float) (z2 * (double) w1 + w2 * (double) z1 + x2 * (double) y1 - y2 * (double) x1);
    result.W = (float) (w2 * (double) w1 - (y2 * (double) y1 + x2 * (double) x1 + z2 * (double) z1));
  }

  public static Quaternion Multiply(Quaternion left, Quaternion right)
  {
    Quaternion quaternion = new();
    float x1 = left.X;
    float y1 = left.Y;
    float z1 = left.Z;
    float w1 = left.W;
    float x2 = right.X;
    float y2 = right.Y;
    float z2 = right.Z;
    float w2 = right.W;
    quaternion.X = (float) (x2 * (double) w1 + w2 * (double) x1 + y2 * (double) z1 - z2 * (double) y1);
    quaternion.Y = (float) (y2 * (double) w1 + w2 * (double) y1 + z2 * (double) x1 - x2 * (double) z1);
    quaternion.Z = (float) (z2 * (double) w1 + w2 * (double) z1 + x2 * (double) y1 - y2 * (double) x1);
    quaternion.W = (float) (w2 * (double) w1 - (y2 * (double) y1 + x2 * (double) x1 + z2 * (double) z1));
    return quaternion;
  }

  public static void Negate(ref Quaternion quaternion, out Quaternion result)
  {
    result.X = -quaternion.X;
    result.Y = -quaternion.Y;
    result.Z = -quaternion.Z;
    result.W = -quaternion.W;
  }

  public static Quaternion Negate(Quaternion quaternion)
  {
    return new()
    {
      X = -quaternion.X,
      Y = -quaternion.Y,
      Z = -quaternion.Z,
      W = -quaternion.W
    };
  }

  public static void RotationAxis(ref Vector3 axis, float angle, out Quaternion result)
  {
    ref Vector3 local = ref axis;
    Vector3.Normalize(ref local, out local);
    float num1 = angle * 0.5f;
    float num2 = (float) Math.Sin(num1);
    float num3 = (float) Math.Cos(num1);
    result.X = axis.X * num2;
    result.Y = axis.Y * num2;
    result.Z = axis.Z * num2;
    result.W = num3;
  }

  public static Quaternion RotationAxis(Vector3 axis, float angle)
  {
    Quaternion quaternion = new();
    Vector3.Normalize(ref axis, out axis);
    float num1 = angle * 0.5f;
    float num2 = (float) Math.Sin(num1);
    float num3 = (float) Math.Cos(num1);
    quaternion.X = axis.X * num2;
    quaternion.Y = axis.Y * num2;
    quaternion.Z = axis.Z * num2;
    quaternion.W = num3;
    return quaternion;
  }

  public static void RotationMatrix(ref Matrix matrix, out Quaternion result)
  {
    float num1 = matrix.M22 + matrix.M11 + matrix.M33;
    if (num1 > 0.0)
    {
      float num2 = (float) Math.Sqrt(num1 + 1.0);
      result.W = num2 * 0.5f;
      float num3 = 0.5f / num2;
      result.X = (matrix.M23 - matrix.M32) * num3;
      result.Y = (matrix.M31 - matrix.M13) * num3;
      result.Z = (matrix.M12 - matrix.M21) * num3;
    }
    else if (matrix.M11 >= (double) matrix.M22 && matrix.M11 >= (double) matrix.M33)
    {
      float num4 = (float) Math.Sqrt(matrix.M11 + 1.0 - matrix.M22 - matrix.M33);
      float num5 = 0.5f / num4;
      result.X = num4 * 0.5f;
      result.Y = (matrix.M21 + matrix.M12) * num5;
      result.Z = (matrix.M13 + matrix.M31) * num5;
      result.W = (matrix.M23 - matrix.M32) * num5;
    }
    else if (matrix.M22 > (double) matrix.M33)
    {
      float num6 = (float) Math.Sqrt(matrix.M22 + 1.0 - matrix.M11 - matrix.M33);
      float num7 = 0.5f / num6;
      result.X = (matrix.M21 + matrix.M12) * num7;
      result.Y = num6 * 0.5f;
      result.Z = (matrix.M32 + matrix.M23) * num7;
      result.W = (matrix.M31 - matrix.M13) * num7;
    }
    else
    {
      float num8 = (float) Math.Sqrt(matrix.M33 + 1.0 - matrix.M11 - matrix.M22);
      float num9 = 0.5f / num8;
      result.X = (matrix.M13 + matrix.M31) * num9;
      result.Y = (matrix.M32 + matrix.M23) * num9;
      result.Z = num8 * 0.5f;
      result.W = (matrix.M12 - matrix.M21) * num9;
    }
  }

  public static Quaternion RotationMatrix(Matrix matrix)
  {
    Quaternion quaternion = new();
    float num1 = matrix.M22 + matrix.M11 + matrix.M33;
    if (num1 > 0.0)
    {
      float num2 = (float) Math.Sqrt(num1 + 1.0);
      quaternion.W = num2 * 0.5f;
      float num3 = 0.5f / num2;
      quaternion.X = (matrix.M23 - matrix.M32) * num3;
      quaternion.Y = (matrix.M31 - matrix.M13) * num3;
      quaternion.Z = (matrix.M12 - matrix.M21) * num3;
      return quaternion;
    }
    if (matrix.M11 >= (double) matrix.M22 && matrix.M11 >= (double) matrix.M33)
    {
      float num4 = (float) Math.Sqrt(matrix.M11 + 1.0 - matrix.M22 - matrix.M33);
      float num5 = 0.5f / num4;
      quaternion.X = num4 * 0.5f;
      quaternion.Y = (matrix.M21 + matrix.M12) * num5;
      quaternion.Z = (matrix.M13 + matrix.M31) * num5;
      quaternion.W = (matrix.M23 - matrix.M32) * num5;
      return quaternion;
    }
    if (matrix.M22 > (double) matrix.M33)
    {
      float num6 = (float) Math.Sqrt(matrix.M22 + 1.0 - matrix.M11 - matrix.M33);
      float num7 = 0.5f / num6;
      quaternion.X = (matrix.M21 + matrix.M12) * num7;
      quaternion.Y = num6 * 0.5f;
      quaternion.Z = (matrix.M32 + matrix.M23) * num7;
      quaternion.W = (matrix.M31 - matrix.M13) * num7;
      return quaternion;
    }
    float num8 = (float) Math.Sqrt(matrix.M33 + 1.0 - matrix.M11 - matrix.M22);
    float num9 = 0.5f / num8;
    quaternion.X = (matrix.M13 + matrix.M31) * num9;
    quaternion.Y = (matrix.M32 + matrix.M23) * num9;
    quaternion.Z = num8 * 0.5f;
    quaternion.W = (matrix.M12 - matrix.M21) * num9;
    return quaternion;
  }

  public static void RotationYawPitchRoll(
    float yaw,
    float pitch,
    float roll,
    out Quaternion result)
  {
    float num1 = roll * 0.5f;
    float num2 = (float) Math.Sin(num1);
    float num3 = (float) Math.Cos(num1);
    float num4 = pitch * 0.5f;
    float num5 = (float) Math.Sin(num4);
    float num6 = (float) Math.Cos(num4);
    float num7 = yaw * 0.5f;
    float num8 = (float) Math.Sin(num7);
    float num9 = (float) Math.Cos(num7);
    double num10 = num9 * (double) num5;
    double num11 = num8 * (double) num6;
    result.X = (float) (num2 * num11 + num3 * num10);
    result.Y = (float) (num3 * num11 - num2 * num10);
    double num12 = num9 * (double) num6;
    double num13 = num8 * (double) num5;
    result.Z = (float) (num2 * num12 - num3 * num13);
    result.W = (float) (num2 * num13 + num3 * num12);
  }

  public static Quaternion RotationYawPitchRoll(float yaw, float pitch, float roll)
  {
    Quaternion quaternion = new();
    float num1 = roll * 0.5f;
    float num2 = (float) Math.Sin(num1);
    float num3 = (float) Math.Cos(num1);
    float num4 = pitch * 0.5f;
    float num5 = (float) Math.Sin(num4);
    float num6 = (float) Math.Cos(num4);
    float num7 = yaw * 0.5f;
    float num8 = (float) Math.Sin(num7);
    float num9 = (float) Math.Cos(num7);
    double num10 = num9 * (double) num5;
    double num11 = num8 * (double) num6;
    quaternion.X = (float) (num2 * num11 + num3 * num10);
    quaternion.Y = (float) (num3 * num11 - num2 * num10);
    double num12 = num9 * (double) num6;
    double num13 = num8 * (double) num5;
    quaternion.Z = (float) (num2 * num12 - num3 * num13);
    quaternion.W = (float) (num2 * num13 + num3 * num12);
    return quaternion;
  }

  public static void Slerp(
    ref Quaternion start,
    ref Quaternion end,
    float amount,
    out Quaternion result)
  {
    float d = (float) (start.Y * (double) end.Y + start.X * (double) end.X + start.Z * (double) end.Z + start.W * (double) end.W);
    bool flag = false;
    if (d < 0.0)
    {
      flag = true;
      d = -d;
    }
    float num1;
    float num2;
    if (d > 0.9999989867210388)
    {
      num1 = 1f - amount;
      num2 = !flag ? amount : -amount;
    }
    else
    {
      float a = (float) Math.Acos(d);
      float num3 = (float) (1.0 / Math.Sin(a));
      num1 = (float) Math.Sin((1.0 - amount) * a) * num3;
      num2 = !flag ? (float) Math.Sin(a * (double) amount) * num3 : (float) -Math.Sin(a * (double) amount) * num3;
    }
    result.X = (float) (end.X * (double) num2 + start.X * (double) num1);
    result.Y = (float) (end.Y * (double) num2 + start.Y * (double) num1);
    result.Z = (float) (end.Z * (double) num2 + start.Z * (double) num1);
    result.W = (float) (end.W * (double) num2 + start.W * (double) num1);
  }

  public static Quaternion Slerp(Quaternion start, Quaternion end, float amount)
  {
    Quaternion quaternion = new();
    float d = (float) (start.Y * (double) end.Y + start.X * (double) end.X + start.Z * (double) end.Z + start.W * (double) end.W);
    bool flag = false;
    if (d < 0.0)
    {
      flag = true;
      d = -d;
    }
    float num1;
    float num2;
    if (d > 0.9999989867210388)
    {
      num1 = 1f - amount;
      num2 = !flag ? amount : -amount;
    }
    else
    {
      float a = (float) Math.Acos(d);
      float num3 = (float) (1.0 / Math.Sin(a));
      num1 = (float) Math.Sin((1.0 - amount) * a) * num3;
      num2 = !flag ? (float) Math.Sin(a * (double) amount) * num3 : (float) -Math.Sin(a * (double) amount) * num3;
    }
    quaternion.X = (float) (end.X * (double) num2 + start.X * (double) num1);
    quaternion.Y = (float) (end.Y * (double) num2 + start.Y * (double) num1);
    quaternion.Z = (float) (end.Z * (double) num2 + start.Z * (double) num1);
    quaternion.W = (float) (end.W * (double) num2 + start.W * (double) num1);
    return quaternion;
  }

  public static void Subtract(ref Quaternion left, ref Quaternion right, out Quaternion result)
  {
    result.X = left.X - right.X;
    result.Y = left.Y - right.Y;
    result.Z = left.Z - right.Z;
    result.W = left.W - right.W;
  }

  public static Quaternion Subtract(Quaternion left, Quaternion right)
  {
    return new()
    {
      X = left.X - right.X,
      Y = left.Y - right.Y,
      Z = left.Z - right.Z,
      W = left.W - right.W
    };
  }

  public static Quaternion operator *(float scale, Quaternion quaternion)
  {
    return new()
    {
      X = quaternion.X * scale,
      Y = quaternion.Y * scale,
      Z = quaternion.Z * scale,
      W = quaternion.W * scale
    };
  }

  public static Quaternion operator *(Quaternion quaternion, float scale)
  {
    return new()
    {
      X = quaternion.X * scale,
      Y = quaternion.Y * scale,
      Z = quaternion.Z * scale,
      W = quaternion.W * scale
    };
  }

  public static Quaternion operator *(Quaternion left, Quaternion right)
  {
    Quaternion quaternion = new();
    float x1 = left.X;
    float y1 = left.Y;
    float z1 = left.Z;
    float w1 = left.W;
    float x2 = right.X;
    float y2 = right.Y;
    float z2 = right.Z;
    float w2 = right.W;
    quaternion.X = (float) (x2 * (double) w1 + w2 * (double) x1 + y2 * (double) z1 - z2 * (double) y1);
    quaternion.Y = (float) (y2 * (double) w1 + w2 * (double) y1 + z2 * (double) x1 - x2 * (double) z1);
    quaternion.Z = (float) (z2 * (double) w1 + w2 * (double) z1 + x2 * (double) y1 - y2 * (double) x1);
    quaternion.W = (float) (w2 * (double) w1 - (y2 * (double) y1 + x2 * (double) x1 + z2 * (double) z1));
    return quaternion;
  }

  public static Quaternion operator /(Quaternion left, float right)
  {
    return new()
    {
      X = left.X / right,
      Y = left.Y / right,
      Z = left.Z / right,
      W = left.W / right
    };
  }

  public static Quaternion operator +(Quaternion left, Quaternion right)
  {
    return new()
    {
      X = left.X + right.X,
      Y = left.Y + right.Y,
      Z = left.Z + right.Z,
      W = left.W + right.W
    };
  }

  public static Quaternion operator -(Quaternion quaternion)
  {
    return new()
    {
      X = -quaternion.X,
      Y = -quaternion.Y,
      Z = -quaternion.Z,
      W = -quaternion.W
    };
  }

  public static Quaternion operator -(Quaternion left, Quaternion right)
  {
    return new()
    {
      X = left.X - right.X,
      Y = left.Y - right.Y,
      Z = left.Z - right.Z,
      W = left.W - right.W
    };
  }

  public static bool operator ==(Quaternion left, Quaternion right)
  {
    return Equals(ref left, ref right);
  }

  public static bool operator !=(Quaternion left, Quaternion right)
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

  public static bool Equals(ref Quaternion value1, ref Quaternion value2)
  {
    return value1.X == (double) value2.X && value1.Y == (double) value2.Y && value1.Z == (double) value2.Z && value1.W == (double) value2.W;
  }

  public bool Equals(Quaternion other)
  {
    return X == (double) other.X && Y == (double) other.Y && Z == (double) other.Z && W == (double) other.W;
  }

  public override bool Equals(object obj)
  {
    return obj != null && !(obj.GetType() != GetType()) && Equals((Quaternion) obj);
  }
}
