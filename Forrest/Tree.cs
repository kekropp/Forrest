using System.Collections;
using System.Linq.Expressions;

namespace Forrest;

public abstract class Tree
{
    public static Tree<T> Create<T>(IEnumerable<T> nodes, Expression<Func<T, IEnumerable<T>?>> childrenSelector)
    {
        return new Tree<T>(nodes, childrenSelector);
    }

    public static Tree<T> Create<T>(IEnumerable<T> nodes, Expression<Func<T, T?>> parentSelector)
    {
        return new Tree<T>(nodes, parentSelector);
    }

    public static Tree<T> Create<T>(IEnumerable<T> nodes)
    {
        // Check if there is a property of type T?
        var type = typeof(T);
        var properties = type.GetProperties();
        var parentProperty = properties.FirstOrDefault(p => p.PropertyType == typeof(T?));
        if (parentProperty is not null)
        {
            var parameter = Expression.Parameter(type);
            var property = Expression.Property(parameter, parentProperty);
            var lambda = Expression.Lambda<Func<T, T?>>(property, parameter);
            return new Tree<T>(nodes, lambda);
        }

        // find any ICollection with a generic argument of type T
        var childrenProperty = properties.FirstOrDefault(p => p.PropertyType.IsGenericType &&
                                                              typeof(IEnumerable).IsAssignableFrom(p.PropertyType) &&
                                                              p.PropertyType.GetGenericArguments()[0] == typeof(T));
        if (childrenProperty is not null)
        {
            var parameter = Expression.Parameter(type);
            var property = Expression.Property(parameter, childrenProperty);
            var lambda = Expression.Lambda<Func<T, IEnumerable<T>?>>(property, parameter);
            return new Tree<T>(nodes, lambda);
        }

        throw new ArgumentException("No parent or children property found", nameof(nodes));
    }
}

public class Tree<T> : ITree<T>
{
    private readonly List<TreeNode<T>> _roots = new();

    public Tree(IEnumerable<T> nodes, Expression<Func<T, IEnumerable<T>?>> childrenSelector)
    {
        var compiled = childrenSelector.Compile();
        // start building the tree by creating a TreeNode for each node
        var nodeMap = nodes.ToDictionary(n => n, n => new TreeNode<T> { Value = n });

        // set the parent of each node
        foreach (var node in nodeMap.Values)
        {
            var children = compiled(node.Value);
            foreach (var child in children ?? Enumerable.Empty<T>())
            {
                nodeMap[child].Parent = node;
            }
        }

        // add each node to its parent's children
        var valueSet = new HashSet<T>();
        foreach (var node in nodeMap.Values)
        {
            if (!valueSet.Add(node.Value))
            {
                throw new ArgumentException("Duplicate value in tree", nameof(nodes));
            }

            if (node.Parent is null)
            {
                _roots.Add(node);
            }
            else
            {
                node.Parent.Children.Add(node);
            }
        }
    }

    public Tree(IEnumerable<T> nodes, Expression<Func<T, T?>> parentSelector)
    {
        var compiled = parentSelector.Compile();
        // start building the tree by creating a TreeNode for each node
        var nodeMap = nodes.ToDictionary(n => n, n => new TreeNode<T> { Value = n });
        // set the parent of each node
        foreach (var node in nodeMap.Values)
        {
            var parent = compiled(node.Value);
            if (parent is not null)
            {
                node.Parent = nodeMap[parent];
            }
        }

        // add each node to its parent's children
        var valueSet = new HashSet<T>();
        foreach (var node in nodeMap.Values)
        {
            if (!valueSet.Add(node.Value))
            {
                throw new ArgumentException("Duplicate value in tree", nameof(nodes));
            }

            if (node.Parent is null)
            {
                _roots.Add(node);
            }
            else
            {
                node.Parent.Children.Add(node);
            }
        }
    }

    private TreeNode<T>? FindNode(T value)
    {
        var queue = new Queue<TreeNode<T>>(_roots);
        while (queue.Count > 0)
        {
            var node = queue.Dequeue();
            if (node.Value!.Equals(value))
            {
                return node;
            }

            foreach (var child in node.Children)
            {
                queue.Enqueue(child);
            }
        }

        return null;
    }

    public void AddChildOf(T parent, T child)
    {
        var parentNode = FindNode(parent);
        if (parentNode is null) throw new ArgumentException("Parent not found in tree", nameof(parent));
        parentNode.Children.Add(new TreeNode<T> { Value = child, Parent = parentNode });
    }

    public bool IsChildOf(T parent, T child)
    {
        var parentNode = FindNode(parent);
        if (parentNode is null) throw new ArgumentException("Parent not found in tree", nameof(parent));
        var childNode = FindNode(child);
        if (childNode is null) throw new ArgumentException("Child not found in tree", nameof(child));
        return TraverseToRoot(childNode.Value).Any(n => n?.Equals(parent) ?? false);
    }

    public void Remove(T value, bool removeDescendants = false)
    {
        var node = FindNode(value);
        if (node is null) throw new ArgumentException("Value not found in tree", nameof(value));

        if (removeDescendants)
        {
            node.Parent?.Children.Remove(node);
        }
        else
        {
            var children = node.Children;
            foreach (var child in children)
            {
                child.Parent = node.Parent;
            }

            node.Parent?.Children.Remove(node);
            node.Parent?.Children.AddRange(children);
        }
    }

    public bool IsEmpty => _roots.Count == 0;

    public bool Contains(T value)
    {
        var node = FindNode(value);
        return node is not null;
    }

    public IEnumerable<T> TraverseToRoot(T value)
    {
        var node = FindNode(value);
        if (node is null) throw new ArgumentException("Value not found in tree", nameof(value));
        return TraverseUp(node).Select(n => n.Value);
    }

    public IEnumerable<T> Flatten(TraversalMode mode = TraversalMode.BreadthFirst)
    {
        return _roots.SelectMany(r => Flatten(r, mode)).Select(x => x.Value);
    }

    public IEnumerable<T> Flatten(T value, TraversalMode mode = TraversalMode.BreadthFirst)
    {
        var node = FindNode(value);
        if (node is null) throw new ArgumentException("Value not found in tree", nameof(value));
        return Flatten(node, mode).Select(n => n.Value);
    }

    public void Move(T value, T newParent)
    {
        var node = FindNode(value);
        if (node is null) throw new ArgumentException("Value not found in tree", nameof(value));
        var newParentNode = FindNode(newParent);
        if (newParentNode is null) throw new ArgumentException("New parent not found in tree", nameof(newParent));
        node.Parent?.Children.Remove(node);
        node.Parent = newParentNode;
        newParentNode.Children.Add(node);
    }

    public int LevelOf(T value)
    {
        var node = FindNode(value);
        if (node is null) throw new ArgumentException("Value not found in tree", nameof(value));
        return node.Level;
    }

    public int RootCount => _roots.Count;
    public int NodeCount => _roots.Sum(x => x.Size);
    public int Depth => _roots.Max(r => r.Depth);

    private static IEnumerable<TreeNode<T>> Flatten(TreeNode<T> node, TraversalMode mode)
    {
        switch (mode)
        {
            case TraversalMode.BreadthFirst:
                var queue = new Queue<TreeNode<T>>();
                queue.Enqueue(node);
                while (queue.Count > 0)
                {
                    var n = queue.Dequeue();
                    yield return n;
                    foreach (var child in n.Children)
                    {
                        queue.Enqueue(child);
                    }
                }

                break;
            case TraversalMode.DepthFirst:
                var stack = new Stack<TreeNode<T>>();
                stack.Push(node);
                while (stack.Count > 0)
                {
                    var n = stack.Pop();
                    yield return n;
                    foreach (var child in n.Children)
                    {
                        stack.Push(child);
                    }
                }

                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
        }
    }

    private static IEnumerable<TreeNode<T>> TraverseUp(TreeNode<T> start)
    {
        var node = start;
        while (node.Parent is not null)
        {
            yield return node;
            node = node.Parent;
        }
    }
}

public class TreeNode<T> : IEquatable<TreeNode<T>>
{
    public TreeNode<T>? Parent { get; set; }
    public T Value { get; set; }
    public List<TreeNode<T>> Children { get; set; } = new();
    public bool IsRoot => Parent is null;
    public bool IsLeaf => Children.Count == 0;
    public int Level => IsRoot ? 0 : Parent!.Level + 1;
    public int Depth => IsLeaf ? 1 : Children.Max(c => c.Depth) + 1;
    public int Size => Children.Sum(c => c.Size) + 1;

    public bool Equals(TreeNode<T>? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return EqualityComparer<T>.Default.Equals(Value, other.Value);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((TreeNode<T>)obj);
    }

    public override int GetHashCode()
    {
        return EqualityComparer<T>.Default.GetHashCode(Value);
    }
}