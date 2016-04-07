using System;
using System.Linq;
using System.Reflection;

namespace CSScriptNpp
{
    public static class ReflectionExtensions
    {
        public static T To<T>(this object obj)
        {
            return (T) obj;
        }

        public static object GetField(this object obj, string name, bool throwOnError = true)
        {
            //Note GetField(s) does not return base class fields like GetProperty does.
            //BindingFlags.FlattenHierarchy does not make any difference (Reflection bug).
            //Thus we have to process base classes explicitly.
            var type = obj.GetType();
            while (type != null)
            {
                var field = type.GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (field != null)
                    return field.GetValue(obj);
                type = type.BaseType;
            }

            if (throwOnError)
                throw new Exception("ReflectionExtensions: cannot find field " + name);

            return null;
        }

        public static void SetField(this object obj, string name, object value, bool throwOnError = true)
        {
            //Note GetField(s) does not return base class fields like GetProperty does.
            //BindingFlags.FlattenHierarchy does not make any difference (Reflection bug).
            //Thus we have to process base classes explicitly.
            var type = obj.GetType();
            while (type != null)
            {
                var field = type.GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (field != null)
                {
                    field.SetValue(obj, value);
                    return;
                }
                type = type.BaseType;
            }

            if (throwOnError)
                throw new Exception("ReflectionExtensions: cannot find field " + name);
        }

        public static object GetProp(this object obj, string name)
        {
            var property = obj.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (property == null)
                throw new Exception("ReflectionExtensions: cannot find property " + name);
            return property.GetValue(obj, null);
        }

        public static void SetProp(this object obj, string name, object value)
        {
            var property = obj.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (property == null)
                throw new Exception("ReflectionExtensions: cannot find property " + name);
            property.SetValue(obj, value, null);
        }

        public static object Call(this object obj, string name, params object[] args)
        {
            var method = obj.GetType().GetMethod(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod);
            if (method == null)
                throw new Exception("ReflectionExtensions: cannot find method " + name);
            return method.Invoke(obj, args);
        }

        public static void CallIfExists(this object obj, string name, params object[] args)
        {
            var method = obj.GetType().GetMethod(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod);
            if (method != null)
                method.Invoke(obj, args);
        }
    }
}