using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using Common.Collections;
using Common.Storage;
using Common.Values;
using Core;
using Microsoft.DirectX;
using OmegaEngine;
using OmegaEngine.Content;
using OmegaEngine.Graphics;
using OmegaEngine.Graphics.Cameras;
using OmegaEngine.Graphics.Entities;
using OmegaEngine.Sound;
using WorldEntity=World.Entities.Entity;
using WorldBody = World.Entities.Body;

namespace Presentation
{
    /// <summary>
    /// Handles the visual representation of World content in the Engine
    /// </summary>
    public abstract class Presenter : IDisposable
    {
        #region Variables
        protected readonly Engine engine;
        protected IList<View> views = new List<View>();
        protected MusicManager musicManager;
        protected bool lighting = true;

        protected WorldEntity cameraTarget;
        protected DecimalVector3 cameraTargetShift;

        /// <summary>
        /// 1:n association of WorldEntity to Body
        /// </summary>
        protected MultiDictionary<WorldEntity, Body> assoc = new MultiDictionary<WorldEntity, Body>(true);
        #endregion

        #region Properties
        /// <summary>
        /// The view this presenter uses for rendering
        /// </summary>
        public View MainView { get; protected set; }

        /// <summary>
        /// The scene as it is used by the engine
        /// </summary>
        public Scene Scene { get; protected set; }

        /// <summary>
        /// The universe data for this scene
        /// </summary>
        public World.Universe Universe { get; protected set; }

        /// <summary>
        /// The camera used to view the scene
        /// </summary>
        public Camera Camera { get { return MainView.Camera; } set { MainView.Camera = value; } }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new presenter
        /// </summary>
        /// <param name="engine">The engine to use for rendering</param>
        /// <param name="musicManager">The music manager</param>
        /// <param name="universe">The universe to display</param>
        protected Presenter(Engine engine, MusicManager musicManager, World.Universe universe)
        {
            this.engine = engine;
            this.musicManager = musicManager;
            Universe = universe;
        }
        #endregion


        #region Assoc helper
        /// <summary>
        /// Returns the first <see cref="Body"/> that is associated with the given <see cref="WorldEntity"/>
        /// </summary>
        /// <param name="entity">The <see cref="WorldEntity"/> to search for</param>
        /// <exception cref="ArgumentException">Thrown if the body was not found</exception>
        /// <returns>The first <see cref="Body"/> that was found; null if none was found</returns>
        protected Body GetBody(WorldEntity entity)
        {
            // Get first engine body associated with camera target
            foreach (Body graphicsBody in assoc[entity])
                return graphicsBody;

            throw new ArgumentException("Body not found");
        }

        /// <summary>
        /// Returns the <see cref="WorldBody"/> that is associated with the given <see cref="Body"/>
        /// </summary>
        /// <param name="body">The <see cref="Body"/> to search for</param>
        /// <exception cref="ArgumentException">Thrown if the entity was not found</exception>
        /// <returns>The <see cref="WorldBody"/> that was found; null if none was found</returns>
        protected WorldBody GetWorldBody(Body body)
        {
            foreach (KeyValuePair<WorldEntity, ICollection<Body>> pair in assoc)
            {
                if (pair.Value.Contains(body))
                    return pair.Key as WorldBody;
            }
            throw new ArgumentException("Body not found");
        }
        #endregion

        #region Camera
        protected void UpdateCamera(Camera camera)
        {
            var trackCamera = camera as TrackCamera;
            if (trackCamera != null && cameraTarget != null)
                trackCamera.Target = GetBody(cameraTarget).Position + cameraTargetShift;
        }

        protected virtual TrackCamera GenerateCamera(WorldBody target)
        {
            return new TrackCamera
            {
                Name = Camera.Name,
                NearClip = 10.0f,
                FarClip = 1e+8f,
                ClipPlane = Camera.ClipPlane,
                Radius = target.ScaledRadius * 3,
                HorizontalRotation = 90
            };
        }

        /// <summary>
        /// Focuses the camera onto a specific body
        /// </summary>
        /// <param name="target">The body to lock the camera on to</param>
        public void FocusCamera(WorldBody target)
        {
            TrackCamera newCamera = GenerateCamera(target);

            cameraTarget = target;
            UpdateCamera(newCamera);

            MainView.Camera = newCamera;
        }

        /// <summary>
        /// Swings the camera to a specific body
        /// </summary>
        /// <param name="target">The body to lock the camera on to</param>
        public void SwingCamera(WorldBody target)
        {
            var newCamera = GenerateCamera(target);
            var oldCamera = Camera as TrackCamera;
            if (oldCamera != null)
                newCamera.HorizontalRotation = oldCamera.HorizontalRotation;

            cameraTarget = target;
            UpdateCamera(newCamera);

            MainView.SwingCameraTo(newCamera, 0.8f);
        }
        #endregion

        #region Music Control
        /// <summary>
        /// Starts playing music again after it had been deactivated
        /// </summary>
        public virtual void RestoreMusic()
        {}
        #endregion

        #region Engine Hook-in
        /// <summary>
        /// Hook the presenter's views into the engine's render system
        /// </summary>
        public virtual void HookIn()
        {
            // Load universe info for the first time
            foreach (WorldEntity entity in Universe.Entities)
                AddEntity(entity);

            // Keep info about the universe updated
            Universe.Entities.Added += AddEntity;
            Universe.Entities.Removing += RemoveEntity;

            // Initialize the camera
            UpdateCamera(Camera);

            // Hook into engine
            foreach (View view in views)
                engine.Views.Add(view);
        }

        /// <summary>
        /// Hook the presenter's views out of the engine's render system
        /// </summary>
        public virtual void HookOut()
        {
            foreach (View view in views)
                engine.Views.Remove(view);

            // Stop updating universe
            Universe.Entities.Added -= AddEntity;
            Universe.Entities.Removing -= RemoveEntity;

            // Clean up assocs
            foreach (WorldEntity entity in Universe.Entities)
                RemoveEntity(entity);
            assoc.Clear();
        }
        #endregion


        #region Add entities
        /// <summary>
        /// Sets up a <see cref="WorldEntity"/> for rendering via a <see cref="Body"/>
        /// </summary>
        /// <param name="entity">The <see cref="WorldEntity"/> to be displayed</param>
        protected virtual void AddEntity(WorldEntity entity)
        {
            var sun = entity as World.Entities.Sun;
            if (sun != null)
            {
                #region Sun
                // Create the sun  and add it to the scene
                VertexGroup sunFace = GetSun(sun);
                Scene.Bodies.Add(sunFace);
                assoc.Add(entity, sunFace);
                #endregion
            }
            else
            {
                var body = entity as WorldBody;
                if (body != null)
                {
                    #region Other bodies
                    var meshBody = GetBody(body);
                    Scene.Bodies.Add(meshBody);
                    assoc.Add(entity, meshBody);

                    var atmosphereBody = entity as World.Entities.IAtmosphere;
                    if (atmosphereBody != null && atmosphereBody.AtmosphereInfo != null)
                    {
                        Model meshAtmosphere = GetAtmosphere(body, atmosphereBody.AtmosphereInfo, meshBody);
                        Scene.Bodies.Add(meshAtmosphere);
                        assoc.Add(entity, meshAtmosphere);
                    }

                    var planetoidBody = entity as World.Entities.Planetoid;
                    if (planetoidBody != null && planetoidBody.Rings != null)
                    {
                        var meshRing = GetRings(planetoidBody, meshBody);
                        Scene.Bodies.Add(meshRing);
                        assoc.Add(entity, meshRing);
                    }
                    #endregion
                }
                else throw new ArgumentException("Unkown entity type", "entity");
            }

            UpdateEntity(entity);
            entity.RenderPropertyChanged += UpdateEntity;
        }

        #region Sun
        protected virtual VertexGroup GetSun(World.Entities.Sun body)
        {
            VertexGroup sunFace = VertexGroup.Quad(engine, 1,
                XMaterial.FromCache(engine, "Sun/" + body.Name + ".png", null, null, null, "Sun/" + body.Name + "_glow.png"));
            sunFace.Billboard = BillboardMode.Spherical;
            sunFace.Position = body.ScaledPosition;
            sunFace.Alpha = 256;
            sunFace.Name = body.Name;

            // Resize the sun based on the distance to the camera, to increase the effective visiblity range
            MainView.PreRender += delegate
            {
                float sunDistance = ((Vector3)(sunFace.Position - Camera.Position)).Length();
                sunFace.SetScale(body.ScaledRadius * (float)Math.Pow(sunDistance, Settings.Current.Graphics.SunPower) / 48);
            };

            return sunFace;
        }
        #endregion

        #region Normal body
        protected virtual Model GetBody(WorldBody body)
        {
            var classableInterface = body as World.Entities.IClassable;
            string id = (classableInterface == null) ? body.Name : classableInterface.ClassName;

            // Find the surface map (bump map if available) for this body
            string filename;
            if (body is World.Entities.Moon)
                filename = "Moons/" + id;
            else if (body is World.Entities.Planetoid)
                filename = "Planetoids/" + id;
            else throw new InvalidOperationException("Unknown body type!");
            string filenameBump = filename + "_bump.jpg";
            string filenameGlow = filename + "_glow.jpg";
            filename += ".jpg";

            // Build the surface material
            Model meshBody = Model.Sphere(engine, filename, 1, 96, 96);
            if (ContentManager.FileExists("Textures", filenameBump, true))
                meshBody.Materials[0].NormalMap = XTexture.FromCache(engine, filenameBump);
            if (ContentManager.FileExists("Textures", filenameGlow, true))
            {
                meshBody.Materials[0].Glow = Color.White;
                meshBody.Materials[0].GlowMap = XTexture.FromCache(engine, filenameGlow);
            }

            // Set further parameters
            meshBody.Position = body.ScaledPosition;
            meshBody.Rotation = body.Rotation;
            if (lighting) meshBody.SurfaceEffect = SurfaceEffect.Shader;

            meshBody.Name = body.Name;
            return meshBody;
        }
        #endregion

        #region Atmosphere
        protected virtual Model GetAtmosphere(WorldBody body, World.Atmosphere atmosphere, Model meshBody)
        {
            Model meshAtmosphere;
            if (atmosphere is World.AtmospherePlain)
            {
                var atmospherePlain = (World.AtmospherePlain)atmosphere;

                // Generate an atmosphere for this body
                meshAtmosphere = Model.Sphere(engine, null, 1, 64, 64);
                meshAtmosphere.Alpha = atmospherePlain.Transparency;
            }
            else if (atmosphere is World.AtmosphereCloud)
            {
                // Load the cload map for this body
                string filename = "Atmopsheres/" + body.Name + "-Clouds.png";
                meshAtmosphere = Model.Sphere(engine, filename, 1, 96, 96);
                meshAtmosphere.Alpha = 256;

                // Cloud rotation
                meshAtmosphere.PreRender += delegate { meshAtmosphere.Rotation = ((World.AtmosphereCloud)atmosphere).Rotation; };
            }
            else throw new InvalidOperationException("Invalid type");

            if (lighting) meshAtmosphere.SurfaceEffect = SurfaceEffect.Shader;
            meshAtmosphere.Position = meshBody.Position;
            meshAtmosphere.Name = body.Name + " atmosphere";
            return meshAtmosphere;
        }
        #endregion

        #region Rings
        protected virtual Model GetRings(World.Entities.Planetoid planetoid, Model meshBody)
        {
            // Find the ring map for this planet
            string filename;
            if (planetoid is World.Entities.Moon)
                filename = "Moons/" + planetoid.Name + "-Rings.png";
            else
                filename = "Planetoids/" + planetoid.Name + "-Rings.png";

            Model discModel = Model.Disc(engine, filename, planetoid.Rings.ScaledInnerRadius, planetoid.Rings.ScaledOuterRadius, 0.1f, 100);
            discModel.Alpha = 256;
            if (lighting) discModel.SurfaceEffect = SurfaceEffect.Shader;

            discModel.Position = meshBody.Position;
            discModel.VisibilityDistance = planetoid.Rings.ScaledOuterRadius * Settings.Current.Graphics.VisibilityFactor;
            discModel.Name = planetoid.Name + " rings";
            return discModel;
        }
        #endregion

        #endregion

        #region Remove entities
        protected virtual void RemoveEntity(WorldEntity entity)
        {
            foreach (Body renderable in assoc[entity])
                Scene.Bodies.Remove(renderable);
            assoc.Remove(entity);
            entity.RenderPropertyChanged -= UpdateEntity;
        }
        #endregion

        #region Update entities
        protected virtual void UpdateEntity(WorldEntity entity)
        {
            // Get all associated engine bodies from the association list
            foreach (Body renderable in assoc[entity])
            {
                renderable.Position = entity.ScaledPosition;

                var body = entity as WorldBody;
                if (body != null)
                {
                    #region Scaling
                    if (body is World.Entities.Sun)
                    {
                        // Sun is permanently auto-rescaled)
                    }
                    else if (renderable.Name.Contains("rings"))
                    {
                        // Rings cannot be easily rescaled
                    }
                    else if (renderable.Name.Contains("atmosphere"))
                    {
                        // Atmosphere is an additional layer around the existing body
                        var atmosphereBody = body as World.Entities.IAtmosphere;
                        if (atmosphereBody != null && atmosphereBody.AtmosphereInfo != null)
                            renderable.SetScale(body.ScaledRadius + atmosphereBody.AtmosphereInfo.ScaledThickness);
                    }
                    else
                    {
                        renderable.SetScale(body.ScaledRadius);
                    }

                    if (body.ScaledRadius > 0)
                        renderable.VisibilityDistance = body.ScaledRadius * Settings.Current.Graphics.VisibilityFactor;
                    #endregion

                    renderable.Rotation = body.Rotation;
                }
            }

            if (entity == cameraTarget)
                UpdateCamera(Camera);
        }
        #endregion


        #region Dispose
        /// <summary>
        /// Releases unmanaged resources for this presenter
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Auto-dispose destructor
        ~Presenter()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            foreach (View view in views)
                view.Dispose();
        }
        #endregion
    }
}
