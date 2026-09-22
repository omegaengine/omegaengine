/*
 * Renders Frame of Reference maps to image files without a human at the keyboard.
 * See ..\SKILL.md for why each step below is necessary.
 */

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using AlphaFramework.World.Positionables;
using FrameOfReference.Presentation;
using FrameOfReference.World;
using FrameOfReference.World.Templates;
using OmegaEngine;
using OmegaEngine.Foundation.Geometry;
using OmegaEngine.Foundation.Light;
using OmegaEngine.Foundation.Storage;
using OmegaEngine.Graphics.Cameras;
using SlimDX.Direct3D9;
using EngineView = OmegaEngine.Graphics.View; // System.Windows.Forms.View collides with this

namespace Shots;

internal static class Program
{
    [STAThread]
    private static int Main(string[] args)
    {
        Options options;
        try
        {
            options = Options.Parse(args);
        }
        catch (ArgumentException ex)
        {
            Console.Error.WriteLine(ex.Message);
            Console.Error.WriteLine(Options.Usage);
            return 1;
        }

        // Without this, members of SlimDX vector/color structs silently deserialize to zero,
        // because the maps store them as XML attributes and the plain serializer expects elements.
        XmlStorageConfig.Apply();

        // Universe deserialization resolves entity and terrain templates by name as it goes
        EntityTemplate.LoadAll();
        TerrainTemplate.LoadAll();

        using var form = new HiddenForm(options.Size);
        form.Show(); // Engine.Render does nothing at all while the target is invisible
        Application.DoEvents();

        using var engine = new Engine(form, new EngineConfig {TargetSize = form.ClientSize});
        engine.Anisotropic = options.Anisotropic;
        engine.Effects.PostScreenEffects = options.PostScreenEffects;
        if (options.WaterEffects is {} waterEffects) engine.Effects.WaterEffects = waterEffects;

        Directory.CreateDirectory(options.OutputDir);

        foreach (float phase in options.LightPhases)
            Capture(engine, options, phase);

        return 0;
    }

    private static void Capture(Engine engine, Options options, float lightPhase)
    {
        if (options.Seed is {} seed) SeedRandom(seed);

        var universe = Universe.FromContent($"{options.Map}.FrameOfReferenceMap");
        universe.LightPhase = lightPhase;

        var (target, radius) = ResolveCamera(options, universe);
        var presenter = new FixedCameraPresenter(engine, universe, target, options.Yaw, options.Pitch, radius);
        try
        {
            presenter.Initialize();
            presenter.HookIn();

            // Terrain, shaders and textures load lazily over the first frames, so an
            // immediate screenshot catches a half-built scene
            for (int i = 0; i < options.WarmupFrames; i++)
            {
                engine.Render(elapsedGameTime: 0, noPresent: true);
                Application.DoEvents();
            }

            string name = options.Name ?? $"{options.Map}-phase{lightPhase:0.##}";
            string file = Path.Combine(options.OutputDir, $"{name}.jpg");
            engine.Screenshot(file);
            Console.WriteLine($"{file}  camera target={target} radius={radius:0}");

            if (options.DumpRenderTargets) DumpRenderTargets(presenter, options.OutputDir, name);
        }
        finally
        {
            // Engine.Dispose forces a blocking finalizer run, and an undisposed EngineElement
            // throws from its finalizer in Debug builds, which takes the whole process down
            presenter.HookOut();
            presenter.Dispose();
            engine.Cache.Clean();
        }
    }

    /// <summary>
    /// Replaces the random number generator behind <c>RandomUtils</c> with a seeded one, so particle
    /// systems spawn identically across runs and builds. On .NET Framework it is a lazily created
    /// thread-static in NanoByte.Common that has no public setter.
    /// </summary>
    private static void SeedRandom(int seed)
    {
        var field = typeof(NanoByte.Common.RandomShared).GetField("_local", BindingFlags.NonPublic | BindingFlags.Static)
                 ?? throw new InvalidOperationException("RandomShared no longer has a '_local' field; update the harness");
        field.SetValue(null, new Random(seed));
    }

    /// <summary>
    /// Writes the child views' textures (water reflection/refraction, glow, ...) to disk. Useful for
    /// telling apart "this intermediate is wrong" from "the shader consuming it is wrong".
    /// </summary>
    private static void DumpRenderTargets(FixedCameraPresenter presenter, string outputDir, string name)
    {
        foreach (var child in presenter.View.ChildViews)
        {
            string childName = (child.Name ?? "unnamed").Replace(' ', '_');
            string file = Path.Combine(outputDir, $"{name}-rt-{childName}.png");
            BaseTexture.ToFile(child.GetRenderTarget().Texture, file, ImageFileFormat.Png);
            Console.WriteLine($"  {file}");
        }
    }

    private static (DoubleVector3 target, float radius) ResolveCamera(Options options, Universe universe)
    {
        if (options.Target is {} explicitTarget)
            return (explicitTarget, options.Radius ?? 2200);

        if (options.AimAtWater)
        {
            // Map X = Engine +X, Map Y = Engine -Z, Map height = Engine +Y
            var water = universe.Positionables.OfType<Water>()
                                .OrderByDescending(x => x.Size.X * x.Size.Y)
                                .FirstOrDefault()
                     ?? throw new InvalidOperationException($"Map '{options.Map}' contains no water");
            var center = new DoubleVector3(
                water.EnginePosition.X + water.Size.X / 2,
                water.Height + 20,
                water.EnginePosition.Z - water.Size.Y / 2);
            return (center, options.Radius ?? Math.Max(water.Size.X, water.Size.Y) * 0.45f);
        }

        var terrainCenter = new DoubleVector3(universe.Terrain.Center.X, 100, -universe.Terrain.Center.Y);
        return (terrainCenter, options.Radius ?? 2200);
    }

    /// <summary>A window that exists only to own the Direct3D swap chain.</summary>
    private sealed class HiddenForm : Form
    {
        public HiddenForm(Size size)
        {
            ClientSize = size;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;
            Location = new Point(-32000, -32000); // Off-screen, but still "visible" as far as the engine is concerned
        }

        protected override bool ShowWithoutActivation => true;
    }

    /// <summary>
    /// A presenter with a camera that does not move. The stock presenters animate their cameras off a
    /// wall clock, which makes two runs frame differently and ruins before/after comparison.
    /// </summary>
    private sealed class FixedCameraPresenter : Presenter
    {
        public FixedCameraPresenter(Engine engine, Universe universe, DoubleVector3 target, double yaw, double pitch, float radius)
            : base(engine, universe)
        {
            View = new EngineView(Scene, new ArcballCamera
            {
                Name = "Shot",
                Target = target,
                Radius = radius,
                Yaw = yaw,
                Pitch = pitch,
                FieldOfView = 45,
                NearClip = 10,
                FarClip = universe.Fog ? universe.FogDistance : 1e+6f
            })
            {
                Name = "Shot",
                Lighting = true,
                BackgroundColor = universe.FogColor
            };
        }
    }

    private sealed class Options
    {
        public const string Usage = """
            Usage: Shots.exe --out <dir> [options]
              --out <dir>            Where to write the images (required)
              --map <name>           Map base name (default: Mountains)
              --phase <n>[,<n>...]   Light phase 0=dawn 1=noon 2=dusk 3=midnight (default: 1)
              --name <name>          Base file name; defaults to "<map>-phase<n>"
              --target <x,y,z>       Camera target in engine space
              --water                Aim at the map's largest water plane instead
              --yaw <deg>            Default: 135
              --pitch <deg>          Default: 18
              --radius <units>       Distance from the target
              --size <w>x<h>         Render size (default: 1024x768)
              --warmup <n>           Frames to render before capturing (default: 12)
              --water-effects <n>    None | RefractionOnly | ReflectTerrain | ReflectAll
              --no-post              Disable post-screen effects
              --no-aniso             Disable anisotropic filtering
              --dump-render-targets  Also write the child views' textures as PNG
              --seed <n>             Seed the random numbers (particle systems) for repeatable images
            """;

        public string OutputDir = "";
        public string Map = "Mountains";
        public IReadOnlyList<float> LightPhases = [1];
        public string? Name;
        public DoubleVector3? Target;
        public bool AimAtWater;
        public double Yaw = 135, Pitch = 18;
        public float? Radius;
        public Size Size = new(1024, 768);
        public int WarmupFrames = 12;
        public WaterEffectsType? WaterEffects;
        public bool PostScreenEffects = true;
        public bool Anisotropic = true;
        public bool DumpRenderTargets;
        public int? Seed;

        public static Options Parse(string[] args)
        {
            var options = new Options();
            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "--out": options.OutputDir = Next(); break;
                    case "--map": options.Map = Next(); break;
                    case "--phase": options.LightPhases = Next().Split(',').Select(ParseFloat).ToList(); break;
                    case "--name": options.Name = Next(); break;
                    case "--target": options.Target = ParseVector(Next()); break;
                    case "--water": options.AimAtWater = true; break;
                    case "--yaw": options.Yaw = ParseFloat(Next()); break;
                    case "--pitch": options.Pitch = ParseFloat(Next()); break;
                    case "--radius": options.Radius = ParseFloat(Next()); break;
                    case "--size": options.Size = ParseSize(Next()); break;
                    case "--warmup": options.WarmupFrames = int.Parse(Next(), CultureInfo.InvariantCulture); break;
                    case "--water-effects": options.WaterEffects = (WaterEffectsType)Enum.Parse(typeof(WaterEffectsType), Next(), ignoreCase: true); break;
                    case "--no-post": options.PostScreenEffects = false; break;
                    case "--no-aniso": options.Anisotropic = false; break;
                    case "--dump-render-targets": options.DumpRenderTargets = true; break;
                    case "--seed": options.Seed = int.Parse(Next(), CultureInfo.InvariantCulture); break;
                    default: throw new ArgumentException($"Unknown argument: {args[i]}");
                }

                string Next()
                    => ++i < args.Length ? args[i] : throw new ArgumentException($"Missing value after {args[i - 1]}");
            }

            if (string.IsNullOrEmpty(options.OutputDir)) throw new ArgumentException("--out is required");
            return options;
        }

        private static float ParseFloat(string value) => float.Parse(value, CultureInfo.InvariantCulture);

        private static DoubleVector3 ParseVector(string value)
        {
            var parts = value.Split(',');
            if (parts.Length != 3) throw new ArgumentException($"Expected x,y,z but got '{value}'");
            return new(ParseFloat(parts[0]), ParseFloat(parts[1]), ParseFloat(parts[2]));
        }

        private static Size ParseSize(string value)
        {
            var parts = value.Split('x');
            if (parts.Length != 2) throw new ArgumentException($"Expected <width>x<height> but got '{value}'");
            return new(int.Parse(parts[0], CultureInfo.InvariantCulture), int.Parse(parts[1], CultureInfo.InvariantCulture));
        }
    }
}
