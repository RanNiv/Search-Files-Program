using DAL;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL
{

      public  class SearchManager
        {
            private string[] drives;
            public StringCollection Log { get; set; } = new StringCollection();
            public event Action<FileInfo> FileSearchHandler;
            public string SearchString { get; set; }
            public DirectoryInfo SpecificFolder { get; set; }
            public int DbResultsIndication { get; set; }


            //constructor for searching file,with no folder specification
            public SearchManager(string text)
            {
                SearchString = text;
                drives = System.Environment.GetLogicalDrives();
                DbResultsIndication = DbManager.SearchExistingResults(text);
            }

            //constructor for searching file,with folder specification
            public SearchManager(string text, string path)
            {
                //First Test Folder Path 
                try
                {
                    SpecificFolder = new DirectoryInfo(path);
                }
                catch (Exception)
                {
                    DbResultsIndication = -1;
                    return;

                }
                if (!SpecificFolder.Exists)
                {
                    DbResultsIndication = -1;
                    return;
                }
                SearchString = text;
                drives = System.Environment.GetLogicalDrives();
            }

            //Save search Term
            public void Save(bool withfolder = false)
            {
                if (withfolder)
                {
                    DbManager.SaveSearchTerm(SearchString, SpecificFolder.FullName);
                }
                else
                    DbManager.SaveSearchTerm(SearchString);
            }



            //Search proccess
            public void SearchDrives()
            {

                foreach (string dr in drives)
                {
                    System.IO.DriveInfo di = new System.IO.DriveInfo(dr);
                    //skip the drive if it is not ready to be read.
                    if (!di.IsReady)
                    {
                        Log.Add($"The drive {di.Name} could not be read");
                        continue;
                    }
                    System.IO.DirectoryInfo rootDir = di.RootDirectory;
                    WalkDirectoryTree(new DirectoryInfo(dr));
                }
            }
            public void WalkDirectoryTree(System.IO.DirectoryInfo root)
            {
                System.IO.FileInfo[] files = null;
                System.IO.DirectoryInfo[] subDirs = null;
                // First, process all the files directly under this folder
                try
                {
                    files = root.GetFiles($"*.*");
                }
                catch (UnauthorizedAccessException e)
                {

                    Log.Add(e.Message);
                }
                catch (System.IO.DirectoryNotFoundException e)
                {

                Log.Add(e.Message);

                }

                int CurrentSearchId = DbManager.GetLastSearchId();
                if (files != null)
                {
                    foreach (System.IO.FileInfo fi in files)
                    {
                        if (fi.Name.ToLower().Contains(SearchString.ToLower()))
                        {
                            //raze event when file found
                            FileSearchHandler?.Invoke(fi);
                            //save file in db;
                            DbManager.saveFile(fi, CurrentSearchId);
                        }
                    }

                    // find all the subdirectories under this directory.
                    subDirs = root.GetDirectories();
                    foreach (System.IO.DirectoryInfo dirInfo in subDirs)
                    {
                        // Resursive call for each subdirectory.
                        WalkDirectoryTree(dirInfo);
                    }
                }

            
        }
    }
}
