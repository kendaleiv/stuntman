#tool "nuget:?package=xunit.runner.console"

var target = Argument("target", "Default");

string configuration = "Release";

var artifactsDir = Directory("./artifacts");
var solution = "./Stuntman.sln";

Task("Clean")
    .Does(() =>
    {
        CleanDirectory(artifactsDir);
    });

Task("Restore-NuGet-Packages")
    .IsDependentOn("Clean")
    .Does(() =>
    {
        NuGetRestore(solution);
        // DotNetCoreRestore();
    });

Task("Build")
    .IsDependentOn("Restore-NuGet-Packages")
    .Does(() =>
    {
        MSBuild(solution, settings =>
            settings.SetConfiguration(configuration)
                .WithProperty("TreatWarningsAsErrors", "True")
                .SetVerbosity(Verbosity.Minimal)
                .AddFileLogger());
    });

Task("Run-Tests")
    .IsDependentOn("Build")
    .Does(() =>
    {
        XUnit2("./tests/**/bin/" + configuration + "/*.Tests.dll");
    });

Task("Package")
    .IsDependentOn("Run-Tests")
    .Does(() =>
    {
        NuGetPack("./src/Core/Core.csproj", new NuGetPackSettings
        {
            OutputDirectory = artifactsDir,
            Properties = new Dictionary<string, string>
            {
                { "Configuration", configuration }
            }
        });

        DotNetCorePack("./src/RimDev.Stuntman.AspNetCore/RimDev.Stuntman.AspNetCore.csproj", new DotNetCorePackSettings
        {
            OutputDirectory = artifactsDir,
            MSBuildSettings = new DotNetCoreMSBuildSettings().WithProperty("Configuration", configuration)
        });
    });

Task("Default")
    .IsDependentOn("Package");

RunTarget(target);
