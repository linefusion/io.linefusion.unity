using System;

namespace Linefusion.Generator
{
    [Flags]
    [Serializable]
    public enum TemplateType
    {
        Auto = 0,
        Include = 1,
        Generator = 2,
    }
}
