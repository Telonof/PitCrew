using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel;
using System.Reflection;

namespace PitCrew.Models
{
    /**
    <summary>
    Serves as a bridge between the models in PitCrewCommon and the GUI.
    </summary>
    */
    public abstract class ModelConverter<T> : ObservableObject
    {
        public readonly T BaseModel;

        public ModelConverter(T baseModel)
        {
            BaseModel = baseModel;
        }

        //If a property gets edited in the GUI, find if the BaseModel has a property with the same name and edit it there.
        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            PropertyInfo? baseModelProperty = typeof(T).GetProperty(e.PropertyName);
            PropertyInfo? GUIModelProperty = GetType().GetProperty(e.PropertyName);

            if (baseModelProperty == null || GUIModelProperty == null)
                return;

            var value = GUIModelProperty.GetValue(this);
            baseModelProperty.SetValue(BaseModel, value);
        }
    }
}
