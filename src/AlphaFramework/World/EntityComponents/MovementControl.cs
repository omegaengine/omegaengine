/*
 * Copyright 2006-2014 Bastian Eicher
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
using System.ComponentModel;
using AlphaFramework.World.Positionables;

namespace AlphaFramework.World.EntityComponents
{
    /// <summary>
    /// Controls the basic movement parameters.
    /// </summary>
    public class MovementControl : ICloneable
    {
        /// <inheritdoc/>
        public override string ToString()
        {
            return GetType().Name;
        }

        private float _speed = 200;

        /// <summary>
        /// How many units the <see cref="EntityBase{TSelf,TCoordinates,TTemplate}"/> can walk per second.
        /// </summary>
        [DefaultValue(200f), Description("How many units the entity can walk per second.")]
        public float Speed { get { return _speed; } set { _speed = value; } }

        private float _curveRadius = 1;

        /// <summary>
        /// The length of the curve radius.
        /// </summary>
        [DefaultValue(1f), Description("The length of the curve radius.")]
        public float CurveRadius { get { return _curveRadius; } set { _curveRadius = value; } }

        //--------------------//

        #region Clone
        /// <summary>
        /// Creates a shallow copy of this <see cref="MovementControl"/>
        /// </summary>
        /// <returns>The cloned <see cref="MovementControl"/>.</returns>
        public MovementControl Clone()
        {
            // Perform initial shallow copy
            return (MovementControl)MemberwiseClone();
        }

        object ICloneable.Clone()
        {
            return Clone();
        }
        #endregion
    }
}
