using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using PitCrewCommon;

namespace PitCrew.Views;

public partial class MessageBox : Window
{
    private Result lastResult = Result.None;

    public MessageBox()
    {
        InitializeComponent();
    }

    public async Task<Result> ShowDialog(Window owner, string text, ButtonType type, string title)
    {
        //Set title
        if (title.Equals("Info"))
            title = Translate.Get("messagebox-title-default");
        this.Title = title;

        //Set text
        InfoText.Text = text.Replace("\\n", Environment.NewLine);

        //Setup button types
        if (type == ButtonType.YesNo)
        {
            ConfirmButton.Content = Translate.Get("button.yes");
            CancelButton.Content = Translate.Get("button.no");
        }
        if (type != ButtonType.Ok)
            CancelButton.IsVisible = true;

        //Show
        await ShowDialog(owner);
        return lastResult;
    }

    private void ConfirmButton_Click(object sender, RoutedEventArgs e)
    {
        lastResult = Result.OK;
        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        lastResult = Result.Cancel;
        Close();
    }

    public enum Result
    {
        /// <summary>
        /// OK is for both the OK and Yes buttons.
        /// </summary>
        OK,
        /// <summary>
        /// None is when the user exits out the message box.
        /// </summary>
        None,
        /// <summary>
        /// Cancel works for both the cancel and no buttons.
        /// </summary>
        Cancel
    }

    public enum ButtonType
    {
        YesNo,
        Ok,
        OkCancel
    }
}