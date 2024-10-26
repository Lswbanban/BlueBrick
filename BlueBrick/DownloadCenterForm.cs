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
using System.Drawing;
using System.Windows.Forms;
using System.Net;
using System.IO;

namespace BlueBrick
{
	public partial class DownloadCenterForm : Form
	{
		/// <summary>
		/// A convenient class to sumarize the all info regarding a file to download, such as the source URL,
		/// the local destination folder, and so on.
		/// </summary>
		public class DownloadableFileInfo
		{
			public string FileName = string.Empty;
			public string Version = string.Empty;
			public string SourceURL = string.Empty;
			public string DestinationFolder = string.Empty;

			public DownloadableFileInfo()
			{
			}

			public DownloadableFileInfo(string filename, string version, string source, string destination)
			{
				FileName = filename;
				Version = version;
				SourceURL = source;
				DestinationFolder = destination;
			}
		}

		private const int SUBITEM_PERCENTAGE_INDEX = 0;
		private const int SUBITEM_FILE_NAME_INDEX = 1;
		//private const int SUBITEM_VERSION_INDEX = 2;
		private const int SUBITEM_DEST_INDEX = 3;
		private const int SUBITEM_URL_INDEX = 4;
		private const int NUMBER_OF_STEP_PER_FILE_FOR_TOTAL_PROGRESS_BAR = 10;

		// a flag to tell if the user has canceled the download
		private bool mHasDownloadBeenCancelled = false;

		// a list to store the file that need to be downloaded
		private List<DownloadableFileInfo> mFilesToDownload = null;

		// a list to store the successful download
		private List<DownloadableFileInfo> mSuccessfullyDownloadedFiles = new List<DownloadableFileInfo>();
		public List<DownloadableFileInfo> SuccessfullyDownloadedFiles
		{
			get { return mSuccessfullyDownloadedFiles; }
		}

		/// <summary>
		/// Instantiate a form and fill it with the specified file list. Each entry in the list isan instance of the 
		/// DownloadableFileInfo class containing the necessary field to know what to download and where.
		/// Also the specified boolean change a bit the behavior of the download form, if <c>true</c> is specified
		/// then the explanation text will changed, and the files will be unziped after download.
		/// </summary>
		/// <param name="fileList">A list of file info to download (containing all the info necessary for the download)</param>
		/// <param name="isUsedToDownloadLibraryPackage">if <c>true</c> then the form will adapt its behavior for library package download</param>
		public DownloadCenterForm(List<DownloadableFileInfo> fileList, bool isUsedToDownloadLibraryPackage)
		{
			InitializeComponent();

			// change the explanation text, if we want to download brick package
			if (isUsedToDownloadLibraryPackage)
				this.ExplanationLabel.Text = Properties.Resources.DownloadLibraryPackageExplanation;

			// reset the counter of downloaded files
			mSuccessfullyDownloadedFiles.Clear();

			// memorize the list of files to download
			mFilesToDownload = fileList;

			// fill the list with the filles
			fillListView(fileList);
		}

		#region event
		private void StartStopButton_Click(object sender, EventArgs e)
		{
			// disable this button
			this.StartButton.Enabled = false;
			// set the hourglass except for the cancel button
			this.Cursor = Cursors.WaitCursor;
			this.cancelButton.Cursor = Cursors.Default;
			// reset the cancel flag
			mHasDownloadBeenCancelled = false;
			// launch the download
			downloadAllTheFile();
		}

		private void DownloadCenterForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			// cancel the background download thread if the cancel button is pressed.
			mHasDownloadBeenCancelled = true;
			this.downloadBackgroundWorker.CancelAsync();
			// re-enable the start button
			this.StartButton.Enabled = true;
			// reset the default cursor
			this.Cursor = Cursors.Default;
		}

		private void DownloadListView_AfterLabelEdit(object sender, LabelEditEventArgs e)
		{
			// The event can be sent with a null label if the label was not changed
			if (e.Label != null)
			{
				// make sure the label start with the folder separator
				if (!e.Label.StartsWith(@"\") && !e.Label.StartsWith(@"/"))
				{
					e.CancelEdit = true;
					this.DownloadListView.Items[e.Item].Text = @"/" + e.Label;
				}
			}
		}
		#endregion

		#region ListView
		/// <summary>
		/// Fill the download form with the specified list of file. Each entry in the list is an instance of the 
		/// DownloadableFileInfo class containing the necessary field to know what to download and where.
		/// </summary>
		/// <param name="fileList">A list of file info to download (containing all the info necessary for the download)</param>
		private void fillListView(List<DownloadableFileInfo> fileList)
		{
			// start of the update of the control
			this.DownloadListView.BeginUpdate();

			// item count to take the item one by one in order
			int itemIndex = 0;
			this.DownloadListView.Items.Clear();

			foreach (DownloadableFileInfo downloadInfo in fileList)
			{
				// create an item
				ListViewItem item = new ListViewItem(new string[] { string.Empty, downloadInfo.FileName, downloadInfo.Version, downloadInfo.DestinationFolder, downloadInfo.SourceURL });
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
			subitems[SUBITEM_PERCENTAGE_INDEX].Text = ComputePercentageBarAsString(percentage);

			// change the color according to the percentage value
			this.DownloadListView.Items[fileIndex].ForeColor = ComputeColorFromPercentage(100 - percentage, 0);
		}

		private void updatePercentageOfTotalBar(int fileIndex, int percentage)
		{
			this.TotalProgressBar.Value = (fileIndex * NUMBER_OF_STEP_PER_FILE_FOR_TOTAL_PROGRESS_BAR) +
											(percentage / NUMBER_OF_STEP_PER_FILE_FOR_TOTAL_PROGRESS_BAR);
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
			// does nothing if the search been canceled
			if (e.Cancelled || mHasDownloadBeenCancelled)
				return;

			// get the result object
			ResultParameter result = (e.Result) as ResultParameter;

			// update the overall percentage for this file
			updatePercentageOfTotalBar(result.fileIndex, 100);

			// check if there was an error to change the color
			if (result.hasErrorOccurs)
			{
				// get the item
				ListViewItem item = this.DownloadListView.Items[result.fileIndex];
				// change the color for this item to red
				item.ForeColor = Color.Red;
				// change the text
				item.SubItems[SUBITEM_PERCENTAGE_INDEX].Text = Properties.Resources.ErrorMsgTitleError;
			}
			else
			{
				// update the full percentage of for the file to 100% (but not if there was errors)
				updatePercentageOfOneFile(result.fileIndex, 100);

				// save the file in the list of successfull download
				mSuccessfullyDownloadedFiles.Add(mFilesToDownload[result.fileIndex]);
			}

			// then download the next file
			downloadOneFile(result.fileIndex + 1);
		}

		private void downloadComplete()
		{
			// reset the default cursor
			this.Cursor = Cursors.Default;
			// Hide the Cancel Button and show the close button
			this.cancelButton.Hide();
			this.closeButton.Show();
		}

		private void downloadOneFile(int fileIndex)
		{
			// check if we reach the end of the list
			if (fileIndex < this.DownloadListView.Items.Count)
			{
				// check if we need to download the file or if we need to skip it
				ListViewItem item = this.DownloadListView.Items[fileIndex];
				if (item.Checked)
				{
					// get the URL and destination and create a parameter for async work
					ListViewItem.ListViewSubItemCollection subitems = item.SubItems;
					DownloadParameter parameters = new DownloadParameter();
					parameters.url = subitems[SUBITEM_URL_INDEX].Text;
					parameters.destination = Application.StartupPath + subitems[SUBITEM_DEST_INDEX].Text;
					parameters.fileIndex = fileIndex;

					// start the download asynchronously by giving the parameters
					downloadBackgroundWorker.RunWorkerAsync(parameters);
					// this method will be called again when the background worker will send it complete event
				}
				else
				{
					// clear the download area
					item.SubItems[SUBITEM_PERCENTAGE_INDEX].Text = string.Empty;
					// if we need to skip this file, download the next one
					updatePercentageOfTotalBar(fileIndex, 100);
					downloadOneFile(fileIndex + 1);
				}
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
			request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.103 Safari/537.36";
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
			catch (Exception)
			{
				// An access denied exception can be raised by a firewall or an antivirus
				// preventing BlueBrick to write the file on the hard drive.
				// this exception can be raised by the FileStream constructor

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
																  // update the progress bar of one file and global progress bar
			updatePercentageOfOneFile(fileIndex, percentage);
			updatePercentageOfTotalBar(fileIndex, percentage);
		}
		#endregion

		#region tool function

		/// <summary>
		/// Transform the specified percentage in a string representing a percentage bar follower by the percentage value.
		/// If the value is greater than 100, the bar will be full as if it will be 100%, but the value display after will be correct.
		/// </summary>
		/// <param name="percentage">The percentage value betwen 0 and 100 (can be greater than 100)</param>
		/// <returns></returns>
		public static string ComputePercentageBarAsString(float percentage)
		{
			string percentageString = "";
			int nbTenth = (int)(percentage * 0.1);
			if (nbTenth > 10)
				nbTenth = 10;
			// write the first tenth of characters
			for (int i = 0; i < nbTenth; ++i)
				percentageString += char.ConvertFromUtf32(0x2588);
			// write the middle character in between the filled part of the bar and the empty part
			if (nbTenth < 10)
			{
				float remain = (percentage - (nbTenth * 10));
				if (remain >= 8.75) // 7/8
					percentageString += char.ConvertFromUtf32(0x2588);
				else if (remain >= 7.5) // 6/8 i.e. 3/4
					percentageString += char.ConvertFromUtf32(0x2589);
				else if (remain >= 6.25) // 5/8
					percentageString += char.ConvertFromUtf32(0x258A);
				else if (remain >= 5.0) // 4/8 i.e. 1/2
					percentageString += char.ConvertFromUtf32(0x258B);
				else if (remain >= 3.75) // 3/8
					percentageString += char.ConvertFromUtf32(0x258C);
				else if (remain >= 2.5) // 2/8 i.e. 1/4
					percentageString += char.ConvertFromUtf32(0x258D);
				else if (remain >= 1.25) // 1/8
					percentageString += char.ConvertFromUtf32(0x258E);
				else
					percentageString += char.ConvertFromUtf32(0x258F);
				++nbTenth;
			}
			// write the remaining spaces
			int nbSpaces = 10 - nbTenth;
			for (int i = 0; i < nbSpaces; ++i)
				percentageString += char.ConvertFromUtf32(0x254C); //0x2550 //0x2591 //0x2007

			// return the result
			return percentageString + " " + percentage.ToString("N0") + "%";
		}

		/// <summary>
		/// Compute a gradient color from green to red (if maxRedValue > 0) or from green to black depending
		/// on the percentage value given in parameter. The maxRedValue can be set to 0 to have a green to black gradient.
		/// </summary>
		/// <param name="percentage">a value between 0 to 100 to compute the gradient of color</param>
		/// <param name="maxRedValue">The maximum value for the red component of the color (when percentage == 100)</param>
		/// <param name="returnRedIfAbove100">If <c>true</c> and if the percentage is above 100, it will return the red color (r=255, g=0, b=0), not the color including the maxRedValue, otherwise if <c>false</c> the color will be the one computed from the maxRedValue.</param>
		/// <returns>A color corresponding to the specified percentage inside a green to red or green to black gradient.</returns>
		public static Color ComputeColorFromPercentage(int percentage, int maxRedValue, bool returnRedIfAbove100 = false)
		{
			// make sure percentage stay in the range [0..100]
			if (percentage < 0)
				percentage = 0;
			else if (percentage > 100)
			{
				if (returnRedIfAbove100)
					return Color.Red;
				else
					percentage = 100;
			}

			// the value of green and red
			int redColor = 0;
			int greenColor = 0;

			if (maxRedValue > 0)
			{
				const double PERCENTAGE_GAP = 7.5;
				const double GREEN_SLOPE = (50 / (50.0 + PERCENTAGE_GAP));
				double redSlope = (maxRedValue / (75.0 + PERCENTAGE_GAP));

				// compute the red color
				if (percentage <= (75.0 + PERCENTAGE_GAP))
					redColor = (int)(redSlope * percentage);
				else
					redColor = maxRedValue;

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
				greenColor = 200 - (int)(2 * percentage);
			}

			return Color.FromArgb(0xFF, redColor, greenColor, 0x00);
		}
		#endregion
	}
}