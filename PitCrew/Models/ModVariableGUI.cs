using System.ComponentModel;
using System.Xml.Linq;
using CommunityToolkit.Mvvm.ComponentModel;

namespace PitCrew.Models;

public partial class ModVariableGUI : ObservableObject
{
    [ObservableProperty]
    public string name;
        
    [ObservableProperty]
    public string value;

    private string fullPath;

    
    public ModVariableGUI(string name, string value, string fullPath)
    {
        this.name = name;
        this.value = value;
        this.fullPath = fullPath;
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        XDocument doc = XDocument.Load(fullPath);
        if (doc.Root.Element("variables")?.Element(Name) == null)
            return;
        
        doc.Root.Element("variables").Element(Name).Value = Value;
        doc.Save(fullPath);
        
        base.OnPropertyChanged(e);
    }

}