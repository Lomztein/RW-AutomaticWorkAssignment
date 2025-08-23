Write-Host "Copy files"
dotnet msbuild -target:CopyModPatch
dotnet msbuild -target:CopyMod
