using System;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Linefusion.Generators.Editor.CodeModel.Models;
using NTypewriter.CodeModel;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEditorInternal;
using UnityEngine;
using Assembly = System.Reflection.Assembly;

namespace Linefusion.Generators.Editor
{
    public class UnityAssembly
    {
        public Assembly Assembly { get; set; }
        public ICodeModel Model { get; set; }

        public UnityAssembly(Assembly assembly, ICodeModel model)
        {
            Assembly = assembly;
            Model = model;
        }
    }

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
            get { return UnityUtils.ProjectDir; }
        }

        public string AssetsDir
        {
            get { return UnityUtils.AssetsDir; }
        }

        public UnityAssembly[] Assemblies
        {
            get
            {
                return CompilationPipeline
                    .GetAssemblies()
                    .Select(asm => asm.name)
                    .Select(CompilationPipeline.GetAssemblyDefinitionFilePathFromAssemblyName)
                    .Where(path =>
                        AssetDatabase.GetMainAssetTypeAtPath(path)
                        == typeof(AssemblyDefinitionAsset)
                    )
                    .Select(AssetDatabase.LoadAssetAtPath<AssemblyDefinitionAsset>)
                    .Select(asset => asset.text)
                    .Select(JsonUtility.FromJson<AssemblyDefinitionData>)
                    .Select(data => data.name)
                    .Select(Assembly.Load)
                    .Where(assembly => assembly != null)
                    .Select(v =>
                    {
                        return new UnityAssembly(v, Model.From(v));
                    })
                    .ToArray();
            }
        }

        public static void Log(params object[] messages)
        {
            var builder = new StringBuilder();
            foreach (var message in messages)
            {
                builder.Append(message);
                builder.Append(" ");
            }
            Debug.Log(builder.ToString());
        }

        public static void Clear()
        {
            var logEntries = System.Type.GetType("UnityEditor.LogEntries, UnityEditor.dll");
            var clearMethod = logEntries.GetMethod(
                "Clear",
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public
            );
            clearMethod.Invoke(null, null);
        }
    }
}
