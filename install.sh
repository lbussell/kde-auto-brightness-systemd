#!/bin/sh
set -e

installDir=$HOME/.local/share/auto-brightness
installLocation=$installDir/auto-brightness

systemdDir=$HOME/.config/systemd/user
serviceFile=$systemdDir/auto-brightness.service
timerFile=$systemdDir/auto-brightness.timer

# Build
echo "Building..."
dotnet publish AutoBrightness.cs
echo "Build complete."

# Install binary in ~/.local/share/
mkdir -p $installDir
cp artifacts/AutoBrightness/AutoBrightness $installLocation
echo "Copied binary to $installLocation"

# Install systemd service and timer
cp auto-brightness.service $serviceFile
echo "Installed service as $serviceFile"
cp auto-brightness.timer $timerFile
echo "Installed timer as $timerFile"

# Enable and start the systemd timer
systemctl --user daemon-reload
systemctl --user enable --now auto-brightness.timer
echo "Reloaded systemd user daemon and started timer."

echo ""
echo "Installation complete. The auto-brightness service is now running."
echo ""
echo "To manually run the service now:"
echo "  systemctl --user start auto-brightness.service"
echo "To check the status of the service:"
echo "  systemctl --user status auto-brightness.service"
echo "To check the status of the timer:"
echo "  systemctl --user status auto-brightness.timer"
echo ""
