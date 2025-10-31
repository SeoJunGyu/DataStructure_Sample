using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ChainingHashTable<TKey, TValue> : IDictionary<TKey, TValue>
{
    private const int DefaultCapacity = 16; //기본 생성 사이즈
    private const double LoadFactor = 0.75; //적재율

    private LinkedList<KeyValuePair<TKey, TValue>>[] table; //해시 테이블 배열

    private int size; //실제 배열 사이즈
    private int count; //실제로 들어가있는 갯수

    public TValue this[TKey key]
    {
        get
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            int index = FindIndex(key);
            var list = table[index];

            foreach (var kvp in list)
            {
                if (kvp.Key.Equals(key))
                {
                    return kvp.Value;
                }
            }

            throw new KeyNotFoundException("키 없음!");
        }

        set
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            int index = FindIndex(key);
            var list = table[index];

            //기존 키와 값이 있으면 값 업데이트
            foreach (var kvp in list)
            {
                if (kvp.Key.Equals(key))
                {
                    list.Remove(kvp);
                    list.AddLast(new KeyValuePair<TKey, TValue>(key, value));
                    return;
                }
            }
        }
    }

    public ICollection<TKey> Keys
    {
        get
        {
            var keys = new List<TKey>();
            for (int i = 0; i < size; i++)
            {
                var list = table[i];
                foreach (var kvp in list)
                {
                    keys.Add(kvp.Key);
                }
            }

            return keys;
        }
    }

    public ICollection<TValue> Values
    {
        get
        {
            var values = new List<TValue>();
            for (int i = 0; i < size; i++)
            {
                var list = table[i];
                foreach (var kvp in list)
                {
                    values.Add(kvp.Value);
                }
            }

            return values;
        }
    }

    public int Count => count;

    public bool IsReadOnly => false;

    public ChainingHashTable()
    {
        table = new LinkedList<KeyValuePair<TKey, TValue>>[DefaultCapacity];
        for (int i = 0; i < table.Length; i++)
        {
            table[i] = new LinkedList<KeyValuePair<TKey, TValue>>();
        }

        size = DefaultCapacity;
        count = 0;
    }

    public int GetProbeIndex(TKey key)
    {
        if (key == null)
        {
            throw new ArgumentNullException(nameof(key));
        }

        int hash = key.GetHashCode();
        return Math.Abs(hash) % size;
    }

    private void Resize()
    {
        var oldTable = table;
        size *= 2;

        table = new LinkedList<KeyValuePair<TKey, TValue>>[size];
        for (int i = 0; i < table.Length; i++)
        {
            table[i] = new LinkedList<KeyValuePair<TKey, TValue>>();
        }

        count = 0;

        foreach (var list in oldTable)
        {
            foreach (var kvp in list)
            {
                Add(kvp.Key, kvp.Value);
            }
        }
    }

    public int FindIndex(TKey key)
    {
        if(key == null)
        {
            throw new ArgumentNullException(nameof(key));
        }

        var index = GetProbeIndex(key);
        return index;
    }

    public void Add(TKey key, TValue value)
    {
        if (key == null)
        {
            throw new ArgumentNullException(nameof(key));
        }

        if ((double)count / size >= LoadFactor)
        {
            Resize();
        }

        int index = GetProbeIndex(key);
        var list = table[index];

        foreach (var kvp in list)
        {
            if (kvp.Key.Equals(key))
            {
                throw new InvalidOperationException("키가 이미 존재합니다.");
            }
        }
        
        list.AddLast(new KeyValuePair<TKey, TValue>(key, value));
        count++;
    }

    public void Add(KeyValuePair<TKey, TValue> item)
    {
        Add(item.Key, item.Value);
    }

    public void Clear()
    {
        Array.Clear(table, 0, table.Length);
        for (int i = 0; i < table.Length; i++)
        {
            table[i] = new LinkedList<KeyValuePair<TKey, TValue>>();
        }
        count = 0;
    }

    public bool Contains(KeyValuePair<TKey, TValue> item)
    {
        int index = FindIndex(item.Key);
        var list = table[index];
        foreach (var kvp in list)
        {
            if (kvp.Key.Equals(item.Key) && kvp.Value.Equals(item.Value))
            {
                return true;
            }
        }
        
        return false;
    }

    public bool ContainsKey(TKey key)
    {
        if (key == null)
        {
            throw new ArgumentNullException(nameof(key));
        }
        
        int index = FindIndex(key);
        var list = table[index];
        foreach (var kvp in list)
        {
            if (kvp.Key.Equals(key))
            {
                return true;
            }
        }
        
        return false;
    }

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        int index = arrayIndex;
        foreach(var kvp in this)
        {
            array[index++] = kvp;
        }
    }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        for(int i = 0; i < size; i++)
        {
            var list = table[i];
            foreach(var kvp in list)
            {
                yield return kvp;
            }
        }
    }

    public bool Remove(TKey key)
    {
        int index = FindIndex(key);
        var list = table[index];

        foreach (var kvp in list)
        {
            if (kvp.Key.Equals(key))
            {
                list.Remove(kvp);
                count--;
                return true;
            }
        }

        return false;
    }

    public bool Remove(KeyValuePair<TKey, TValue> item)
    {
        int index = FindIndex(item.Key);
        var list = table[index];

        foreach (var kvp in list)
        {
            if (kvp.Key.Equals(item.Key) && kvp.Value.Equals(item.Value))
            {
                list.Remove(kvp);
                count--;
                return true;
            }
        }

        return false;
    }

    public bool TryGetValue(TKey key, out TValue value)
    {
        if (key == null)
        {
            throw new ArgumentNullException(nameof(key));
        }

        int index = FindIndex(key);
        var list = table[index];

        foreach (var kvp in list)
        {
            if (kvp.Key.Equals(key))
            {
                value = kvp.Value;
                return true;
            }
        }

        value = default;
        return false;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
