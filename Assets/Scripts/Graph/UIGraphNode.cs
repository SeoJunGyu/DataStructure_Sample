using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;

public class UIGraphNode : MonoBehaviour
{
    public Image image;
    public TextMeshProUGUI text;

    public void Reset()
    {
        SetColor(node.CanVisit ? Color.white : Color.gray);
        SetText($"ID: {node.id}\nWeight: {node.weight}");
    }

    private GraphNode node;

    public void SetNode(GraphNode node)
    {
        this.node = node;
    }

    public void SetColor(Color color)
    {
        image.color = color;
    }

    public void SetText(string text)
    {
        this.text.text = text;
    }
}
