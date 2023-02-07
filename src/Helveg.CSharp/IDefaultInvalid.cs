using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helveg.CSharp;

/// <summary>
/// Used for types that guaratee that their default instance is Invalid.
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IDefaultInvalid<T>
    where T : IDefaultInvalid<T>, new()
{
    public static T Invalid { get; } = new();
}
