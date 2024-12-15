using System.Data.SqlClient;
using System.Diagnostics;

namespace InformacijosKodavimas
{
    public class Database
    {
        private string _connectionString
        {
            get => $"Data Source={Server}; Initial Catalog={DatabaseName}; User ID={User}; Password={Password}";
        }

        public string? Server = "localhost";
        public string? DatabaseName;
        public string? User = "sa";
        public string? Password;
        public string? SelectedTable;
        public string IdentityColumn = "id";
        public string? IdentityValue;
        public string? EditableColumn;

        private void HandleException(Exception exception)
        {
            Console.WriteLine("Įvyko klaida vykdant SQL užklausą!");
            Console.Write(">>> ");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(exception.Message);
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        public string? GetRowColumnText()
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                connection.Open();
                var query = $"SELECT [{EditableColumn}] FROM [{SelectedTable}] WHERE [{IdentityColumn}] = {IdentityValue}";
                using var command = new SqlCommand(query, connection);
                return command.ExecuteScalar()?.ToString();
            }
            catch (Exception exception)
            {
                HandleException(exception);
                return null;
            }
        }

        public Dictionary<string, string>? SelectData(List<string> columns)
        {
            try
            {
                Dictionary<string, string> data = new();
                using SqlConnection connection = new(_connectionString);
                connection.Open();
                var columnsString = string.Join(",", columns);
                var query = $"SELECT TOP 1 {columnsString} FROM [{SelectedTable}] WHERE [{IdentityColumn}] = {IdentityValue}";
                Debug.WriteLine(query);
                using var command = new SqlCommand(query, connection);
                using var reader = command.ExecuteReader();
                while (reader.Read())
                    for (int i = 0; i < reader.FieldCount; i++)
                        data.Add(columns[i], reader[i].ToString() ?? "");
                return data;
            }
            catch (Exception exception)
            {
                HandleException(exception);
                return null;
            }
        }

        public string? SetRowColumnText(string newValue)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                connection.Open();
                var query = $"UPDATE [{SelectedTable}] SET [{EditableColumn}] = @value WHERE [{IdentityColumn}] = {IdentityValue}";
                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@value", newValue);
                return command.ExecuteScalar()?.ToString();
            }
            catch (Exception exception)
            {
                HandleException(exception);
                return null;
            }
        }

        public List<string>? GetTables()
        {
            try
            {
                List<string> tables = new();
                using SqlConnection connection = new(_connectionString);
                connection.Open();
                var query = "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'";
                using var command = new SqlCommand(query, connection);
                using var reader = command.ExecuteReader();
                while (reader.Read())
                    tables.Add(reader.GetString(0));
                return tables;
            }
            catch (Exception exception)
            {
                HandleException(exception);
                return null;
            }
        }

        public List<string>? GetTableColumns(out string? identityColumn)
        {
            identityColumn = null;
            try
            {
                List<string> columns = new();
                using SqlConnection connection = new(_connectionString);
                connection.Open();
                var query = @"
                    SELECT
                        COLUMN_NAME,
                        COLUMNPROPERTY(OBJECT_ID(TABLE_NAME), COLUMN_NAME, 'IsIdentity')
                    FROM INFORMATION_SCHEMA.COLUMNS
                        WHERE TABLE_NAME = @table";
                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@table", SelectedTable);
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    columns.Add(reader.GetString(0));
                    if (reader.GetInt32(1) == 1)
                        identityColumn = reader.GetString(0);
                }
                return columns;
            }
            catch (Exception exception)
            {
                HandleException(exception);
                return null;
            }
        }
    }
}
