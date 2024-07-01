using System.Text.RegularExpressions;

namespace PromITTestTask;

public partial class TextParser {
  private const string WordCheckString = @"^[A-zА-я]{3,20}$";
  [GeneratedRegex(WordCheckString)]
  private static partial Regex WordCheckRegex();

  private readonly Regex _wordCheckRegex = WordCheckRegex();

  public IReadOnlyDictionary<string, int> WordDictionary => _wordDictionary;

  private readonly Dictionary<string, int> _wordDictionary = [];

  public void Append(params string[] lines) {
    foreach (var line in lines) {
      foreach (var word in line.Split(" ", StringSplitOptions.RemoveEmptyEntries)) {
        if (!_wordCheckRegex.IsMatch(word)) {
          continue;
        }
        if (_wordDictionary.TryGetValue(word, out int count)) {
          _wordDictionary[word] = count + 1;
          continue;
        }
        _wordDictionary.Add(word, 1);
      }
    }
  }
}