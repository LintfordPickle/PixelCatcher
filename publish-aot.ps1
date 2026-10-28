param(
    [string]$Configuration = "Release",
    [string]$OutputDir = "./publish/aot"
)

Write-Host "Building PixelCatcher Native AOT (win-x64)..." -ForegroundColor Cyan

# Check if link.exe is available in PATH or via Visual Studio
$hasLinker = (Get-Command link.exe -ErrorAction SilentlyContinue) -ne $null
if (-not $hasLinker) {
    $vsWhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
    if (Test-Path $vsWhere) {
        $vcDir = & $vsWhere -latest -find **/VC/Tools/MSVC/**/bin/Hostx64/x64
        if ($vcDir) {
            $env:PATH = "$vcDir;$env:PATH"
            $hasLinker = $true
        }
    }
}

if (-not $hasLinker) {
    Write-Warning "Visual Studio C++ build tools (link.exe) were not detected on this machine."
    Write-Warning "Native AOT on Windows requires the 'Desktop development with C++' workload in Visual Studio."
    Write-Warning "You can install it via the Visual Studio Installer, or push to GitHub where GitHub Actions builds AOT automatically."
}

dotnet publish PixelCatcher/PixelCatcher/PixelCatcher.csproj `
    -c $Configuration `
    -f net10.0-windows `
    -r win-x64 `
    -o $OutputDir

if ($LASTEXITCODE -eq 0) {
    # Ensure Sprites are copied to output
    if (Test-Path "PixelCatcher/PixelCatcher/Sprites") {
        Copy-Item -Path "PixelCatcher/PixelCatcher/Sprites" -Destination "$OutputDir/Sprites" -Recurse -Force
    }
    Write-Host "Native AOT build succeeded! Binary located at: $OutputDir" -ForegroundColor Green
} else {
    Write-Error "Native AOT build failed. Check the error output above."
}
