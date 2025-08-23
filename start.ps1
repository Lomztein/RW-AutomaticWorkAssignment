param(
    [Parameter(Position=0,mandatory=$true)][String] $SteamAppsPath
)

$PID_FILE='.rw.pid'
if(Test-Path $PID_FILE -PathType Leaf){
    Write-Host "Kill previous instance of RimWorld"
    $prevPid = Get-Content $PID_FILE
    $process = Get-Process -Id $prevPid
    if($process && $process.Name -ilike "RimWorld*"){
        Stop-Process $prevPid
    }
    Remove-Item $PID_FILE
}

. copy.ps1

Write-Host "Starting RimWorld"
$rwProcess = Start-Process "$SteamAppsPath\common\RimWorld\RimWorldWin64.exe" -passthru
Write-Output $rwProcess.Id | Out-File $PID_FILE
