using System;
using System.Collections.Generic;
using System.Linq;

namespace helpers.Network.TypeUtils;

public class NetworkTypeRecorder
{
    private readonly Dictionary<string, NetworkType> _recordedTypes = new Dictionary<string, NetworkType>();
    private const int TypeRecordIdSize = 15;

    public static string NextTypeId => helpers.RandomGenerator.Ticket(TypeRecordIdSize);
    public static NetworkTypeRecorder GlobalRecorder { get; } = new NetworkTypeRecorder();

    public Type GetRecordType(NetworkType record) => Type.GetType(record.AssemblyName);
    public string GetRecordId(Type type) => _recordedTypes.Values.FirstOrDefault(x => x.AssemblyName == type.AssemblyQualifiedName)?.Id ?? null;
    public NetworkType GetRecord(Type type)
    {
        var recordId = GetRecordId(type);
        if (recordId is null) return null;
        else return _recordedTypes[recordId];
    }
    
    public NetworkType GetOrAdd(Type type)
    {
        if (!HasRecord(type)) return AddRecord(type);
        else return GetRecord(type);
    }
    
    public NetworkType AddRecord(Type type)
    {
        if (HasRecord(type)) throw new ArgumentException($"Type {type.AssemblyQualifiedName} has already been recorded.");
        var record = NetworkType.Create(type, this);
        _recordedTypes[record.Id] = record;
        return record;
    }
    
    public bool HasRecord(Type type) => _recordedTypes.Values.Any(x => x.AssemblyName == type.AssemblyQualifiedName);
}