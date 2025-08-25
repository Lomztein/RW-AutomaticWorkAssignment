param(
    [Parameter(Position=0,mandatory=$true)][String] $SteamAppsPath,
    [Parameter(Position=1,mandatory=$true)][String] $Configuration
)

$PID_FILE='.rw.pid'
if(Test-Path $PID_FILE -PathType Leaf){
    Write-Host "Kill previous instance of RimWorld"
    $prevPid = Get-Content $PID_FILE
    $process = Get-Process -Id $prevPid 2>$null
    if($process && $process.Name -ilike "RimWorld*"){
        Stop-Process $prevPid
    }
    Remove-Item $PID_FILE
}

. .\copy.ps1 $Configuration

Write-Host "Starting RimWorld"
$rwProcess = Start-Process "$SteamAppsPath\common\RimWorld\RimWorldWin64.exe" -passthru
Write-Output $rwProcess.Id | Out-File $PID_FILE
