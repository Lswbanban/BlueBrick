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
		private static void SplashScreenLoop()
		{
			SplashScreen  splashScreen = new SplashScreen();
			splashScreen.Show();
			while (!MainForm.IsMainFormReady)
			{
				Thread.Sleep(200);
			}
			MainForm.CheckForIllegalCrossThreadCalls = false;
			MainForm.Instance.BringToFront();
			MainForm.CheckForIllegalCrossThreadCalls = true;
			splashScreen.Hide();
			splashScreen.Dispose();
		}

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

				// create a separate thread for the splash screen
				ThreadStart threadDelegate = new ThreadStart(SplashScreenLoop);
				Thread newThread = new Thread(threadDelegate);
				newThread.IsBackground = true;
				newThread.Start();

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

				Application.Run(new MainForm(fileToOpen));
			}
			catch (Exception e)
			{
				// final catch for exeption
				string message = Properties.Resources.ErrorMsgCrash;
				message += e.ToString();
				MessageBox.Show(null, message,
					Properties.Resources.ErrorMsgTitleError, MessageBoxButtons.OK,
					MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
			}
		}
	}
}