using System;
using System.Collections.Generic;

namespace a_i.DataStructures
{
    /// <summary>
    /// Min Heap (İkili Yığın)
    /// Öncelik kuyruğu olarak kullanılır; en küçük eleman her zaman kökte bulunur.
    ///
    /// === GENEL ZAMAN KARMAŞIKLIĞI ===
    /// Insert      : O(log N) — List.Add O(1) amortized + HeapifyUp O(log N).
    /// Peek        : O(1)     — kök eleman doğrudan döner.
    /// ExtractMin  : O(log N) — kök alınır, son eleman köke taşınır, HeapifyDown O(log N).
    /// HeapifyUp   : O(log N) — en fazla ağaç yüksekliği kadar (⌊log₂N⌋) karşılaştırma.
    /// HeapifyDown : O(log N) — en fazla ağaç yüksekliği kadar iniş yapılır.
    /// Swap        : O(1)     — sabit sayıda atama işlemi.
    /// Count/IsEmpty: O(1)   — List.Count önceden tutulan sayacı döner.
    ///
    /// === UZAY KARMAŞIKLIĞI ===
    /// O(N) — N eleman List içinde ardışık bellekte tutulur.
    /// Özyineleme yok; HeapifyUp ve HeapifyDown yinelemeli → O(1) yığın kullanımı.
    /// </summary>
    public class MinHeap<T> where T : IComparable<T>
    {
        private readonly List<T> _items;

        // O(1) — boş List sabit sürede oluşturulur.
        public MinHeap()
        {
            _items = new List<T>();
        }

        // O(1) — List.Count önceden tutulan bir sayaç; hesaplanmaz.
        public int Count
        {
            get { return _items.Count; }
        }

        // O(1) — Count karşılaştırması sabit sürede tamamlanır.
        public bool IsEmpty()
        {
            return _items.Count == 0;
        }

        // O(1) — sabit aritmetik; yalnızca indeks hesabı yapılır.
        private int Parent(int index)
        {
            return (index - 1) / 2;
        }

        // O(1) — sabit aritmetik; yalnızca indeks hesabı yapılır.
        private int LeftChild(int index)
        {
            return 2 * index + 1;
        }

        // O(1) — sabit aritmetik; yalnızca indeks hesabı yapılır.
        private int RightChild(int index)
        {
            return 2 * index + 2;
        }

        // O(log N) — List.Add O(1) amortized + HeapifyUp O(log N).
        // En kötü durum: yeni eleman köke kadar taşınır → ⌊log₂N⌋ swap.
        // O(N) en kötü durum (nadir) — List kapasitesi dolup Resize tetiklenirse.
        public void Insert(T value)
        {
            _items.Add(value);            // O(1) amortized
            HeapifyUp(_items.Count - 1);  // O(log N)
        }

        // O(1) — kök eleman indeks 0'da sabit; hiçbir gezme yapılmaz.
        public T Peek()
        {
            if (IsEmpty())
            {
                throw new InvalidOperationException("MinHeap bos oldugu icin en kucuk eleman okunamaz.");
            }

            return _items[0]; // O(1)
        }

        // O(log N) — üç aşamalı işlem:
        //   1) Kök alınır              : O(1)
        //   2) Son eleman köke taşınır : O(1)
        //   3) HeapifyDown             : O(log N)
        // Heap özelliği her zaman korunur; N-1 elemanlı geçerli heap kalır.
        public T ExtractMin()
        {
            if (IsEmpty())
            {
                throw new InvalidOperationException("MinHeap bos oldugu icin eleman cikarilamaz.");
            }

            T minValue = _items[0];              // O(1) — kök okunur

            int lastIndex = _items.Count - 1;
            _items[0] = _items[lastIndex];       // O(1) — son eleman köke yazılır
            _items.RemoveAt(lastIndex);          // O(1) — listenin sonundan silme

            if (!IsEmpty())
            {
                HeapifyDown(0);                  // O(log N) — heap özelliği yeniden sağlanır
            }

            return minValue;
        }

        // O(log N) — ağaç yüksekliği ⌊log₂N⌋; her iterasyonda O(1) karşılaştırma + swap.
        // En iyi durum: O(1) — eleman zaten doğru konumdaysa ilk karşılaştırmada durur.
        // Yönü: yaprak → kök (yukarı doğru).
        private void HeapifyUp(int index)
        {
            while (index > 0)                                         // en fazla log N iterasyon
            {
                int parentIndex = Parent(index);                      // O(1)

                if (_items[parentIndex].CompareTo(_items[index]) <= 0) // O(1) karşılaştırma
                {
                    break; // heap özelliği sağlandı, erken çıkış
                }

                Swap(parentIndex, index); // O(1)
                index = parentIndex;      // bir seviye yukarı çıkılır
            }
        }

        // O(log N) — ağaç yüksekliği ⌊log₂N⌋; her iterasyonda en fazla 2 karşılaştırma + 1 swap.
        // En iyi durum: O(1) — kök zaten iki çocuğundan küçükse ilk iterasyonda durur.
        // Yönü: kök → yaprak (aşağı doğru); her adımda daha küçük çocukla yer değiştirilir.
        private void HeapifyDown(int index)
        {
            while (true)
            {
                int leftIndex = LeftChild(index);   // O(1)
                int rightIndex = RightChild(index);  // O(1)
                int smallestIndex = index;

                // Sol çocuk mevcutsa ve daha küçükse güncelle — O(1)
                if (leftIndex < _items.Count &&
                    _items[leftIndex].CompareTo(_items[smallestIndex]) < 0)
                {
                    smallestIndex = leftIndex;
                }

                // Sağ çocuk mevcutsa ve daha küçükse güncelle — O(1)
                if (rightIndex < _items.Count &&
                    _items[rightIndex].CompareTo(_items[smallestIndex]) < 0)
                {
                    smallestIndex = rightIndex;
                }

                if (smallestIndex == index)
                {
                    break; // heap özelliği sağlandı, erken çıkış — O(1)
                }

                Swap(index, smallestIndex); // O(1)
                index = smallestIndex;      // bir seviye aşağı inilir
            }
        }

        // O(1) — üç atama işlemi; geçici değişken ile sabit sürede tamamlanır.
        private void Swap(int firstIndex, int secondIndex)
        {
            T temp = _items[firstIndex];  // O(1)
            _items[firstIndex] = _items[secondIndex]; // O(1)
            _items[secondIndex] = temp;                // O(1)
        }
    }
}