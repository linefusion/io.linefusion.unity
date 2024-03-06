using UnityEngine;
using UnityEditor;
using UnityEditor.AssetImporters;
using System.IO;

using Linefusion.Generator;
using System.Diagnostics;

using Debug = UnityEngine.Debug;
using System;
using System.Linq;

namespace Linefusion.Generators.Editor
{
    [InitializeOnLoad]
    public class TemplateWatcher
    {
        private static FileSystemWatcher? watcher = null;
        private static readonly string[] extensions = new string[] {
            "liquid",
            "scriban",
            "scriban-cs",
            "scriban-txt",
            "scriban-htm",
            "scriban-html",
            "sbn-cs",
            "sbn-txt",
            "sbn-htm",
            "sbn-html",
            "sbn",
            "sbncs",
            "sbntxt",
            "sbnhtm",
            "sbnhtml",
        };

        static TemplateWatcher()
        {
            Initialize();
        }

        public static void RegisterExtensions()
        {
            var exts = EditorSettings.projectGenerationUserExtensions.ToList();
            exts.AddRange(extensions);
            exts.Sort();

            EditorSettings.projectGenerationUserExtensions = exts.Distinct().ToArray();
        }

        public static void Initialize()
        {
            RegisterExtensions();

            if (watcher != null)
            {
                watcher.Dispose();
                watcher = null;
            }

            watcher = new FileSystemWatcher(UnityUtils.AssetsDir.Value)
            {
                NotifyFilter = NotifyFilters.Attributes
                                  | NotifyFilters.CreationTime
                                  | NotifyFilters.DirectoryName
                                  | NotifyFilters.FileName
                                  | NotifyFilters.LastAccess
                                  | NotifyFilters.LastWrite
                                  | NotifyFilters.Security
                                  | NotifyFilters.Size
            };

            watcher.Changed += (sender, args) =>
            {
                Dispatcher.Enqueue(() =>
                {
                    if (IsTemplateAsset(args.FullPath))
                    {
                        OnChanged(sender, args);
                    }
                });
            };

            watcher.Created += (sender, args) =>
            {
                Dispatcher.Enqueue(() =>
                {
                    if (IsTemplateAsset(args.FullPath))
                    {
                        OnCreated(sender, args);
                    }
                });
            };

            watcher.Deleted += (sender, args) =>
            {
                Dispatcher.Enqueue(() =>
                {
                    if (IsTemplateAsset(args.FullPath))
                    {
                        OnDeleted(sender, args);
                    }
                });
            };

            watcher.Renamed += (sender, args) =>
            {
                Dispatcher.Enqueue(() =>
                {
                    if (IsTemplateAsset(args.FullPath))
                    {
                        OnRenamed(sender, args);
                    }
                });
            };

            watcher.Error += (sender, args) =>
            {
                Dispatcher.Enqueue(() => OnError(sender, args));
            };

            watcher.IncludeSubdirectories = true;
            watcher.EnableRaisingEvents = true;
        }

        private static bool IsTemplateAsset(string assetPath)
        {
            // Debug.Log("IsTemplate? " + assetPath);

            var relativePath = UnityUtils.GetRelativeProjectPath(assetPath);
            var extension = Path.GetExtension(relativePath).TrimStart('.');
            if (!extensions.Contains(extension))
            {
                // Debug.Log("Unknown extension: " + extension);
                return false;
            }

            var guid = AssetDatabase.AssetPathToGUID(relativePath, AssetPathToGUIDOptions.OnlyExistingAssets);
            if (string.IsNullOrEmpty(guid))
            {
                // Debug.Log("Guid is null or empty");
                return false;
            }

            try
            {
                AssetDatabase.ImportAsset(relativePath, ImportAssetOptions.ForceUpdate);
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to check if asset is a template: " + relativePath + " - " + e.Message + " - " + e.StackTrace);
                Debug.LogException(e);
            }
            finally
            {
            }
            var assetType = AssetDatabase.GetMainAssetTypeAtPath(relativePath);
            var templateType = typeof(Template);
            // Debug.Log("Is it? " + assetType.FullName + " == " + templateType.FullName);
            return assetType == templateType;
        }

        private static Template GetTemplate(string path)
        {
            var relativePath = UnityUtils.GetRelativeProjectPath(path);
            try
            {
                AssetDatabase.ImportAsset(relativePath, ImportAssetOptions.ForceUpdate);
            }
            finally
            {
            }
            return AssetDatabase.LoadAssetAtPath<Template>(relativePath);
        }

        private static void OnChanged(object sender, FileSystemEventArgs args)
        {
            try
            {
                var template = GetTemplate(args.FullPath);
                if (template != null && template.AutoGenerate)
                {
                    TemplateGenerator.Generate(template, args.FullPath);
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private static void OnCreated(object sender, FileSystemEventArgs args)
        {
            // Debug.Log("OnCreated: " + args.FullPath);
        }

        private static void OnDeleted(object sender, FileSystemEventArgs args)
        {
            // Debug.Log("OnDeleted: " + args.FullPath);
        }

        private static void OnRenamed(object sender, RenamedEventArgs args)
        {
            // Debug.Log("OnRenamed: " + args.FullPath);
        }

        private static void OnError(object sender, ErrorEventArgs args)
        {
            // Debug.Log("OnError: " + args.GetException().Message);
        }
    }
}