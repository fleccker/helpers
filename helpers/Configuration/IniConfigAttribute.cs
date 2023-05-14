using System;
using System.Linq;

namespace helpers.Configuration
{
    [AttributeUsage(
        AttributeTargets.Property | AttributeTargets.Field,

        AllowMultiple = false,
        Inherited = false)]
    public class IniConfigAttribute : Attribute
    {
        internal string _name;
        private string _pairName;
        private string[] _description;

        public IniConfigAttribute(string name = null, params string[] description)
        {
            _name = name;
            _description = description;
        }

        public IniConfigAttribute(string name = null, string pairName = null)
        {
            _name = name;
            _pairName = pairName;
            _description = null;
        }

        public IniConfigAttribute(string name = null, string pairedName = null, params string[] description)
        {
            _name = name;
            _pairName = pairedName;
            _description = description.Select(x => x?.ToString() ?? "null string").ToArray();
        }

        public string GetName() => _name;
        public string GetPairName() => string.IsNullOrWhiteSpace(_pairName) ? "global" : _pairName;

        public string[] GetDescription() => _description ?? new string[] { };
    }
}