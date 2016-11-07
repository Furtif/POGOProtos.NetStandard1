#addin nuget:?package=Cake.Git

var target = Argument("target", "Default");

Task("Clean").Does(() => {
  if (DirectoryExists("POGOProtos")) {
    DeleteDirectory("POGOProtos", recursive:true);
  }
});

Task("POGOProtos-Tools").Does(() => {
    NuGetInstall("Google.Protobuf.Tools", new NuGetInstallSettings {
        ExcludeVersion = true,
        OutputDirectory = "./tools",
        Version = "3.1.0"
    });
});

Task("POGOProtos-Clone").Does(() => {
  GitClone("https://github.com/AeonLucid/POGOProtos.git", new DirectoryPath("POGOProtos"));
});

Task("POGOProtos-Compile").Does(() => {
  StartProcess("C:/Python27/python.exe", new ProcessSettings()
        .WithArguments(args => 
            args.AppendQuoted(System.IO.Path.GetFullPath("./POGOProtos/compile.py"))
                .Append("-p")
                .AppendQuoted(System.IO.Path.GetFullPath("./tools/Google.Protobuf.Tools/tools/windows_x64/protoc.exe"))
                .Append("-o")
                .AppendQuoted(System.IO.Path.GetFullPath("./POGOProtos/out"))
                .Append("csharp")));
});

Task("POGOProtos-Move").Does(() => {
  CopyDirectory("./POGOProtos/out/POGOProtos", "./src/POGOProtos.NetStandard1");
});

Task("Version").Does(() =>
{
  var version = System.IO.File.ReadAllText("./POGOProtos/.current-version");
  var updatedProjectJson = System.IO.File.ReadAllText("./src/POGOProtos.NetStandard1/project.json").Replace("1.0.0-*", version);

  System.IO.File.WriteAllText("./src/POGOProtos.NetStandard1/project.json", updatedProjectJson);
});

Task("Default")
  .IsDependentOn("Clean")
  .IsDependentOn("POGOProtos-Tools")
  .IsDependentOn("POGOProtos-Clone")
  .IsDependentOn("POGOProtos-Compile")
  .IsDependentOn("POGOProtos-Move")
  .IsDependentOn("Version")
  .Does(() =>
{
  DotNetCoreRestore("./src/POGOProtos.NetStandard1");

  var buildSettings = new DotNetCorePackSettings();
  buildSettings.Configuration = "Release";

  DotNetCorePack("./src/POGOProtos.NetStandard1", buildSettings);
});

RunTarget(target);