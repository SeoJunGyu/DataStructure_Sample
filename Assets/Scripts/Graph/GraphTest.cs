using System.Collections.Generic;
using UnityEngine;

public class GraphTest : MonoBehaviour
{
    public enum Algorithm
    {
        DFS,
        BFS,
        DFSRecursiv,
        PathFindingBFS,
        Dikjstra,
        AStar,

    }

    public UIGraphNode nodePrefab;
    public List<UIGraphNode> uiNodes;

    public Transform uiNodeRoot;

    private Graph graph;

    void Start()
    {
        int[,] map = new int[5, 5]
        {
            {1, -1, 1, 3, 1},
            {1, -1, 1, 1, 1},
            {1, -1, 8, 5, 1},
            {1, -1, 3, 1, 1},
            {1, 1, 1, 1, 1}
        };

        graph = new Graph();
        graph.Init(map);
        InitUiNodes(graph);
    }

    private void InitUiNodes(Graph graph)
    {
        foreach (var node in graph.nodes)
        {
            var uiNode = Instantiate(nodePrefab, uiNodeRoot);
            uiNode.SetNode(node);
            uiNode.Reset();
            uiNodes.Add(uiNode);
        }
    }

    private void ResetUiNodes()
    {
        foreach (var uiNode in uiNodes)
        {
            uiNode.Reset();
        }
    }

    public Algorithm algorithm;
    public int startIndex; //시작 노드 인덱스
    public int endIndex; //도착 노드 인덱스
    [ContextMenu("Search")]
    public void Search()
    {
        var search = new GraphSearch();
        search.Init(graph);

        switch (algorithm)
        {
            case Algorithm.DFS:
                search.DFS(graph.nodes[startIndex]);
                break;
            case Algorithm.BFS:
                search.BFS(graph.nodes[startIndex]);
                break;
            case Algorithm.DFSRecursiv:
                search.RecursiveDFS(graph.nodes[startIndex]);
                break;
            case Algorithm.PathFindingBFS:
                search.PathFindingBFS(graph.nodes[startIndex], graph.nodes[endIndex]);
                break;
            case Algorithm.Dikjstra:
                search.Dikjstra(graph.nodes[startIndex], graph.nodes[endIndex]);
                break;
            case Algorithm.AStar:
                search.AStar(graph.nodes[startIndex], graph.nodes[endIndex]);
                break;
        }

        ResetUiNodes();

        for(int i = 0; i < search.path.Count; i++)
        {
            var node = search.path[i]; //path에는 방문한 노드들이 담겨있다.
            var color = Color.Lerp(Color.red, Color.green, (float)i / (search.path.Count - 1));
            uiNodes[node.id].SetColor(color);
            uiNodes[node.id].SetText($"ID: {node.id}\nWeight: {node.weight} \nPath:{i}");
        }
    }
}
