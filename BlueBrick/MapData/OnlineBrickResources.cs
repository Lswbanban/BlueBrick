using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace BlueBrick.MapData
{
	class OnlineBrickResources
	{
        public static List<string[]> getUninstalledBrickPackageAvailableOnline(string url)
        {
            // get all the package available online
            List<string[]> installableBrickPackageList = getAvailableBrickPackageOnline(url);
            // filter those packages to only keep the one not currently installed
            return removeAlreadyInstalledPackagesFromList(installableBrickPackageList);
        }

        private static List<string[]> removeAlreadyInstalledPackagesFromList(List<string[]> packageListToFilter)
        {
            // get all the folders in the parts folder to know what is already installed
            DirectoryInfo partsFolder = new DirectoryInfo(PartLibraryPanel.sFullPathForLibrary);
            DirectoryInfo[] directoriesInPartsFolder = partsFolder.GetDirectories();

            // create a list with only the name of the directory (adding the zip at the end to facilitate comparison
            List<string> installedPackageFolder = new List<string>();
            foreach (DirectoryInfo directory in directoriesInPartsFolder)
                installedPackageFolder.Add(@"/parts/" + directory.Name.ToLower() + ".zip");

            // then iterate on the list to filter and remove the items already installed
            for (int i = 0; i < packageListToFilter.Count; ++i)
                if (installedPackageFolder.Contains(packageListToFilter[i][0].ToLower()))
                {
                    packageListToFilter.RemoveAt(i);
                    i--;
                }

            return packageListToFilter;
        }

        public static List<string[]> getAvailableBrickPackageOnline(string url)
		{
			const string partsFolder = @"/parts/";
			List<string[]> resultBrickPackageList = new List<string[]>();

            // create a web request to browse the url
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
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
                            resultBrickPackageList.Add(destAndSource);
                        }
                }
            }

            // return the list of files that we found
            return resultBrickPackageList;
		}
    }
}
