using helpers.Configuration.Converters;
using helpers.Timeouts;
using helpers.Extensions;
using helpers.Events;

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
        public const string IniFilter = "*.ini";

        private bool _saveHistory;
        private bool _useWatcher;

        private string _path;

        private ConfigNamingRule _rule = ConfigNamingRule.SetValue;

        private Dictionary<object, Tuple<IniConfigAttribute, object>> _registeredConfigs = new Dictionary<object, Tuple<IniConfigAttribute, object>>();
        private FileSystemWatcher _watcher;
        private IniConfigReader _reader;

        internal IConfigConverter _converter;

        public readonly EventProvider OnRegistered = new EventProvider();
        public readonly EventProvider OnReadStarted = new EventProvider();
        public readonly EventProvider OnReadConfig = new EventProvider();
        public readonly EventProvider OnReadFinished = new EventProvider();
        public readonly EventProvider OnSaved = new EventProvider();

        public readonly EventProvider OnWatcherStarted = new EventProvider();
        public readonly EventProvider OnWatcherStopped = new EventProvider();

        public IReadOnlyDictionary<object, Tuple<IniConfigAttribute, object>> Registry => _registeredConfigs;
        public IConfigConverter Converter { get => _converter; set => _converter = value; }

        public ConfigNamingRule NamingRule { get => _rule; set => _rule = value; }

        public string Path { get => _path; set => _path = value; }

        public bool IsUsingWatcher => _watcher != null && _watcher.EnableRaisingEvents;

        public bool ShouldUseWatcher
        {
            get => _useWatcher;
            set
            {
                if (_useWatcher == value)
                    return;

                _useWatcher = value;

                if (_useWatcher)
                {
                    if (_watcher is null)
                    {
                        _watcher = new FileSystemWatcher(System.IO.Path.GetDirectoryName(_path), IniFilter);
                        _watcher.NotifyFilter = NotifyFilters.LastWrite;
                        _watcher.Changed += OnChange;
                        _watcher.EnableRaisingEvents = true;

                        OnWatcherStarted.Invoke(_watcher);
                    }
                }
                else
                {
                    if (_watcher != null)
                    {
                        _watcher.Changed -= OnChange;
                        _watcher.EnableRaisingEvents = false;
                        _watcher.NotifyFilter = default;
                        _watcher.Dispose();
                        _watcher = null;

                        OnWatcherStopped.Invoke(_watcher);
                    }
                }
            }
        }

        private void FinishReading()
        {
            OnReadFinished.Invoke();
        }

        public void Read()
        {
            // watcher prevention

            if (_saveHistory)
                return;

            try
            {
                Log.Debug($"Starting reading {Path}");

                OnReadStarted.Invoke();

                if (!File.Exists(Path))
                {
                    Save();
                    FinishReading();
                    return;
                }

                if (_reader != null)
                {
                    _reader.Dispose();
                    _reader = null;
                }

                var text = File.ReadAllText(Path);

                if (string.IsNullOrWhiteSpace(text))
                {
                    Save();
                    FinishReading();
                    return;
                }

                _reader = new IniConfigReader(File.ReadAllLines(Path));
                _registeredConfigs.ForEach(cfg => cfg.Value.Item1.ResetSet());

                while (_reader.TryMove())
                {
                    if (!_reader.IsLineValid)
                        continue;

                    if (!_registeredConfigs.TryGetFirst(cfg => cfg.Value != null && cfg.Value.Item1.Name == _reader.CurrentKey, out var config))
                    {
                        Log.Warn($"Failed to find a config with key {_reader.CurrentKey}!");
                        continue;
                    }

                    Type objType = null;

                    if (config.Key is PropertyInfo propInfo)
                        objType = propInfo.PropertyType;
                    else if (config.Key is FieldInfo fieldInfo)
                        objType = fieldInfo.FieldType;

                    Log.Debug($"Config object type: {objType.FullName}");

                    if (objType is null)
                    {
                        Log.Error($"Unknown config target! ({config.Key}) ({_reader.CurrentKey})");
                        continue;
                    }

                    if (_converter.TryConvert(_reader.CurrentValue, objType, out var configValue))
                    {
                        Log.Debug($"Converted value for key {_reader.CurrentKey}: {configValue?.ToString() ?? null}");

                        if (config.Key is PropertyInfo prop)
                        {
                            prop.SetValue(config.Value.Item2, configValue);
                            OnReadConfig.Invoke(_reader.CurrentKey, _reader.CurrentValue, configValue);
                            config.Value.Item1.Set();
                        }
                        else if (config.Key is FieldInfo field)
                        {
                            field.SetValue(config.Value.Item2, configValue);
                            OnReadConfig.Invoke(_reader.CurrentKey, _reader.CurrentValue, configValue);
                            config.Value.Item1.Set();
                        }
                        else
                        {
                            Log.Error($"Unknown config target! ({config.Key}) ({_reader.CurrentKey})");
                            continue;
                        }
                    }
                    else
                    {
                        Log.Error($"Failed to convert key {_reader.CurrentKey} (type: {objType.FullName})! Value: {_reader.CurrentValue}");
                        continue;
                    }
                }

                if (_registeredConfigs.Any(cfg => !cfg.Value.Item1._lastSet))
                {
                    Log.Warn($"Missing config keys detected! Regenerating file ..");

                    var directory = System.IO.Path.GetDirectoryName(Path);
                    var newPath = $"{directory}/{System.IO.Path.GetFileNameWithoutExtension(Path)}-old.ini";

                    File.Move(Path, newPath);

                    Save();
                }

                FinishReading();

                Log.Info($"Finished reading file {Path}!");
            }
            catch (Exception ex)
            {
                FinishReading();
                Log.Error(ex);
            }
        }

        public void Save()
        {
            _saveHistory = true;

            try
            {
                var writer = new IniConfigWriter(_converter);

                foreach (var key in _registeredConfigs)
                {
                    writer.WriteObject(
                        key.Value.Item1.Name,
                        key.Value.Item1.Description,

                        (key.Key is PropertyInfo property ? property.GetValue(key.Value.Item2) : (key.Key as FieldInfo).GetValue(key.Value.Item2)));
                }

                File.WriteAllText(Path, writer.ToString());
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }

            Timeout.Delay(500, () => _saveHistory = false);

            OnSaved.Invoke();
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

                    if (string.IsNullOrWhiteSpace(iniConfigAttribute.Name))
                    {
                        Log.Warn($"Config attribute property {property.DeclaringType.FullName}::{property.Name} is missing it's name!");

                        if (_rule is ConfigNamingRule.ThrowException)
                            throw new Exception($"Config attribute property {property.DeclaringType.FullName}::{property.Name} is missing it's name!");
                        else if (_rule is ConfigNamingRule.Skip)
                            continue;
                        else if (_rule is ConfigNamingRule.SetValue)
                            iniConfigAttribute.Name = property.Name;
                        else
                            iniConfigAttribute.Name = $"{property.DeclaringType.Name}.{property.Name}";
                    }

                    _registeredConfigs[property] = new Tuple<IniConfigAttribute, object>(iniConfigAttribute, typeInstance);
                    OnRegistered.Invoke(iniConfigAttribute, typeInstance);
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

                    if (string.IsNullOrWhiteSpace(iniConfigAttribute.Name))
                    {
                        Log.Warn($"Config attribute field {field.DeclaringType.FullName}::{field.Name} is missing it's name!");

                        if (_rule is ConfigNamingRule.ThrowException)
                            throw new Exception($"Config attribute field {field.DeclaringType.FullName}::{field.Name} is missing it's name!");
                        else if (_rule is ConfigNamingRule.Skip)
                            continue;
                        else if (_rule is ConfigNamingRule.SetValue)
                            iniConfigAttribute.Name = field.Name;
                        else
                            iniConfigAttribute.Name = $"{field.DeclaringType.Name}.{field.Name}";
                    }

                    _registeredConfigs[field] = new Tuple<IniConfigAttribute, object>(iniConfigAttribute, typeInstance);
                    OnRegistered.Invoke(iniConfigAttribute, typeInstance);
                }
            }
        }

        private void OnChange(object sender, FileSystemEventArgs ev)
        {
            if (ev.ChangeType != WatcherChangeTypes.Changed)
                return;

            if (ev.FullPath != Path)
                return;

            Timeout.Delay(300, () =>
            {
                Read();
            });
        }
    }
}