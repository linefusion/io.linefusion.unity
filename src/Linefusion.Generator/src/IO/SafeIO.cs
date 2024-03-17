using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using NTypewriter.Runtime;

namespace Linefusion.Generator.IO
{
    public static class SafeIO
    {
        private static readonly string DirSeparator = "/";
        private static readonly char DirSeparatorChar = '/';
        private static readonly string AltDirSeparator = "\\";
        private static readonly char AltDirSeparatorChar = '\\';
        private static readonly string ParentDir = "..";
        private static readonly string CurrentDir = ".";

        private static readonly string DirSeparatorToken = "";
        private static readonly string AltDirSeparatorToken = "";
        private static readonly string DirSeparatorsExpression = "";
        private static readonly string ParentDirToken = "";
        private static readonly string CurrentDirToken = "";
        private static readonly string SpecialDirExpression = "";

        public static bool IsSafe
        {
            get
            {

                lock (locker)
                {
                    return safetyStack.Value.Count > 0 ? safetyStack.Value.Peek() : true;
                }
            }
        }

        private static readonly object locker = new();

        private static readonly ThreadLocal<Stack<string>> pathStack = new(() => new());
        private static readonly ThreadLocal<Stack<bool>> safetyStack = new(() => new());

        public static string Root { get; set; } = Directory.GetCurrentDirectory();

        public static string CurrentDirectory
        {
            get
            {
                lock (locker)
                {
                    return pathStack.Value.Count > 0 ? pathStack.Value.Peek() : Root;
                }
            }
        }

        private class PathOverride : IDisposable
        {
            public PathOverride(string path)
            {
                lock (locker)
                {
                    Resolve(path);
                    pathStack.Value.Push(path);
                }
            }

            public void Dispose()
            {
                lock (locker)
                {
                    pathStack.Value.Pop();
                }
            }
        }

        public static IDisposable UseSafetyChecks(bool enabled = true)
        {
            return new SatefyCheckOverride(enabled);
        }

        private class SatefyCheckOverride : IDisposable
        {
            public SatefyCheckOverride(bool enabled)
            {
                lock (locker)
                {
                    safetyStack.Value.Push(enabled);
                }
            }

            public void Dispose()
            {
                lock (locker)
                {
                    safetyStack.Value.Pop();
                }
            }
        }

        public static IDisposable UsePath(string path)
        {
            return new PathOverride(path);
        }

        static SafeIO()
        {
            ParentDirToken = Regex.Escape(ParentDir);
            CurrentDirToken = Regex.Escape(CurrentDir);
            DirSeparatorToken = Regex.Escape(DirSeparator);
            AltDirSeparatorToken = Regex.Escape(AltDirSeparator);
            DirSeparatorsExpression = $"({DirSeparatorToken}|{AltDirSeparatorToken})";
            SpecialDirExpression = $"({ParentDirToken}|{CurrentDirToken})";
        }

        public static bool IsChildOf(string parent, string child)
        {
            parent = Resolve(parent);
            child = Resolve(child);

            if (!IsDirectory(parent) && !IsDirectory(parent + DirSeparator))
            {
                return false;
            }

            if (parent == child)
            {
                return true;
            }

            if (!EndsWithDirectorySeparator(parent))
            {
                parent = parent + DirSeparator;
            }

            return child.StartsWith(parent) && child.Length > parent.Length;
        }

        public static string GetFullPath(string path)
        {
            if (IsPartiallyQualified(path))
            {
                path = Path.Combine(SafeIO.CurrentDirectory, path);
            }
            return Path.GetFullPath(path);
        }

        public static bool IsRooted(string path) => Path.IsPathRooted(path);

        public static bool IsRelative(string path) => IsPartiallyQualified(path);

        public static bool IsAbsolute(string path) => IsFullyQualified(path);

        public static bool IsFile(string path)
        {
            path = Resolve(path);
            if (Exists(path))
            {
                return File.Exists(path);
            }
            else
            {
                return !EndsWithDirectorySeparator(path);
            }
        }

        public static bool IsDirectory(string path)
        {
            path = Resolve(path);
            if (Exists(path))
            {
                return Directory.Exists(path);
            }
            else
            {
                return EndsWithDirectorySeparator(path);
            }
        }

        public static bool EndsWithDirectorySeparator(string path) =>
            path.EndsWith(Path.DirectorySeparatorChar.ToString())
            || path.EndsWith(Path.AltDirectorySeparatorChar.ToString());

        public static bool IsFullyQualified(string path) => !IsPartiallyQualified(path);

        public static bool IsPartiallyQualified(string path)
        {
            if (Regex.IsMatch(path, @"^[A-Za-z]:(\\|/)"))
            {
                return false;
            }

            if (Regex.IsMatch(path, $"^${SpecialDirExpression}${DirSeparatorsExpression}?"))
            {
                return true;
            }

            if (!Regex.IsMatch(path, $"^${DirSeparatorsExpression}+"))
            {
                return true;
            }

            return !IsRooted(path);
        }

        public static IEnumerable<string> GetSegments(string path)
        {
            return path.Replace(AltDirSeparator, DirSeparator)
                .Split(DirSeparator)
                .Where(segment => !string.IsNullOrEmpty(segment));
        }

        public static string Assemble(IEnumerable<string> segments)
        {
            return string.Join(DirSeparator, segments);
        }

        public static string GetParent(string path)
        {
            return Assemble(GetSegments(path).SkipLast(1));
        }

        public static bool IsDirectorySeparator(char c) =>
            c == DirSeparatorChar || c == AltDirSeparatorChar;

        public static bool IsValidDriveChar(char value)
        {
            return (value is >= 'A' and <= 'Z') || (value is >= 'a' and <= 'z');
        }

        public static bool IsValidPattern(string pattern)
        {
            return !pattern.Contains(Path.DirectorySeparatorChar)
                & !pattern.Contains(Path.AltDirectorySeparatorChar);
        }

        public static void ValidatePattern(string pattern)
        {
            if (!IsValidPattern(pattern))
            {
                throw new Exception($"Pattern should not contain directory separators: ${pattern}");
            }
        }

        public static string ToAbsolute(string path)
        {
            if (IsAbsolute(path))
            {
                return path;
            }
            return Resolve(path);
        }

        public static string GetRelativePath(string relativeTo, string path)
        {
            relativeTo = Resolve(relativeTo);
            path = Resolve(path);

            return Normalize(Path.GetRelativePath(relativeTo, path));
        }

        public static string Normalize(string path)
        {
            return Assemble(GetSegments(path));
        }

        public static string Resolve(string path)
        {
            var absolute = Normalize(
                Path.GetFullPath(IsAbsolute(path) ? path : Path.Combine(CurrentDirectory, path))
            );

            if (IsSafe)
            {
                using var checks = UseSafetyChecks(false);

                if (!IsChildOf(Root, absolute))
                {
                    throw new RuntimeException(
                        "Path is not within root directory: " + absolute + "\nRoot: " + Root
                    );
                }
            }

            return absolute;
        }

        public static bool IsSpecial(string path)
        {
            var last = GetSegments(path).Last();
            return IsMeta(path) || last == ".git" || last == ".svn" || last == ".hg";
        }

        public static bool IsMeta(string path)
        {
            return path.EndsWith(".meta", StringComparison.OrdinalIgnoreCase);
        }

        public static string GetMetaFor(string path)
        {
            if (IsMeta(path))
            {
                return path;
            }

            return Resolve(path).TrimEnd(DirSeparatorChar, AltDirSeparatorChar) + ".meta";
        }

        #region Creation

        public static void CreateParent(string path)
        {
            var parent = Resolve(GetParent(path));
            Directory.CreateDirectory(parent);
        }

        public static void WriteAllText(string path, string contents)
        {
            path = Resolve(path);
            CreateParent(path);
            File.WriteAllText(path, contents);
        }

        #endregion

        #region Existence

        public static bool Exists(string path)
        {
            return FileExists(path) || DirectoryExists(path);
        }

        public static bool MetaExists(string path)
        {
            return FileExists(GetMetaFor(path));
        }

        public static bool FileExists(string path)
        {
            return File.Exists(Resolve(path));
        }

        public static bool DirectoryExists(string path)
        {
            return Directory.Exists(Resolve(path));
        }

        #endregion

        #region Deletion

        public static void DeleteMeta(string path)
        {
            DeleteFile(GetMetaFor(path), false);
        }

        public static void DeleteDirectory(string path, bool meta = false)
        {
            path = Resolve(path);

            if (DirectoryExists(path))
            {
                Directory.Delete(path);
            }

            if (meta)
            {
                DeleteMeta(path);
            }
        }

        public static void DeleteFile(string path, bool meta = false)
        {
            path = Resolve(path);

            if (FileExists(path))
            {
                File.Delete(path);
            }

            if (meta)
            {
                DeleteMeta(path);
            }
        }

        public static void Delete(string path, bool meta = false)
        {
            if (IsDirectory(path))
            {
                DeleteDirectory(path);
            }
            else if (IsFile(path))
            {
                DeleteFile(path);
            }
        }

        #endregion

        #region Listing

        private static readonly EnumerationOptions enumFileOptions = new EnumerationOptions()
        {
            ReturnSpecialDirectories = false,
            RecurseSubdirectories = false,
            IgnoreInaccessible = true
        };

        private static readonly EnumerationOptions enumDirOptions = new EnumerationOptions()
        {
            ReturnSpecialDirectories = false,
            RecurseSubdirectories = false,
            IgnoreInaccessible = true
        };

        public static IEnumerable<string> ListFiles(
            string path,
            string pattern,
            bool recursive = true
        )
        {
            ValidatePattern(pattern);

            path = Resolve(path);
            if (!DirectoryExists(path))
            {
                throw new Exception("Directory does not exist: " + path);
            }

            foreach (
                var file in Directory
                    .EnumerateFiles(path, pattern, enumFileOptions)
                    .Where(file => !IsMeta(file))
                    .Select(Resolve)
            )
            {
                yield return file;
            }

            if (recursive)
            {
                var directories = Directory.EnumerateDirectories(path, "*", enumDirOptions);
                foreach (var directory in directories)
                {
                    foreach (
                        var file in ListFiles(directory, pattern, recursive)
                            .Where(file => !IsMeta(file))
                            .Select(Resolve)
                    )
                    {
                        yield return file;
                    }
                }
            }
        }

        public static IEnumerable<string> ListDirectories(
            string path,
            string pattern = "*",
            bool recursive = true
        )
        {
            path = Resolve(path);
            if (!DirectoryExists(path))
            {
                throw new Exception("Directory does not exist: " + path);
            }

            foreach (
                var directory in ListDirectories(path, pattern, recursive)
                    .Where(dir => !IsSpecial(dir))
                    .Select(Resolve)
            )
            {
                yield return directory;
            }

            if (recursive)
            {
                var children = Directory.EnumerateDirectories(path, "*", enumDirOptions);
                foreach (var child in children)
                {
                    foreach (
                        var directory in ListDirectories(child)
                            .Where(dir => !IsSpecial(dir))
                            .Select(Resolve)
                    )
                    {
                        yield return directory;
                    }
                }
            }
        }
        #endregion
    }
}
