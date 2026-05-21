[CmdletBinding()]
param (
    [Parameter(Mandatory = $true)]
    [string]$Version
)

$ErrorActionPreference = "Stop"

Write-Host "=== Starting release process: v$Version ===" -ForegroundColor Cyan

# Validate version format
if ($Version -notmatch '^\d+\.\d+\.\d+\.\d+$') {
    Write-Error "Version number must be in X.X.X.X format."
}

# Locate ISCC
$IsccPath = "C:\Program Files (x86)\Inno Setup 6\ISCC.exe"
if (-not (Test-Path $IsccPath)) {
    Write-Error "Inno Setup Compiler (ISCC.exe) not found at: $IsccPath"
}

# 1. dotnet build
Write-Host "1. Building Release package..." -ForegroundColor Green
dotnet build -c Release ImageCropper/ImageCropper.csproj
if ($LASTEXITCODE -ne 0) {
    Write-Error "dotnet build failed."
}

# 2. Inno Setup compile
Write-Host "2. Compiling installer with Inno Setup..." -ForegroundColor Green
& $IsccPath installer/ImageCropperSetup.iss
if ($LASTEXITCODE -ne 0) {
    Write-Error "Inno Setup compilation failed."
}

# Confirm setup file exists
$SetupFile = "installer/output/ImageCropperSetup_$Version.exe"
if (-not (Test-Path $SetupFile)) {
    Write-Error "Generated setup file not found: $SetupFile"
}

# 3. Git commit & push
Write-Host "3. Committing and pushing changes to Git..." -ForegroundColor Green

$gitStatus = git status --porcelain
if ([string]::IsNullOrWhiteSpace($gitStatus)) {
    Write-Host "No changes to commit in Git. Skipping commit." -ForegroundColor Yellow
} else {
    git add .
    git commit -m "bump: update version to $Version and update release notes"
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Git commit failed."
    }
    
    git push origin main
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Git push failed."
    }
}

# 4. GitHub Release creation
Write-Host "4. Creating GitHub Release..." -ForegroundColor Green

$ReleaseNotesFile = "release_notes_temp.txt"
$inVersionSection = $false
$notes = [System.Collections.Generic.List[string]]::new()

$lines = Get-Content -Path VersionHistory.md -Encoding utf8
foreach ($line in $lines) {
    if ($line -match "^# v $Version") {
        $inVersionSection = $true
        $notes.Add($line)
        continue
    }
    if ($inVersionSection) {
        if ($line -match "^# v \d+\.\d+\.\d+\.\d+") {
            break
        }
        $notes.Add($line)
    }
}

if ($notes.Count -gt 0) {
    $notes | Out-File -FilePath $ReleaseNotesFile -Encoding utf8
} else {
    "ImageCropper v$Version Release" | Out-File -FilePath $ReleaseNotesFile -Encoding utf8
}

# Create GitHub Release via gh CLI
gh release create "v$Version" $SetupFile --title "v$Version" --notes-file $ReleaseNotesFile
$releaseResult = $LASTEXITCODE

# Cleanup temp file
if (Test-Path $ReleaseNotesFile) {
    Remove-Item $ReleaseNotesFile
}

if ($releaseResult -ne 0) {
    Write-Error "GitHub Release creation failed."
}

Write-Host "=== Release process completed successfully: v$Version ===" -ForegroundColor Cyan
