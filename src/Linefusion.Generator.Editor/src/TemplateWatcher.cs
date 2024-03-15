using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Linefusion.Generator;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Linefusion.Generators.Editor
{
    [InitializeOnLoad]
    public class TemplateWatcher
    {
        private static readonly List<FileSystemWatcher> watchers = new();

        private static readonly HashSet<string> watchedFiles = new();

        private static readonly string[] extensions = new string[]
        {
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

            foreach (var watcher in watchers)
            {
                watcher.Dispose();
            }

            watchers.Clear();

            foreach (var extension in extensions)
            {
                watchers.Add(CreateFileWatcher(UnityUtils.AssetsDir.Value, $"*.{extension}"));
                watchers.Add(CreateFileWatcher(UnityUtils.PackagesDir.Value, $"*.{extension}"));
            }

            watchedFiles.Clear();
        }

        public static void WatchFile(string path)
        {
            path = Path.GetFullPath(path);
            watchedFiles.Add(path);
        }

        private static FileSystemWatcher CreateFileWatcher(string path, string filter)
        {
            var watcher = new FileSystemWatcher(path)
            {
                Filter = filter,
                InternalBufferSize = 1024 * 64,
                IncludeSubdirectories = true,
                NotifyFilter =
                    NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.Size,
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

            return watcher;
        }

        private static bool IsTemplateAsset(string assetPath)
        {
            var relativePath = UnityUtils.GetRelativeProjectPath(assetPath);
            var extension = Path.GetExtension(relativePath).TrimStart('.');
            if (!extensions.Contains(extension))
            {
                return false;
            }

            var guid = AssetDatabase.AssetPathToGUID(
                relativePath,
                AssetPathToGUIDOptions.OnlyExistingAssets
            );
            if (string.IsNullOrEmpty(guid))
            {
                return false;
            }

            var assetType = AssetDatabase.GetMainAssetTypeAtPath(relativePath);
            var templateType = typeof(Template);

            return assetType == templateType;
        }

        private static Template GetTemplate(string path)
        {
            var relativePath = UnityUtils.GetRelativeProjectPath(path);
            return AssetDatabase.LoadAssetAtPath<Template>(relativePath);
        }

        private static void OnChanged(object sender, FileSystemEventArgs args)
        {
            try
            {
                var template = GetTemplate(args.FullPath);
                if (template != null)
                {
                    if (template.Type.HasFlag(TemplateType.Include))
                    {
                        TemplateGenerator.Generate(false);
                    }
                    else if (template.Type.HasFlag(TemplateType.Generator))
                    {
                        template.Contents = File.ReadAllText(args.FullPath);
                        TemplateGenerator.Generate(template, args.FullPath);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private static void OnCreated(object sender, FileSystemEventArgs args)
        {
        }

        private static void OnDeleted(object sender, FileSystemEventArgs args)
        {
        }

        private static void OnRenamed(object sender, RenamedEventArgs args)
        {
        }

        private static void OnError(object sender, ErrorEventArgs args)
        {
            Debug.LogError("Template Watch Exception: " +args.GetException().Message);
            Debug.LogException(args.GetException());
        }
    }
}
