using System;
using System.IO;

namespace helpers.IO.Binary.Serializers
{
    public class DelegateSerializer : BinarySerializerBase
    {
        private Func<Type, BinaryReader, object> _deserialize;
        private Action<Type, BinaryWriter, object> _serialize;

        private Type[] _acceptedTypes;

        public override Type[] AcceptedTypes => _acceptedTypes ?? (_acceptedTypes = new Type[] { });

        public DelegateSerializer(Func<Type, BinaryReader, object> deserialize, Action<Type, BinaryWriter, object> serialize, params Type[] acceptedTypes)
        {
            _deserialize = deserialize;
            _serialize = serialize;
            _acceptedTypes = acceptedTypes;
        }

        public override object Deserialize(Type type, BinaryReader reader)
        {
            return _deserialize?.Invoke(type, reader);
        }

        public override void Serialize(object obj, BinaryWriter writer)
        {
            _serialize?.Invoke(obj.GetType(), writer, obj);
        }
    }
}