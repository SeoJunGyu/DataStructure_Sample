using UnityEngine;

public class TreeNode<TKey, TValue>
{
    public TKey Key { get; set; }
    public TValue Value { get; set; }
    public int Height{ get; set; } //깊이

    public TreeNode<TKey, TValue> Left { get; set; }
    public TreeNode<TKey, TValue> Right { get; set; }

    public TreeNode(TKey key, TValue value)
    {
        Key = key;
        Value = value;
        Height = 1; //단말 노드의 높이는 1로 놓는다.
    }
}
