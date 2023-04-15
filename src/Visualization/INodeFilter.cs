using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helveg.Visualization;

public interface INodeFilter
{
    string Label { get; }
    string Kind { get; }
}
