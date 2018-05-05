@if not defined _echo echo off
setlocal enabledelayedexpansion

echo msbuild binlog viewer : http://msbuildlog.com/

set "EXTRAPARAMS=/nologo /bl:3PA\bin\msbuild.binlog"

@REM Determine if MSBuild is already in the PATH
for /f "usebackq delims=" %%I in (`where msbuild.exe 2^>nul`) do (
    "%%I" %EXTRAPARAMS% %*
    exit /b !ERRORLEVEL!
)

@REM Find the latest MSBuild that supports our projects
for /f "usebackq delims=" %%I in ('"%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe" -version "[15.0,)" -latest -prerelease -products * -requires Microsoft.Component.MSBuild Microsoft.VisualStudio.Component.Roslyn.Compiler Microsoft.VisualStudio.Component.VC.140 -property InstallationPath') do (
    for /f "usebackq delims=" %%J in (`where /r "%%I\MSBuild" msbuild.exe 2^>nul ^| sort /r`) do (
        "%%J" %* %EXTRAPARAMS% 
        exit /b !ERRORLEVEL!
    )
)

echo Could not find msbuild.exe 1>&2
exit /b 2