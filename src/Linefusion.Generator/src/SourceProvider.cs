using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

using Microsoft.CodeAnalysis;

using NTypewriter.Runtime;

namespace Linefusion.Generator;

public class SourceProvider : IUserCodeProvider
{
    private readonly List<string> searchPaths = new();

    public SourceProvider()
    {
        this.searchPaths = new();
    }

    public SourceProvider(List<string> paths)
    {
        this.searchPaths = paths;
    }

    public IEnumerable<string> GetUserCodeFilePathsFromProject(string projectPath)
    {
        return searchPaths;
    }
}
