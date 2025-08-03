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

public struct Matrix : IEquatable<Matrix>
{
  public float M11;
  public float M12;
  public float M13;
  public float M14;
  public float M21;
  public float M22;
  public float M23;
  public float M24;
  public float M31;
  public float M32;
  public float M33;
  public float M34;
  public float M41;
  public float M42;
  public float M43;
  public float M44;

  [Browsable(false)]
  public float this[int row, int column]
  {
    get
    {
      switch (row)
      {
        case 0:
        case 1:
        case 2:
        case 3:
          switch (column)
          {
            case 0:
            case 1:
            case 2:
            case 3:
              switch (row * 4 + column)
              {
                case 0:
                  return M11;
                case 1:
                  return M12;
                case 2:
                  return M13;
                case 3:
                  return M14;
                case 4:
                  return M21;
                case 5:
                  return M22;
                case 6:
                  return M23;
                case 7:
                  return M24;
                case 8:
                  return M31;
                case 9:
                  return M32;
                case 10:
                  return M33;
                case 11:
                  return M34;
                case 12:
                  return M41;
                case 13:
                  return M42;
                case 14:
                  return M43;
                case 15:
                  return M44;
                default:
                  return 0.0f;
              }
            default:
              throw new ArgumentOutOfRangeException(nameof (column), "Rows and columns for matrices run from 0 to 3, inclusive.");
          }
        default:
          throw new ArgumentOutOfRangeException(nameof (row), "Rows and columns for matrices run from 0 to 3, inclusive.");
      }
    }
    set
    {
      switch (row)
      {
        case 0:
        case 1:
        case 2:
        case 3:
          switch (column)
          {
            case 0:
            case 1:
            case 2:
            case 3:
              switch (row * 4 + column)
              {
                case 0:
                  M11 = value;
                  return;
                case 1:
                  M12 = value;
                  return;
                case 2:
                  M13 = value;
                  return;
                case 3:
                  M14 = value;
                  return;
                case 4:
                  M21 = value;
                  return;
                case 5:
                  M22 = value;
                  return;
                case 6:
                  M23 = value;
                  return;
                case 7:
                  M24 = value;
                  return;
                case 8:
                  M31 = value;
                  return;
                case 9:
                  M32 = value;
                  return;
                case 10:
                  M33 = value;
                  return;
                case 11:
                  M34 = value;
                  return;
                case 12:
                  M41 = value;
                  return;
                case 13:
                  M42 = value;
                  return;
                case 14:
                  M43 = value;
                  return;
                case 15:
                  M44 = value;
                  return;
                default:
                  return;
              }
            default:
              throw new ArgumentOutOfRangeException(nameof (column), "Rows and columns for matrices run from 0 to 3, inclusive.");
          }
        default:
          throw new ArgumentOutOfRangeException(nameof (row), "Rows and columns for matrices run from 0 to 3, inclusive.");
      }
    }
  }

  public Vector4 get_Rows(int row) => new(this[row, 0], this[row, 1], this[row, 2], this[row, 3]);

  public void set_Rows(int row, Vector4 value)
  {
    this[row, 0] = value.X;
    this[row, 1] = value.Y;
    this[row, 2] = value.Z;
    this[row, 3] = value.W;
  }

  public Vector4 get_Columns(int column) => new(this[0, column], this[1, column], this[2, column], this[3, column]);

  public void set_Columns(int column, Vector4 value)
  {
    this[0, column] = value.X;
    this[1, column] = value.Y;
    this[2, column] = value.Z;
    this[3, column] = value.W;
  }

  public static Matrix Identity =>
      new()
      {
          M11 = 1f,
          M22 = 1f,
          M33 = 1f,
          M44 = 1f
      };

  [Browsable(false)]
  public bool IsIdentity => M11 == 1.0 && M22 == 1.0 && M33 == 1.0 && M44 == 1.0 && M12 == 0.0 && M13 == 0.0 && M14 == 0.0 && M21 == 0.0 && M23 == 0.0 && M24 == 0.0 && M31 == 0.0 && M32 == 0.0 && M34 == 0.0 && M41 == 0.0 && M42 == 0.0 && M43 == 0.0;

  public float[] ToArray()
  {
    return
    [
      M11,
      M12,
      M13,
      M14,
      M21,
      M22,
      M23,
      M24,
      M31,
      M32,
      M33,
      M34,
      M41,
      M42,
      M43,
      M44
    ];
  }

  public float Determinant()
  {
    float num1 = (float) (M44 * (double) M33 - M43 * (double) M34);
    float num2 = (float) (M32 * (double) M44 - M42 * (double) M34);
    float num3 = (float) (M32 * (double) M43 - M42 * (double) M33);
    float num4 = (float) (M31 * (double) M44 - M41 * (double) M34);
    float num5 = (float) (M31 * (double) M43 - M41 * (double) M33);
    float num6 = (float) (M31 * (double) M42 - M41 * (double) M32);
    return (float) ((M22 * (double) num1 - M23 * (double) num2 + M24 * (double) num3) * M11 - (M21 * (double) num1 - M23 * (double) num4 + M24 * (double) num5) * M12 + (M21 * (double) num2 - M22 * (double) num4 + M24 * (double) num6) * M13 - (M21 * (double) num3 - M22 * (double) num5 + M23 * (double) num6) * M14);
  }

  public static void Add(ref Matrix left, ref Matrix right, out Matrix result)
  {
    result = new()
    {
      M11 = left.M11 + right.M11,
      M12 = left.M12 + right.M12,
      M13 = left.M13 + right.M13,
      M14 = left.M14 + right.M14,
      M21 = left.M21 + right.M21,
      M22 = left.M22 + right.M22,
      M23 = left.M23 + right.M23,
      M24 = left.M24 + right.M24,
      M31 = left.M31 + right.M31,
      M32 = left.M32 + right.M32,
      M33 = left.M33 + right.M33,
      M34 = left.M34 + right.M34,
      M41 = left.M41 + right.M41,
      M42 = left.M42 + right.M42,
      M43 = left.M43 + right.M43,
      M44 = left.M44 + right.M44
    };
  }

  public static Matrix Add(Matrix left, Matrix right)
  {
    return new()
    {
      M11 = left.M11 + right.M11,
      M12 = left.M12 + right.M12,
      M13 = left.M13 + right.M13,
      M14 = left.M14 + right.M14,
      M21 = left.M21 + right.M21,
      M22 = left.M22 + right.M22,
      M23 = left.M23 + right.M23,
      M24 = left.M24 + right.M24,
      M31 = left.M31 + right.M31,
      M32 = left.M32 + right.M32,
      M33 = left.M33 + right.M33,
      M34 = left.M34 + right.M34,
      M41 = left.M41 + right.M41,
      M42 = left.M42 + right.M42,
      M43 = left.M43 + right.M43,
      M44 = left.M44 + right.M44
    };
  }

  public static void Subtract(ref Matrix left, ref Matrix right, out Matrix result)
  {
    result = new()
    {
      M11 = left.M11 - right.M11,
      M12 = left.M12 - right.M12,
      M13 = left.M13 - right.M13,
      M14 = left.M14 - right.M14,
      M21 = left.M21 - right.M21,
      M22 = left.M22 - right.M22,
      M23 = left.M23 - right.M23,
      M24 = left.M24 - right.M24,
      M31 = left.M31 - right.M31,
      M32 = left.M32 - right.M32,
      M33 = left.M33 - right.M33,
      M34 = left.M34 - right.M34,
      M41 = left.M41 - right.M41,
      M42 = left.M42 - right.M42,
      M43 = left.M43 - right.M43,
      M44 = left.M44 - right.M44
    };
  }

  public static Matrix Subtract(Matrix left, Matrix right)
  {
    return new()
    {
      M11 = left.M11 - right.M11,
      M12 = left.M12 - right.M12,
      M13 = left.M13 - right.M13,
      M14 = left.M14 - right.M14,
      M21 = left.M21 - right.M21,
      M22 = left.M22 - right.M22,
      M23 = left.M23 - right.M23,
      M24 = left.M24 - right.M24,
      M31 = left.M31 - right.M31,
      M32 = left.M32 - right.M32,
      M33 = left.M33 - right.M33,
      M34 = left.M34 - right.M34,
      M41 = left.M41 - right.M41,
      M42 = left.M42 - right.M42,
      M43 = left.M43 - right.M43,
      M44 = left.M44 - right.M44
    };
  }

  public static void Multiply(ref Matrix left, float right, out Matrix result)
  {
    result = new()
    {
      M11 = left.M11 * right,
      M12 = left.M12 * right,
      M13 = left.M13 * right,
      M14 = left.M14 * right,
      M21 = left.M21 * right,
      M22 = left.M22 * right,
      M23 = left.M23 * right,
      M24 = left.M24 * right,
      M31 = left.M31 * right,
      M32 = left.M32 * right,
      M33 = left.M33 * right,
      M34 = left.M34 * right,
      M41 = left.M41 * right,
      M42 = left.M42 * right,
      M43 = left.M43 * right,
      M44 = left.M44 * right
    };
  }

  public static Matrix Multiply(Matrix left, float right)
  {
    return new()
    {
      M11 = left.M11 * right,
      M12 = left.M12 * right,
      M13 = left.M13 * right,
      M14 = left.M14 * right,
      M21 = left.M21 * right,
      M22 = left.M22 * right,
      M23 = left.M23 * right,
      M24 = left.M24 * right,
      M31 = left.M31 * right,
      M32 = left.M32 * right,
      M33 = left.M33 * right,
      M34 = left.M34 * right,
      M41 = left.M41 * right,
      M42 = left.M42 * right,
      M43 = left.M43 * right,
      M44 = left.M44 * right
    };
  }

  public static void Multiply(ref Matrix left, ref Matrix right, out Matrix result)
  {
    result = new()
    {
      M11 = (float) (left.M12 * (double) right.M21 + left.M11 * (double) right.M11 + left.M13 * (double) right.M31 + left.M14 * (double) right.M41),
      M12 = (float) (left.M12 * (double) right.M22 + left.M11 * (double) right.M12 + left.M13 * (double) right.M32 + left.M14 * (double) right.M42),
      M13 = (float) (left.M12 * (double) right.M23 + left.M11 * (double) right.M13 + left.M13 * (double) right.M33 + left.M14 * (double) right.M43),
      M14 = (float) (left.M12 * (double) right.M24 + left.M11 * (double) right.M14 + left.M13 * (double) right.M34 + left.M14 * (double) right.M44),
      M21 = (float) (left.M22 * (double) right.M21 + left.M21 * (double) right.M11 + left.M23 * (double) right.M31 + left.M24 * (double) right.M41),
      M22 = (float) (left.M22 * (double) right.M22 + left.M21 * (double) right.M12 + left.M23 * (double) right.M32 + left.M24 * (double) right.M42),
      M23 = (float) (left.M22 * (double) right.M23 + left.M21 * (double) right.M13 + left.M23 * (double) right.M33 + left.M24 * (double) right.M43),
      M24 = (float) (left.M22 * (double) right.M24 + left.M21 * (double) right.M14 + left.M23 * (double) right.M34 + left.M24 * (double) right.M44),
      M31 = (float) (left.M32 * (double) right.M21 + left.M31 * (double) right.M11 + left.M33 * (double) right.M31 + left.M34 * (double) right.M41),
      M32 = (float) (left.M32 * (double) right.M22 + left.M31 * (double) right.M12 + left.M33 * (double) right.M32 + left.M34 * (double) right.M42),
      M33 = (float) (left.M32 * (double) right.M23 + left.M31 * (double) right.M13 + left.M33 * (double) right.M33 + left.M34 * (double) right.M43),
      M34 = (float) (left.M32 * (double) right.M24 + left.M31 * (double) right.M14 + left.M33 * (double) right.M34 + left.M34 * (double) right.M44),
      M41 = (float) (left.M42 * (double) right.M21 + left.M41 * (double) right.M11 + left.M43 * (double) right.M31 + left.M44 * (double) right.M41),
      M42 = (float) (left.M42 * (double) right.M22 + left.M41 * (double) right.M12 + left.M43 * (double) right.M32 + left.M44 * (double) right.M42),
      M43 = (float) (left.M42 * (double) right.M23 + left.M41 * (double) right.M13 + left.M43 * (double) right.M33 + left.M44 * (double) right.M43),
      M44 = (float) (left.M42 * (double) right.M24 + left.M41 * (double) right.M14 + left.M43 * (double) right.M34 + left.M44 * (double) right.M44)
    };
  }

  public static Matrix Multiply(Matrix left, Matrix right)
  {
    return new()
    {
      M11 = (float) (right.M21 * (double) left.M12 + left.M11 * (double) right.M11 + right.M31 * (double) left.M13 + right.M41 * (double) left.M14),
      M12 = (float) (right.M22 * (double) left.M12 + right.M12 * (double) left.M11 + right.M32 * (double) left.M13 + right.M42 * (double) left.M14),
      M13 = (float) (right.M23 * (double) left.M12 + right.M13 * (double) left.M11 + right.M33 * (double) left.M13 + right.M43 * (double) left.M14),
      M14 = (float) (right.M24 * (double) left.M12 + right.M14 * (double) left.M11 + right.M34 * (double) left.M13 + right.M44 * (double) left.M14),
      M21 = (float) (left.M22 * (double) right.M21 + left.M21 * (double) right.M11 + left.M23 * (double) right.M31 + left.M24 * (double) right.M41),
      M22 = (float) (left.M22 * (double) right.M22 + left.M21 * (double) right.M12 + left.M23 * (double) right.M32 + left.M24 * (double) right.M42),
      M23 = (float) (right.M23 * (double) left.M22 + right.M13 * (double) left.M21 + right.M33 * (double) left.M23 + left.M24 * (double) right.M43),
      M24 = (float) (right.M24 * (double) left.M22 + right.M14 * (double) left.M21 + right.M34 * (double) left.M23 + right.M44 * (double) left.M24),
      M31 = (float) (left.M32 * (double) right.M21 + left.M31 * (double) right.M11 + left.M33 * (double) right.M31 + left.M34 * (double) right.M41),
      M32 = (float) (left.M32 * (double) right.M22 + left.M31 * (double) right.M12 + left.M33 * (double) right.M32 + left.M34 * (double) right.M42),
      M33 = (float) (right.M23 * (double) left.M32 + left.M31 * (double) right.M13 + left.M33 * (double) right.M33 + left.M34 * (double) right.M43),
      M34 = (float) (right.M24 * (double) left.M32 + right.M14 * (double) left.M31 + right.M34 * (double) left.M33 + right.M44 * (double) left.M34),
      M41 = (float) (left.M42 * (double) right.M21 + left.M41 * (double) right.M11 + left.M43 * (double) right.M31 + left.M44 * (double) right.M41),
      M42 = (float) (left.M42 * (double) right.M22 + left.M41 * (double) right.M12 + left.M43 * (double) right.M32 + left.M44 * (double) right.M42),
      M43 = (float) (right.M23 * (double) left.M42 + left.M41 * (double) right.M13 + left.M43 * (double) right.M33 + left.M44 * (double) right.M43),
      M44 = (float) (right.M24 * (double) left.M42 + left.M41 * (double) right.M14 + right.M34 * (double) left.M43 + left.M44 * (double) right.M44)
    };
  }

  public static void Divide(ref Matrix left, float right, out Matrix result)
  {
    float num = 1f / right;
    result = new()
    {
      M11 = left.M11 * num,
      M12 = left.M12 * num,
      M13 = left.M13 * num,
      M14 = left.M14 * num,
      M21 = left.M21 * num,
      M22 = left.M22 * num,
      M23 = left.M23 * num,
      M24 = left.M24 * num,
      M31 = left.M31 * num,
      M32 = left.M32 * num,
      M33 = left.M33 * num,
      M34 = left.M34 * num,
      M41 = left.M41 * num,
      M42 = left.M42 * num,
      M43 = left.M43 * num,
      M44 = left.M44 * num
    };
  }

  public static Matrix Divide(Matrix left, float right)
  {
    Matrix matrix = new();
    float num = 1f / right;
    matrix.M11 = left.M11 * num;
    matrix.M12 = left.M12 * num;
    matrix.M13 = left.M13 * num;
    matrix.M14 = left.M14 * num;
    matrix.M21 = left.M21 * num;
    matrix.M22 = left.M22 * num;
    matrix.M23 = left.M23 * num;
    matrix.M24 = left.M24 * num;
    matrix.M31 = left.M31 * num;
    matrix.M32 = left.M32 * num;
    matrix.M33 = left.M33 * num;
    matrix.M34 = left.M34 * num;
    matrix.M41 = left.M41 * num;
    matrix.M42 = left.M42 * num;
    matrix.M43 = left.M43 * num;
    matrix.M44 = left.M44 * num;
    return matrix;
  }

  public static void Divide(ref Matrix left, ref Matrix right, out Matrix result)
  {
    result = new()
    {
      M11 = left.M11 / right.M11,
      M12 = left.M12 / right.M12,
      M13 = left.M13 / right.M13,
      M14 = left.M14 / right.M14,
      M21 = left.M21 / right.M21,
      M22 = left.M22 / right.M22,
      M23 = left.M23 / right.M23,
      M24 = left.M24 / right.M24,
      M31 = left.M31 / right.M31,
      M32 = left.M32 / right.M32,
      M33 = left.M33 / right.M33,
      M34 = left.M34 / right.M34,
      M41 = left.M41 / right.M41,
      M42 = left.M42 / right.M42,
      M43 = left.M43 / right.M43,
      M44 = left.M44 / right.M44
    };
  }

  public static Matrix Divide(Matrix left, Matrix right)
  {
    return new()
    {
      M11 = left.M11 / right.M11,
      M12 = left.M12 / right.M12,
      M13 = left.M13 / right.M13,
      M14 = left.M14 / right.M14,
      M21 = left.M21 / right.M21,
      M22 = left.M22 / right.M22,
      M23 = left.M23 / right.M23,
      M24 = left.M24 / right.M24,
      M31 = left.M31 / right.M31,
      M32 = left.M32 / right.M32,
      M33 = left.M33 / right.M33,
      M34 = left.M34 / right.M34,
      M41 = left.M41 / right.M41,
      M42 = left.M42 / right.M42,
      M43 = left.M43 / right.M43,
      M44 = left.M44 / right.M44
    };
  }

  public static void Negate(ref Matrix matrix, out Matrix result)
  {
    result = new()
    {
      M11 = -matrix.M11,
      M12 = -matrix.M12,
      M13 = -matrix.M13,
      M14 = -matrix.M14,
      M21 = -matrix.M21,
      M22 = -matrix.M22,
      M23 = -matrix.M23,
      M24 = -matrix.M24,
      M31 = -matrix.M31,
      M32 = -matrix.M32,
      M33 = -matrix.M33,
      M34 = -matrix.M34,
      M41 = -matrix.M41,
      M42 = -matrix.M42,
      M43 = -matrix.M43,
      M44 = -matrix.M44
    };
  }

  public static Matrix Negate(Matrix matrix)
  {
    return new()
    {
      M11 = -matrix.M11,
      M12 = -matrix.M12,
      M13 = -matrix.M13,
      M14 = -matrix.M14,
      M21 = -matrix.M21,
      M22 = -matrix.M22,
      M23 = -matrix.M23,
      M24 = -matrix.M24,
      M31 = -matrix.M31,
      M32 = -matrix.M32,
      M33 = -matrix.M33,
      M34 = -matrix.M34,
      M41 = -matrix.M41,
      M42 = -matrix.M42,
      M43 = -matrix.M43,
      M44 = -matrix.M44
    };
  }

  public static void Lerp(ref Matrix start, ref Matrix end, float amount, out Matrix result)
  {
    result = new()
    {
      M11 = (end.M11 - start.M11) * amount + start.M11,
      M12 = (end.M12 - start.M12) * amount + start.M12,
      M13 = (end.M13 - start.M13) * amount + start.M13,
      M14 = (end.M14 - start.M14) * amount + start.M14,
      M21 = (end.M21 - start.M21) * amount + start.M21,
      M22 = (end.M22 - start.M22) * amount + start.M22,
      M23 = (end.M23 - start.M23) * amount + start.M23,
      M24 = (end.M24 - start.M24) * amount + start.M24,
      M31 = (end.M31 - start.M31) * amount + start.M31,
      M32 = (end.M32 - start.M32) * amount + start.M32,
      M33 = (end.M33 - start.M33) * amount + start.M33,
      M34 = (end.M34 - start.M34) * amount + start.M34,
      M41 = (end.M41 - start.M41) * amount + start.M41,
      M42 = (end.M42 - start.M42) * amount + start.M42,
      M43 = (end.M43 - start.M43) * amount + start.M43,
      M44 = (end.M44 - start.M44) * amount + start.M44
    };
  }

  public static Matrix Lerp(Matrix start, Matrix end, float amount)
  {
    return new()
    {
      M11 = (end.M11 - start.M11) * amount + start.M11,
      M12 = (end.M12 - start.M12) * amount + start.M12,
      M13 = (end.M13 - start.M13) * amount + start.M13,
      M14 = (end.M14 - start.M14) * amount + start.M14,
      M21 = (end.M21 - start.M21) * amount + start.M21,
      M22 = (end.M22 - start.M22) * amount + start.M22,
      M23 = (end.M23 - start.M23) * amount + start.M23,
      M24 = (end.M24 - start.M24) * amount + start.M24,
      M31 = (end.M31 - start.M31) * amount + start.M31,
      M32 = (end.M32 - start.M32) * amount + start.M32,
      M33 = (end.M33 - start.M33) * amount + start.M33,
      M34 = (end.M34 - start.M34) * amount + start.M34,
      M41 = (end.M41 - start.M41) * amount + start.M41,
      M42 = (end.M42 - start.M42) * amount + start.M42,
      M43 = (end.M43 - start.M43) * amount + start.M43,
      M44 = (end.M44 - start.M44) * amount + start.M44
    };
  }

  public static void Billboard(
    ref Vector3 objectPosition,
    ref Vector3 cameraPosition,
    ref Vector3 cameraUpVector,
    ref Vector3 cameraForwardVector,
    out Matrix result)
  {
    Vector3 vector3_1 = objectPosition - cameraPosition;
    float d = vector3_1.LengthSquared();
    Vector3 vector3_2 = d >= 9.999999747378752E-05 ? vector3_1 * (float) (1.0 / Math.Sqrt(d)) : -cameraForwardVector;
    Vector3.Cross(ref cameraUpVector, ref vector3_2, out var result1);
    result1.Normalize();
    Vector3.Cross(ref vector3_2, ref result1, out var result2);
    result.M11 = result1.X;
    result.M12 = result1.Y;
    result.M13 = result1.Z;
    result.M14 = 0.0f;
    result.M21 = result2.X;
    result.M22 = result2.Y;
    result.M23 = result2.Z;
    result.M24 = 0.0f;
    result.M31 = vector3_2.X;
    result.M32 = vector3_2.Y;
    result.M33 = vector3_2.Z;
    result.M34 = 0.0f;
    result.M41 = objectPosition.X;
    result.M42 = objectPosition.Y;
    result.M43 = objectPosition.Z;
    result.M44 = 1f;
  }

  public static Matrix Billboard(
    Vector3 objectPosition,
    Vector3 cameraPosition,
    Vector3 cameraUpVector,
    Vector3 cameraForwardVector)
  {
    Matrix matrix = new();
    Vector3 vector3_1 = objectPosition - cameraPosition;
    float d = vector3_1.LengthSquared();
    Vector3 vector3_2 = d >= 9.999999747378752E-05 ? vector3_1 * (float) (1.0 / Math.Sqrt(d)) : -cameraForwardVector;
    Vector3.Cross(ref cameraUpVector, ref vector3_2, out var result1);
    result1.Normalize();
    Vector3.Cross(ref vector3_2, ref result1, out var result2);
    matrix.M11 = result1.X;
    matrix.M12 = result1.Y;
    matrix.M13 = result1.Z;
    matrix.M14 = 0.0f;
    matrix.M21 = result2.X;
    matrix.M22 = result2.Y;
    matrix.M23 = result2.Z;
    matrix.M24 = 0.0f;
    matrix.M31 = vector3_2.X;
    matrix.M32 = vector3_2.Y;
    matrix.M33 = vector3_2.Z;
    matrix.M34 = 0.0f;
    matrix.M41 = objectPosition.X;
    matrix.M42 = objectPosition.Y;
    matrix.M43 = objectPosition.Z;
    matrix.M44 = 1f;
    return matrix;
  }

  public static void RotationX(float angle, out Matrix result)
  {
    float num1 = (float) Math.Cos(angle);
    float num2 = (float) Math.Sin(angle);
    result.M11 = 1f;
    result.M12 = 0.0f;
    result.M13 = 0.0f;
    result.M14 = 0.0f;
    result.M21 = 0.0f;
    result.M22 = num1;
    result.M23 = num2;
    result.M24 = 0.0f;
    result.M31 = 0.0f;
    result.M32 = -num2;
    result.M33 = num1;
    result.M34 = 0.0f;
    result.M41 = 0.0f;
    result.M42 = 0.0f;
    result.M43 = 0.0f;
    result.M44 = 1f;
  }

  public static Matrix RotationX(float angle)
  {
    Matrix matrix = new();
    float num1 = (float) Math.Cos(angle);
    float num2 = (float) Math.Sin(angle);
    matrix.M11 = 1f;
    matrix.M12 = 0.0f;
    matrix.M13 = 0.0f;
    matrix.M14 = 0.0f;
    matrix.M21 = 0.0f;
    matrix.M22 = num1;
    matrix.M23 = num2;
    matrix.M24 = 0.0f;
    matrix.M31 = 0.0f;
    matrix.M32 = -num2;
    matrix.M33 = num1;
    matrix.M34 = 0.0f;
    matrix.M41 = 0.0f;
    matrix.M42 = 0.0f;
    matrix.M43 = 0.0f;
    matrix.M44 = 1f;
    return matrix;
  }

  public static void RotationY(float angle, out Matrix result)
  {
    float num1 = (float) Math.Cos(angle);
    float num2 = (float) Math.Sin(angle);
    result.M11 = num1;
    result.M12 = 0.0f;
    result.M13 = -num2;
    result.M14 = 0.0f;
    result.M21 = 0.0f;
    result.M22 = 1f;
    result.M23 = 0.0f;
    result.M24 = 0.0f;
    result.M31 = num2;
    result.M32 = 0.0f;
    result.M33 = num1;
    result.M34 = 0.0f;
    result.M41 = 0.0f;
    result.M42 = 0.0f;
    result.M43 = 0.0f;
    result.M44 = 1f;
  }

  public static Matrix RotationY(float angle)
  {
    Matrix matrix = new();
    float num1 = (float) Math.Cos(angle);
    float num2 = (float) Math.Sin(angle);
    matrix.M11 = num1;
    matrix.M12 = 0.0f;
    matrix.M13 = -num2;
    matrix.M14 = 0.0f;
    matrix.M21 = 0.0f;
    matrix.M22 = 1f;
    matrix.M23 = 0.0f;
    matrix.M24 = 0.0f;
    matrix.M31 = num2;
    matrix.M32 = 0.0f;
    matrix.M33 = num1;
    matrix.M34 = 0.0f;
    matrix.M41 = 0.0f;
    matrix.M42 = 0.0f;
    matrix.M43 = 0.0f;
    matrix.M44 = 1f;
    return matrix;
  }

  public static void RotationZ(float angle, out Matrix result)
  {
    float num1 = (float) Math.Cos(angle);
    float num2 = (float) Math.Sin(angle);
    result.M11 = num1;
    result.M12 = num2;
    result.M13 = 0.0f;
    result.M14 = 0.0f;
    result.M21 = -num2;
    result.M22 = num1;
    result.M23 = 0.0f;
    result.M24 = 0.0f;
    result.M31 = 0.0f;
    result.M32 = 0.0f;
    result.M33 = 1f;
    result.M34 = 0.0f;
    result.M41 = 0.0f;
    result.M42 = 0.0f;
    result.M43 = 0.0f;
    result.M44 = 1f;
  }

  public static Matrix RotationZ(float angle)
  {
    Matrix matrix = new();
    float num1 = (float) Math.Cos(angle);
    float num2 = (float) Math.Sin(angle);
    matrix.M11 = num1;
    matrix.M12 = num2;
    matrix.M13 = 0.0f;
    matrix.M14 = 0.0f;
    matrix.M21 = -num2;
    matrix.M22 = num1;
    matrix.M23 = 0.0f;
    matrix.M24 = 0.0f;
    matrix.M31 = 0.0f;
    matrix.M32 = 0.0f;
    matrix.M33 = 1f;
    matrix.M34 = 0.0f;
    matrix.M41 = 0.0f;
    matrix.M42 = 0.0f;
    matrix.M43 = 0.0f;
    matrix.M44 = 1f;
    return matrix;
  }

  public static void RotationAxis(ref Vector3 axis, float angle, out Matrix result)
  {
    if (axis.LengthSquared() != 1.0)
      axis.Normalize();
    float x = axis.X;
    float y = axis.Y;
    float z = axis.Z;
    float num1 = (float) Math.Cos(angle);
    float num2 = (float) Math.Sin(angle);
    double num3 = x;
    float num4 = (float) (num3 * num3);
    double num5 = y;
    float num6 = (float) (num5 * num5);
    double num7 = z;
    float num8 = (float) (num7 * num7);
    float num9 = y * x;
    float num10 = z * x;
    float num11 = z * y;
    result.M11 = (1f - num4) * num1 + num4;
    double num12 = num9;
    double num13 = num12 - num1 * num12;
    double num14 = num2 * (double) z;
    result.M12 = (float) (num14 + num13);
    double num15 = num10;
    double num16 = num15 - num1 * num15;
    double num17 = num2 * (double) y;
    result.M13 = (float) (num16 - num17);
    result.M14 = 0.0f;
    result.M21 = (float) (num13 - num14);
    result.M22 = (1f - num6) * num1 + num6;
    double num18 = num11;
    double num19 = num18 - num1 * num18;
    double num20 = num2 * (double) x;
    result.M23 = (float) (num20 + num19);
    result.M24 = 0.0f;
    result.M31 = (float) (num17 + num16);
    result.M32 = (float) (num19 - num20);
    result.M33 = (1f - num8) * num1 + num8;
    result.M34 = 0.0f;
    result.M41 = 0.0f;
    result.M42 = 0.0f;
    result.M43 = 0.0f;
    result.M44 = 1f;
  }

  public static Matrix RotationAxis(Vector3 axis, float angle)
  {
    if (axis.LengthSquared() != 1.0)
      axis.Normalize();
    Matrix matrix = new();
    float x = axis.X;
    float y = axis.Y;
    float z = axis.Z;
    float num1 = (float) Math.Cos(angle);
    float num2 = (float) Math.Sin(angle);
    double num3 = x;
    float num4 = (float) (num3 * num3);
    double num5 = y;
    float num6 = (float) (num5 * num5);
    double num7 = z;
    float num8 = (float) (num7 * num7);
    float num9 = y * x;
    float num10 = z * x;
    float num11 = z * y;
    matrix.M11 = (1f - num4) * num1 + num4;
    double num12 = num9;
    double num13 = num12 - num1 * num12;
    double num14 = num2 * (double) z;
    matrix.M12 = (float) (num14 + num13);
    double num15 = num10;
    double num16 = num15 - num1 * num15;
    double num17 = num2 * (double) y;
    matrix.M13 = (float) (num16 - num17);
    matrix.M14 = 0.0f;
    matrix.M21 = (float) (num13 - num14);
    matrix.M22 = (1f - num6) * num1 + num6;
    double num18 = num11;
    double num19 = num18 - num1 * num18;
    double num20 = num2 * (double) x;
    matrix.M23 = (float) (num20 + num19);
    matrix.M24 = 0.0f;
    matrix.M31 = (float) (num17 + num16);
    matrix.M32 = (float) (num19 - num20);
    matrix.M33 = (1f - num8) * num1 + num8;
    matrix.M34 = 0.0f;
    matrix.M41 = 0.0f;
    matrix.M42 = 0.0f;
    matrix.M43 = 0.0f;
    matrix.M44 = 1f;
    return matrix;
  }

  public static void RotationQuaternion(ref Quaternion rotation, out Matrix result)
  {
    double x = rotation.X;
    float num1 = (float) (x * x);
    double y = rotation.Y;
    float num2 = (float) (y * y);
    double z = rotation.Z;
    float num3 = (float) (z * z);
    float num4 = rotation.Y * rotation.X;
    float num5 = rotation.W * rotation.Z;
    float num6 = rotation.Z * rotation.X;
    float num7 = rotation.W * rotation.Y;
    float num8 = rotation.Z * rotation.Y;
    float num9 = rotation.W * rotation.X;
    result.M11 = (float) (1.0 - (num3 + (double) num2) * 2.0);
    result.M12 = (float) ((num5 + (double) num4) * 2.0);
    result.M13 = (float) ((num6 - (double) num7) * 2.0);
    result.M14 = 0.0f;
    result.M21 = (float) ((num4 - (double) num5) * 2.0);
    result.M22 = (float) (1.0 - (num3 + (double) num1) * 2.0);
    result.M23 = (float) ((num9 + (double) num8) * 2.0);
    result.M24 = 0.0f;
    result.M31 = (float) ((num7 + (double) num6) * 2.0);
    result.M32 = (float) ((num8 - (double) num9) * 2.0);
    result.M33 = (float) (1.0 - (num2 + (double) num1) * 2.0);
    result.M34 = 0.0f;
    result.M41 = 0.0f;
    result.M42 = 0.0f;
    result.M43 = 0.0f;
    result.M44 = 1f;
  }

  public static Matrix RotationQuaternion(Quaternion rotation)
  {
    Matrix matrix = new();
    double x = rotation.X;
    float num1 = (float) (x * x);
    double y = rotation.Y;
    float num2 = (float) (y * y);
    double z = rotation.Z;
    float num3 = (float) (z * z);
    float num4 = rotation.Y * rotation.X;
    float num5 = rotation.W * rotation.Z;
    float num6 = rotation.Z * rotation.X;
    float num7 = rotation.W * rotation.Y;
    float num8 = rotation.Z * rotation.Y;
    float num9 = rotation.W * rotation.X;
    matrix.M11 = (float) (1.0 - (num3 + (double) num2) * 2.0);
    matrix.M12 = (float) ((num5 + (double) num4) * 2.0);
    matrix.M13 = (float) ((num6 - (double) num7) * 2.0);
    matrix.M14 = 0.0f;
    matrix.M21 = (float) ((num4 - (double) num5) * 2.0);
    matrix.M22 = (float) (1.0 - (num3 + (double) num1) * 2.0);
    matrix.M23 = (float) ((num9 + (double) num8) * 2.0);
    matrix.M24 = 0.0f;
    matrix.M31 = (float) ((num7 + (double) num6) * 2.0);
    matrix.M32 = (float) ((num8 - (double) num9) * 2.0);
    matrix.M33 = (float) (1.0 - (num2 + (double) num1) * 2.0);
    matrix.M34 = 0.0f;
    matrix.M41 = 0.0f;
    matrix.M42 = 0.0f;
    matrix.M43 = 0.0f;
    matrix.M44 = 1f;
    return matrix;
  }

  public static void RotationYawPitchRoll(float yaw, float pitch, float roll, out Matrix result)
  {
      Quaternion.RotationYawPitchRoll(yaw, pitch, roll, out var result1);
    RotationQuaternion(ref result1, out result);
  }

  public static Matrix RotationYawPitchRoll(float yaw, float pitch, float roll)
  {
      Quaternion.RotationYawPitchRoll(yaw, pitch, roll, out var result2);
    RotationQuaternion(ref result2, out var result1);
    return result1;
  }

  public static void Reflection(ref Plane plane, out Matrix result)
  {
    plane.Normalize();
    float x = plane.Normal.X;
    float y = plane.Normal.Y;
    float z = plane.Normal.Z;
    float num1 = x * -2f;
    float num2 = y * -2f;
    float num3 = z * -2f;
    result.M11 = (float) (num1 * (double) x + 1.0);
    result.M12 = num2 * x;
    result.M13 = num3 * x;
    result.M14 = 0.0f;
    result.M21 = num1 * y;
    result.M22 = (float) (num2 * (double) y + 1.0);
    result.M23 = num3 * y;
    result.M24 = 0.0f;
    result.M31 = num1 * z;
    result.M32 = num2 * z;
    result.M33 = (float) (num3 * (double) z + 1.0);
    result.M34 = 0.0f;
    result.M41 = plane.D * num1;
    result.M42 = plane.D * num2;
    result.M43 = plane.D * num3;
    result.M44 = 1f;
  }

  public static Matrix Reflection(Plane plane)
  {
    Matrix matrix = new();
    plane.Normalize();
    float x = plane.Normal.X;
    float y = plane.Normal.Y;
    float z = plane.Normal.Z;
    float num1 = x * -2f;
    float num2 = y * -2f;
    float num3 = z * -2f;
    matrix.M11 = (float) (num1 * (double) x + 1.0);
    matrix.M12 = num2 * x;
    matrix.M13 = num3 * x;
    matrix.M14 = 0.0f;
    matrix.M21 = num1 * y;
    matrix.M22 = (float) (num2 * (double) y + 1.0);
    matrix.M23 = num3 * y;
    matrix.M24 = 0.0f;
    matrix.M31 = num1 * z;
    matrix.M32 = num2 * z;
    matrix.M33 = (float) (num3 * (double) z + 1.0);
    matrix.M34 = 0.0f;
    matrix.M41 = plane.D * num1;
    matrix.M42 = plane.D * num2;
    matrix.M43 = plane.D * num3;
    matrix.M44 = 1f;
    return matrix;
  }

  public static void Scaling(ref Vector3 scale, out Matrix result)
  {
    result.M11 = scale.X;
    result.M12 = 0.0f;
    result.M13 = 0.0f;
    result.M14 = 0.0f;
    result.M21 = 0.0f;
    result.M22 = scale.Y;
    result.M23 = 0.0f;
    result.M24 = 0.0f;
    result.M31 = 0.0f;
    result.M32 = 0.0f;
    result.M33 = scale.Z;
    result.M34 = 0.0f;
    result.M41 = 0.0f;
    result.M42 = 0.0f;
    result.M43 = 0.0f;
    result.M44 = 1f;
  }

  public static Matrix Scaling(Vector3 scale)
  {
    return new()
    {
      M11 = scale.X,
      M12 = 0.0f,
      M13 = 0.0f,
      M14 = 0.0f,
      M21 = 0.0f,
      M22 = scale.Y,
      M23 = 0.0f,
      M24 = 0.0f,
      M31 = 0.0f,
      M32 = 0.0f,
      M33 = scale.Z,
      M34 = 0.0f,
      M41 = 0.0f,
      M42 = 0.0f,
      M43 = 0.0f,
      M44 = 1f
    };
  }

  public static void Scaling(float x, float y, float z, out Matrix result)
  {
    result.M11 = x;
    result.M12 = 0.0f;
    result.M13 = 0.0f;
    result.M14 = 0.0f;
    result.M21 = 0.0f;
    result.M22 = y;
    result.M23 = 0.0f;
    result.M24 = 0.0f;
    result.M31 = 0.0f;
    result.M32 = 0.0f;
    result.M33 = z;
    result.M34 = 0.0f;
    result.M41 = 0.0f;
    result.M42 = 0.0f;
    result.M43 = 0.0f;
    result.M44 = 1f;
  }

  public static Matrix Scaling(float x, float y, float z)
  {
    return new()
    {
      M11 = x,
      M12 = 0.0f,
      M13 = 0.0f,
      M14 = 0.0f,
      M21 = 0.0f,
      M22 = y,
      M23 = 0.0f,
      M24 = 0.0f,
      M31 = 0.0f,
      M32 = 0.0f,
      M33 = z,
      M34 = 0.0f,
      M41 = 0.0f,
      M42 = 0.0f,
      M43 = 0.0f,
      M44 = 1f
    };
  }

  public static void Shadow(ref Vector4 light, ref Plane plane, out Matrix result)
  {
    plane.Normalize();
    float num1 = (float) (plane.Normal.Y * (double) light.Y + plane.Normal.X * (double) light.X + plane.Normal.Z * (double) light.Z);
    float num2 = -plane.Normal.X;
    float num3 = -plane.Normal.Y;
    float num4 = -plane.Normal.Z;
    float num5 = -plane.D;
    result.M11 = light.X * num2 + num1;
    result.M21 = light.X * num3;
    result.M31 = light.X * num4;
    result.M41 = light.X * num5;
    result.M12 = light.Y * num2;
    result.M22 = light.Y * num3 + num1;
    result.M32 = light.Y * num4;
    result.M42 = light.Y * num5;
    result.M13 = light.Z * num2;
    result.M23 = light.Z * num3;
    result.M33 = light.Z * num4 + num1;
    result.M43 = light.Z * num5;
    result.M14 = 0.0f;
    result.M24 = 0.0f;
    result.M34 = 0.0f;
    result.M44 = num1;
  }

  public static Matrix Shadow(Vector4 light, Plane plane)
  {
    Matrix matrix = new();
    plane.Normalize();
    float num1 = (float) (plane.Normal.Y * (double) light.Y + plane.Normal.X * (double) light.X + plane.Normal.Z * (double) light.Z);
    float num2 = -plane.Normal.X;
    float num3 = -plane.Normal.Y;
    float num4 = -plane.Normal.Z;
    float num5 = -plane.D;
    matrix.M11 = light.X * num2 + num1;
    matrix.M21 = light.X * num3;
    matrix.M31 = light.X * num4;
    matrix.M41 = light.X * num5;
    matrix.M12 = light.Y * num2;
    matrix.M22 = light.Y * num3 + num1;
    matrix.M32 = light.Y * num4;
    matrix.M42 = light.Y * num5;
    matrix.M13 = light.Z * num2;
    matrix.M23 = light.Z * num3;
    matrix.M33 = light.Z * num4 + num1;
    matrix.M43 = light.Z * num5;
    matrix.M14 = 0.0f;
    matrix.M24 = 0.0f;
    matrix.M34 = 0.0f;
    matrix.M44 = num1;
    return matrix;
  }

  public static void Translation(ref Vector3 amount, out Matrix result)
  {
    result.M11 = 1f;
    result.M12 = 0.0f;
    result.M13 = 0.0f;
    result.M14 = 0.0f;
    result.M21 = 0.0f;
    result.M22 = 1f;
    result.M23 = 0.0f;
    result.M24 = 0.0f;
    result.M31 = 0.0f;
    result.M32 = 0.0f;
    result.M33 = 1f;
    result.M34 = 0.0f;
    result.M41 = amount.X;
    result.M42 = amount.Y;
    result.M43 = amount.Z;
    result.M44 = 1f;
  }

  public static Matrix Translation(Vector3 amount)
  {
    return new()
    {
      M11 = 1f,
      M12 = 0.0f,
      M13 = 0.0f,
      M14 = 0.0f,
      M21 = 0.0f,
      M22 = 1f,
      M23 = 0.0f,
      M24 = 0.0f,
      M31 = 0.0f,
      M32 = 0.0f,
      M33 = 1f,
      M34 = 0.0f,
      M41 = amount.X,
      M42 = amount.Y,
      M43 = amount.Z,
      M44 = 1f
    };
  }

  public static void Translation(float x, float y, float z, out Matrix result)
  {
    result.M11 = 1f;
    result.M12 = 0.0f;
    result.M13 = 0.0f;
    result.M14 = 0.0f;
    result.M21 = 0.0f;
    result.M22 = 1f;
    result.M23 = 0.0f;
    result.M24 = 0.0f;
    result.M31 = 0.0f;
    result.M32 = 0.0f;
    result.M33 = 1f;
    result.M34 = 0.0f;
    result.M41 = x;
    result.M42 = y;
    result.M43 = z;
    result.M44 = 1f;
  }

  public static Matrix Translation(float x, float y, float z)
  {
    return new()
    {
      M11 = 1f,
      M12 = 0.0f,
      M13 = 0.0f,
      M14 = 0.0f,
      M21 = 0.0f,
      M22 = 1f,
      M23 = 0.0f,
      M24 = 0.0f,
      M31 = 0.0f,
      M32 = 0.0f,
      M33 = 1f,
      M34 = 0.0f,
      M41 = x,
      M42 = y,
      M43 = z,
      M44 = 1f
    };
  }

  public static void Transpose(ref Matrix matrix, out Matrix result)
  {
    result = new()
    {
      M11 = matrix.M11,
      M12 = matrix.M21,
      M13 = matrix.M31,
      M14 = matrix.M41,
      M21 = matrix.M12,
      M22 = matrix.M22,
      M23 = matrix.M32,
      M24 = matrix.M42,
      M31 = matrix.M13,
      M32 = matrix.M23,
      M33 = matrix.M33,
      M34 = matrix.M43,
      M41 = matrix.M14,
      M42 = matrix.M24,
      M43 = matrix.M34,
      M44 = matrix.M44
    };
  }

  public static Matrix Transpose(Matrix matrix)
  {
    return new()
    {
      M11 = matrix.M11,
      M12 = matrix.M21,
      M13 = matrix.M31,
      M14 = matrix.M41,
      M21 = matrix.M12,
      M22 = matrix.M22,
      M23 = matrix.M32,
      M24 = matrix.M42,
      M31 = matrix.M13,
      M32 = matrix.M23,
      M33 = matrix.M33,
      M34 = matrix.M43,
      M41 = matrix.M14,
      M42 = matrix.M24,
      M43 = matrix.M34,
      M44 = matrix.M44
    };
  }

  public static Matrix operator -(Matrix left, Matrix right)
  {
    return new()
    {
      M11 = left.M11 - right.M11,
      M12 = left.M12 - right.M12,
      M13 = left.M13 - right.M13,
      M14 = left.M14 - right.M14,
      M21 = left.M21 - right.M21,
      M22 = left.M22 - right.M22,
      M23 = left.M23 - right.M23,
      M24 = left.M24 - right.M24,
      M31 = left.M31 - right.M31,
      M32 = left.M32 - right.M32,
      M33 = left.M33 - right.M33,
      M34 = left.M34 - right.M34,
      M41 = left.M41 - right.M41,
      M42 = left.M42 - right.M42,
      M43 = left.M43 - right.M43,
      M44 = left.M44 - right.M44
    };
  }

  public static Matrix operator -(Matrix matrix)
  {
    return new()
    {
      M11 = -matrix.M11,
      M12 = -matrix.M12,
      M13 = -matrix.M13,
      M14 = -matrix.M14,
      M21 = -matrix.M21,
      M22 = -matrix.M22,
      M23 = -matrix.M23,
      M24 = -matrix.M24,
      M31 = -matrix.M31,
      M32 = -matrix.M32,
      M33 = -matrix.M33,
      M34 = -matrix.M34,
      M41 = -matrix.M41,
      M42 = -matrix.M42,
      M43 = -matrix.M43,
      M44 = -matrix.M44
    };
  }

  public static Matrix operator +(Matrix left, Matrix right)
  {
    return new()
    {
      M11 = left.M11 + right.M11,
      M12 = left.M12 + right.M12,
      M13 = left.M13 + right.M13,
      M14 = left.M14 + right.M14,
      M21 = left.M21 + right.M21,
      M22 = left.M22 + right.M22,
      M23 = left.M23 + right.M23,
      M24 = left.M24 + right.M24,
      M31 = left.M31 + right.M31,
      M32 = left.M32 + right.M32,
      M33 = left.M33 + right.M33,
      M34 = left.M34 + right.M34,
      M41 = left.M41 + right.M41,
      M42 = left.M42 + right.M42,
      M43 = left.M43 + right.M43,
      M44 = left.M44 + right.M44
    };
  }

  public static Matrix operator /(Matrix left, float right)
  {
    return new()
    {
      M11 = left.M11 / right,
      M12 = left.M12 / right,
      M13 = left.M13 / right,
      M14 = left.M14 / right,
      M21 = left.M21 / right,
      M22 = left.M22 / right,
      M23 = left.M23 / right,
      M24 = left.M24 / right,
      M31 = left.M31 / right,
      M32 = left.M32 / right,
      M33 = left.M33 / right,
      M34 = left.M34 / right,
      M41 = left.M41 / right,
      M42 = left.M42 / right,
      M43 = left.M43 / right,
      M44 = left.M44 / right
    };
  }

  public static Matrix operator /(Matrix left, Matrix right)
  {
    return new()
    {
      M11 = left.M11 / right.M11,
      M12 = left.M12 / right.M12,
      M13 = left.M13 / right.M13,
      M14 = left.M14 / right.M14,
      M21 = left.M21 / right.M21,
      M22 = left.M22 / right.M22,
      M23 = left.M23 / right.M23,
      M24 = left.M24 / right.M24,
      M31 = left.M31 / right.M31,
      M32 = left.M32 / right.M32,
      M33 = left.M33 / right.M33,
      M34 = left.M34 / right.M34,
      M41 = left.M41 / right.M41,
      M42 = left.M42 / right.M42,
      M43 = left.M43 / right.M43,
      M44 = left.M44 / right.M44
    };
  }

  public static Matrix operator *(float left, Matrix right) => right * left;

  public static Matrix operator *(Matrix left, float right)
  {
    return new()
    {
      M11 = left.M11 * right,
      M12 = left.M12 * right,
      M13 = left.M13 * right,
      M14 = left.M14 * right,
      M21 = left.M21 * right,
      M22 = left.M22 * right,
      M23 = left.M23 * right,
      M24 = left.M24 * right,
      M31 = left.M31 * right,
      M32 = left.M32 * right,
      M33 = left.M33 * right,
      M34 = left.M34 * right,
      M41 = left.M41 * right,
      M42 = left.M42 * right,
      M43 = left.M43 * right,
      M44 = left.M44 * right
    };
  }

  public static Matrix operator *(Matrix left, Matrix right)
  {
    return new()
    {
      M11 = (float) (right.M21 * (double) left.M12 + left.M11 * (double) right.M11 + right.M31 * (double) left.M13 + right.M41 * (double) left.M14),
      M12 = (float) (right.M22 * (double) left.M12 + right.M12 * (double) left.M11 + right.M32 * (double) left.M13 + right.M42 * (double) left.M14),
      M13 = (float) (right.M23 * (double) left.M12 + right.M13 * (double) left.M11 + right.M33 * (double) left.M13 + right.M43 * (double) left.M14),
      M14 = (float) (right.M24 * (double) left.M12 + right.M14 * (double) left.M11 + right.M34 * (double) left.M13 + right.M44 * (double) left.M14),
      M21 = (float) (left.M22 * (double) right.M21 + left.M21 * (double) right.M11 + left.M23 * (double) right.M31 + left.M24 * (double) right.M41),
      M22 = (float) (left.M22 * (double) right.M22 + left.M21 * (double) right.M12 + left.M23 * (double) right.M32 + left.M24 * (double) right.M42),
      M23 = (float) (right.M23 * (double) left.M22 + right.M13 * (double) left.M21 + right.M33 * (double) left.M23 + left.M24 * (double) right.M43),
      M24 = (float) (right.M24 * (double) left.M22 + right.M14 * (double) left.M21 + right.M34 * (double) left.M23 + right.M44 * (double) left.M24),
      M31 = (float) (left.M32 * (double) right.M21 + left.M31 * (double) right.M11 + left.M33 * (double) right.M31 + left.M34 * (double) right.M41),
      M32 = (float) (left.M32 * (double) right.M22 + left.M31 * (double) right.M12 + left.M33 * (double) right.M32 + left.M34 * (double) right.M42),
      M33 = (float) (right.M23 * (double) left.M32 + left.M31 * (double) right.M13 + left.M33 * (double) right.M33 + left.M34 * (double) right.M43),
      M34 = (float) (right.M24 * (double) left.M32 + right.M14 * (double) left.M31 + right.M34 * (double) left.M33 + right.M44 * (double) left.M34),
      M41 = (float) (left.M42 * (double) right.M21 + left.M41 * (double) right.M11 + left.M43 * (double) right.M31 + left.M44 * (double) right.M41),
      M42 = (float) (left.M42 * (double) right.M22 + left.M41 * (double) right.M12 + left.M43 * (double) right.M32 + left.M44 * (double) right.M42),
      M43 = (float) (right.M23 * (double) left.M42 + left.M41 * (double) right.M13 + left.M43 * (double) right.M33 + left.M44 * (double) right.M43),
      M44 = (float) (right.M24 * (double) left.M42 + left.M41 * (double) right.M14 + right.M34 * (double) left.M43 + left.M44 * (double) right.M44)
    };
  }

  public static bool operator ==(Matrix left, Matrix right) => Equals(ref left, ref right);

  public static bool operator !=(Matrix left, Matrix right) => !Equals(ref left, ref right);

  public override string ToString()
  {
    object[] objArray = new object[16 /*0x10*/];
    float m11 = M11;
    objArray[0] = m11.ToString(CultureInfo.CurrentCulture);
    float m12 = M12;
    objArray[1] = m12.ToString(CultureInfo.CurrentCulture);
    float m13 = M13;
    objArray[2] = m13.ToString(CultureInfo.CurrentCulture);
    float m14 = M14;
    objArray[3] = m14.ToString(CultureInfo.CurrentCulture);
    float m21 = M21;
    objArray[4] = m21.ToString(CultureInfo.CurrentCulture);
    float m22 = M22;
    objArray[5] = m22.ToString(CultureInfo.CurrentCulture);
    float m23 = M23;
    objArray[6] = m23.ToString(CultureInfo.CurrentCulture);
    float m24 = M24;
    objArray[7] = m24.ToString(CultureInfo.CurrentCulture);
    float m31 = M31;
    objArray[8] = m31.ToString(CultureInfo.CurrentCulture);
    float m32 = M32;
    objArray[9] = m32.ToString(CultureInfo.CurrentCulture);
    float m33 = M33;
    objArray[10] = m33.ToString(CultureInfo.CurrentCulture);
    float m34 = M34;
    objArray[11] = m34.ToString(CultureInfo.CurrentCulture);
    float m41 = M41;
    objArray[12] = m41.ToString(CultureInfo.CurrentCulture);
    float m42 = M42;
    objArray[13] = m42.ToString(CultureInfo.CurrentCulture);
    float m43 = M43;
    objArray[14] = m43.ToString(CultureInfo.CurrentCulture);
    float m44 = M44;
    objArray[15] = m44.ToString(CultureInfo.CurrentCulture);
    return string.Format(CultureInfo.CurrentCulture, "[[M11:{0} M12:{1} M13:{2} M14:{3}] [M21:{4} M22:{5} M23:{6} M24:{7}] [M31:{8} M32:{9} M33:{10} M34:{11}] [M41:{12} M42:{13} M43:{14} M44:{15}]]", objArray);
  }

  public override int GetHashCode()
  {
    float m11 = M11;
    float m12 = M12;
    float m13 = M13;
    float m14 = M14;
    float m21 = M21;
    float m22 = M22;
    float m23 = M23;
    float m24 = M24;
    float m31 = M31;
    float m32 = M32;
    float m33 = M33;
    float m34 = M34;
    float m41 = M41;
    float m42 = M42;
    float m43 = M43;
    float m44 = M44;
    int num = m43.GetHashCode() + m44.GetHashCode() + m42.GetHashCode() + m41.GetHashCode() + m34.GetHashCode() + m33.GetHashCode() + m32.GetHashCode() + m31.GetHashCode() + m24.GetHashCode() + m23.GetHashCode() + m22.GetHashCode() + m21.GetHashCode() + m14.GetHashCode() + m13.GetHashCode() + m12.GetHashCode();
    return m11.GetHashCode() + num;
  }

  public static bool Equals(ref Matrix value1, ref Matrix value2)
  {
    return value1.M11 == (double) value2.M11 && value1.M12 == (double) value2.M12 && value1.M13 == (double) value2.M13 && value1.M14 == (double) value2.M14 && value1.M21 == (double) value2.M21 && value1.M22 == (double) value2.M22 && value1.M23 == (double) value2.M23 && value1.M24 == (double) value2.M24 && value1.M31 == (double) value2.M31 && value1.M32 == (double) value2.M32 && value1.M33 == (double) value2.M33 && value1.M34 == (double) value2.M34 && value1.M41 == (double) value2.M41 && value1.M42 == (double) value2.M42 && value1.M43 == (double) value2.M43 && value1.M44 == (double) value2.M44;
  }

  public bool Equals(Matrix other)
  {
    return M11 == (double) other.M11 && M12 == (double) other.M12 && M13 == (double) other.M13 && M14 == (double) other.M14 && M21 == (double) other.M21 && M22 == (double) other.M22 && M23 == (double) other.M23 && M24 == (double) other.M24 && M31 == (double) other.M31 && M32 == (double) other.M32 && M33 == (double) other.M33 && M34 == (double) other.M34 && M41 == (double) other.M41 && M42 == (double) other.M42 && M43 == (double) other.M43 && M44 == (double) other.M44;
  }

  public override bool Equals(object obj)
  {
    return obj != null && !(obj.GetType() != GetType()) && Equals((Matrix) obj);
  }
}
