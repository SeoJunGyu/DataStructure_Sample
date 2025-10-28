using System;
using UnityEngine;

public class AVLTree<TKey, TValue> : BinarySearchTree<TKey, TValue> where TKey : IComparable<TKey>
{
    public AVLTree()
        : base()
    {

    }

    protected override TreeNode<TKey, TValue> Add(TreeNode<TKey, TValue> node, TKey key, TValue value)
    {
        node = base.Add(node, key, value);

        return Balance(node);
    }

    protected override TreeNode<TKey, TValue> AddOrUpdate(TreeNode<TKey, TValue> node, TKey key, TValue value)
    {
        node = base.AddOrUpdate(node, key, value);

        return Balance(node);
    }

    protected override TreeNode<TKey, TValue> Remove(TreeNode<TKey, TValue> node, TKey key)
    {
        node = base.Remove(node, key);

        if(node == null)
        {
            return null;
        }

        return Balance(node);
    }

    //좌우 높이 차 확인 -> 회전을 해야하나 하지 않아도 되나 판단
    protected int BalanceFactor(TreeNode<TKey, TValue> node)
    {
        return node == null ? 0 : Height(node.Left) - Height(node.Right);
    }

    //밸런스 작업 실행
    protected TreeNode<TKey, TValue> Balance(TreeNode<TKey, TValue> node)
    {
        int balanceFactor = BalanceFactor(node);

        //왼쪽이 큰 상황
        if (balanceFactor > 1)
        {
            //한번 더 돌려야하는지 확인
            //
            if(BalanceFactor(node.Left) < 0)
            {
                node.Left = RotateLeft(node.Left);
            }

            return RotateRight(node);
        }

        //오른쪽이 큰 상황
        if (balanceFactor < -1)
        {
            //한번 더 돌려야하는지 확인
            //
            if(BalanceFactor(node.Right) < 0)
            {
                node.Right = RotateLeft(node.Right);
            }

            return RotateLeft(node);
        }

        //다 아니라면 그대로 반환하면 된다.
        return node;
    }

    //LL 회전
    protected TreeNode<TKey, TValue> RotateRight(TreeNode<TKey, TValue> node)
    {
        var leftChild = node.Left;
        var rightSubtreeOfLeftChild = leftChild.Right; //왼쪽 자식의 오른쪽 서브트리 세이브

        leftChild.Right = node; //왼쪽 자식의 오른쪽 노드가 현재 노드가 된다.
        node.Left = rightSubtreeOfLeftChild;

        UpdateHeight(node);
        UpdateHeight(leftChild);

        return leftChild;
    }
    
    protected TreeNode<TKey, TValue> RotateLeft(TreeNode<TKey, TValue> node)
    {
        var rightChild = node.Right;
        var LeftSubtreeOfRightChild = rightChild.Left; //오른쪽 자식의 왼쪽 서브트리 세이브

        rightChild.Left = node; //오른쪽 자식의 왼쪽 노드가 현재 노드가 된다.
        node.Right = LeftSubtreeOfRightChild;

        UpdateHeight(node);
        UpdateHeight(rightChild);

        return rightChild;
    }
}
