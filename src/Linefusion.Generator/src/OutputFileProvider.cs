using System;
using System.Composition;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.IO;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NTypewriter.Runtime;
using System.Linq;

namespace Linefusion.Generator;

public class OutputFileProvider : IGeneratedFileReaderWriter
{
    private readonly GeneratorExecutionContext context;

    public OutputFileProvider(GeneratorExecutionContext context)
    {
        this.context = context;
    }

    public bool Exists(string path)
    {
        return !IsHiddenSource(path) && File.Exists(path);
    }

    public Task<string> Read(string path)
    {
        return Task.FromResult(IsHiddenSource(path) ? string.Empty : File.ReadAllText(path));
    }

    public async Task Write(string path, string source)
    {
        if (IsHiddenSource(path))
        {
            context.AddSource(GetFileNameWithHash(path), source);
        }
        else
        {
            await WriteFile(path, source);
        }
    }

    private bool IsHiddenFile(string path)
    {
        return Path.GetFileName(path).EndsWith(".hidden.cs", StringComparison.OrdinalIgnoreCase);
    }

    private bool IsHiddenSource(string path)
    {
        return IsHiddenFile(path);
    }

    private byte[] GetHashFromBuffer(byte[] data)
    {
        using var sha = SHA256.Create();
        return sha.ComputeHash(data);
    }

    private byte[] GetHashFromFile(string path)
    {
        if (File.Exists(path) == false)
        {
            return Array.Empty<byte>();
        }

        var buffer = File.ReadAllBytes(path);

        using var sha = SHA256.Create();
        return sha.ComputeHash(buffer);
    }

    private string ToHex(byte[] hash)
    {
        return BitConverter.ToString(hash).Replace("-", String.Empty).ToLowerInvariant();
    }

    private async Task WriteFile(string path, string source)
    {
        var encoder = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
        if (Path.GetExtension(path).Equals(".cs", StringComparison.OrdinalIgnoreCase))
        {
            source = CSharpSyntaxTree.ParseText(source).GetRoot().NormalizeWhitespace().SyntaxTree.GetText().ToString();
        }

        source = encoder.GetString(encoder.GetBytes(source));

        var data = encoder.GetBytes(source);

        await Task.Run(() =>
        {
            var dir = Path.GetDirectoryName(path);
            if (Directory.Exists(dir) == false)
            {
                Directory.CreateDirectory(dir);
            }

            var fileHash = GetHashFromFile(path);
            var sourceHash = GetHashFromBuffer(data);

            if (fileHash.SequenceEqual(sourceHash))
            {
                return;
            }

            Files.Write(path, data);
        });
    }

    private string GetFileNameWithHash(string path)
    {
        var hash = ToHex(GetHashFromFile(path)).Substring(8);
        return $"{Path.GetFileNameWithoutExtension(path)}.{hash}{Path.GetExtension(path)}";
    }
}
