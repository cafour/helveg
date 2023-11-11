using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Helveg.Visualization;

namespace Helveg.UI;

public static class UIConst
{
    public const string ExplorerCssResourceName = "helveg-explorer.css";
    public const string DiagramJsResourceName = "helveg-diagram.js";
    public const string ExplorerJsResourceName = "helveg-explorer.js";
    public const string VsIconSetResourceName = "helveg-icons-vs.json";
    public const string NugetIconSetResourceName = "helveg-icons-nuget.json";
    public const string PizzaIconSetResourceName = "helveg-icons-pizza.json";
    public const string DataFileName = "helveg-data.json";
    public static readonly DataModel InvalidDataModel = new DataModel
    {
        Name = Const.Invalid,
        Data = null!
    };
}
