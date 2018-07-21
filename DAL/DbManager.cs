using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public static class DbManager
    {
        private static readonly string connectionString;

        //static constructor for connection string initialization
        static DbManager()
        {
            string str = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase).Replace("\\bin\\Debug", "").Replace("file:\\", "");
            connectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=\"" + str + "\\FilesDb.mdf\";Integrated Security=True";
        }

        //Get last Search Id from the SearchHistory table
        public static int GetLastSearchId()
        {
            int searchId = 0;
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand command = new SqlCommand($"select top 1 * from( SELECT  SearchId from SearchHistory union all select 0) as Search order by Search.SearchId desc ", conn);
                    searchId = (int)command.ExecuteScalar();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return searchId;
        }

        //Search existing results
        public static int SearchExistingResults(string text)
        {
            int searchId = 0;
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand command = new SqlCommand($"SELECT SearchId from SearchHistory where SearchText='{text}' and Folder is null union all select 0", conn);
                    searchId = (int)command.ExecuteScalar();
                    if (searchId > 0)
                    {
                        Console.WriteLine("Files founded in Database");
                        command = new SqlCommand($"SELECT * from FilesInfo where SearchId={searchId}", conn);
                        SqlDataReader reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            Console.WriteLine($"{reader["Name"]}");
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return searchId;
        }

        //Save Search in SearchHistory Table only if not already exists in the table 
        public static void SaveSearchTerm(string text, string folder = "")
        {
            string folderValue = folder == "" ? "null" : $"'{folder}'";


            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand query = new SqlCommand($"select count (*) as count from [dbo].[SearchHistory]", conn);
                    int testId = (int)query.ExecuteScalar();
                    if (testId == 0)
                    {
                        query = new SqlCommand($"INSERT INTO [dbo].[SearchHistory] values ('{text}',{folderValue}) ", conn);
                        query.ExecuteNonQuery();

                    }
                    else
                    {
                        query = new SqlCommand($"select top 1 1 from [dbo].[SearchHistory] where searchtext='{text}' and Folder={folderValue} union select -1 order by 1", conn);
                        testId = (int)query.ExecuteScalar();
                        if (testId != 1)
                        {
                            query = new SqlCommand($"INSERT INTO [dbo].[SearchHistory] values ('{text}',{folderValue}) ", conn);
                            query.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }


        //save file in FilesInfo table
        public static void saveFile(FileInfo file, int id)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    SqlCommand command = new SqlCommand($"INSERT INTO [dbo].[FilesInfo] values ({id},'{file.FullName}') ", conn);
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }


        }
    }
}
