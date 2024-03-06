using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;

using Linefusion.Generator.Paths;

namespace Linefusion.Generator.IO
{
    public static class PathUtils
    {
        private static readonly string DirSeparator = "/";
        private static readonly string AltDirSeparator = "\\";
        private static readonly string ParentDir = "..";
        private static readonly string CurrentDir = ".";

        private static readonly string DirSeparatorExpression = "";
        private static readonly string AltDirSeparatorExpression = "";
        private static readonly string DirSeparatorsExpression = "";
        private static readonly string ParentDirExpression = "";
        private static readonly string CurrentDirExpression = "";
        private static readonly string RelativeExpression = "";

        private static readonly object locker = new();

        private static readonly ThreadLocal<Stack<string>> pathStack = new(() => new());

        public static string WorkingPath
        {
            get
            {
                lock (locker)
                {
                    return pathStack.Value.Count > 0 ? pathStack.Value.Peek() : Directory.GetCurrentDirectory();
                }
            }
        }

        private class PathOverride : IDisposable
        {
            public PathOverride(string path)
            {
                lock (locker)
                {
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

        public static IDisposable UsePath(string path)
        {
            return new PathOverride(path);
        }

        static PathUtils()
        {
            DirSeparatorExpression = Regex.Escape(DirSeparator);
            AltDirSeparatorExpression = Regex.Escape(AltDirSeparator);
            DirSeparatorsExpression = $"({DirSeparatorExpression}|{AltDirSeparatorExpression})";
            ParentDirExpression = Regex.Escape(ParentDir);
            CurrentDirExpression = Regex.Escape(CurrentDir);
            RelativeExpression = $"({ParentDirExpression}|{CurrentDirExpression})";
        }

        public static bool IsChildOf(string parent, string child)
        {
            return FullPath.FromPath(parent).IsChildOf(FullPath.FromPath(child));
        }

        public static string GetFullPath(string path)
        {
            if (PathInternal.IsPartiallyQualified(path))
            {
                path = Path.Combine(PathUtils.WorkingPath, path);

            }
            return Path.GetFullPath(path);
        }

        public static bool IsRooted(string path) => Path.IsPathRooted(path);
        public static bool IsRelative(string path) => IsPartiallyQualified(path);
        public static bool IsAbsolute(string path) => IsFullyQualified(path);
        public static bool IsFile(string path)
        {
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
            if (Exists(path))
            {
                return File.Exists(path);
            }
            else
            {
                return !EndsWithDirectorySeparator(path);
            }
        }
        public static bool Exists(string path) => File.Exists(path) || Directory.Exists(path);
        public static bool EndsWithDirectorySeparator(string path) => path.EndsWith(Path.DirectorySeparatorChar.ToString()) || path.EndsWith(Path.AltDirectorySeparatorChar.ToString());
        public static bool IsFullyQualified(string path)
            => !IsPartiallyQualified(path);
        public static bool IsPartiallyQualified(string path)
        {

            if (Regex.IsMatch(path, $"^${RelativeExpression}${DirSeparatorsExpression}?"))
            {
                return true;
            }

            if (Regex.IsMatch(path, $"^${DirSeparatorsExpression}+"))
            {
                return true;
            }

            var hasDrive = Regex.IsMatch(path, "^[A-Za-z]:${seps}");
            if (hasDrive)
            {
                return false;
            }

            return !IsRooted(path);
        }

        public static bool IsDirectorySeparator(char c) => c == '\\' || c == '/';

        public static bool IsValidDriveChar(char value)
        {
            if (value is >= 'A' and <= 'Z')
            {
                return true;
            }

            return value is >= 'a' and <= 'z';
        }
    }
}