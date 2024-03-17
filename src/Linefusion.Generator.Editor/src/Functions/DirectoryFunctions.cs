using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Linefusion.Generator.IO;
using NTypewriter.CodeModel;
using Scriban;
using Scriban.Runtime;
using UnityEditor;
using UnityEngine;

namespace Linefusion.Generators.Functions
{
    public class DirFunctions
    {
        public static IEnumerable<string> ListFiles(
            TemplateContext context,
            string path,
            string pattern = "*",
            bool recursive = true
        )
        {
            using var workingDir = context.UseCurrentFileAsWorkingDir();
            return SafeIO.ListFiles(path, pattern, recursive);
        }

        public static IEnumerable<string> ListDirectories(
            TemplateContext context,
            string path,
            string pattern = "*",
            bool recursive = true
        )
        {
            using var workingDir = context.UseCurrentFileAsWorkingDir();
            return SafeIO.ListDirectories(path, pattern, recursive);
        }


        public static void Delete(TemplateContext context, string path, bool meta = false)
        {
            using var workingDir = context.UseCurrentFileAsWorkingDir();

            SafeIO.DeleteDirectory(path, meta);
        }
    }
}
