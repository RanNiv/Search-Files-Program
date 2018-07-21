using BLL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Search_Files_Program
{
    class Program
    {
        //diplay Search screen options
        static void SearchScreenOptions(out int option)
        {
            List<string> optionList = new List<string> { "1", "2", "3" };
            option = 0;
            Console.WriteLine("1. Enter file name to search.");
            Console.WriteLine("2. Enter File name to search + parent directory to search in.");
            Console.WriteLine("3. Exit");

            string textOptions = Console.ReadLine();
            bool isValidOption = true;
            if (textOptions == "")
            {
                isValidOption = false;
            }
            if (isValidOption)
                isValidOption = optionList.Exists(x => x == textOptions.Substring(0, 1) && textOptions.Length == 1);
            if (!isValidOption)
            {
                Console.WriteLine("You entered invalid option");
                Console.WriteLine("You can enter only 1,2 or 3 options");
                SearchProgram();
            }
            int.TryParse(textOptions, out option);
        }

        //Start search program
        static void SearchProgram()
        {
            string textSearch = string.Empty;
            string rootDirectory = string.Empty;
            int option = 0;
            SearchScreenOptions(out option);
            SearchManager searchManager = null;
            int b = option;

            switch (b)
            {
                case 1: //  searching file,with no folder specification
                    Console.Write("Enter file name to search: ");
                    textSearch = Console.ReadLine();
                    if (textSearch.Length==0 || textSearch.Length>1000)
                    {
                        Console.WriteLine("Search file name must have at least one letter and no more than 1000");
                        goto case 1;
                    }

                    Console.WriteLine("Start Searching...");
                    searchManager = new SearchManager(textSearch);
                    if (searchManager.DbResultsIndication == 0)
                    {
                        searchManager.Save();
                        searchManager.FileSearchHandler += (file) => Console.WriteLine(file.FullName);
                        searchManager.SearchDrives();
                    }
                    break;
                case 2: //searching file,with folder specification
                    Console.Write("Enter file name to search: ");
                    textSearch = Console.ReadLine();
                    if (textSearch.Length == 0 || textSearch.Length > 1000)
                    {
                        Console.WriteLine("Search file name must have at least one letter and no more than 1000");
                        goto case 2;
                    }
                    Console.Write("Enter root directory to search in: ");
                    rootDirectory = Console.ReadLine();

                    if (rootDirectory.Length == 0 || rootDirectory.Length > 1000)
                    {
                        Console.WriteLine("Search folder name must have at least one letter and no more than 1000");
                        goto case 2;
                    }
                    Console.WriteLine("Start Searching...");
                    searchManager = new SearchManager(textSearch, rootDirectory);
                    if (searchManager.DbResultsIndication == -1)
                    {
                        Console.WriteLine($"The Folder {rootDirectory} does not exists in this pc");
                        goto case 2;
                    }
                    else
                    {
                        searchManager.Save(true);
                        searchManager.FileSearchHandler += (file) => Console.WriteLine(file.FullName);
                        searchManager.WalkDirectoryTree(searchManager.SpecificFolder);
                    }
                    break;
                case 3: //Exit
                    Environment.Exit(0);
                    break;

            }
            Console.WriteLine("Search Complete");
            Console.WriteLine("Errors found in the proccess:");


            foreach (var item in searchManager.Log)
            {
                Console.WriteLine(item.ToString());
            }

            Console.WriteLine("Press any key to search again");
            Console.ReadKey();
            Console.Clear();
            SearchProgram();

        }

        static void Main(string[] args)
        {
            SearchProgram();
        }
    }
}
