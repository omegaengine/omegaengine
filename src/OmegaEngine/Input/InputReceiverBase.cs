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
    public abstract void PerspectiveChange(DoubleVector3 translation, DoubleVector3 rotation);

    /// <inheritdoc />
    public virtual void Hover(Point target)
    {}

    /// <inheritdoc />
    public virtual void AreaSelection(Rectangle area, bool accumulate, bool done)
    {}

    /// <inheritdoc />
    public virtual void Click(MouseEventArgs e, bool accumulate)
    {}

    /// <inheritdoc />
    public virtual void DoubleClick(MouseEventArgs e)
    {}
}
