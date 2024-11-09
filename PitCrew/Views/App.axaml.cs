using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using PitCrewCommon;

namespace PitCrew.Views;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        ConfigManager config = new ConfigManager();

        //Setup language
        Translate.Initialize(config.GetSetting("Lang") + ".json");
        
        MainWindow owner = new MainWindow();
        IM manager = new IM();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = owner;
        }

        manager.Initialize(owner, config);
        owner.Initialize();
        base.OnFrameworkInitializationCompleted();
    }
}