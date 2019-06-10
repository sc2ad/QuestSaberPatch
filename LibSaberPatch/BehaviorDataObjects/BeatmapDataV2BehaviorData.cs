using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace LibSaberPatch.BehaviorDataObjects
{
    public class BeatmapDataV2BehaviorData : BehaviorData
    {
        public static byte[] ScriptID = Utils.HexToBytes("95AF3C8D406FF35C9DA151E0EE1E0013");
        public static byte[] TypeHash = Utils.HexToBytes("87650EB74BF6109EE482D14C881EDC21");

        public string jsonData;
        public byte[] signature;
        public byte[] projectedData;

        public BeatmapDataV2BehaviorData() { }

        public BeatmapDataV2BehaviorData(BinaryReader reader, int length)
        {
            jsonData = reader.ReadAlignedString();
            int nullFields = 2;
            if (length - reader.BaseStream.Position >= nullFields * 4)
            {
                // This means that at least the last bit of data is legible
                // How do we know if the last or the second-to-last data is null?
                // For now, let's assume that they both can't be null at the same time.
                signature = reader.ReadPrefixedBytes();
                projectedData = reader.ReadPrefixedBytes();
            }
        }

        public override void WriteTo(BinaryWriter w, Apk.Version v)
        {
            w.WriteAlignedString(jsonData);
            if (signature != null)
                w.WritePrefixedBytes(signature);
            if (projectedData != null)
                w.WritePrefixedBytes(projectedData);
        }

        public override int SharedAssetsTypeIndex()
        {
            return 15;
        }

        public static BeatmapDataV2BehaviorData FromJsonFile(string path) {
            string jsonData = File.ReadAllText(path);
            BeatmapSaveData saveData = JsonConvert.DeserializeObject<BeatmapSaveData>(jsonData);

            return new BeatmapDataV2BehaviorData() {
                jsonData = jsonData,
            };
        }
    }
}
