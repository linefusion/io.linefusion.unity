using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

using Linefusion.Generator.IO;


using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using Scriban;

using UnityEditor;

using UnityEngine;


namespace Linefusion.Generators.Functions
{
    public class CsharpFunctions
    {
        public static string Format(string source)
        {
            try
            {
                return CSharpSyntaxTree.ParseText(source).GetRoot().NormalizeWhitespace().SyntaxTree.GetText().ToString();
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to format C# source code");
                Debug.LogException(e);
                throw;
            }
        }
    }
}
