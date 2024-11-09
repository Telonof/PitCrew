using PitCrewCommon;

namespace PitCrew.Models;

internal class ModInfo
{
    public string ID { get; set; }

    public string Name { get; set; }

    public string Author {  get; set; }

    public string Description { get; set; }

    public bool Enabled { get; set; }

    public ModInfo()
    {
        ID="Default";
        Name = Translate.Get("modinfo.default-name");
        Author=Translate.Get("modinfo.default-author");
        Description=Translate.Get("modinfo.default-description");
        Enabled = true;
    }
}
