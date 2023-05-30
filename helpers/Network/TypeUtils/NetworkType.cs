using System;
using System.Collections.Generic;

namespace helpers.Network.TypeUtils;

public class NetworkType
{
    public string AssemblyName { get; private set; }
    public string Name { get; private set; }
    public string Namespace { get; private set; }
    public string Id { get; private set; }
    
    public NetworkField[] Fields { get; private set; }

    public static NetworkType Create(Type type, NetworkTypeRecorder networkTypeRecorder)
    {
        networkTypeRecorder ??= NetworkTypeRecorder.GlobalRecorder;

        var fields = new List<NetworkField>();
        var networkType = new NetworkType
        {
            AssemblyName = type.AssemblyQualifiedName,
            Name = type.Name,
            Namespace = type.Namespace,
            Id = NetworkTypeRecorder.NextTypeId
        };
        
        foreach (var field in type.GetFields()) fields.Add(NetworkField.Create(field, networkTypeRecorder));
        foreach (var property in type.GetProperties()) fields.Add(NetworkField.Create(property, networkTypeRecorder));

        networkType.Fields = fields.ToArray();
        return networkType;
    }
}