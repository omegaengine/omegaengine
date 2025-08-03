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

// ReSharper disable once CheckNamespace
namespace SlimDX;

public struct Color3(float red, float green, float blue) : IEquatable<Color3>
{
    public float Red = red;
    public float Green = green;
    public float Blue = blue;

    public static bool operator ==(Color3 left, Color3 right) => Equals(ref left, ref right);

    public static bool operator !=(Color3 left, Color3 right) => !Equals(ref left, ref right);

    public override int GetHashCode()
    {
        float red = Red;
        float green = Green;
        float blue = Blue;
        int num = green.GetHashCode() + blue.GetHashCode();
        return red.GetHashCode() + num;
    }

    public static bool Equals(ref Color3 value1, ref Color3 value2)
    {
        return value1.Red == (double) value2.Red && value1.Green == (double) value2.Green && value1.Blue == (double) value2.Blue;
    }

    public bool Equals(Color3 other)
    {
        return Red == (double) other.Red && Green == (double) other.Green && Blue == (double) other.Blue;
    }

    public override bool Equals(object obj)
    {
        return obj != null && !(obj.GetType() != GetType()) && Equals((Color3) obj);
    }
}

