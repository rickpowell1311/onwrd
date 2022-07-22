using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.Execution;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.Docker;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.EnvironmentInfo;
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

    Target Test => _ => _
        .DependsOn(Initialize)
        .Executes(() =>
        {
            /* Ensure there are no clashes between the mounted volumnes used by the test 
            * containers and the source directories by copying them before mounting and testing */
            var containerTestingVolumeDir = RepositoryRoot / "container-testing-volume";

            foreach (var projectDirectory in ProjectDirectories)
            {
                var targetDir = containerTestingVolumeDir / projectDirectory.Name;
                EnsureCleanDirectory(targetDir);
                CopyDirectoryRecursively(projectDirectory, targetDir, DirectoryExistsPolicy.Merge);
            }

            foreach (var testProjectDirectory in containerTestingVolumeDir.GlobDirectories(TestProjectDirectoryGlob))
            {
                Docker("compose up --abort-on-container-exit", workingDirectory: testProjectDirectory);
            }
        });

    Target Compile => _ => _
        .DependsOn(Test)
        .Executes(() =>
        {
            foreach (var packagableProjectDirectory in PackagableProjectDirectories)
            {
                DotNet("build -c Release", workingDirectory: packagableProjectDirectory);
            }
        });

    Target GenerateArtifacts => _ => _
        .DependsOn(Compile)
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

    AbsolutePath RepositoryRoot => RootDirectory / "../..";

    AbsolutePath SourceDirectory => RootDirectory / "../../src";

    IEnumerable<AbsolutePath> ProjectDirectories => SourceDirectory.GlobDirectories("Onwrd.*");

    IEnumerable<AbsolutePath> PackagableProjectDirectories => ProjectDirectories
        .Where(x => !TestProjectDirectories.Contains(x));

    IEnumerable<AbsolutePath> TestProjectDirectories => SourceDirectory.GlobDirectories(TestProjectDirectoryGlob);

    string TestProjectDirectoryGlob => "Onwrd.*Tests*";
}