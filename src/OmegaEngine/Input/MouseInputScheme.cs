using static OmegaEngine.Input.NavigationAxis;

namespace OmegaEngine.Input;

/// <summary>
/// Controls which mouse button does what.
/// </summary>
/// <param name="LeftDrag">How to handle dragging the mouse while the left button is pressed.</param>
/// <param name="RightDrag">How to handle dragging the mouse while the right button is pressed.</param>
/// <param name="MiddleDrag">How to handle dragging the mouse while the middle button or both the left and the right button are pressed.</param>
public record MouseInputScheme(InputAction? LeftDrag = null, InputAction? RightDrag = null, InputAction? MiddleDrag = null)
{
    /// <summary>
    /// Scene navigation with full six degrees of freedom.
    /// Left button for XY panning, right button for rotation, middle button for roll and zoom.
    /// Designed for general-purpose scene inspection.
    /// </summary>
    public static MouseInputScheme Scene => new(
        LeftDrag: new Navigation(X: TranslationX, Y: TranslationY),
        RightDrag: new Navigation(X: RotationX, Y: RotationY),
        MiddleDrag: new Navigation(X: RotationZ, Y: TranslationZ));

    /// <summary>
    /// Orbit around a fixed target.
    /// Left button for rotation, right button for roll and zoom.
    /// Suitable for inspecting a single object or focal point.
    /// </summary>
    public static MouseInputScheme Orbit => new(
        LeftDrag: new Navigation(X: RotationX, Y: RotationY),
        RightDrag: new Navigation(X: RotationZ, Y: TranslationZ));

    /// <summary>
    /// Focuses on movement constrained to a plane.
    /// Left button for area selection, right button for XY panning, middle button for rotation and zoom.
    /// Ideal for top-down or strategy-style navigation.
    /// </summary>
    public static MouseInputScheme Planar => new(
        LeftDrag: new AreaSelection(),
        RightDrag: new Navigation(X: TranslationX, Y: TranslationY),
        MiddleDrag: new Navigation(X: RotationX, Y: TranslationZ));

    /// <summary>
    /// Enables free-look navigation.
    /// Left button for rotation, right button for XZ movement.
    /// Suitable for first-person navigation scenarios.
    /// </summary>
    public static MouseInputScheme FreeLook { get; }
        = new(
            LeftDrag: new Navigation(X: RotationX, Y: RotationY),
            RightDrag: new Navigation(X: TranslationX, Y: TranslationZ));
}
