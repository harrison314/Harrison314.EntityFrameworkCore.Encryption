#addin nuget:?package=Cake.Git&version=0.21.0

var target = Argument("target", "Default");
var configuration = Argument("Configuration", "Release");
var versionSuffix = Argument<string>("VersionSuffix", null);

//*****************************************************************************
// Constants

string artefacts = "./Artifacts";

void UpdateSettings(DotNetCoreSettings settings)
{
    if (settings.EnvironmentVariables == null)
    {
        settings.EnvironmentVariables = new Dictionary<string, string>();
    }

    var branch = GitBranchCurrent("..");
    settings.EnvironmentVariables.Add("RepositoryBranch", branch.FriendlyName);
    settings.EnvironmentVariables.Add("RepositoryCommit", branch.Tip.Sha);
}

// ****************************************************************************

Task("Clean")
     .Does(()=>
     {
          foreach(var projFile in GetFiles("../src/src/*/*.csproj"))
          {
              var projDirectory = projFile.GetDirectory();
              Information($"Clear {projDirectory}");
              CleanDirectory(projDirectory + Directory("/obj"));
              CleanDirectory(projDirectory + Directory("/bin"));
          }
     
          foreach(var projFile in GetFiles("../src/test/*/*.csproj"))
          {
              var projDirectory = projFile.GetDirectory();
              Information($"Clear {projDirectory}");
              CleanDirectory(projDirectory + Directory("/obj"));
              CleanDirectory(projDirectory + Directory("/bin"));
          }
     
          CleanDirectory(artefacts);
     });

Task("Build-Harrison314EntityFrameworkCoreEncryption")
    .IsDependentOn("Clean")
    .Does(() =>
    {
        DotNetCorePackSettings settings = new  DotNetCorePackSettings()
        {
            Configuration = configuration,
            OutputDirectory = artefacts,
            IncludeSource = false,
            IncludeSymbols = false,
            NoBuild = false,
            VersionSuffix = versionSuffix
        };

        UpdateSettings(settings);
        DotNetCorePack("../src/src/Harrison314.EntityFrameworkCore.Encryption/Harrison314.EntityFrameworkCore.Encryption.csproj", settings);
    });

// ****************************************************************************

Task("Test-Harrison314EntityFrameworkCoreEncryption")
    .IsDependentOn("Clean")
    .Does(() =>
    {
        DotNetCoreTestSettings settings = new  DotNetCoreTestSettings()
        {
            Configuration = configuration
        };

        DotNetCoreTest("../src/test/Harrison314.EntityFrameworkCore.Encryption.Tests/Harrison314.EntityFrameworkCore.Encryption.Tests.csproj", settings);
    });


// ****************************************************************************

Task("Test")
    .IsDependentOn("Test-Harrison314EntityFrameworkCoreEncryption");

Task("Build")
    .IsDependentOn("Build-Harrison314EntityFrameworkCoreEncryption");

Task("Default")
    .IsDependentOn("Test")
    .IsDependentOn("Build");

//*****************************************************************************

RunTarget(target);