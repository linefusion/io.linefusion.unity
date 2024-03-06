using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

using NTypewriter.Runtime;

namespace Linefusion.Generator;

public class Files
{
    public static byte[] Read(string path)
    {
        if (File.Exists(path))
        {
            return File.ReadAllBytes(path);
        }
        return new byte[0];
    }

    public static void Write(string path, string contents)
    {
        var encoder = new UTF8Encoding(false);
        Write(path, encoder.GetBytes(contents));
    }

    public static void Write(string path, byte[] contents)
    {
        var dir = Path.GetDirectoryName(path);
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        if (File.Exists(path))
        {
            var previous = File.ReadAllBytes(path);
            if (previous.SequenceEqual(contents))
            {
                return;
            }
        }

        for (int i = 0; i < 15; i++)
        {
            try
            {
                File.WriteAllBytes(path, contents);
                return;
            }
            catch (Exception)
            {
                Thread.Sleep(100);
            }
        }

        throw new RuntimeException("Failed to write generated file to disk");
    }
}
