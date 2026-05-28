using System;

namespace a_i.DataStructures
{
    public class MinHeap<T> where T : IComparable<T>
    {
        private T[] heap;
        private int size;
        private int capacity;

        public MinHeap(int capacity)
        {
            this.capacity = capacity;
            heap = new T[capacity];
            size = 0;
        }

        private int Parent(int i) { return (i - 1) / 2; }
        private int LeftChild(int i) { return 2 * i + 1; }
        private int RightChild(int i) { return 2 * i + 2; }

        private void HeapifyUp(int index)
        {
            while (index > 0 && heap[Parent(index).CompareTo(heap[index]) > 0])
            {
                Swap(index, Parent(index));
                index = Parent(index);
            }
        }

        private void HeapifyDown(int index)
        {
            int left = LeftChild(index);
            int right = RightChild(index);
            int smallest = index;

            if (left < size && heap[left].CompareTo(heap[smallest]) < 0)
                smallest = left;

            if (right < size && heap[right].CompareTo(heap[smallest]) < 0)
                smallest = right;

            if (smallest != index)
            {
                Swap(index, smallest);
                HeapifyDown(smallest);
            }
        }

        private void Swap(int i, int j)
        {
            T temp = heap[i];
            heap[i] = heap[j];
            heap[j] = temp;
        }

        public void Insert(T value)
        {
            if (size == capacity)
            {
                Console.WriteLine("Heap is full!");
                return;
            }

            heap[size] = value;
            size++;
            HeapifyUp(size - 1);
        }

        public T ExtractMin()
        {
            if (size == 0)
            {
                Console.WriteLine("Heap is empty!");
                return default(T);
            }

            T root = heap[0];
            heap[0] = heap[size - 1];
            size--;
            HeapifyDown(0);
            return root;
        }

        public T GetMin()
        {
            if (size == 0)
            {
                Console.WriteLine("Heap is empty!");
                return default(T);
            }
            return heap[0];
        }

        // LinkedList'teki verileri MinHeap'e ekler
        public void AddFromLinkedList(CustomLinkedList<T> linkedList)
        {
            linkedList.Traverse(data =>
            {
                Insert(data);  // LinkedList'teki her öğeyi MinHeap'e ekle
            });
        }

        // Graph'teki kenarları sıralamak için kullanılacak
        public void AddEdgeFromGraph(List<Edge> edges)
        {
            foreach (var edge in edges)
            {
                Insert(edge);  // Graph'teki her kenarı MinHeap'e ekle
            }
        }

        // BSP Tree'deki WallSegmentleri sıralamak için kullanılacak
        public void AddWallSegmentFromBSP(List<WallSegment> walls)
        {
            foreach (var wall in walls)
            {
                Insert(wall);  // BSP Tree'deki her wall segmentini MinHeap'e ekle
            }
        }
    }
}