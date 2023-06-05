using System;

namespace helpers.Pooling.Exceptions
{
    public class PoolEmptyException : Exception
    {
        public PoolEmptyException(Type type) : base($"The pool of type {type.FullName} is empty! You can disable exceptions by setting the behaviour to NewOnEmpty or DefaultOnEmpty.") { }
    }
}