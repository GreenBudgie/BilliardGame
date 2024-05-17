using System;
using System.Linq;
using System.Reflection;
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
        foreach (var property in node.GetType().GetProperties(Flags))
        {
            InitNodeAttribute(node, property);
        }
    }

    private static void InitNodeAttribute(Node node, MemberInfo member)
    {
        var attribute = Attribute.GetCustomAttribute(member, typeof(NodeAttribute));
        if (attribute is not NodeAttribute nodeAttribute)
        {
            return;
        }

        var nodeName = nodeAttribute.NodeName;

        var childNode = nodeName == null
            ? FindNodeByType(node, member)
            : node.GetNode(nodeName) ??
              throw new Exception($"No node with name {nodeName} exist on {node.Name}");
        switch (member)
        {
            case FieldInfo field:
                field.SetValue(node, childNode);
                break;
            case PropertyInfo property:
                property.SetValue(node, childNode);
                break;
        }
    }

    private static Node FindNodeByType(Node rootNode, MemberInfo member)
    {
        Type nodeType;
        switch (member)
        {
            case FieldInfo field:
                nodeType = field.FieldType;
                break;
            case PropertyInfo property:
                nodeType = property.PropertyType;
                break;
            default:
                throw new Exception("Unexpected member used for node attribute");
        }
        var childNode = rootNode.GetChildren().FirstOrDefault(child => nodeType.IsInstanceOfType(child)) ??
                        throw new Exception($"No node of type {nodeType} exist on {rootNode.Name}");
        return childNode;
    }
}