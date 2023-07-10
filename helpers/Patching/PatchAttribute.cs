using System;

namespace helpers.Patching
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class PatchAttribute : Attribute
    {
        internal PatchTarget _target;
        internal PatchType _type = PatchType.Prefix;
        internal PatchMethodType _methodType = PatchMethodType.Method;
        internal string _name = "Default Name";
        internal int _priority = 0;
        internal PatchInfo _info;

        public PatchAttribute(Type declaringType, string methodName)
        {
            _target = new PatchTarget(declaringType, methodName);
        }

        public PatchAttribute(Type declaringType, string methodName, params Type[] parameters)
        {
            _target = new PatchTarget(declaringType, methodName, parameters);
        }

        public PatchAttribute(Type declaringType, string methodName, PatchType type, params Type[] parameters)
        {
            _target = new PatchTarget(declaringType, methodName, parameters);
            _type = type;
        }

        public PatchAttribute(Type declaringType, string methodName, PatchType type, PatchMethodType patchMethodType, params Type[] parameters)
        {
            _target = new PatchTarget(declaringType, methodName, parameters);
            _type = type;
            _methodType = patchMethodType;
        }

        public PatchAttribute(Type declaringType, string methodName, PatchType type, string name, params Type[] parameters)
        {
            _target = new PatchTarget(declaringType, methodName, parameters);
            _type = type;
            _name = name;
        }

        public PatchAttribute(Type declaringType, string methodName, PatchType type, int priority, params Type[] parameters)
        {
            _target = new PatchTarget(declaringType, methodName, parameters);
            _type = type;
            _priority = priority;
        }

        public PatchAttribute(Type declaringType, string methodName, PatchType type, string name, int priority, params Type[] parameters)
        {
            _target = new PatchTarget(declaringType, methodName, parameters);
            _type = type;
            _name = name;
            _priority = priority;
        }

        public PatchAttribute(Type declaringType, string methodName, PatchType type, PatchMethodType patchMethodType, int priority, params Type[] parameters)
        {
            _target = new PatchTarget(declaringType, methodName, parameters);
            _type = type;
            _methodType = patchMethodType;
            _priority = priority;
        }

        public PatchAttribute(Type declaringType, string methodName, PatchType type, PatchMethodType patchMethodType, string name, int priority, params Type[] parameters)
        {
            _target = new PatchTarget(declaringType, methodName, parameters);
            _type = type;
            _methodType = patchMethodType;
            _priority = priority;
            _name = name;
        }

        public PatchAttribute(Type declaringType, string methodName, PatchType type, PatchMethodType patchMethodType, string name, params Type[] parameters)
        {
            _target = new PatchTarget(declaringType, methodName, parameters);
            _type = type;
            _methodType = patchMethodType;
            _name = name;
        }
    }
}