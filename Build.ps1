Add-Type -AssemblyName System.IO.Compression.FileSystem
Import-Module BitsTransfer

$url = "https://build.torchapi.net/job/Torch/job/Torch/job/master/lastSuccessfulBuild/artifact/bin/torch-server.zip"
$output = "$PSScriptRoot\torch-server.zip"
$start_time = Get-Date

function Unzip
{
    param([string]$zipfile, [string]$outpath)

    [System.IO.Compression.ZipFile]::ExtractToDirectory($zipfile, $outpath)
}

Start-BitsTransfer -Source $url -Destination $output

Write-Output "Time taken: $((Get-Date).Subtract($start_time).Seconds) second(s)"
Unzip "$PSScriptRoot\torch-server.zip" "$PSScriptRoot\TorchBinaries"
Write-Output "Torch Binaries Extracted."

$url = "https://steamcdn-a.akamaihd.net/client/installer/steamcmd.zip"
$output = "$PSScriptRoot\steamcmd.zip"
$start_time = Get-Date

Start-BitsTransfer -Source $url -Destination $output
Write-Output "Time taken: $((Get-Date).Subtract($start_time).Seconds) second(s)"
Unzip "$PSScriptRoot\steamcmd.zip" "$PSScriptRoot\GameBinaries"
iex "$PSScriptRoot\GameBinaries\steamcmd.exe +login anonymous +force_install_dir . +app_update 298740 +quit"
Write-Output "Game Binaries Extracted."