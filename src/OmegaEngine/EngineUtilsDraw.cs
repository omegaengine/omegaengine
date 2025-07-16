/*
 * Copyright 2006-2014 Bastian Eicher
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this file,
 * You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Drawing;
using OmegaEngine.Graphics.Shaders;
using OmegaEngine.Graphics.VertexDecl;
using SlimDX;
using SlimDX.Direct3D9;

namespace OmegaEngine
{
    /// <summary>
    /// Provides simple draw helpers for the <see cref="Engine"/>.
    /// </summary>
    public static class EngineUtilsDraw
    {
        /// <summary>
        /// Draws a 2D colored rectangle outline
        /// </summary>
        /// <param name="engine">The engine to use for rendering</param>
        /// <param name="rectangle">The rectangle to draw</param>
        /// <param name="color">The color to draw the rectangle in</param>
        public static void DrawRectangleOutline(this Engine engine, Rectangle rectangle, Color color)
        {
            #region Sanity checks
            if (engine == null) throw new ArgumentNullException(nameof(engine));
            #endregion

            TransformedColored[] vertexes =
            [
                new(rectangle.Left, rectangle.Top, 0, 1, color.ToArgb()),
                new(rectangle.Right, rectangle.Top, 0, 1, color.ToArgb()),
                new(rectangle.Right, rectangle.Bottom, 0, 1, color.ToArgb()),
                new(rectangle.Left, rectangle.Bottom, 0, 1, color.ToArgb())
            ];
            engine.Device.VertexFormat = TransformedColored.Format;
            engine.State.SetTexture(null);
            engine.Device.DrawIndexedUserPrimitives(PrimitiveType.LineStrip, 0, 5, 4,
                // ToDo: Properly determine vertex stride
                [0, 1, 2, 3, 0], Format.Index32, vertexes, 20);
        }

        /// <summary>
        /// Draw a textured quad filling the current <see cref="Viewport"/> using <see cref="PositionTextured"/> - useful for <see cref="PostShader"/>
        /// </summary>
        /// <param name="engine">The engine to use for rendering</param>
        public static void DrawQuadShader(this Engine engine)
        {
            #region Sanity checks
            if (engine == null) throw new ArgumentNullException(nameof(engine));
            #endregion

            using (new ProfilerEvent("Rendering textured quad for post-screen shaders"))
            {
                // Prepare render states
                engine.State.FillMode = FillMode.Solid;
                engine.State.CullMode = Cull.Counterclockwise;
                engine.State.FfpLighting = false;
                engine.State.Fog = false;

                PositionTextured[] vertexes =
                [
                    new(-1, 1, 0, 0, 0),
                    new(-1, -1, 0, 0, 1),
                    new(1, 1, 0, 1, 0),
                    new(1, -1, 0, 1, 1)
                ];

                engine.Device.VertexFormat = PositionTextured.Format;
                engine.Device.DrawUserPrimitives(PrimitiveType.TriangleStrip, 2, vertexes);
            }
        }

        /// <summary>
        /// Draw a textured quad filling the current <see cref="Viewport"/> using <see cref="TransformedTextured"/>
        /// </summary>
        /// <param name="engine">The engine to use for rendering</param>
        public static void DrawQuadTextured(this Engine engine)
        {
            #region Sanity checks
            if (engine == null) throw new ArgumentNullException(nameof(engine));
            #endregion

            using (new ProfilerEvent("Rendering textured quad"))
            {
                // Prepare render states
                engine.State.FillMode = FillMode.Solid;
                engine.State.CullMode = Cull.Counterclockwise;
                engine.State.FfpLighting = false;
                engine.State.Fog = false;

                // Calculate texture coordinates in the centers of the corner texels
                TransformedTextured[] vertexes =
                [
                    new(engine.Device.Viewport.X, engine.Device.Viewport.Y + engine.Device.Viewport.Height - 1, 0, 1, 0, 1),
                    new(engine.Device.Viewport.X, engine.Device.Viewport.Y, 0, 1, 0, 0),
                    new(engine.Device.Viewport.X + engine.Device.Viewport.Width - 1, engine.Device.Viewport.Y + engine.Device.Viewport.Height - 1, 0, 1, 1, 1),
                    new(engine.Device.Viewport.X + engine.Device.Viewport.Width - 1, engine.Device.Viewport.Y, 0, 1, 1, 0)
                ];

                // Disable texture filtering
                engine.Device.SetSamplerState(0, SamplerState.MinFilter, TextureFilter.None);
                engine.Device.SetSamplerState(0, SamplerState.MagFilter, TextureFilter.None);

                engine.Device.VertexFormat = TransformedTextured.Format;
                engine.Device.DrawUserPrimitives(PrimitiveType.TriangleStrip, 2, vertexes);

                // Restore texture filtering
                engine.SetupTextureFiltering();
            }
        }

        /// <summary>
        /// Draws a colored quad filling the current <see cref="Viewport"/> using <see cref="TransformedColored"/> - useful for fading
        /// </summary>
        /// <param name="engine">The engine to use for rendering</param>
        /// <param name="color">The color of the quad</param>
        public static void DrawQuadColored(this Engine engine, Color color)
        {
            #region Sanity checks
            if (engine == null) throw new ArgumentNullException(nameof(engine));
            #endregion

            using (new ProfilerEvent("Rendering colored quad"))
            {
                int colVal = color.ToArgb();

                // Prepare render states
                engine.State.FillMode = FillMode.Solid;
                engine.State.CullMode = Cull.Counterclockwise;
                engine.State.FfpLighting = false;
                engine.State.Fog = false;

                TransformedColored[] vertexes =
                [
                    new(engine.Device.Viewport.X, engine.Device.Viewport.Y + engine.Device.Viewport.Height - 1, 0, 1, colVal),
                    new(engine.Device.Viewport.X, engine.Device.Viewport.Y, 0, 1, colVal),
                    new(engine.Device.Viewport.X + engine.Device.Viewport.Width - 1, engine.Device.Viewport.Y + engine.Device.Viewport.Height - 1, 0, 1, colVal),
                    new(engine.Device.Viewport.X + engine.Device.Viewport.Width - 1, engine.Device.Viewport.Y, 0, 1, colVal)
                ];

                engine.State.SetTexture(null);
                engine.Device.VertexFormat = TransformedColored.Format;
                engine.Device.DrawUserPrimitives(PrimitiveType.TriangleStrip, 2, vertexes);
            }
        }

        /// <summary>
        /// Display a <see cref="BoundingSphere"/> as a wireframe model
        /// </summary>
        /// <param name="engine">The engine to use for rendering</param>
        /// <param name="sphere">The sphere to be display</param>
        public static void DrawBoundingSphere(this Engine engine, BoundingSphere sphere)
        {
            #region Sanity checks
            if (engine == null) throw new ArgumentNullException(nameof(engine));
            #endregion

            using (new ProfilerEvent("Draw bounding sphere"))
            {
                // Backup current render states
                Matrix lastWorldTransform = engine.State.WorldTransform;
                Cull lastCullMode = engine.State.CullMode;
                FillMode lastFillMode = engine.State.FillMode;
                bool lastLighting = engine.State.FfpLighting;

                // Set new states
                engine.State.WorldTransform = Matrix.Scaling(new(sphere.Radius)) * Matrix.Translation(sphere.Center);
                engine.State.CullMode = Cull.None;
                engine.State.FillMode = FillMode.Wireframe;
                engine.State.FfpLighting = false;

                // Render the sphere
                engine.State.SetTexture(null);
                engine.SimpleSphere.DrawSubset(0);

                // Restore the old states
                engine.State.WorldTransform = lastWorldTransform;
                engine.State.CullMode = lastCullMode;
                engine.State.FillMode = lastFillMode;
                engine.State.FfpLighting = lastLighting;
            }
        }

        /// <summary>
        /// Display a <see cref="BoundingBox"/> as a wireframe model
        /// </summary>
        /// <param name="engine">The engine to use for rendering</param>
        /// <param name="box">The box to be display</param>
        public static void DrawBoundingBox(this Engine engine, BoundingBox box)
        {
            #region Sanity checks
            if (engine == null) throw new ArgumentNullException(nameof(engine));
            #endregion

            using (new ProfilerEvent("Draw bounding box"))
            {
                // Backup current render states
                Matrix lastWorldTransform = engine.State.WorldTransform;
                Cull lastCullMode = engine.State.CullMode;
                FillMode lastFillMode = engine.State.FillMode;
                bool lastLighting = engine.State.FfpLighting;

                // Set new states
                Vector3 boxCenter = box.Minimum + (box.Maximum - box.Minimum) * 0.5f;
                engine.State.WorldTransform = Matrix.Scaling(box.Maximum - box.Minimum) * Matrix.Translation(boxCenter);
                engine.State.CullMode = Cull.None;
                engine.State.FillMode = FillMode.Wireframe;
                engine.State.FfpLighting = false;

                // Render the box
                engine.State.SetTexture(null);
                engine.SimpleBox.DrawSubset(0);

                // Restore the old states
                engine.State.WorldTransform = lastWorldTransform;
                engine.State.CullMode = lastCullMode;
                engine.State.FillMode = lastFillMode;
                engine.State.FfpLighting = lastLighting;
            }
        }
    }
}
