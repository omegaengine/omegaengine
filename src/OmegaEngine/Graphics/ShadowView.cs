/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Drawing;
using OmegaEngine.Graphics.Renderables;

namespace OmegaEngine.Graphics
{
    /// <summary>
    /// A <see cref="SpecialView"/> for rendering shadow maps
    /// </summary>
    public sealed class ShadowView : SpecialView
    {
        #region Constructor
        /// <summary>
        /// Creates a new view for rendering shadow maps
        /// </summary>
        /// <param name="baseView">The <see cref="View"/> to base this shadow-view on</param>
        internal ShadowView(View baseView) : base(baseView, baseView.Camera)
        {
            BackgroundColor = Color.Black;
        }
        #endregion

        //--------------------//

        #region Render body
        /// <inheritdoc />
        protected override void RenderBody(PositionableRenderable body)
        {
            #region Sanity checks
            if (body == null) throw new ArgumentNullException("body");
            #endregion

            // Backup the current surface effect and replace it by a special one for depth
            SurfaceEffect surfaceEffect = body.SurfaceEffect;
            body.SurfaceEffect = SurfaceEffect.Depth;

            base.RenderBody(body);

            // Restore the original surface effect
            body.SurfaceEffect = surfaceEffect;
        }
        #endregion
    }
}
