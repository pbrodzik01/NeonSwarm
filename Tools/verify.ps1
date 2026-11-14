param(
    [string]$Unity = "A:\Unity\2022.3.62f3\Editor\Unity.exe",
    [switch]$SkipGenerate,
    [switch]$SkipTests,
    [switch]$SkipBuild
)

$project = Split-Path -Parent $PSScriptRoot
$logs = Join-Path $project "Logs"
New-Item -ItemType Directory -Force $logs | Out-Null

function Invoke-Unity([string[]]$Arguments, [string]$Log) {
    $all = @('-batchmode', '-projectPath', $project, '-buildTarget', 'Android', '-logFile', (Join-Path $logs $Log)) + $Arguments
    $process = Start-Process -FilePath $Unity -ArgumentList $all -Wait -PassThru -NoNewWindow
    return $process.ExitCode
}

function Show-Results([string]$File) {
    [xml]$xml = Get-Content $File
    $run = $xml.'test-run'
    "{0}: total={1} passed={2} failed={3}" -f (Split-Path $File -Leaf), $run.total, $run.passed, $run.failed
    $xml.SelectNodes("//test-case[@result='Failed']") | ForEach-Object { "  FAILED {0}: {1}" -f $_.name, $_.failure.message.InnerText.Trim() }
}

if (-not $SkipGenerate) {
    $code = Invoke-Unity @('-executeMethod', 'NeonSwarm.EditorTools.ProjectBuilder.BuildAllBatch') 'generate.log'
    "generate: exit $code"
    if ($code -ne 0) { exit $code }
}

if (-not $SkipTests) {
    foreach ($platform in 'EditMode', 'PlayMode') {
        $results = Join-Path $logs ("{0}-results.xml" -f $platform.ToLower())
        Remove-Item $results -ErrorAction SilentlyContinue
        $code = Invoke-Unity @('-runTests', '-testPlatform', $platform, '-testResults', $results) ("{0}.log" -f $platform.ToLower())
        if (Test-Path $results) { Show-Results $results } else { "${platform}: no results (exit $code)" }
    }
}

if (-not $SkipBuild) {
    $code = Invoke-Unity @('-executeMethod', 'NeonSwarm.EditorTools.AndroidBuilder.BuildApkBatch') 'build.log'
    "build: exit $code"
    Select-String -Path (Join-Path $logs 'build.log') -Pattern '\[NeonSwarm\] Build' | ForEach-Object { $_.Line }
}
