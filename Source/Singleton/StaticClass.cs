using System;
using System.Collections.Generic;
using System.Linq;

namespace CPPUtility
{
    public class StaticClass
    {
        protected static readonly Dictionary<Type, object> Instances = new Dictionary<Type, object>();


        public static T Get<T>() where T : class, new()
        {
            var type=typeof(T);
            if (Instances.TryGetValue(type, out var obj))
            {
                return obj as T;
            }

            var newInstance = new T();
            Instances.Add(type, newInstance);

            return newInstance;
        }

        public static object Get(Type type)
        {
            if (Instances.TryGetValue(type, out var obj))
            {
                return obj;
            }

            var newInstance = Activator.CreateInstance(type);
            Instances.Add(type, newInstance);

            return newInstance;
        }

        public static bool IsConstructed<T>() => IsConstructed(typeof(T));
        public static bool IsConstructed(Type type) => Instances.ContainsKey(type);


        public static IEnumerable<T> GetSubclasses<T>() where T : class
        {
            return GetSubclasses(typeof(T)).Cast<T>();
        }
        public static IEnumerable<object> GetSubclasses(Type baseType)
        {
            var assembly = baseType.Assembly;

            var types = assembly.GetExportedTypes()
                .Where(t => t.IsSubclassOf(baseType))
                .Where(t => !t.IsAbstract && t.GetConstructor(Type.EmptyTypes) != null);

            return types.Select(t => Get(t));
        }
    }

    public class StaticClass<T> : StaticClass where T : class, new()
    {
        private static readonly Lazy<T> LazyValue = new Lazy<T>(() =>
        {
            return StaticClass.Get<T>();
        });

        public static T Value => LazyValue.Value;
    }
}
