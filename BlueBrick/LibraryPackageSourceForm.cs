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
		private const string mOfficialPartLibraryURL = "http://bluebrick.lswproject.com/download/package/";
		private const string mOfficialNonLegoPartLibraryURL = "http://bluebrick.lswproject.com/download/packageOther/";

		private List<string[]> mFilesToDownload = new List<string[]>();

		#region get set
		public List<string[]> FilesToDownload
		{
			get { return mFilesToDownload; }
		}
		#endregion

		public LibraryPackageSourceForm()
		{
			InitializeComponent();
		}

		#region UI event
		private void buttonCancel_Click(object sender, EventArgs e)
		{
			mFilesToDownload = BlueBrick.MapData.OnlineBrickResources.getUninstalledBrickPackageAvailableOnline(mOfficialPartLibraryURL);
		}

		private void buttonSearch_Click(object sender, EventArgs e)
		{
			// disable the button and change its text
			buttonSearch.Enabled = false;
			buttonSearch.Text = Properties.Resources.ButtonPleaseWait;

			// get the URLs depending on what is checked and create a parameter for async work
			SearchParameter parameters = new SearchParameter();
			if (checkBoxSearchOfficial.Checked)
				parameters.searchURLs.Add(mOfficialPartLibraryURL);
			if (checkBoxSearchNonLego.Checked)
				parameters.searchURLs.Add(mOfficialNonLegoPartLibraryURL);
			if (checkBoxSearchUnofficial.Checked)
				parameters.searchURLs.Add(textBoxUnofficialPartLibraryURL.Text);

			// start the download asynchronously by giving the parameters
			backgroundWorkerSearchOnline.RunWorkerAsync(parameters);
		}

		private void checkBoxSearchUnofficial_CheckedChanged(object sender, EventArgs e)
		{
			// enable the URL text box, only if the checkbox is checked
			textBoxUnofficialPartLibraryURL.Enabled = checkBoxSearchUnofficial.Checked;
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
			public class BrickPackageListForOneURL
			{
				public List<string[]> resultBrickPackageList = new List<string[]>();
				public bool hasErrorOccurs = false;
			}

			// this list contains all the package list found for all the url we search in the same order as the url list which is in the SearchParameter class
			public List<BrickPackageListForOneURL> allPackageListFound = new List<BrickPackageListForOneURL>();
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

				// create the result for the current url and add it to the overall result instance
				ResultParameter.BrickPackageListForOneURL brickPackageListForCurrentURL = new ResultParameter.BrickPackageListForOneURL();
				result.allPackageListFound.Add(brickPackageListForCurrentURL);

				try
				{
					// report the begining of the search
					worker.ReportProgress(searchStep, new ProgressParameter(currentUrlIndex, parameters.searchURLs.Count));

					// create a web request to browse the url
					HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
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
									// get the file name found
									string fileName = match.Groups["name"].Value;
									// create an array to store the destination file name, and url source file name
									string[] destAndSource = new string[] { partsFolder + fileName, url + fileName, string.Empty };
									// add the array in the result list
									brickPackageListForCurrentURL.resultBrickPackageList.Add(destAndSource);
								}

							// report that we have finished to parse the html
							worker.ReportProgress(++searchStep, new ProgressParameter(currentUrlIndex, parameters.searchURLs.Count));
						}
					}
				}
				catch
				{
					// set the error flag if the online search add problems
					brickPackageListForCurrentURL.hasErrorOccurs = true;
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

		}
		#endregion
	}
}
