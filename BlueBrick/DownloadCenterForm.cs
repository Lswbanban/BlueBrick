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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.Threading;

namespace BlueBrick
{
	public partial class DownloadCenterForm : Form
	{
		const int SUBITEM_PERCENTAGE_INDEX = 0;
		const int SUBITEM_URL_INDEX = 1;
		const int SUBITEM_DEST_INDEX = 2;
		const int NUMBER_OF_STEP_PER_FILE_FOR_TOTAL_PROGRESS_BAR = 10;

		public DownloadCenterForm()
		{
			InitializeComponent();
		}

		#region event
		private void StartStopButton_Click(object sender, EventArgs e)
		{
			// disable this button
			this.StartButton.Enabled = false;
			// set the hourglass except for the cancel button
			this.Cursor = Cursors.WaitCursor;
			this.CancelButton.Cursor = Cursors.Default;
			// launch the download
			downloadAllTheFile();
		}

		private void DownloadCenterForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			// cancel the background download thread if the cancel button is pressed.
			this.downloadBackgroundWorker.CancelAsync();
			// re-enable the start button
			this.StartButton.Enabled = true;
			// reset the default cursor
			this.Cursor = Cursors.Default;
		}
		#endregion

		#region ListView
		public void fillListView(List<string[]> fileList)
		{
			// start of the update of the control
			this.DownloadListView.BeginUpdate();

			// item count to take the item one by one in order
			int itemIndex = 0;
			this.DownloadListView.Items.Clear();

			foreach (string[] file in fileList)
			{
				// create an item
				ListViewItem item = new ListViewItem(file);
				// add it to the list
				this.DownloadListView.Items.Add(item);
				// call the update of the percentage for updating the color
				updatePercentageOfOneFile(itemIndex, 0);
				// inc the index
				itemIndex++;
			}

			// end of the update of the control
			this.DownloadListView.EndUpdate();

			// set the parameter of the progress bar depending on the total number of files to download
			if (fileList.Count > 0)
				this.TotalProgressBar.Maximum = (fileList.Count * NUMBER_OF_STEP_PER_FILE_FOR_TOTAL_PROGRESS_BAR);
		}

		private void updatePercentageOfOneFile(int fileIndex, int percentage)
		{
			// get the corresponding download bar subitem
			ListViewItem.ListViewSubItemCollection subitems = this.DownloadListView.Items[fileIndex].SubItems;

			// add the percentage bar
			string percentageString = "";
			int nbTenth = (int)(percentage * 0.1);
			if (nbTenth > 10)
				nbTenth = 10;
			for (int i = 0; i < nbTenth; ++i)
				percentageString += char.ConvertFromUtf32(0x2588);
			if ((nbTenth < 10) && ((percentage - (nbTenth * 10)) >= 5.0))
			{
				percentageString += char.ConvertFromUtf32(0x258C);
				++nbTenth;
			}
			int nbSpaces = 10 - nbTenth;
			for (int i = 0; i < nbSpaces; ++i)
				percentageString += char.ConvertFromUtf32(0x2550);

			subitems[SUBITEM_PERCENTAGE_INDEX].Text = percentageString + " " + percentage.ToString();

			// change the color according to the percentage value
			subitems[SUBITEM_PERCENTAGE_INDEX].ForeColor = ComputeColorFromPercentage(100 - percentage, false);
		}
		#endregion

		#region download
		private class DownloadParameter
		{
			public string url = string.Empty;
			public string destination = string.Empty;
			public int fileIndex = 0;
		}

		private void downloadAllTheFile()
		{
			// reset the total progress bar
			this.TotalProgressBar.Value = 0;
			// just call the download on the first file, and then in the event of the background worker complete
			// it will be called again on the next files.
			downloadOneFile(0);
		}

		private void downloadBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			// the result contains the index of the next file
			downloadOneFile((int)(e.Result));
		}

		private void downloadComplete()
		{
			// reset the default cursor
			this.Cursor = Cursors.Default;
			// Hide the Cancel Button and show the close button
			this.CancelButton.Hide();
			this.CloseButton.Show();
		}

		private void downloadOneFile(int fileIndex)
		{
			// check if we reach the end of the list
			if (fileIndex < this.DownloadListView.Items.Count)
			{
				// get the URL and destination and create a parameter for async work
				ListViewItem.ListViewSubItemCollection subitems = this.DownloadListView.Items[fileIndex].SubItems;
				DownloadParameter parameters = new DownloadParameter();
				parameters.url = subitems[SUBITEM_URL_INDEX].Text;
				parameters.destination = System.Windows.Forms.Application.StartupPath + subitems[SUBITEM_DEST_INDEX].Text;
				parameters.fileIndex = fileIndex;

				// start the download asynchronously by giving the parameters
				downloadBackgroundWorker.RunWorkerAsync(parameters);
				// this method will be called again when the background worker will send it complete event
			}
			else
			{
				// if we reach the end of the list, the download is complete
				downloadComplete();
			}
		}

		/// <summary>
		/// Be careful, this method is called from another Thread. DO NOT call any control method in it.
		/// Instead use the 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void downloadBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			const int BUFFER_SIZE = 1024;
			// Get the BackgroundWorker that raised this event.
            BackgroundWorker worker = sender as BackgroundWorker;
			// and get the parameters
			DownloadParameter parameters = e.Argument as DownloadParameter;

			// create a http request
			HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(parameters.url);
			// Set some reasonable limits on resources used by this request
			request.MaximumAutomaticRedirections = 4;
			request.MaximumResponseHeadersLength = 4;
			request.Credentials = CredentialCache.DefaultCredentials;

			try
			{
				// get the response
				HttpWebResponse response = (HttpWebResponse)(request.GetResponse());
				// Get the stream associated with the response.
				Stream receiveStream = response.GetResponseStream();
				// Pipes the stream to a higher level stream reader with the required encoding format. 
				BinaryReader readStream = new BinaryReader(receiveStream, Encoding.UTF8);

				// create a file stream to save the file
				FileStream file = new FileStream(parameters.destination, FileMode.Create);
				BinaryWriter binaryWriter = new BinaryWriter(file);

				long nbLoop = (response.ContentLength / BUFFER_SIZE) + 1;
				for (long i = 0; i < nbLoop; i++)
				{
					binaryWriter.Write(readStream.ReadBytes(BUFFER_SIZE));
					// compute the download percentage
					int downloadPercentage = (int)(((BUFFER_SIZE * 100) * i) / response.ContentLength);
					// call the report progress method that will send an event on the thread of the form to update the progress bar
					worker.ReportProgress(downloadPercentage, parameters.fileIndex);
				}

				// close the binary writer and the file
				binaryWriter.Close();
				file.Close();

				// close the respons and read Stream
				response.Close();
				readStream.Close();

				// if the download was ok, the result is the index of the next file
				e.Result = parameters.fileIndex + 1;
			}
			catch (WebException exeption)
			{
				exeption.Response.Close();

				// if the download had problems, try to download again
				e.Result = parameters.fileIndex;
			}
		}

		/// <summary>
		/// This method is called from the thread that create the form, so it's safe to access controls of the form
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void downloadBackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			// get the parameters
			int fileIndex = (int)(e.UserState);
			int percentage = Math.Min(e.ProgressPercentage, 100); // clamp the value to 100
			// update the progress bar of one file
			updatePercentageOfOneFile(fileIndex, percentage);
			// update also the total progress bar
			this.TotalProgressBar.Value = (fileIndex * NUMBER_OF_STEP_PER_FILE_FOR_TOTAL_PROGRESS_BAR) +
											(percentage / NUMBER_OF_STEP_PER_FILE_FOR_TOTAL_PROGRESS_BAR);
		}
		#endregion

		#region tool function
		/// <summary>
		/// Compute a gradient color from green to red (if shouldGoToRed is true) or from green to Black depending
		/// on the percentage value given in parameter
		/// </summary>
		/// <param name="percentage">a value between 0 to 100 to compute the gradient of color</param>
		/// <param name="shouldGoToRed">if true, the color will be red when percent == 100</param>
		/// <returns></returns>
		public static Color ComputeColorFromPercentage(int percentage, bool shouldGoToRed)
		{
			if (percentage < 0.0)
			{
				return Color.FromArgb(255, 0, 200, 0);
			}
			else if (percentage > 100.0)
			{
				if (shouldGoToRed)
					return Color.Red;
				else
					return Color.Black;
			}
			else
			{
				// the value of green and red
				int redColor = 0;
				int greenColor = 0;

				if (shouldGoToRed)
				{
					const double PERCENTAGE_GAP = 7.5;
					const double RED_SLOPE = (255 / (50.0 + PERCENTAGE_GAP));
					const double GREEN_SLOPE = (200 / (50.0 + PERCENTAGE_GAP));

					// compute the red color
					if (percentage <= (50.0 + PERCENTAGE_GAP))
						redColor = (int)(RED_SLOPE * percentage);
					else
						redColor = 255;

					// compute the green color
					if (percentage >= (50.0 - PERCENTAGE_GAP))
						greenColor = 200 - (int)(GREEN_SLOPE * (percentage - (50.0 - PERCENTAGE_GAP)));
					else
						greenColor = 200;
				}
				else
				{
					// the red component stay null
					// linear inc for the green
					greenColor = 255 - (int)(2.55 * percentage);
				}

				return Color.FromArgb(0xFF, redColor, greenColor, 0x00);
			}
		}
		#endregion
	}
}