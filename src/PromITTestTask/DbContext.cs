using Microsoft.Data.SqlClient;

namespace PromITTestTask;

public class DbContext : IDisposable {
  private const string DefaultTableName = "my_test_table";
  private readonly SqlConnection _sqlConnection;
  private readonly string _checkTableCommand = string.Format("SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{0}';", DefaultTableName);
  private readonly string _addValueCommand = string.Format(@"INSERT INTO {0} (word, repeats)
VALUES (@word, @repeats);", DefaultTableName);
  private readonly string[] _initCommands = [
    string.Format(@"CREATE TABLE {0} (
        id INT IDENTITY(1,1) PRIMARY KEY,
        word varchar(20) NOT NULL CHECK (LEN(word) >= 3),
        repeats int NOT NULL CHECK (repeats >= 4)
    );", DefaultTableName),
    string.Format("CREATE INDEX word_index ON {0} (word);", DefaultTableName)
  ];
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
      }

      _disposedValue = true;
    }
  }

  public void Dispose() {
    Dispose(disposing: true);
    GC.SuppressFinalize(this);
  }

  private bool Init() {
    using var request = new SqlCommand(_checkTableCommand, _sqlConnection);

    var exists = request.ExecuteScalar() != null;

    if (exists) {
      return false;
    }

    foreach (var initCommand in _initCommands) {
      using var command = _sqlConnection.CreateCommand();
      command.CommandText = initCommand;

      command.ExecuteNonQuery();
    }

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