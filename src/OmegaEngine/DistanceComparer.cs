using System.Collections.Generic;

namespace OmegaEngine;

/// <summary>
/// Compares the distances of objects to fixed root position.
/// </summary>
/// <param name="rootPosition">The root position to compare other objects to.</param>
/// <param name="inverse"><c>false</c> to sort from closest to furthest, <c>true</c> to sort from furthest to closest.</param>
public class DistanceComparer(IPositionable rootPosition, bool inverse = false) : Comparer<IPositionable>
{
    /// <inheritdoc />
    public override int Compare(IPositionable x, IPositionable y)
    {
        int compare = (x.Position - rootPosition.Position).Length().CompareTo((y.Position - rootPosition.Position).Length());
        return inverse ? -compare : compare;
    }
}
