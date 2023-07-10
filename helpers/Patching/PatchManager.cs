using Fasterflect;

using HarmonyLib;

using helpers.Extensions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace helpers.Patching
{
    public static class PatchManager
    {
        private static readonly Harmony m_HarmonyInstance = new Harmony($"com.patching.helpers.{DateTime.Now.Ticks}");
        private static readonly List<PatchInfo> m_Patches = new List<PatchInfo>();

        public static Harmony HarmonyPatcher => m_HarmonyInstance;

        public static IReadOnlyList<PatchInfo> AllPatches => m_Patches;
        public static IReadOnlyList<PatchInfo> ActivePatches => m_Patches.Where(patch => patch.IsApplied(m_HarmonyInstance)).ToList();
        public static IReadOnlyList<PatchInfo> InactivePatches => m_Patches.Where(patch => !patch.IsApplied(m_HarmonyInstance)).ToList();

        public static void PatchAssembly()
            => PatchAssemblies(Assembly.GetCallingAssembly());

        public static void PatchAssemblies(params Assembly[] assemblies)
            => assemblies.ForEach(assembly => PatchTypes(assembly.GetTypes()));

        public static void PatchTypes(params Type[] types)
            => types.ForEach(type =>
            {
                PatchFields(type.GetFields(Reflection.AllFlags));
                PatchProperties(type.GetProperties(Reflection.AllFlags));
                PatchMethods(type.GetMethods(Reflection.AllFlags));
            });

        public static void PatchFields(params FieldInfo[] fields)
            => fields.ForEach(field =>
            {
                if (!field.IsStatic)
                    return;

                if (!field.IsReadable())
                    return;

                if (field.FieldType != typeof(PatchInfo))
                    return;

                var patchInfoObj = field.GetValue(null);

                if (patchInfoObj is null || patchInfoObj is not PatchInfo patchInfo)
                    return;

                Patch(patchInfo);
            });

        public static void PatchProperties(params PropertyInfo[] properties)
            => properties.ForEach(property =>
            {
                if (!property.IsStatic())
                    return;

                if (!property.IsReadable())
                    return;

                if (property.PropertyType != typeof(PatchInfo))
                    return;

                var patchInfoObj = property.GetValue(null);

                if (patchInfoObj is null || patchInfoObj is not PatchInfo patchInfo)
                    return;

                Patch(patchInfo);
            });

        public static void PatchMethods(params MethodBase[] methods)
            => methods.ForEach(method =>
            {
                if (!method.IsStatic)
                    return;

                if (!method.TryGetAttribute<PatchAttribute>(out var patchAttribute) || patchAttribute is null)
                    return;

                var mParams = method.GetParameters()?.Select(p => p.ParameterType)?.ToArray();
                var hasParams = mParams != null && mParams.Any();

                if (!hasParams)
                    patchAttribute._info ??= new PatchInfo(patchAttribute._target, new PatchTarget(method.DeclaringType, method.Name), info =>
                    {
                        info.MethodType = patchAttribute._methodType;
                        info.Type = patchAttribute._type;
                        info.Priority = patchAttribute._priority;
                        info.Name = patchAttribute._name;
                    });
                else
                    patchAttribute._info ??= new PatchInfo(patchAttribute._target, new PatchTarget(method.DeclaringType, method.Name, mParams), info =>
                    {
                        info.MethodType = patchAttribute._methodType;
                        info.Type = patchAttribute._type;
                        info.Priority = patchAttribute._priority;
                        info.Name = patchAttribute._name;
                    });

                Patch(patchAttribute._info);
            });

        public static void Patch(params PatchInfo[] patches)
        {
            patches.ForEach(patch =>
            {
                if (!m_Patches.Contains(patch))
                {
                    m_Patches.Add(patch);
                }

                patch.Apply(m_HarmonyInstance);
            });
        }

        public static void UnpatchAssembly()
            => UnpatchAssemblies(Assembly.GetCallingAssembly());

        public static void UnpatchAssemblies(params Assembly[] assemblies)
            => assemblies.ForEach(assembly => UnpatchTypes(assembly.GetTypes()));

        public static void UnpatchTypes(params Type[] types)
            => types.ForEach(type =>
            {
                UnpatchFields(type.GetFields(Reflection.AllFlags));
                UnpatchProperties(type.GetProperties(Reflection.AllFlags));
                UnpatchMethods(type.GetMethods(Reflection.AllFlags));
            });

        public static void UnpatchFields(params FieldInfo[] fields)
            => fields.ForEach(field =>
            {
                if (!field.IsStatic)
                    return;

                if (!field.IsReadable())
                    return;

                if (field.FieldType != typeof(PatchInfo))
                    return;

                var patchInfoObj = field.GetValue(null);

                if (patchInfoObj is null || patchInfoObj is not PatchInfo patchInfo)
                    return;

                Unpatch(patchInfo);
            });

        public static void UnpatchProperties(params PropertyInfo[] properties)
            => properties.ForEach(property =>
            {
                if (!property.IsStatic())
                    return;

                if (!property.IsReadable())
                    return;

                if (property.PropertyType != typeof(PatchInfo))
                    return;

                var patchInfoObj = property.GetValue(null);

                if (patchInfoObj is null || patchInfoObj is not PatchInfo patchInfo)
                    return;

                Unpatch(patchInfo);
            });

        public static void UnpatchMethods(params MethodBase[] methods)
            => methods.ForEach(method =>
            {
                if (!method.IsStatic)
                    return;

                if (!method.TryGetAttribute<PatchAttribute>(out var patchAttribute) || patchAttribute is null)
                    return;

                if (patchAttribute._info is null)
                    return;

                Unpatch(patchAttribute._info);
            });

        public static void Unpatch(params PatchInfo[] patches)
        {
            patches.ForEach(patch =>
            {
                if (!m_Patches.Contains(patch))
                {
                    m_Patches.Add(patch);
                }

                patch.Remove(m_HarmonyInstance);
            });
        }
    }
}