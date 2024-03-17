using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Linefusion.Generator.IO
{
    public class SafePath
    {
        private readonly string value;

        public bool Exists
        {
            get { return SafeIO.Exists(Value); }
        }

        public bool IsFile
        {
            get { return SafeIO.IsFile(Value); }
        }

        public bool IsDirectory
        {
            get { return SafeIO.IsDirectory(Value); }
        }

        public bool IsAbsolute
        {
            get { return SafeIO.IsAbsolute(Value); }
        }

        public bool IsRelative
        {
            get { return !IsAbsolute; }
        }

        public bool IsFullyQualified
        {
            get { return SafeIO.IsFullyQualified(Value); }
        }

        public bool IsPartiallyQualified
        {
            get { return SafeIO.IsPartiallyQualified(Value); }
        }

        public bool IsRooted
        {
            get { return SafeIO.IsRooted(Value); }
        }

        public virtual string Value
        {
            get { return value; }
        }

        public virtual string ValueWithoutEndingSeparator
        {
            get { return value.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar); }
        }

        public SafePath Parent
        {
            get { return new SafePath(Path.GetDirectoryName(Value)); }
        }

        public SafePath(string value)
        {
            this.value = SafeIO.Normalize(value);
        }

        public SafePath Absolute
        {
            get
            {
                if (this.IsAbsolute)
                {
                    return this;
                }
                return new SafePath(SafeIO.Resolve(this));
            }
        }

        public SafePath RelativeTo(SafePath other)
        {
            return SafeIO.GetRelativePath(other, this);
        }

        public static implicit operator string(SafePath path) => path.Value;

        public static implicit operator SafePath(string path) => new(path);

        public static implicit operator SafePath(string[] path) => new(Path.Combine(path));

        public override string ToString()
        {
            return this.Value;
        }
    }
}
