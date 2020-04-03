///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var stable = Argument("stable", "false");
var apiKey = Argument("apiKey", "");

var solutionPath = "./WeihanLi.EntityFramework.sln";
var srcProjects  = GetFiles("./src/**/*.csproj");
var packProjects = GetFiles("./src/WeihanLi.EntityFramework/*.csproj");

var artifacts = "./artifacts/packages";
var isWindowsAgent = (EnvironmentVariable("Agent_OS") ?? "Windows_NT") == "Windows_NT";
var branchName = EnvironmentVariable("BUILD_SOURCEBRANCHNAME") ?? "local";

void PrintBuildInfo(){
   Information($@"branch:{branchName}, agentOs={EnvironmentVariable("Agent_OS")}
   BuildID:{EnvironmentVariable("BUILD_BUILDID")},BuildNumber:{EnvironmentVariable("BUILD_BUILDNUMBER")},BuildReason:{EnvironmentVariable("BUILD_REASON")}
   ");
}

///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////

Setup(ctx =>
{
   // Executed BEFORE the first task.
   Information("Running tasks...");
   PrintBuildInfo();
});

Teardown(ctx =>
{
   // Executed AFTER the last task.
   Information("Finished running tasks.");
});

///////////////////////////////////////////////////////////////////////////////
// TASKS
///////////////////////////////////////////////////////////////////////////////

Task("clean")
    .Description("Clean")
    .Does(() =>
    {
       var deleteSetting = new DeleteDirectorySettings()
       {
          Force = true,
          Recursive = true
       };
      if (DirectoryExists(artifacts))
      {
         DeleteDirectory(artifacts, deleteSetting);
      }
    });

Task("restore")
    .Description("Restore")
    .Does(() => 
    {
      DotNetCoreRestore(solutionPath);
    });

Task("build")    
    .Description("Build")
    .IsDependentOn("clean")
    .IsDependentOn("restore")
    .Does(() =>
    {
      var buildSetting = new DotNetCoreBuildSettings{
         NoRestore = true,
         Configuration = configuration
      };
      DotNetCoreBuild(solutionPath, buildSetting);
    });

Task("pack")
    .Description("Pack package")
    .IsDependentOn("build")
    .Does(() =>
    {
      var settings = new DotNetCorePackSettings
      {
         Configuration = configuration,
         OutputDirectory = artifacts,
         VersionSuffix = "",
         NoRestore = true,
         NoBuild = true
      };
      if(branchName != "master" && stable != "true"){
         settings.VersionSuffix = $"preview-{DateTime.UtcNow:yyyyMMdd-HHmmss}";
      }
      foreach (var project in packProjects)
      {
         DotNetCorePack(project.FullPath, settings);
      }
      PublishArtifacts();
    });

bool PublishArtifacts(){
   if(!isWindowsAgent){
      return false;
   }
   if(branchName == "master" || branchName == "preview" || !string.IsNullOrEmpty(apiKey))
   {
      var pushSetting =new DotNetCoreNuGetPushSettings
      {
         Source = EnvironmentVariable("Nuget__SourceUrl") ?? "https://api.nuget.org/v3/index.json",
         ApiKey = EnvironmentVariable("Nuget__ApiKey") ?? apiKey
      };
      var packages = GetFiles($"{artifacts}/*.nupkg");
      foreach(var package in packages)
      {
         DotNetCoreNuGetPush(package.FullPath, pushSetting);
      }
      return true;
   }
   return false;
}

Task("Default")
    .IsDependentOn("pack");

RunTarget(target);