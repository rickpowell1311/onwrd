using System;
using System.Collections.Generic;
using System.Linq;
using Nuke.Common;
using Nuke.Common.IO;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.Tools.Docker.DockerTasks;

class Build : NukeBuild
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode

    public Build()
    {
        /* Docker outputs everything as standard error. Should convert to info will not show docker
         * informational messages as errors. The Nuke docker process will still fail if any 
         * exceptions occur whilst running a container */

        DockerLogger = (o, s) =>
        {
            Serilog.Log.Information(s);
        };
    }

    public static int Main() => Execute<Build>(x => x.GenerateArtifacts);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Parameter("Artifact Directory")]
    string ArtifactDirectory { get; set; }

    Target Initialize => _ => _
        .Executes(() =>
        {
            Console.WriteLine($"{nameof(SourceDirectory)}: {SourceDirectory}");
            Console.WriteLine($"{nameof(ArtifactDirectory)}: {ArtifactDirectory}");
            Console.WriteLine($"{nameof(ProjectDirectories)}: {ProjectDirectories.Select(x => $"\r\n  - {x}").Aggregate((prev, curr) => $"{prev}{curr}")}");
            Console.WriteLine($"{nameof(RootDirectory)}: {RootDirectory}");
            Console.WriteLine($"{nameof(Configuration)}: {Configuration}");
        });

    Target Compile => _ => _
        .DependsOn(Initialize)
        .Executes(() =>
        {
            foreach (var packagableProjectDirectory in PackagableProjectDirectories)
            {
                DotNet("build -c Release", workingDirectory: packagableProjectDirectory);
            }
        });


    Target Test => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            foreach (var testProjectDirectory in TestProjectDirectories)
            {
                DotNet("test -c Debug", workingDirectory: testProjectDirectory);
            }
        });

    Target GenerateArtifacts => _ => _
        .DependsOn(Test)
        .Executes(() =>
        {
            var artifactsDirectory = RepositoryRoot / ArtifactDirectory;
            EnsureCleanDirectory(artifactsDirectory);

            foreach (var artifactProjectDirectory in PackagableProjectDirectories)
            {
                var targetDir = artifactsDirectory / artifactProjectDirectory.Name;

                CopyDirectoryRecursively(artifactProjectDirectory, targetDir);
            }
        });

    static AbsolutePath RepositoryRoot => RootDirectory / "../..";

    static AbsolutePath SourceDirectory => RootDirectory / "../../src";

    static IEnumerable<AbsolutePath> ProjectDirectories => SourceDirectory.GlobDirectories("Onwrd.*");

    static IEnumerable<AbsolutePath> PackagableProjectDirectories => ProjectDirectories
        .Where(x => !TestProjectDirectories.Contains(x));

    static IEnumerable<AbsolutePath> TestProjectDirectories => SourceDirectory.GlobDirectories(TestProjectDirectoryGlob);

    static string TestProjectDirectoryGlob => "Onwrd.*Tests*";
}