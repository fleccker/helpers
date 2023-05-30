using System;
using System.Reflection;
using Fasterflect;

namespace helpers.Network.TypeUtils;

public struct NetworkField
{
    public NetworkFieldType Type { get; private set; }
    
    public bool IsStatic { get; private set; }
    
    public string Name { get; private set; }
    public string ValueTypeId { get; private set; }

    public static NetworkField Create(MemberInfo member, NetworkTypeRecorder networkTypeRecorder)
    {
        if (member.MemberType != MemberTypes.Property && member.MemberType != MemberTypes.Field) throw new ArgumentException($"Cannot create a network field from member type {member.MemberType}");
        return new NetworkField
        {
            Type = member.MemberType is MemberTypes.Field ? NetworkFieldType.Field : NetworkFieldType.Property,
            Name = member.Name,
            ValueTypeId = networkTypeRecorder.GetOrAdd(member.DeclaringType).Id,
            IsStatic = member.IsStatic()
        };
    }
}