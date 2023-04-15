using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helveg.Visualization;

public record DocumentInfo(
    string Name,
    DateTimeOffset CreatedOn,
    string HelvegVersion,
    string? Revision
);
