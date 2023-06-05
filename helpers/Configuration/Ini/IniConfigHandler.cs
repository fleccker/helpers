using helpers.Configuration.Converters;

using Fasterflect;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace helpers.Configuration.Ini
{
    [LogSource("Ini Handler")]
    public class IniConfigHandler
    {
        private Dictionary<object, Tuple<IniConfigAttribute, object>> _registeredConfigs = new Dictionary<object, Tuple<IniConfigAttribute, object>>();
        
        internal Dictionary<string, string> _paths = new Dictionary<string, string>();
        internal IConfigConverter _converter;

        public event Action<IniConfigAttribute, object> OnRegistered;

        public void Read()
        {
            var readers = new Dictionary<string, IniConfigReader>();

            foreach (var path in _paths.Values)
            {
                if (!File.Exists(path))
                {
                    Log.Debug("IniConfigHandler", $"Generating default file for: {path} (file missing)");

                    Save(path);
                    continue;
                }

                var text = File.ReadAllText(path);

                if (string.IsNullOrWhiteSpace(text))
                {
                    Log.Debug("IniConfigHandler", $"Generating default file for: {path} (file empty)");

                    Save(path);
                    continue;
                }

                readers[path] = new IniConfigReader(File.ReadAllLines(path));

                Log.Debug("IniConfigHandler", $"Assigned reader for path: {path}");
            }

            foreach (var reader in readers.Values)
            {
                while (reader.TryMove())
                {
                    if (!reader.IsLineValid)
                        continue;

                    Log.Debug($"Key: {reader.CurrentKey}");
                    Log.Debug($"Value: {reader.CurrentValue}");

                    var configType = _registeredConfigs.FirstOrDefault(x => x.Value.Item1.GetName() == reader.CurrentKey);

                    if (configType.Value is null)
                        continue;

                    var objectType = (configType.Key is PropertyInfo objectPropertyInfo ? objectPropertyInfo.PropertyType : (configType.Key as FieldInfo).FieldType);

                    if (_converter.TryConvert(reader.CurrentValue, objectType, out var result))
                    {
                        if (configType.Key is FieldInfo field)
                            field.SetValue(configType.Value.Item2, result);
                        else if (configType.Key is PropertyInfo property)
                            property.SetValue(configType.Value.Item2, result);
                        else
                            Log.Error("IniConfigHandler", $"Unknown config target: {configType.Key}");
                    }
                }
            }
        }

        public void Save()
        {
            var writers = new Dictionary<string, IniConfigWriter>();

            foreach (var key in _registeredConfigs)
            {
                var filePath = _paths[key.Value.Item1.GetPairName()];
                var writer = (writers.TryGetValue(filePath, out var writerValue) ? writerValue : (writers[filePath] = new IniConfigWriter(_converter)));

                writer.WriteObject(
                    key.Value.Item1.GetName(),   
                    key.Value.Item1.GetDescription(), 
                    
                    (key.Key is PropertyInfo property ? property.GetValue(key.Value.Item2) : (key.Key as FieldInfo).GetValue(key.Value.Item2)));
            }

            foreach (var pair in writers)
            {
                var value = pair.Value.ToString();

                File.WriteAllText(pair.Key, value);
            }
        }

        private void Save(string path)
        {

            var keys = _registeredConfigs.Where(x => _paths[x.Value.Item1.GetPairName()] == path);
            if (keys.Any())
            {
                var writer = new IniConfigWriter(_converter);

                foreach (var key in keys)
                {
                    writer.WriteObject(key.Value.Item1.GetName(), key.Value.Item1.GetDescription(), (key.Key is PropertyInfo property ? property.GetValue(key.Value.Item2) : (key.Key as FieldInfo).GetValue(key.Value.Item2)));
                }

                File.WriteAllText(path, writer.ToString());
            }
            else
            {
                Log.Warn("IniConfigHandler", $"No keys with the specified path were found.");
            }
        }

        public void Register(Type type, object typeInstance = null)
        {
            RegisterProperties(type, typeInstance);
            RegisterFields(type, typeInstance);
        }

        public bool IsRegistered(object targetType, object typeInstance = null)
        {
            foreach (var pair in _registeredConfigs)
            {
                if (pair.Key is PropertyInfo property 
                    && targetType is PropertyInfo targetProperty)
                {
                    if (property.Equals(targetProperty))
                    {
                        if (typeInstance != null)
                        {
                            if (pair.Value.Item2 != null)
                            {
                                return typeInstance == pair.Value.Item2;
                            }
                        }

                        return true;
                    }
                }
                else if (pair.Key is FieldInfo field
                    && targetType is FieldInfo targetField)
                {
                    if (field.Equals(targetField))
                    {
                        if (typeInstance != null)
                        {
                            if (pair.Value.Item2 != null)
                            {
                                return typeInstance == pair.Value.Item2;
                            }
                        }

                        return true;
                    }
                }
            }

            return false;
        }

        public void Unregister(Type type, object typeInstance = null) 
        {
            var removeKeys = new HashSet<object>();

            foreach (var pair in _registeredConfigs)
            {
                if (pair.Key is PropertyInfo property && property.DeclaringType == type)
                {
                    if (typeInstance != null 
                        && pair.Value.Item2 != null
                        && typeInstance != pair.Value.Item2)
                    {
                        continue;
                    }

                    removeKeys.Add(pair.Key);
                }
            }

            foreach (var key in removeKeys)
            {
                _registeredConfigs.Remove(key);
            }

            removeKeys.Clear();
            removeKeys = null;
        }

        private void RegisterProperties(Type type, object typeInstance = null)
        {
            foreach (var property in type.GetProperties())
            {
                if (property.TryGetAttribute<IniConfigAttribute>(out var iniConfigAttribute))
                {
                    var getMethod = property.GetGetMethod();
                    var setMethod = property.GetSetMethod();

                    if (getMethod is null
                        || setMethod is null)
                    {
                        Log.Warn("IniConfigHandler", $"Property \"{property.Name}\" of type \"{type.FullName}\" has the IniConfigAttribute, but is missing it's setter or getter.");
                        continue;
                    }

                    if (IsRegistered(property, (getMethod.IsStatic && setMethod.IsStatic) ? null : typeInstance))
                    {
                        Log.Warn("IniConfigHandler", $"Property \"{property.Name}\" of type \"{type.FullName}\" is already registered.");
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(iniConfigAttribute.GetName()))
                        iniConfigAttribute._name = property.Name;

                    _registeredConfigs[property] = new Tuple<IniConfigAttribute, object>(iniConfigAttribute, typeInstance);
                    OnRegistered?.Invoke(iniConfigAttribute, typeInstance);
                }
            }
        }

        private void RegisterFields(Type type, object typeInstance = null)
        {
            foreach (var field in type.GetFields())
            {
                if (field.TryGetAttribute<IniConfigAttribute>(out var iniConfigAttribute))
                {
                    if (!field.IsWritable()
                        || !field.IsReadable())
                        continue;

                    if (IsRegistered(field, typeInstance))
                        continue;

                    if (string.IsNullOrWhiteSpace(iniConfigAttribute.GetName()))
                        iniConfigAttribute._name = field.Name;

                    _registeredConfigs[field] = new Tuple<IniConfigAttribute, object>(iniConfigAttribute, typeInstance);
                    OnRegistered?.Invoke(iniConfigAttribute, typeInstance);
                }
            }
        }
    }
}