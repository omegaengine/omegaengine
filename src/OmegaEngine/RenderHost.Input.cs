using OmegaEngine.Input;

namespace OmegaEngine;

partial class RenderHost
{
    /// <summary>
    /// A default <see cref="Input.KeyboardInputProvider"/> hooked up to the <see cref="Form"/>.
    /// </summary>
    public KeyboardInputProvider KeyboardInputProvider { get; }

    /// <summary>
    /// A default <see cref="Input.MouseInputProvider"/> hooked up to the <see cref="Form"/>.
    /// </summary>
    public MouseInputProvider MouseInputProvider { get; }

    /// <summary>
    /// A default <see cref="Input.TouchInputProvider"/> hooked up to the <see cref="Form"/>.
    /// </summary>
    public TouchInputProvider TouchInputProvider { get; }

    /// <summary>
    /// Calls <see cref="InputProvider.AddReceiver"/> for all default <see cref="InputProvider"/>s.
    /// </summary>
    /// <param name="receiver">The object to receive the commands.</param>
    public void AddInputReceiver(IInputReceiver receiver)
    {
        KeyboardInputProvider.AddReceiver(receiver);
        MouseInputProvider.AddReceiver(receiver);
        TouchInputProvider.AddReceiver(receiver);
    }

    /// <summary>
    /// Calls <see cref="InputProvider.RemoveReceiver"/> for all default <see cref="InputProvider"/>s.
    /// </summary>
    /// <param name="receiver">The object to no longer receive the commands.</param>
    public void RemoveInputReceiver(IInputReceiver receiver)
    {
        KeyboardInputProvider.RemoveReceiver(receiver);
        MouseInputProvider.RemoveReceiver(receiver);
        TouchInputProvider.RemoveReceiver(receiver);
    }
}
