using System;

namespace Helveg.Analysis
{
    [Flags]
    public enum Diagnosis
    {
        None = 0,
        Error = 1 << 0,
        Warning = 1 << 1,
    }
}
