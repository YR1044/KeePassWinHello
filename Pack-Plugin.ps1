param (
    [string] $ProjectDir = $null,
    [string] $OutputFileNameBase = 'KeePassWinHelloPlugin',
    [string] $OutputDir = $null,
    [string] $Version = $null,
    [string] $ReleaseNotes = '[TBD]'
)
$keePassExe = 'C:\Program Files (x86)\KeePass Password Safe 2\KeePass.exe'
$versionPattern = '[\d\.]+(?:\-\w+)?'

if (!$PSScriptRoot) {
    $PSScriptRoot = Split-Path -Parent -Path $MyInvocation.MyCommand.Definition
}
if (!$ProjectDir) {
    $ProjectDir = $PSScriptRoot
}
if (!$OutputDir) {
    $OutputDir = "$ProjectDir\out"
}
if (!$Version) {
    $Version = (Select-String -Pattern "(?<=\:)$versionPattern" -Path ".\keepass.version").Matches[0].Value
}

$sources = "$ProjectDir\src"
$tempDirName = $OutputFileNameBase
$packingSourcesFolder = "$OutputDir\$tempDirName"
if (Test-Path $packingSourcesFolder) {
    Remove-Item $packingSourcesFolder -Force -Recurse
}
New-Item $packingSourcesFolder -Type Directory > $null


$excludedItems = '.*', '*.sln', 'bin', 'obj', '*.user', '*.ps1', '*.plgx', '*.md', $tempDirName, 'Screenshots'
Get-ChildItem $sources | 
    Where-Object   { $i=$_; $res=$true; $excludedItems | ForEach-Object { $i -notlike $_ } | ForEach-Object { $res = $_ -and $res }; $res } |
    ForEach-Object { Copy-Item $_.FullName $packingSourcesFolder -Recurse }

try {
    Push-Location
    Set-Location $OutputDir
    Start-Process $keePassExe -arg '--plgx-create',"`"$packingSourcesFolder`"" -Wait
} finally {
    Pop-Location
}

Remove-Item $packingSourcesFolder -Force -Recurse

$outputFile = "$OutputDir\$OutputFileNameBase.plgx"
$chocoDir = "$ProjectDir\Chocolatey"
$chocoInstallScriptFile = "$chocoDir\tools\ChocolateyInstall.ps1"
$hash = (Get-FileHash $outputFile -Algorithm SHA256).Hash
(Get-Content $chocoInstallScriptFile) `
    -replace "\`$version\s*\=\s*['`"]$versionPattern['`"]", "`$version = '$Version'" `
    -replace "\`$checksum\s*\=\s*['`"][\w\d]+['`"]", "`$checksum = '$hash'" `
    | Set-Content $chocoInstallScriptFile

& choco pack "`"$chocoDir\keepass-plugin-winhello.nuspec`"" --version $Version --out `"$OutputDir`" ReleaseNotes=`"$ReleaseNotes`"