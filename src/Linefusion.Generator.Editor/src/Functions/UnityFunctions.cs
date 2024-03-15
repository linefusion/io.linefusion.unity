
using UnityEditor;
using UnityEngine;
using UnityEditor.Compilation;
using System.Linq;
using UnityEditorInternal;
using Assembly = System.Reflection.Assembly;
using JetBrains.Annotations;
using NTypewriter.CodeModel;
using Linefusion.Generators.Editor.CodeModel.Models;

namespace Linefusion.Generators.Editor
{
    public class UnityFunctions
    {
        public struct AssemblyDefinitionData
        {
            public string name;
        }

        public struct AssemblyDefinitionReferenceData
        {
            public string reference;
        }

        public string ProjectDir
        {
            get
            {
                return UnityUtils.ProjectDir;
            }
        }

        public string AssetsDir
        {
            get
            {
                return UnityUtils.AssetsDir;
            }
        }

        public Assembly[] Assemblies
        {
            get
            {
                return CompilationPipeline.GetAssemblies()
                    .Select(asm => asm.name)
                    .Select(CompilationPipeline.GetAssemblyDefinitionFilePathFromAssemblyName)
                    .Where(path => AssetDatabase.GetMainAssetTypeAtPath(path) == typeof(AssemblyDefinitionAsset))
                    .Where(path => path.StartsWith("Assets/"))
                    .Select(AssetDatabase.LoadAssetAtPath<AssemblyDefinitionAsset>)
                    .Select(asset => asset.text)
                    .Select(JsonUtility.FromJson<AssemblyDefinitionData>)
                    .Select(data => data.name)
                    .Select(Assembly.Load)
                    .Select(v =>
                    {
                        return v;
                    })
                    .ToArray();
            }
        }

        public ICodeModel[] code
        {
            get
            {
                return CompilationPipeline.GetAssemblies()
                    .Select(asm => asm.name)
                    .Select(CompilationPipeline.GetAssemblyDefinitionFilePathFromAssemblyName)
                    .Where(path => AssetDatabase.GetMainAssetTypeAtPath(path) == typeof(AssemblyDefinitionAsset))
                    //.Where(path => path.StartsWith("Assets/"))
                    .Select(AssetDatabase.LoadAssetAtPath<AssemblyDefinitionAsset>)
                    .Select(asset => asset.text)
                    .Select(JsonUtility.FromJson<AssemblyDefinitionData>)
                    .Select(data => data.name)
                    .Select(Assembly.Load)
                    .Select(v => Model.From(v))
                    .ToArray();
            }
        }

        public static void Log(object message)
        {
            Debug.Log(message);
        }

        public static void Clear()
        {
            var logEntries = System.Type.GetType("UnityEditor.LogEntries, UnityEditor.dll");
            var clearMethod = logEntries.GetMethod("Clear", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            clearMethod.Invoke(null, null);
        }
    }
}