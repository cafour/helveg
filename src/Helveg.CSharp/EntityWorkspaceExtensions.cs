using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helveg.CSharp;

public static class EntityWorkspaceExtensions
{
    public static ProjectDefinition Resolve(this EntityWorkspace workspace, ProjectReference reference)
    {
        return (ProjectDefinition)workspace.Resolve(reference);
    }
}
