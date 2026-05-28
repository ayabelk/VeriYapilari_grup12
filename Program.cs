using System;
using a_i.DataStructures;

class Program
{
    static void Main()
    {
        // MinHeap ve CustomLinkedList oluştur
        MinHeap<int> minHeap = new MinHeap<int>(10);
        CustomLinkedList<int> linkedList = new CustomLinkedList<int>();

        // LinkedList'e bazı öğeler ekle
        linkedList.AddFirst(10);
        linkedList.AddFirst(4);
        linkedList.AddFirst(15);
        linkedList.AddFirst(1);
        linkedList.AddFirst(7);

        // LinkedList'teki verileri MinHeap'e ekle
        minHeap.AddFromLinkedList(linkedList);

        // MinHeap'teki en küçük öğeyi al
        Console.WriteLine("En küçük öğe: " + minHeap.GetMin());

        // MinHeap'ten en küçük öğeyi çıkaralım
        Console.WriteLine("Çıkarılan en küçük öğe: " + minHeap.ExtractMin());
        Console.WriteLine("Yeni en küçük öğe: " + minHeap.GetMin());

        // Graph ve BSP Tree testleri için örnek ekleyebiliriz
        Graph graph = new Graph();
        graph.AddNode(new GraphNode(1, 0, 0));
        graph.AddNode(new GraphNode(2, 1, 1));
        graph.AddEdge(1, 2, 10);

        Console.WriteLine("En düşük maliyetli kenar: " + minHeap.GetMin());

        BspTree bspTree = new BspTree();
        bspTree.AddWallSegmentsFromTree(new List<WallSegment> {
            new WallSegment(0, 0, 1, 1),
            new WallSegment(1, 1, 2, 2)
        });
        
        Console.WriteLine("BSP Tree'deki en küçük duvar segmenti: " + minHeap.GetMin());
    }
}