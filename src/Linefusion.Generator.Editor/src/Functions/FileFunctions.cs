using System.IO;
using System.Security.Cryptography;
using System.Text;

using Linefusion.Generator.IO;

using Scriban;

using UnityEditor;

using UnityEngine;


namespace Linefusion.Generators.Functions
{
    public class FileFunctions
    {
        public static void Write(TemplateContext context, string path, string contents, object? meta = null)
        {
            var referencePath = new PathValue(context.CurrentSourceFile).Parent;
            using var workingDir = PathUtils.UsePath(referencePath);

            var target = new PathValue(path).Absolute;

            try
            {
                AssetDatabase.StartAssetEditing();

                File.WriteAllText(target, contents);

                var relative = UnityUtils.GetRelativeProjectPath(target);
                var relativeNormalized = relative.Replace("\\", "/");

                var guid = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(relativeNormalized));
                if (!File.Exists(target + ".meta"))
                {
                    var metaPath = target + ".meta";
                    var metaContents = $"fileFormatVersion: 2\nguid: {System.BitConverter.ToString(guid).Replace("-", "").ToLower()}\n";
                    File.WriteAllText(metaPath, metaContents);
                }
                
                AssetDatabase.ImportAsset(relative, ImportAssetOptions.ForceUpdate);
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }
        }
    }
}
