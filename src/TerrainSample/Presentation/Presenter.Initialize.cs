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
using System.IO;
using Common.Storage;
using OmegaEngine.Assets;
using OmegaEngine.Graphics.Renderables;
using OmegaEngine.Graphics.Shaders;
using TerrainSample.World.Config;

namespace TerrainSample.Presentation
{
    partial class Presenter
    {
        #region Initialize
        /// <summary>
        /// Generate <see cref="Terrain"/> and <see cref="Renderable"/>s from <see cref="World.Universe.Positionables"/> and keeps everything in sync using events
        /// </summary>
        /// <exception cref="FileNotFoundException">Thrown if a required <see cref="Asset"/> file could not be found.</exception>
        /// <exception cref="IOException">Thrown if there was an error reading an <see cref="Asset"/> file.</exception>
        /// <exception cref="InvalidDataException">Thrown if an <see cref="Asset"/> file contains invalid data.</exception>
        /// <remarks>Should be called before <see cref="HookIn"/> is used</remarks>
        public virtual void Initialize()
        {
            #region Sanity checks
            if (Disposed) throw new ObjectDisposedException(ToString());
            if (Initialized) return;
            #endregion

            // Handle fog and keep updated
            Universe.FogChanged += UpdateFog;
            UpdateFog();

            if (Lighting) SetupLighting();
            SetupTerrain();
            SetupEntities();

            Initialized = true;
        }
        #endregion

        #region Lighting
        /// <summary>
        /// Helper method for setting up the lighting and post-screen effects
        /// </summary>
        /// <seealso cref="Initialize"/>
        private void SetupLighting()
        {
            // Prepare post-screen shaders if the hardware supports them
            if (Engine.Capabilities.MaxShaderModel >= PostBleachShader.MinShaderModel)
            {
                // Auto-setup glow shader
                View.SetupGlow();

                // Pre-load deactivated effects for later use
                View.PostShaders.Add(_bleachShader = new PostBleachShader {Enabled = false});
                View.PostShaders.Add(_colorCorrectionShader = new PostColorCorrectionShader {Enabled = false});
                View.PostShaders.Add(_sepiaShader = new PostSepiaShader {Enabled = false, Desaturation = 0, Toning = 0});

                // Update when map settings change
                Universe.BleachChanged += UpdateBleach;
                UpdateBleach();
            }

            // Add the lights to the scene
            Scene.Lights.Add(_light1);
            Scene.Lights.Add(_light2);

            // Update when the lighting changes
            Universe.LightingChanged += UpdateLighting;
            UpdateLighting();
        }
        #endregion

        #region Terrain
        /// <summary>
        /// Helper method for setting up the <see cref="OmegaEngine.Graphics.Renderables.Terrain"/>.
        /// </summary>
        /// <seealso cref="Initialize"/>
        private void SetupTerrain()
        {
            if (Universe.Terrain == null) return;

            // Build texture array
            var textures = new string[Universe.Terrain.Templates.Length];
            for (int i = 0; i < textures.Length; i++)
            {
                if (Universe.Terrain.Templates[i] != null && !string.IsNullOrEmpty(Universe.Terrain.Templates[i].Texture))
                {
                    // Prefix directory name
                    textures[i] = Path.Combine("Terrain", Universe.Terrain.Templates[i].Texture);
                }
            }

            // Create Engine Terrain and add to the Scene
            Terrain = Terrain.Create(
                Engine, Universe.Terrain.Size,
                Universe.Terrain.Size.StretchH, Universe.Terrain.Size.StretchV,
                Universe.Terrain.HeightMap, Universe.Terrain.LightRiseAngleMap, Universe.Terrain.LightSetAngleMap,
                Universe.Terrain.TextureMap, textures,
                Lighting,
                Settings.Current.Graphics.TerrainBlockSize);
            Terrain.Wireframe = WireframeTerrain;
            Scene.Positionables.Add(Terrain);
        }

        /// <summary>
        /// Rebuilds the terrain from <see cref="World.Universe.Terrain"/> to reflect any modifications performed.
        /// </summary>
        public void RebuildTerrain()
        {
            if (Terrain != null)
            {
                View.Scene.Positionables.Remove(Terrain);
                Terrain.Dispose();
                Terrain = null;
            }
            SetupTerrain();
        }
        #endregion

        #region Entities
        /// <summary>
        /// Helper method for setting up the <see cref="Skybox"/> and <see cref="PositionableRenderable"/>s
        /// </summary>
        /// <seealso cref="Initialize"/>
        private void SetupEntities()
        {
            // Load skybox and keep updated
            UpdateSykbox();

            // Load universe info for the first time
            foreach (var entity in Universe.Positionables)
                AddPositionable(entity);

            // Update when entities change
            Universe.SkyboxChanged += UpdateSykbox;
            Universe.Positionables.Added += AddPositionable;
            Universe.Positionables.Removing += RemovePositionable;
        }
        #endregion

        #region Skybox
        private void UpdateSykbox()
        {
            // Clean up the old skybox if any
            if (Scene.Skybox != null)
            {
                Scene.Skybox.Dispose();
                Scene.Skybox = null;
            }

            // Allow for no skybox at all
            if (string.IsNullOrEmpty(Universe.Skybox)) return;

            // Right, Left, Up, Down, Front, Back texture filenames
            string rt = "Skybox/" + Universe.Skybox + "/rt.jpg";
            string lf = "Skybox/" + Universe.Skybox + "/lf.jpg";
            string up = "Skybox/" + Universe.Skybox + "/up.jpg";
            string dn = "Skybox/" + Universe.Skybox + "/dn.jpg";
            string ft = "Skybox/" + Universe.Skybox + "/ft.jpg";
            string bk = "Skybox/" + Universe.Skybox + "/bk.jpg";

            if (ContentManager.FileExists("Textures", up, true) && ContentManager.FileExists("Textures", dn, true))
            { // Full skybox
                Scene.Skybox = SimpleSkybox.FromAssets(Engine, rt, lf, up, dn, ft, bk);
            }
            else
            { // Cardboard-style skybox (missing top and bottom)
                Scene.Skybox = SimpleSkybox.FromAssets(Engine, rt, lf, null, null, ft, bk);
            }
        }
        #endregion
    }
}
