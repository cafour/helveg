﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helveg.CommandLine;

public enum AnalysisScope
{
    None,
    WithoutSymbols,
    PublicApi,
    Explicit,
    All
}
