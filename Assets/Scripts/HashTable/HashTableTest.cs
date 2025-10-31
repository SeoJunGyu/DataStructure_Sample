using UnityEditor.Rendering;
using UnityEngine;

public class HashTableTest : MonoBehaviour
{
    private void Start()
    {
        var hashTable = new SimpleHashTable<string, int>();

        for(int i = 0; i < 20; i++)
        {
            hashTable.Add($"키{i}", i);
        }

        foreach (var kvp in hashTable)
        {
            Debug.Log($"Key: {kvp.Key}, Value: {kvp.Value}");
        }

        Debug.Log($"탐색 하나: {hashTable.ContainsKey("하나")}");

        hashTable.Remove("하나");

        Debug.Log($"탐색 하나: {hashTable.ContainsKey("하나")}");
    }
}
