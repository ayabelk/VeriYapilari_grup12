using System;
using System.Collections.Generic;

namespace back_End.DataStructures
{
    /// <summary>
    /// Generic Dinamik Dizi
    /// FOV ışın sonuçlarını (RayHit) saklamak için kullanılır.
    ///
    /// === GENEL ZAMAN KARMAŞIKLIĞI ===
    /// Add      : O(1) amortized — boyutlandırma 2x büyüme ile geometrik seri
    ///            oluşturur; N ekleme toplamda O(N) maliyet verir → O(1) amortized.
    ///            O(N) en kötü durum — Resize tetiklendiğinde Array.Copy çalışır.
    /// Get/Set  : O(1) — dizi indeksiyle doğrudan erişim.
    /// Pop      : O(1) — son eleman alınır, dizi küçültülmez.
    /// Clear    : O(1) — yeni sabit boyutlu dizi tahsis edilir.
    /// Resize   : O(N) — mevcut N eleman kopyalanır; Add içinden çağrılır.
    /// GetEnumerator: O(N) — tüm elemanlar bir kez ziyaret edilir.
    ///
    /// === UZAY KARMAŞIKLIĞI ===
    /// O(N) — _data dizisi en fazla 2N eleman kapasitesinde tutulur
    ///         (2x büyüme stratejisi nedeniyle kapasite < 2·count garantisi).
    /// </summary>
    public class DynamicArray<T>
    {
        private T[] _data;
        private int _count;
        private int _capacity;
        private const int DefaultCapacity = 4;

        public int Count => _count;
        public bool IsEmpty => _count == 0;

        // O(1) — sabit boyutlu dizi tahsisi.
        public DynamicArray(int capacity = DefaultCapacity)
        {
            _capacity = capacity > 0 ? capacity : DefaultCapacity;
            _data = new T[_capacity];
        }

        // O(1) amortized / O(N) en kötü durum (Resize tetiklenirse).
        // Kanıt: k. yeniden boyutlandırma 2^k kopyalamaya yol açar.
        // Toplam N ekleme için toplam kopya: 1+2+4+...+N = 2N → O(1) amortized.
        public void Add(T item)
        {
            if (_count == _capacity) Resize(_capacity * 2); // O(N) — nadir tetiklenir
            _data[_count++] = item;                          // O(1)
        }

        // O(1) — dizi belleğinde sabit offsetli doğrudan erişim.
        public T Get(int i)
        {
            if (i < 0 || i >= _count) throw new IndexOutOfRangeException(); // O(1)
            return _data[i];                                                  // O(1)
        }

        // O(1) get ve set — indeksleyici, Get/Set ile aynı karmaşıklık.
        public T this[int i] { get => Get(i); set { if (i < 0 || i >= _count) throw new IndexOutOfRangeException(); _data[i] = value; } }

        // O(1) — yalnızca sayaç azaltılır; kapasite korunur (küçültme yapılmaz).
        // Not: Bellek geri verilmez; gerekirse ShrinkToFit eklenebilir.
        public T Pop()
        {
            if (IsEmpty) throw new InvalidOperationException("Dizi bos");
            var item = _data[--_count]; // O(1)
            _data[_count] = default!;   // O(1) — GC referansı serbest bırakır
            return item;
        }

        // O(1) — yeni DefaultCapacity boyutlu dizi tahsis edilir;
        // eski elemanlar kopyalanmaz, doğrudan atılır.
        public void Clear() { _data = new T[DefaultCapacity]; _capacity = DefaultCapacity; _count = 0; }

        // O(N) — Array.Copy ile _count elemanı yeni diziye taşır.
        // Her zaman Add içinden çağrılır; doğrudan çağrı önerilmez.
        private void Resize(int newCap)
        {
            newCap = Math.Max(newCap, DefaultCapacity);  // O(1)
            var nd = new T[newCap];                       // O(1) tahsis
            Array.Copy(_data, nd, _count);               // O(N) kopyalama
            _data = nd; _capacity = newCap;              // O(1)
        }

        // O(N) — her eleman yield return ile teker teker üretilir.
        // foreach döngüsü bu yineleyiciyi kullandığında toplam maliyet O(N).
        public IEnumerator<T> GetEnumerator()
        { for (int i = 0; i < _count; i++) yield return _data[i]; }
    }
}