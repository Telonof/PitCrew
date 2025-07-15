namespace PitCrewCommon.Models
{
    public struct ModFile
    {
        public string Location { get; set; }
        public int Priority { get; set; }
        public Mod ParentMod { get; set; }

        public ModFile(Mod parentMod, string location, int priority)
        {
            this.ParentMod = parentMod;
            this.Location = location;
            this.Priority = priority;
        }
    }
}
