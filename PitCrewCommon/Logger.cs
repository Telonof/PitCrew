using System.Collections.ObjectModel;

namespace PitCrewCommon
{
    public class Logger
    {
        public static ObservableCollection<string> Logs { get; set; } = [];

        public static void WriteAndFlush()
        {
            Write();
            Logs.Clear();
        }

        public static void Write()
        {
            File.AppendAllLines("PitCrew.log", Logs);
        }

        public static void Print(string message)
        {
            DateTime time = DateTime.Now;
            string customTime = time.ToString("HH:mm:ss");
            message = $"[{customTime}] {message}";

            Logs.Add(message);
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
            Write();
        }

        public static void EraseLog()
        {
            File.WriteAllLines("PitCrew.log", []);
        }
    }
}
