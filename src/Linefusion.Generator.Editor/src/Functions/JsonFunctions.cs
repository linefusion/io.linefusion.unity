using Scriban;
using UnityEngine;
using Newtonsoft.Json;

namespace Linefusion.Generators.Functions
{
    public class JsonFunctions
    {
        public static object FromJson(TemplateContext context, string json)
        {
            return JsonUtility.FromJson<object>(json);
        }

        public static string ToJsonWithTypes(TemplateContext context, object value)
        {
            return JsonConvert.SerializeObject(value, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Formatting.Indented,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                TypeNameHandling = TypeNameHandling.All,
                PreserveReferencesHandling = PreserveReferencesHandling.All,
            });
        }

        public static string ToJson(TemplateContext context, object value)
        {
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Include,
                Formatting = Formatting.Indented,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                TypeNameHandling = TypeNameHandling.None,
            };
            return JsonConvert.SerializeObject(value.ToObject(), settings);
        }
    }
}
