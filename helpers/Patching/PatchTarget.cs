using helpers.Extensions;

using System;
using System.Linq;
using System.Reflection;

namespace helpers.Patching
{
    public struct PatchTarget
    {
        private string m_TargetName;

        private Type m_TargetType;
        private Type[] m_Params;

        public PatchTarget(Type targetType, string targetName, params Type[] parameters)
        {
            m_TargetName = targetName;
            m_TargetType = targetType;
            m_Params = parameters ?? Array.Empty<Type>();
        }

        public bool TryFind(out MethodInfo target)
        {
            if (m_TargetType is null)
            {
                target = null;
                return false;
            }

            if (string.IsNullOrWhiteSpace(m_TargetName))
            {
                target = null;
                return false;
            }

            foreach (var method in m_TargetType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
            {
                if (method.Name == m_TargetName)
                {
                    if (m_Params.Any())
                    {
                        var mParams = method.GetParameters();
                        if (mParams is null || !mParams.Any())
                            continue;

                        var mParamTypes = mParams.Select(param => param.ParameterType);
                        if (!mParamTypes.Match(m_Params))
                            continue;
                    }

                    target = method;
                    return true;
                }
            }

            target = null;
            return false;
        }
    }
}