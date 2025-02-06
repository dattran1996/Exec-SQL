using System;
using System.Text;
using System.Data.SqlClient;
using System.IO;
using System.Configuration;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace SQLExecutor
{
    class Program
    {
        static void Main(string[] args)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["SqlConnection"].ConnectionString; // Database connection string
            string sqlFilesDirectory = ConfigurationManager.AppSettings["SqlFilesDirectory"]; // Directory containing SQL files
            string logFilePath = "execution_log.txt"; // Path to the log file

            SqlExecutor executor = new SqlExecutor(connectionString, sqlFilesDirectory, logFilePath);
            executor.ExecuteSqlFiles();
            Console.WriteLine("Process completed.");
        }  
    }
}
