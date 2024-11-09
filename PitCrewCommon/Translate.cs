using System.Text.Json;

namespace PitCrewCommon;

public class Translate
{
    private static Dictionary<string, string> translations { get; set; } = [];

    public static void Initialize(string fileName)
    {
        string path = Path.Combine("Languages", fileName);

        if (!File.Exists(path))
            fileName = "English.json";

        Load(fileName);
    }

    public static void Load(string fileName)
    {
        string path = Path.Combine("Languages", fileName);

        if (!File.Exists(path))
            return;

        string json = File.ReadAllText(path);
        
        translations.Clear();
        //If invalid json, simply don't use it.
        try 
        {
            translations = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
        }
        catch (JsonException e)
        {
            Console.WriteLine(e.StackTrace);
        }
    }

    public static string Get(string key)
    {
        if (translations.TryGetValue(key, out string? value))
            return value;

        //Missing translation
        return key;
    }
}
