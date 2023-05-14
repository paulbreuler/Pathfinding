using System;

/// <summary>
/// Generic heap.
/// 
/// ** Where T implements IHeapItem<t> guarantees that we can sort the item.
/// </summary>
/// <typeparam name="T"></typeparam>
public class Heap<T> where T : IHeapItem<T>
{
    /// <summary>
    /// The heap
    /// </summary>
    private readonly T[] _items;

    /// <summary>
    /// Number of items in the heap
    /// </summary>
    private int _currentItemCount;

    public Heap(int maxHeapSize)
    {
        _items = new T[maxHeapSize];
    }

    /// <summary>
    /// Add item to heap.
    /// </summary>
    /// <param name="item"> Item to add to heap </param>
    public void Add(T item)
    {
        item.HeapIndex = _currentItemCount;
        _items[_currentItemCount] = item;
        SortUp(item);
        _currentItemCount++;
    }


    /// <summary>
    /// Remove the first item of the heap
    /// </summary>
    /// <returns>First item of heap</returns>
    public T RemoveFirst()
    {
        // Remove first item and reduce heap count.
        var firstItem = _items[0];
        _currentItemCount--;

        // Take last item and make it first
        _items[0] = _items[_currentItemCount];
        _items[0].HeapIndex = 0;

        // Sort item down to maintain order
        SortDown(_items[0]);

        return firstItem;
    }

    /// <summary>
    /// Resort item
    /// </summary>
    /// <param name="item"></param>
    public void UpdateItem(T item)
    {
        SortUp(item);
        // ***May need to also sort down. For pathfinding case not needed.***
    }

    public int Count
    {
        get
        {
            return _currentItemCount;
        }
    }

    public bool Contains(T item)
    {
        return Equals(_items[item.HeapIndex], item);
    }

    /// <summary>
    /// Sort item down towards bottom of heap
    /// </summary>
    /// <param name="item"></param>
    void SortDown(T item)
    {
        while (true)
        {
            var childIndexLeft = item.HeapIndex * 2 + 1;    // 2n + 1
            var childIndexRight = item.HeapIndex * 2 + 2;   // 2n + 2

            // Does this item have a child on the left. Set to left by default.
            if (childIndexLeft < _currentItemCount)
            {
                var swapIndex = childIndexLeft;

                // Does this item have a child on the right?
                if (childIndexRight < _currentItemCount)
                {

                    // Which child has the highest priority
                    if (_items[childIndexLeft].CompareTo(_items[childIndexRight]) < 0)
                    {
                        swapIndex = childIndexRight;
                    }
                }

                // If the parent is lower priority then swap.
                if (item.CompareTo(_items[swapIndex]) < 0)
                {
                    Swap(item, _items[swapIndex]);
                }
                else
                {
                    // Parent is in correct position. Exit loop.
                    return;
                }

            }
            else
            {
                // No children
                return;
            }

        }
    }

    /// <summary>
    /// Sort item towards top of heap
    /// </summary>
    /// <param name="item"></param>
    void SortUp(T item)
    {
        // find parent (n-1)/2
        var parentIndex = (item.HeapIndex - 1) / 2;

        while (true)
        {
            var parentItem = _items[parentIndex];
            if (item.CompareTo(parentItem) > 0)
            {
                Swap(item, parentItem);
            }
            else
            {
                break;
            }

            parentIndex = (item.HeapIndex - 1) / 2;
        }
    }

    /// <summary>
    /// Swap itemA with itemB in the heap
    /// </summary>
    /// <param name="itemA"></param>
    /// <param name="itemB"></param>
    void Swap(T itemA, T itemB)
    {
        _items[itemA.HeapIndex] = itemB;
        _items[itemB.HeapIndex] = itemA;
        var itemAIndex = itemA.HeapIndex;
        itemA.HeapIndex = itemB.HeapIndex;
        itemB.HeapIndex = itemAIndex;
    }
}

/// <summary>
/// 
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IHeapItem<T> : IComparable<T>
{
    int HeapIndex
    {
        get;
        set;
    }
}
