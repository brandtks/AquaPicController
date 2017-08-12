#NUGET="mono /path/to/nuget.exe"
NUGET="nuget"

# This variable isn't required, just shortens the execution time of the build script
# The script will try to find MSBuild and if it can't find it, it will download
# MSBuild using NuGet
#MSBUILD=""
MSBUILD="msbuild"
