using System;
using System.Data.SqlClient;
using System.IO;
using System.Text.RegularExpressions;

namespace SQLExecutor
{
    public class SqlExecutor
    {
        private readonly string _connectionString;   // Database connection string
        private readonly string _sqlFilesDirectory;  // Directory containing SQL files
        private readonly string _logFilePath;        // Path for the execution log file

        // Constructor to initialize the SqlExecutor with necessary parameters
        public SqlExecutor(string connectionString, string sqlFilesDirectory, string logFilePath)
        {
            _connectionString = connectionString;
            _sqlFilesDirectory = sqlFilesDirectory;
            _logFilePath = logFilePath;
        }

        // Method to execute all SQL files in the specified directory
        public void ExecuteSqlFiles()
        {
            // Define the path for the directory to store failed SQL files
            string failedSqlDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FailedSQL");

            // Clear any existing failed SQL files
            FileHandler.ClearFailedSqlDirectory(failedSqlDirectory);

            // Initialize the log file
            FileHandler.InitializeLogFile(_logFilePath);

            // Iterate through each .sql file in the directory and execute it
            foreach (var sqlFilePath in Directory.GetFiles(_sqlFilesDirectory, "*.sql", SearchOption.AllDirectories))
            {
                ExecuteSqlFile(sqlFilePath, failedSqlDirectory); 
            }
        }

        // Method to execute a single SQL file
        private void ExecuteSqlFile(string sqlFilePath, string failedSqlDirectory)
        {
            try
            {
                // Read the SQL file content with the appropriate encoding
                string sqlContent = FileHandler.ReadFileWithEncoding(sqlFilePath);

                // Comment out 'USE' statements in the SQL content
                sqlContent = SqlUtility.CommentOutUseStatements(sqlContent);

                // Adjust CREATE/ALTER statements based on the existence of the objects
                sqlContent = SqlUtility.AdjustCreateAlterStatements(_connectionString, sqlContent);

                // Execute the SQL batches
                ExecuteSqlBatches(sqlContent);

                // Log the successful execution of the SQL file
                FileHandler.Log(_logFilePath, sqlFilePath, true, null);
                Console.WriteLine($"Success: {sqlFilePath}"); 
            }
            catch (SqlException ex)
            {
                // Handle SQL-specific exceptions
                HandleSqlException(ex, sqlFilePath, failedSqlDirectory);
            }
            catch (Exception ex)
            {
                // Handle general exceptions
                HandleGeneralException(ex, sqlFilePath, failedSqlDirectory);
            }
        }

        // Method to execute SQL content split into batches based on 'GO' statements
        private void ExecuteSqlBatches(string sqlContent)
        {
            // Split the SQL content into batches using 'GO' as the delimiter
            var batches = Regex.Split(sqlContent, @"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase);

            // Establish a SQL connection
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open(); 
            
                foreach (var batch in batches)
                {
                    var trimmedBatch = batch.Trim(); 
                    if (string.IsNullOrEmpty(trimmedBatch)) continue; // Skip empty batches

                    // Execute the trimmed batch
                    using (SqlCommand command = new SqlCommand(trimmedBatch, connection))
                    {
                        command.ExecuteNonQuery(); 
                    }
                }
            }
        }

        // Method to handle SQL exceptions during execution
        private void HandleSqlException(SqlException ex, string sqlFilePath, string failedSqlDirectory)
        {
            // Get detailed error information from the SQL exception
            string errorDetails = FileHandler.GetSqlExceptionDetails(ex);

            // Log the error details to the log file
            FileHandler.Log(_logFilePath, sqlFilePath, false, errorDetails);
            Console.WriteLine($"SQL Error processing file {sqlFilePath}: {errorDetails}"); 

            // Copy the failed SQL file to the failed SQL directory
            FileHandler.CopyToFailedDirectory(sqlFilePath, failedSqlDirectory);
        }

        // Method to handle general exceptions during execution
        private void HandleGeneralException(Exception ex, string sqlFilePath, string failedSqlDirectory)
        {
            // Log the general exception message
            FileHandler.Log(_logFilePath, sqlFilePath, false, ex.Message);
            Console.WriteLine($"General Error processing file {sqlFilePath}: {ex.Message}"); 

            // Copy the failed SQL file to the failed SQL directory
            FileHandler.CopyToFailedDirectory(sqlFilePath, failedSqlDirectory);
        }
    }

}
