using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.IO;
using System;

using Linefusion.Generator.IO;

namespace Linefusion.Generator.Paths;

public readonly struct FullPath : IEquatable<FullPath>, IComparable<FullPath>
{
    internal readonly string? _value;

    private FullPath(string path)
    {
        _value = path;
    }

    public static FullPath Empty => default;

    public bool IsEmpty => _value is null;

    public string Value => _value ?? "";

    public static implicit operator string(FullPath fullPath) => fullPath.ToString();

    public static bool operator ==(FullPath path1, FullPath path2) => path1.Equals(path2);
    public static bool operator !=(FullPath path1, FullPath path2) => !(path1 == path2);
    public static bool operator <(FullPath path1, FullPath path2) => path1.CompareTo(path2) < 0;
    public static bool operator >(FullPath path1, FullPath path2) => path1.CompareTo(path2) > 0;
    public static bool operator <=(FullPath path1, FullPath path2) => path1.CompareTo(path2) <= 0;
    public static bool operator >=(FullPath path1, FullPath path2) => path1.CompareTo(path2) >= 0;

    public static FullPath operator /(FullPath rootPath, string relativePath) => Combine(rootPath, relativePath);

    public FullPath Parent
    {
        get
        {
            var result = Path.GetDirectoryName(_value);
            if (result is null)
                return Empty;

            return new FullPath(result);
        }
    }

    public string? Name => Path.GetFileName(_value);

    public string? NameWithoutExtension => Path.GetFileNameWithoutExtension(_value);

    public string? Extension => Path.GetExtension(_value);

    public int CompareTo(FullPath other) => FullPathComparer.Default.Compare(this, other);
    public int CompareTo(FullPath other, bool ignoreCase) => FullPathComparer.GetComparer(ignoreCase).Compare(this, other);

    public override bool Equals(object? obj) => obj is FullPath path && Equals(path);
    public bool Equals(FullPath other) => FullPathComparer.Default.Equals(this, other);
    public bool Equals(FullPath other, bool ignoreCase) => FullPathComparer.GetComparer(ignoreCase).Equals(this, other);

    public override int GetHashCode() => FullPathComparer.Default.GetHashCode(this);
    public int GetHashCode(bool ignoreCase) => FullPathComparer.GetComparer(ignoreCase).GetHashCode(this);

    public override string ToString() => Value;

    public string MakePathRelativeTo(FullPath rootPath)
    {
        if (IsEmpty)
            throw new InvalidOperationException("The path is empty");

        if (rootPath.IsEmpty)
            return Value;

        if (rootPath == this)
            return ".";

        return PathDifference(rootPath.Value, Value, compareCase: FullPathComparer.Default.IsCaseSensitive);
    }

    private static string PathDifference(string path1, string path2, bool compareCase)
    {
        var directorySeparator = Path.DirectorySeparatorChar;

        int i;
        var si = -1;
        for (i = 0; (i <= path1.Length) && (i < path2.Length); ++i)
        {
            char c1 = i == path1.Length ? directorySeparator : path1[i];
            char c2 = path2[i];

            if ((c1 != c2) && (compareCase || (char.ToUpperInvariant(c1) != char.ToUpperInvariant(c2))))
                break;

            if (c1 == directorySeparator)
            {
                si = i;
            }
        }

        if (i == 0)
            return path2;

        if ((i == path1.Length + 1) && (i == path2.Length))
            return string.Empty;

        var relPath = new StringBuilder();
        // Walk down several dirs
        for (; i <= path1.Length; ++i)
        {
            char c = i == path1.Length ? directorySeparator : path1[i];
            if (c == directorySeparator)
            {
                relPath.Append("..");
                relPath.Append(directorySeparator);
            }
        }

        return relPath.Append(path2.Substring(si + 1)).ToString();
    }

    public bool IsChildOf(FullPath rootPath)
    {
        if (IsEmpty)
            throw new InvalidOperationException("Path is empty");
        if (rootPath.IsEmpty)
            throw new ArgumentException("Root path is empty", nameof(rootPath));

        if (Value.Length <= rootPath.Value.Length)
            return false;

        if (!Value.StartsWith(rootPath.Value, StringComparison.Ordinal))
            return false;

        // rootpath: /a/b
        // current:  /a/b/c => true
        // current:  /a/b/  => false
        // current:  /a/bc  => false
        if (Value[rootPath.Value.Length] == Path.DirectorySeparatorChar && Value.Length > rootPath.Value.Length + 1)
            return true;

        return false;
    }

    public void CreateParentDirectory()
    {
        if (IsEmpty)
            return;

        var parent = Path.GetDirectoryName(Value);
        if (parent is not null)
        {
            Directory.CreateDirectory(parent);
        }
    }

    public FullPath ChangeExtension(string? extension)
    {
        if (IsEmpty)
            return Empty;

        return new FullPath(Path.ChangeExtension(Value, extension));
    }

    public static FullPath GetTempPath() => FromPath(Path.GetTempPath());

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static FullPath GetTempFileName() => FromPath(Path.GetTempFileName());

    public static FullPath CreateTempFile() => FromPath(Path.GetTempFileName());

    public static FullPath GetFolderPath(Environment.SpecialFolder folder) => FromPath(Environment.GetFolderPath(folder));
    public static FullPath CurrentDirectory() => FromPath(PathUtils.WorkingPath);

    public static FullPath FromPath(string path)
    {
        if (PathInternal.IsExtended(path))
        {
            path = path.Substring(4);
        }

        var fullPath = PathUtils.GetFullPath(path);
        var fullPathWithoutTrailingDirectorySeparator = TrimEndingDirectorySeparator(fullPath);
        if (string.IsNullOrEmpty(fullPathWithoutTrailingDirectorySeparator))
            return Empty;

        return new FullPath(fullPathWithoutTrailingDirectorySeparator);
    }


    private static string TrimEndingDirectorySeparator(string path)
    {
        if (!path.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal) && !path.EndsWith(Path.AltDirectorySeparatorChar.ToString(), StringComparison.Ordinal))
            return path;

        if (path.StartsWith("\\", StringComparison.Ordinal))
            throw new ArgumentException("UNC paths are not supported", nameof(path));

        return path.Substring(0, path.Length - 1);
    }

    public static FullPath Combine(string rootPath, string relativePath) => FromPath(Path.Combine(rootPath, relativePath));
    public static FullPath Combine(string rootPath, string path1, string path2) => FromPath(Path.Combine(rootPath, path1, path2));
    public static FullPath Combine(string rootPath, string path1, string path2, string path3) => FromPath(Path.Combine(rootPath, path1, path2, path3));
    public static FullPath Combine(params string[] paths) => FromPath(Path.Combine(paths));

    public static FullPath Combine(FullPath rootPath, string relativePath)
    {
        if (rootPath.IsEmpty)
            return FromPath(relativePath);

        return FromPath(Path.Combine(rootPath.Value, relativePath));
    }

    public static FullPath Combine(FullPath rootPath, string path1, string path2)
    {
        if (rootPath.IsEmpty)
            return FromPath(Path.Combine(path1, path2));

        return FromPath(Path.Combine(rootPath.Value, path1, path2));
    }

    public static FullPath Combine(FullPath rootPath, params string[] paths)
    {
        if (rootPath.IsEmpty)
            return FromPath(Path.Combine(paths));

        return FromPath(Path.Combine(rootPath.Value, Path.Combine(paths)));
    }

    public static FullPath Combine(FullPath rootPath, string path1, string path2, string path3)
    {
        if (rootPath.IsEmpty)
            return FromPath(Path.Combine(path1, path2, path3));

        return FromPath(Path.Combine(rootPath.Value, path1, path2, path3));
    }

    public static FullPath FromFileSystemInfo(FileSystemInfo? fsi)
    {
        if (fsi is null)
            return Empty;

        return FromPath(fsi.FullName);
    }
}