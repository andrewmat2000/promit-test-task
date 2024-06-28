using Microsoft.Data.SqlClient;

namespace PromITTestTask;

public class DbContext : IDisposable {
  private const string DefaultTableName = "my_test_table";
  private const string CheckTableCommand = "SELECT * FROM INFORMATION_SCHEMA.TABLES;";
  private readonly SqlConnection _sqlConnection;
  private readonly string _addValueCommand = string.Format(@"INSERT INTO {0} (word, repeats)
VALUES (@word, @repeats);", DefaultTableName);
  private readonly string _initCommand = string.Format(@"CREATE TABLE {0} (
        id INT IDENTITY(1,1) PRIMARY KEY,
        word varchar(20) NOT NULL,
        repeats int NOT NULL
    );", DefaultTableName);
  private readonly string _getAllCommand = string.Format(@"SELECT DISTINCT word, MAX(repeats)
    FROM {0}
    GROUP BY word
    ORDER BY MAX(repeats) DESC, word;", DefaultTableName);

  private readonly string _getLastCommand = string.Format("SELECT TOP 1 * FROM {0} WHERE word=@word ORDER BY repeats DESC;", DefaultTableName);
  private bool _disposedValue = false;

  public async Task<IReadOnlyDictionary<string, int>> GetAllWordsWithCount() {
    var dictionary = new Dictionary<string, int>();

    using var command = new SqlCommand(_getAllCommand, _sqlConnection);

    using var reader = await command.ExecuteReaderAsync();

    while (await reader.ReadAsync()) {
      dictionary.Add(reader.GetString(0), reader.GetInt32(1));
    }

    return dictionary;
  }

  public async Task<bool> AddWordAsync(string word, int count) {
    int repeats = 0;

    using var request = new SqlCommand(_getLastCommand, _sqlConnection);

    request.Parameters.AddWithValue("@word", word);

    using (var reader = await request.ExecuteReaderAsync()) {

      while (await reader.ReadAsync()) {
        repeats = (int)reader["repeats"];
      }
    }

    using var command = new SqlCommand(_addValueCommand, _sqlConnection);

    command.Parameters.AddWithValue("@word", word);
    command.Parameters.AddWithValue("@repeats", repeats + count);

    await command.ExecuteNonQueryAsync();

    return true;
  }

  protected virtual void Dispose(bool disposing) {
    if (!_disposedValue) {
      if (disposing) {
        _sqlConnection.Close();
        Console.WriteLine("Connection closed.");
        // TODO: dispose managed state (managed objects)
      }

      // TODO: free unmanaged resources (unmanaged objects) and override finalizer
      // TODO: set large fields to null
      _disposedValue = true;
    }
  }

  // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
  // ~DbContext()
  // {
  //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
  //     Dispose(disposing: false);
  // }

  public void Dispose() {
    // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    Dispose(disposing: true);
    GC.SuppressFinalize(this);
  }

  private bool Init() {
    var request = new SqlCommand(CheckTableCommand, _sqlConnection);

    using (var reader = request.ExecuteReader()) {

      while (reader.Read()) {
        var tableName = reader["TABLE_NAME"];

        if (tableName is string n && n == DefaultTableName) {
          return false;
        }
      }
    }

    var command = _sqlConnection.CreateCommand();
    command.CommandText = _initCommand;

    command.ExecuteNonQuery();
    return true;
  }

  public DbContext() {
    SqlConnection sqlConnection = new(Environment.GetEnvironmentVariable("CONNECTION_STRING"));

    sqlConnection.Open();

    _sqlConnection = sqlConnection;

    Console.WriteLine("Connection successfully openned.");

    Console.WriteLine(Init() ? "Table was created." : "Table already exists.");
  }
}