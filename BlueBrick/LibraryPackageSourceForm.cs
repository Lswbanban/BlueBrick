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
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace BlueBrick
{
	public partial class LibraryPackageSourceForm : Form
	{
		private const string mOfficialPartLibraryURL = "https://bluebrick.lswproject.com/download/package/";
		private const string mOfficialNonLegoPartLibraryURL = "https://bluebrick.lswproject.com/download/packageOther/";

		// a variable to memorize the button text because we will change it
		private string mOriginalSearchButtonLabel = string.Empty;

		// a flag set in case the search was successful, and the form auto close
		private bool mIsFormClosingSuccessfully = false;
		private bool mHasSearchBeenCancelled = false;

		// the result of this search form
		private List<DownloadCenterForm.DownloadableFileInfo> mFilesToDownload = new List<DownloadCenterForm.DownloadableFileInfo>();

		#region get set
		public List<DownloadCenterForm.DownloadableFileInfo> FilesToDownload
		{
			get { return mFilesToDownload; }
		}
		#endregion

		#region init
		public LibraryPackageSourceForm()
		{
			InitializeComponent();
			mOriginalSearchButtonLabel = buttonSearch.Text;
		}

		private void changeSearchButton(bool isEnabled)
		{
			buttonSearch.Enabled = isEnabled;
			buttonSearch.Text = isEnabled ? mOriginalSearchButtonLabel : Properties.Resources.ButtonPleaseWait;
		}
		#endregion

		#region UI event
		private void LibraryPackageSourceForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (!mIsFormClosingSuccessfully)
			{
				// if the user cancel, stop the background worker
				mHasSearchBeenCancelled = true;
				backgroundWorkerSearchOnline.CancelAsync();
				// then clear the result list and close the form
				mFilesToDownload.Clear();
			}
		}

		private void buttonSearch_Click(object sender, EventArgs e)
		{
			// disable the button and change its text
			changeSearchButton(false);

			// set the hourglass except for the cancel button
			this.Cursor = Cursors.WaitCursor;
			this.buttonCancel.Cursor = Cursors.Default;

			// get the URLs depending on what is checked and create a parameter for async work
			SearchParameter parameters = new SearchParameter();
			if (checkBoxSearchOfficial.Checked)
				parameters.searchURLs.Add(mOfficialPartLibraryURL);
			if (checkBoxSearchNonLego.Checked)
				parameters.searchURLs.Add(mOfficialNonLegoPartLibraryURL);
			if (checkBoxSearchUnofficial.Checked)
				parameters.searchURLs.Add(textBoxUnofficialPartLibraryURL.Text);

			// reset the cancel flag
			mHasSearchBeenCancelled = false;
			// start the download asynchronously by giving the parameters
			backgroundWorkerSearchOnline.RunWorkerAsync(parameters);
		}

		/// <summary>
		/// Check if the search can be launch, at least one checkbox should be checked, and if the unofficial location is checked,
		/// the URL address cannot be empty; and then enable or disable the button that launch the search
		/// </summary>
		private void enableDisableTheSearchButton()
		{
			buttonSearch.Enabled = checkBoxSearchOfficial.Checked || checkBoxSearchNonLego.Checked || checkBoxSearchUnofficial.Checked;
			if (checkBoxSearchUnofficial.Checked && textBoxUnofficialPartLibraryURL.Text == string.Empty)
				buttonSearch.Enabled = false;
		}

		private void checkBoxSearchOfficial_CheckedChanged(object sender, EventArgs e)
		{
			// enable the search button if the search is valid
			enableDisableTheSearchButton();
		}

		private void checkBoxSearchNonLego_CheckedChanged(object sender, EventArgs e)
		{
			// enable the search button if the search is valid
			enableDisableTheSearchButton();
		}

		private void checkBoxSearchUnofficial_CheckedChanged(object sender, EventArgs e)
		{
			// enable the search button if the search is valid
			enableDisableTheSearchButton();
			// enable the URL text box, only if the checkbox is checked
			textBoxUnofficialPartLibraryURL.Enabled = checkBoxSearchUnofficial.Checked;
		}

		private void textBoxUnofficialPartLibraryURL_TextChanged(object sender, EventArgs e)
		{
			// enable the search button if the search is valid
			enableDisableTheSearchButton();
		}
		#endregion

		#region package list filtering
		private string getPackageVersionInAboutFile(string aboutFileName)
		{
			const string version = "version=";
			string[] lines = System.IO.File.ReadAllLines(aboutFileName);
			// search the version line
			foreach (string line in lines)
				if (line.StartsWith(version, StringComparison.OrdinalIgnoreCase))
					return line.Substring(version.Length).Trim();
			return string.Empty;
		}

		private List<DownloadCenterForm.DownloadableFileInfo> removeAlreadyInstalledPackagesFromList(List<DownloadCenterForm.DownloadableFileInfo> packageListToFilter)
		{
			// get all the folders in the parts folder to know what is already installed
			DirectoryInfo partsFolder = new DirectoryInfo(PartLibraryPanel.sFullPathForLibrary);
			DirectoryInfo[] directoriesInPartsFolder = new DirectoryInfo[] { };
			if (partsFolder.Exists)
			{
				directoriesInPartsFolder = partsFolder.GetDirectories();
			}
			else
			{
				// if the part folder doesn't exist, try to create it
				partsFolder.Create();
				// if the part folder doesn't exist we need to get all the package found
				return packageListToFilter;
			}


			// create a list with only the name of the directory
			List<DownloadCenterForm.DownloadableFileInfo> installedPackages = new List<DownloadCenterForm.DownloadableFileInfo>();
			foreach (DirectoryInfo directory in directoriesInPartsFolder)
			{
				// save the package name in the package info
				DownloadCenterForm.DownloadableFileInfo package = new DownloadCenterForm.DownloadableFileInfo();
				package.FileName = directory.Name + ".zip"; // add the zip extension to facilitate the string comparison

				// check the about file on the local drive to get the version number
				string aboutFileName = Application.StartupPath + @"/parts/" + directory.Name + @"/config/About.txt";
				if (File.Exists(aboutFileName))
					package.Version = getPackageVersionInAboutFile(aboutFileName);

				// add the package to the list
				installedPackages.Add(package);
			}

			// then iterate on the list to filter and remove the items already installed that has the good version
			for (int i = 0; i < packageListToFilter.Count; ++i)
				foreach (DownloadCenterForm.DownloadableFileInfo package in installedPackages)
					if (package.FileName.Equals(packageListToFilter[i].FileName, StringComparison.OrdinalIgnoreCase) && string.Compare(package.Version, packageListToFilter[i].Version) >= 0)
					{
						packageListToFilter.RemoveAt(i);
						i--;
						break;
					}

			return packageListToFilter;
		}
		#endregion

		#region background worker event
		/// <summary>
		/// a small container to provide parameters to the background worker thread
		/// </summary>
		private class SearchParameter
		{
			public List<string> searchURLs = new List<string>(3);
		}

		/// <summary>
		/// a small container to send back information from the background thread
		/// </summary>
		private class ResultParameter
		{
			// this list contains all the package list found for all the url we search
			public List<DownloadCenterForm.DownloadableFileInfo> allPackageListFound = new List<DownloadCenterForm.DownloadableFileInfo>();
		}

		/// <summary>
		/// A small container to pass progress information from the background thread
		/// </summary>
		private class ProgressParameter
		{
			public int currentIndex;
			public int totalCount;
			public ProgressParameter(int current, int total)
			{
				currentIndex = current;
				totalCount = total;
			}
		}

		private void backgroundWorkerSearchOnline_DoWork(object sender, DoWorkEventArgs e)
		{
			const string partsFolder = @"/parts/";

			// Get the BackgroundWorker that raised this event.
			BackgroundWorker worker = sender as BackgroundWorker;
			// and get the parameters
			SearchParameter parameters = e.Argument as SearchParameter;
			ResultParameter result = new ResultParameter();

			// declare a variable to report the progress on the search
			int currentUrlIndex = 0;

			// iterate on all the url in the search parameters
			foreach (string url in parameters.searchURLs)
			{
				// increase the current url and init the serach step
				currentUrlIndex++;
				int searchStep = 0;

				try
				{
					// report the begining of the search
					worker.ReportProgress(searchStep, new ProgressParameter(currentUrlIndex, parameters.searchURLs.Count));

					// create a web request to browse the url
					HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
					request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.103 Safari/537.36";
					using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
					{
						// report that we got an answer
						worker.ReportProgress(++searchStep, new ProgressParameter(currentUrlIndex, parameters.searchURLs.Count));

						using (StreamReader reader = new StreamReader(response.GetResponseStream()))
						{
							string html = reader.ReadToEnd();
							// use a regexp to parse all the listed files from the html page
							Regex regex = new Regex("<a href=\".+\\.zip\">(?<name>.+\\.zip)</a>");
							MatchCollection matches = regex.Matches(html);
							foreach (Match match in matches)
								if (match.Success)
								{
									// create an array to store the destination file name, and url source file name
									DownloadCenterForm.DownloadableFileInfo downloadInfo = new DownloadCenterForm.DownloadableFileInfo();
									// get the file name found
									string fileName = match.Groups["name"].Value;
									string fileNameWithoutExtension = fileName.Remove(fileName.Length - 4);
									// check if the file name contains a version number
									int dotIndex = fileNameWithoutExtension.IndexOf('.');
									if (dotIndex > 0)
									{
										downloadInfo.FileName = fileNameWithoutExtension.Remove(dotIndex) + ".zip";
										downloadInfo.Version = fileNameWithoutExtension.Substring(dotIndex + 1);
									}
									else
									{
										downloadInfo.FileName = fileName;
										downloadInfo.Version = string.Empty;
									}
									downloadInfo.SourceURL = url + fileName;
									downloadInfo.DestinationFolder = partsFolder + fileName;
									// add the array in the result list
									result.allPackageListFound.Add(downloadInfo);
								}

							// report that we have finished to parse the html
							worker.ReportProgress(++searchStep, new ProgressParameter(currentUrlIndex, parameters.searchURLs.Count));
						}
					}
				}
				catch
				{
					// report that the error
					worker.ReportProgress(3, new ProgressParameter(currentUrlIndex, parameters.searchURLs.Count));
				}
			}

			// set the result in the event of the background worker
			e.Result = result;
		}

		private void backgroundWorkerSearchOnline_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			string[] progressStates = { Properties.Resources.SearchStatusWaitResponse, Properties.Resources.SearchStatusReadStream, Properties.Resources.SearchStatusFinished, Properties.Resources.SearchStatusError };
			ProgressParameter progress = e.UserState as ProgressParameter;
			labelSearchStatus.Text = progress.currentIndex.ToString() + "/" + progress.totalCount.ToString() + ": " + progressStates[e.ProgressPercentage];
			labelSearchStatus.BackColor = e.ProgressPercentage == 3 ? Color.Salmon : Color.LightGreen;
		}

		private void backgroundWorkerSearchOnline_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			// does nothing if the search been canceled
			if (e.Cancelled || mHasSearchBeenCancelled)
				return;

			// get the result object
			ResultParameter result = (e.Result) as ResultParameter;

			// filter all the package we have found, from the one already installed locally, and save it in the result list of the form
			mFilesToDownload = removeAlreadyInstalledPackagesFromList(result.allPackageListFound);

			// if the list is empty, display an error message, but stay on this form to let the user try again
			if (mFilesToDownload.Count == 0)
			{
				// display a warning message and reload the library
				MessageBox.Show(this, BlueBrick.Properties.Resources.ErrorMsgNoAvailablePartsPackageToDownload,
								BlueBrick.Properties.Resources.ErrorMsgTitleWarning, MessageBoxButtons.OK,
								MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);

				// reenable the search button for a second chance
				changeSearchButton(true);

				// and reset the default cursor for the form
				this.Cursor = Cursors.Default;
			}
			else
			{
				// if we have something to download, close the form, to let the main form open the next form to download the packages
				mIsFormClosingSuccessfully = true;
				this.Close();
			}
		}
		#endregion
	}
}
