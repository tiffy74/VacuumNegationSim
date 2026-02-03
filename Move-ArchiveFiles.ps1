# Documentation Consolidation - File Mover Script
# Moves archive files to Documentation/Archive/ folder

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Documentation Consolidation - File Mover" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Get current directory
$rootPath = Get-Location

# Create Archive directory if it doesn't exist
$archivePath = Join-Path $rootPath "Documentation\Archive"
if (!(Test-Path $archivePath)) {
    New-Item -Path $archivePath -ItemType Directory -Force | Out-Null
    Write-Host "? Created Documentation\Archive\" -ForegroundColor Green
}

# Define files to archive
$filesToArchive = @(
    # Architecture phase docs
    "ARCHITECTURE_PHASE1_COMPLETE.md",
    "ARCHITECTURE_PHASE2_COMPLETE.md",
    "ARCHITECTURE_PHASE3_COMPLETE.md",
    "ARCHITECTURE_PHASE4_COMPLETE.md",
    "ARCHITECTURE_PHASE5_COMPLETE.md",
    "ARCHITECTURE_PHASE6_COMPLETE.md",
    "ARCHITECTURE_PHASE7_COMPLETE.md",
    
    # Stage implementation docs
    "STAGE8_IMPLEMENTATION.md",
    "STAGE9_IMPLEMENTATION.md",
    "STAGE10_IMPLEMENTATION.md",
    "STAGE12_COMPLETE.md",
    
    # Old UI implementation guides (Assets folder - will list separately)
    # These are in Assets/Viable/Core.Unity/UI/
    
    # Unity UI learning guides
    "UNITY_UI_BASICS_GUIDE.md",
    "UNITY_UI_CHEAT_SHEET.md",
    "UNITY_UI_VISUAL_GUIDE.md",
    "UNITY_MIGRATION_GUIDE.md",
    
    # Temporary fix documents (superseded by consolidated troubleshooting)
    "DROPDOWN_FIX_SUMMARY.md",
    "DROPDOWN_TROUBLESHOOTING.md",
    "DROPDOWN_TEMPLATE_HEIGHT_FIX.md",
    "TOPBAR_PRESET_FIX.md",
    "POINT_SOURCE_EDITOR_GUIDE.md",
    "WIRE_ONMECHANISMCHANGED_GUIDE.md",
    "URGENT_FIX_CHILDREN_ESCAPING_RIGHTDOCK.md",
    "UI_REFRESH_METHODS_IMPLEMENTATION_GUIDE.md",
    
    # UI positioning guides (superseded by consolidated)
    "UI_POSITIONING_YOUR_GAMEOBJECTS.md",
    "UI_POSITIONING_QUICK_CHECKLIST.md",
    "YOUR_CUSTOM_UI_POSITIONING_GUIDE.md",
    
    # Consolidation planning docs (move after completion)
    "DOCUMENTATION_CONSOLIDATION_PLAN.md",
    "CONSOLIDATION_STATUS.md"
)

# Move files
$movedCount = 0
$notFoundCount = 0

foreach ($file in $filesToArchive) {
    $sourcePath = Join-Path $rootPath $file
    $destPath = Join-Path $archivePath $file
    
    if (Test-Path $sourcePath) {
        Move-Item -Path $sourcePath -Destination $destPath -Force
        Write-Host "? Moved: $file" -ForegroundColor Green
        $movedCount++
    }
    else {
        Write-Host "??  Not found: $file" -ForegroundColor Yellow
        $notFoundCount++
    }
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan

# Move UI implementation docs from Assets folder
Write-Host ""
Write-Host "Moving Assets/Viable/Core.Unity/UI/ docs..." -ForegroundColor Cyan

$uiDocsPath = Join-Path $rootPath "Assets\Viable\Core.Unity\UI"
$uiArchivePath = Join-Path $archivePath "UI_Implementation_Guides"

if (Test-Path $uiDocsPath) {
    if (!(Test-Path $uiArchivePath)) {
        New-Item -Path $uiArchivePath -ItemType Directory -Force | Out-Null
    }
    
    $uiFiles = @(
        "MASTER_UI_SETUP_GUIDE.md",
        "RIGHTDOCK_SETUP.md",
        "TOPBAR_SETUP.md",
        "DETAIL_SECTIONS_SETUP.md",
        "CORE_PARAMETERS_SETUP.md",
        "IMPLEMENTATION_SUMMARY.md",
        "IMPLEMENTATION_PROGRESS.md",
        "DOCUMENTATION_INDEX.md",
        "QUICK_REFERENCE_HIERARCHY.md",
        "DESIGN_ALIGNMENT_CHECK.md"
    )
    
    foreach ($file in $uiFiles) {
        $sourcePath = Join-Path $uiDocsPath $file
        $destPath = Join-Path $uiArchivePath $file
        
        if (Test-Path $sourcePath) {
            Move-Item -Path $sourcePath -Destination $destPath -Force
            Write-Host "? Moved: UI/$file" -ForegroundColor Green
            $movedCount++
        }
        else {
            Write-Host "??  Not found: UI/$file" -ForegroundColor Yellow
            $notFoundCount++
        }
    }
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Summary" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "? Moved: $movedCount files" -ForegroundColor Green
Write-Host "??  Not found: $notFoundCount files" -ForegroundColor Yellow
Write-Host ""
Write-Host "Archive location: $archivePath" -ForegroundColor Cyan
Write-Host ""
Write-Host "? DONE!" -ForegroundColor Green
Write-Host ""

# Optionally open archive folder
$openFolder = Read-Host "Open archive folder? (y/n)"
if ($openFolder -eq "y") {
    Invoke-Item $archivePath
}
