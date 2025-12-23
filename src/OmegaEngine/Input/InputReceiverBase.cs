using System.Drawing;
using System.Windows.Forms;
using OmegaEngine.Foundation.Geometry;

namespace OmegaEngine.Input;

/// <summary>
/// Base class to simplify implementing <see cref="IInputReceiver"/>
/// </summary>
public abstract class InputReceiverBase : IInputReceiver
{
    /// <inheritdoc />
    public abstract void Navigate(DoubleVector3 translation = default, DoubleVector3 rotation = default);

    /// <inheritdoc />
    public virtual void Hover(Point target)
    {}

    /// <inheritdoc />
    public virtual void AreaSelection(Rectangle area, bool done, bool accumulate = false)
    {}

    /// <inheritdoc />
    public virtual void Click(MouseEventArgs e, bool accumulate = false)
    {}

    /// <inheritdoc />
    public virtual void DoubleClick(MouseEventArgs e)
    {}
}
