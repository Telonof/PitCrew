using Avalonia.Controls;
using Avalonia.Input;
using System.Diagnostics;

namespace PitCrew.Views;

internal partial class AboutWindow : Window
{
    public AboutWindow()
    {
        InitializeComponent();
    }

    private void DiscordLinkEnter(object sender, PointerEventArgs e)
    {
        if (sender is not TextBlock block)
            return;
        
        block.Cursor = new Cursor(StandardCursorType.Hand);
    }

    private void DiscordLinkClick(object sender, PointerPressedEventArgs e) {
        ProcessStartInfo discordLink = new ProcessStartInfo("https://discord.com/invite/gUczTkphGE")
        {
            UseShellExecute = true
        };
        Process.Start(discordLink);
    }
}