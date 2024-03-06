using Scriban.Helpers;
using Scriban.Runtime;
using System.Reflection;
using System;
using System.Linq;
using UnityEngine;

namespace Linefusion.Generators
{
    public static class TemplateObject2
    {
        public static ScriptObject Wrap(Type type)
        {
            var obj = new Scriban.Runtime.ScriptObject();

            //obj.Import(typeof(T));

            foreach (var methods in type.GetMethods(BindingFlags.Public | BindingFlags.Static).Where(m => !m.IsSpecialName).GroupBy(method => method.Name))
            {
                obj.Import("static_" + StandardMemberRenamer.Rename(methods.Key), (object[] parameters) =>
                {
                    return type.InvokeMember(
                        methods.Key,
                        BindingFlags.Public |
                        BindingFlags.Static |
                        BindingFlags.InvokeMethod |
                        BindingFlags.NonPublic |
                        BindingFlags.OptionalParamBinding,
                        Type.DefaultBinder,
                        null,
                        parameters
                    );
                });
            }

            foreach (var methods in type.GetMethods(BindingFlags.Public | BindingFlags.Instance).Where(m => !m.IsSpecialName).GroupBy(method => method.Name))
            {
                obj.Import(StandardMemberRenamer.Rename(methods.Key), (object[] parameters) =>
                {
                    if (parameters.Length == 0)
                    {
                        throw new Exception("No instance of '" + type.FullName + "' provided on method call '" + methods.Key + "'");
                    }

                    var instance = parameters[0];
                    var instanceType = instance.GetType();
                    if (!type.IsAssignableFrom(instanceType))
                    {
                        throw new Exception("Expected instance of '" + type.FullName + "' on method '" + methods.Key + "', received '" + instanceType.FullName + "'");
                    }

                    var methodParameters = parameters.Skip(1).ToArray();

                    return type.InvokeMember(
                        methods.Key,
                        BindingFlags.Public |
                        BindingFlags.Instance |
                        BindingFlags.InvokeMethod |
                        BindingFlags.NonPublic |
                        BindingFlags.OptionalParamBinding,
                        Type.DefaultBinder,
                        instance,
                        methodParameters
                    );
                });
            }

            return obj;
        }

        public static ScriptObject Set<T>(this ScriptObject so, string name) where T : new()
        {
            var obj = Create<T>();
            so.SetValue(name, obj, true);
            return so;
        }

        public static ScriptObject Set(this ScriptObject so, string name, object value)
        {
            so.SetValue(name, value, true);
            return so;
        }

        public static ScriptObject Merge(this ScriptObject so, IScriptObject other)
        {
            so.Import(other);
            return so;
        }

        public static ScriptObject Set(this ScriptObject so, string name, object? value, bool readOnly)
        {
            so.SetValue(name, value, readOnly);
            return so;
        }

        public static ScriptObject Create<T>(string name) where T : new()
        {
            var wrapper = new ScriptObject();
            return wrapper.Set<T>(name);
        }

        public static ScriptObject Create<T>() where T : new()
        {
            var obj = new ScriptObject();
            obj.Import(typeof(T));
            obj.Import(new T());
            return obj;
        }

        public static ScriptObject Create()
        {
            return new ScriptObject();
        }
    }
}