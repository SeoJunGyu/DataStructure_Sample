using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class GraphSearch : MonoBehaviour
{
    private Graph graph;
    public List<GraphNode> path = new List<GraphNode>();

    public void Init(Graph graph)
    {
        this.graph = graph;
    }

    //시작 노드를 받아 순회하는 함수
    public void DFS(GraphNode node)
    {
        path.Clear();

        var visited = new HashSet<GraphNode>(); //방문한 노드 저장
        var stack = new Stack<GraphNode>();

        stack.Push(node);
        while (stack.Count > 0)
        {
            var currentNode = stack.Pop(); //
            path.Add(currentNode); //방문했다고 저장
            visited.Add(currentNode); //방문 했다고 확인할 용도로 저장

            foreach (var adjacent in currentNode.adjacents)
            {
                //방문했던 노드는 저장하면 안되고, 방문 못하는 노드도 저장하면 안된다. / 또 두번 방문하면 안된다.
                if (!adjacent.CanVisit || visited.Contains(adjacent) || stack.Contains(adjacent))
                {
                    continue;
                }

                stack.Push(adjacent);
            }
        }
    }

    //시작 노드를 받아 순회하는 함수
    //BFS는 스택이 아니라 큐에 넣는것이다.
    public void BFS(GraphNode node)
    {
        path.Clear();

        var visited = new HashSet<GraphNode>(); //방문한 노드 저장
        var queue = new Queue<GraphNode>();

        queue.Enqueue(node);
        while (queue.Count > 0)
        {
            var currentNode = queue.Dequeue(); //
            path.Add(currentNode); //방문했다고 저장
            visited.Add(currentNode); //방문 했다고 확인할 용도로 저장

            foreach (var adjacent in currentNode.adjacents)
            {
                //방문했던 노드는 저장하면 안되고, 방문 못하는 노드도 저장하면 안된다. / 또 두번 방문하면 안된다.
                if (!adjacent.CanVisit || visited.Contains(adjacent) || queue.Contains(adjacent))
                {
                    continue;
                }

                queue.Enqueue(adjacent);
            }
        }
    }

    public void RecursiveDFS(GraphNode node)
    {
        path.Clear();
        RecursiveDFS(node, new HashSet<GraphNode>());
    }

    protected void RecursiveDFS(GraphNode node, HashSet<GraphNode> visited)
    {
        path.Add(node); //방문했다고 저장
        visited.Add(node); //방문 했다고 확인할 용도로 저장

        foreach (var adjacent in node.adjacents)
        {
            //방문했던 노드는 저장하면 안되고, 방문 못하는 노드도 저장하면 안된다.
            if (!adjacent.CanVisit || visited.Contains(adjacent))
            {
                continue;
            }

            RecursiveDFS(adjacent, visited);
        }
    }

    internal bool PathFindingBFS(GraphNode startNode, GraphNode endNode)
    {
        path.Clear();

        graph.ResetNodePrevious();

        var visited = new HashSet<GraphNode>(); //방문한 노드 저장
        var queue = new Queue<GraphNode>();

        queue.Enqueue(startNode);
        bool success = false;

        var currentNode = queue.Peek();

        while (queue.Count > 0)
        {
            currentNode = queue.Dequeue();
            if(currentNode == endNode)
            {
                success = true;
                break;
            }

            visited.Add(currentNode); //방문 했다고 확인할 용도로 저장

            foreach (var adjacent in currentNode.adjacents)
            {
                //방문했던 노드는 저장하면 안되고, 방문 못하는 노드도 저장하면 안된다. / 또 두번 방문하면 안된다.
                if (!adjacent.CanVisit || visited.Contains(adjacent) || queue.Contains(adjacent))
                {
                    continue;
                }

                adjacent.Previous = currentNode;
                queue.Enqueue(adjacent);
            }
        }

        if (!success)
        {
            return false;
        }

        GraphNode step = endNode;
        while (step != null)
        {
            path.Add(step);
            step = step.Previous;
        }

        path.Reverse();
        return true;
        
    }
}
