using System.IO;

using Linefusion.Generator;
using Linefusion.Generator.IO;

using UnityEngine;

namespace Linefusion.Generators
{
    public static class UnityUtils
    {
        public static PathValue ProjectDir
        {
            get
            {
                return Path.GetFullPath(Path.Combine(AssetsDir, ".."));
            }
        }

        public static PathValue PackagesDir
        {
            get
            {
                return Path.GetFullPath(Path.Combine(AssetsDir, "../Packages"));
            }
        }

        public static PathValue AssetsDir
        {
            get
            {
                return Path.GetFullPath(Path.Combine(Application.dataPath));
            }
        }

        public static string GetRelativeProjectPath(string path)
        {
            return Path.GetRelativePath(UnityUtils.ProjectDir, path);
        }

        public static string GetRelativeAssetsPath(string path)
        {
            return Path.GetRelativePath(UnityUtils.AssetsDir, path);
        }
    }
}