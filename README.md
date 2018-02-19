# BlueBrick version 1.8.2.0

## Overview
Bluebrick is software to plan Lego© Diorama or Layout. It is particularly efficient
to lay down train tracks.

Bluebrick is partially compatible with LDRAW.

Bluebrick use a part library, not included in this repo, but available in [this repo](https://bitbucket.org/bluebrick/bluebrick.bitbucket.org). This library is fully customizable,
and the software can be used to plan non-lego© layout, if you are willing to create the parts.

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
$ mono BlueBrick.exe &

### Mac OS
Warning: it seem the program cannot run on Mac OSX. Several users have reported an
error related to user32.dll. If you know how to fix it, please, contact me.

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
$ mono BlueBrick.exe

## License

BlueBrick is a free software for Windows/Vista developped by Alban Nanty in
open source. You can use it for your personnal purpose but not for commercial
profit. You can redistribute it under the terms of the [GNU General Public
License version 3](http://www.gnu.org/licenses/) as published by the [Free
Software Foundation](http://www.fsf.org/licensing/licenses/gpl.html). This
program is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY.

This software was designed specially for the AFOLs who want to prepare the
layouts of their LEGO© exhibitions. BlueBrick is compatible with [LDRAW](http://www.ldraw.org/)
and ["Train Depot Track Designer"](http://www.ngltc.org/Train_Depot/td.htm)
and was designed to extend easily
its part database. Its layers feature allow you to better organize your map,
and some specific layers make possible the addition of annotation and area
assignment.