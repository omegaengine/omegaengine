namespace OmegaEngine.Input;

/// <summary>
/// A navigation action mapping two input axes to degrees-of-freedom.
/// </summary>
/// <param name="X">The action to apply for movement along the X axis.</param>
/// <param name="Y">The action to apply for movement along the Y axis.</param>
/// <param name="ViewportScaling">Whether to scale the input based on the size of the input area / viewport.</param>
/// <param name="CaptureCursor">Whether to capture and hide the mouse cursor. Only relevant for mouse input.</param>
public record Navigation(NavigationAxis X, NavigationAxis Y, bool ViewportScaling = false, bool CaptureCursor = true) : InputAction;

/// <summary>
/// An axis / degree-of-freedom manipulatable by input.
/// </summary>
public enum NavigationAxis
{
    TranslationX, TranslationY, TranslationZ,
    RotationX, RotationY, RotationZ
}
