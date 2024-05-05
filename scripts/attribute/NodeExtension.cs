using System;
using System.Linq;
using System.Reflection;
using Common;
using Godot;

public static class NodeExtension
{
    private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

    public static void InitAttributes(this Node node)
    {
        foreach (var field in node.GetType().GetFields(Flags))
        {
            InitNodeAttribute(node, field);
        }
    }

    private static void InitNodeAttribute(Node node, FieldInfo field)
    {
        var attribute = Attribute.GetCustomAttribute(field, typeof(NodeAttribute));
        if (attribute is not NodeAttribute nodeAttribute)
        {
            return;
        }

        var nodeName = nodeAttribute.NodeName;

        var childNode = nodeName == null
            ? FindNodeByType(node, field)
            : node.GetNode(nodeName) ??
              throw new Exception($"No node with name {nodeName} exist on {node.Name}");
        field.SetValue(node, childNode);
    }

    private static Node FindNodeByType(Node rootNode, FieldInfo field)
    {
        var nodeType = field.FieldType;
        var childNode = rootNode.GetChildren().FirstOrDefault(child => nodeType.IsInstanceOfType(child)) ??
                        throw new Exception($"No node of type {nodeType} exist on {rootNode.Name}");
        return childNode;
    }
}