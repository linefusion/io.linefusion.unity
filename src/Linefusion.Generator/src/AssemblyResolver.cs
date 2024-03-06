using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Linefusion.Generator;

public static class AssemblyResolver
{
    public static Assembly Resolve(object sender, ResolveEventArgs args)
    {
        return Resolve(Assembly.GetExecutingAssembly(), sender, args);
    }

    public static Assembly Resolve(Assembly context, object sender, ResolveEventArgs args)
    {
        var asmName = new AssemblyName(args.Name);
        Trace.WriteLine($"AssemblyResolver.Resolve : {asmName.FullName}");

        var loadedAssembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().FullName == asmName.FullName);
        if (loadedAssembly != null)
        {
            Trace.WriteLine($"AssemblyResolver.Resolve : resolved from AppDomain.CurrentDomain");
            return loadedAssembly;
        }

        string resourceName = $"{context.FullName}.{asmName.Name}.dll";
        var manifestResourceNames = context.GetManifestResourceNames();
        if (manifestResourceNames.Contains(resourceName))
        {
        }

        using Stream resourceStream = context.GetManifestResourceStream(resourceName);

        if (resourceStream == null)
        {
            Trace.WriteLine($"AssemblyResolver.Resolve : failure");
#pragma warning disable CS8603 // Possible null reference return.
            return null;
#pragma warning restore CS8603 // Possible null reference return.
        }

        try
        {
            var dllName = $"{asmName.Name}-{asmName.Version}.dll";
            var tempPath = Path.Combine(Path.GetTempPath(), "Linefusion.Generator");
            var filePath = Path.Combine(tempPath, dllName);
            Directory.CreateDirectory(tempPath);

            if (!File.Exists(filePath))
            {
                using FileStream fileStream = File.Create(filePath);
                resourceStream.CopyTo(fileStream);
            }

            return Assembly.LoadFile(filePath);
        }
        catch
        {

        }

        using MemoryStream memoryStream = new MemoryStream();

        Trace.WriteLine($"AssemblyResolver.Resolve : resolved from ResourceStream");
        resourceStream.CopyTo(memoryStream);

        return Assembly.Load(memoryStream.ToArray());
    }
}