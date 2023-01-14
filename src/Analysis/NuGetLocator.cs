using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;

namespace Helveg.Analysis
{
    /// <summary>
    /// Hack around https://github.com/microsoft/MSBuildLocator/issues/86 based on
    /// https://github.com/microsoft/MSBuildLocator/pull/93.
    /// </summary>
    public static class NuGetLocator
    {
        private const string NuGetPublicKeyToken = "31bf3856ad364e35";

        private static readonly string[] NuGetAssemblyNames =
        {
            "NuGet.Common",
            "NuGet.Frameworks",
            "NuGet.Packaging",
            "NuGet.ProjectModel",
            "NuGet.Versioning"
        };

        public static void Register(string msbuildPath)
        {
            AssemblyLoadContext.Default.Resolving += (context, name) =>
            {
                if (!NuGetAssemblyNames.Contains(name.Name, StringComparer.OrdinalIgnoreCase)
                    || !HasPublicKeyToken(name, NuGetPublicKeyToken))
                {
                    return null;
                }

                var targetAssembly = Path.Combine(msbuildPath, name.Name + ".dll");
                if (!File.Exists(targetAssembly))
                {
                    return null;
                }
                return Assembly.LoadFrom(targetAssembly);
            };
            AssemblyLoadContext.Default.Resolving += (context, name) =>
            {
                var targetAssembly = Path.Combine(msbuildPath, name.Name + ".dll");
                if (!File.Exists(targetAssembly))
                {
                    return null;
                }
                return Assembly.LoadFrom(targetAssembly);
            };
        }

        private static bool HasPublicKeyToken(AssemblyName assemblyName, string expectedPublicKeyToken)
        {
            var publicKeyToken = assemblyName.GetPublicKeyToken();
            if (publicKeyToken is null || publicKeyToken.Length == 0)
            {
                return false;
            }

            var sb = new StringBuilder();
            foreach (var b in publicKeyToken)
            {
                sb.Append($"{b:x2}");
            }

            return sb.ToString().Equals(expectedPublicKeyToken, StringComparison.OrdinalIgnoreCase);
        }
    }
}
