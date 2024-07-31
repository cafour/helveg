<img src="./helveg.png" width="128px" />

# Helveg

<a href="https://nuget.org">
    <img src="https://img.shields.io/nuget/v/helveg?color=005aa7&label=NuGet.org&logo=nuget&style=flat-square" />
</a>

<a href="https://gitlab.com/helveg/helveg/-/packages">
    <img src="https://img.shields.io/badge/GitLab-Preview_Packages-e24329?logo=gitlab&style=flat-square" />
</a>

<a href="./LICENSE">
    <img src="https://img.shields.io/gitlab/license/helveg/helveg?style=flat-square&label=License" />
</a>

A prototype of a tool and an extensible API for the visualization of C# codebases.

* **[Docs](https://helveg.net/docs/)**
* **[Samples](https://helveg.net/samples/)**

## Installing

```bash
dotnet tool install --global helveg
```

## Usage

To produce a diagram of a codebase, navigate into a directory with a Visual Studio Solution file or a MSBuild C# project file and run:

```bash
helveg
```

Alternatively, to produce a diagram of a solution in `C:\dev\my-code\MyCode.sln`, run:

```bash
helveg 'C:\dev\my-code\MyCode.sln'
```

For a complete list of command-line options, see:

```bash
helveg --help
```


## Limitations

The visualized codebase must be able to build using `dotnet build`. Codebases relying on .NET Core and .NET 5+ should be fine. However, you may encounter issues when visualizing projects relying on the old .NET Framework, such as old-style ASP.NET projects and WPF applications.


## Building

### Prerequisites

* .NET 6.0 or newer
* Node 22.4.0 or newer
* an up-to-date web browser

### Steps

To build Helveg from its source code, do the following:

1. Make sure you have installed the prerequisites listed above.
2. Make sure your computer is connected to the internet.
3. Open a command-line or a terminal in the root of the repository.
4. Restore .NET tools by running `dotnet tool restore`.
5. Enable corepack by running `corepack enable`.
6. Install the `pnpm` package manager by running `corepack prepare pnpm@latest --activate`.
7. Install client dependencies: `pnpm install`.
8. Build client scripts: `pnpm run build`.
9. Pack the .NET solution: `dotnet pack Helveg.sln -o artifacts -c Release`.
10. Make sure no prior version of Helveg is installed: `dotnet tool uninstall helveg`.
11. Install the newly built Helveg: `dotnet tool install --add-source ./artifacts helveg`.


## Dependencies

This software has many dependencies. You do not need to install these explicitly. See Build Instructions above. These are the most significant dependencies:

**Back-end**

* [MSBuild](https://github.com/dotnet/msbuild)
* [Roslyn](https://github.com/dotnet/roslyn/)
* [NuGet](https://nuget.org/)

**Front-end**

* [Sigma.js](https://github.com/jacomyal/sigma.js)
* [Graphology](https://github.com/graphology/graphology)
* [Svelte](https://svelte.dev/)
* [Visual Studio Image Library](https://www.microsoft.com/en-us/download/details.aspx?id=35825)


## License

* The source code is licensed under the [BSD 3-Clause License](./LICENSE).
* Visual Studio Icons in `./packages/helveg-diagram/icons/vs` are licensed under the [Visual Studio Image Library License](./packages/helveg-diagram/icons/vs/VS_LICENSE.rtf), allowing their use in applications.
* The NuGet icon in `./packages/helveg-diagram/icons/nuget/NuGet.svg` is licensed under the [CC-BY License](https://creativecommons.org/licenses/by/2.0/). Also see https://github.com/NuGet/Media.
* The ingredients used in the _CodePizza_ feature are based on a number of royalty-free images. A complete list can be found in [~/docs/codepizza.md](./codepizza.md).
