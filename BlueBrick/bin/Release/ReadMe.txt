                    +---------------------------+
                    | BlueBrick version 1.5.0.0 |
                    +---------------------------+

-----------------------------------------------------------------------------------------------------------------------------------------------------
1) License
-----------------------------------------------------------------------------------------------------------------------------------------------------

BlueBrick is a free software for Windows/Vista developped by Alban Nanty in open source. You can use it for your personnal purpose but not for commercial profit. You can redistribute it under the terms of the GNU General Public License version 3 (http://www.gnu.org/licenses/) as published by the Free Software Foundation (http://www.fsf.org/licensing/licenses/gpl.html). This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY.

This software was designed specially for the AFOLs who want to prepare the layouts of their LEGO(c) exhibitions. BlueBrick is compatible with LDRAW (http://www.ldraw.org/) and "Train Depot Track Designer" (http://www.ngltc.org/Train_Depot/td.htm) and was designed to extend easily its part database. Its layers feature allow you to better organize your map, and some specific layers make possible the addition of annotation and area assignment.


------------------------------------------------------------------------------------------------------------------------------------------------------
2) Install
------------------------------------------------------------------------------------------------------------------------------------------------------

To run BlueBrick, you should download and install the Microsoft .NET FrameWork version 2.0, that can be found on the
Microsoft website:

Microsoft .NET FrameWork version 2.0
http://www.microsoft.com/downloads/details.aspx?FamilyID=0856eacb-4362-4b0d-8edd-aab15c5e04f5&DisplayLang=en

Microsoft .NET FrameWork version 3.0
http://www.microsoft.com/downloads/thankyou.aspx?familyId=10cc340b-f857-4a14-83f5-25634c3bf043&displayLang=en

The version 2.0 is enough to run BlueBrick and I suggest you to install this one, since the install of the version 3.0 seems more complex.

Then extract the files contained in the zip, somewhere on your hardrive, by keeping the same folders organization.

------------------------------------------------------------------------------------------------------------------------------------------------------
3) Quick Reference
------------------------------------------------------------------------------------------------------------------------------------------------------

The window is split in 3: on the left the map panel where you build your layout, on the top right the part library panel, and on the bottom right the layers panel.

3.1) Layer panel

The layer panel contains a stack of layers, that are rendered from the bottom to the top.
- you can select a layer by clicking on it
- by clicking on the eye button of a layer, you can show/hide
- by double-cliking on a layer, you open an option window to tune parameters of the layer (like its name for example)
- you can create new layers by cliking on the small button under the stack
- you can delete a layer by cliking on the small trashbin under the stack
- you can reorder the layers by clicking on the arrow under the stack

One important thing in BlueBrick: the layers are typed. There are 4 type of layers: Text, Grid, Area and Brick. You can only add bricks in a brick layer and text in a text layer.

3.2) The part library

The part library is separate in different tabs. Each tab corresponds to a folder in the "parts" folder of the BlueBrick install folder. The folders are parsed at the launch of BlueBrick. You can add your own folders, and parts as you want. The convention is that a STUD should be represented by 8 pixels, and only GIF files are taken into account by the software (other format are ignored).

To add a brick on you map, just drag and drop a part on the map, but of course a Brick layer should be selected. If you see a interdiction cursor while dragging it is probably because the current selected layer is not a brick layer.

If a brick (and only one) is selected on the map, you can just click one time on a part in the part library, to add this part next to the selected one. This is working for a baseplate (the new part is added on the right) and also for a rail part (which is more usefull because the new part is added to the current connexion point (red circle)).

3.3) The map

BlueBrick is a single document application, you can not work on different map at the same time.

Here are the key to navigate on the map:
- left mouse click: select a part or a text and move it, or draw an area
- drag'n'drop with left mouse button: select parts or texts in rectangle, or draw an area
- CTRL + left mouse click: add or remove an item to/from the current selection, also working in rectangle
- mouse wheel: zoom in/out
- mouse wheel button + move: pan the view
- CTRL + SHIFT + left mouse click + move: zoom in/out
- ALT + SHIFT + left mouse click + move: pan the view

Note: the two last shortcuts may be usefull if you use a notebook without any mouse wheel.

Buttons in the toolbar:
- undo/redo (by clicking on the little down triangle, you can undo/redo several actions)
- trashbin: delete the current selection
- grid snapping: by cliking on the button enable/disbale the snapping. By cliking on the little down triangle, choose the step of the grid snapping
- rotation step: by cliking on it choose the step of the rotation (the rotation can not be enable/disable)
- left rotation: rotate the selection to the left by the step angle
- right rotation: rotate the selection to the right by the step angle
- paint tool: you can choose between the pencil or the eraser by clicking on the little down triangle ; clicking directly on the button can change the color

Usefull Shortcut key:
- CTRl+Z: undo
- CTRL+Y: redo
- CTRL+S: save
- DEL or BACKSPACE: delete the current selection
- CTRL+A: select all items in the current layer

The selection is only sensitive to the current selected layer.

To add a new text in your map, add a text layer (or select a text layer), then double-click somewhere on the map. Type your text in the opened window and choose OK. To edit a text in the map, double click on it.

For the parts that have connexion points (like rail), a small dot appears on each of their free connexion. If you select a single rail part, a red dot is also displayed: this is the current selected connexion point. By cliking inside this part, you can change the current selected connection point.

Moving a group of part (drag'n'drop selected parts): if the grid snapping is disable, it's a free move. If it is enable it depend where you grabed the group of parts. The part that is under the mouse is the master part for the snapping (this part only will be snapped on the grid, the other part keep their relative position according to the master part). If there's no part under the mouse, the snapping is relative to their original position. If the master part is a part with connexion points, then this master part try to connect to the free connection points around the mouse. Note that the snapping on the connexion do not care about the orientation of the part.

If you try to rotate a rail that is connected, it will not rotate according to the rotation step, but it will rotate from one connection to the other.

3.4) Save/Export

The default extension for BlueBrick map file is BBM and in fact is XML (can be open with a text editor). You can also save in LDR or MPD format (LDRAW) but of course some information (like the texts, the area, the grid...) will be lost. You can load BBM, LDR, MPD and TDL format.

To export, go in the File menu, or CTRL+E: an export window appears. You can choose the area you want to export by drag/drop. A simple click in the area choose the whole area. Specify the size of the image that you want to export (or specify a scale, and the size is automatically computed from your chosen area). Then click ok and enter a file name. You can export your image in BMP, GIF, JPG or PNG.

3.5) Part list

In the View menu you can show/hide the part list. The part list is in a seperate window and is updated dynamically while you are editing the map. There's an option to split the list by layer or not. You can also export the part list in TXT or HTML format.

------------------------------------------------------------------------------------------------------------------------------------------------------
4) History
------------------------------------------------------------------------------------------------------------------------------------------------------

Open bugs:
- MISSING FEATURE (Alban): Help
- MISSING FEATURE (Denis): Feature to replace a set of brick by another one.
- MISSING FEATURE (Alban): Compatibility with TrackDraw?
- MISSING FEATURE (Alban): Split the toolbar in two (one for file, one for edition) + add a new toolbar for navigation
- MISSING FEATURE (Alban): Add two buttons for "Send To back" and "Bring to Front"
- MISSING FEATURE (Alex & Alban): When you drag'n'drop a connected brick, the dragged brick is not rotated for a proper connection
- MISSING FEATURE (Alban): Add a warning message box when saving in LDR or TDL and some data will be lost
- MISSING FEATURE (Patrick): Add an optionnal part outline to see more clearly the different parts when exporting the image
- MISSING FEATURE (Loys): Support Multi-Document
- BUG (Alban): The saving of TDL file is not perfect (problems with polarity, flags, slopes and altitude)
- BUG (Ghislain): Under Linux the drag and drop of part is not working
- BUG (Loys): wrong keyboard setup in the save file dialog when exporting an image
- BUG (Didier & Alexander): SEEMS UNFIXABLE: The standard windows (choose date, color picker, save/open dialog) are spawn in the language of the OS, not the one selected in the application.
- BUG (Alex & Alban): Some picture URL are not correct in the HTML exported list (put the URL in XML file).
- BUG (Alex): The "Restore Default" in the Global options should only restore the default option of the current tab.
- REFACTORING (Alban): Do not fill the optim combo box in code

Bug fixed in 1.5.0.0:
- BUG (Alban): The saving as a new map do not add the file in the recent list

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

