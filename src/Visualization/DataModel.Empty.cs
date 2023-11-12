using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Helveg.Visualization;

public partial class DataModel
{
    public static DataModel Create(string name)
    {
        return new()
        {
            Name = name,
            CreatedOn = DateTimeOffset.UtcNow,
            Analyzer = new() {
                Name = Assembly.GetEntryAssembly()?.GetName().Name
                    ?? Process.GetCurrentProcess().ProcessName,
                Version = GitVersionInformation.SemVer
            }
        };
    }

    public static DataModel CreateEmpty()
    {
        return Create(Const.Empty);
    }

    public static DataModel CreateInvalid(
        string? name = null,
        string? rootNodeName = null,
        IEnumerable<MultigraphDiagnostic>? diagnostics = null)
    {
        name ??= Const.Invalid;
        rootNodeName ??= name;
        var model = Create(name);
        if (diagnostics is not null)
        {
            model.Data = new()
            {
                Nodes = {
                [rootNodeName] = {
                    Name = rootNodeName,
                    Diagnostics = diagnostics?.ToList() ?? new()
                }
            }
            };
        }
        return model;
    }

    public static DataModel CreateInvalid(
        string? name = null,
        string? rootNodeName = null,
        params MultigraphDiagnostic[] diagnostics)
    {
        return CreateInvalid(name, rootNodeName, diagnostics.AsEnumerable());
    }
}
