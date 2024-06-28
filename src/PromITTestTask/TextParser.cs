using System.Text.RegularExpressions;

namespace PromITTestTask;

public partial class TextParser {
  private const string WordCheckString = @"^[A-zА-я]{3,20}$";
  [GeneratedRegex(WordCheckString)]
  private static partial Regex WordCheckRegex();

  private readonly Regex _wordCheckRegex = WordCheckRegex();

  public IReadOnlyDictionary<string, int> KeyDictionary { get; }

  public TextParser(params string[] lines) {
    var dictionary = new Dictionary<string, int>();

    foreach (var line in lines) {
      foreach (var word in line.Split(" ", StringSplitOptions.RemoveEmptyEntries)) {
        if (!_wordCheckRegex.IsMatch(word)) {
          continue;
        }
        if (dictionary.TryGetValue(word, out int count)) {
          dictionary[word] = count + 1;
          continue;
        }
        dictionary.Add(word, 1);
      }
    }

    KeyDictionary = dictionary;
  }
}