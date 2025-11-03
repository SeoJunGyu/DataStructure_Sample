using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem;

public enum ProbingStrategy
{
    Linear, //선형
    Quadratic, //제곱
    DoubleHash, //이중 해싱

}

public class OpenAddressingHashTable<TKey, TValue> : IDictionary<TKey, TValue>
{
    private const int DefaultCapacity = 16; //기본 생성 사이즈
    private const double LoadFactor = 0.6; //적재율

    private KeyValuePair<TKey, TValue>[] table; //해시 테이블 배열
    private bool[] occupied; //해당 인덱스가 사용중인지 확인하는 용도
    private bool[] deleted; //삭제된 인덱스인지 확인하는 용도

    private int size; //실제 배열 사이즈
    public int Size{get { return size; }}
    public bool isSizeChanged{ get; set; }

    private int count; //실제로 들어가있는 갯수

    private ProbingStrategy probingStrategy; //탐사 전략
    public ProbingStrategy ProbingStrategy { get { return probingStrategy; } set { probingStrategy = value; }}

    public OpenAddressingHashTable(ProbingStrategy strategy = ProbingStrategy.Linear)
    {
        table = new KeyValuePair<TKey, TValue>[DefaultCapacity];
        occupied = new bool[DefaultCapacity];
        deleted = new bool[DefaultCapacity];
        size = DefaultCapacity;
        count = 0;
        probingStrategy = strategy;
    }

    public int GetPrimaryHash(TKey key)
    {
        if (key == null)
        {
            throw new ArgumentNullException(nameof(key));
        }

        int hash = key.GetHashCode();
        return Math.Abs(hash) % size;
    }

    public int GetSecondaryHash(TKey key)
    {
        int hash = key.GetHashCode();

        // 0이 반환되지 않도록 1을 더함
        return 1 + (Math.Abs(hash) % (size - 1));
    }

    //attempt : 시도 횟수
    public int GetProbeIndex(TKey key, int attempt)
    {
        int primaryHash = GetPrimaryHash(key);
        int secondaryHash = GetSecondaryHash(key);
        
        switch (probingStrategy)
        {
            case ProbingStrategy.Linear:
                //첫 번째 시도 : primaryHash + 0
                return (primaryHash + attempt) % size;
            case ProbingStrategy.Quadratic:
                //첫 번째 시도 : primaryHash + 0^2
                return (primaryHash + attempt * attempt) % size;
            case ProbingStrategy.DoubleHash:
                //첫 번째 시도 : primaryHash + 1 * secondaryHash
                return (primaryHash + attempt * secondaryHash) % size;
        }

        throw new ArgumentException(nameof(probingStrategy));
    }

    public TValue this[TKey key]
    {
        get
        {
            if (TryGetValue(key, out TValue value))
            {
                return value;
            }
            
            throw new KeyNotFoundException("키가 존재하지 않습니다.");
        }
        set
        {
            //Add와 차이점 -> 없이면 삽입, 있으면 교체
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            //로드 팩터부터 검사
            if ((double)count / size >= LoadFactor)
            {
                Resize();
            }

            //해시 충돌까지 포함한 add 검사
            int attempt = 0;

            do
            {
                int index = GetProbeIndex(key, attempt);
                if (!occupied[index] || deleted[index]) //지연삭제 된 경우를 확인하는 과정이다.
                {
                    table[index] = new KeyValuePair<TKey, TValue>(key, value);
                    occupied[index] = true;
                    deleted[index] = false;
                    count++;

                    return;
                }

                //인덱스가 사용중이고, 삭제된 인덱스 공간이 아닐 경우 -> 교체
                if (table[index].Key.Equals(key))
                {
                    table[index] = new KeyValuePair<TKey, TValue>(key, value);
                    return;
                }

                attempt++;

                if(attempt > size)
                {
                    Resize();
                    attempt = 0;
                }
            }
            while(true);
        }
    }

    public ICollection<TKey> Keys => Enumerable.Range(0, size).Where(i => occupied[i] && !deleted[i]).Select(i => table[i].Key).ToList();

    public ICollection<TValue> Values => Enumerable.Range(0, size).Where(i => occupied[i] && !deleted[i]).Select(i => table[i].Value).ToList();

    public int Count => count;

    public bool IsReadOnly => false;

    private void Resize()
    {
        var oldTable = table;
        var oldOccupied = occupied;
        var oldDeleted = deleted;
        var oldSize = size;

        //비우고 사이즈 키우기
        size *= 2;
        table = new KeyValuePair<TKey, TValue>[size];
        occupied = new bool[size];
        deleted = new bool[size];
        count = 0;

        for (int i = 0; i < oldSize; i++)
        {
            if (oldOccupied[i] && !oldDeleted[i])
            {
                Add(oldTable[i].Key, oldTable[i].Value);
            }
        }

        isSizeChanged = true;
    }
    
    //키에 매칭되는 인덱스 찾기, 없으면 -1 반환
    public int FindIndex(TKey key)
    {
        if (key == null)
        {
            throw new ArgumentNullException(nameof(key));
        }

        int attempt = 0;

        do
        {
            int index = GetProbeIndex(key, attempt);
            if (!occupied[index] && !deleted[index]) //한번도사용 안했고, 지워진적도 없다면
            {
                return -1;
            }

            //인덱스가 사용중이고, 삭제가 되지 않았고, 키가 일치하는 경우
            if (occupied[index] && !deleted[index] && table[index].Key.Equals(key))
            {
                return index;
            }

            attempt++;
        }
        while (attempt < size);

        return -1;
    }

    public void Add(TKey key, TValue value)
    {
        if (key == null)
        {
            throw new ArgumentNullException(nameof(key));
        }

        //로드 팩터부터 검사
        if ((double)count / size >= LoadFactor)
        {
            Resize();
        }

        //해시 충돌까지 포함한 add 검사
        int attempt = 0;

        do
        {
            int index = GetProbeIndex(key, attempt);
            if (!occupied[index] || deleted[index]) //지연삭제 된 경우를 확인하는 과정이다.
            {
                table[index] = new KeyValuePair<TKey, TValue>(key, value);
                occupied[index] = true;
                deleted[index] = false;
                count++;

                return;
            }

            //인덱스가 사용중이고, 삭제된 인덱스 공간이 아닐 경우
            if (table[index].Key.Equals(key))
            {
                throw new ArgumentException("이미 존재하는 키입니다.");
            }

            attempt++;

            if(attempt > size)
            {
                Resize();
                attempt = 0;
            }
        }
        while(true);
    }

    public void Add(KeyValuePair<TKey, TValue> item)
    {
        Add(item.Key, item.Value);
    }

    public void Clear()
    {
        Array.Clear(table, 0, size);
        Array.Clear(occupied, 0, size);
        Array.Clear(deleted, 0, size);
        count = 0;
    }

    public bool Contains(KeyValuePair<TKey, TValue> item)
    {
        int index = FindIndex(item.Key);
        if(index != -1)
        {
            return table[index].Value.Equals(item.Value);
        }

        return false;
    }

    public bool ContainsKey(TKey key)
    {
        return FindIndex(key) != -1;
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
            //사용중이고, 삭제되지 않은 인덱스의 값만 반환
            if(occupied[i] && !deleted[i])
            {
                yield return table[i];
            }
        }
    }

    public bool Remove(TKey key)
    {
        int index = FindIndex(key);

        if (index != -1)
        {
            deleted[index] = true; //실제로 삭제하기 않고 삭제된 공간이라고 표시만 한다.
            count--;
            return true;
        }

        //삭제할게 없는 경우
        return false;
    }

    public bool Remove(KeyValuePair<TKey, TValue> item)
    {
        int index = FindIndex(item.Key);

        if (index != -1 && table[index].Value.Equals(item.Value))
        {
            deleted[index] = true; //실제로 삭제하기 않고 삭제된 공간이라고 표시만 한다.
            count--;
            return true;
        }

        //삭제할게 없는 경우
        return false;
    }

    public bool TryGetValue(TKey key, out TValue value)
    {
        int index = FindIndex(key);

        //있는 경우
        if (index != -1)
        {
            value = table[index].Value;
            return true;
        }

        //없는 경우
        value = default;
        return false;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
