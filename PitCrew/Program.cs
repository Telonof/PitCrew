using Avalonia;
using PitCrewCommon;
using System;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Text;

namespace PitCrew
{
    internal sealed class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static void Main(string[] args)
        {
            //It acted solely as a client to install mods to.
            if (CreateNamedClient(args))
                return;

            //Apparently documentation says this is more than fine to catch crashes.
            try
            {
                BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
            }
            catch (Exception ex)
            {
                Logger.Print("====================");
                Logger.Print("CRASHDUMP");
                Logger.Print($"Version: {Constants.VERSION}");
                Logger.Print($"OS: {RuntimeInformation.OSDescription}");
                Logger.Error(0, $"{ex.Message}");
                Logger.Print($"Trace: {ex.StackTrace}");
                Logger.Print("====================");
            }
        }

        private static bool CreateNamedClient(string[] args)
        {
            if (args.Length <= 0)
                return false;

            using NamedPipeClientStream client = new NamedPipeClientStream(".", "pitcrewlistener", PipeDirection.InOut, PipeOptions.Asynchronous);
            client.Connect(100);

            if (client.IsConnected)
            {
                byte[] data = Encoding.UTF8.GetBytes(args[0]);
                byte[] dataLength = BitConverter.GetBytes(data.Length);
                client.Write(dataLength, 0, dataLength.Length);
                client.Write(data, 0, data.Length);
            }

            return true;
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .WithInterFont()
                .LogToTrace();
    }
}
