using System.Collections.ObjectModel;

namespace PitCrewCommon
{
    public class Logger
    {
        private static readonly object LogsLock = new object();
        
        public static ObservableCollection<string> Logs { get; set; } = [];
        
        
        public static void Print(string message)
        {
            DateTime time = DateTime.Now;
            string customTime = time.ToString("HH:mm:ss");
            message = $"[{customTime}] {message}";

            lock (LogsLock)
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
        }

        public static void WriteAndFlush()
        {
            Write();
            Logs.Clear();
        }

        public static void Write()
        {
            File.AppendAllLines("PitCrew.log", Logs);
        }

        public static void EraseLog()
        {
            File.WriteAllLines("PitCrew.log", []);
        }
        
        public static string GetLogsForGui()
        {
            string logs;
            lock (LogsLock)
                logs = string.Join(Environment.NewLine, Logs);
            
            return logs;
        }
    }
}
