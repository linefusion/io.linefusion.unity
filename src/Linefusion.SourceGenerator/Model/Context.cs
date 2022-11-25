using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.CodeAnalysis;

using Roslyn.Reflection;

namespace Linefusion.SourceGenerator.Model;

public class Context
{
    private readonly MetadataLoadContext? _metadata;
    private readonly System.Reflection.Assembly _reflectionAssembly;

    private readonly Assembly _assembly;
    public Assembly Assembly => _assembly;

    public Context(System.Reflection.Assembly from)
    {
        this._reflectionAssembly = from;
        this._metadata = null;
        this._assembly = new Assembly(_reflectionAssembly);
    }

    public Context(Compilation compilation)
    {
        this._metadata = new(compilation);
        this._reflectionAssembly = _metadata.Assembly;
        this._assembly = new Assembly(_reflectionAssembly);
    }
}
