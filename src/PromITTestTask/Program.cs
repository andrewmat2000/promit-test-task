using PromITTestTask;

if (args.Length == 0) {
  Console.Error.WriteLine("Add files to open.");
  return 1;
}

using var dbContext = new DbContext();

const int MaxFileSize = 1000 * 1024 * 1024;


foreach (var path in args) {
  if (!File.Exists(path)) {
    Console.Error.WriteLine(string.Format("File with path '{0}' does not exists.", path));

    continue;
  }
  var fileInfo = new FileInfo(path);

  if (fileInfo.Length > MaxFileSize) {
    Console.Error.WriteLine(string.Format("Too much file size."));

    continue;
  }

  using var stream = new StreamReader(fileInfo.OpenRead());

  var textParser = new TextParser(stream.ReadToEnd().Replace("\r\n", "\n").Split("\n"));

  foreach (var (word, count) in textParser.KeyDictionary) {
    if (count < 4) {
      continue;
    }

    await dbContext.AddWordAsync(word, count);
  }
}

foreach (var (word, count) in await dbContext.GetAllWordsWithCount()) {
  Console.WriteLine(string.Format("'{0}': {1};", word, count));
}


return 0;
