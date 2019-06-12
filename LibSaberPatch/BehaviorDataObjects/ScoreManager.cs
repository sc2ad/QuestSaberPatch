using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LibSaberPatch.BehaviorDataObjects
{
    public class ScoreManager : BehaviorData
    {
        public static readonly byte[] ScriptID = Utils.HexToBytes("7E915E06E3729FC6C8768C3E024690E7");

        public int feverModeRequiredCombo;
        public float feverModeDuration;
        public AssetPtr gameplayModifiersModel;

        public ScoreManager(BinaryReader reader, int _length, Apk.Version v)
        {
            feverModeRequiredCombo = reader.ReadInt32();
            feverModeDuration = reader.ReadSingle();
            gameplayModifiersModel = new AssetPtr(reader);
        }

        public override void WriteTo(BinaryWriter w, Apk.Version v)
        {
            w.Write(feverModeRequiredCombo);
            w.Write(feverModeDuration);
            gameplayModifiersModel.WriteTo(w);
        }

        public override void Trace(Action<AssetPtr> action)
        {
            action(gameplayModifiersModel);
        }
    }
}
