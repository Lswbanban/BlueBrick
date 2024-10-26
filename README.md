# BlueBrick version 1.9.2.0

## Overview
Bluebrick is software to plan LEGO© Diorama or Layout. It is particularly efficient
to lay down train tracks.

Bluebrick is partially compatible with [LDRAW](https://www.ldraw.org/), 
[Train Depot Track Designer](http://www.ngltc.org/Train_Depot/td.htm),
and [4DBrix nControl](https://www.4dbrix.com/).

Bluebrick use a part library, not included in this repo, but available in [this repo](https://github.com/Lswbanban/BlueBrickParts). This library is fully customizable,
and the software can be used to plan non-LEGO© layout, if you are willing to create the parts.

## Install

Note: you can refer to the [BlueBrick website](http://bluebrick.lswproject.com/),
in the Download section, for detailed install instructions with pictures.

### Windows Vista or Windows 7, 8 or 10
Nothing special need to be install first. Simply extract the files contained in
the zip, somewhere on your hardrive, by keeping the same folders organization.
Then double click on the BlueBrick.exe file in the install folder to run
BlueBrick.

### Windows XP
a) First, you need to download and install the Microsoft .NET FrameWork (at
least version 4.0), that can be found on the
[Microsoft website](http://www.microsoft.com/en-us/download/details.aspx?id=17851)
(if not already installed on your machine).

b) Then extract the files contained in the BlueBrick zip file, somewhere on
your hardrive, by keeping the same folders organization.

c) Double click on the BlueBrick.exe file in the install folder.

### Linux
#### - MonoDevelop
a) You need to install Mono for Linux first. I suggest you to use the Software
Center, and search for "MonoDevelop".

a.bis) If you want to be able to open the documentation file, you should also
install a CHM reader, for example "xCHM" which can also be installed from the
Software Center.

b) Unzip the BlueBrick zip file somewhere on your hardrive, for example
in ~/BlueBrick

c) Then you should run the "linux-install.sh" shell script inside the install
folder. This will create a BlueBrick.desktop shortcut on your Desktop. Once the
shortcut is created, simply double click it to run BlueBrick.

c.bis) Or simply, if you don't want to create a shortcut, to run BlueBrick in
a Terminal Shell, type:

$ mono32 BlueBrick.exe &

#### - PlayOnLinux
a) Create a new wineprefix.

b) Click "Configure" and under "Install components" choose `dotnet45`.

c) Copy the BlueBrick folder to the virtual drive and create a shortcut.

### Mac OS
- Alternatively, you can also install PlayOnMac and use the same instructions as above for PlayOnLinux.

a) You need to install Mono for Mac first which you can download here:
http://www.mono-project.com/download/

If you want some help on installing Mono on MaC OSX, please visit:
http://www.mono-project.com/docs/about-mono/supported-platforms/osx/

If you got some DllNotFoundException error, please read the following page
to learn how to solve it:
http://www.mono-project.com/docs/advanced/pinvoke/dllnotfoundexception/

a.bis) If you want to be able to open the documentation file, you should also
install a CHM reader, for example:
[iCHM](http://www.macupdate.com/app/mac/28171/ichm)
or [Chmox](http://chmox.sourceforge.net/).

b) Unzip the BlueBrick zip file somewhere on your hardrive.

c) To run BlueBrick, in a Terminal Shell type, go to the BlueBrick installation
folder using the "cd" command then type:

$ mono32 BlueBrick.exe

## License

BlueBrick is a free and open source software for Windows developped by Alban Nanty.
Copyright (C) since 2009  Alban Nanty

This program is free software: you can redistribute it and/or modify it under
the terms of the GNU General Public License version 3 as published by the Free
Software Foundation (https://www.gnu.org/licenses/gpl-3.0.html).

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

This software was designed specially for the AFOLs who want to prepare the
layouts of their LEGO(c) exhibitions. BlueBrick is compatible with:
- LDRAW (https://www.ldraw.org/)
- "Train Depot Track Designer" (http://www.ngltc.org/Train_Depot/td.htm)
- 4DBrix nControl (https://www.4dbrix.com/)

BlueBrick was designed to extend easily its part database. Its layers feature
allow you to better organize your map, and some specific layers make possible
the addition of annotation and area assignment.
