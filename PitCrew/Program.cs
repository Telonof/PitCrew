using Avalonia;
using PitCrewCommon;
using System;
using System.Runtime.InteropServices;

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

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .WithInterFont()
                .LogToTrace();
    }
}
