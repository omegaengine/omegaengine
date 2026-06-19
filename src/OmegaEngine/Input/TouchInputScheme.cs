using static OmegaEngine.Input.NavigationAxis;

namespace OmegaEngine.Input;

/// <summary>
/// Controls which touch gesture does what.
/// </summary>
/// <param name="Pan">How to handle dragging one or more contacts (panning).</param>
/// <param name="Pinch">Which axis the pinch-to-zoom (scale) gesture drives.</param>
/// <param name="Twist">Which axis the two-finger rotation (twist) gesture drives.</param>
public record TouchInputScheme(Navigation? Pan = null, NavigationAxis? Pinch = null, NavigationAxis? Twist = null)
{
    /// <summary>
    /// Pan translates XY, pinch zooms, twist rolls.
    /// Designed for general-purpose scene inspection.
    /// </summary>
    public static TouchInputScheme Scene => new(
        Pan: new Navigation(X: TranslationX, Y: TranslationY),
        Pinch: TranslationZ,
        Twist: RotationZ);

    /// <summary>
    /// Rotation around a fixed target.
    /// Pan rotates, pinch zooms, twist rolls.
    /// </summary>
    public static TouchInputScheme Orbit => new(
        Pan: new Navigation(X: RotationX, Y: RotationY),
        Pinch: TranslationZ,
        Twist: RotationZ);

    /// <summary>
    /// Focuses on movement constrained to a plane.
    /// Pan translates XY, pinch zooms, twist rotates (yaw).
    /// Ideal for top-down or strategy-style navigation.
    /// </summary>
    public static TouchInputScheme Planar => new(
        Pan: new Navigation(X: TranslationX, Y: TranslationY),
        Pinch: TranslationZ,
        Twist: RotationX);

    /// <summary>
    /// Enables free-look navigation.
    /// Pan rotates, pinch moves forward, twist rolls.
    /// Suitable for first-person navigation scenarios.
    /// </summary>
    public static TouchInputScheme FreeLook { get; }
        = new(
            Pan: new Navigation(X: RotationX, Y: RotationY),
            Pinch: TranslationZ,
            Twist: RotationZ);
}
