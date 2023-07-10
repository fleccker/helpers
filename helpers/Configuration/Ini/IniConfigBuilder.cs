using helpers.Configuration.Converters;
using helpers.Extensions;

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace helpers.Configuration.Ini
{
    [LogSource("Ini Builder")]
    public class IniConfigBuilder : DisposableBase
    {
        private IniConfigHandler _handler;
        private IConfigConverter _converter;
        private ConfigNamingRule _rule = ConfigNamingRule.SetValue;
        private string _path;
        private bool _useWatcher;
        private Dictionary<Type, object> _preRegistered = new Dictionary<Type, object>();

        public IniConfigBuilder WithGeneric<T>(T instance = default)
        {
            _preRegistered[typeof(T)] = instance;
            return this;
        }

        public IniConfigBuilder WithWatcher()
        {
            _useWatcher = true;
            return this;
        }

        public IniConfigBuilder WithNamingRule(ConfigNamingRule configNamingRule)
        {
            _rule = configNamingRule;
            return this;
        }

        public IniConfigBuilder WithAssembly(Assembly assembly)
            => WithTypes(assembly.GetTypes());

        public IniConfigBuilder WithAssembly()
            => WithAssembly(Assembly.GetCallingAssembly());

        public IniConfigBuilder WithTypes(params Type[] types)
        {
            types.ForEach(x => _preRegistered[x] = null);
            return this;
        }

        public IniConfigBuilder WithConverter<T>() where T : IConfigConverter, new()
        {
            _converter = new T();
            return this;
        }

        public IniConfigBuilder WithConverter(IConfigConverter converter)
        {
            _converter = converter;
            return this;
        }

        public IniConfigBuilder WithPath(string path)
        {
            _path = Path.GetFullPath(path);
            return this;
        }

        public IniConfigHandler Build()
        {
            CheckDisposed();

            _handler ??= new IniConfigHandler();
            _handler.Converter = _converter;
            _handler.Path = _path;
            _handler.ShouldUseWatcher = _useWatcher;
            _handler.NamingRule = _rule;

            foreach (var register in _preRegistered)
                _handler.Register(register.Key, register.Value);

            return _handler;
        }

        public IniConfigBuilder Build(ref IniConfigHandler handler)
        {
            handler = Build();
            return this;
        }

        public override void Dispose()
        {
            _preRegistered.Clear();
            _preRegistered = null;
            _path = null;
            _useWatcher = false;

            base.Dispose();
        }
    }
}