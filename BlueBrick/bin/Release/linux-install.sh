#!/bin/bash
# script to install BlueBrick on Linux
# this script will create a desktop shortcut

# get the current BlueBrick folder
BBFolder=$(pwd)

cat > BlueBrick.desktop << _EOF_
[Desktop Entry]
Encoding=UTF-8
Version=1.0
Name=BlueBrick
GenericName=BlueBrick Lego Layout Planer
Terminal=false
Type=Application
Categories=Application
Comment=Plan Lego Layout with BlueBrick
Icon=$BBFolder/icons/32x32/apps/BlueBrick.png
Exec=mono $BBFolder/BlueBrick.exe &
_EOF_

# make the shortcut executable
chmod 775 BlueBrick.desktop

# move the shortcut to the desktop
DesktopFolder=$(xdg-user-dir DESKTOP)
mv BlueBrick.desktop $DesktopFolder


