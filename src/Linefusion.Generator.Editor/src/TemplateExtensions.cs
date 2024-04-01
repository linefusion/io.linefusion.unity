using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Linefusion.Generator;
using Linefusion.Generators.Functions;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Linefusion.Generators.Editor
{
    public static class TemplateExtensions
    {
        public static readonly string[] Extensions = new string[]
        {
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
            "liquid"
        };
    }
}
