param(
    [string]$UnityEditor = 'C:\Users\IdrZog_MEM\UnityEditors\2022.3.62f2\Editor\Unity.exe',
    [int]$VersionCode = 2
)
$ErrorActionPreference = 'Stop'
$project = 'C:\Users\IdrZog_MEM\MemotopiaAndroidBuild\Project'
$outputFolder = 'C:\Users\IdrZog_MEM\MemotopiaAndroidBuild\Output'
$signingFolder = 'C:\Users\IdrZog_MEM\.android-signing\memotopia'
if (!(Test-Path -LiteralPath $UnityEditor)) { throw 'Unity Editor is not installed yet.' }
if (!(Test-Path -LiteralPath (Join-Path $project 'Assets\Editor\MemotopiaPlayBuild.cs'))) { throw 'Separate project copy is missing.' }
$credentialsFile = Join-Path $signingFolder 'memotopia-upload-credentials.local.txt'
if (!(Test-Path -LiteralPath $credentialsFile)) { throw 'Local signing credentials file is missing.' }
New-Item -ItemType Directory -Path $outputFolder -Force | Out-Null
$output = Join-Path $outputFolder "Memotopia-$VersionCode.aab"
if (Test-Path -LiteralPath $output) { throw 'Output AAB already exists; choose a new version code.' }
$env:MEMOTOPIA_ANDROID_KEYSTORE_PATH = Join-Path $signingFolder 'memotopia-upload.jks'
$env:MEMOTOPIA_CREDENTIALS_FILE = $credentialsFile
$env:MEMOTOPIA_AAB_OUTPUT = $output
$env:MEMOTOPIA_VERSION_CODE = [string]$VersionCode
try {
    $arguments = @('-batchmode','-nographics','-quit','-accept-apiupdate','-buildTarget','Android',
        '-projectPath',('"' + $project + '"'),'-executeMethod','MemotopiaPlayBuild.Build',
        '-logFile',('"' + (Join-Path $outputFolder 'unity-build.log') + '"'))
    $process = Start-Process -FilePath $UnityEditor -ArgumentList $arguments -WindowStyle Hidden -PassThru -Wait
    if ($process.ExitCode -ne 0) { throw "Unity failed with exit code $($process.ExitCode). Inspect Output\unity-build.log." }
    if (!(Test-Path -LiteralPath $output)) { throw 'Unity exited without creating the AAB.' }
    Write-Output "BUILD_OUTPUT=$output"
} finally {
    foreach ($name in @('MEMOTOPIA_ANDROID_KEYSTORE_PATH','MEMOTOPIA_CREDENTIALS_FILE','MEMOTOPIA_AAB_OUTPUT','MEMOTOPIA_VERSION_CODE')) {
        [Environment]::SetEnvironmentVariable($name, $null, 'Process')
    }
}
