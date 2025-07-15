using System.Collections.ObjectModel;

namespace PitCrewCommon
{
    public class Logger
    {
        public static ObservableCollection<string> Logs { get; set; } = [];

        public static void Log(string message)
        {
            //Keep buffer clean, this is an abritary number.
            if (Logs.Count > 150)
                Logs.Clear();


            Logs.Add(message);
            File.AppendAllLines("PitCrew.log", [message]);
        }

        public static void Print(string message)
        {
            DateTime time = DateTime.Now;
            string customTime = time.ToString("HH:mm:ss");
            message = $"[{customTime}] {message}";

            Log(message);
            Console.WriteLine(message);
            Console.ResetColor();
        }

        public static void Warn(int warnNumber, string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Print($"WARN{warnNumber}: {message}");
        }

        public static void Error(int errorNumber, string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Print($"ERR{errorNumber}: {message}");
        }

        public static void EraseLog()
        {
            File.WriteAllLines("PitCrew.log", []);
        }
    }
}
