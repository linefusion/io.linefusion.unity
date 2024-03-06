using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

using Linefusion.Generator.IO;

using Scriban;

using UnityEditor;

using UnityEngine;


namespace Linefusion.Generators.Functions
{
    public class ReflectionFunctions
    {
        public static Type ToType(TypeInfo value)
        {
            return value.AsType();
        }

        public static Type GetType(string name)
        {
            return Type.GetType(name);
        }
    }
    
    
}
