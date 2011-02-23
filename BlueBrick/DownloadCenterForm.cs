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
		const int SUBITEM_URL_INDEX = 0;
		const int SUBITEM_DEST_INDEX = 1;
		const int SUBITEM_PERCENTAGE_INDEX = 2;

		public DownloadCenterForm()
		{
			InitializeComponent();
		}

		#region event
		private void StartStopButton_Click(object sender, EventArgs e)
		{
			downloadAllTheFile();
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
		}

		private void updatePercentageOfOneFile(int fileIndex, int percentage)
		{
			// get the corresponding download bar subitem
			ListViewItem.ListViewSubItemCollection subitems = this.DownloadListView.Items[fileIndex].SubItems;
			if (percentage > 100)
				percentage = 100;

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
			subitems[SUBITEM_PERCENTAGE_INDEX].ForeColor = ComputeColorFromPercentage(percentage);
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
			// just call the download on the first file, and then in the event of the background worker complete
			// it will be called again on the next files.
			downloadOneFile(0);
		}

		private void downloadBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			// the result contains the index of the next file
			downloadOneFile((int)(e.Result));
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
			updatePercentageOfOneFile((int)(e.UserState), e.ProgressPercentage);
		}
		#endregion

		#region tool function
		public static Color ComputeColorFromPercentage(double percentage)
		{
			if (percentage < 0.0)
			{
				return Color.FromArgb(255, 0, 200, 0);
			}
			else if (percentage > 100.0)
			{
				return Color.Red;
			}
			else
			{
				const double PERCENTAGE_GAP = 7.5;
				const double RED_SLOPE = (255 / (50.0 + PERCENTAGE_GAP));
				const double GREEN_SLOPE = (200 / (50.0 + PERCENTAGE_GAP));

				// compute the red color
				int redColor = 0;
				if (percentage <= (50.0 + PERCENTAGE_GAP))
					redColor = (int)(RED_SLOPE * percentage);
				else
					redColor = 255;

				// compute the green color
				int greenColor = 0;
				if (percentage >= (50.0 - PERCENTAGE_GAP))
					greenColor = 200 - (int)(GREEN_SLOPE * (percentage - (50.0 - PERCENTAGE_GAP)));
				else
					greenColor = 200;

				return Color.FromArgb(0xFF, redColor, greenColor, 0x00);
			}
		}
		#endregion
	}
}