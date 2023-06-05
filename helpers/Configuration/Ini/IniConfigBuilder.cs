using helpers.Configuration.Converters;
using helpers.Extensions;

using System;
using System.Collections.Generic;

namespace helpers.Configuration.Ini
{
    [LogSource("Ini Builder")]
    public class IniConfigBuilder
    {
        private IniConfigHandler _handler;
        private Dictionary<Type, object> _preRegistered = new Dictionary<Type, object>();
        private IConfigConverter _converter;
        private string _globalPath;
        private Dictionary<string, string> _customPaths = new Dictionary<string, string>();

        public IniConfigBuilder WithType<T>(T instance = default)
        {
            _preRegistered[typeof(T)] = instance;
            return this;
        }

        public IniConfigBuilder WithType(Type type, object instance = null)
        {
            _preRegistered[type] = instance;
            return this;
        }

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

        public IniConfigBuilder WithGlobalPath(string globalPath)
        {
            _globalPath = globalPath;
            return this;
        }

        public IniConfigBuilder WithAlias(string aliasName, string aliasPath)
        {
            _customPaths[aliasName] = aliasPath;
            return this;
        }

        public IniConfigHandler Build()
        {
            _handler = new IniConfigHandler();
            _handler._converter = _converter;

            if (!string.IsNullOrWhiteSpace(_globalPath))
                _handler._paths["global"] = _globalPath;

            return _handler;
        }

        public IniConfigBuilder Register(ref IniConfigHandler handler)
        {
            if (_handler is null)
            {
                _handler = Build();

                handler = _handler;
            }

            foreach (var pair in _preRegistered)
            {
                _handler.Register(pair.Key, pair.Value);
            }
            foreach (var path in _customPaths)
            {
                _handler._paths[path.Key] = path.Value;
            }

            _preRegistered.Clear();
            _preRegistered = null;
            _customPaths.Clear();
            _customPaths = null;
            _converter = null;
            _globalPath = null;

            return this;
        }
    }
}