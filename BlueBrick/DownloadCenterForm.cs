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
		const int SUBITEM_DEST_INDEX = 0;
		const int SUBITEM_URL_INDEX = 1;
		const int SUBITEM_PERCENTAGE_INDEX = 2;
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

		private void DownloadListView_AfterLabelEdit(object sender, LabelEditEventArgs e)
		{
			// make sure the label start with the folder separator
			if (!e.Label.StartsWith(@"\"))
			{
				e.CancelEdit = true;
				this.DownloadListView.Items[e.Item].Text = @"\" + e.Label;
			}
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
				item.Checked = true; // by default we download all the files
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
		/// <summary>
		/// a small container to provide parameters to the background worker thread
		/// </summary>
		private class DownloadParameter
		{
			public string url = string.Empty;
			public string destination = string.Empty;
			public int fileIndex = 0;
		}

		/// <summary>
		/// a small container to send back information from the background thread
		/// </summary>
		private class ResultParameter
		{
			public int fileIndex = 0;
			public bool hasErrorOccurs = false;

			public ResultParameter(int index, bool error)
			{
				fileIndex = index;
				hasErrorOccurs = error;
			}
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
			// get the result object
			ResultParameter result = (e.Result) as ResultParameter;

			// check if there was an error to change the color
			if (result.hasErrorOccurs)
				this.DownloadListView.Items[result.fileIndex].SubItems[SUBITEM_PERCENTAGE_INDEX].ForeColor = Color.Red;

			// then download the next file
			downloadOneFile(result.fileIndex + 1);
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
		private void downloadBackgroundWorker_DoWork(object sender, DoWorkEventArgs eventArgs)
		{
			// Get the BackgroundWorker that raised this event.
            BackgroundWorker worker = sender as BackgroundWorker;
			// and get the parameters
			DownloadParameter parameters = eventArgs.Argument as DownloadParameter;

			// create a http request
			HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(parameters.url);
			// Set some reasonable limits on resources used by this request
			request.MaximumAutomaticRedirections = 4;
			request.MaximumResponseHeadersLength = 4;
			request.Credentials = CredentialCache.DefaultCredentials;

			try
			{
				// get the response
				HttpWebResponse response = request.GetResponse() as HttpWebResponse;
				// check if we get a 404 error redirected by the server on a 404 web page
				// in that case the getResponse will not throw an error
				if (!response.ResponseUri.AbsoluteUri.Equals(parameters.url))
					throw new WebException(String.Empty, null, WebExceptionStatus.UnknownError, response);

				// Pipes the stream associated with the response to a higher level stream reader
				BinaryReader readStream = new BinaryReader(response.GetResponseStream());

				// create a file stream to save the file
				FileStream file = new FileStream(parameters.destination, FileMode.Create);
				BinaryWriter binaryWriter = new BinaryWriter(file);

				// write in buffer the data read from the stream reader
				const int BUFFER_SIZE = 1024;
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

				// close read stream and the response
				readStream.Close();
				response.Close();

				// save the result object in the result property
				eventArgs.Result = new ResultParameter(parameters.fileIndex, false);
			}
			catch (WebException e)
			{
				if (e.Response != null)
					e.Response.Close();
				// return the file index with the error code
				eventArgs.Result = new ResultParameter(parameters.fileIndex, true);
			}
			// the following exception will normally never happen, however, we catch them and ignore them to not
			// crash the application
			catch (InvalidOperationException)
			{
				// The stream is already in use by a previous call to BeginGetResponse.
				// -or- 
				// TransferEncoding is set to a value and SendChunked is false. 

				// Method is GET or HEAD, and either ContentLength is greater or equal to zero or SendChunked is true.
				// -or- 
				// KeepAlive is true, AllowWriteStreamBuffering is false, ContentLength is -1, SendChunked is false, and Method is POST or PUT. 

				// return the file index with the error code
				eventArgs.Result = new ResultParameter(parameters.fileIndex, true);
			}
			catch (NotSupportedException)
			{
				// The request cache validator indicated that the response for this request can be served from
				// the cache; however, this request includes data to be sent to the server. Requests that send
				// data must not use the cache. This exception can occur if you are using a custom cache validator
				// that is incorrectly implemented. 

				// return the file index with the error code
				eventArgs.Result = new ResultParameter(parameters.fileIndex, true);
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