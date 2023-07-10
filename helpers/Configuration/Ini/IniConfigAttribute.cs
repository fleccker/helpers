using helpers.Extensions;

using System;

namespace helpers.Configuration.Ini
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class IniConfigAttribute : Attribute
    {
        internal bool _lastSet;

        private string[] _description;

        public string Name { get; set; }

        public string Description
        {
            get
            {
                return (_description is null || !_description.Any()) ? "No description." : string.Join(Environment.NewLine, _description);
            }
            set
            {
                _description = value.SplitLines();
            }
        }

        public IniConfigAttribute() { }

        public void ResetSet() 
            => _lastSet = false;

        public void Set() 
            => _lastSet = true;
    }
}