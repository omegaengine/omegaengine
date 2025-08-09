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

using System.Drawing;
using System.Globalization;

// ReSharper disable once CheckNamespace
namespace SlimDX;

public struct Color4(float alpha, float red, float green, float blue)
{
  public float Red = red;
  public float Green = green;
  public float Blue = blue;
  public float Alpha = alpha;

  public Color4(int argb)
      : this((argb >> 24 & byte.MaxValue) / (float) byte.MaxValue, (argb >> 16 & byte.MaxValue) / (float) byte.MaxValue, (argb >> 8 & byte.MaxValue) / (float) byte.MaxValue, (argb & byte.MaxValue) / (float) byte.MaxValue)
  {}

  public Color4(Vector4 color)
      : this(color.W, color.X, color.Y, color.Z)
  {}

  public Color4(Vector3 color)
      : this(1f, color.X, color.Y, color.Z)
  {}

  public Color4(Color3 color)
      : this(1f, color.Red, color.Green, color.Blue)
  {}

  public Color4(Color color)
      : this(color.A / (float) byte.MaxValue, color.R / (float) byte.MaxValue, color.G / (float) byte.MaxValue, color.B / (float) byte.MaxValue)
  {}

  public Color4(float red, float green, float blue)
      : this(1f, red, green, blue)
  {}

  public Color3 ToColor3() => new(Red, Green, Blue);

  public Color ToColor()
  {
    return Color.FromArgb((int) (Alpha * (double) byte.MaxValue), (int) (Red * (double) byte.MaxValue), (int) (Green * (double) byte.MaxValue), (int) (Blue * (double) byte.MaxValue));
  }

  public int ToArgb()
  {
    return (((int) (uint) (Alpha * (double) byte.MaxValue) * 256 /*0x0100*/ + (int) (uint) (Red * (double) byte.MaxValue)) * 256 /*0x0100*/ + (int) (uint) (Green * (double) byte.MaxValue)) * 256 /*0x0100*/ + (int) (uint) (Blue * (double) byte.MaxValue);
  }

  public Vector3 ToVector3() => new(Red, Green, Blue);

  public Vector4 ToVector4() => new(Red, Green, Blue, Alpha);

  public static void Add(ref Color4 color1, ref Color4 color2, out Color4 result)
  {
    Color4 color4;
    color4.Alpha = color1.Alpha + color2.Alpha;
    color4.Red = color1.Red + color2.Red;
    color4.Green = color1.Green + color2.Green;
    color4.Blue = color1.Blue + color2.Blue;
    result = color4;
  }

  public static Color4 Add(Color4 color1, Color4 color2)
  {
    Color4 color4;
    color4.Alpha = color1.Alpha + color2.Alpha;
    color4.Red = color1.Red + color2.Red;
    color4.Green = color1.Green + color2.Green;
    color4.Blue = color1.Blue + color2.Blue;
    return color4;
  }

  public static void Subtract(ref Color4 color1, ref Color4 color2, out Color4 result)
  {
    Color4 color4;
    color4.Alpha = color1.Alpha - color2.Alpha;
    color4.Red = color1.Red - color2.Red;
    color4.Green = color1.Green - color2.Green;
    color4.Blue = color1.Blue - color2.Blue;
    result = color4;
  }

  public static Color4 Subtract(Color4 color1, Color4 color2)
  {
    Color4 color4;
    color4.Alpha = color1.Alpha - color2.Alpha;
    color4.Red = color1.Red - color2.Red;
    color4.Green = color1.Green - color2.Green;
    color4.Blue = color1.Blue - color2.Blue;
    return color4;
  }

  public static void Modulate(ref Color4 color1, ref Color4 color2, out Color4 result)
  {
    Color4 color4;
    color4.Alpha = color1.Alpha * color2.Alpha;
    color4.Red = color1.Red * color2.Red;
    color4.Green = color1.Green * color2.Green;
    color4.Blue = color1.Blue * color2.Blue;
    result = color4;
  }

  public static Color4 Modulate(Color4 color1, Color4 color2)
  {
    Color4 color4;
    color4.Alpha = color1.Alpha * color2.Alpha;
    color4.Red = color1.Red * color2.Red;
    color4.Green = color1.Green * color2.Green;
    color4.Blue = color1.Blue * color2.Blue;
    return color4;
  }

  public static void Lerp(ref Color4 color1, ref Color4 color2, float amount, out Color4 result)
  {
    Color4 color4;
    color4.Alpha = (color2.Alpha - color1.Alpha) * amount + color1.Alpha;
    color4.Red = (color2.Red - color1.Red) * amount + color1.Red;
    color4.Green = (color2.Green - color1.Green) * amount + color1.Green;
    color4.Blue = (color2.Blue - color1.Blue) * amount + color1.Blue;
    result = color4;
  }

  public static Color4 Lerp(Color4 color1, Color4 color2, float amount)
  {
    Color4 color4;
    color4.Alpha = (color2.Alpha - color1.Alpha) * amount + color1.Alpha;
    color4.Red = (color2.Red - color1.Red) * amount + color1.Red;
    color4.Green = (color2.Green - color1.Green) * amount + color1.Green;
    color4.Blue = (color2.Blue - color1.Blue) * amount + color1.Blue;
    return color4;
  }

  public static void Negate(ref Color4 color, out Color4 result)
  {
    Color4 color4;
    color4.Alpha = 1f - color.Alpha;
    color4.Red = 1f - color.Red;
    color4.Green = 1f - color.Green;
    color4.Blue = 1f - color.Blue;
    result = color4;
  }

  public static Color4 Negate(Color4 color)
  {
    Color4 color4;
    color4.Alpha = 1f - color.Alpha;
    color4.Red = 1f - color.Red;
    color4.Green = 1f - color.Green;
    color4.Blue = 1f - color.Blue;
    return color4;
  }

  public static void Scale(ref Color4 color, float scale, out Color4 result)
  {
    float alpha = color.Alpha;
    Color4 color4;
    color4.Alpha = alpha;
    color4.Red = color.Red * scale;
    color4.Green = color.Green * scale;
    color4.Blue = color.Blue * scale;
    result = color4;
  }

  public static Color4 Scale(Color4 color, float scale)
  {
    float alpha = color.Alpha;
    Color4 color4;
    color4.Alpha = alpha;
    color4.Red = color.Red * scale;
    color4.Green = color.Green * scale;
    color4.Blue = color.Blue * scale;
    return color4;
  }

  public static void AdjustContrast(ref Color4 color, float contrast, out Color4 result)
  {
    float alpha = color.Alpha;
    Color4 color4;
    color4.Alpha = alpha;
    color4.Red = (float) ((color.Red - 0.5) * contrast + 0.5);
    color4.Green = (float) ((color.Green - 0.5) * contrast + 0.5);
    color4.Blue = (float) ((color.Blue - 0.5) * contrast + 0.5);
    result = color4;
  }

  public static Color4 AdjustContrast(Color4 color, float contrast)
  {
    float alpha = color.Alpha;
    Color4 color4;
    color4.Alpha = alpha;
    color4.Red = (float) ((color.Red - 0.5) * contrast + 0.5);
    color4.Green = (float) ((color.Green - 0.5) * contrast + 0.5);
    color4.Blue = (float) ((color.Blue - 0.5) * contrast + 0.5);
    return color4;
  }

  public static void AdjustSaturation(ref Color4 color, float saturation, out Color4 result)
  {
    float num = (float) (color.Green * 0.715399980545044 + color.Red * 0.21250000596046448 + color.Blue * 0.07209999859333038);
    float alpha = color.Alpha;
    Color4 color4;
    color4.Alpha = alpha;
    color4.Red = (color.Red - num) * saturation + num;
    color4.Green = (color.Green - num) * saturation + num;
    color4.Blue = (color.Blue - num) * saturation + num;
    result = color4;
  }

  public static Color4 AdjustSaturation(Color4 color, float saturation)
  {
    float num = (float) (color.Green * 0.715399980545044 + color.Red * 0.21250000596046448 + color.Blue * 0.07209999859333038);
    float alpha = color.Alpha;
    Color4 color4;
    color4.Alpha = alpha;
    color4.Red = (color.Red - num) * saturation + num;
    color4.Green = (color.Green - num) * saturation + num;
    color4.Blue = (color.Blue - num) * saturation + num;
    return color4;
  }

  public static Color4 operator +(Color4 left, Color4 right)
  {
    Color4 color4;
    color4.Alpha = left.Alpha + right.Alpha;
    color4.Red = left.Red + right.Red;
    color4.Green = left.Green + right.Green;
    color4.Blue = left.Blue + right.Blue;
    return color4;
  }

  public static Color4 operator -(Color4 value)
  {
    Color4 color4;
    color4.Alpha = 1f - value.Alpha;
    color4.Red = 1f - value.Red;
    color4.Green = 1f - value.Green;
    color4.Blue = 1f - value.Blue;
    return color4;
  }

  public static Color4 operator -(Color4 left, Color4 right)
  {
    Color4 color4;
    color4.Alpha = left.Alpha - right.Alpha;
    color4.Red = left.Red - right.Red;
    color4.Green = left.Green - right.Green;
    color4.Blue = left.Blue - right.Blue;
    return color4;
  }

  public static Color4 operator *(Color4 color1, Color4 color2)
  {
    Color4 color4;
    color4.Alpha = color1.Alpha * color2.Alpha;
    color4.Red = color1.Red * color2.Red;
    color4.Green = color1.Green * color2.Green;
    color4.Blue = color1.Blue * color2.Blue;
    return color4;
  }

  public static Color4 operator *(float scale, Color4 value) => value * scale;

  public static Color4 operator *(Color4 value, float scale)
  {
    float alpha = value.Alpha;
    Color4 color4;
    color4.Alpha = alpha;
    color4.Red = value.Red * scale;
    color4.Green = value.Green * scale;
    color4.Blue = value.Blue * scale;
    return color4;
  }

  public static bool operator ==(Color4 left, Color4 right) => Equals(ref left, ref right);

  public static bool operator !=(Color4 left, Color4 right) => !Equals(ref left, ref right);

  public static explicit operator int(Color4 value) => value.ToArgb();

  public static explicit operator Color3(Color4 value) => new(value.Red, value.Green, value.Blue);

  public static explicit operator Vector3(Color4 value) => new(value.Red, value.Green, value.Blue);

  public static explicit operator Vector4(Color4 value) => new(value.Red, value.Green, value.Blue, value.Alpha);

  public static implicit operator Color4(Color value) => new(value);

  public static explicit operator Color4(Vector4 value)
  {
    Vector4 vector4 = value;
    Color4 color4;
    color4.Alpha = vector4.W;
    color4.Red = vector4.X;
    color4.Green = vector4.Y;
    color4.Blue = vector4.Z;
    return color4;
  }

  public static explicit operator Color4(Vector3 value)
  {
    Vector3 vector3 = value;
    Color4 color4;
    color4.Alpha = 1f;
    color4.Red = vector3.X;
    color4.Green = vector3.Y;
    color4.Blue = vector3.Z;
    return color4;
  }

  public static explicit operator Color4(Color3 value)
  {
    Color3 color3 = value;
    Color4 color4;
    color4.Alpha = 1f;
    color4.Red = color3.Red;
    color4.Green = color3.Green;
    color4.Blue = color3.Blue;
    return color4;
  }

  public static explicit operator Color4(int value) => new(value);

  public static explicit operator Color(Color4 value) => value.ToColor();

  public override string ToString()
  {
    object[] objArray = new object[4];
    float alpha = Alpha;
    objArray[0] = alpha.ToString(CultureInfo.CurrentCulture);
    float red = Red;
    objArray[1] = red.ToString(CultureInfo.CurrentCulture);
    float green = Green;
    objArray[2] = green.ToString(CultureInfo.CurrentCulture);
    float blue = Blue;
    objArray[3] = blue.ToString(CultureInfo.CurrentCulture);
    return string.Format(CultureInfo.CurrentCulture, "A:{0} R:{1} G:{2} B:{3}", objArray);
  }

  public override int GetHashCode()
  {
    float alpha = Alpha;
    float red = Red;
    float green = Green;
    float blue = Blue;
    int num = green.GetHashCode() + blue.GetHashCode() + red.GetHashCode();
    return alpha.GetHashCode() + num;
  }

  public static bool Equals(ref Color4 value1, ref Color4 value2)
  {
    return value1.Alpha == (double) value2.Alpha && value1.Red == (double) value2.Red && value1.Green == (double) value2.Green && value1.Blue == (double) value2.Blue;
  }

  public bool Equals(Color4 other)
  {
    return Alpha == (double) other.Alpha && Red == (double) other.Red && Green == (double) other.Green && Blue == (double) other.Blue;
  }

  public override bool Equals(object obj)
  {
    return obj != null && !(obj.GetType() != GetType()) && Equals((Color4) obj);
  }
}
