using System;
using System.Collections.Generic;


public class PriorityQueue<TElement, TPriority>
{
    private readonly List<(TElement Element, TPriority Priority)> _heap;
    private readonly IComparer<TPriority> _comparer;

    public PriorityQueue(IComparer<TPriority> comparer = null)
    {
        _heap = new List<(TElement, TPriority)>();
        _comparer = comparer ?? Comparer<TPriority>.Default;
    }

    public int Count => _heap.Count;
    public bool IsEmpty => Count == 0;

    public void Enqueue(TElement element, TPriority priority)
    {
        _heap.Add((element, priority));
        BubbleUp(_heap.Count - 1);
    }

    public TElement Dequeue()
    {
        if (_heap.Count == 0)
            throw new InvalidOperationException("队列为空");

        var top = _heap[0];
        _heap[0] = _heap[_heap.Count - 1];
        _heap.RemoveAt(_heap.Count - 1);
        BubbleDown(0);
        return top.Element;
    }

    public bool TryDequeue(out TElement element)
    {
        if (_heap.Count == 0)
        {
            element = default(TElement);
            return false;
        }

        element = Dequeue();
        return true;
    }

    public TElement Peek()
    {
        if (_heap.Count == 0)
            throw new InvalidOperationException("队列为空");
        return _heap[0].Element;
    }

    public bool TryPeek(out TElement element)
    {
        if (_heap.Count == 0)
        {
            element = default(TElement);
            return false;
        }
        element = Peek();
        return true;
    }

    private void BubbleUp(int index)
    {
        while (index > 0)
        {
            int parentIndex = (index - 1) / 2;
            if (_comparer.Compare(_heap[index].Priority, _heap[parentIndex].Priority) >= 0)
                break;

            Swap(index, parentIndex);
            index = parentIndex;
        }
    }

    private void BubbleDown(int index)
    {
        while (true)
        {
            int leftChild = 2 * index + 1;
            int rightChild = 2 * index + 2;
            int smallest = index;

            if (leftChild < _heap.Count &&
                _comparer.Compare(_heap[leftChild].Priority, _heap[smallest].Priority) < 0)
            {
                smallest = leftChild;
            }

            if (rightChild < _heap.Count &&
                _comparer.Compare(_heap[rightChild].Priority, _heap[smallest].Priority) < 0)
            {
                smallest = rightChild;
            }

            if (smallest == index) break;

            Swap(index, smallest);
            index = smallest;
        }
    }

    private void Swap(int i, int j)
    {
        (_heap[i], _heap[j]) = (_heap[j], _heap[i]);
    }
}
