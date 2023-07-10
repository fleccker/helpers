using HarmonyLib;

using System;
using System.Reflection;

namespace helpers.Patching
{
    public class PatchInfo
    {
        private string _harmonyId;

        private HarmonyMethod m_Replacement;

        private MethodInfo m_Target;
        private MethodInfo m_Patch;

        private PatchType m_Type = PatchType.Unknown;
        private PatchMethodType m_MethodType = PatchMethodType.Enumerator;

        private string m_Name = "Unknown";

        private int m_Priority = 0;

        public string Name { get => m_Name; internal set => m_Name = value; }

        public int Priority { get => m_Priority; internal set => m_Priority = value; }

        public PatchType Type { get => m_Type; internal set => m_Type = value; }
        public PatchMethodType MethodType { get => m_MethodType; internal set => m_MethodType = value; }

        public MethodInfo Target => m_Target;
        public MethodInfo Replacement => m_Replacement.method;

        public MethodInfo Patch => m_Patch;

        public PatchInfo(PatchTarget target, PatchTarget replacement)
        {
            if (!target.TryFind(out var targetMethod))
                throw new ArgumentException($"Failed to find target method!");

            if (!replacement.TryFind(out var replacementMethod))
                throw new ArgumentException($"Failed to find replacement method!");

            m_Target = targetMethod;
            m_Replacement = new HarmonyMethod(replacementMethod);

            ValidateNameAndType();
        }

        public PatchInfo(PatchTarget target, PatchTarget replacement, Action<PatchInfo> setup)
        {
            if (!target.TryFind(out var targetMethod))
                throw new ArgumentException($"Failed to find target method!");

            if (!replacement.TryFind(out var replacementMethod))
                throw new ArgumentException($"Failed to find replacement method!");

            m_Target = targetMethod;
            m_Replacement = new HarmonyMethod(replacementMethod);

            setup.Invoke(this);

            ValidateNameAndType();
        }

        public PatchInfo(PatchTarget target, PatchTarget replacement, PatchType type)
        {
            if (!target.TryFind(out var targetMethod))
                throw new ArgumentException($"Failed to find target method!");

            if (!replacement.TryFind(out var replacementMethod))
                throw new ArgumentException($"Failed to find replacement method!");

            m_Target = targetMethod;
            m_Replacement = new HarmonyMethod(replacementMethod);
            m_Type = type;

            ValidateNameAndType();
        }

        public PatchInfo(PatchTarget target, PatchTarget replacement, PatchMethodType methodType)
        {
            if (!target.TryFind(out var targetMethod))
                throw new ArgumentException($"Failed to find target method!");

            if (!replacement.TryFind(out var replacementMethod))
                throw new ArgumentException($"Failed to find replacement method!");

            m_Target = targetMethod;
            m_Replacement = new HarmonyMethod(replacementMethod);
            m_MethodType = methodType;

            ValidateNameAndType();
        }

        public PatchInfo(PatchTarget target, PatchTarget replacement, PatchType type, PatchMethodType methodType)
        {
            if (!target.TryFind(out var targetMethod))
                throw new ArgumentException($"Failed to find target method!");

            if (!replacement.TryFind(out var replacementMethod))
                throw new ArgumentException($"Failed to find replacement method!");

            m_Target = targetMethod;
            m_Replacement = new HarmonyMethod(replacementMethod);
            m_Type = type;
            m_MethodType = methodType;

            ValidateNameAndType();
        }

        public PatchInfo(PatchTarget target, PatchTarget replacement, PatchType type, string name)
        {
            if (!target.TryFind(out var targetMethod))
                throw new ArgumentException($"Failed to find target method!");

            if (!replacement.TryFind(out var replacementMethod))
                throw new ArgumentException($"Failed to find replacement method!");

            m_Target = targetMethod;
            m_Replacement = new HarmonyMethod(replacementMethod);
            m_Type = type;
            m_Name = name;

            ValidateNameAndType();
        }

        public PatchInfo(PatchTarget target, PatchTarget replacement, PatchType type, string name, PatchMethodType methodType)
        {
            if (!target.TryFind(out var targetMethod))
                throw new ArgumentException($"Failed to find target method!");

            if (!replacement.TryFind(out var replacementMethod))
                throw new ArgumentException($"Failed to find replacement method!");

            m_Target = targetMethod;
            m_Replacement = new HarmonyMethod(replacementMethod);
            m_Type = type;
            m_Name = name;
            m_MethodType = methodType;

            ValidateNameAndType();
        }

        public PatchInfo(Delegate target, Delegate replacement)
        {
            m_Target = target.GetMethodInfo();
            m_Replacement = new HarmonyMethod(replacement.GetMethodInfo());

            ValidateNameAndType();
        }

        public PatchInfo(Delegate target, Delegate replacement, PatchType type)
        {
            m_Target = target.GetMethodInfo();
            m_Replacement = new HarmonyMethod(replacement.GetMethodInfo());
            m_Type = type;

            ValidateNameAndType();
        }

        public PatchInfo(Delegate target, Delegate replacement, PatchMethodType methodType)
        {
            m_Target = target.GetMethodInfo();
            m_Replacement = new HarmonyMethod(replacement.GetMethodInfo());
            m_MethodType = methodType;

            ValidateNameAndType();
        }

        public PatchInfo(Delegate target, Delegate replacement, PatchType type, PatchMethodType methodType)
        {
            m_Target = target.GetMethodInfo();
            m_Replacement = new HarmonyMethod(replacement.GetMethodInfo());
            m_Type = type;
            m_MethodType = methodType;

            ValidateNameAndType();
        }

        public PatchInfo(Delegate target, Delegate replacement, PatchType type, string name)
        {
            m_Target = target.GetMethodInfo();
            m_Replacement = new HarmonyMethod(replacement.GetMethodInfo());
            m_Type = type;
            m_Name = name;

            ValidateNameAndType();
        }

        public PatchInfo(Delegate target, Delegate replacement, PatchMethodType methodType, string name)
        {
            m_Target = target.GetMethodInfo();
            m_Replacement = new HarmonyMethod(replacement.GetMethodInfo());
            m_MethodType = methodType;
            m_Name = name;

            ValidateNameAndType();
        }

        public PatchInfo(Delegate target, Delegate replacement, PatchType type, PatchMethodType methodType, string name)
        {
            m_Target = target.GetMethodInfo();
            m_Replacement = new HarmonyMethod(replacement.GetMethodInfo());
            m_Type = type;
            m_Name = name;
            m_MethodType = methodType;

            ValidateNameAndType();
        }

        public PatchInfo(Type targetType, string targetName, Delegate replacement, params Type[] targetParameters)
        {
            var targetInfo = new PatchTarget(targetType, targetName, targetParameters);
            if (!targetInfo.TryFind(out var targetMethod))
                throw new ArgumentException($"Failed to find target method!");

            m_Target = targetMethod;
            m_Replacement = new HarmonyMethod(replacement.GetMethodInfo());

            ValidateNameAndType();
        }

        public PatchInfo(Type targetType, string targetName, Delegate replacement, PatchMethodType methodType, params Type[] targetParameters)
        {
            var targetInfo = new PatchTarget(targetType, targetName, targetParameters);
            if (!targetInfo.TryFind(out var targetMethod))
                throw new ArgumentException($"Failed to find target method!");

            m_Target = targetMethod;
            m_Replacement = new HarmonyMethod(replacement.GetMethodInfo());
            m_MethodType = methodType;

            ValidateNameAndType();
        }

        public PatchInfo(Type targetType, string targetName, Delegate replacement, PatchType type, params Type[] targetParameters)
        {
            var targetInfo = new PatchTarget(targetType, targetName, targetParameters);
            if (!targetInfo.TryFind(out var targetMethod))
                throw new ArgumentException($"Failed to find target method!");

            m_Target = targetMethod;
            m_Replacement = new HarmonyMethod(replacement.GetMethodInfo());
            m_Type = type;

            ValidateNameAndType();
        }

        public PatchInfo(Type targetType, string targetName, Delegate replacement, PatchType type, string name, params Type[] targetParameters)
        {
            var targetInfo = new PatchTarget(targetType, targetName, targetParameters);
            if (!targetInfo.TryFind(out var targetMethod))
                throw new ArgumentException($"Failed to find target method!");

            m_Target = targetMethod;
            m_Replacement = new HarmonyMethod(replacement.GetMethodInfo());
            m_Type = type;
            m_Name = name;

            ValidateNameAndType();
        }

        public PatchInfo(Type targetType, string targetName, Delegate replacement, PatchType type, PatchMethodType methodType, string name, params Type[] targetParameters)
        {
            var targetInfo = new PatchTarget(targetType, targetName, targetParameters);
            if (!targetInfo.TryFind(out var targetMethod))
                throw new ArgumentException($"Failed to find target method!");

            m_Target = targetMethod;
            m_Replacement = new HarmonyMethod(replacement.GetMethodInfo());
            m_Type = type;
            m_Name = name;
            m_MethodType = methodType;

            ValidateNameAndType();
        }

        public PatchInfo(Delegate target, Type replacementType, string replacementName, params Type[] replacementParameters)
        {
            var replacementInfo = new PatchTarget(replacementType, replacementName, replacementParameters);
            if (!replacementInfo.TryFind(out var replacementMethod))
                throw new ArgumentException($"Failed to find replacement method!");

            m_Target = target.GetMethodInfo();
            m_Replacement = new HarmonyMethod(replacementMethod);

            ValidateNameAndType();
        }

        public PatchInfo(Delegate target, Type replacementType, string replacementName, PatchMethodType methodType, params Type[] replacementParameters)
        {
            var replacementInfo = new PatchTarget(replacementType, replacementName, replacementParameters);
            if (!replacementInfo.TryFind(out var replacementMethod))
                throw new ArgumentException($"Failed to find replacement method!");

            m_Target = target.GetMethodInfo();
            m_Replacement = new HarmonyMethod(replacementMethod);
            m_MethodType = methodType;

            ValidateNameAndType();
        }

        public PatchInfo(Delegate target, Type replacementType, string replacementName, PatchType type, params Type[] replacementParameters)
        {
            var replacementInfo = new PatchTarget(replacementType, replacementName, replacementParameters);
            if (!replacementInfo.TryFind(out var replacementMethod))
                throw new ArgumentException($"Failed to find replacement method!");

            m_Target = target.GetMethodInfo();
            m_Replacement = new HarmonyMethod(replacementMethod);
            m_Type = type;

            ValidateNameAndType();
        }

        public PatchInfo(Delegate target, Type replacementType, string replacementName, PatchType type, string name, params Type[] replacementParameters)
        {
            var replacementInfo = new PatchTarget(replacementType, replacementName, replacementParameters);
            if (!replacementInfo.TryFind(out var replacementMethod))
                throw new ArgumentException($"Failed to find replacement method!");

            m_Target = target.GetMethodInfo();
            m_Replacement = new HarmonyMethod(replacementMethod);
            m_Type = type;
            m_Name = name;

            ValidateNameAndType();
        }

        public PatchInfo(Delegate target, Type replacementType, string replacementName, PatchMethodType methodType, string name, params Type[] replacementParameters)
        {
            var replacementInfo = new PatchTarget(replacementType, replacementName, replacementParameters);
            if (!replacementInfo.TryFind(out var replacementMethod))
                throw new ArgumentException($"Failed to find replacement method!");

            m_Target = target.GetMethodInfo();
            m_Replacement = new HarmonyMethod(replacementMethod);
            m_MethodType = methodType;
            m_Name = name;

            ValidateNameAndType();
        }

        public PatchInfo(Delegate target, Type replacementType, string replacementName, PatchType type, PatchMethodType methodType, string name, params Type[] replacementParameters)
        {
            var replacementInfo = new PatchTarget(replacementType, replacementName, replacementParameters);
            if (!replacementInfo.TryFind(out var replacementMethod))
                throw new ArgumentException($"Failed to find replacement method!");

            m_Target = target.GetMethodInfo();
            m_Replacement = new HarmonyMethod(replacementMethod);
            m_Type = type;
            m_Name = name;
            m_MethodType = methodType;

            ValidateNameAndType();
        }

        public bool IsApplied(Harmony harmony)
        {
            Log.Debug($"IsApplied({harmony.Id}) :: curId: {_harmonyId ?? "null"}");

            if (string.IsNullOrWhiteSpace(_harmonyId))
                return false;

            if (_harmonyId != harmony.Id)
                return false;

            foreach (var patch in harmony.GetPatchedMethods())
            {
                Log.Debug($"IsApplied :: Patched method: {patch.DeclaringType.FullName}::{patch.Name}");
                Log.Debug($"IsApplied :: Patch target: {m_Target.DeclaringType.FullName}::{m_Target.Name}");

                if (patch == m_Target)
                {
                    return true;
                }
            }

            return false;
        }

        public void Apply(Harmony harmony)
        {
            if (IsApplied(harmony))
                return;

            Log.Debug($"Applying with harmony {harmony.Id}");

            switch (m_Type)
            {
                case PatchType.Unknown:
                    throw new InvalidOperationException($"Cannot make a patch of an unknown type!");

                case PatchType.Finalizer:
                    m_Patch = harmony.Patch(Target, null, null, null, m_Replacement);
                    return;

                case PatchType.Transpiler:
                    m_Patch = harmony.Patch(Target, null, null, m_Replacement);
                    return;

                case PatchType.Prefix:
                    m_Patch = harmony.Patch(Target, m_Replacement);
                    return;

                case PatchType.Postfix:
                    m_Patch = harmony.Patch(Target, null, m_Replacement);
                    return;
            }

            if (m_Patch != null)
                _harmonyId = harmony.Id;
            else
                throw new InvalidOperationException($"Cannot make a patch of type {m_Type}!");

            Log.Debug($"Applied patch {Name}");
        }

        public void Remove(Harmony harmony)
        {
            if (!IsApplied(harmony))
                return;

            Log.Debug($"Removing patch {Name}");

            harmony.Unpatch(Target, m_Patch);

            m_Patch = null;
            _harmonyId = null;
        }

        private void ValidateNameAndType()
        {
            Log.Debug($"Validating patch info {m_Type} / {m_Name} / {m_MethodType} / {m_Priority}");

            if (m_Type is PatchType.Unknown)
            {
                if (string.IsNullOrWhiteSpace(m_Name))
                    m_Name = m_Replacement.method.Name;

                if (m_Name == "Prefix")
                    m_Type = PatchType.Prefix;
                else if (m_Name == "Postfix")
                    m_Type = PatchType.Postfix;
                else if (m_Name == "Transpiler")
                    m_Type = PatchType.Transpiler;
                else if (m_Name == "Finalizer")
                    m_Type = PatchType.Finalizer;
                else
                    throw new ArgumentException($"Failed to automatically determine patch type from name: {m_Name}");
            }

            m_Replacement.debug = true;
            m_Replacement.methodName = m_Name;
            m_Replacement.methodType = ConvertType();
            m_Replacement.priority = m_Priority;
        }

        private MethodType ConvertType()
        {
            switch (m_MethodType)
            {
                case PatchMethodType.PropertyGetter:
                    return HarmonyLib.MethodType.Getter;

                case PatchMethodType.PropertySetter:
                    return HarmonyLib.MethodType.Setter;

                case PatchMethodType.Constructor:
                    return HarmonyLib.MethodType.Constructor;

                case PatchMethodType.StaticConstructor:
                    return HarmonyLib.MethodType.StaticConstructor;

                case PatchMethodType.Enumerator:
                    return HarmonyLib.MethodType.Enumerator;

                case PatchMethodType.Method:
                    return HarmonyLib.MethodType.Normal;

                default:
                    throw new ArgumentException($"Failed to convert patch method type {m_MethodType} to a method type!");
            }
        }

        public override string ToString()
            => m_Name;
    }
}