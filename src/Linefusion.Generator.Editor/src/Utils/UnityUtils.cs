using System.IO;

using Linefusion.Generator;
using Linefusion.Generator.IO;

using UnityEditor;

using UnityEngine;

namespace Linefusion.Generators
{
    [InitializeOnLoad]
    public static class UnityUtils
    {
        static UnityUtils()
        {
            SafeIO.Root = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        }

        public static SafePath ProjectDir => new SafePath(".");
        public static SafePath PackagesDir => new SafePath("Packages");
        public static SafePath AssetsDir => new SafePath("Assets");

        public static string GetPathRelativeToProject(string path)
        {
            return SafeIO.GetRelativePath(ProjectDir, path);
        }

        public static string GetPathRelativeToAssets(string path)
        {
            return SafeIO.GetRelativePath(AssetsDir, path);
        }
    }
}