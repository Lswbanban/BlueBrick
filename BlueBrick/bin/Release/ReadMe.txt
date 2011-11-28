                    +---------------------------+
                    | BlueBrick version 1.7.0.0 |
                    +---------------------------+

-----------------------------------------------------------------------------------------------------------------------------------------------------
1) License
-----------------------------------------------------------------------------------------------------------------------------------------------------

BlueBrick is a free software for Windows/Vista developped by Alban Nanty in open source. You can use it for your personnal purpose but not for commercial profit. You can redistribute it under the terms of the GNU General Public License version 3 (http://www.gnu.org/licenses/) as published by the Free Software Foundation (http://www.fsf.org/licensing/licenses/gpl.html). This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY.

This software was designed specially for the AFOLs who want to prepare the layouts of their LEGO(c) exhibitions. BlueBrick is compatible with LDRAW (http://www.ldraw.org/) and "Train Depot Track Designer" (http://www.ngltc.org/Train_Depot/td.htm) and was designed to extend easily its part database. Its layers feature allow you to better organize your map, and some specific layers make possible the addition of annotation and area assignment.

------------------------------------------------------------------------------------------------------------------------------------------------------
2) Install
------------------------------------------------------------------------------------------------------------------------------------------------------

If your OS is Vista or Windows 7, nothing special need to be install first. If your OS is Windows XP, you need to download and install the Microsoft .NET FrameWork (at least version 2.0), that can be found on the Microsoft website (if not already install on your machine):

Microsoft .NET FrameWork version 3.5 SP1
http://msdn.microsoft.com/en-us/netframework/aa569263.aspx

or Microsoft .NET FrameWork version 2.0
http://www.microsoft.com/downloads/details.aspx?FamilyID=0856eacb-4362-4b0d-8edd-aab15c5e04f5&DisplayLang=en

The version 2.0 is enough to run BlueBrick.

Then extract the files contained in the zip, somewhere on your hardrive, by keeping the same folders organization.

------------------------------------------------------------------------------------------------------------------------------------------------------
3) Help
------------------------------------------------------------------------------------------------------------------------------------------------------

Please refer to the help file that comes with the application (menu Help > Content and Index...)

------------------------------------------------------------------------------------------------------------------------------------------------------
4) History
------------------------------------------------------------------------------------------------------------------------------------------------------

Open bugs:
- [target 1.8] MISSING FEATURE (FreeBee): Snap through layers (handy if you have multiple layers of track to connect)
- [target 1.8] MISSING FEATURE (Larry): Select parts in multiple layers
- [target 1.8] MISSING FEATURE (Erik): A new layer type for mesurement tools.
- [target 1.8] MISSING FEATURE (Alban): A feature to check if there's new parts available online.
- [target 1.8] MISSING FEATURE (Alban): A feature to save a group that was created in BlueBrick, in the library
- MISSING FEATURE (Alban): Enhance layer stack usability: add a context menu to delete a layer, allow drag'n'drop reorganization.
- MISSING FEATURE (Alban): Enhance the part list export in HTML with some javascript for making the image optionnal.
- MISSING FEATURE (Alban): Enhance the part list export in HTML to export the group set only one time.
- MISSING FEATURE (Alban): Support mouse wheel in the library and layer panels.
- MISSING FEATURE (Alban): Add a filter combobox in the bottom of the library
- MISSING FEATURE (Alban): Autosave.
- MISSING FEATURE (Alban): Split the toolbar in two (one for file, one for edition) + add a new toolbar for navigation
- MISSING FEATURE (Patrick): Add an optionnal part outline to see more clearly the different parts when exporting the image
- MISSING FEATURE (Loys): Support Multi-Document or allow copy paste between instance (or after a load) -> use clipboard for that
- MISSING FEATURE (Alban): Compatibility with TrackDraw?
- MISSING FEATURE (FreeBee): Mirroring selected (just like rotating) -> almost impossible to do
- MISSING FEATURE (Alban): The selection should be an undoable action
- MISSING FEATURE (Alban): Slide bars on the map view?
- [target 1.8] BUG (Alban): In the Preferences Shortcut tab, adding a shortcut for a key already existing should replace it (and the selection should modify the combo boxes)
- [target 1.8] BUG (Alban): The groups are not saved in LDraw format (so Flex part, cannot be reloaded as a group in BB)
- [target 1.8] BUG (Alban): Rotate a group part and Delete a group part is not fully working (should remove Count == 1 in the code)
- [target 1.8] BUG (Alban): The snapping margin defined in the XML file has no effect for a group
- [target 1.8] BUG (Alban): For some mice, the middle button doesn't work all the time.
- [target 1.8] BUG (Alban): Bug in the creation of the library image for some group (for example 4728-1)
- [target 1.8] BUG (Alban): If you do a flex move on a hinged set with other elements (for example 4728-1), only the flex part are selected, so you can split the set without ungrouping it
- [target 1.7.1] BUG (Alban): The Find and Replace window is resizable but controls in it doesn't resize
- [target 1.7.1] BUG (Alban): Fix the maximum of bugs under Mono while keeping dot net working:
	- The Error Window was not displayed correctly
	- Patch to catch an exception while loading the 17th part and the following
	+++ drag and drop of file is not working (and confusing with the drag and drop of parts)
	++ wrong icon for the drag and drop of a part into a text layer
	++ The text edit window is not displayed correctly
	+++ Cannot move or edit text by double-clicking it
	+ No cursor when editing the name of a Text layer
	+ No cursor in the Text edit window
	++ Part List Window was not displayed correctly
	++ The Export picture window is not displayed correctly
	++ the size of the layer window is bigger than necessary at the launch time
	+ wrong cursor when pressing SHIFT+LMB
	+++ press enter on a part without connection (crash exit)
	+ General Info window: no cursor for editing the first fields
	++ No scrollbar for the translator and strange title color in the About Box
- BUG (Loys): wrong keyboard setup in the save file dialog when exporting an image
- BUG (Didier & Alexander): SEEMS UNFIXABLE: The standard windows (choose date, color picker, save/open dialog) are spawn in the language of the OS, not the one selected in the application.
- REFACTORING (Alban): Check where I could have used the directive "where" (remove code duplication)

Bug fixed in 1.7.1.0:
- BUGFIX (ZueriHB): CRASH! When opening some BBM files (when checking if some links should be broken).
- BUGFIX (Alban): CRASH!! In the download window, crash when attempting to rename a file to download without changing his name.
- BUGFIX (Alban): In the download window, remove the useless File column and fix a bug about the edition of the file name.
- BUGFIX (Alban): The XML files of the parts should be save in UTF-8 (for better linux handling)
- BUGFIX (Alex): The "Restore Default" in the Global options should only restore the default option of the current tab.
- BUGFIX (Alban): The selection/duplication keys were not reset to the default value when clicking the "restore default" button in the Preferences Window
- BUGFIX (Alban): The order of the shortcut key list was switching when the "restore default" button was clicked in the Preferences Window
- BUGFIX (Ghislain): Under Linux (with Mono) the drag and drop of part is not working

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

