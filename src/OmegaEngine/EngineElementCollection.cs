using System.Collections;
using System.Collections.Generic;

namespace OmegaEngine
{
    /// <summary>
    /// A collection of <see cref="EngineElement"/>s.
    /// Applies the composite pattern: automatically handles <see cref="EngineElement.Engine"/> setting and <see cref="EngineElement.Dispose"/> calling.
    /// </summary>
    /// <typeparam name="T">The specific type of <see cref="EngineElement"/> contained in the collection.</typeparam>
    internal sealed class EngineElementCollection<T> : EngineElement, ICollection<T>
        where T : EngineElement
    {
        private readonly List<T> _innerList = new List<T>();

        protected override void OnEngineSet()
        {
            base.OnEngineSet();
            foreach (var element in _innerList)
                element.Engine = Engine;
        }

        protected override void OnDispose()
        {
            try
            {
                foreach (var element in _innerList)
                    element.Dispose();
            }
            finally
            {
                base.OnDispose();
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _innerList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(T item)
        {
            if (IsEngineSet) item.Engine = Engine;
            _innerList.Add(item);
        }

        public void Clear()
        {
            _innerList.Clear();
        }

        public bool Contains(T item)
        {
            return _innerList.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _innerList.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            return _innerList.Remove(item);
        }

        public int Count { get { return _innerList.Count; } }

        public bool IsReadOnly { get { return false; } }
    }
}
