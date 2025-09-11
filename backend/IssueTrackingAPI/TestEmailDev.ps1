# Development Email Testing Script
# This script helps test the email notification feature in development mode

Write-Host "=== Issue Tracking Email Notification Test ===" -ForegroundColor Green
Write-Host ""

# Check if the API is running
Write-Host "1. Checking if API is running..." -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "http://localhost:5000/health" -Method GET -TimeoutSec 5
    Write-Host "✓ API is running" -ForegroundColor Green
} catch {
    Write-Host "✗ API is not running. Please start the API first:" -ForegroundColor Red
    Write-Host "  dotnet run" -ForegroundColor Cyan
    exit 1
}

Write-Host ""
Write-Host "2. Email notification will be logged to:" -ForegroundColor Yellow
Write-Host "   - Console output (with DEBUG level logging)" -ForegroundColor Cyan
Write-Host "   - logs/email-notifications.log file" -ForegroundColor Cyan

Write-Host ""
Write-Host "3. To test the email feature:" -ForegroundColor Yellow
Write-Host "   a) Start the frontend: cd ../frontend/IssueTrackingFrontend && npm start" -ForegroundColor Cyan
Write-Host "   b) Create a ticket as a regular user" -ForegroundColor Cyan
Write-Host "   c) Login as a Rep and close the ticket" -ForegroundColor Cyan
Write-Host "   d) Check the console output and log file for email content" -ForegroundColor Cyan

Write-Host ""
Write-Host "4. Development Configuration:" -ForegroundColor Yellow
Write-Host "   - Email sending is disabled in development mode" -ForegroundColor Cyan
Write-Host "   - Email content is logged instead of sent" -ForegroundColor Cyan
Write-Host "   - Check appsettings.Development.json for email settings" -ForegroundColor Cyan

Write-Host ""
Write-Host "=== Ready for Testing ===" -ForegroundColor Green
