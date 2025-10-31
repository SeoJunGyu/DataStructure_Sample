using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework.Constraints;
using UnityEngine;
using UnityEngine.InputSystem;

public class SimpleHashTable<TKey, TValue> : IDictionary<TKey, TValue>
{
    private const int DefaultCapacity = 16; //기본 사이즈
    private const double LoadFactor = 0.75; //기본 적재율

    private KeyValuePair<TKey, TValue>[] table; //해시 테이블 배열
    private bool[] occupied; //해당 인덱스가 사용중인지 확인하는 용도
    private int size; //실제 배열 사이즈
    private int count; //키 벨류 페어의 갯수

    public SimpleHashTable()
    {
        table = new KeyValuePair<TKey, TValue>[DefaultCapacity];
        occupied = new bool[DefaultCapacity];
        size = DefaultCapacity;
        count = 0;
    }

    //사이즈가 들어왔을 경우
    private int GetIndex(TKey key, int size)
    {
        if(key == null)
        {
            throw new ArgumentException(nameof(key));
        }

        int hash = key.GetHashCode(); //해쉬 코드 가져오기
        return Math.Abs(hash) % size; //배열의 사이즈가 바뀔때마다 다른 인덱스를 반환하게 된다.
    }

    private int GetIndex(TKey key)
    {
        return GetIndex(key, this.size);
    }

    public TValue this[TKey key]
    {
        get
        {
            if (TryGetValue(key, out TValue value))
            {
                //키가 있는 경우
                return value;
            }

            //키가 없는 경우
            throw new System.NotImplementedException("키 없음!");
        }

        set
        {
            if (key == null)
            {
                throw new System.NotImplementedException(nameof(key));
            }

            //키가 이미 있으면 value 교체
            int index = GetIndex(key);
            if (occupied[index] && table[index].Key.Equals(key))
            {
                table[index] = new KeyValuePair<TKey, TValue>(key, value);
            }
            else if (!occupied[index]) //키가 사용중이지 않으면
            {
                table[index] = new KeyValuePair<TKey, TValue>(key, value);
                occupied[index] = true;
                count++;
            }
            else //해시 충돌이 일어난 경우
            {
                throw new InvalidOperationException("해시 충돌");
            }
        }
        }

    //Range(0, size) ; 0부터 size-1까지의 숫자 생성 (컬렉션이다.)
    public ICollection<TKey> Keys => Enumerable.Range(0, size).Where(i => occupied[i]).Select(i => table[i].Key).ToList();

    public ICollection<TValue> Values => Enumerable.Range(0, size).Where(i => occupied[i]).Select(i => table[i].Value).ToList();

    public int Count => count;

    public bool IsReadOnly => false;

    public void Add(TKey key, TValue value)
    {
        //널 체크는 나중에 필요하면 넣어라
        //목표 적재율을 넘으면 리사이즈를 해야한다.
        if ((double)count / size >= LoadFactor) //적재율 확인
        {
            Resize();
        }

        int index = GetIndex(key);
        if (!occupied[index]) //해당 인덱스가 비어있는지 확인
        {
            table[index] = new KeyValuePair<TKey, TValue>(key, value);
            occupied[index] = true; //사용 했다고 표시
            count++;
        }
        else if (table[index].Key.Equals(key)) //중복된 키 일 경우
        {
            throw new ArgumentException("이미 존재하는 키입니다.");
        }
        else //값이 충돌할 경우
        {
            throw new InvalidOperationException("해시 충돌");
        }
    }
    
    public void Resize()
    {
        int newSize = size * 2;
        var newTable = new KeyValuePair<TKey, TValue>[newSize];
        var newOccupied = new bool[newSize];

        for (int i = 0; i < size; i++)
        {
            if (!occupied[i])
            {
                continue;
            }

            //새로 만들어진 테이블에 할당
            int newIndex = GetIndex(table[i].Key, newSize);

            //예외 처리 : 바뀐 사이즈에서 해시 충돌이 일어날 경우
            if (newOccupied[newIndex])
            {
                throw new InvalidOperationException("해시 충돌 - 리사이즈 중");
            }

            newTable[newIndex] = table[i];
            newOccupied[newIndex] = true;
        }

        table = newTable;
        occupied = newOccupied;
        size = newSize;
    }

    public void Add(KeyValuePair<TKey, TValue> item)
    {
        Add(item.Key, item.Value);
    }

    public void Clear()
    {
        //테이블, 사용중 확인 배열, 요소 갯수 초기화
        Array.Clear(table, 0, size);
        Array.Clear(occupied, 0, size);
        count = 0;
    }

    public bool Contains(KeyValuePair<TKey, TValue> item)
    {
        if(TryGetValue(item.Key, out TValue value))
        {
            return value.Equals(item.Value);
        }

        return false;
    }

    public bool ContainsKey(TKey key)
    {
        if (key == null)
        {
            throw new ArgumentNullException(nameof(key));
        }

        int index = GetIndex(key);

        //키가 사용중인지 확인하고, 진짜 키가 같은지 확인
        return occupied[index] && table[index].Key.Equals(key); 
    }

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        int currentIndex = arrayIndex;
        foreach(var kvp in this)
        {
            array[currentIndex++] = kvp;
        }
    }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        for(int i = 0; i < size; i++)
        {
            if (occupied[i])
            {
                yield return table[i];
            }
        }
    }

    public bool Remove(TKey key)
    {
        if (key == null)
        {
            throw new ArgumentNullException(nameof(key));
        }

        int index = GetIndex(key);
        if (occupied[index] && table[index].Key.Equals(key))
        {
            //삭제 성공하는 경우
            occupied[index] = false;
            table[index] = default;
            count--;

            return true;
        }

        return false;
    }

    public bool Remove(KeyValuePair<TKey, TValue> item)
    {
        return Remove(item.Key);
    }

    public bool TryGetValue(TKey key, out TValue value)
    {
        if (key == null)
        {
            throw new ArgumentNullException(nameof(key));
        }

        int index = GetIndex(key);
        if (occupied[index] && table[index].Key.Equals(key))
        {
            value = table[index].Value;
            return true;
        }

        value = default;
        return false;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
