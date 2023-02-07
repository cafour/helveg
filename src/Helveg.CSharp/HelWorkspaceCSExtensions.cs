using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helveg.CSharp;

public static class HelWorkspaceCSExtensions
{
    public static HelProjectCS Resolve(this HelWorkspaceCS workspace, HelProjectReferenceCS reference)
    {
        return (HelProjectCS)workspace.Resolve(reference);
    }
}
