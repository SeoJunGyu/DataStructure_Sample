using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework.Constraints;
using NUnit.Framework.Interfaces;
using UnityEngine;
using UnityEngine.InputSystem;

public class BinarySearchTree<TKey, TValue> : IDictionary<TKey, TValue> where TKey : IComparable<TKey>
{
    protected TreeNode<TKey, TValue> root;

    public BinarySearchTree()
    {
        root = null;
    }

    public TValue this[TKey key]
    {
        get
        {
            if (TryGetValue(key, out TValue value))
            {
                return value;
            }
            else
            {
                throw new KeyNotFoundException($"키 {key}를 찾을 수 없습니다.");
            }
        }
        set
        {
            root = AddOrUpdate(root, key, value);
        }
    }
    
    protected virtual TreeNode<TKey, TValue> AddOrUpdate(TreeNode<TKey, TValue> node, TKey key, TValue value)
    {
        //비교 후 왼쪽자식, 오른쪽 자식에 재귀호출시킨다.
        if (node == null)
        {
            return new TreeNode<TKey, TValue>(key, value);
        }

        int compare = key.CompareTo(node.Key);
        if (compare < 0)
        {
            //재귀 호출 - 왼쪽
            node.Left = AddOrUpdate(node.Left, key, value);
        }
        else if (compare > 0)
        {
            //재귀 호출 - 오른쪽
            node.Right = AddOrUpdate(node.Right, key, value);
        }
        else
        {
            //키를 업데이트 해야하는 경우
            node.Value = value;
        }

        UpdateHeight(node);

        return node;
    }

    public ICollection<TKey> Keys => InOrderTraversal().Select(kvp => kvp.Key).ToList();

    public ICollection<TValue> Values => InOrderTraversal().Select(kvp => kvp.Value).ToList();

    public int Count => CountNodes(root);

    protected virtual int CountNodes(TreeNode<TKey, TValue> node)
    {
        if (node == null)
        {
            return 0;
        }


        //왜 1부터 시작인가
        return 1 + CountNodes(node.Left) + CountNodes(node.Right);
    }

    public bool IsReadOnly => false;
    public void Add(TKey key, TValue value)
    {
        root = Add(root, key, value); //재귀적으로 노드를 추가하여 내가 있을 자리를 찾음
    }

    public void Add(KeyValuePair<TKey, TValue> item)
    {
        Add(item.Key, item.Value);
    }

    protected virtual TreeNode<TKey, TValue> Add(TreeNode<TKey, TValue> node, TKey key, TValue value)
    {
        if (node == null)
        {
            return new TreeNode<TKey, TValue>(key, value); //새 노드 생성
        }

        int compare = key.CompareTo(node.Key);
        if (compare < 0)
        {
            node.Left = Add(node.Left, key, value); //왼쪽 서브트리에 삽입 -> 재귀 호출 - null이 나올때까지 반복한다.
        }
        else if (compare > 0)
        {
            node.Right = Add(node.Right, key, value); //오른쪽 서브트리에 삽입
        }
        else
        {
            //node.Value = value; //키가 이미 존재하면 값을 업데이트 -> 중복된 값 허용
            throw new ArgumentException($"키 {key}는 이미 존재합니다."); //중복된 값 허용 안함
        }

        UpdateHeight(node);

        return node; //변경된 노드 반환
    }

    public void Clear()
    {
        root = null;
    }

    public bool Contains(KeyValuePair<TKey, TValue> item)
    {
        return ContainsKey(item.Key);
    }

    public bool ContainsKey(TKey key)
    {
        return TryGetValue(key, out _);
    }

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        //예외
        foreach(var item in this)
        {
            array[arrayIndex++] = item;
        }
    }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        return InOrderTraversal().GetEnumerator();
    }

    public bool Remove(TKey key)
    {
        int initialCount = Count;
        root = Remove(root, key);
        return Count < initialCount; //삭제가 되었다면 true, 안되면 fasle;
    }

    public bool Remove(KeyValuePair<TKey, TValue> item)
    {
        return Remove(item.Key);
    }

    protected virtual TreeNode<TKey, TValue> Remove(TreeNode<TKey, TValue> node, TKey key)
    {
        //지우려고 했던 노드가 사라지는 경우 
        if (node == null)
        {
            return node;
        }

        int compare = key.CompareTo(node.Key);
        if (compare < 0)
        {
            node.Left = Remove(node.Left, key);
        }
        else if (compare > 0)
        {
            node.Right = Remove(node.Right, key);
        }
        else
        {
            if (node.Left == null)
            {
                return node.Right;
            }
            else if (node.Right == null)
            {
                return node.Left;
            }

            TreeNode<TKey, TValue> minNode = FindMin(node.Right);

            node.Key = minNode.Key;
            node.Value = minNode.Value;

            node.Right = Remove(node.Right, minNode.Key);
        }

        UpdateHeight(node);

        return node;
    }

    protected virtual TreeNode<TKey, TValue> FindMin(TreeNode<TKey, TValue> node)
    {
        while(node.Left != null)
        {
            node = node.Left;
        }

        return node;
    }

    public bool TryGetValue(TKey key, out TValue value)
    {
        return TryGetValue(root, key, out value);
    }

    public bool TryGetValue(TreeNode<TKey, TValue> node, TKey key, out TValue value)
    {
        if (node == null)
        {
            value = default(TValue);
            return false;
        }

        int compare = key.CompareTo(node.Key);
        if (compare == 0)
        {
            value = node.Value;
            return true;
        }
        else if (compare < 0)
        {
            return TryGetValue(node.Left, key, out value);
        }
        else
        {
            return TryGetValue(node.Right, key, out value);
        }
    }

    //순회를 위한 IEnumerable 구현
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    //중위 순회
    public virtual IEnumerable<KeyValuePair<TKey, TValue>> InOrderTraversal()
    {
        return InOrderTraversal(root);
    }

    public virtual IEnumerable<KeyValuePair<TKey, TValue>> InOrderTraversal(TreeNode<TKey, TValue> node)
    {
        if(node != null)
        {
            foreach(var kvp in InOrderTraversal(node.Left))
            {
                yield return kvp; //모든 키 벨류 페어가 리턴된다.
            }

            yield return new KeyValuePair<TKey, TValue>(node.Key, node.Value);

            foreach(var kvp in InOrderTraversal(node.Right))
            {
                yield return kvp; //모든 키 벨류 페어가 리턴된다.
            }
        }
    }

    //전위 순회
    public virtual IEnumerable<KeyValuePair<TKey, TValue>> PreOrderTraversal()
    {
        return PreOrderTraversal(root);
    }

    public virtual IEnumerable<KeyValuePair<TKey, TValue>> PreOrderTraversal(TreeNode<TKey, TValue> node)
    {
        if(node != null)
        {
            yield return new KeyValuePair<TKey, TValue>(node.Key, node.Value);

            foreach(var kvp in PreOrderTraversal(node.Left))
            {
                yield return kvp; //모든 키 벨류 페어가 리턴된다.
            }

            foreach(var kvp in PreOrderTraversal(node.Right))
            {
                yield return kvp; //모든 키 벨류 페어가 리턴된다.
            }
        }
    }

    //후위 순회
    public virtual IEnumerable<KeyValuePair<TKey, TValue>> PostOrderTraversal()
    {
        return PostOrderTraversal(root);
    }

    public virtual IEnumerable<KeyValuePair<TKey, TValue>> PostOrderTraversal(TreeNode<TKey, TValue> node)
    {
        if (node != null)
        {
            foreach (var kvp in PostOrderTraversal(node.Left))
            {
                yield return kvp; //모든 키 벨류 페어가 리턴된다.
            }

            foreach (var kvp in PostOrderTraversal(node.Right))
            {
                yield return kvp; //모든 키 벨류 페어가 리턴된다.
            }

            yield return new KeyValuePair<TKey, TValue>(node.Key, node.Value);
        }
    }
    
    public virtual IEnumerable<KeyValuePair<TKey, TValue>> LevelOrderTraversal()
    {
        return LevelOrderTraversal(root);
    }

    public virtual IEnumerable<KeyValuePair<TKey, TValue>> LevelOrderTraversal(TreeNode<TKey, TValue> node)
    {
        if (node == null)
        {
            yield break;
        }

        Queue<TreeNode<TKey, TValue>> queue = new Queue<TreeNode<TKey, TValue>>();
        queue.Enqueue(node);

        //순서 : 반환 -> 왼쪽 확인 : 있으면 Enque -> 오른쪽 확인 : 있으면 Enque -> queue.Count 확인 : 0 아니면 반복
        while (queue.Count > 0)
        {
            TreeNode<TKey, TValue> current = queue.Dequeue();
            yield return new KeyValuePair<TKey, TValue>(current.Key, current.Value);

            if (current.Left != null)
            {
                queue.Enqueue(current.Left);
            }

            if (current.Right != null)
            {
                queue.Enqueue(current.Right);
            }
        }
    }

    protected virtual int Height(TreeNode<TKey, TValue> node)
    {
        return node == null ? 0 : node.Height;
    }
    
    protected virtual void UpdateHeight(TreeNode<TKey, TValue> node)
    {
        node.Height = Mathf.Max(Height(node.Left), Height(node.Right)) + 1; //내 높이 + 좌우 자식 중 큰 높이
    }
}