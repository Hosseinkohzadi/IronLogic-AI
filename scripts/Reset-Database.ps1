# Clean Database Script for IronLogic AI
# This script deletes the SQLite database to force recreation with correct admin user

Write-Host "?? IronLogic AI - Database Reset Script" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$dbPath = "C:\Projects\IronLogic-AI\src\IronLogic.Api\bin\Debug\net10.0\ironlogic.db"
$dbShmPath = "C:\Projects\IronLogic-AI\src\IronLogic.Api\bin\Debug\net10.0\ironlogic.db-shm"
$dbWalPath = "C:\Projects\IronLogic-AI\src\IronLogic.Api\bin\Debug\net10.0\ironlogic.db-wal"

# Check if database exists
if (Test-Path $dbPath) {
    Write-Host "? Found database file: $dbPath" -ForegroundColor Green
    
    # Delete main database file
    try {
        Remove-Item $dbPath -Force
        Write-Host "? Deleted: ironlogic.db" -ForegroundColor Green
    }
    catch {
        Write-Host "? Error deleting ironlogic.db: $_" -ForegroundColor Red
        exit 1
    }
    
    # Delete WAL file if exists
    if (Test-Path $dbWalPath) {
        try {
            Remove-Item $dbWalPath -Force
            Write-Host "? Deleted: ironlogic.db-wal" -ForegroundColor Green
        }
        catch {
            Write-Host "??  Warning: Could not delete ironlogic.db-wal" -ForegroundColor Yellow
        }
    }
    
    # Delete SHM file if exists
    if (Test-Path $dbShmPath) {
        try {
            Remove-Item $dbShmPath -Force
            Write-Host "? Deleted: ironlogic.db-shm" -ForegroundColor Green
        }
        catch {
            Write-Host "??  Warning: Could not delete ironlogic.db-shm" -ForegroundColor Yellow
        }
    }
    
    Write-Host ""
    Write-Host "?? Database deleted successfully!" -ForegroundColor Green
    Write-Host ""
    Write-Host "?? Next Steps:" -ForegroundColor Cyan
    Write-Host "1. Start the application (F5 or dotnet run)" -ForegroundColor White
    Write-Host "2. Wait for seeding to complete" -ForegroundColor White
    Write-Host "3. Look for: 'Admin user created successfully'" -ForegroundColor White
    Write-Host "4. Test login with:" -ForegroundColor White
    Write-Host "   Email: admin@ironlogic.ai" -ForegroundColor Yellow
    Write-Host "   Password: Admin@123456" -ForegroundColor Yellow
    Write-Host ""
}
else {
    Write-Host "??  Database file not found at: $dbPath" -ForegroundColor Yellow
    Write-Host "   This is OK if the application hasn't been run yet." -ForegroundColor White
    Write-Host ""
    Write-Host "?? Next Steps:" -ForegroundColor Cyan
    Write-Host "1. Start the application (F5 or dotnet run)" -ForegroundColor White
    Write-Host "2. Database will be created with correct admin user" -ForegroundColor White
    Write-Host ""
}

Write-Host "Press any key to exit..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
