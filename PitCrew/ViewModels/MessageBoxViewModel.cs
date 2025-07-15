using CommunityToolkit.Mvvm.ComponentModel;
using PitCrew.Systems;
using PitCrewCommon;
using System;

namespace PitCrew.ViewModels
{
    public partial class MessageBoxViewModel : ViewModelBase
    {
        [ObservableProperty]
        public string message;

        [ObservableProperty]
        public string okButtonText = Translatable.Get("button.ok");

        [ObservableProperty]
        public string cancelButtonText = Translatable.Get("button.cancel");

        [ObservableProperty]
        public bool cancelVisible = false;

        public ResultType Result { get; set; } = ResultType.None;

        public MessageBoxViewModel(string text, ButtonType type = ButtonType.Ok)
        {
            Message = text.Replace("\\n", Environment.NewLine);

            if (type == ButtonType.YesNo)
            {
                OkButtonText = Translatable.Get("button.yes");
                CancelButtonText = Translatable.Get("button.no");
            }

            if (type != ButtonType.Ok)
                CancelVisible = true;
        }

        public void Confirm()
        {
            Result = ResultType.OK;
            Service.WindowManager.CloseWindow(this);
        }

        public void Cancel()
        {
            Result = ResultType.Cancel;
            Service.WindowManager.CloseWindow(this);
        }

        public enum ResultType
        {
            /** 
            <summary>
            OK is for both the OK and Yes buttons.
            </summary>
            */
            OK,
            /** 
            <summary>
            None is when the user exits out the message box.
            </summary>
            */
            None,
            /**
            <summary>
            Cancel works for both the cancel and no buttons.
            </summary>
            */
            Cancel
        }

        public enum ButtonType
        {
            YesNo,
            Ok,
            OkCancel
        }
    }
}
