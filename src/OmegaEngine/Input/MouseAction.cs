namespace OmegaEngine.Input;

/// <summary>
/// An action bound to a mouse input.
/// </summary>
public abstract record MouseAction;

/// <summary>
/// A navigation action bound to a mouse input.
/// </summary>
/// <param name="X">The action to apply for mouse movement along the X axis.</param>
/// <param name="Y">The action to apply for mouse movement along the Y axis.</param>
/// <param name="ViewportScaling">Whether to scale the input based on the size of the input area / viewport.</param>
/// <param name="CaptureCursor">Whether to capture and hide the mouse cursor.</param>
public record MouseNavigation(MouseNavigationAxis X, MouseNavigationAxis Y, bool ViewportScaling = false, bool CaptureCursor = true) : MouseAction;

/// <summary>
/// An axis / degree-of-freedom manipulatable by mouse.
/// </summary>
public enum MouseNavigationAxis
{
    TranslationX, TranslationY, TranslationZ,
    RotationX, RotationY, RotationZ
}

/// <summary>
/// A rectangular selection action bound to a mouse input.
/// </summary>
public record MouseAreaSelection : MouseAction;
