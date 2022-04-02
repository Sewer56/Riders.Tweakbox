
# Set Working Directory
Split-Path $MyInvocation.MyCommand.Path | Push-Location
[Environment]::CurrentDirectory = $PWD

./Publish.ps1 -ProjectPath "Riders.Tweakbox/Riders.Tweakbox.csproj" `
              -PackageName "Riders.Tweakbox" `
              -PublishOutputDir "Publish/ToUpload/Tweakbox" `
              -MetadataFileName "Riders.Tweakbox.ReleaseMetadata.json" `

./Publish.ps1 -ProjectPath "Riders.Tweakbox.CharacterPack.DX/Riders.Tweakbox.CharacterPack.DX.csproj" `
              -PackageName "Riders.Tweakbox.CharacterPack.DX" `
              -PublishOutputDir "Publish/ToUpload/CharactersDX" `
              -MetadataFileName "Riders.Tweakbox.CharacterPack.DX.ReleaseMetadata.json" `

./Publish.ps1 -ProjectPath "Riders.Tweakbox.Gearpack/Riders.Tweakbox.Gearpack.csproj" `
              -PackageName "Riders.Tweakbox.Gearpack" `
              -PublishOutputDir "Publish/ToUpload/GearPack" `
              -MetadataFileName "Riders.Tweakbox.Gearpack.ReleaseMetadata.json" `

Pop-Location