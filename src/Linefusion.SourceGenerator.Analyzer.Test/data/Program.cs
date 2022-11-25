using System;
using Linefusion.SourceGenerator.Abstractions.Attributes;

[AttributeUsage(AttributeTargets.Struct)]
public class MyAttrAttribute : Attribute
{
}

[MyAttr]
public struct Test
{
}

public static class Program
{
    public static void Main()
    {
        Console.WriteLine("Hello World");
    }
}

[assembly: UseGenerator("./Generators/MyGenerator.cs")]