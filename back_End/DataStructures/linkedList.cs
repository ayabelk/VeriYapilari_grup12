using System;

namespace back_End.DataStructures 
{
    /// <summary>
    /// Zaman Karmaşıklığı: 
    /// - Ekleme (Başa/Sona): O(1)
    /// - Arama/Erişim: O(n)
    /// - Silme: O(n)
    /// Uzay Karmaşıklığı: O(n)
    /// </summary>
    public class LinkedListNode<T>
    {
        public T Data;
        public LinkedListNode<T> Next;

        public LinkedListNode(T data)
        {
            Data = data;
            Next = null;
        }
    }

    public class CustomLinkedList<T>
    {
        private LinkedListNode<T> _head;
        private int _count;

        public int Count => _count;

        public CustomLinkedList()
        {
            _head = null;
            _count = 0;
        }

        // Listeye yeni eleman ekler (Başa ekleme)
        public void AddFirst(T data)
        {
            LinkedListNode<T> newNode = new LinkedListNode<T>(data);
            newNode.Next = _head;
            _head = newNode;
            _count++;
        }

        // Listeyi tamamen temizler
        public void Clear()
        {
            _head = null;
            _count = 0;
        }

        // Belirli bir veriyi listeden siler
        public bool Remove(T data)
        {
            if (_head == null) return false;

            if (_head.Data.Equals(data))
            {
                _head = _head.Next;
                _count--;
                return true;
            }

            LinkedListNode<T> current = _head;
            while (current.Next != null)
            {
                if (current.Next.Data.Equals(data))
                {
                    current.Next = current.Next.Next;
                    _count--;
                    return true;
                }
                current = current.Next;
            }
            return false;
        }

        // Liste üzerinde işlem yapmak için (Örn: FOV içindeki duvarları gezmek)
        public void Traverse(Action<T> action)
        {
            LinkedListNode<T> current = _head;
            while (current != null)
            {
                action(current.Data);
                current = current.Next;
            }
        }
    }
}