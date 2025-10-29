using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class GraphNode
{
    public int id;
    public int weight = 1;

    public List<GraphNode> adjacents = new List<GraphNode>();

    public GraphNode Previous = null;

    public bool CanVisit => adjacents.Count > 0;
}
