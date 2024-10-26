                    +---------------------------+
                    | BlueBrick version 1.9.2.0 |
                    +---------------------------+

-------------------------------------------------------------------------------
1) License
-------------------------------------------------------------------------------

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

-------------------------------------------------------------------------------
2) Install
-------------------------------------------------------------------------------

Note: you can refer to the BlueBrick website (https://bluebrick.lswproject.com/),
in the Download section, for detailed install instructions with pictures.

2.1) Windows Vista or Windows 7, 8 or 10
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
Nothing special need to be install first. Simply extract the files contained in
the zip, somewhere on your hardrive, by keeping the same folders organization.
Then double click on the BlueBrick.exe file in the install folder to run
BlueBrick.

2.2) Windows XP
~~~~~~~~~~~~~~~
a) First, you need to download and install the Microsoft .NET FrameWork (at
least version 4.0), that can be found on the Microsoft website (if not already
installed on your machine):
http://www.microsoft.com/en-us/download/details.aspx?id=17851

b) Then extract the files contained in the BlueBrick zip file, somewhere on
your hardrive, by keeping the same folders organization.

c) Double click on the BlueBrick.exe file in the install folder.

2.3) Linux
~~~~~~~~~~
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

2.4) Mac OSX
~~~~~~~~~~~~
a) You need to install Mono for Mac first which you can download here:
http://www.mono-project.com/download/

If you want some help on installing Mono on MaC OSX, please visit:
http://www.mono-project.com/docs/about-mono/supported-platforms/osx/

If you got some DllNotFoundException error, please read the following page
to learn how to solve it:
http://www.mono-project.com/docs/advanced/pinvoke/dllnotfoundexception/

a.bis) If you want to be able to open the documentation file, you should also
install a CHM reader, for example: 
"iCHM" (http://www.macupdate.com/app/mac/28171/ichm)
or "Chmox" (http://chmox.sourceforge.net/).

b) Unzip the BlueBrick zip file somewhere on your hardrive.

c) To run BlueBrick, in a Terminal Shell type, go to the BlueBrick installation
folder using the "cd" command then type:
$ mono32 BlueBrick.exe

-------------------------------------------------------------------------------
3) Help
-------------------------------------------------------------------------------

Please refer to the help file that comes with the application (menu Help > 
Content and Index...)
For Linux and Mac user, you should install a CHM reader, in order to be able
to open the help file (see above for more details).

-------------------------------------------------------------------------------
4) History
-------------------------------------------------------------------------------
Open bugs:
- [target 1.10] MISSING FEATURE (Alban): Add some !BLUEBRICK meta commands in LDraw format to save all the data of a BlueBrick file in LDraw. Then remove the warning for saving in LDRAW.
- MISSING FEATURE (Alban): A feature to check if there's new parts available online in the official library (when opening a file with missing parts).
- MISSING FEATURE (Matthias): Add a simple view for the track with the part number displayed overlaid (add a new tag in the xml for this simplified name to avoid too long names).
- MISSING FEATURE (Matthias): BlueBrick should check if there's new library package available in background after launch.
- MISSING FEATURE (Matthias): Better resolution for part images (Images can have an additional <scale> property in the xml-File, if no scale parameter is provided, the default resolution of 8 pixels/stud is used)
- MISSING FEATURE (zephyr1934): Add a favorite feature where you can tag a part as your favorite, and display all your favorite parts in a specific tab of the library
- MISSING FEATURE (Alban): Add some preference to edit the Watermark property (font, text color, background color and transparency, content displayed, which corner)
- MISSING FEATURE (Alban): Add a trackbar in the bottom left of the layers to edit the transparency of the selected layer
- MISSING FEATURE (Alban): Add a version number in the general info?
- MISSING FEATURE (Alban): Extend the ConnectionTypeList.xml to precise for each connection, to which connection it can connect (this allow a male/female connections types, by default it connect to itself, otherwise if another type is specified, it connect to that type)
- MISSING FEATURE #49 (Alban): add the support of copy/paste of layer (including grid and area layers which can not be pasted for now)
- MISSING FEATURE #43 (Alban): add the support of move/copy/paste a part of an area layer
- MISSING FEATURE #44 (Alban): Need a "color picker" and "font size" shortcut in the contextual menu on the Text layer, or in the toolbar
- MISSING FEATURE #12 (FreeBee) : Snap through layers (handy if you have multiple layers of track to connect) or make transparent all other layers except the current one
- MISSING FEATURE (Larry): Select parts in multiple layers
- MISSING FEATURE (Alban): Enhance layer stack usability: add a context menu to delete a layer, allow drag'n'drop reorganization.
- MISSING FEATURE (Alban): Enhance the part list export in HTML with some javascript for making the image optional.
- MISSING FEATURE #7 (Alban): Split the toolbar in two (one for file, one for edition) + add a new toolbar for navigation
- MISSING FEATURE #10 (Alban): Compatibility with TrackDraw?
- MISSING FEATURE #11 (FreeBee): Mirroring selected (just like rotating) -> almost impossible to do
- MISSING FEATURE (Alban): The selection should be an undoable action
- MISSING FEATURE (Ludo & Denis): Add some optional arrows at the extremities of the linear rulers.
- MISSING FEATURE (Ludo): Move the measure values along the linear rulers in order to avoid overlapping of close parallel rulers.
- MISSING FEATURE (Denis): Add an option to place the measure above the line of the ruler
- MISSING FEATURE (Ludo): Add a context menu on the Part Usage list, in order to export the list from there.
- MISSING FEATURE (Vincent): Display the number of part selected (probably in the status bar).
- BUG #57 (Alban): The drag and drop of file is not working under Mono 2.10
- BUG #58 (Alban): The cancel of the edition with the right click mouse button doesn't work under Mono
- BUG (Alban): The saving of a group in the brick library after rotating it will create a correct image, but wrong part positions when the group is placed back on the map
- REFACTORING (Alban): In the part XML, promote the <LDraw><PreferredHeight> to normal part properties, and use it to set the default Altitude of parts
- REFACTORING (Alban): The two buttons to raise and lower parts in the toolbar should probably change the Altitude of the parts for a brick layer (that's what the users may expect)

Bug fixed in 1.9.2.0:
- NEW FEATURE (Mattzobricks): Add snapping angles of 11.25° and 5.625° in the rotation snapping menu
- BUGFIX (Mattzobricks): The download new parts feature was displaying an error (the HTTP request was refused with a 403 error)
- BUGFIX #62 on Bitbucket (FreeStorm): Text cells on Text Layer is not drawn at the correct position on Linux (thanks @Shevonar for the fix)
- BUGFIX (Vincent): open the popup modial dialog box on the same screen as the Main Application (when you have multiple screens and run BlueBrick on the second monitor)
- BUGFIX (Vincent): Make the combo box for the grid size a bit larger in the Grid Layer Properties window

Bug fixed in 1.9.1.0:
- BUGFIX (Alban): Update the application to download the part package in https instead of http, since the website has moved to https
- BUGFIX (Alban): Fix the height calculation of the preview image in the Export Image Windows
- BUGFIX (Alban): Fix the drawing of the watermark when the scrollbar is displayed

Bug fixed in 1.9.0.0:
- NEW FEATURE (Alban): The window to edit the general info has been moved to a tab next to the layer list, including the edition of the map background color
- NEW FEATURE (Alban): The part list window (now renamed into Part Usage) has been moved to a tab next to the layers list
- NEW FEATURE (Alban & Matthias): The Part Usage now also display the Budget count, the missing count and the part usage percentage against the current budget, when a budget is opened
- NEW FEATURE (Ludo): The Part Usage now display the sum of all the parts on each layer (or globally) and the part usage percentage per layer (or globally)
- NEW FEATURE (Alban): The Part Usage can now be exported in CSV format (on top of HTML and Text)
- NEW FEATURE (Alban): The Part Usage can now be reordered by clicking on the columns headers
- NEW FEATURE (Alban): Added a checkbox to include (or not) hidden layers in the Part Usage list
- NEW FEATURE (Alban): The export of part usage list is now localized in the language of the application
- NEW FEATURE (Vincent): An option to revert the counting of the parts in the budget (display remaining count instead of used count, option available in Preferences)
- NEW FEATURE (Vincent): Make the error message when pasting on the wrong layer, forgettable. Just beep instead.
- NEW FEATURE (Alban): A feature to download library package online from various sources (official web site, and non official url).
- NEW FEATURE (Alban): Support local Connection Type file in order to facilitate the deployment of packages (now you can add a "config" sub folder inside a part category folder, and add a ConnectionTypeList.xml file inside in order to define the connections of your package)
- NEW FEATURE (Alban): Support the loading and saving of *.ncp file (from the thirdparty 4dbrix.com)
- NEW FEATURE (supertruper1988): Add the possibility to set a template BBM file that is loaded when creating a new map.
- NEW FEATURE (Matthias): An option to display part names below the parts, in the part library.
- NEW FEATURE (Alban): The new layers are now inserted above the selected layer, not at the top of the stack by default
- NEW FEATURE (Matthias): When you delete a part, the active connection of the new selected part is the one where the deleted part was connected.
- NEW FEATURE (supertruper1988): The default export image extension is now PNG instead of BMP (and the list of available formats has been reordered)
- NEW FEATURE (Alban): Add a new mouse cursor for when the user press the shift key (before moving the mouse)
- NEW FEATURE (Alban): Add a "Properties..." menu item in the contextual menu to edit the texts and rulers
- NEW FEATURE (Alban): Add some settings to edit the two Hull colors and thickness (brick and other) in the Preference window (Appearance tab)
- NEW FEATURE (Matthias): Add the 2 studs snapping.
- NEW FEATURE (Alban): Improve the usuability of the select path feature (through the menu items), now the path is selected between the last two selected objects, and the previous selection is not cleared.
- NEW FEATURE (Alban): Simplify the Path selection with a shortcut key (Multi selection Key + Pan View Key) + click on the second part (instead of going through the contextual menu).
- NEW FEATURE (Ludo): The export window now propose to export in multiple images, which extend indefinitely the export area at any scale.
- NEW FEATURE (Alban): Add scrollbars on the map view (can be shown or hidden from the map context menu and the View menu)
- NEW FEATURE (Evans): Show the current interruption of the 12V circuit breaker rail (hard-coded for the 2 specific parts)
- NEW FEATURE (Alban): For more flexibility during export and also during the edition, the hulls are now displayable PER layer, with different color and thickness for each layer
- NEW FEATURE (Alban): Support the brick elevation by displaying it on each part if the option is checked on the layer, and save the display elevation property in the BBM file
- NEW FEATURE #6 (Alban): Save a backup file of the map if the application crash.
- NEW FEATURE (Alban): Add the "Use as Template" context menu for the text cells (works like for the Rulers).
- BUGFIX (Alban): Fix a crash when saving in the library a group of parts without connection points.
- BUGFIX (Alban): Fix a crash when duplicating parts very quickly with the ALT key (crash due to delay with the clipboard)
- BUGFIX (Ludo): If there was an empty description in a part xml file (without language tags, just <Description></Description>) then the connection points of the parts were missing.
- BUGFIX (Vincent): Probably fixed sometimes when drag-n-dropping parts, only the selection box and the free connection point were moving (and not the grabbed parts)
- BUGFIX (Alban): Now you can open a file which is in readonly access.
- BUGFIX (Alban): Finally found a way to release the lock on the images when reloading the part library (well hopefully this time it will be fixed).
- BUGFIX (Alban): The scroll of the mouse wheel was not happening when moving the mouse over the part lib or layer stack, only after you click on it (now the focus is automatically given to the region of the application where the mouse move)
- BUGFIX (Alban): When updating in the library a group of parts that was not displayed in the library (because filtered with keyword) the image was added two times
- BUGFIX (Alban): The cancel of the files download from the download center form was not properly stopped
- BUGFIX (Matthias): The color name was "unknown" in the Bubble info at BlueBrick startup or after every Part Library Reload (and could only be valid after changing the preferences regarding the Bubble info)
- BUGFIX (Alban): Adding a part on a hidden layer was counting that part in the Part Usage list
- BUGFIX (Alban): The flag that check if a budget was modified was not correctly set
- BUGFIX (Alban): The Part Usage list was not properly updated when ungrouping a named group while being visible.
- BUGFIX (Matthias): The next connection preference set in part xml file was not respected when connecting a brick with drag'n'drop from the library, or after a move.
- BUGFIX (Thai Bricks): Now the "&" character is visible in layer's name if the user edits the layer's name with an ampersome.
- BUGFIX (Matthias): If a part name contains a dot, the full part id was not properly displayed in the part list on in the status bar.
- BUGFIX (Alban): The selection was displayed in the exported the image.
- BUGFIX (Alban): The ruler measurement text was not scaled in the exported image (which is the intended behavior on the map panel, but not on the exported image)
- BUGFIX (Matthias): The saving of a group in the Brick Library was incorrect when the group contain a sub-group, and you moved the group before saving.
- BUGFIX (Alban): If you move a large group with connections by picking it oustside its parts, but the mouse is under another part with connection not in the group, the group was still try to connect to free connection points, instead of just translate.
- BUGFIX (Alban): If you open the export image window and change the rendering settings, and cancel to close the window without exporting the image, then the settings were changed on the map
- REFACTORING (Alban): Optimization of the update of the part list panel (use AddRange() instead of Add() to add the items in the ListView)
- REFACTORING (Alban): Modernize the HTML code of the part usage list export (add some CSS and colors)
- REFACTORING (Alban): Replace the progress bar by track bar for editing transparency of the layers
- REFACTORING (Alban): In the Preference Window, add a combo box to choose the pan view key (before the key was deducted from the 2 other modifier keys which were set)


Bug fixed in 1.8.2.0:
- NEW FEATURE (Alban): Implement the saving of the "Connection Preference List" when saving a group in the library from the File menu.
- NEW FEATURE (Ludo): Make the link to the bluebrick website clickable in the About Box.
- BUGFIX (Alban & many users): CRITICAL! The unique id generator was not so unique, preventing sometime the BBM files to be reloaded (especially the large file which had more chance to have a collision of ids)
- BUGFIX (Nicolas): Potential CRASH! when loading a file containing a part with connection, for which the connection list has been updated in the library between the save and the load of the file.
- BUGFIX #54 (Alban): If you select a named group on the map and click on a part in the part lib, the part is added in the middle of the group.
- BUGFIX (Alban): The xml parsing of the <GroupConnectionPreferenceList> tag for the SET xml files, was skipping one connection preference every two.
- BUGFIX (Alban): When you just form a group, there was no active connection set by default.
- BUGFIX #55 (Alban): When you replace a part in a group, the display area of the group is not recomputed
- BUGFIX #56 (Alban): When you replace a part in a group, the replaced part doesn't belong to the group
- BUGFIX (Alban): After reloading the part library, all the settings of the part lib are reset to what it was when the application started instead of what it was just before reloading
- BUGFIX (Alban): If there's an error while loading a file, the loading progress bar was staying visible after closing the error window
- BUGFIX (Ludo): Improve the rendering of the LOD images of each brick. There was a 1 pixel shift, visible when you rotate a baseplate for example.
- BUGFIX (Christopher): The report email in the crash window is no longer working.

Bug fixed in 1.8.1.0:
- NEW FEATURE (Alban): Add a install script file and icons for Linux
- REFACTORING #29 (Anonymous): Upgrade the target platform to .NET 4.0
- BUGFIX (Alban): CRASH!! 100% crash at Startup on Linux (due to multi-threading of the SplashScreen).
- BUGFIX (Alban): CRASH!! If you change the Preference when the Part Library is empty
- BUGFIX #59 (freestorm): the parts with upper case .XML extensions cannot be loaded on Linux
- BUGFIX #46 (Anonymous + Alban): Display the Help when pressing F1 (or via the Help menu) on Mac and Linux, if you have a CHM viewer installed.
- BUGFIX (Alban): Fix some bug for the behaviour of the text box in the Save Group in Library window.
- BUGFIX (Alban): Now the selection is not cleared after an Export (but still not visible in the exported image)
- BUGFIX #53 (Alban): In the export window you can specify a selection rectangle null by setting a (bottom < top) or (right < left)
- BUGFIX (Alban): Handle correctly the new line char in the load part file Error window (on Linux and Mac). Make a pass on all the  other places in the code where endline is used, to use the platform dependant end of line.

Bug fixed in 1.8.0.0:
- NEW FEATURE (Erik): A new layer type for measurement tools.
- NEW FEATURE (Alban): Add a filter combobox in the bottom of the library
- NEW FEATURE (Alban): Add a sorting order for the parts in the library based on a new <SortingKey> tag in the XML file of the part
- NEW FEATURE #23 (many users): A feature to set a budget of parts and let BlueBrick warn you when you reach the budget
- NEW FEATURE (Alban): Add more contextual cursors inside the layout panel (hidden layer, default grid cursor, new text, ruler cursors, etc...)
- NEW FEATURE (Alban): More precise Selection: Now the picking and rectangle selection use the hull of the part, and the text rectangle for rotated text, instead of the axis aligned bounding box of the part or text.
- NEW FEATURE #9 and #45 (Loys): Use the clipboard to allow copy paste between several BleuBrick instances (support also copy + load + paste)
- NEW FEATURE (Alban): A feature to save a group that was created with BlueBrick in the library
- NEW FEATURE (David): Add a option in the preference to let the user choose if he wants an offset after a copy/paste
- NEW FEATURE #37 (doc_brown): Add a checkbox in the warning message box when not saving in BBM format.
- NEW FEATURE (Alban): Add more options in the preference to create any of the 5 types of layer for a new map
- NEW FEATURE (Pierre): The right click button can now cancel the current edition if you are in the middle of an edition (for any type of layer), but not under Mono
- NEW FEATURE (Alban): Add some checkbox in the export window to choose what to export (currently you can add the watermark, the Hull for bricks, the electric circuit and the connection points)
- NEW FEATURE #8 (Patrick): Add an optional part outline to see more clearly the different parts when exporting the image: now you can draw the Hull of the part in the exported image.
- NEW FEATURE (Alban): In the generated part list, don't list the sub part of a set, list only the set (unless you ungroup the set).
- REFACTORING (Alban): Move the display of free connection point and watermark in the View menu and add the display of the hull.
- REFACTORING #18 (Alban): Check where I could have used the directive "where" (remove code duplication)
- REFACTORING (Alban): Replace all the type test using a string with the "is" keyword
- REFACTORING (Alban): Improve the rendering speed of Text layer.
- REFACTORING (Alban): Move the draw and save of the image from the MainForm to the ExportImageForm
- BUGFIX #40 (Steffen): If you connect a gray track with a blue track, the sleeper between them should be the white 3034 one. 
- BUGFIX #42 (Larry): CRASH!! if you export a small area after exporting a big area
- BUGFIX #47 (doc_brown): the LDraw header is not following the standard definition
- BUGFIX (Vincent): When exporting an image for the first time AFTER reloading the BBM, the export file name was the name of the folder instead of the name of the BBM
- BUGFIX (Alban): Find/Replace was not working with the named groups from the library (it didn't appear in the list and the replacement did nothing anyway) 
- BUGFIX (Alban): When changing the order of the part lib tabs, the previously selected tab was not reselected
- BUGFIX (Alban): The current selected tab of the Part Lib was not saved when exiting the application, and not reset when restarting.
- BUGFIX (Alban): If you do a flex move on a hinged set with other elements (for example 4728-1), only the flex part are selected, so you can split the set without ungrouping it
- BUGFIX (Alban): Bug in the creation of the library image for some group (for example 4728-1)
- BUGFIX (Alban): The groups are not saved in LDraw format (so Flex part, cannot be reloaded as a group in BB)
- BUGFIX (Alban): Rotate and Delete a group part is not working as for a single brick. Rotate a group now rotate according to the connections, delete a group now select the next connected brick
- BUGFIX (Alban): Now you can duplicate bricks and connect them in the same mouse move
- BUGFIX (Alban): The snapping margin defined in the XML file has no effect for a group
- BUGFIX (Alban): In the Preferences Shortcut tab, adding a shortcut for a key already existing should ask if we need to replace it or associate a new action on the same key.
- BUGFIX (Alban): In the Preferences Shortcut tab, selecting a shortcut in the list updates the combo boxes below (for easy modification of an existing shortcut)
- BUGFIX (Alban): Precision error during the computation of the size for parts with a Hull defined in the xml.
- BUGFIX (Alban): When adding a group from the library, the brick were added in reverse order.
- BUGFIX (Alban): The group of Texts were not correctly duplicated (specially group of group)
- BUGFIX (Alban): When adding a new text, it was added in the back instead of the front like for parts
- BUGFIX (Alban): The style (italic, bold, strikeout, etc...) for texts were not saved.
- BUGFIX (Alban): Make the first display of each tab of the part library faster (especially on Mono which could take several seconds before)
- BUGFIX (Alban): The brick connections were not correctly recomputed after a series of undo

Bug fixed in 1.7.1.0:
- NEW FEATURE (cimddwc): A shortcut key to bring the selection to front or send it to back (page up/page down by default)
- NEW FEATURE (Alban): Add two new cursors for panning and zooming the view (mainly for Mono support reason)
- BUGFIX (ZueriHB): CRASH! When opening some BBM files (when checking if some links should be broken).
- BUGFIX #34 (Alban+Steffen): CRASH! When saving a file with rail tracks in LDraw format (bug introduced in 1.7.0)
- BUGFIX (Alban): CRASH!! If you move and snap a group of track then finally go back to the original snapping in one move, you will have a crash during the next move of a group without grabbing one part
- BUGFIX (Alban): CRASH!! In the download window, crash when attempting to rename a file to download without changing his name.
- BUGFIX (Alban): CRASH!! Press enter key on a part without connection makes the application crash
- BUGFIX (Lesgoss+Daniel): CRASH! When opening the Export Window (negative size computed for the preview bitmap)
- BUGFIX (Alban): In the download window, remove the useless File column and fix a bug about the edition of the file name.
- BUGFIX (Alban): The Find and Replace window was resizeable but controls in it were not resized
- BUGFIX (Alex): The "Restore Default" in the Global options should only restore the default option of the current tab.
- BUGFIX (Alban): The selection/duplication keys were not reset to the default value when clicking the "restore default" button in the Preferences Window
- BUGFIX (Alban): The order of the shortcut key list was switching when the "restore default" button was clicked in the Preferences Window
- BUGFIX (Alban): Cannot open the help file after opening a map file
- BUGFIX (Steve): Double-clicking on a Text Layer brings up the options box for a Brick Layer (in fact the title was incorrect).
- BUGFIX (Alban): Fix some enabling/disabling issues for the toolbar buttons and the menu items
- BUGFIX (Alban): The 'R' and 'L' shortcutkey were inversed
- BUGFIX (Alban): The red rectangle was not drawn on the preview image of the export window at the first display after loading a file
- BUGFIX (Alban): When moving a selection, the brick under the mouse was highlighted even if it was not inside the selection
- BUGFIX (Alban): Fix a bug regarding the color of the grid (the grid colors were transparent in the option window if you modify the transparency of the layer)
- BUGFIX (Alban): The XML files of the parts should be save in UTF-8 (for better linux handling)
- BUGFIX (Ghislain): Under Linux (with Mono) the drag and drop of part is not working
- BUGFIX (Alban): Fix the maximum of bugs under Mono while keeping .NET working:
	- The Error Window was not displayed correctly
	- Patch to catch an exception while loading the 17th part and the following
	- the size of the layer window is bigger than necessary at the launch time, or when you move the horizontal splitter
	- No cursor when editing the name of a Text or Brick layer
	- wrong icon for the drag and drop of a part
	- The text edit window is not displayed correctly and there's no cursor in the Text edit window
	- Part List Window was not displayed correctly
	- No scrollbar for the translator
	- Cannot move or edit text by double-clicking it
	- wrong cursor when pressing SHIFT+LMB
	- The Export picture window is not displayed correctly
	- General Info window: no cursor for editing the first fields
	- Recent Files list was not saved

Bug fixed in 1.7.0.0:
- NEW FEATURE (Alban): support Flex PF track.
- NEW FEATURE (Alban): The application may support dynamically language addition/removal (dll and chm file inside the folder).
- NEW FEATURE (Denis): Feature to search and replace a set of brick type by another one.
- NEW FEATURE (Alban): The connection points are now configurable and extendable in an XML file.
- NEW FEATURE (Alban): Display the general info on top of the map (this can be disabled in the option settings).
- NEW FEATURE (Alban): Display the mouse coordinates in stud in the status bar
- NEW FEATURE (Larry): Display the XML exception when BB cannot load XML files.
- NEW FEATURE (Alban): Add the polarity check for detecting electric shortcut.
- NEW FEATURE (cimddwc): Add the transparency for all the layers (and not only the Area layer)
- NEW FEATURE (Larry): Save the export image settings in the BBM file.
- NEW FEATURE (Alban): You can group/ungroup hierarchically parts and texts.
- NEW FEATURE (Larry): You can create XML files that are actually groups of parts and see them in the Library
- NEW FEATURE (Alban): New secondary zooming/panning method much easier for laptop, that replace CTRL+SHIFT+LMB and ALT+SHIFT+LMB. Now it is SHIFT+LMB and SHIFT+RMB. Mouse Wheel still supported.
- BUGFIX (Larry): CRASH! when opening a BBM file which is write protected.
- BUGFIX (Larry): Remove the offset when copying parts (was handy before, but now seems a bit odd)
- BUGFIX (Larry): The copy/paste of a group of bricks or text didn't copy in the same order
- BUGFIX (Vincent): CRASH! In the preference you could set a sub grid number to 0 or 1, leading to a later crash (when relaunching BlueBrick or editing the grid layer options)
- BUGFIX (Vincent): In the export window, the maximum scale was limited by the size of the total area, not the size of the selected area.
- BUGFIX (Daniel): CRASH! When the export window is minimized.
- BUGFIX (Alban): The saving of TDL file is not perfect (problems with polarity, flags, slopes and altitude). Well Slope is still not handle, but I don't plan to do more for now.
- BUGFIX (Alban): The snapping of connected brick tried to snap to free connection point inside the selection, making the snapping becoming crazy.
- BUGFIX (Alban): Another bug on the snapping of connected brick: now the selected bricks are unlinked from the non selected brick during the mouse down to have a stable snapping.
- BUGFIX (Alban): When applying 2 times in a row a brick duplication with the first duplication rotated, during the second duplication, the rotation of the first one was canceled.
- BUGFIX (Alban): When undoing the deletion of the top layer, the top layer where not replaced on the top
- BUGFIX (Alban): When the last layer deleted was a Brick layer, you could still add parts

Bug fixed in 1.6.1.0:
- NEW FEATURE (Alex & Alban): Add a context menu in the part lib to choose more options: large/small icons, respect proportions, display bubble info
- NEW FEATURE (Alex & Alban): Add a tab in the Global Option for the part lib: now you can choose the back ground color and sort the tabs of the part lib
- BUGFIX (Larry): CRASH! if you add a small brick in the part lib (1 stud wide) BlueBrick was crashing in Export window or when zooming out
- BUGFIX 21 (Alban): The rail snapping is bugged if you change the current connection point in the same drag'n'drop
- BUGFIX (Alban): fix a bug in the error message dialog for list of files that could not be loaded (wrong name and duplicated files in the list)
- BUGFIX (Alban): fix the bug that the ampersome character "&" was not displayed in the status bar
- REFACTORING 17 (Alban): Do not fill the optim combo box in code

Bug fixed in 1.6.0.0:
- NEW FEATURE (Alex & Alban): When you drag'n'drop a connected brick, the dragged brick is rotated for a proper connection
- NEW FEATURE (Alban, Stephan): Highlight even more the grabbed part in a group to help the user understand that this is the snapping part
- NEW FEATURE (Alban): Change the highlight method of the part (remove the ugly squares) and add options to customize the highlight value
- NEW FEATURE (Alban): Move the grid origin with the mouse when the grid layer is selected
- NEW FEATURE (Alex): Remember the window size.
- NEW FEATURE (Richie): The grid step and rotation step could be saved in the preference of the application.
- NEW FEATURE (Alban): Save even more UI status in the preference of the application (Part list visibility, size and position, paint color, toolbar and status bar visibility, split panel position)
- NEW FEATURE (Didier): Add a configurable list of event in a config file
- BUGFIX (Alex): CRASH! In the XML loading code, when you load a part that has a different number of connection in the file and in the part library, you may crash.
- BUGFIX (Alban): CRASH! The opening of the Global Option window was crashing the application if the part library was empty
- BUGFIX (Thomas): A message box appears for the multiple selection key when switching the application to German language on a German Windows OS.
- BUGFIX (Alban): The duplication cursor could appear in wrong situation (when you press duplication key during moving parts/text)
- BUGFIX (Alban): You can now also duplicate brick/text if you press the duplication key after the left mouse key (but before moving)
- BUGFIX (Alban): the area doesn't move if you just move along the Y axis
- REFACTORING (Alban): Move the TD and LDRAW Remap information in the XML part description file
- REFACTORING (Alban): Remove the PartRemap.txt file and put the remap information in the XML part description file

Bug fixed in 1.5.1.0:
- BUG (Stefan): The parts with hull moved at the wrong place after a rotation (basically bug in the rotation algorithm).
- BUG (1000steine): The tooltips for rotate CCW and CW was inverted in the german translation.

Bug fixed in 1.5.0.0:
- NEW FEATURE (Alban, Alex, JB): Fully comprehensive and integrated offline help file in English and Dutch.
- NEW FEATURE (Alban): Added two buttons for "Send To back" and "Bring to Front"
- NEW FEATURE (Alban): Added a warning message when saving in LDR or TDL to notice the user that some data will be lost
- NEW FEATURE (Alban): Added the Author tag in the XML files of the parts
- NEW FEATURE (Alban): Added an auto-restart when the user change the language.
- NEW FEATURE (Alban, Alex, JB): A lot of new parts in the library, and improvment of some old parts, in details:
	* Completely Revised and extended track library (34 GIFs)
	* Completely revised and extended track side buildings library (27 Sets with minifigs and vehicles)
	* Completely new town set additions (46 new sets with minifigs and vehicles)
	* Completely new space set library (4 sets including monorail!)
	* Completely revised duplo parts library (6 GIFs)
	* 9 new baseplates including space
	* 9 new custom bases (incl. US and EU standard size tables)
- BUGFIX (Alban): The saving as a new map do not add the file in the recent list
- BUGFIX (Alex & Alban): Some picture URL are not correct in the HTML exported list (put the URL in XML file).
- REFACTORING (Alban): Move the part description in the XML file of the parts.

Bug fixed in 1.4.0.0:
- NEW FEATURE (Alban): Custom mouse cursors.
- NEW FEATURE (Patrick): A feature to move the area
- NEW FEATURE (Sergio): The italian language is integrated.
- NEW FEATURE (Patrick & Alban): Add new parts in the part library (new road + 48x48 green baseplate)
- BUGFIX (Patrick): The text is unreadable in the text edit box if you choose a small font size.
- BUGFIX (Patrick): If you save a file with a text containing a cariage return when you reload the file and edit it, the return is lost in the edit box.
- BUGFIX (Alban): The export area could be truncated in the exported image for large layout + add a 32 studs margin
- BUGFIX (Alban): When you press the duplicated key (ALT) above a brick/text not in the selection, the selection can be duplicated.
- BUGFIX (Alban): The aplication was not in foreground when the splashscreen disapear.
- BUGFIX (Alban): Some improvment has been made in the spanish and german translation.
- BUGFIX (Alban): Add the @ in front of the path for cross platform support
- BUGFIX (Alban): The style of the text (bold, italic, etc...) were not used on the map
- BUGFIX (Alban): Shortcut conflict between the standard shortcut and the customizable shortcuts (like CTRL+S and S)
- BUGFIX (Alban): Support the repeated shortcut (if you keep pressing a shortcut key)
- BUGFIX (Alban): Improve shortcut support, the repeated rotation is now recorded as only one undoable action (just like the move, also debuggued)
- BUGFIX (Alban): The TD 4.5V level crossing (#153) is missing in the BB part library for compatibility with TrackDesigner
- BUGFIX (Alban): The content of the combobox for the key CTRL, ALT and SHIFT is not localized (because added in code).

Bug fixed in 1.3.0.0:
- NEW FEATURE (Alban): The LDraw 4.5V rail are now fully supported thanks to the new LDRAW parts created recently
- NEW FEATURE (Alban): The Spanish language is integrated
- NEW FEATURE (Alban): The Portuguese language is integrated (not totally finished)
- NEW FEATURE (Alban): The German language is integrated (not totally finished)
- NEW FEATURE (Larry): Possibility to reload the part library through the File menu.
- NEW FEATURE (Alban): Due to the reload feature, the bbm format change again to version 4 (less data to save), so now the connexion position are not saved anymore
- BUGFIX (Elroy): CRASH! If you choose an area cell smaller than 8 in the global option, BB will crash when trying to edit a area property
- BUGFIX (Alban): The open recent file was not checking if the current files was modified
- BUGFIX (Alban): CRASH! if you load an empty tdl file

Bug fixed in 1.2.0.0:
- NEW FEATURE (Alban): Improve compatibility with TD, and implement the SAVE in TDL format
- NEW FEATURE (Alban): Open Recent files menu item in the File menu
- NEW FEATURE (Alban): Add different alignment (left, right, center) for the Text cells
- NEW FEATURE (Alban): I changed some image of the part library, and that will move a little some parts when loading an old file
- NEW FEATURE (Alban): Implement a system to handle the rename of some parts in the part library (to avoid loosing parts when loading an old file)
- BUGFIX (Alban): Crash if you add a road, and then add a normal baseplate just by clicking on it on the library
- BUGFIX (Alban): When creating a new text cell with several lines, move the mouse on it, it was displaying the text with several lines in the status bar
- BUGFIX (Alban): Improve the compatibility with LDRAW when exporting 4.5V or 12V tracks.
- BUGFIX (Alban): When you delete an unknown part that was linked, that doesn't break the links
- BUGFIX (Alban): When you use the "Send to back" or "Bring to front" that break the links
- BUGFIX (Ghislain): Improve the compatibility with Linux

Bug fixed in 1.1.0.0:
- NEW FEATURE (Jeramy): Add shortcuts to add bricks.
- NEW FEATURE (Jeramy): Implement A*, to select the shortest way between two connected part choosen by the user
- NEW FEATURE (Alban): duplicate bricks with ALT+drag
- NEW FEATURE (Alban): Add and save a Z coord in the BBM file for improving the compatibility with LDRAW
- NEW FEATURE (Alban): Add a "Deselect All" function in both the Edit menu and contextual menu (with CTRL+D shortcut)
- NEW FEATURE (Alban): Add "Select All" and "Select Path" in contextual menu
- NEW FEATURE (Alban): A status bar to display the description of the parts and to handle a progress bar used for save/load
- NEW FEATURE (Alban): Splash screen!
- BUGFIX (Bert): CRASH!! One user met a crash while the application is starting because he put a too big texture in the part database, so I add a message bug for the image BlueBrick can't open
- BUGFIX (Sorry I forgot the reporter): The Edit text dialog let you enter an empty text that you can not see anymore on the map. So I disable the ok button if the text is empty and add a default text.
- BUGFIX (Alban): The restore defaut setting in global option was also reseting the language, which is not logical IMO
- BUGFIX (Alban): When you change the language in the global option form it also change the default Author/LUG/Show in the correct language (unless something was set by the user)
- BUGFIX (Alban): Due to a case sensitive search in the library, some part could not be found. Now all the parts are saved upper case in the library.
- BUGFIX (Alban): The rescale of the area should not move the area
- BUGPATCH (Alban): when you open a file, if when you click OK, the ok button was not above the BlueBrick application, then the waitcursor (hourglass) is not displayed. Now I use a progress bar during loading and saving.


Bug fixed in 1.0.0.0:
- NEW FEATURE (Alban): The Preview Export window is now Resizable ! And a simple click now do the same thing as the double click
- NEW FEATURE (Alban): when we delete a single brick, we should select the next connected brick
- NEW FEATURE (Alban): Now you can drag and drop a file in the application to open it
- CHANGE (Alban): generate more picure dynamically on demand to speed up the loading of large map (the move/zoom can be slower the first time on large map)
- BUGFIX (Alban): CRASH!!! open a file, select a part and rotate it, it was crashing.
- BUGFIX (Alban): the french version is now totally translated
- BUGFIX (Alban): if you double-click on a BBM file to launch the application, the file is not opened
- BUGFIX (Alban): CTRL+left click when the selection is empty, select and unselect in the same time
- BUGFIX (Alban): improve the mouse handling with the CTRL key, and change the cursor according to the action performed
- BUGFIX (Alban): you can not move a connected rail, disconnect it and reconnect to another rail in the same drag/drop (you need to release the part, and move again)
- BUGFIX (Alban): when you rotate a linked part with several connexion type, the software doesn't care about the connexion type. Also when you add a brick with different connexion part.
- BUGFIX (Alban): reascaling an area layer should not change the area

Bug fixed in 0.8.2.0:
- NEW FEATURE (Alban): After the loading of a map, center the view on the center of the map
- CHANGE (Alban): Since I optimized the rendering, I increased again the limit for the zoom out.
- BUGFIX (Alban): The connectivity points were not correct after loading a BBM file (this bug appeared in version 0.8.1 after refactoring for the different type of connexion per part).
- BUGFIX (Alban): the load of TDL file was broken (this bug appeared in version 0.8.1 after refactoring for the different type of connexion per part).
- BUGFIX (Alban): Optimize the speed of the rendering, and keep a good interpolation for big scale down (and add an option in the global option to tune it)
- BUGFIX (Alban): Remove the save in the map of the grid/rotation snapping, because it was stupid and difficult to synchronize after the loading (and not undoable)

Bug fixed in 0.8.1.0:
- NEW FEATURE (Alban): Now you can also disable the main grid in the options of the grid layer
- NEW FEATURE (Alban): When loading a TDL files, the generated layers are correctly named
- NEW FEATURE (Alban): refactor the data of the parts to be able to have two kind of connections on the same part.
- BUGFIX (Alban): in the Export Picture window, the selection rectangle was bugged with bricks in the negative positions
- BUGFIX (Alban): The Export picture window now also include the area layer in consideration
- BUGFIX (Alban): The export picture didn't have the right blending mode for the layer which may let think the layer order is not respected
- BUGFIX (Jeramy): When zooming in and out, the level of detail for the studs is incorrect.
- BUGFIX (Jeramy): When I open the parts viewer window, it opens fine, but when I move my cursor into the map area, it disappears under the main window. There is no way for me to keep this window open on top of the main window.

Bug fixed in 0.8.0.0:
- NEW FEATURE (Alban): The part list is now implemented, and you can export in txt or html.
- NEW FEATURE (Didier): Add an item in the Edit menu to edit the options of the current selected layer.
- NEW FEATURE (Didier): Now the background color is saved in the map and I added a menu item to edit the background color of the current map
- NEW FEATURE (Alban): "Send To Back" and "Bring to Front" (to order the part rendering in the same layer) in a context menu
- NEW FEATURE (Alban): You can change the size of the part in the part library and switch between 128x128 and 64x64 with a right click (like in MLCAD) (in fact this feature was disabled before because of a bug, so I fixed the bug and enabled the feature)
- CHANGE (Alban): I change the computation of the zoom, which now keep the same speed if you are near of far, I also increased the limit for the zoom out.
- BUGFIX (Alban): CRASH!! create a new map, select the grid layer, undo 3 times, redo 3 times, then it crash
- BUGFIX (Alban): FREEZE!! by loading certain TDL map, the application could freeze (not responding)
- BUGFIX (Alban): Select the first layer by default after deleting a layer (because we always must have a layer selected)
- BUGFIX (Alban): Now the click to select a brick or a text choose the brick or text in the front, not the one in the back
- BUGFIX (Alban): Now if you change the appearance in global option and choose yes to change your current map, it only record one action in the undo stack
- BUGFIX (Alban): When you edit a text if you resize the window, now the controls are also resized.
- BUGFIX (Alban): The start position of the mouse was not correct when creating an area in the negative part of the map.
- BUGFIX (Alban): fix a bug in compute origin button in the grid option, when some parts are in the negative part of the map.
- BUGFIX (Alban): do not draw the selection in rectangle when the current selected layer is a grid.

Bug fixed in 0.7.0.0:
- NEW FEATURE (Alban): The Area Layer is now implemented including a new paint tool in the toolbar
- NEW FEATURE (Alban): I add all the actions you can perform on the layers in the Edit menu + shortcup for rotating left and right
- NEW FEATURE (Alban): The toolbar buttons and menu items to perform the actions on layer, are now enable/disabled according to the selected layer
- NEW FEATURE (Alban): Add 2 default parameters for the area layer in the global option (of course these parameters can also be tuned by layer) + add a parameter to display the free connexion points
- BUGFIX (Alban): if you click on the current selected layer in the layer stack, it was adding a useless action in the undo stack.
- BUGFIX (Alban): Some monorail parts (points and short curve) have the connexion point not perfectly set.
- BUGFIX (Alban): Some monorail parts are not correctly exported in LDRAW format.

Bug fixed in 0.6.0.0:
- NEW FEATURE (Alban/Larry): Compatibility with Track Designer!!! Now you can load TDL files!!!! (except for the 12V and 4.5V not finished)
- NEW FEATURE (Alban): I added a lot of parts in the library: all the parts that you can find in TrackDesigner + the table parts
- BUGFIX (Alban): If you try to edit the LUG info just after launching the application everything is fine, but if you open a file and then try to edit the LUG info, then the combo list is empty.
- BUGFIX (Alban): Sometimes you could drag and drop a part from the toolbar into the map
- BUGFIX (Alban): Fix several bug about the update of the parts connectivity (sometime some connexion were not detected after an undo or a copy/paste)
- BUGFIX (Alban): Fix another bug on the connectivity, now the connexion can never be stolen, the connxion must be free to connect, and can not be replaced.

Bug fixed in 0.5.0.0:
- NEW FEATURE (Alban): Compatibility with LDRAW. Now you can save/load LDR and MPD files.
- NEW FEATURE (Alban): Add some road baseplate as example with connexion, and manage a different connexion type for the road
- NEW PARTS (Alban): I added some parts in the library (for more tests): 4 road baseplates and the custom half straight and half curve 9V rail.
- BUGFIX (Alban): CRASH!! if you save a map with several brick layer or several text layer, and try to re-open them.
- BUGFIX (Didier/Alban): The yellow dot are still here when you export the map picture.
- BUGFIX (Alban): if you rotate a curve rail the selection frame does not fit exactly the part
- BUGFIX (Alban): When you add a rail or road that close a loop, one connexion point was still free

Bug fixed in 0.4.0.0:
- NEW FEATURE (Didier): The LUG/TLC info in both general info and default value in global option, is now a ComboBox providing a list (maybe not exhaustive) of a lot of LUG/LTC. You can provide your own lug name, or use one from the list. (The lug list is recorded in the LugList.txt file which is customizable).
- NEW FEATURE (Alban): The selected texts and bricks are overlayed with a white transparent frame (make easier to see which item is selected)
- NEW FEATURE (Alban): Selection in rectangle + CTRL (add or remove item to/from the selection with a rectangle)
- BUGFIX (Alban): export in BMP will give you an ugly picture, probably because it is a paletted BMP not a true color one. In fact it was a mismatch in the format, the export in bmp was exporting in gif, and the other export type were also not correct.
- BUGFIX (Didier): In the global options, panel Appearance, the grid subdivision number was said to be in stud, which is not the case.
- BUGFIX (Alban): If you change the current selected layer, this will not be detected as a modification of the file (no * in the title bar, no warning if you close the map) but nevertheless the current selected layer is supposed to be saved in your map.
- BUGFIX (Alban): reset the undo/redo stacks when you create or load a new map
- BUGFIX (Alban): BlueBrick does not support correctly unknow part (if you try to load a map, but some part are missing in the library). It should display a red cross instead

Bug fixed in 0.3.0.0:
- NEW FEATURE (Alban): In the option window, add the default grid size
- NEW FEATURE (Alban): Cut/Copy/Paste of brick and Texts (you can now transfer some brick from one layer to another with a cut/paste)
- NEW FEATURE (Alban): Add more buttons in the toolbar for new map, open, save, cut, copy, paste.
- NEW FEATURE (Alban): CTRL+A to select all the items in the current layer.
- NEW FEATURE (Alban): You can see and edit the Author/LUG/Show/Date/Comment of your map
- BUGFIX (Alban): CRASH!! if you try to move a group of selected bricks, by grabbing them from an empty part (no brick under the mouse)
- BUGFIX (Alban): In the option window: if you choose "Restore Default Settings" then click "Cancel", your old settings are conserved.
- BUGFIX (Didier): when draging a part from the part library and drop it on the border of the main window, then it is not possible to select it. In fact the bug was: when you drag a part from the part library, then move the mouse outside of the main window and release the button, the part is not selectionnable, not undoable, and in fact disapear if you drag another part.
- BUGFIX (Didier): The baseplate 48x48 has the same size as a 32x32 (thank you for reminding me this bug, well since only this part has this size, I was thinking it will reduce the size of all the parts only for this one... What a pity :-( ) But Anyway I fix it, you can tell me if you still think it is ok.

