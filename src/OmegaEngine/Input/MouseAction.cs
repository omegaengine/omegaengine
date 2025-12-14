namespace OmegaEngine.Input;

/// <summary>
/// An action bound to a mouse input.
/// </summary>
public abstract record MouseAction;

/// <summary>
/// A navigation action bound to a mouse input.
/// </summary>
public record MouseNavigation(MouseNavigationAxis X, MouseNavigationAxis Y, bool CaptureCursor = true) : MouseAction;

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
