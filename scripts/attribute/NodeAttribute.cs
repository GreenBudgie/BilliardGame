using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Godot;

/// <summary>
/// Allows to get the specified child node or a list of child nodes without calling <see cref="Node.GetNode"/>.
/// Providing node name in constructor is only required when multiple child nodes of this type may exist.
/// Invoke <see cref="NodeExtension.InitAttributes"/> at the first line of <see cref="Node._Ready"/> method
/// for this to work.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class NodeAttribute : Attribute
{
    public string NodeName { get; }

    public NodeAttribute(string nodeName = null)
    {
        NodeName = nodeName;
    }

    public void Init(Node node, MemberInfo member)
    {
        var nodeName = NodeName;

        if (nodeName == null)
        {
            SetNodesByType(node, member);
        }
        else
        {
            SetNodeByName(node, member);
        }
    }

    private void SetNodeByName(Node node, MemberInfo member)
    {
        var childNode = node.GetNode(NodeName) ??
                        throw new Exception($"No node with name {NodeName} exist on {node.Name}");
        AssignToMember(node, childNode, member);
    }

    private void SetNodesByType(Node node, MemberInfo member)
    {
        var nodeTypeInfo = GetNodeType(member);
        var nodeType = nodeTypeInfo.Type;
        var children = node.GetChildren().Where(child => nodeType.IsInstanceOfType(child)).ToList();

        if (nodeTypeInfo.IsList)
        {
            AssignToMember(node, children, member);
            return;
        }

        var firstNode = children.FirstOrDefault() ??
                        throw new Exception($"No node of type {nodeTypeInfo} exist on {node.Name}");
        AssignToMember(node, firstNode, member);
    }

    private void AssignToMember(Node rootNode, object value, MemberInfo member)
    {
        switch (member)
        {
            case FieldInfo field:
                field.SetValue(rootNode, value);
                break;
            case PropertyInfo property:
                property.SetValue(rootNode, value);
                break;
            default:
                throw new Exception("Unexpected member used for node attribute");
        }
    }

    private NodeTypeInfo GetNodeType(MemberInfo member)
    {
        var rawType = member switch
        {
            FieldInfo field => field.FieldType,
            PropertyInfo property => property.PropertyType,
            _ => throw new Exception("Unexpected member used for node attribute")
        };
        if (!rawType.IsAssignableTo(typeof(Node)))
        {
            throw new Exception($"Node attribute is used on a wrong type: \"{rawType}\"");
        }

        if (!rawType.IsGenericType)
        {
            return new NodeTypeInfo(rawType, false);
        }

        if (!rawType.GetGenericTypeDefinition().IsAssignableFrom(typeof(List<>)))
        {
            throw new Exception("Can only assign a list of nodes to a List type or its supertypes");
        }

        return new NodeTypeInfo(rawType.GetGenericArguments()[0], true);
    }

    private record NodeTypeInfo(Type Type, bool IsList);
}