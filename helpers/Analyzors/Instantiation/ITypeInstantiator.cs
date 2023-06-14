using System;

namespace helpers.Analyzors.Instantiation
{
    public interface ITypeInstantiator
    {
        byte Speed { get; }

        bool IsAvailable(Type type);

        object Instantiate(TypeAnalyzorResult type, object[] args);
    }
}