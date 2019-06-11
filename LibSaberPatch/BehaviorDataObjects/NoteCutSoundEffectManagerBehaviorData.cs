using LibSaberPatch.AssetDataObjects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LibSaberPatch.BehaviorDataObjects
{
    public class NoteCutSoundEffectManagerBehaviorData : BehaviorData
    {
        // 144, 297 (get hash of this)
        public static readonly byte[] ScriptID = Utils.HexToBytes("BD30549906EE2F82984D617A365590FB");

        // DEFAULTS:
        // Long: (3=123)
        // 3,71; 3,69; 3,64; 3,78; 3,73; 3,61; 3,77; 3,68, 3,72; 3,67
        // Short: (3=123)
        // 3,66; 3,81; 3,82; 3,60; 3,74; 3,63; 3,79; 3,70; 3,83; 3,76

        public AssetPtr gameDidPauseSignal;
        public AssetPtr gameDidResumeSignal;
        public AssetPtr audioManager;
        public float audioSamplesBeatAlignOffset;
        public int maxNumberOfEffects;
        public List<AssetPtr> longCutEffectsAudioClips;
        public List<AssetPtr> shortCutEffectsAudioClips;
        public AssetPtr testAudioClip;

        public NoteCutSoundEffectManagerBehaviorData(BinaryReader reader, int length, Apk.Version version)
        {
            gameDidPauseSignal = new AssetPtr(reader);
            gameDidResumeSignal = new AssetPtr(reader);
            audioManager = new AssetPtr(reader);
            audioSamplesBeatAlignOffset = reader.ReadSingle();
            maxNumberOfEffects = reader.ReadInt32();
            longCutEffectsAudioClips = reader.ReadPrefixedList(r => new AssetPtr(r));
            shortCutEffectsAudioClips = reader.ReadPrefixedList(r => new AssetPtr(r));
            testAudioClip = new AssetPtr(reader);
        }

        public override void WriteTo(BinaryWriter w, Apk.Version v)
        {
            gameDidPauseSignal.WriteTo(w);
            gameDidResumeSignal.WriteTo(w);
            audioManager.WriteTo(w);
            w.Write(audioSamplesBeatAlignOffset);
            w.Write(maxNumberOfEffects);
            w.WritePrefixedList(longCutEffectsAudioClips, a => a.WriteTo(w));
            w.WritePrefixedList(shortCutEffectsAudioClips, a => a.WriteTo(w));
            testAudioClip.WriteTo(w);
        }

        public override void Trace(Action<AssetPtr> action)
        {
            action(gameDidPauseSignal);
            action(gameDidResumeSignal);
            action(audioManager);
            longCutEffectsAudioClips.ForEach(action);
            shortCutEffectsAudioClips.ForEach(action);
            action(testAudioClip);
        }

        public override List<string> OwnedFiles(SerializedAssets assets)
        {
            var l = new List<string>();
            foreach (var ap in longCutEffectsAudioClips)
            {
                l.AddRange(ap.Follow(assets).OwnedFiles(assets));
            }
            foreach (var ap in shortCutEffectsAudioClips)
            {
                l.AddRange(ap.Follow(assets).OwnedFiles(assets));
            }
            return l;
        }

        public static void CreateSoundEffectsFromFiles(Apk.Transaction apk, SerializedAssets assets, SerializedAssets soundAssets, List<string> audioFiles)
        {
            var old = assets.FindScript<NoteCutSoundEffectManagerBehaviorData>(mb => true); // Only one NoteCutSoundEffectManager
            if (old == null)
            {
                throw new ApplicationException("Could not find NoteCutSoundEffectManager!");
            }
            old.longCutEffectsAudioClips.Clear();
            old.shortCutEffectsAudioClips.Clear();
            // For now, just add each file to both short and long (I don't think there is a time constraint on them)
            foreach (string s in audioFiles)
            {
                var ptr = soundAssets.AppendAsset(Utils.CreateAudioAsset(apk, "NoteCutEffect" + s.Substring(0, 4), s)); // Kind of a Unique ID?
                //ptr.fileID = 3; // MAGIC NUMBER BECAUSE LAZY
                ptr.fileID = assets.externals.FindIndex(e => e.pathName.Contains("sharedassets13.assets")) + 1;
                old.longCutEffectsAudioClips.Add(ptr);
                old.shortCutEffectsAudioClips.Add(ptr);
            }
        }
    }
}
