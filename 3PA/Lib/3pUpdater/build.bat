@echo off
REM Builds the 2 .exe

REM You need msbuild in your path "C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\msbuild.exe"

msbuild 3pUpdater\3pUpdater.csproj /p:Configuration=Release /p:Platform=AnyCPU /t:Rebuild /verbosity:minimal /p:AdminManifest=true
msbuild 3pUpdater\3pUpdater.csproj /p:Configuration=Release /p:Platform=AnyCPU /t:Rebuild /verbosity:minimal /p:AdminManifest=false
