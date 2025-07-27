@echo off
echo Cleaning up old FinHubAddOns files...

cd /d "D:\inetpub\wwwroot\finhub\DesktopModules\FinHubAddOns"

echo Deleting old Component files...
del /f "Components\IItemRepository.cs" 2>nul
del /f "Components\ItemRepository.cs" 2>nul

echo Deleting old Service files...
del /f "Services\ItemController.cs" 2>nul

echo Deleting old Script files...
del /f "Scripts\ItemEdit.js" 2>nul
del /f "Scripts\ItemView.js" 2>nul
del /f "Scripts\QuickSettings.js" 2>nul

echo Deleting old HTML files...
del /f "Edit.html" 2>nul
del /f "Settings.html" 2>nul

echo Deleting old Resource files...
del /f "App_LocalResources\Edit.resx" 2>nul
del /f "App_LocalResources\Settings.resx" 2>nul
del /f "App_LocalResources\View.resx" 2>nul

echo Cleanup complete!
echo.
echo Please rebuild your solution in Visual Studio.
pause