using Gibbed.Dunia2.FileFormats;
using PitCrewCommon;
using System;
using System.Xml.Linq;

namespace PitCrewCompiler.ServerIDs
{
    internal class ItemsServerIDInserter : BaseServerIDInserter
    {
        private readonly string physParts = "physparts";

        private readonly string renderParts = "renderparts";

        public ItemsServerIDInserter(string filePath, string database) : base(filePath, database)
        {
        }

        /* Grab the first comment which contains documentation
         * and populate root with the two seperate nodes.
         */
        public override void InsertInitialInformation()
        {
            XDocument? itemsInfo = null;
            itemsInfo = XDocument.Load(Path.Combine("Assets", Constants.SERVER_ID_ITEMS_FILE));
            XmlData.Root.Add(new XElement(renderParts));
            XmlData.Root.Add(new XElement(physParts));
            XmlData.Add(itemsInfo.DescendantNodes().OfType<XComment>().First());
        }

        public void AddItem(string type, BabelDBFile file, int rowIndex, bool phys, int db_index)
        {
            //phys parts are set with Slot ID's. Render not so much.
            string addTo = renderParts;
            if (phys)
                addTo = physParts;

            XElement item = new XElement(type);

            //Grab all the data from one specific row and add it as an item to the ID list.
            for (int i = 0; i < file.Columns.Count; i++)
            {
                byte[] data = (byte[])file.Rows[rowIndex].data[i].Clone();
                Array.Reverse(data);

                item.SetAttributeValue(file.Columns[i].Name, $"0x{Convert.ToHexString(data)}");
            }

            //set the index based on vanilla values + where it exists modded.
            item.SetAttributeValue("db_index", db_index);

            XmlData.Root.Element(addTo).Add(item);
        }
    }
}
