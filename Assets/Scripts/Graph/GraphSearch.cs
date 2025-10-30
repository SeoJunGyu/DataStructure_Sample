using System;
using System.Collections.Generic;
using Unity.Properties;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

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
            if (currentNode == endNode)
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

    //다익스트라 알고리즘
    public bool Dikjstra(GraphNode start, GraphNode goal)
    {
        path.Clear();
        graph.ResetNodePrevious();

        var visited = new HashSet<GraphNode>(); //방문한 노드 확인용
        var pQueue = new PriorityQueue<GraphNode, int>(); //int : 거리(가중치) 를 이용해서 거리를 측정한다. (가중치는 이동될때마다 누적되서 갱신한다.)

        var distances = new int[graph.nodes.Length]; //모든 노드들의 거리 배열 / 노드의 id가 인덱스가 될 것이다.
        for (int i = 0; i < distances.Length; i++)
        {
            distances[i] = int.MaxValue; //초기값은 int 최대값으로 한다.
        }

        distances[start.id] = start.weight; //시작 노드 거리 저장
        pQueue.Enqueue(start, distances[start.id]); //우선순위 확인용 저장

        bool success = false; //경로가 없는 거리도 있으니 확인용
        while (pQueue.Count > 0)
        {
            var currentNode = pQueue.Dequeue();
            //이미 방문한 경우 건너 뛴다.
            //이미 방문된 노드를 저장하는 경우가 있다. -> 가중치가 변하는데 이게 다시 인접한 노드였을경우 추가하는 경우가 생길 수 있다.
            if (visited.Contains(currentNode))
            {
                continue;
            }

            //목적지에 도착했을 경우
            if (currentNode == goal)
            {
                success = true;
                break;
            }

            visited.Add(currentNode);

            foreach (var adjacent in currentNode.adjacents)
            {
                if (!adjacent.CanVisit || visited.Contains(adjacent))
                {
                    continue;
                }

                var newDistance = distances[currentNode.id] + adjacent.weight; //추가할 인접 노드의 가중치 갱신
                if (distances[adjacent.id] > newDistance)
                {
                    //무조건 갱신하는게 아니라 비교해서 더 클 경우 갱신하는 것이다.
                    distances[adjacent.id] = newDistance;
                    adjacent.Previous = currentNode;
                    pQueue.Enqueue(adjacent, newDistance);
                }
            }
        }

        //갈 수 있는 경로가 없는 경우
        if (!success)
        {
            return false;
        }

        //실제 길을 잇기 위한 왔던 노드 역추적
        GraphNode step = goal;
        while (step != null)
        {
            path.Add(step);
            step = step.Previous;
        }
        path.Reverse();

        return true;
    }

    //휴리스틱 함수
    protected int Heuristic(GraphNode a, GraphNode b)
    {
        //a의 x 인덱스와 y 인덱스
        int ax = a.id % graph.cols;
        int ay = a.id / graph.cols;

        int bx = b.id % graph.cols;
        int by = b.id / graph.cols;

        return Mathf.Abs(ax - bx) + Mathf.Abs(ay - by);
    }

    //A* 알고리즘
    public bool AStar(GraphNode start, GraphNode goal)
    {
        path.Clear();
        graph.ResetNodePrevious();

        var visited = new HashSet<GraphNode>(); //방문한 노드 확인용
        var pQueue = new PriorityQueue<GraphNode, int>(); //int : 거리(가중치) 를 이용해서 거리를 측정한다. (가중치는 이동될때마다 누적되서 갱신한다.)

        var distances = new int[graph.nodes.Length]; //모든 노드들의 거리 배열 / 노드의 id가 인덱스가 될 것이다.
        var scores = new int[graph.nodes.Length];
        for (int i = 0; i < distances.Length; i++)
        {
            scores[i] = distances[i] = int.MaxValue; //초기값은 int 최대값으로 한다.
        }

        distances[start.id] = start.weight;
        scores[start.id] = distances[start.id] + Heuristic(start, goal); // 기존 거리와 휴리스틱 값을 더한 계산값
        pQueue.Enqueue(start, scores[start.id]);

        bool success = false;
        while(pQueue.Count > 0)
        {
            var currentNode = pQueue.Dequeue();
            if (visited.Contains(currentNode))
            {
                continue;
            }

            if (currentNode == goal)
            {
                success = true;
                break;
            }

            visited.Add(currentNode);
            foreach(var adjacent in currentNode.adjacents)
            {
                if (!adjacent.CanVisit || visited.Contains(adjacent))
                {
                    continue;
                }

                var newDistance = distances[currentNode.id] + adjacent.weight; //인접 노드까지의 실제 비용(거리)
                if(distances[adjacent.id] > newDistance)
                {
                    distances[adjacent.id] = newDistance;
                    scores[adjacent.id] = distances[adjacent.id] + Heuristic(adjacent, goal);
                    adjacent.Previous = currentNode;

                    pQueue.Enqueue(adjacent, scores[adjacent.id]);
                }
            }
        }

        //갈 수 있는 경로가 없는 경우
        if (!success)
        {
            return false;
        }

        //실제 길을 잇기 위한 왔던 노드 역추적
        GraphNode step = goal;
        while (step != null)
        {
            path.Add(step);
            step = step.Previous;
        }
        path.Reverse();

        return true;
    }
}
