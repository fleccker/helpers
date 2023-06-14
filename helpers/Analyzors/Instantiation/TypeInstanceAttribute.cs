using System;

namespace helpers.Analyzors.Instantiation
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class TypeInstanceAttribute : Attribute
    {
    }
}