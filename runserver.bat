REM CM14 rework: non-RMC edit marker.
@echo off
dotnet run --project Content.Server -- --cvar config.presets=Corvax/main
pause
