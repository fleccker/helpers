using helpers.Configuration.Converters;
using helpers.Configuration.Converters.Yaml;
using helpers.Events;
using helpers.Extensions;
using helpers.IO;

using System;
using System.Collections.Generic;
using System.Linq;

namespace helpers.Configuration.Register
{
    public class RegisterConfigHandler
    {
        private readonly HashSet<RegisterConfigBase> _registeredConfigs = new HashSet<RegisterConfigBase>();
        private readonly RegisterConfigWriter _writer;
        private readonly RegisterConfigReader _reader;
        private string _file;

        public IReadOnlyCollection<RegisterConfigBase> Configs => _registeredConfigs;
        public string File => _file;

        public readonly EventProvider OnPathChanged = new EventProvider();
        public readonly EventProvider OnBind = new EventProvider();
        public readonly EventProvider OnUnbind = new EventProvider();
        public readonly EventProvider OnSaved = new EventProvider();
        public readonly EventProvider OnLoaded = new EventProvider();

        public RegisterConfigHandler(string path, IConfigConverter converter = null)
        {
            ChangePath(path);

            if (converter is null) converter = new YamlConfigConverter();

            _writer = new RegisterConfigWriter(converter);
            _reader = new RegisterConfigReader(converter);
        }

        public bool TryBind<TValue>(string configName, TValue defaultValue, Action<TValue> setValue, Func<TValue> getValue, string description = null) => TryAdd(RegisterConfigUtils.Bind<TValue>(configName, defaultValue, setValue, getValue, description));

        public bool TryBindProperty(object target, string propertyName, string configName = null, object defaultValue = null, string description = null) => TryAdd(target.BindProperty(propertyName, configName, defaultValue, description));
        public bool TryBindProperty(Type target, string propertyName, string configName = null, object defaultValue = null, object targetHandle = null, string description = null) => TryAdd(target.BindProperty(propertyName, configName, targetHandle, defaultValue, description));
        public bool TryBindProperty(Type target, string propertyName, string configName, object defaultValue = null, string description = null) => TryAdd(target.BindProperty(propertyName, configName, defaultValue, description));

        public bool TryBindField(object target, string fieldName, string configName = null, object defaultValue = null, string description = null) => TryAdd(target.BindField(fieldName, configName, defaultValue, description));
        public bool TryBindField(Type target, string fieldName, string configName = null, object defaultValue = null, object targetHandle = null, string description = null) => TryAdd(target.BindField(fieldName, configName, targetHandle, defaultValue, description));
        public bool TryBindField(Type target, string fieldName, string configName = null, object defaultValue = null, string description = null) => TryAdd(target.BindField(fieldName, configName, defaultValue, description));
        
        public bool TryGetConfig(string configKey, out RegisterConfigBase configBase) => _registeredConfigs.TryGetFirst(x => x.Name == configKey, out configBase);
        public bool TryAdd(RegisterConfigBase configBase)
        {
            if (TryGetConfig(configBase.Name, out _))
            {
                Log.Warn($"Config Handler", $"Tried registering a duplicate key: {configBase.Name}");
                return false;
            }

            _registeredConfigs.Add(configBase);
            configBase.OnRegistered.Invoke();
            OnBind.Invoke(configBase);
            return true;
        }

        public bool TryRemove(string configKey)
        {
            if (!TryGetConfig(configKey, out var config))
            {
                Log.Warn($"Config Handler", $"Tried removing an unknown key: {configKey}");
                return false;
            }

            if (_registeredConfigs.Remove(config))
            {
                config.OnUnregistered.Invoke();
                OnUnbind.Invoke(config);
                return true;
            }
            else 
                return false;
        }

        public void ChangePath(string newFile)
        {
            ThrowIfPathInvalid(newFile);
            var oldPath = new string(_file.ToArray());
            _file = newFile;
            OnPathChanged.Invoke(newFile, oldPath);
        }
 
        public void Reload()
        {
            if (!System.IO.File.Exists(_file))
            {
                Save();
                OnLoaded.Invoke();
                return;
            }

            var lines = System.IO.File.ReadAllLines(_file);
            if (lines is null || !lines.Any())
            {
                Log.Warn("Config Handler", $"The targeted file ({_file}) is empty.");
                return;
            }

            _reader.ReadConfig(lines);
            _reader.ReadAll(_registeredConfigs);

            OnLoaded.Invoke();
        }

        public void Save()
        {
            _writer.WriteAll(_registeredConfigs);
            System.IO.File.WriteAllText(_file, _writer.ToString());
            OnSaved.Invoke();
        }

        private void ThrowIfPathInvalid(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException($"The file path cannot be null or white spaced!");
            if (!FileManager.IsValidPath(path)) throw new ArgumentException($"The file path is invalid!");
        }
    }
}