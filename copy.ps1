param(
    [Parameter(Position=0,mandatory=$true)][String] $Configuration
)

Write-Host "Copy files"
dotnet msbuild -property:Configuration=$Configuration -target:CopyMod
dotnet msbuild -property:Configuration=$Configuration -target:CopyModPatch
