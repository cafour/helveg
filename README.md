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

* **[Docs](https://helveg.net/)**
* **[Samples](https://helveg.net/samples/)**

<a href="./screenshots/helveg_node_with_metadata.png" title="Node with metadata">
    <img width="24%" src="./screenshots/helveg_node_with_metadata_thumb.jpg" />
</a>
<a href="./screenshots/kafe_data_types.png" title="Kafe.Data types">
    <img width="24%" src="./screenshots/kafe_data_types_thumb.jpg" />
</a>
<a href="./screenshots/kafe_solution.png" title="KAFE solution">
    <img width="24%" src="./screenshots/kafe_solution_thumb.jpg" />
</a>
<a href="./screenshots/helveg_with_errors.png" title="Helveg with errors">
    <img width="24%" src="./screenshots/helveg_with_errors_thumb.jpg" />
</a>


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

**Note:** If you don't have any C# codebase at hand, run Helveg on itself or on one of the simple samples in the `./samples` directory.

Upon completion, Helveg produces a single `.html` file.
This file contains the interactive diagram.
Open it in an up-to-date browser.
For information on how to use the user interface, see `supplementary_meterial.pdf` or the Help panel (denoted by an encircled question mark) within the interface itself.


## Limitations

The visualized codebase must be able to build using `dotnet build`. Codebases relying on .NET Core and .NET 5+ should be fine. However, you may encounter issues when visualizing projects relying on the old .NET Framework, such as old-style ASP.NET projects and WPF applications.


## Building

### Prerequisites

* .NET 9.0 or newer
* Node 22.4.0 or newer
* an up-to-date web browser
* git (optional)

### Steps

To build Helveg from its source code, do the following:

1. Make sure you have installed the prerequisites listed above.
2. Make sure your computer is connected to the internet.
3. Open a command-line or a terminal in the root of the repository.
4. Restore .NET tools:
    ```bash
    dotnet tool restore
    ```
5. Enable `corepack` (Node's manager of package managers):
    ```bash
    corepack enable
    ```
6. Install the `pnpm` package manager:
    ```bash
    corepack prepare pnpm@latest --activate
    ```
7. Install client dependencies: 
    ```bash
    pnpm install
    ```
8. Build client scripts:
    ```bash
    pnpm run build
    ```
9. Pack the .NET solution:
    ```bash
    dotnet pack Helveg.sln -o artifacts -c Release
    ```
10. Make sure no prior version of Helveg is installed:
    ```bash
    dotnet tool uninstall --global helveg
    ```
11. Install the newly built Helveg:
    ```bash
    dotnet tool install --global --add-source "./artifacts" --version "0.0.0-local" helveg
    ```


### About version strings

This repository uses [GitVersion](https://gitversion.net/) to produce NuGet packages with correct semantic version strings.
If you obtained Helveg's code repository without its history (i.e. the `.git` folder), you won't be able to use GitVersion and all packages will have the `0.0.0-local` fallback version.

If the `.git` directory is present and you have `git` installed on your computer, you can generate NuGet packages with correct version strings by following steps 1. - 7. as above and then the following:

8. Set the version string in `package.json` files:
    ```bash
    pnpm run git-version
    ```
9. Build client scripts:
    ```bash
    pnpm run build
    ```
10. Pack the .NET solution:
    ```bash
    dotnet pack Helveg.sln -o artifacts -c Release -p:AllowGitVersion=true
    ```
11. Make sure no prior version of Helveg is installed:
    ```bash
    dotnet tool uninstall --global helveg
    ```
12. Look into the `./artifacts` directory and make note of `<version>` in the filenames.
    For example in `helveg.42.0.0-answer.nupkg`, `<version>` is `42.0.0-answer`.
13. Install the newly built Helveg:
    ```bash
    dotnet tool install --global --add-source "./artifacts" --version "<version>" helveg
    ```


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


## License

* The source code is licensed under the [BSD 3-Clause License](./LICENSE).
* Icons in `./packages/helveg-diagram/icons/vscode` are from the [`vscode-codicons` repository](https://github.com/microsoft/vscode-codicons), were created by Microsoft, and are licensed under the [Creative Commons Attribution 4.0 International Public License](https://creativecommons.org/licenses/by/4.0/legalcode).
* The ingredients used in the _CodePizza_ feature are based on a number of royalty-free images. A complete list can be found in [~/docs/codepizza.md](./codepizza.md).
