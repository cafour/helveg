using Helveg.CSharp.Symbols;
using Helveg.Visualization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helveg.CSharp;

public static class VisualizationModelBuilderExtensions
{
    public static VisualizationModelBuilder UseCSharp(this VisualizationModelBuilder vmb)
    {
        vmb.AddNodeFilter("IsNotImplicitlyDeclared", new PropertyValueFilter(
            "IsNotImplicitlyDeclared",
            nameof(IMemberDefinition.IsImplicitlyDeclared),
            false.ToString()));
        vmb.AddNodeFilter("CanBeReferencedByName", new PropertyValueFilter(
            "CanBeReferencedByName",
            nameof(IMemberDefinition.CanBeReferencedByName),
            true.ToString()));
        return vmb;
    }
}
