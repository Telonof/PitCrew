using Avalonia.Controls;
using Avalonia.Interactivity;

namespace PitCrew.Views;

public partial class TextInputWindow : Window
{
    private string text = "";

    public TextInputWindow()
    {
        InitializeComponent();
    }

    public string GetText()
    {
        return text;
    }

    private void OnLoad(object sender, RoutedEventArgs e)
    {
        Input.Focus();
    }

    private void SubmitButton_Click(object sender, RoutedEventArgs e)
    {
        text = Input.Text;
        Close();
    }
}