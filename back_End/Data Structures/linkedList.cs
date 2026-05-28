using System;

namespace a_i.DataStructures
{
    /// <summary>
    /// Tek Yönlü Bağlı Liste
    /// FOV raycasting'de görünür duvar segmentlerini tutmak için kullanılır.
    ///
    /// === GENEL ZAMAN KARMAŞIKLIĞI ===
    /// AddFirst  : O(1) — yeni düğüm doğrudan başa bağlanır, liste gezilmez.
    /// Remove    : O(N) — hedef düğüm en kötü durumda listenin sonundadır.
    /// Traverse  : O(N) — tüm düğümler bir kez ziyaret edilir.
    /// Clear     : O(1) — yalnızca head pointer sıfırlanır.
    /// Contains  : O(N) — Remove ile aynı doğrusal arama mantığı.
    ///
    /// === UZAY KARMAŞIKLIĞI ===
    /// O(N) — N düğüm, her biri sabit boyutlu Data + Next alanı taşır.
    /// Ekstra bellek: O(1) — tüm operasyonlar sabit yardımcı değişken kullanır.
    /// </summary>
    public class LinkedListNode<T>
    {
        public T Data;
        public LinkedListNode<T>? Next;

        // O(1) — sabit alan tahsisi: Data ve Next atanır.
        public LinkedListNode(T data) { Data = data; Next = null; }
    }

    public class CustomLinkedList<T>
    {
        private LinkedListNode<T>? _head;
        private int _count;

        public int Count => _count;       // O(1) — önceden tutulan sayaç
        public bool IsEmpty => _count == 0; // O(1) — sayaç karşılaştırması

        // O(1) — yalnızca pointer ve sayaç atanır.
        public CustomLinkedList() { _head = null; _count = 0; }

        // O(1) — yeni düğüm oluşturulur, head'in önüne eklenir.
        // Listenin geri kalanı hiç gezilmez; tail erişimi gerekmez.
        public void AddFirst(T data)
        {
            var node = new LinkedListNode<T>(data); // O(1) tahsis
            node.Next = _head;                       // O(1) bağlama
            _head = node;                            // O(1) head güncelleme
            _count++;                                // O(1)
        }

        // O(1) — head pointer null yapılır; GC geri kalan düğümleri temizler.
        // Not: Düğümler tek tek serbest bırakılmaz; GC yükü O(N) olabilir.
        public void Clear() { _head = null; _count = 0; }

        // O(N) — hedef eleman en kötü durumda son düğümdedir (N karşılaştırma).
        // O(1) en iyi durum — aranan eleman head'dedir.
        // Her adımda O(1) Equals çağrısı yapılır (referans/değer türüne bağlı).
        public bool Remove(T data)
        {
            if (_head == null) return false; // O(1) — boş liste erken çıkış

            // O(1) — head eşleşirse doğrudan kopar
            if (_head.Data!.Equals(data)) { _head = _head.Next; _count--; return true; }

            // O(N) — iç düğümler doğrusal tarama ile aranır
            var cur = _head;
            while (cur.Next != null)
            {
                // O(1) — her düğümde sabit işlem
                if (cur.Next.Data!.Equals(data)) { cur.Next = cur.Next.Next; _count--; return true; }
                cur = cur.Next;
            }
            return false; // O(1) — eleman bulunamadı
        }

        // O(N) — tüm düğümler ziyaret edilir; her birinde O(1) action çağrısı.
        // Toplam: O(N) × O(1) = O(N).
        // Not: action'ın kendi karmaşıklığı bu analize dahil değildir.
        public void Traverse(Action<T> action)
        {
            var cur = _head;
            while (cur != null) { action(cur.Data); cur = cur.Next; } // O(N)
        }
    }
}