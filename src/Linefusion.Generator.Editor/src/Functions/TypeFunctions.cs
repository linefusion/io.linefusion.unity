using System;
using System.Linq;
using System.Reflection;
using Linefusion.Generators.Editor;
using Linefusion.Generators.Editor.CodeModel;

using Scriban;

namespace Linefusion.Generators.Functions
{
    public class TypeFunctions
    {
        public static Type GetType(TemplateContext context, object obj)
        {
            return obj.GetType();
        }

        public static string GetTypeName(TemplateContext context, object obj)
        {
            return GetType(context, obj).FullName;
        }

        public static Type Find(TemplateContext context, string name)
        {
            var unity = new UnityFunctions();

            return AppDomain
                .CurrentDomain.GetAssemblies()
                .Reverse()
                .Concat(unity.Assemblies.Select(assembly => assembly.Assembly))
                .Select(assembly => assembly.GetType(name, false, false))
                .FirstOrDefault(type =>
                {
                    return type != null;
                });
        }

        public static bool Instanceof(TemplateContext context, object obj, Type type)
        {
            return type.IsAssignableFrom(GetType(context, obj));
        }

        public static bool IsAssignableTo(TemplateContext context, object type, object other)
        {
            var t1 = type switch
            {
                string typeName => Find(context, typeName),
                TypeInfo typeInfo => typeInfo.AsType(),
                Type typeType => typeType,
                _ => type.GetType()
            };

            var t2 = other switch
            {
                string typeName => Find(context, typeName),
                TypeInfo typeInfo => typeInfo.AsType(),
                Type typeType => typeType,
                _ => other.GetType()
            };

            return t1.IsAssignableTo(t2!);
        }

        public static bool IsAssignableFrom(TemplateContext context, object type, object other)
        {
            var t1 = type switch
            {
                string typeName => Find(context, typeName),
                TypeInfo typeInfo => typeInfo.AsType(),
                Type typeType => typeType,
                _ => type.GetType()
            };

            var t2 = other switch
            {
                string typeName => Find(context, typeName),
                TypeInfo typeInfo => typeInfo.AsType(),
                Type typeType => typeType,
                _ => other.GetType()
            };

            return t1.IsAssignableFrom(t2);
        }

    }
}
