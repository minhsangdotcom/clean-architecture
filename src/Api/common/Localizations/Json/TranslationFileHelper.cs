using System.Text.Json;

namespace Api.common.Localizations.Json;

public class TranslationFileHelper
{
    public static void SyncTranslationFile(string filePath, List<string> sourceMessages)
    {
        Dictionary<string, string> existing = [];

        if (!File.Exists(filePath))
        {
            File.WriteAllText(filePath, "{}");
        }

        // 2. Read current file
        string json = File.ReadAllText(filePath);
        if (!string.IsNullOrWhiteSpace(json) && json != "{}")
        {
            existing = JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? [];
        }

        // 3. add missing key
        List<string> keysToAdd = sourceMessages.FindAll(x => !existing.ContainsKey(x));
        foreach (var key in keysToAdd)
        {
            existing[key] = "";
        }

        // 4. Remove deleted keys
        List<string> keysToRemove = [.. existing.Keys.Where(key => !sourceMessages.Contains(key))];
        foreach (var key in keysToRemove)
        {
            existing.Remove(key);
        }

        // 5. Save pretty JSON
        if (keysToAdd.Count == 0 && keysToRemove.Count == 0)
        {
            return;
        }
        string output = JsonSerializer.Serialize(
            existing,
            new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            }
        );

        File.WriteAllText(filePath, output);
    }
}
