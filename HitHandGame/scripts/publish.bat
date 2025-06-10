@echo off
echo 正在發布 HitHandGame 為獨立執行檔...
echo.

echo 清理舊的發布檔案...
if exist "publish" rmdir /s /q "publish"

echo 發布 Windows x64 版本...
dotnet publish -c Release -r win-x64 --self-contained -o "publish\win-x64"

echo 發布 Windows x86 版本...
dotnet publish -c Release -r win-x86 --self-contained -o "publish\win-x86"

echo.
echo 發布完成！
echo Windows x64 版本位於: publish\win-x64\
echo Windows x86 版本位於: publish\win-x86\
echo.
echo 請將音效檔案放入對應目錄的 Sounds 資料夾中
pause
