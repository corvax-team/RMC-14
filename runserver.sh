#!/bin/sh
# CM14 rework: non-RMC edit marker.
dotnet run --project Content.Server -- --cvar config.presets=Corvax/main
read -p "Press enter to continue"
