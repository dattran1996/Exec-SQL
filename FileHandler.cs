using System;
using System.IO;
using System.Text;
using System.Data.SqlClient;

namespace SQLExecutor
{
    public static class FileHandler
    {
        // Method to clear the failed SQL directory by deleting it if it exists
        public static void ClearFailedSqlDirectory(string failedSqlDirectory)
        {
            if (Directory.Exists(failedSqlDirectory))
            {
                Directory.Delete(failedSqlDirectory, true); // Delete the directory and its contents
            }
        }

        // Method to initialize the log file with a header containing the current date and time
        public static void InitializeLogFile(string logFilePath)
        {
            File.WriteAllText(logFilePath, $"Execution Log - {DateTime.Now}\n\n"); // Write header to log file
        }

        // Method to read the content of a file while detecting its encoding
        public static string ReadFileWithEncoding(string filePath)
        {
            // Read all bytes from the specified file
            byte[] fileBytes = File.ReadAllBytes(filePath);

            // Determine the encoding of the file
            Encoding encoding = GetEncoding(fileBytes, out int bomLength);

            // Skip BOM bytes if they are detected
            return encoding.GetString(fileBytes, bomLength, fileBytes.Length - bomLength);
        }
        // Private method to determine the encoding of the file based on its byte content
        private static Encoding GetEncoding(byte[] fileBytes, out int bomLength)
        {
            bomLength = 0;
            // Check BOM to determine encoding
            if (fileBytes.Length >= 3)
            {
                // UTF-8 detection
                if (fileBytes[0] == 0xEF && fileBytes[1] == 0xBB && fileBytes[2] == 0xBF)
                {
                    bomLength = 3;
                    return Encoding.UTF8; // UTF-8
                }
                    
            }

            if (fileBytes.Length >= 2)
            {
                // UTF-16 Little Endian
                if (fileBytes[0] == 0xFF && fileBytes[1] == 0xFE)
                {
                    bomLength = 2;
                    return Encoding.Unicode; // UTF-16 Little Endian
                }

                // UTF-16 Big Endian
                if (fileBytes[0] == 0xFE && fileBytes[1] == 0xFF)
                {
                    bomLength = 2;
                    return Encoding.BigEndianUnicode; // UTF-16 Big Endian
                }
            }

            // If there's no BOM, try to determine if it's UTF-8
            if (IsUtf8(fileBytes))
            {
                return Encoding.UTF8; // No BOM, UTF-8 detected
            }

            return Encoding.GetEncoding("shift_jis"); // Default to Shift JIS encoding for Japanese
        }

        // Private method to check if the byte array is UTF-8 encoded
        private static bool IsUtf8(byte[] bytes)
        {
            int i = 0;
            while (i < bytes.Length)
            {
                if (bytes[i] <= 0x7F) { i++; continue; }
                else if ((bytes[i] >= 0xC2 && bytes[i] <= 0xDF) && (i + 1 < bytes.Length && bytes[i + 1] >= 0x80 && bytes[i + 1] <= 0xBF)) { i += 2; continue; }
                else if ((bytes[i] >= 0xE0 && bytes[i] <= 0xEF) && (i + 2 < bytes.Length && bytes[i + 1] >= 0x80 && bytes[i + 1] <= 0xBF && bytes[i + 2] >= 0x80 && bytes[i + 2] <= 0xBF)) { i += 3; continue; }
                else if ((bytes[i] >= 0xF0 && bytes[i] <= 0xF4) && (i + 3 < bytes.Length && bytes[i + 1] >= 0x80 && bytes[i + 1] <= 0xBF && bytes[i + 2] >= 0x80 && bytes[i + 3] <= 0xBF)) { i += 4; continue; }
                else { return false; }
            }
            return true;
        }

        // Method to log the execution status of SQL files to the log file
        public static void Log(string logFilePath, string sqlFilePath, bool isSuccess, string errorMessage)
        {
            string status = isSuccess ? "Success" : "Failure"; // Determine success or failure status
            string message = $"{DateTime.Now}: {status} - {Path.GetFileName(sqlFilePath)}"; // Create log message

            // If there was an error, append the error message to the log
            if (!isSuccess && !string.IsNullOrEmpty(errorMessage))
            {
                message += $"\n\tError: {errorMessage}";
            }

            // Append the message to the log file
            File.AppendAllText(logFilePath, message + "\n");
        }

        // Method to get detailed information about a SQL exception
        public static string GetSqlExceptionDetails(SqlException ex)
        {
            // Start building the details string with the main error information
            var details = $"SQL Error Number: {ex.Number}\n\tMessage: {ex.Message}";

            // Iterate through each error in the exception to gather details
            foreach (SqlError error in ex.Errors)
            {
                details += $"\n\tError #{error.Number}: {error.Message} (Line: {error.LineNumber})"; // Append error details
            }

            return details; // Return all error details as a string
        }

        // Method to copy a failed SQL file to the specified failed SQL directory
        public static void CopyToFailedDirectory(string sqlFilePath, string failedSqlDirectory)
        {
            Directory.CreateDirectory(failedSqlDirectory); // Create the directory if it doesn't exist
            string failedSqlFilePath = Path.Combine(failedSqlDirectory, Path.GetFileName(sqlFilePath)); 
            File.Copy(sqlFilePath, failedSqlFilePath, true); 
        }  
    }

}
