using System.IO;

using System.Collections.Generic;

using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Linefusion.Generators.Editor
{
    public static class Content
    {
        public const string AssetsRoot = "Assets/Linefusion/Generators/Content/";
        public const string PackagesRoot = "Packages/io.linefusion.unity.generator/Content/";

        private static readonly Dictionary<string, object> loaded = new Dictionary<string, object>();

        public static T Load<T>(string path, bool cache = true) where T : Object
        {
            if (cache && loaded.TryGetValue(path, out object value)) {
                return (T)value;
            }

            var result = TryLoad(PackagesRoot) ?? TryLoad(AssetsRoot);
            if (result) {
                loaded[path] = result;
            }

            return result;

            T TryLoad(string root)
            {
                string fullPath = Path.Combine(root, path);
                return AssetDatabase.LoadAssetAtPath<T>(fullPath);
            }
        }
        
        public static bool Exists(string pathFromRoot)
        {
            return ExistsInPackages(pathFromRoot) || ExistsInAssets(pathFromRoot);
        }

        private static bool ExistsInAssets(string pathFromRoot)
        {
            return File.Exists(AssetsRoot + pathFromRoot);
        }

        private static bool ExistsInPackages(string pathFromRoot)
        {
            return File.Exists(PackagesRoot + pathFromRoot);
        }
    }
}