/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using JetBrains.Annotations;
using OmegaEngine.Values;
using SlimDX;

namespace OmegaEngine
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
        [Pure]
        public static double Clamp(this double value, double min = 0, double max = 1)
        {
            #region Sanity checks
            if (value < min) return min;
            if (value > max) return max;
            if (min > max) throw new ArgumentException(Properties.Resources.MinLargerMax, nameof(min));
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
        [Pure]
        public static float Clamp(this float value, float min = 0, float max = 1)
        {
            #region Sanity checks
            if (value < min) return min;
            if (value > max) return max;
            if (min > max) throw new ArgumentException(Properties.Resources.MinLargerMax, nameof(min));
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
        [Pure]
        public static int Clamp(this int value, int min = 0, int max = 1)
        {
            #region Sanity checks
            if (value < min) return min;
            if (value > max) return max;
            if (min > max) throw new ArgumentException(Properties.Resources.MinLargerMax, nameof(min));
            #endregion

            if (value < min) return min;
            if (value > max) return max;
            return value;
        }
        #endregion

        #region Modulo
        /// <summary>
        /// Calculates a modulus (always positive).
        /// </summary>
        [Pure]
        public static double Modulo(this double dividend, double divisor)
        {
            double result = dividend % divisor;
            if (result < 0) result += divisor;
            return result;
        }

        /// <summary>
        /// Calculates a modulus (always positive).
        /// </summary>
        [Pure]
        public static float Modulo(this float dividend, float divisor)
        {
            float result = dividend % divisor;
            if (result < 0) result += divisor;
            return result;
        }

        /// <summary>
        /// Calculates a modulus (always positive).
        /// </summary>
        [Pure]
        public static int Modulo(this int dividend, int divisor)
        {
            int result = dividend % divisor;
            if (result < 0) result += divisor;
            return result;
        }
        #endregion

        #region Degree-Radian Conversion
        /// <summary>
        /// Converts an angle in degrees to radians
        /// </summary>
        /// <param name="value">The angle in degrees</param>
        /// <returns>The angle in radians</returns>
        [Pure]
        public static float DegreeToRadian(this float value) => value * ((float)Math.PI / 180);

        /// <summary>
        /// Converts an angle in degrees to radians
        /// </summary>
        /// <param name="value">The angle in degrees</param>
        /// <returns>The angle in radians</returns>
        [Pure]
        public static double DegreeToRadian(this double value) => value * (Math.PI / 180);

        /// <summary>
        /// Converts an angle in radians to degrees
        /// </summary>
        /// <param name="value">The angle in radians</param>
        /// <returns>The angle in degrees</returns>
        [Pure]
        public static float RadianToDegree(this float value) => value * (180 / (float)Math.PI);

        /// <summary>
        /// Converts an angle in radians to degrees
        /// </summary>
        /// <param name="value">The angle in radians</param>
        /// <returns>The angle in degrees</returns>
        [Pure]
        public static double RadianToDegree(this double value) => value * (180 / Math.PI);
        #endregion

        #region Sphere coordinates
        /// <summary>
        /// Calculates a unit vector using spherical coordinates.
        /// </summary>
        /// <param name="inclination">Angle away from positive Z axis in radians. Values from 0 to Pi.</param>
        /// <param name="azimuth">Angle away from from positive X axis in radians. Values from 0 to 2*Pi.</param>
        [Pure]
        public static Vector3 UnitVector(double inclination, double azimuth) => new(
            (float)(Math.Sin(inclination) * Math.Cos(azimuth)),
            (float)(Math.Sin(inclination) * Math.Sin(azimuth)),
            (float)Math.Cos(inclination));
        #endregion

        #region Byte angles
        /// <summary>
        /// Maps a 0-255 byte value to a 0�-180� angle in radians.
        /// </summary>
        [Pure]
        public static double ByteToAngle(this byte b) => b / 255.0 * Math.PI;

        /// <summary>
        /// Maps a vector of 0-255 byte values to a vector of 0�-180� angles in radians.
        /// </summary>
        [Pure]
        public static Vector4 ByteToAngle(this ByteVector4 vector) => new(
            (float)vector.X.ByteToAngle(),
            (float)vector.Y.ByteToAngle(),
            (float)vector.Z.ByteToAngle(),
            (float)vector.W.ByteToAngle());
        #endregion

        //--------------------//

        #region Interpolate
        /// <summary>
        /// Performs smooth (trigonometric) interpolation between two or more values
        /// </summary>
        /// <param name="factor">A factor between 0 and <paramref name="values"/>.Length</param>
        /// <param name="values">The value checkpoints</param>
        [Pure]
        public static double InterpolateTrigonometric(double factor, [NotNull] params double[] values)
        {
            #region Sanity checks
            if (values == null) throw new ArgumentNullException(nameof(values));
            if (values.Length < 2) throw new ArgumentException(Properties.Resources.AtLeast2Values);
            #endregion

            // Handle value overflows
            if (factor <= 0) return values[0];
            if (factor >= values.Length - 1) return values[values.Length - 1];

            // Isolate index shift from factor
            int index = (int)Math.Floor(factor);

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
        [Pure]
        public static Vector4 InterpolateTrigonometric(float factor, [NotNull] params Vector4[] values)
        {
            #region Sanity checks
            if (values == null) throw new ArgumentNullException(nameof(values));
            if (values.Length < 2) throw new ArgumentException(Properties.Resources.AtLeast2Values);
            #endregion

            // Handle value overflows
            if (factor <= 0) return values[0];
            if (factor >= values.Length - 1) return values[values.Length - 1];

            // Isolate index shift from factor
            int index = (int)factor;

            // Remove index shift from factor
            factor -= index;

            // Apply sinus smoothing to factor component-wise
            return new(
                values[index].X + factor * (values[index + 1].X - values[index].X),
                values[index].Y + factor * (values[index + 1].Y - values[index].Y),
                values[index].Z + factor * (values[index + 1].Z - values[index].Z),
                values[index].W + factor * (values[index + 1].W - values[index].W));
        }
        #endregion

        #region Gauss
        /// <summary>
        /// Generates a Gaussian kernel.
        /// </summary>
        /// <param name="sigma">The standard deviation of the Gaussian distribution.</param>
        /// <param name="kernelSize">The size of the kernel. Should be an uneven number.</param>
        [Pure]
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
        [Pure]
        public static short LoWord(uint l)
        {
            unchecked
            {
                return (short)(l & 0xffff);
            }
        }

        [Pure]
        public static short HiWord(uint l)
        {
            unchecked
            {
                return (short)(l >> 16);
            }
        }

        [Pure] public static byte CombineHiLoByte(int high, int low) => (byte)((high << 4) + low);
        #endregion

        //--------------------//

        #region Rotate
        /// <summary>
        /// Rotates a <see cref="Vector2"/> by <paramref name="rotation"/> around the origin.
        /// </summary>
        /// <param name="value">The original vector.</param>
        /// <param name="rotation">The angle to rotate by in degrees.</param>
        /// <returns>The rotated <see cref="Vector2"/>.</returns>
        [Pure]
        public static Vector2 Rotate(this Vector2 value, float rotation)
        {
            double phi = DegreeToRadian(rotation);
            return new(
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
        [Pure]
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
            return new(
                new(outputMin[0], outputMin[1], outputMin[2]),
                new(outputMax[0], outputMax[1], outputMax[2]));
        }
        #endregion

        #region Bounding sphere
        /// <summary>
        /// Applies a matrix transform to a bounding sphere.
        /// </summary>
        /// <param name="sphere">The bounding sphere to apply the transform to.</param>
        /// <param name="matrix">The transformation matrix to apply.</param>
        /// <returns>The transformed bounding sphere.</returns>
        [Pure]
        public static BoundingSphere Transform(this BoundingSphere sphere, Matrix matrix)
        {
            // Extract translation data from the matrix
            var translation = new Vector3(matrix.M41, matrix.M42, matrix.M43);

            if (sphere.Radius <= 0)
                return new(sphere.Center + translation, 0);

            // Scale, rotate and transform the center of the bounding sphere
            var newCenter = Vector3.TransformCoordinate(sphere.Center, matrix);

            // Scale a reference vector to determine the average axis factor for sphere scaling
            var referenceVector = Vector3.TransformCoordinate(new(_sqrtThreeThirds, _sqrtThreeThirds, _sqrtThreeThirds), matrix) - translation;
            float scale = referenceVector.Length();

            return new(newCenter, sphere.Radius * scale);
        }
        #endregion
    }
}
