/*
 * Copyright 2006-2013 Bastian Eicher
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

using System;
using System.Diagnostics.CodeAnalysis;
using Common.Values;
using LuaInterface;
using SlimDX;
using Resources = Common.Properties.Resources;

namespace Common.Utils
{
    /// <summary>
    /// Designed to keep other code clean of messy spaghetti code required for some math operations.
    /// </summary>
    public static class MathUtils
    {
        #region Constants
        /// <summary>
        /// Pseudo-constant containing the value of sqrt(3)/3
        /// </summary>
        private static readonly float _sqrtThreeThirds = (float)Math.Sqrt(3) / 3;
        #endregion

        //--------------------//

        #region Clamp
        /// <summary>
        /// Makes a value stay within a certain range
        /// </summary>
        /// <param name="value">The number to clamp</param>
        /// <param name="min">The minimum number to return</param>
        /// <param name="max">The maximum number to return</param>
        /// <returns>The <paramref name="value"/> if it was in range, otherwise <paramref name="min"/> or <paramref name="max"/>.</returns>
        [LuaGlobal(Description = "Makes a value stay within a certain range")]
        public static decimal Clamp(this decimal value, decimal min, decimal max)
        {
            #region Sanity checks
            if (value < min) return min;
            if (value > max) return max;
            if (min > max) throw new ArgumentException(Resources.MinLargerMax, "min");
            #endregion

            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        /// <summary>
        /// Makes a value stay within a certain range
        /// </summary>
        /// <param name="value">The number to clamp</param>
        /// <param name="min">The minimum number to return</param>
        /// <param name="max">The maximum number to return</param>
        /// <returns>The <paramref name="value"/> if it was in range, otherwise <paramref name="min"/> or <paramref name="max"/>.</returns>
        [LuaGlobal(Description = "Makes a value stay within a certain range")]
        public static double Clamp(this double value, double min, double max)
        {
            #region Sanity checks
            if (value < min) return min;
            if (value > max) return max;
            if (min > max) throw new ArgumentException(Resources.MinLargerMax, "min");
            #endregion

            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        /// <summary>
        /// Makes a value stay within a certain range
        /// </summary>
        /// <param name="value">The number to clamp</param>
        /// <param name="min">The minimum number to return</param>
        /// <param name="max">The maximum number to return</param>
        /// <returns>The <paramref name="value"/> if it was in range, otherwise <paramref name="min"/> or <paramref name="max"/>.</returns>
        [LuaGlobal(Description = "Makes a value stay within a certain range")]
        public static float Clamp(this float value, float min, float max)
        {
            #region Sanity checks
            if (value < min) return min;
            if (value > max) return max;
            if (min > max) throw new ArgumentException(Resources.MinLargerMax, "min");
            #endregion

            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        /// <summary>
        /// Makes a value stay within a certain range
        /// </summary>
        /// <param name="value">The number to clamp</param>
        /// <param name="min">The minimum number to return</param>
        /// <param name="max">The maximum number to return</param>
        /// <returns>The <paramref name="value"/> if it was in range, otherwise <paramref name="min"/> or <paramref name="max"/>.</returns>
        public static int Clamp(this int value, int min, int max)
        {
            #region Sanity checks
            if (value < min) return min;
            if (value > max) return max;
            if (min > max) throw new ArgumentException(Resources.MinLargerMax, "min");
            #endregion

            if (value < min) return min;
            if (value > max) return max;
            return value;
        }
        #endregion

        #region Degree-Radian Conversion
        /// <summary>
        /// Converts an angle in degrees to radians
        /// </summary>
        /// <param name="value">The angle in degrees</param>
        /// <returns>The angle in radians</returns>
        [LuaGlobal(Description = "Converts an angle in degrees to radians")]
        public static float DegreeToRadian(this float value)
        {
            return value * ((float)Math.PI / 180);
        }

        /// <summary>
        /// Converts an angle in degrees to radians
        /// </summary>
        /// <param name="value">The angle in degrees</param>
        /// <returns>The angle in radians</returns>
        public static double DegreeToRadian(this double value)
        {
            return value * (Math.PI / 180);
        }

        /// <summary>
        /// Converts an angle in radians to degrees
        /// </summary>
        /// <param name="value">The angle in radians</param>
        /// <returns>The angle in degrees</returns>
        [LuaGlobal(Description = "Converts an angle in radians to degrees")]
        public static float RadianToDegree(this float value)
        {
            return value * (180 / (float)Math.PI);
        }

        /// <summary>
        /// Converts an angle in radians to degrees
        /// </summary>
        /// <param name="value">The angle in radians</param>
        /// <returns>The angle in degrees</returns>
        public static double RadianToDegree(this double value)
        {
            return value * (180 / Math.PI);
        }
        #endregion

        //--------------------//

        #region Interpolate
        /// <summary>
        /// Performs smooth (trigonometric) interpolation between two or more values
        /// </summary>
        /// <param name="factor">A factor between 0 and <paramref name="values"/>.Length</param>
        /// <param name="values">The value checkpoints</param>
        [LuaGlobal(Description = "Performs smooth (sinus-based) interpolation between two or more values")]
        public static double InterpolateTrigonometric(double factor, params double[] values)
        {
            #region Sanity checks
            if (values == null) throw new ArgumentNullException("values");
            if (values.Length < 2) throw new ArgumentException(Resources.AtLeast2Values);
            #endregion

            // Handle value overflows
            if (factor <= 0) return values[0];
            if (factor >= values.Length - 1) return values[values.Length - 1];

            // Isolate index shift from factor
            var index = (int)Math.Floor(factor);

            // Remove index shift from factor
            factor -= index;

            // Apply sinus smoothing to factor
            factor = -0.5 * Math.Cos(factor * Math.PI) + 0.5;

            return values[index] + factor * (values[index + 1] - values[index]);
        }

        /// <summary>
        /// Performs smooth (trigonometric) interpolation between two or more values
        /// </summary>
        /// <param name="factor">A factor between 0 and <paramref name="values"/>.Length</param>
        /// <param name="values">The value checkpoints</param>
        [LuaHide]
        public static float InterpolateTrigonometric(this float factor, params float[] values)
        {
            #region Sanity checks
            if (values == null) throw new ArgumentNullException("values");
            if (values.Length < 2) throw new ArgumentException(Resources.AtLeast2Values);
            #endregion

            // Handle value overflows
            if (factor <= 0) return values[0];
            if (factor >= values.Length - 1) return values[values.Length - 1];

            // Isolate index shift from factor
            var index = (int)factor;

            // Remove index shift from factor
            factor -= index;

            // Apply sinus smoothing to factor
            factor = (float)(-0.5 * Math.Cos(factor * Math.PI) + 0.5);

            return values[index] + factor * (values[index + 1] - values[index]);
        }

        /// <summary>
        /// Performs smooth (trigonometric) interpolation between two or more values
        /// </summary>
        /// <param name="factor">A factor between 0 and <paramref name="values"/>.Length</param>
        /// <param name="values">The value checkpoints</param>
        [LuaHide]
        public static Vector4 InterpolateTrigonometric(float factor, params Vector4[] values)
        {
            #region Sanity checks
            if (values == null) throw new ArgumentNullException("values");
            if (values.Length < 2) throw new ArgumentException(Resources.AtLeast2Values);
            #endregion

            // Handle value overflows
            if (factor <= 0) return values[0];
            if (factor >= values.Length - 1) return values[values.Length - 1];

            // Isolate index shift from factor
            var index = (int)factor;

            // Remove index shift from factor
            factor -= index;

            // Apply sinus smoothing to factor component-wise
            return new Vector4(
                values[index].X + factor * (values[index + 1].X - values[index].X),
                values[index].Y + factor * (values[index + 1].Y - values[index].Y),
                values[index].Z + factor * (values[index + 1].Z - values[index].Z),
                values[index].W + factor * (values[index + 1].W - values[index].W));
        }

        // ToDo: InterpolateGrid()
        #endregion

        #region Factorial
        /// <summary>Pre-calculated factorial lookup-table</summary>
        private static readonly double[] _factorialLookup = {1, 1, 2, 6, 24, 120, 720, 5040, 40320, 362880, 3628800, 39916800, 479001600, 6227020800, 87178291200, 1307674368000, 20922789888000};

        /// <summary>
        /// Calculates the factorial of n (n!)
        /// </summary>
        /// <param name="n">A value between 0 and 32768</param>
        /// <remarks>Values between n=0 and n=16 have been pre-calculated and are therefor very fast</remarks>
        public static double Factorial(int n)
        {
            #region Sanity checks
            if (n < 0) throw new ArgumentException(Resources.ArgMustNotBeNegative, "n");
            #endregion

            // Use the lookup-table when possible, otherwise use recursion
            if (n < _factorialLookup.Length) return _factorialLookup[n];
            return n * Factorial(n - 1);
        }

        /// <summary>
        /// Calculates the binomial coefficient (n choose k)
        /// </summary>
        /// <param name="n">A value between 0 and 32768</param>
        /// <param name="k">An integer</param>
        public static double BinomCoeff(int n, int k)
        {
            #region Sanity checks
            if (n < 0) throw new ArgumentException(Resources.ArgMustNotBeNegative, "n");
            #endregion

            return Factorial(n) / (Factorial(k) * Factorial(n - k));
        }

        /// <summary>
        /// Calculates a Bernstein basis polynomial
        /// </summary>
        private static double Bernstein(int n, int i, double t)
        {
            return BinomCoeff(n, i) * Math.Pow(t, i) * Math.Pow((1 - t), (n - i));
        }
        #endregion

        #region Bezier
        /// <summary>
        /// Calculates points on a 2D bezier curve
        /// </summary>
        /// <param name="controlPoints">An array of control points; the curve will pass through the first and the last entry</param>
        /// <param name="resolution">The desired number of output points</param>
        /// <returns>An array of <paramref name="resolution"/> points on the curve</returns>
        public static Vector2[] Bezier(int resolution, params Vector2[] controlPoints)
        {
            #region Sanity checks
            if (controlPoints == null) throw new ArgumentNullException("controlPoints");
            #endregion

            var output = new Vector2[resolution];
            double t = 0;
            double step = 1.0 / (resolution - 1);

            // Loop through the desired number of output points
            for (int iOut = 0; iOut < resolution; iOut++)
            {
                if ((1 - t) < 5e-6) t = 1.0;

                // Loop through the available control points
                for (int iIn = 0; iIn < controlPoints.Length; iIn++)
                {
                    var basis = (float)Bernstein(controlPoints.Length - 1, iIn, t);
                    output[iOut] += new Vector2(
                        basis * controlPoints[iIn].X,
                        basis * controlPoints[iIn].Y);
                }

                t += step;
            }

            return output;
        }

        /// <summary>
        /// Calculates points on a 3D bezier curve
        /// </summary>
        /// <param name="resolution">The desired number of output points</param>
        /// <param name="controlPoints">An array of control points; the curve will pass through the first and the last entry</param>
        /// <returns>An array of <paramref name="resolution"/> points on the curve</returns>
        public static DoubleVector3[] Bezier(int resolution, params DoubleVector3[] controlPoints)
        {
            #region Sanity checks
            if (controlPoints == null) throw new ArgumentNullException("controlPoints");
            #endregion

            var output = new DoubleVector3[resolution];
            double t = 0;
            double step = 1.0 / (resolution - 1);

            // Loop through the desired number of output points
            for (int iOut = 0; iOut < resolution; iOut++)
            {
                if ((1 - t) < 5e-6) t = 1.0;

                // Loop through the available control points
                for (int iRef = 0; iRef < controlPoints.Length; iRef++)
                {
                    var basis = Bernstein(controlPoints.Length - 1, iRef, t);
                    output[iOut] += new DoubleVector3(
                        basis * controlPoints[iRef].X,
                        basis * controlPoints[iRef].Y,
                        basis * controlPoints[iRef].Z);
                }

                t += step;
            }

            return output;
        }
        #endregion

        #region Gauss
        /// <summary>
        /// Generates a Gaussian kernel.
        /// </summary>
        /// <param name="sigma">The standard deviation of the Gaussian distribution.</param>
        /// <param name="kernelSize">The size of the kernel. Should be an uneven number.</param>
        public static double[] GaussKernel(double sigma, int kernelSize)
        {
            var kernel = new double[kernelSize];
            double sum = 0;
            for (int i = 0; i < kernel.Length; i++)
            {
                double x = i - kernelSize / 2;
                sum += kernel[i] = Math.Exp(-x * x / (2 * sigma * sigma));
            }

            // Normalize
            for (int i = 0; i < kernel.Length; i++)
                kernel[i] /= sum;

            return kernel;
        }
        #endregion

        //--------------------//

        #region High/low values
        [CLSCompliant(false)]
        public static short LoWord(uint l)
        {
            unchecked
            {
                return (short)(l & 0xffff);
            }
        }

        [CLSCompliant(false)]
        public static short LoWord(int l)
        {
            unchecked
            {
                return (short)(l & 0xffff);
            }
        }

        [CLSCompliant(false)]
        public static short HiWord(uint l)
        {
            unchecked
            {
                return (short)(l >> 16);
            }
        }

        [CLSCompliant(false)]
        public static short HiWord(int l)
        {
            unchecked
            {
                return (short)(l >> 16);
            }
        }

        public static int HiByte(byte b)
        {
            unchecked
            {
                return b >> 4;
            }
        }

        public static int LoByte(byte b)
        {
            unchecked
            {
                return b & 15;
            }
        }
        #endregion

        #region Flags
        /// <summary>
        /// Checks whether a certain flag is set in a flags value
        /// </summary>
        /// <param name="value">The value to check for a certain flag</param>
        /// <param name="flag">The flag to look for</param>
        /// <returns><see langword="true"/> if the <paramref name="flag"/> was set in <paramref name="value"/>.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", Justification = "The Term 'flag' is still in general use")]
        public static bool CheckFlag(this byte value, byte flag)
        {
            return (value & flag) == flag;
        }

        /// <summary>
        /// Checks whether a certain flag is set in a flags value
        /// </summary>
        /// <param name="value">The value to check for a certain flag</param>
        /// <param name="flag">The flag to look for</param>
        /// <returns><see langword="true"/> if the <paramref name="flag"/> was set in <paramref name="value"/>.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", Justification = "The Term 'flag' is still in general use")]
        public static bool CheckFlag(this int value, int flag)
        {
            return (value & flag) == flag;
        }
        #endregion

        //--------------------//

        #region Quaternions
        /// <summary>
        /// Calculates a rotation quaternion for a view vector
        /// </summary>
        /// <param name="view">The view vector</param>
        /// <param name="roll">The roll value</param>
        /// <returns>A normalized quaternion</returns>
        public static Quaternion ViewQuaternion(this Vector3 view, float roll)
        {
            return Quaternion.Normalize(new Quaternion(view.X, view.Y, view.Z, roll));
        }
        #endregion

        #region Rotate
        /// <summary>
        /// Rotates a <see cref="Vector2"/> by <paramref name="rotation"/> around the origin.
        /// </summary>
        /// <param name="value">The original vector.</param>
        /// <param name="rotation">The angle to rotate by in degrees.</param>
        /// <returns>The rotated <see cref="Vector2"/>.</returns>
        public static Vector2 Rotate(this Vector2 value, float rotation)
        {
            double phi = DegreeToRadian(rotation);
            return new Vector2(
                (float)(value.X * Math.Cos(phi) - value.Y * Math.Sin(phi)),
                (float)(value.X * Math.Sin(phi) + value.Y * Math.Cos(phi)));
        }
        #endregion

        #region Bounding box
        /// <summary>
        /// Applies a matrix transform to a bounding box.
        /// </summary>
        /// <param name="box">The bounding box to apply the transform to.</param>
        /// <param name="matrix">The transformation matrix to apply.</param>
        /// <returns>The transformed bounding box.</returns>
        public static BoundingBox Transform(this BoundingBox box, Matrix matrix)
        {
            float[] inputMin = new float[3], inputMax = new float[3];
            float[] outputMin = new float[3], outputMax = new float[3];
            var m = new float[3, 3];

            // Copy data into arrays for easy reference
            inputMin[0] = box.Minimum.X;
            inputMax[0] = box.Maximum.X;
            inputMin[1] = box.Minimum.Y;
            inputMax[1] = box.Maximum.Y;
            inputMin[2] = box.Minimum.Z;
            inputMax[2] = box.Maximum.Z;
            m[0, 0] = matrix.M11;
            m[0, 1] = matrix.M12;
            m[0, 2] = matrix.M13;
            m[1, 0] = matrix.M21;
            m[1, 1] = matrix.M22;
            m[1, 2] = matrix.M23;
            m[2, 0] = matrix.M31;
            m[2, 1] = matrix.M32;
            m[2, 2] = matrix.M33;

            // Account for the translation
            outputMin[0] = outputMax[0] = matrix.M41;
            outputMin[1] = outputMax[1] = matrix.M42;
            outputMin[2] = outputMax[2] = matrix.M43;

            // Find the extreme points by considering the product of the min and max with each component of M
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    float a = m[i, j] * inputMin[i];
                    float b = m[i, j] * inputMax[i];
                    if (a < b)
                    {
                        outputMin[j] += a;
                        outputMax[j] += b;
                    }
                    else
                    {
                        outputMin[j] += b;
                        outputMax[j] += a;
                    }
                }
            }

            // Copy the result into the new box
            return new BoundingBox(
                new Vector3(outputMin[0], outputMin[1], outputMin[2]),
                new Vector3(outputMax[0], outputMax[1], outputMax[2]));
        }
        #endregion

        #region Bounding sphere
        /// <summary>
        /// Applies a matrix transform to a bounding sphere.
        /// </summary>
        /// <param name="sphere">The bounding sphere to apply the transform to.</param>
        /// <param name="matrix">The transformation matrix to apply.</param>
        /// <returns>The transformed bounding sphere.</returns>
        public static BoundingSphere Transform(this BoundingSphere sphere, Matrix matrix)
        {
            // Extract translation data from the matrix
            var translation = new Vector3(matrix.M41, matrix.M42, matrix.M43);

            if (sphere.Radius <= 0)
                return new BoundingSphere(sphere.Center + translation, 0);

            // Scale, rotate and transform the center of the bounding sphere
            Vector3 newCenter = Vector3.TransformCoordinate(sphere.Center, matrix);

            // Scale a reference vector to determine the average axis factor for sphere scaling
            var referenceVector = Vector3.TransformCoordinate(new Vector3(_sqrtThreeThirds, _sqrtThreeThirds, _sqrtThreeThirds), matrix) - translation;
            float scale = referenceVector.Length();

            return new BoundingSphere(newCenter, sphere.Radius * scale);
        }
        #endregion
    }
}
