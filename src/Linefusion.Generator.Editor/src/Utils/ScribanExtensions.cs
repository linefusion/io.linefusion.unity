using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading;

using Linefusion.Generator.IO;

using Newtonsoft.Json.Linq;

using Scriban;
using Scriban.Helpers;
using Scriban.Runtime;

using UnityEngine;

namespace Linefusion.Generators.Functions
{
    /// <summary>
    /// Extensions attached to an <see cref="JsonElement"/>.
    /// </summary>
    internal static class ScribanExtensions
    {
        public static IDisposable UseCurrentFileAsWorkingDir(this TemplateContext context)
        {
            var basePath = new SafePath(context.CurrentSourceFile).Parent;
            return SafeIO.UsePath(basePath);
        }

        internal static dynamic ToObject(this object value)
        {
            if (value is ScriptObject scriptObject)
            {
                var obj = new ExpandoObject() as IDictionary<string, object>;
                foreach (var property in scriptObject)
                {
                    obj[property.Key] = property.Value.ToObject();
                }
                return obj;
            }
            else if (value is ScriptArray scriptArray)
            {
                var obj = new List<dynamic>();
                foreach (var item in scriptArray)
                {
                    obj.Add(item.ToObject());
                }
                return obj;
            }
            else if (value is IDictionary<string, object> dictValue)
            {
                var obj = new ExpandoObject() as IDictionary<string, object>;
                foreach (var property in dictValue)
                {
                    obj[property.Key] = property.Value.ToObject();
                }
                return obj;
            }
            else if (value is IList<object> listValue)
            {
                var obj = new List<dynamic>();
                foreach (var item in listValue)
                {
                    obj.Add(item.ToObject());
                }
                return obj;
            }
            else
            {
                return value;
            }
        }
    }
}