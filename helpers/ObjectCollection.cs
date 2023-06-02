using System;
using System.Collections.Generic;
using System.Linq;

namespace helpers
{

    public class ObjectCollection
    {
        private List<object> _collection = new List<object>();
        private List<string> _itemIds = new List<string>();

        public MethodLock Lock { get; }

        public ObjectCollection(bool useLock = false)
        {
            if (useLock) Lock = new MethodLock();
            else Lock = null;
        }

        public ObjectCollection Clear()
        {
            if (Lock != null) Lock.ThrowIfUnauthorized();
            _collection.Clear();
            _itemIds.Clear();
            return this;
        }

        public ObjectCollection Add(object obj, string identifier = null)
        {
            if (Lock != null) Lock.ThrowIfUnauthorized();
            _collection.Add(obj);
            if (!string.IsNullOrWhiteSpace(identifier))
            {
                if (_itemIds.Contains(identifier)) throw new ArgumentException($"An item with ID {identifier} already exists in this collection.");
                _itemIds.Add(identifier);
            }
            else _itemIds.Add(null);

            return this;
        }

        public ObjectCollection Remove(object instance)
        {
            var index = _collection.IndexOf(instance);
            if (index < 0) throw new KeyNotFoundException($"This object does not exist in this collection.");
            else
            {
                _collection.RemoveAt(index);
                _itemIds.RemoveAt(index);
            }

            return this;
        }

        public ObjectCollection Replace<TObject>(TObject newInstance, string newId = null)
        {
            var index = IndexOf(typeof(TObject));
            if (index < 0) throw new KeyNotFoundException($"An item of type {typeof(TObject).FullName} does not exist in this collection!");
            _collection[index] = newInstance;
            if (_itemIds[index] is null && newId != null) _itemIds[index] = newId;
            return this;
        }

        public ObjectCollection Replace(string itemId, object newInstance)
        {
            var index = IndexOf(itemId);
            if (index < 0)
                throw new KeyNotFoundException($"An item with ID {itemId} has not been found in this collection.");
            else _collection[index] = newInstance;
            return this;
        }

        public int IndexOf(Type type)
        {
            var instance = _collection.FirstOrDefault(x => x != null && x.GetType() == type);
            if (instance != null) return _collection.IndexOf(instance);
            else return -1;
        }

        public int IndexOf(string itemId)
        {
            var index = _itemIds.IndexOf(itemId);
            if (index < 0) return -1;
            else return index;
        }

        public int IndexOf(object instance)
        {
            var index = _collection.IndexOf(instance);
            if (index < 0) return -1;
            else return index;
        }

        public string IdOf(object instance)
        {
            var index = IndexOf(instance);
            if (index is -1) throw new KeyNotFoundException($"Cannot find {instance} in this collection.");
            else return _itemIds[index];
        }

        public TObject Get<TObject>(string identifier = null)
        {
            if (!string.IsNullOrWhiteSpace(identifier))
            {
                var index = _itemIds.IndexOf(identifier);
                if (index < 0) throw new KeyNotFoundException($"An item with ID {identifier} does not exist in this collection.");
                if (index >= _collection.Count) throw new IndexOutOfRangeException($"Identifier {identifier} seems to have been removed improperly!");
                if (!(_collection[index] is TObject tObj)) throw new InvalidCastException($"Item of type {_collection[index]?.GetType()?.FullName ?? "null"} cannot be cast to {typeof(TObject).FullName}!");
                return tObj;
            }
            else
            {
                var obj = _collection.FirstOrDefault(x => x is TObject);
                if (obj is null) throw new KeyNotFoundException($"Item of type {typeof(TObject).FullName} does not exist in this collection!");
                if (!(obj is TObject tObj)) throw new InvalidCastException($"Item of type {obj?.GetType()?.FullName ?? "null"} cannot be cast to {typeof(TObject).FullName}!");
                return tObj;
            }
        }
    }
}