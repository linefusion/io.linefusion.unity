using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

using Linefusion.Generator.Paths;

namespace Linefusion.Generator.IO
{
    public class PathValue
    {
        private string value;

        public bool Exists
        {
            get
            {
                return PathUtils.Exists(Value);
            }
        }

        public bool IsFile
        {
            get
            {
                return PathUtils.IsFile(Value);
            }
        }

        public bool IsDirectory
        {
            get
            {
                return PathUtils.IsDirectory(Value);
            }
        }

        public bool IsAbsolute
        {
            get
            {
                return Regex.IsMatch(value, @"^[a-zA-Z]:[\\/]") || value.StartsWith("/");
            }
        }

        public bool IsRelative
        {
            get
            {
                return !IsAbsolute;
            }
        }

        public bool IsFullyQualified
        {
            get
            {
                return PathUtils.IsFullyQualified(Value);
            }
        }

        public bool IsPartiallyQualified
        {
            get
            {
                return PathUtils.IsPartiallyQualified(Value);
            }
        }

        public bool IsRooted
        {
            get
            {
                return PathUtils.IsRooted(Value);
            }
        }

        public virtual string Value
        {
            get
            {
                return value;
            }
        }

        public PathValue Parent
        {
            get
            {
                return new PathValue(Path.GetDirectoryName(Value));
            }
        }

        public PathValue(string value)
        {
            this.value = value.Replace("\\", "/");
        }

        public PathValue Absolute
        {
            get
            {
                if (this.IsAbsolute)
                {
                    return this;
                }
                return new PathValue(Path.Combine(PathUtils.WorkingPath, Value));
            }
        }

        public PathValue RelativeTo(PathValue other)
        {
            var source = FullPath.FromPath(this.Absolute);
            var target = FullPath.FromPath(other.Absolute);
            
            return source.MakePathRelativeTo(target);
        }

        public static implicit operator string(PathValue path) => path.Value;
        public static implicit operator PathValue(string path) => new(path);
        public static implicit operator PathValue(string[] path) => new(Path.Combine(path));

        public override string ToString()
        {
            return this.Value;
        }
    }

}