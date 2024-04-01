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
    public class PathFunctions
    {
        public static string combine(
            TemplateContext context,
            params string[] fragments
        )
        {
            return Path.Combine(fragments).Replace('\\', '/');
        }
    }
}
