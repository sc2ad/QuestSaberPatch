using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LibSaberPatch.AssetDataObjects
{
    public sealed class TextAssetAssetData : AssetData
    {
        // Master Polyglot: c4dc0d059266d8d47862f46460cf8f31, 1
        // BeatSaber: 231368cb9c1d5dd43988f2a85226e7d7, 1
        public const int ClassID = 0x31;

        public string name;
        public string script;

        public TextAssetAssetData(BinaryReader reader, int _length)
        {
            name = reader.ReadAlignedString();
            script = reader.ReadAlignedString();
        }

        public sealed override int SharedAssetsTypeIndex()
        {
            return 0;
        }

        public sealed override void WriteTo(BinaryWriter w)
        {
            w.WriteAlignedString(name);
            w.WriteAlignedString(script);
        }
    }
}
