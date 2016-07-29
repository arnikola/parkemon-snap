# Configuration
param (
    [string]$StorageConnectionString = "ADDME",
    [string]$DeploymentId = "624f004bf80f4195a12692df4915c797",
    [string]$Configuration = "Debug",
    [string]$SolutionDirectory = $(Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Definition))
)

pushd $SolutionDirectory
try {
    $BuildCmd = $([System.IO.Path]::Combine($SolutionDirectory, "build.cmd"));
    & $BuildCmd $Configuration
    $DeployToolExe = [System.IO.Path]::Combine($SolutionDirectory, "Deploy", "bin", $Configuration, "Deploy.exe");
    $Applications = @(
        @{Id = "orleans"; SourceDirectory = [System.IO.Path]::Combine($SolutionDirectory, "OrleansSilo", "bin", $Configuration);},
        @{Id = "web"; SourceDirectory = [System.IO.Path]::Combine($SolutionDirectory, "Web", "bin", $Configuration);},
        @{Id = "logretriever"; SourceDirectory = [System.IO.Path]::Combine($SolutionDirectory, "LogRetriever", "bin", $Configuration);}
    );

    $Applications | ConvertTo-Json | & $DeployToolExe -StdIn -ConnectionString $StorageConnectionString -DeploymentId $DeploymentId
} finally {
    popd
}