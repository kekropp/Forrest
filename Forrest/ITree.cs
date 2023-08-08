using System.Collections;
using System.Linq.Expressions;

namespace Forrest
{
    // Describes a generic, tree-like structure
    public interface ITree<T>
    {
        public void AddChildOf(T parent, T child);
        public bool IsChildOf(T parent, T child);
        public void Remove(T value, bool removeDescendants = false);
        public bool IsEmpty { get; }
        public bool Contains(T value);
        public IEnumerable<T> TraverseToRoot(T value);
        public IEnumerable<T> Flatten(TraversalMode mode = TraversalMode.BreadthFirst);
        public IEnumerable<T> Flatten(T value, TraversalMode mode = TraversalMode.BreadthFirst);
        public void Move(T value, T newParent);
        public int LevelOf(T value);
        public int RootCount { get; }
        public int NodeCount { get; }
        public int Depth { get; }
    }
}