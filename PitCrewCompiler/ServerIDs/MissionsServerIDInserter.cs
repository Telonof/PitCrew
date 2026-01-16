using PitCrewCommon;
using System.Xml.Linq;

namespace PitCrewCompiler.ServerIDs
{
    internal class MissionsServerIDInserter : BaseServerIDInserter
    {
        public MissionsServerIDInserter(string filePath, string database) : base(filePath, database)
        {
        }

        public override void InsertInitialInformation()
        {
            XDocument? itemsInfo = null;
            itemsInfo = XDocument.Load(Path.Combine("Assets", Constants.SERVER_ID_MISSIONS_FILE));
            XmlData.Add(itemsInfo.DescendantNodes().OfType<XComment>().First());
        }

        /* Missions do not require any indexing nor do they have their own babdb system,
         * so modders will just need to make them manually.
         */
        public void AddMissions(XElement element)
        {
            foreach (XElement mission in element.Descendants())
            {
                XmlData.Root.Add(mission);
            }
        }
    }
}
