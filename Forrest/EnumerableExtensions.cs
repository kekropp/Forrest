using System.Linq.Expressions;

namespace Forrest;

public static class EnumerableExtensions
{
    public static ITree<T> ToTree<T>(this IEnumerable<T> enumerable, Expression<Func<T, T?>> parentSelector)
    {
        return Tree.Create(enumerable, parentSelector);
    }

    public static ITree<T> ToTree<T>(this IEnumerable<T> enumerable,
        Expression<Func<T, IEnumerable<T>?>> childrenSelector)
    {
        return Tree.Create(enumerable, childrenSelector);
    }

    public static ITree<T> ToTree<T>(this IEnumerable<T> enumerable)
    {
        return Tree.Create(enumerable);
    }
}