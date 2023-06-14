using helpers.Extensions;

using System;
using System.Collections.Generic;

namespace helpers.Analyzors
{
    public static class TypeAnalyzor
    {
        private static readonly HashSet<TypeAnalyzorResult> m_Results = new HashSet<TypeAnalyzorResult>();

        public static TypeAnalyzorResult Analyze(Type type)
        {
            if (m_Results.TryGetFirst(res => res.TypeQualifiedName == type.AssemblyQualifiedName, out var result))
                return result;

            result = new TypeAnalyzorResult(type);

            m_Results.Add(result);

            return result;
        }
    }
}