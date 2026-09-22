using System;
using System.Collections;
using System.Collections.Generic;

namespace OmegaEngine;

/// <summary>
/// A collection of <see cref="EngineElement"/>s.
/// Applies the composite pattern: automatically handles <see cref="EngineElement.Engine"/> setting and <see cref="EngineElement.Dispose"/> calling.
/// </summary>
/// <typeparam name="T">The specific type of <see cref="EngineElement"/> contained in the collection.</typeparam>
internal class EngineElementCollection<T> : EngineElement, ICollection<T>
    where T : EngineElement
{
    private readonly List<T> _innerList = [];

    /// <summary>
    /// Returns a non-allocating enumerator, for traversals that run every frame.
    /// </summary>
    public List<T>.Enumerator GetEnumerator() => _innerList.GetEnumerator();

    IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void Add(T item)
    {
        #region Sanity checks
        if (item == null) throw new ArgumentNullException(nameof(item));
        #endregion

        // Fail on an incompatible or disposed element before OnAdding changes any state
        if (IsEngineSet) item.Engine = Engine;

        OnAdding(item);
        RegisterChild(item);
        _innerList.Add(item);
    }

    public void Clear()
    {
        var removed = _innerList.ToArray();
        _innerList.Clear();
        foreach (var element in removed)
        {
            UnregisterChild(element);
            OnRemoved(element);
        }
    }

    public bool Contains(T item) => _innerList.Contains(item);

    public void CopyTo(T[] array, int arrayIndex) => _innerList.CopyTo(array, arrayIndex);

    public bool Remove(T item)
    {
        if (!_innerList.Remove(item)) return false;
        UnregisterChild(item);
        OnRemoved(item);
        return true;
    }

    public int Count => _innerList.Count;

    public bool IsReadOnly => false;

    /// <summary>
    /// Hook that is called before an element is inserted into the collection.
    /// </summary>
    /// <param name="element">The element that is about to be added.</param>
    protected virtual void OnAdding(T element)
    {}

    /// <summary>
    /// Hook that is called after an element was removed from the collection (also by <see cref="Clear"/>).
    /// </summary>
    /// <param name="element">The element that was removed.</param>
    protected virtual void OnRemoved(T element)
    {}
}
