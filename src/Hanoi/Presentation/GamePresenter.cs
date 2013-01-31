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
using System.Drawing;
using Common.Utils;
using Common.Values;
using Hanoi.Logic;
using OmegaEngine;
using OmegaEngine.Graphics;
using OmegaEngine.Graphics.Cameras;
using OmegaEngine.Graphics.Renderables;
using SlimDX;
using View = OmegaEngine.Graphics.View;

namespace Hanoi.Presentation
{
    /// <summary>
    /// Controls the scene displayed for testing purpose
    /// </summary>
    public sealed class GamePresenter : Presenter
    {
        #region Variables
        private readonly Model[] _pegModels;
        private readonly Model[] _discModels;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new simulation scene
        /// </summary>
        /// <param name="engine">The engine to use for rendering</param>
        /// <param name="universe">The simulation universe to display</param>
        public GamePresenter(Engine engine, Universe universe) : base(engine, universe)
        {
            Scene = new Scene(engine);
            View = new View(engine, Scene, new TrackCamera(10, 1000) {NearClip = 1, FarClip = 1e+6f, Radius = 50, HorizontalRotation = 90})
            {
                Name = "Simulation",
                BackgroundColor = Color.Gray,
            };

            View.PreRender += delegate
            {
                lock (Universe)
                {
                    UpdateDiscPositions();
                }
            };

            Scene.Skybox = SimpleSkybox.FromAsset(engine,
                @"Skybox\rt.jpg", @"Skybox\lf.jpg", @"Skybox\up.jpg", @"Skybox\dn.jpg", @"Skybox\ft.jpg", @"Skybox\bk.jpg");
            Scene.Lights.Add(new DirectionalLight
            {
                Direction = new Vector3(0, -0.5f, -0.5f),
                Specular = Color.Gray,
                Ambient = Color.FromArgb(32, 32, 32)
            });

            _pegModels = new Model[universe.NumberPegs];
            _discModels = new Model[universe.NumberDiscs];

            BuildModels();
        }
        #endregion

        //--------------------//

        #region Build models
        public void BuildModels()
        {
            for (int i = 0; i < Universe.NumberPegs; i++)
            {
                Model pegModel = CreatePeg(
                    ((Universe.NumberPegs / 2) - i) * (Universe.NumberDiscs * 2 + 3),
                    Universe.NumberDiscs, Color.Silver);
                _pegModels[i] = pegModel;
                Scene.Positionables.Add(pegModel);
            }

            for (int i = 0; i < Universe.NumberDiscs; i++)
            {
                Model discModel = CreateDisc(i);
                _discModels[i] = discModel;
                Scene.Positionables.Add(discModel);
            }

            UpdateDiscPositions();
        }
        #endregion

        #region Create bodies
        private const int Slices = 30;

        #region Peg
        private Model CreatePeg(float position, float height, Color color)
        {
            Model peg = Model.Cylinder(Engine, null, 1, 1, height + 2.5f, Slices, 2);
            peg.Rotation = Quaternion.RotationYawPitchRoll(0, (float)Math.PI / 2, 0);
            peg.Position = new DoubleVector3(position, -0.2, 0);
            peg.Materials[0] = new XMaterial(color);
            peg.SurfaceEffect = SurfaceEffect.Shader;
            return peg;
        }
        #endregion

        #region Disc
        private Model CreateDisc(float size)
        {
            Color color = Color.FromArgb(RandomUtils.GetRandomByte(0, 255), RandomUtils.GetRandomByte(0, 255), RandomUtils.GetRandomByte(0, 255));

            Model disc = Model.Disc(Engine, null, 1, size + 2, 1, Slices * 2);
            disc.Materials[0] = new XMaterial(color);
            disc.SurfaceEffect = SurfaceEffect.Shader;
            return disc;
        }
        #endregion

        #endregion

        #region Update disc position
        private void UpdateDiscPositions()
        {
            Peg[] pegs = Universe.GetPegs();
            for (int peg = 0; peg < pegs.Length; peg++)
            {
                Disc[] discs = pegs[peg].GetDiscs();
                for (int height = 0; height < discs.Length; height++)
                {
                    Disc disc = discs[height];

                    double xPos, yPos;

                    if (disc == Universe.LastMovedDisc)
                    {
                        double oldXPos = _pegModels[Universe.GetPegIndex(Universe.LastSourcePeg)].Position.X;
                        double oldYPos = Universe.LastSourcePeg.DiscCount;

                        double newXPos = _pegModels[peg].Position.X;
                        double newYPos = height;

                        // ToDo: Nicer movement
                        double blendFactor = MathUtils.Clamp(Universe.StepTime, 0, 1);

                        xPos = oldXPos * (1 - blendFactor) + newXPos * blendFactor;
                        yPos = oldYPos * (1 - blendFactor) + newYPos * blendFactor;
                    }
                    else
                    {
                        xPos = _pegModels[peg].Position.X;
                        yPos = height;
                    }

                    _discModels[disc.Size].Position = new DoubleVector3(
                        xPos, yPos - (int)(Universe.NumberDiscs / 2f), 0);
                }
            }
        }
        #endregion
    }
}
