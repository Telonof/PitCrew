namespace PitCrew
{
    internal struct FileEntry
    {
        public string modPath;

        public int priority;

        public FileEntry()
        {
            priority = 998;
        }

        public override string ToString()
        {
            return $"{modPath} {priority}";
        }
    }
}
