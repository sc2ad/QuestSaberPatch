using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.PixelFormats;
using LibSaberPatch.BehaviorDataObjects;
using LibSaberPatch.AssetDataObjects;

namespace LibSaberPatch
{
    public static class Utils {
        public static void FindLevels(string startDir, Action<string> del) {
            string infoPath = Path.Combine(startDir, "info.dat");
            if(File.Exists(infoPath)) {
                del(startDir);
            } else {
                foreach (string d in Directory.GetDirectories(startDir)) {
                    FindLevels(d, del);
                }
            }
        }

        public static byte[] ImageFileToMipData(string imagePath, int topDim) {
            // pre-compute size of all mips together
            int totalSize = 0;
            for(int dim = topDim; dim > 0; dim /= 2) {
                totalSize += dim*dim;
            }
            totalSize *= 3; // 3 bytes per pixel

            byte[] imageData = new byte[totalSize];
            Span<byte> imageDataSpan = imageData;
            using (Image<Rgb24> image = Image.Load<Rgb24>(Configuration.Default, imagePath)) {
                int dataWriteIndex = 0;
                for(int dim = topDim; dim > 0; dim /= 2) {
                    image.Mutate(x => x.Resize(dim, dim));
                    for (int y = 0; y < image.Height; y++) {
                        // need to do rows in reverse order to match what Unity wants
                        Span<Rgb24> rowPixels = image.GetPixelRowSpan((image.Height-1)-y);
                        Span<byte> rowData = MemoryMarshal.AsBytes(rowPixels);
                        rowData.CopyTo(imageDataSpan.Slice(dataWriteIndex, rowData.Length));
                        dataWriteIndex += rowData.Length;
                    }
                }
            }
            return imageData;
        }

        // https://stackoverflow.com/questions/321370/how-can-i-convert-a-hex-string-to-a-byte-array
        public static byte[] HexToBytes(string hex) {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

        // This method will fail if given an object that is not a LevelBehaviorData
        public static void RemoveLevel(SerializedAssets assets, SerializedAssets.AssetObject obj, Apk.Transaction apk)
        {
            LevelBehaviorData level = (obj.data as MonoBehaviorAssetData).data as LevelBehaviorData;

            // Remove audio file
            foreach (string s in level.OwnedFiles(assets)) {
                apk.RemoveFileAt($"assets/bin/Data/{s}");
            }

            // Remove things from bottom up, so that pointers to other assets still are rooted
            // and so get fixed up by RemoveAssetAt

            foreach (BeatmapSet s in level.difficultyBeatmapSets) {
                foreach (BeatmapDifficulty d in s.difficultyBeatmaps) {
                    assets.RemoveAssetAt(d.beatmapData.pathID);
                }
            }

            assets.RemoveAssetAt(level.coverImage.pathID);
            assets.RemoveAssetAt(level.audioClip.pathID);

            assets.RemoveAssetAt(obj.pathID);
        }

        public static AudioClipAssetData CreateAudioAsset(Apk.Transaction apk, string levelID, string audioClipFile)
        {
            string sourceFileName = levelID + ".ogg";
            apk.CopyFileInto(audioClipFile, $"assets/bin/Data/{sourceFileName}");
            ulong fileSize = (ulong)new FileInfo(audioClipFile).Length;
            using (NVorbis.VorbisReader v = new NVorbis.VorbisReader(audioClipFile))
            {
                return new AudioClipAssetData()
                {
                    name = levelID,
                    loadType = 1,
                    channels = v.Channels,
                    frequency = v.SampleRate,
                    bitsPerSample = 16,
                    length = (Single)v.TotalTime.TotalSeconds,
                    isTracker = false,
                    subsoundIndex = 0,
                    preloadAudio = false,
                    backgroundLoad = true,
                    legacy3D = true,
                    compressionFormat = 1, // vorbis
                    source = sourceFileName,
                    offset = 0,
                    size = fileSize,
                };
            }
        }
    }

    // loosely based on https://stackoverflow.com/questions/1440392/use-byte-as-key-in-dictionary
    public class ByteArrayComparer : EqualityComparer<byte[]> {
        public override bool Equals(byte[] first, byte[] second) {
            if (first == null || second == null) {
                return first == second;
            }
            return first.SequenceEqual(second);
        }

        public override int GetHashCode(byte[] obj) {
            if (obj.Length >= 4) {
                return BitConverter.ToInt32(obj, 0);
            }
            return 0;
        }

    }
}
