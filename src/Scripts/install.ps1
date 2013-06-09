param($installPath, $toolsPath, $package, $project)

. (Join-Path $toolsPath "GetNativeBuildCmd.ps1")

# Get the current Post Build Event cmd
$currentPostBuildCmd = $project.Properties.Item("PostBuildEvent").Value

# Append our post build command if it's not already there
if (!$currentPostBuildCmd.Contains($SoundfingerprintingPostBuildCmd)) {
    $project.Properties.Item("PostBuildEvent").Value += $SoundfingerprintingPostBuildCmd
}