using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using Microsoft.CodeAnalysis.Text;

using Linefusion.SourceGenerator.Abstractions.Attributes;
using Linefusion.SourceGenerator.Analyzer.Test.Utilities;

namespace Linefusion.SourceGenerator.Analyzer.Test;

public class TestGenerator
{
    [Fact]
    public void TestSourceGenerator2()
    {
        var source = DataFiles.ReadMany("Program.cs");

        var results = TestGeneration(source);

        Assert.Single(results.Results);
        Assert.Single(results.Results[0].Diagnostics);

        //Assert.Equal(output.Item1, results.Results[0].GeneratedSources[0].HintName);
        //Assert.Equal(output.Item2.Source(), results.Results[0].GeneratedSources[0].SourceText.Source(), false, true, true);
    }

    private static GeneratorDriverRunResult TestGeneration(params (string, SourceText)[] input)
    {
        var syntaxTrees = new SyntaxTree[input.Length];
        for (int i = 0; i < input.Length; i++)
        {
            var (file, source) = input[i];
            syntaxTrees[i] = CSharpSyntaxTree.ParseText(source, null, file);
        }

        var inputCompilation = CSharpCompilation.Create(
            "compilation",
            syntaxTrees,
            new[] {
                MetadataReference.CreateFromFile(typeof(Binder).GetTypeInfo().Assembly.Location),
                MetadataReference.CreateFromFile(typeof(UseGenerator).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(ScriptedSourceGenerator).Assembly.Location),
            },
            new CSharpCompilationOptions(OutputKind.ConsoleApplication)
        );

        var generator = new ScriptedSourceGenerator();
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

        driver = driver.RunGeneratorsAndUpdateCompilation(inputCompilation, out var outputCompilation, out var diagnostics);

        return driver.GetRunResult();
    }
}
