using NanoByte.Common;
using OmegaEngine;
using OmegaEngine.Assets;
using OmegaEngine.Audio;
using SoundComponent = AlphaFramework.World.Components.Sound;

namespace AlphaFramework.Presentation;

/// <summary>
/// Converts <see cref="AlphaFramework.World.Components.Sound"/> components into <see cref="Sound3D"/>s.
/// </summary>
public static class SoundComponentPresentation
{
    /// <summary>
    /// Creates a <see cref="Sound3D"/> from a <see cref="AlphaFramework.World.Components.Sound"/> component.
    /// </summary>
    /// <param name="component">The <see cref="AlphaFramework.World.Components.Sound"/> component to visualize using the <see cref="Engine"/>.</param>
    /// <param name="engine">The engine to use for loading and playing the sound.</param>
    /// <returns>The presentation; <c>null</c> if the component configuration is incomplete.</returns>
    public static Sound3D? ToPresentation(this SoundComponent component, Engine engine)
        => XSound.Get(engine, component.Filename)
                ?.To(asset => new Sound3D(asset)
                  {
                      Engine = engine,
                      Volume = component.Volume,
                      Attenuation = component.Attenuation
                  });
}
