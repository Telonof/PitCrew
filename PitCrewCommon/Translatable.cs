using System.Text.Json;

namespace PitCrewCommon
{
    public class Translatable
    {
        private static Dictionary<string, string> Translations { get; set; } = [];

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

            Translations.Clear();
            //If invalid json, simply don't use it.
            try
            {
                Translations = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
            }
            catch (JsonException e)
            {
                Logger.Error(201, e.StackTrace);
            }
        }

        public static string Get(string key)
        {
            if (Translations.TryGetValue(key, out string? value))
                return value;

            //Missing translation
            return key;
        }
    }
}
