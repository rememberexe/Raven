@echo off
title Raven APK Builder

echo ==============================
echo Raven APK Builder - Android Only
echo ==============================

echo.
echo Building Android APK...

dotnet publish RavenMobile.csproj ^
 -f net10.0-android ^
 -c Release ^
 -p:AndroidPackageFormat=apk ^
 --no-restore

echo.
echo ==============================
echo APK Build Completed
echo ==============================

echo APK location:
echo bin\Release\net10.0-android\publish\

pause