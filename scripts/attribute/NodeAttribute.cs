using System;

/// <summary>
/// Allows to get the specified child node without calling <see cref="Node.GetNode"/>.
/// Providing node name in constructor is only required when multiple child nodes of this type may exist.
/// Invoke <see cref="NodeExtension.InitAttributes"/> at the first line of <see cref="Node._Ready"/> method
/// for this to work.
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public class NodeAttribute : Attribute
{

    public string NodeName { get; }

    public NodeAttribute(string nodeName = null)
    {
        NodeName = nodeName;
    }
    
}