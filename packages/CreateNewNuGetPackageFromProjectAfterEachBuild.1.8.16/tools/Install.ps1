param($installPath, $toolsPath, $package, $project)

# Get the current Post-Build Event text.
$postBuildEventText = $project.Properties.Item("PostBuildEvent").Value

###### Start of code to remove older versions of post build event text.
$oldPostBuildEventCode = @'
REM Create a NuGet package for this project and place the .nupkg file in the project's output directory.
ECHO Building NuGet package in Post-Build event...
PowerShell -NoProfile -ExecutionPolicy Bypass -Command "& '$(ProjectDir)PostBuildScripts\BuildNewPackage-RanAutomatically.ps1' -ProjectFilePath '$(ProjectPath)' -OutputDirectory '$(TargetDir)' -Configuration '$(ConfigurationName)' -Platform '$(PlatformName)'"
'@
$postBuildEventText = $postBuildEventText.Replace($oldPostBuildEventCode, [string]::Empty)

$oldPostBuildEventCode = @'
REM Create a NuGet package for this project and place the .nupkg file in the project's output directory.
ECHO Building NuGet package in Post-Build event...
PowerShell -NoProfile -ExecutionPolicy Bypass -Command "& '$(ProjectDir)PostBuildScripts\BuildNewPackage-RanAutomatically.ps1' -ProjectFilePath '$(ProjectPath)' -OutputDirectory '$(TargetDir)' -Configuration='$(ConfigurationName)' -Platform='$(PlatformName)'"
'@
$postBuildEventText = $postBuildEventText.Replace($oldPostBuildEventCode, [string]::Empty)

$oldPostBuildEventCode = @'
REM Create a NuGet package for this project and place the .nupkg file in the project's output directory.
REM If you see this in Visual Studio's Error List window, check the Output window's Build tab for the actual error.
ECHO Building NuGet package in Post-Build event...
PowerShell -NoProfile -ExecutionPolicy Bypass -Command "& '$(ProjectDir)PostBuildScripts\BuildNewPackage-RanAutomatically.ps1' -ProjectFilePath '$(ProjectPath)' -OutputDirectory '$(TargetDir)' -Configuration '$(ConfigurationName)' -Platform '$(PlatformName)'"
'@
$postBuildEventText = $postBuildEventText.Replace($oldPostBuildEventCode, [string]::Empty)
###### End of code to remove older versions of post build event text.

# Define the Post-Build Event Code to add.
$postBuildEventCode = @'
REM Create a NuGet package for this project and place the .nupkg file in the project's output directory.
REM If you see this in Visual Studio's Error List window, check the Output window's Build tab for the actual error.
ECHO Creating NuGet package in Post-Build event...
PowerShell -NoProfile -ExecutionPolicy Bypass -Command "& '$(ProjectDir)_CreateNewNuGetPackage\DoNotModify\CreateNuGetPackage.ps1' -ProjectFilePath '$(ProjectPath)' -OutputDirectory '$(TargetDir)' -BuildConfiguration '$(ConfigurationName)' -BuildPlatform '$(PlatformName)'"
'@

# If there is already a call to the PowerShell script in the post build event, then just exit.
if ($postBuildEventText.Contains($postBuildEventCode))
{
    Write-Verbose "CreateNuGetPackage.ps1 is already referenced in the Post-Build Event, so not updating post-build event code."
	return
}

# Add the Post-Build Event Code to the project (prepend a couple newlines in case there is existing Post Build Event text).
$postBuildEventText += "`n`r`n`r$postBuildEventCode"
$project.Properties.Item("PostBuildEvent").Value = $postBuildEventText.Trim()

# Change the build action of the NuGet.exe file from Content to None.
$nuGetExeFile = $project.ProjectItems.Item("_CreateNewNuGetPackage").ProjectItems.Item("DoNotModify").ProjectItems.Item("NuGet.exe")
$nuGetExeFile.Properties.Item("BuildAction").Value = 0	# Build Action = None

# Save the changes we made.
$project.Save()