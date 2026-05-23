using System;
using System.Collections.Generic;

namespace GameEngine.DataStructures
{
    public class MinHeap<T> where T : IComparable<T>
    {
        private readonly List<T> _items;

        public MinHeap()
        {
            _items = new List<T>();
        }

        public int Count
        {
            get { return _items.Count; }
        }

        public bool IsEmpty()
        {
            return _items.Count == 0;
        }

        private int Parent(int index)
        {
            return (index - 1) / 2;
        }

        private int LeftChild(int index)
        {
            return 2 * index + 1;
        }

        private int RightChild(int index)
        {
            return 2 * index + 2;
        }

        public void Insert(T value)
        {
            _items.Add(value);
            HeapifyUp(_items.Count - 1);
        }

        public T Peek()
        {
            if (IsEmpty())
            {
                throw new InvalidOperationException("MinHeap bos oldugu icin en kucuk eleman okunamaz.");
            }

            return _items[0];
        }

        public T ExtractMin()
        {
            if (IsEmpty())
            {
                throw new InvalidOperationException("MinHeap bos oldugu icin eleman cikarilamaz.");
            }

            T minValue = _items[0];

            int lastIndex = _items.Count - 1;
            _items[0] = _items[lastIndex];
            _items.RemoveAt(lastIndex);

            if (!IsEmpty())
            {
                HeapifyDown(0);
            }

            return minValue;
        }

        private void HeapifyUp(int index)
        {
            while (index > 0)
            {
                int parentIndex = Parent(index);

                if (_items[parentIndex].CompareTo(_items[index]) <= 0)
                {
                    break;
                }

                Swap(parentIndex, index);
                index = parentIndex;
            }
        }

        private void HeapifyDown(int index)
        {
            while (true)
            {
                int leftIndex = LeftChild(index);
                int rightIndex = RightChild(index);
                int smallestIndex = index;

                if (leftIndex < _items.Count &&
                    _items[leftIndex].CompareTo(_items[smallestIndex]) < 0)
                {
                    smallestIndex = leftIndex;
                }

                if (rightIndex < _items.Count &&
                    _items[rightIndex].CompareTo(_items[smallestIndex]) < 0)
                {
                    smallestIndex = rightIndex;
                }

                if (smallestIndex == index)
                {
                    break;
                }

                Swap(index, smallestIndex);
                index = smallestIndex;
            }
        }

        private void Swap(int firstIndex, int secondIndex)
        {
            T temp = _items[firstIndex];
            _items[firstIndex] = _items[secondIndex];
            _items[secondIndex] = temp;
        }
    }
}