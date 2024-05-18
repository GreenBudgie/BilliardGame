using System;
using System.Linq;
using System.Reflection;
using Godot;

public static class NodeExtension
{
    private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

    public static void InitAttributes(this Node node)
    {
        var fields = node.GetType().GetFields(Flags);
        var properties = node.GetType().GetProperties(Flags);
        var fieldsAndProperties = fields.Concat(properties.Cast<MemberInfo>());
        foreach (var member in fieldsAndProperties)
        {
            InitAttribute(node, member);
        }
    }

    private static void InitAttribute(Node node, MemberInfo member)
    {
        var attribute = Attribute.GetCustomAttribute(member, typeof(NodeAttribute));
        if (attribute is NodeAttribute nodeAttribute)
        {
            nodeAttribute.Init(node, member);
        }
    }
}