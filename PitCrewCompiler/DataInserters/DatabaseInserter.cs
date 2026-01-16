using Dunia2.MergeBinaryObject;
using Gibbed.Dunia2.FileFormats;
using PitCrewCommon;
using PitCrewCommon.Utilities;
using PitCrewCompiler.ServerIDs;
using static PitCrewCompiler.DataInserters.FileMerger;

namespace PitCrewCompiler.DataInserters
{
    internal class DatabaseInserter
    {
        private readonly Dictionary<DBType, List<ulong>> ExistingIds = [];
        private readonly Dictionary<DBType, HashSet<ulong>> CurrentSessionIds = [];
        private readonly Dictionary<DBType, HashSet<Entry>> NeededAdditions = [];

        private readonly string MergingFolder;

        private readonly HashSet<string> ValidItemDBs = ["color", "sticker", "rim", "carinteriorpattern", "licenseplate", "driverhelmet", "driversuit", "item"];

        private BinaryObjectFile PhysDB;
        private BinaryObjectFile RenderDB;

        private const int VanillaRenderCount = 11910, VanillaPhysCount = 7260;
        private const int MaxTCURenderSaveCount = 16000, MaxTCUPhysSaveCount = 8960;
        private readonly int PackageVersion;

        public DatabaseInserter(Dictionary<string, HashSet<XmlFile>> mergingBdbs, string mergingFolder, string gameDirectory, int packageVersion)
        {
            bool isCrew1 = packageVersion == 5;
            MergingFolder = mergingFolder;
            PackageVersion = packageVersion;

            ExistingIds.Add(DBType.PHYS, []);
            ExistingIds.Add(DBType.RENDER, []);
            CurrentSessionIds.Add(DBType.PHYS, []);
            CurrentSessionIds.Add(DBType.RENDER, []);
            NeededAdditions.Add(DBType.PHYS, []);
            NeededAdditions.Add(DBType.RENDER, []);

            ItemsServerIDInserter itemInserter = new(Path.Combine(FileUtil.GetParentDir(gameDirectory), Constants.SERVER_ID_ITEMS_FILE), "items");
            BabelDBMerger merger = new BabelDBMerger();
            Stream stream;

            LoadClientDatabases(gameDirectory);

            foreach (string key in mergingBdbs.Keys)
            {
                if (isCrew1 && key.Equals("server_missions"))
                {
                    MissionIDInserter(mergingBdbs[key], gameDirectory);
                    continue;
                }

                BabelDBFile bdb = new BabelDBFile();
                stream = File.OpenRead(key);
                bdb.Deseralize(stream);
                int originalRowCount = bdb.Rows.Count;
                stream.Close();
                
                foreach (XmlFile item in mergingBdbs[key])
                {
					Logger.Print(string.Format(Translatable.Get("compiler.merging-file"), Path.GetFileName(item.Location), Path.GetFileName(key)));
                    bdb = merger.Merge(bdb, item.XmlData);
                    PercentageCalculator.IncrementProgress();
                }

                stream = File.OpenWrite(key);
                bdb.Serialize(stream);
                stream.Close();

                if (!isCrew1 || originalRowCount >= bdb.Rows.Count)
                    continue;

                if (!ValidItemDBs.Contains(Path.GetFileNameWithoutExtension(key)))
                    continue;

                ItemIDInserter(itemInserter, bdb, Path.GetFileNameWithoutExtension(key), originalRowCount);
            }

            if (!isCrew1)
                return;

            OverwriteDeadIDs(itemInserter);

            stream = new MemoryStream();
            PhysDB.Serialize(stream);
            File.WriteAllBytes(Path.Combine(gameDirectory, "mods", "PitCrewPhysDatabase.bin"), ((MemoryStream)stream).ToArray());
            File.WriteAllBytes(Path.Combine(MergingFolder, "road66database", "physcarpartdb.bin"), ((MemoryStream)stream).ToArray());

            stream = new MemoryStream();
            RenderDB.Serialize(stream);
            File.WriteAllBytes(Path.Combine(gameDirectory, "mods", "PitCrewRenderDatabase.bin"), ((MemoryStream)stream).ToArray());
            File.WriteAllBytes(Path.Combine(MergingFolder, "road66database", "rendercarpartdb.bin"), ((MemoryStream)stream).ToArray());

            itemInserter.SaveData();
        }

        private void LoadClientDatabases(string gameDirectory)
        {
            if (PackageVersion == Constants.THE_CREW_2)
                return;

            //Load from direct mods folder if they exist since they persist with previous data.
            string renderdb = Path.Combine(gameDirectory, "mods", "PitCrewRenderDatabase.bin");
            string physdb = Path.Combine(gameDirectory, "mods", "PitCrewPhysDatabase.bin");

            if (!File.Exists(renderdb))
                renderdb = Path.Combine(MergingFolder, "road66database", "rendercarpartdb.bin");
            if (!File.Exists(physdb))
                physdb = Path.Combine(MergingFolder, "road66database", "physcarpartdb.bin");

            FileStream stream = File.OpenRead(physdb);
            PhysDB = new BinaryObjectFile();
            PhysDB.Deserialize(stream);
            stream.Close();

            stream = File.OpenRead(renderdb);
            RenderDB = new BinaryObjectFile();
            RenderDB.Deserialize(stream);
            stream.Close();

            BinaryObject renderList = RenderDB.Root.Children[0];
            BinaryObject physList = PhysDB.Root.Children[0];

            //Add all modded id's to the existing ids list to then check if the mods being loaded
            //at compile already exist in the list.
            for (int i = VanillaRenderCount; i < renderList.Children.Count; i++)
            {
                ExistingIds[DBType.RENDER].Add(BitConverter.ToUInt64(renderList.Children[i].Children[0].Fields[3301132975]));
            }
            for (int i = VanillaPhysCount; i < physList.Children.Count; i++)
            {
                ExistingIds[DBType.PHYS].Add(BitConverter.ToUInt64(physList.Children[i].Children[0].Fields[3301132975]));
            }
        }

        private void MissionIDInserter(HashSet<XmlFile> files, string gameDirectory)
        {
            MissionsServerIDInserter missionInserter = new(Path.Combine(FileUtil.GetParentDir(gameDirectory), Constants.SERVER_ID_MISSIONS_FILE), "missions");

            foreach (XmlFile file in files)
            {
                Logger.Print(string.Format(Translatable.Get("compiler.merging-file"), Path.GetFileName(file.Location), "server_missions"));
                missionInserter.AddMissions(file.XmlData.Root);

                PercentageCalculator.IncrementProgress();
            }
            missionInserter.SaveData();
        }

        private void ItemIDInserter(ItemsServerIDInserter itemInserter, BabelDBFile bdb, string dbField, int originalRowCount)
        {
            //for these items in particular, [0] is always "id"
            for (int i = originalRowCount; i < bdb.Rows.Count; i++)
            {
                ulong id = BitConverter.ToUInt64(bdb.Rows[i].data[0]);

                DBType type = DBType.RENDER;
                if (bdb.Columns.Where(column => column.Name.Equals("SlotID")).Any())
                    type = DBType.PHYS;

                //If the modded id is already cached, do not do anything to the db's and just add
                //them to the server hooks.
                if (ExistingIds[DBType.PHYS].Contains(id))
                {
                    CurrentSessionIds[type].Add(id);
                    itemInserter.AddItem(dbField, bdb, i, true, VanillaPhysCount + ExistingIds[DBType.PHYS].IndexOf(id));
                    continue;
                }

                if (ExistingIds[DBType.RENDER].Contains(id))
                {
                    CurrentSessionIds[type].Add(id);
                    itemInserter.AddItem(dbField, bdb, i, false, VanillaRenderCount + ExistingIds[DBType.RENDER].IndexOf(id));
                    continue;
                }

                //If we haven't hit the save file limit and id is not cached, add to client db's
                //and server hooks.
                if (type == DBType.PHYS && PhysDB.Root.Children[0].Children.Count < MaxTCUPhysSaveCount)
                {
                    PhysDB.Root.Children[0].Children.Add(GenerateBinaryObject(bdb.Rows[i].data[0]));
                    int currentChildCount = PhysDB.Root.Children[0].Children.Count - 1;
                    itemInserter.AddItem(dbField, bdb, i, true, currentChildCount);
                    CurrentSessionIds[type].Add(id);
                    continue;
                }
                
				if (type == DBType.RENDER && RenderDB.Root.Children[0].Children.Count < MaxTCURenderSaveCount)
                {
                    RenderDB.Root.Children[0].Children.Add(GenerateBinaryObject(bdb.Rows[i].data[0]));
                    int currentChildCount = RenderDB.Root.Children[0].Children.Count - 1;
                    itemInserter.AddItem(dbField, bdb, i, false, currentChildCount);
                    CurrentSessionIds[type].Add(id);
                    continue;
                }

                //we hit save file capacity, store info for later.
                Entry entry = new Entry();
                entry.id = id;
                entry.bdb = bdb;
                entry.row_index = i;
                entry.type = dbField;

                NeededAdditions[type].Add(entry);
            }
        }

        /* This method overwrites any ids in the cache that are not currently in use.
         * This is done simply because save files in TC1 can only have a max of 16k and 9k for render and phys.
         */
        private void OverwriteDeadIDs(ItemsServerIDInserter itemInserter)
        {
            List<ulong> deadPhysIDs = new List<ulong>(ExistingIds[DBType.PHYS]);
            List<ulong> deadRenderIDs = new List<ulong>(ExistingIds[DBType.RENDER]);

            deadPhysIDs.RemoveAll(item => CurrentSessionIds[DBType.PHYS].Contains(item));
            deadRenderIDs.RemoveAll(item => CurrentSessionIds[DBType.RENDER].Contains(item));

            if (NeededAdditions[DBType.RENDER].Count - deadRenderIDs.Count > 0)
                Logger.Warn(101, string.Format(Translatable.Get("compiler.too-many-ids"), deadRenderIDs.Count, NeededAdditions[DBType.RENDER].Count));

            if (NeededAdditions[DBType.PHYS].Count - deadPhysIDs.Count > 0)
                Logger.Warn(102, string.Format(Translatable.Get("compiler.too-many-ids"), deadPhysIDs.Count, NeededAdditions[DBType.PHYS].Count));
            
            foreach (Entry entry in NeededAdditions[DBType.RENDER])
            {
                if (deadRenderIDs.Count == 0)
                    break;

                int index = VanillaRenderCount + ExistingIds[DBType.RENDER].IndexOf(deadRenderIDs.First());
                RenderDB.Root.Children[0].Children[index].Children[0].Fields[3301132975] = BitConverter.GetBytes(entry.id);

                itemInserter.AddItem(entry.type, entry.bdb, entry.row_index, false, index);

                deadRenderIDs.Remove(deadRenderIDs.First());
            }

            foreach (Entry entry in NeededAdditions[DBType.PHYS])
            {
                if (deadPhysIDs.Count == 0)
                    break;

                int index = VanillaPhysCount + ExistingIds[DBType.PHYS].IndexOf(deadPhysIDs.First());
                PhysDB.Root.Children[0].Children[index].Children[0].Fields[3301132975] = BitConverter.GetBytes(entry.id);

                itemInserter.AddItem(entry.type, entry.bdb, entry.row_index, true, index);

                deadPhysIDs.Remove(deadPhysIDs.First());
            }
        }

        private BinaryObject GenerateBinaryObject(byte[] id)
        {
            BinaryObject obj = new BinaryObject();
            obj.NameHash = 1594519158;
            BinaryObject internalObj = new BinaryObject();
            internalObj.NameHash = 3682897169;
            internalObj.Fields.Add(3098713127, [67, 73, 84, 67, 97, 114, 80, 97, 114, 116, 69, 108, 101, 109, 101, 110, 116, 0]);
            internalObj.Fields.Add(624329766, [170, 6, 90, 181]);
            internalObj.Fields.Add(3301132975, id);
            obj.Children.Add(internalObj);
            return obj;
        }

        //Gosh I hate the dual render/phys db's
        private enum DBType
        {
            PHYS,
            RENDER
        }

        private record Entry
        {
            public ulong id { get; set; }

            public BabelDBFile bdb { get; set; }

            public int row_index { get; set; }

            public string type { get; set; }
        }
    }
}
