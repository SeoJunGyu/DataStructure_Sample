using UnityEngine;

public class HeapTest : MonoBehaviour
{
    private PriorityQueue<string, int> pq = new PriorityQueue<string, int>();
    void Start()
    {
        pq.Enqueue("Low",1);
        pq.Enqueue("High", 5);
        pq.Enqueue("Medium", 10);

        Debug.Log(pq.Dequeue()); // "High" (우선순위 1)
        Debug.Log(pq.Dequeue()); // "Medium" (우선순위 5)
        Debug.Log(pq.Dequeue()); // "Low" (우선순위 10)
    }
}
