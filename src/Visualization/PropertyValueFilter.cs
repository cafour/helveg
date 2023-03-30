using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helveg.Visualization;

public record PropertyValueFilter(
    string Label,
    string PropertyName,
    string PropertyValue,
    ComparisonKind Comparison = ComparisonKind.Equals
) : INodeFilter
{
    public string Kind => "PropertyValue";
}
