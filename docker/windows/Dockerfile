FROM mcr.microsoft.com/windows/servercore:1909

SHELL ["powershell", "-Command"]

WORKDIR c:/

ADD https://aka.ms/vs/16/release/vs_buildtools.exe c:/temp/vs_buildtools.exe

# some Windows SDKs break docker or smth
RUN c:/temp/vs_buildtools.exe --quiet --wait --norestart --nocache \
    --installPath c:/BuildTools \
    --add Microsoft.VisualStudio.Workload.VCTools \
    --add Microsoft.VisualStudio.Component.VC.Tools.x86.x64 \
    --add Microsoft.VisualStudio.Component.Windows10SDK.18362 \
    --remove Microsoft.VisualStudio.Component.Windows10SDK.10240 \
    --remove Microsoft.VisualStudio.Component.Windows10SDK.10586 \
    --remove Microsoft.VisualStudio.Component.Windows10SDK.14393 \
    --remove Microsoft.VisualStudio.Component.Windows81SDK; \
    if ($env:ErrorCode -eq 3010) { exit 1 }

RUN Set-ExecutionPolicy Bypass -Scope Process -Force; \
    $env:ChocolateyUseWindowsCompression='true'; \
    [System.Net.ServicePointManager]::SecurityProtocol = [System.Net.ServicePointManager]::SecurityProtocol -bor 3072; \
    iex ((New-Object System.Net.WebClient).DownloadString('https://chocolatey.org/install.ps1'));

RUN choco install -y cmake ninja git
# chocolatey doesn't put cmake onto Path for some reason
RUN setx /M PATH $($env:Path + ';C:\Program Files\CMake\bin')

# LunarG is not stoked about me downloading the SDK fourty-two times a day.
# ADD https://sdk.lunarg.com/sdk/download/1.2.141.2/windows/VulkanSDK-1.2.141.2-Installer.exe c:/temp/vulkan-sdk.exe
COPY vulkan-sdk.exe c:/temp/vulkan-sdk.exe

RUN Import-Module "c:/ProgramData/chocolatey/helpers/chocolateyInstaller.psm1"; \
    Install-ChocolateyInstallPackage -PackageName "vulkan-sdk" \
        -FileType "exe" \
        -SilentArgs "/S" \
        -File "c:/temp/vulkan-sdk.exe" \
        -ValidExitCodes @(0); \
    rm c:/temp/vulkan-sdk.exe
