using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text.RegularExpressions;

namespace SQLExecutor
{
    public static class SqlUtility
    {
        public static string CommentOutUseStatements(string sqlContent)
        {
            return Regex.Replace(sqlContent, @"^\s*USE\s+.*$", "-- $0", RegexOptions.Multiline);
        }

        public static string AdjustCreateAlterStatements(string connectionString, string sqlContent)
        {
            // Only apply adjustments for PROCEDURE and FUNCTION
            if (Regex.IsMatch(sqlContent, @"\bCREATE\s+(PROCEDURE|FUNCTION)\s+(\[?\w+\]?\.)?\[?(\w+)\]?", RegexOptions.IgnoreCase))
            {
                var match = Regex.Match(sqlContent, @"\bCREATE\s+(PROCEDURE|FUNCTION)\s+(\[?\w+\]?\.)?\[?(\w+)\]?", RegexOptions.IgnoreCase);
                string objectType = match.Groups[1].Value; // PROCEDURE or FUNCTION
                string schemaName = match.Groups[2].Value.TrimEnd('.').Trim('[', ']'); // Schema 
                string objectName = match.Groups[3].Value; // Object name

                // Default to 'dbo' if no schema 
                if (string.IsNullOrEmpty(schemaName))
                {
                    schemaName = "dbo";
                }

                if (CheckIfObjectExists(connectionString, objectType, schemaName, objectName))
                {
                    // If the object exists, change CREATE to ALTER
                    return Regex.Replace(sqlContent, @"\bCREATE\b", "ALTER", RegexOptions.IgnoreCase);
                }
            }
            else if (Regex.IsMatch(sqlContent, @"\bALTER\s+(PROCEDURE|FUNCTION)\s+(\[?\w+\]?\.)?\[?(\w+)\]?", RegexOptions.IgnoreCase))
            {
                var match = Regex.Match(sqlContent, @"\bALTER\s+(PROCEDURE|FUNCTION)\s+(\[?\w+\]?\.)?\[?(\w+)\]?", RegexOptions.IgnoreCase);
                string objectType = match.Groups[1].Value; // PROCEDURE or FUNCTION
                string schemaName = match.Groups[2].Value.TrimEnd('.').Trim('[', ']'); // Schema 
                string objectName = match.Groups[3].Value; // Object name

                // Default to 'dbo' if no schema 
                if (string.IsNullOrEmpty(schemaName))
                {
                    schemaName = "dbo";
                }

                if (!CheckIfObjectExists(connectionString, objectType, schemaName, objectName))
                {
                    // If the object does not exist, change ALTER to CREATE
                    return Regex.Replace(sqlContent, @"\bALTER\b", "CREATE", RegexOptions.IgnoreCase);
                }
            }

            // Return the original content if no changes are made
            return sqlContent;
        }


        static bool CheckIfObjectExists(string connectionString, string objectType, string schemaName, string objectName)
        {
            string query = $@"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM sys.objects 
                    WHERE object_id = OBJECT_ID(N'[{schemaName}].[{objectName}]') 
                    AND type = '{GetObjectTypeCode(objectType)}') 
                THEN 1 ELSE 0 END";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    return (int)command.ExecuteScalar() == 1;
                }
            }
        }

        // Function to map object types to their corresponding codes in sys.objects
        static string GetObjectTypeCode(string objectType)
        {
            var objectTypeCodes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "PROCEDURE", "P" }, // Stored Procedure
                { "FUNCTION", "FN" }, // Scalar Function
                { "TABLE VALUED FUNCTION", "TF" }, // Table-Valued Function
                { "AGGREGATE FUNCTION", "AF" } // Aggregate Function
            };

            // Try to get the corresponding code from the dictionary
            if (objectTypeCodes.TryGetValue(objectType, out var value))
            {
                return value;
            }

            // Throw an exception if the object type is unknown
            throw new ArgumentException($"Unknown object type: {objectType}");
        }
    }
}
