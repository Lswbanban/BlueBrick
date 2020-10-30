// BlueBrick, a LEGO(c) layout editor.
// Copyright (C) 2008 Alban NANTY
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, version 3 of the License.
// see http://www.fsf.org/licensing/licenses/gpl.html
// and http://www.gnu.org/licenses/
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Threading;

namespace BlueBrick
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			try
			{
				// this two method must be called before the first creation of window
				Application.EnableVisualStyles();
				Application.SetCompatibleTextRenderingDefault(false);

				// create the splash screen and show it (in the main thread)
				SplashScreen splashScreen = new SplashScreen();
				splashScreen.Show();
				Application.DoEvents(); // call the DoEvent for paint event to be done

				// try to load the language saved in the setting,
				// if the setting is set to default, the window culture info is used
				// we should do that before creating the MainForm to have the menu in
				// the right language
				string language = BlueBrick.Properties.Settings.Default.Language;
				if (!language.Equals("default"))
				{
					// create a new culture info based on the property
					System.Globalization.CultureInfo cultureInfo = new System.Globalization.CultureInfo(language);
					// change the culture of the resources and the UI
					BlueBrick.Properties.Resources.Culture = cultureInfo;
					System.Threading.Thread.CurrentThread.CurrentUICulture = cultureInfo;
				}

				// the application can be launched with one argument, which should be a name of a file
				// that we should open. If you double-click on a BlueBrick document (or choose the "open with..."
				// in the right click context menu) the Windows will launch the program and give the filename
				// (full path in parameter)
				string fileToOpen = null;
				if (args.Length > 0)
					fileToOpen = args[0];

				// Create the mainWindow (which is the task that take time)
				MainForm mainWindow = new MainForm(fileToOpen);

				// when the mainwindow constructor is finished, we can hide the splashscreen and destroy it
				splashScreen.Hide();
				splashScreen.Dispose();

				// Finally call the main loop
				Application.Run(mainWindow);
			}
			catch (Exception e)
			{
				// final catch for exeption
				string message = Properties.Resources.ErrorMsgCrash;
				message += e.ToString();
				MessageBox.Show(null, message, Properties.Resources.ErrorMsgTitleError, MessageBoxButtons.OK,
					MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);

				// save the map if there's unsaved changed
				if (MapData.Map.Instance.WasModified)
				{
					// construct a file name for the save
					string backupFileNameEnd = "_autosave.bbm";
					string backupFullFileName = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + System.IO.Path.DirectorySeparatorChar + backupFileNameEnd;
					if (MapData.Map.Instance.IsMapNameValid)
					{
						System.IO.FileInfo fileInfo = new System.IO.FileInfo(MapData.Map.Instance.MapFileName);
						backupFullFileName = fileInfo.FullName.Remove(fileInfo.FullName.LastIndexOf(fileInfo.Extension)) + backupFileNameEnd;
					}

					// call the save function (which can fail for some reason, in such case a flase flag will be returned)
					bool saveDone = SaveLoadManager.save(backupFullFileName);

					// inform the player of the save
					if (saveDone)
						MessageBox.Show(null, Properties.Resources.ErrorMsgAutosaveMap.Replace("&", backupFullFileName), Properties.Resources.ErrorMsgTitleInfo, MessageBoxButtons.OK,
							MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
				}
			}
		}
	}
}