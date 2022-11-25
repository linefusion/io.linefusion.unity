namespace Linefusion.SourceGenerator.Abstractions.Attributes;

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public class UseGenerator : Attribute
{
    public string Path { get; private set; } = "";
    
    public UseGenerator(string path)
    {
        Path = path;
    }
}
