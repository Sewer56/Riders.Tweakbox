# Project Output Paths
$publishDirectory = "Publish"
$dir = Split-Path $MyInvocation.MyCommand.Path

Write-host "My directory is $dir"
Push-Location $dir

[Environment]::CurrentDirectory = $PWD

function Build
{
    param($BuildModOutputPath, $BuildCsProjPath, $BuildPublishName)

    # Cleaup for Mod Build
    Remove-Item $BuildModOutputPath -Recurse
    New-Item $BuildModOutputPath -ItemType Directory

    # Build
    dotnet restore $BuildCsProjPath
    dotnet clean $BuildCsProjPath
    dotnet publish $BuildCsProjPath -c Release -r win-x86 --self-contained false -o "$BuildModOutputPath/x86" /p:PublishReadyToRun=false
    
    # Remove Redundant Files
    Move-Item -Path "$BuildModOutputPath/x86/Tweakbox" -Destination "$BuildModOutputPath/Tweakbox"
    Move-Item -Path "$BuildModOutputPath/x86/Preview.png" -Destination "$BuildModOutputPath/Preview.png"
    Move-Item -Path "$BuildModOutputPath/x86/ModConfig.json" -Destination "$BuildModOutputPath/ModConfig.json"
    Move-Item -Path "$BuildModOutputPath/x86/ReloadedGithubUpdater.json" -Destination "$BuildModOutputPath"
    Move-Item -Path "$BuildModOutputPath/x86/Assets" -Destination "$BuildModOutputPath"

    # Cleanup Unnecessary Files
    Get-ChildItem $BuildModOutputPath -Include *.exe -Recurse | Remove-Item -Force -Recurse
    Get-ChildItem $BuildModOutputPath -Include *.pdb -Recurse | Remove-Item -Force -Recurse
    Get-ChildItem $BuildModOutputPath -Include *.xml -Recurse | Remove-Item -Force -Recurse

    # Compress
    Add-Type -A System.IO.Compression.FileSystem
    [IO.Compression.ZipFile]::CreateFromDirectory($BuildModOutputPath, "$publishDirectory/$BuildPublishName")

    # Cleanup After Build
    Remove-Item $BuildModOutputPath -Recurse
}

# Clean anything in existing Release directory.
Remove-Item $publishDirectory -Recurse
New-Item $publishDirectory -ItemType Directory

# Build Mods
Build "TempBuild" "Riders.Tweakbox/Riders.Tweakbox.csproj" "Riders.Tweakbox.zip"
Build "TempBuild" "Riders.Tweakbox.CharacterPack.DX/Riders.Tweakbox.CharacterPack.DX.csproj" "Riders.Tweakbox.CharacterPack.DX.zip"
Build "TempBuild" "Riders.Tweakbox.Gearpack/Riders.Tweakbox.Gearpack.csproj" "Riders.Tweakbox.Gearpack.zip"

Pop-Location