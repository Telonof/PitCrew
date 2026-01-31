using CommunityToolkit.Mvvm.DependencyInjection;

namespace PitCrew.Systems
{
    internal class Service
    {
        public static ConfigManagerGUI Config => Ioc.Default.GetRequiredService<ConfigManagerGUI>();

        public static IWindowService WindowManager => Ioc.Default.GetRequiredService<IWindowService>();
        public static DownloadManager DownloadManager => Ioc.Default.GetRequiredService<DownloadManager>();
    }
}
