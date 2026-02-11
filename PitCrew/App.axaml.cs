using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using PitCrew.Systems;
using PitCrew.ViewModels;
using PitCrew.Views;
using PitCrewCommon;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using URIScheme;

namespace PitCrew
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);

            var services = new ServiceCollection();
            services.AddSingleton<IWindowService, WindowManager>();
            services.AddSingleton<ViewLocator>();
            services.AddSingleton<ConfigManagerGUI>();
            services.AddSingleton<DownloadManager>();
            Ioc.Default.ConfigureServices(services.BuildServiceProvider());

            //Register for URL hooks
            Environment.CurrentDirectory = Path.GetDirectoryName(Environment.ProcessPath);
            var service = URISchemeServiceFactory.GetURISchemeSerivce("pitcrew", "PitCrew", Environment.ProcessPath);

            if (service.CheckAny())
                service.Delete();

            service.Set();
        }

        public override void OnFrameworkInitializationCompleted()
        {
            string langName = Service.Config.GetSetting(ConfigKey.Language);

            Translatable.Initialize($"{langName}.json");
            Logger.EraseLog();

            //Right to left language support
            if (CultureInfo.GetCultureInfo(langName).TextInfo.IsRightToLeft)
                AddRTLStyle();

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
                // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
                DisableAvaloniaDataAnnotationValidation();
                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel(),
                };
            }
            base.OnFrameworkInitializationCompleted();
        }

        private void DisableAvaloniaDataAnnotationValidation()
        {
            // Get an array of plugins to remove
            var dataValidationPluginsToRemove =
                BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

            // remove each entry found
            foreach (var plugin in dataValidationPluginsToRemove)
            {
                BindingPlugins.DataValidators.Remove(plugin);
            }
        }

        private void AddRTLStyle()
        {
            Style rtlStyle = new Style(x => x.Class("rtl"))
            {
                Setters =
                {
                    new Setter(Control.FlowDirectionProperty, FlowDirection.RightToLeft)
                }
            };

            Style rtlMoveStyle = new Style(x => x.Class("rtlm"))
            {
                Setters =
                {
                    new Setter(Control.HorizontalAlignmentProperty, HorizontalAlignment.Right)
                }
            };

            Styles.Add(rtlStyle);
            Styles.Add(rtlMoveStyle);
        }
    }
}