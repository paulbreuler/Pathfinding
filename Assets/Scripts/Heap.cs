using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// Generic heap.
/// 
/// ** Where T implements IHeapItem<t> garauntees that we can sort the item.
/// </summary>
/// <typeparam name="T"></typeparam>
public class Heap<T> where T : IHeapItem<T>
{
    /// <summary>
    /// The heap
    /// </summary>
    private T[] m_items;

    /// <summary>
    /// Number of items in the heap
    /// </summary>
    private int m_currentItemCount;

    public Heap(int maxHeapSize)
    {
        m_items = new T[maxHeapSize];
    }

    /// <summary>
    /// Add item to heap.
    /// </summary>
    /// <param name="item"> Item to add to heap </param>
    public void Add(T item)
    {
        item.HeapIndex = m_currentItemCount;
        m_items[m_currentItemCount] = item;
        SortUp(item);
        m_currentItemCount++;
    }


    /// <summary>
    /// Remove the first item of the heap
    /// </summary>
    /// <returns>First item of heap</returns>
    public T RemoveFirst()
    {
        // Remove first item and reduce heap count.
        T firstItem = m_items[0];
        m_currentItemCount--;

        // Take last item and make it first
        m_items[0] = m_items[m_currentItemCount];
        m_items[0].HeapIndex = 0;

        // Sort item down to maintain order
        SortDown(m_items[0]);

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
            return m_currentItemCount;
        }
    }

    public bool Contains(T item)
    {
        return Equals(m_items[item.HeapIndex], item);
    }

    /// <summary>
    /// Sort item down towards bottom of heap
    /// </summary>
    /// <param name="item"></param>
    void SortDown(T item)
    {
        while (true)
        {
            int childIndexLeft = item.HeapIndex * 2 + 1;    // 2n + 1
            int childIndexRight = item.HeapIndex * 2 + 2;   // 2n + 2
            int swapIndex = 0;

            // Does this item have a child on the left. Set to left by default.
            if (childIndexLeft < m_currentItemCount)
            {
                swapIndex = childIndexLeft;

                // Does this item have a child on the right?
                if (childIndexRight < m_currentItemCount)
                {

                    // Which child has the highest priority
                    if (m_items[childIndexLeft].CompareTo(m_items[childIndexRight]) < 0)
                    {
                        swapIndex = childIndexRight;
                    }
                }

                // If the parent is lower priority then swap.
                if (item.CompareTo(m_items[swapIndex]) < 0)
                {
                    Swap(item, m_items[swapIndex]);
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
        int parentIndex = (item.HeapIndex - 1) / 2;

        while (true)
        {
            T parentItem = m_items[parentIndex];
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
        m_items[itemA.HeapIndex] = itemB;
        m_items[itemB.HeapIndex] = itemA;
        int itemAIndex = itemA.HeapIndex;
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
