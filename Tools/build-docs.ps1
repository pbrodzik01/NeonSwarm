param(
    [string]$Edge = "C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe"
)

$docs = Join-Path (Split-Path -Parent $PSScriptRoot) "Docs"
$src = Join-Path $docs "src"
$html = Join-Path $docs "Dokumentacja.html"
$pdf = Join-Path $docs "Dokumentacja_NeonSwarm.pdf"

$parts = Get-ChildItem $src -Filter *.html | Sort-Object Name | ForEach-Object { [IO.File]::ReadAllText($_.FullName) }
$content = ($parts -join "`n").Replace('src="../shots/', 'src="shots/').Replace('src="../Assets/', 'src="../Assets/')
[IO.File]::WriteAllText($html, $content, (New-Object Text.UTF8Encoding $false))
"HTML: $html"

if (-not (Test-Path $Edge)) { $Edge = "C:\Program Files\Microsoft\Edge\Application\msedge.exe" }
$uri = ([Uri]$html).AbsoluteUri
$profile = Join-Path $env:TEMP "neonswarm-edge-profile"
& $Edge --headless=new --disable-gpu --no-pdf-header-footer --user-data-dir="$profile" --print-to-pdf="$pdf" $uri | Out-Null
Start-Sleep -Seconds 2
if (Test-Path $pdf) { "PDF: $pdf ({0:0.0} MB)" -f ((Get-Item $pdf).Length / 1MB) } else { "PDF not created" }
