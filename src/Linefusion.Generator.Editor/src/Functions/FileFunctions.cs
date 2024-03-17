using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Linefusion.Generator.IO;
using Scriban;
using Scriban.Runtime;
using UnityEditor;
using UnityEngine;

namespace Linefusion.Generators.Functions
{
    public class FileFunctions
    {
        [ScriptMemberIgnore]
        private static readonly List<string> modifiedFiles = new List<string>();

        [ScriptMemberIgnore]
        public static bool Dirty => modifiedFiles.Count > 0;

        [ScriptMemberIgnore]
        public static void Reset()
        {
            modifiedFiles.Clear();
        }

        [ScriptMemberIgnore]
        public static void Reimport()
        {
            if (modifiedFiles.Count <= 0)
            {
                return;
            }

            try
            {
                AssetDatabase.StartAssetEditing();
                foreach (var path in modifiedFiles)
                {
                    var relative = SafeIO.GetRelativePath(UnityUtils.ProjectDir, path);
                    try
                    {
                        AssetDatabase.ImportAsset(relative, ImportAssetOptions.ForceUpdate);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError("File import error: " + relative);
                        Debug.LogException(e);
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }

            modifiedFiles.Clear();
        }

        public static void Dirtify(string path)
        {
            modifiedFiles.Add(SafeIO.Resolve(path));
        }

        public static void Delete(TemplateContext context, string path, bool meta = false)
        {
            using var workingDir = context.UseCurrentFileAsWorkingDir();

            SafeIO.DeleteFile(path, meta);
        }

        public static void DeleteMany(
            TemplateContext context,
            string path,
            string pattern,
            bool recursive = true,
            bool meta = false
        )
        {
            using var workingDir = context.UseCurrentFileAsWorkingDir();

            foreach (var file in SafeIO.ListFiles(path, pattern, recursive))
            {
                FileFunctions.Dirtify(file);
                SafeIO.DeleteFile(file, meta);
            }
        }

        public static void Write(
            TemplateContext context,
            string path,
            string contents,
            object? meta = null
        )
        {
            using var workingDir = context.UseCurrentFileAsWorkingDir();

            path = SafeIO.Resolve(path);
            SafeIO.CreateParent(path);

            modifiedFiles.Add(path);

            SafeIO.WriteAllText(path, contents);

            var relative = UnityUtils.GetPathRelativeToProject(path);
            var guid = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(relative));
            if (!SafeIO.MetaExists(path))
            {
                var metaPath = path + ".meta";
                var metaContents =
                    $"fileFormatVersion: 2\nguid: {System.BitConverter.ToString(guid).Replace("-", "").ToLower()}\n";
                SafeIO.WriteAllText(metaPath, metaContents);
            }
        }

        public static void RemoveUntouchedUnder(
            TemplateContext context,
            string path,
            string pattern,
            bool meta = false,
            bool recursive = true
        )
        {
            using var workingDir = context.UseCurrentFileAsWorkingDir();

            path = SafeIO.Resolve(path);

            var touchedFiles = modifiedFiles
                .Where(file => SafeIO.IsChildOf(path, file))
                .Select(SafeIO.Normalize);

            var untouchedFiles = SafeIO
                .ListFiles(path, pattern, recursive)
                .Where(entry => !touchedFiles.Contains(entry));

            foreach (var child in untouchedFiles)
            {
                SafeIO.DeleteFile(child, meta);
            }
        }
    }
}
