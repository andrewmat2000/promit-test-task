using System.Text.RegularExpressions;

namespace PromITTestTask;
/// <summary>
/// Класс для парса текста. По заданным условиям принимаются слова от 3 до 20 символов на кириллице или латинице.
/// </summary>
public partial class TextParser {
  /// <summary>
  /// Текст регулярки, которой будет вестить проверка на валидность строки.
  /// </summary>
  private const string WordCheckString = @"^[A-zА-я]{3,20}$";
  /// <summary>
  /// Новая реализация регулярных выражений(>=.net 7).
  /// </summary>
  /// <returns>Regex.</returns>
  [GeneratedRegex(WordCheckString)]
  private static partial Regex WordCheckRegex();
  /// <summary>
  /// Новая реализация регулярных выражений(>=.net 7).
  /// </summary>
  private readonly Regex _wordCheckRegex = WordCheckRegex();
  /// <summary>
  /// Словарь. Ключ - слово, значение - количество повторений.
  /// </summary>
  public IReadOnlyDictionary<string, int> WordDictionary => _wordDictionary;
  /// <summary>
  /// Внутренний изменяемый словарь.
  /// </summary>
  private readonly Dictionary<string, int> _wordDictionary = [];
  /// <summary>
  /// Метод для добавления слов в словарь.
  /// </summary>
  /// <param name="lines">Массив из строк текста.</param>
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