using Gibbed.Dunia2.FileFormats;
using Gibbed.IO;
using Gibbed.ProjectData;
using System.Globalization;
using System.Text;
using Big = Gibbed.Dunia2.FileFormats.Big;

namespace PitCrewCommon.Utilities
{
    /**
     * Truncated methods from Gibbed.Dunia2.Unpack & Pack
     * Original Author: Rick (rick 'at' gibbed 'dot' us)
    */
    public class BigFileUtil
    {
        public static void UnpackBigFile(string fatPath, string outputPath, int packageVersion, string oodleDirectory = "")
        {
            string project = "The Crew";

            if (packageVersion == 6)
            {
                project += " 2";

                //Grab oodle file from game if exists
                if (!File.Exists(Constants.OODLE_FILE) && !string.IsNullOrWhiteSpace(oodleDirectory))
                {
                    File.Copy(Path.Combine(oodleDirectory, Constants.OODLE_FILE), Constants.OODLE_FILE);
                }
            }

            //Load all file names stored in .filelist
            var hashes = Manager.Load(project).LoadListsFileNames(packageVersion);

            BigFile fat;
            using (var input = File.OpenRead(fatPath))
            {
                fat = new BigFile();
                fat.Deserialize(input);
            }

            //Decompress .dat via info from .fat
            using (var input = File.OpenRead(Path.ChangeExtension(fatPath, "dat")))
            {
                foreach (Big.Entry entry in fat.Entries)
                {
                    string entryName = Path.Combine("unknown", entry.NameHash.ToString("X8"));

                    if (hashes[entry.NameHash] != null)
                    {
                        entryName = hashes[entry.NameHash].Replace("/", Path.DirectorySeparatorChar.ToString()).Replace("\\", Path.DirectorySeparatorChar.ToString());
                        if (entryName.StartsWith(Path.DirectorySeparatorChar.ToString()))
                            entryName = entryName.Substring(1);
                    }

                    string entryPath = Path.Combine(outputPath, entryName);

                    input.Seek(entry.Offset, SeekOrigin.Begin);

                    string? entryParent = Path.GetDirectoryName(entryPath);
                    if (entryParent != null && !Directory.Exists(entryParent))
                        Directory.CreateDirectory(entryParent);

                    using (var output = File.Create(entryPath))
                    {
                        Big.EntryDecompression.Decompress(entry, input, output);
                    }
                }
            }
        }

        public static void RepackBigFile(string inputFolder, string outputFatFile, string? author = null, bool compress = true)
        {
            inputFolder = inputFolder.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            byte[]? authorHex = null;
            SortedDictionary<ulong, PendingEntry> pendingEntries = [];
            int byteIndex = 0;
            if (!string.IsNullOrWhiteSpace(author))
                authorHex = Encoding.UTF8.GetBytes(author);

            //Get all files in folder and get their names in CRC64 format.
            foreach (string path in Directory.GetFiles(inputFolder, "*", SearchOption.AllDirectories))
            {
                PendingEntry pendingEntry;

                string fullPath = Path.GetFullPath(path);
                string partPath = path.Substring(inputFolder.Length + 1);

                pendingEntry.FullPath = fullPath;

                var pieces = partPath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                int index = 0;

                if (index >= pieces.Length)
                    continue;

                if (pieces[index].ToUpperInvariant().Equals("UNKNOWN"))
                {
                    var partName = Path.GetFileNameWithoutExtension(partPath);
                    partName = partName.Split(".")[0];

                    if (string.IsNullOrEmpty(partName))
                        continue;

                    if (partName.Length > 16)
                        partName = partName.Substring(0, 16);

                    pendingEntry.NameHash = ulong.Parse(partName, NumberStyles.AllowHexSpecifier);
                }
                else
                {
                    string name = string.Join("\\", pieces.Skip(index).ToArray()).ToLowerInvariant();
                    pendingEntry.NameHash = CRC64.Hash(name, true);
                }

                if (pendingEntries.ContainsKey(pendingEntry.NameHash))
                    continue;

                pendingEntries[pendingEntry.NameHash] = pendingEntry;
            }

            var fat = new BigFile
            {
                Version = 5,
                Platform = Big.Platform.PC,
                Unknown74 = 0
            };

            using (var output = File.Create(Path.ChangeExtension(outputFatFile, "dat")))
            {
                foreach (var pendingEntry in pendingEntries.Select(kv => kv.Value))
                {
                    var entry = new Big.Entry
                    {
                        NameHash = pendingEntry.NameHash,
                        Offset = output.Position,
                        author = 0
                    };

                    //Ingrain author into dummy values.
                    if (authorHex != null)
                    {
                        uint result = 0;
                        for (int i = 0; i < 4; i++)
                        {
                            if (authorHex.Length <= byteIndex)
                            {
                                byteIndex = 0;
                            }
                            result |= (uint)authorHex[byteIndex] << (24 - i * 8);
                            byteIndex++;
                        }
                        entry.author = result;
                    }

                    using (var input = File.OpenRead(pendingEntry.FullPath))
                    {
                        Big.EntryCompression.Compress(fat.Platform, ref entry, input, compress, output);
                        output.Seek(output.Position.Align(16), SeekOrigin.Begin);
                    }

                    fat.Entries.Add(entry);
                }
            }

            using (var output = File.Create(outputFatFile))
            {
                fat.Serialize(output);
            }
        }
    }

    internal struct PendingEntry
    {
        public ulong NameHash;
        public string FullPath;
    }
}
