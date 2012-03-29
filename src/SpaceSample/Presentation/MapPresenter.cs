using System;
using System.Collections.Generic;
using System.Drawing;
using Common.Values;
using Core;
using EngineBody = OmegaEngine.Graphics.Entities.Body;
using OmegaEngine;
using OmegaEngine.Content;
using OmegaEngine.Graphics;
using OmegaEngine.Graphics.Cameras;
using OmegaEngine.Graphics.Entities;
using World;
using World.Entities;
using WorldBody=World.Body;
using WorldEntity=World.Entity;

namespace Presentation
{
    /// <summary>
    /// Controls the scene shown as an in-game map
    /// </summary>
    public sealed class MapPresenter : InteractivePresenter
    {
        #region Constants
        private const float maximumBodyRadius = 150;
        #endregion

        #region Variables
        //private List<ColoredLine> orbitLines;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new map scene
        /// </summary>
        /// <param name="engine">The engine to use for rendering</param>
        /// <param name="universe">The universe to display</param>
        public MapPresenter(Engine engine, Universe universe) : base(engine, null, universe)
        {
            scene = new Scene(engine);
            //lighting = false;
            mainView = new View(engine, scene, new TrackCamera(), new Rectangle(200, 20, 640, 480)) {Name = "Map"};

            #region Shaders
            if (engine.SupportsPerPixelEffects)
            {
                //MainView.SetupGlow();
            }
            #endregion

            views.Add(mainView);
            scene.Lights.Add(new PointLight
            {
                Name = "Main",
                DirectionalForShader = true,
                Diffuse = Color.FromArgb(210, 210, 210),
                Ambient = Color.FromArgb(100, 100, 100),
                Range = (float)Math.Sqrt(float.MaxValue)
            });

            Camera.NearClip = 0.001f;
            Camera.FarClip = 1e+5f;

            mainView.BackgroundColor = Color.CornflowerBlue;
            mainView.FullAlpha = 75;

            //Scene.PreRender += delegate
            //{
            //    foreach (RenderLine line in orbitLines) scene.Objects.Remove(line);
            //    PredictOrbits();
            //};
            //PredictOrbits();
        }
        #endregion

        #region Initialize
        /// <summary>
        /// Hook the presenter's views into the engine's render system
        /// </summary>
        public override void HookIn()
        {
            base.HookIn();

            FocusCamera(Universe.GetBody("Sol"));
        }
        #endregion

        //--------------------//

        #region Camera
        protected override TrackCamera GenerateCamera(WorldBody target)
        {
            return new TrackCamera
            {
                Name = Camera.Name,
                NearClip = 10.0f, FarClip = 1e+5f, ClipPlane = Camera.ClipPlane,
                Radius = (target.ScaledRadius > maximumBodyRadius ? maximumBodyRadius : target.ScaledRadius) * 3,
                HorizontalRotation = 90
            };
        }
        #endregion

        //--------------------//

        #region Add entities

        #region Sun
        protected override VertexGroup GetSun(Sun body)
        {
            VertexGroup sunFace = VertexGroup.Quad(engine, 1,
                XMaterial.FromCache(engine, "Sun/" + body.Name + "_map.png", null, null, null, "Sun/" + body.Name + "_map_glow.png"));
            sunFace.Billboard = BillboardMode.Spherical;
            sunFace.Position = body.ScaledPosition;
            sunFace.Alpha = 256;
            sunFace.Name = body.Name;

            return sunFace;
        }
        #endregion

        #region Rings
        protected override Model GetRings(Planetoid planetoid, Model meshBody)
        {
            // Find the ring map for this planet
            string filename;
            if (planetoid is Moon)
                filename = "Moons/" + planetoid.Name + "-Rings.png";
            else
                filename = "Planetoids/" + planetoid.Name + "-Rings.png";

            Model discModel = Model.Disc(engine, filename, maximumBodyRadius * 1.2f, maximumBodyRadius * 2.5f, 0.1f, 50);
            discModel.Alpha = 256;
            if (lighting) discModel.SurfaceEffect = SurfaceEffect.Shader;

            discModel.Position = meshBody.Position;
            discModel.VisibilityDistance = planetoid.Rings.ScaledOuterRadius * Settings.Current.Graphics.VisibilityFactor;
            discModel.Name = planetoid.Name + " rings";
            return discModel;
        }
        #endregion

        #endregion

        #region Update entities
        protected override void UpdateEntity(WorldEntity entity)
        {
            // Get all associated engine bodies from the association list
            foreach (EngineBody renderable in assoc[entity])
            {
                double distanceFromCenter = entity.ScaledPosition.Length();
                if (distanceFromCenter > 0)
                {
                    // Shrink the distance using a square-root function
                    renderable.Position = entity.ScaledPosition *
                        Math.Pow(distanceFromCenter, -Settings.Current.Graphics.MapPower);
                }

                var body = entity as WorldBody;
                if (body != null)
                {
                    #region Scaling
                    float effectiveRadius = body.ScaledRadius > maximumBodyRadius ? maximumBodyRadius : body.ScaledRadius;

                    if (body is Sun)
                    {
                        // Sun needs special treatment due to its huge size
                        renderable.SetScale(body.ScaledRadius * 0.05f);
                    }
                    else if (renderable.Name.Contains("rings"))
                    {
                        // Rings cannot be easily rescaled
                    }
                    else if (renderable.Name.Contains("atmosphere"))
                    {
                        // Atmosphere is an additional layer around the existing body
                        var atmosphereBody = body as IAtmosphere;
                        if (atmosphereBody != null && atmosphereBody.AtmosphereInfo != null)
                            renderable.SetScale(effectiveRadius + atmosphereBody.AtmosphereInfo.ScaledThickness);
                    }
                    else
                    {
                        renderable.SetScale(effectiveRadius);
                    }
                    #endregion

                    renderable.Rotation = body.Rotation;
                }
            }

            if (entity == cameraTarget)
                UpdateCamera(Camera);
        }
        #endregion

        //--------------------//

        #region Orbit prediction
        /// <summary>
        /// Calculates the future positions of orbiting bodies
        /// </summary>
        private void PredictOrbits()
        {
            #region Calculation
            // Predict one year
            int predictSteps = (365 * 24 * 60 * 60) / Settings.Current.General.UniversePredictSecs;

            // Create multiple prediction universes
            var predictions = new List<Universe> {Universe.CloneUniverse()};
            for (int i = 0; i <= predictSteps; i++)
            {
                predictions.Add(predictions[i].Predict(Settings.Current.General.UniversePredictSecs));
            }

            // Build list of predicted body positions
            var bodyPositions = new SortedList<string, DecimalVector3>();
            foreach (Universe predictedUniverse in predictions)
            {
                foreach (WorldBody body in predictedUniverse.Entities)
                {
                    if (body is Planet)
                        bodyPositions.Add(body.Name, body.ScaledPosition);
                }
            }
            #endregion

            #region Graphics
            //// Build graphical representation of orbits for each body (clearing out the old one)
            //orbitLines = new List<ColoredLine>();
            //foreach (string body in bodyPositions.GetKeys())
            //{
            //    EasyDictionary<Vector3, Color> points = new EasyDictionary<Vector3, Color>();
            //    int i = 0;
            //    foreach (SuperVectorDouble position in bodyPositions.GetValues(body))
            //    {
            //        points.Add(position.GetRelative(cameraOrigin),
            //            ColorHelper.InterpolateColor(Color.Green, Color.Red, ((float)i++ / (float)predictSteps)));
            //    }
            //    orbitLines.Add(new ColoredLine(points));
            //}

            //foreach (ColoredLine line in orbitLines)
            //    scene.Entities.Add(line);
            #endregion
        }
        #endregion
    }
}
