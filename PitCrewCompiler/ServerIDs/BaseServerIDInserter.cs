using PitCrewCommon;
using PitCrewCommon.Utilities;
using System;
using System.Xml.Linq;

namespace PitCrewCompiler.ServerIDs
{
    /*
     * This serves as a tool to automatically generate and add IDs for use in the TCU Server Emulator so things such
     * as custom missions/vinyls/cars (eventually) and more can be added successfully.
     * This tool does not apply to The Crew 2 at all.
     */
    public abstract class BaseServerIDInserter
    {
        private readonly string FilePath;

        protected XDocument XmlData { get; private set; }

        public BaseServerIDInserter(string filePath, string database)
        {
            FilePath = filePath;
            XmlData = new XDocument(new XElement(database));
            InsertInitialInformation();
            XmlData.Root.Add(new XComment(Translatable.Get("compiler.xml-warning")));
        }

        /*
         * This allows different id systems (items/missions/cars) 
         * To alter their structure indivudally depending on how each file needs
         * to be formatted.
         */
        public abstract void InsertInitialInformation();

        public void SaveData()
        {
            FileUtil.CheckAndDeleteFile(FilePath);
            File.WriteAllText(FilePath, XmlData.ToString());
        }
    }
}
