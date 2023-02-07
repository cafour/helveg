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
public interface IInvalidable<T>
    where T : IInvalidable<T>, new()
{
    public abstract static T Invalid { get; }
}
