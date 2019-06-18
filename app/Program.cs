using System;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;
using LibSaberPatch;
using Newtonsoft.Json;
using LibSaberPatch.BehaviorDataObjects;
using LibSaberPatch.AssetDataObjects;
using System.Diagnostics;

namespace app
{
    class LevelPack
    {
        public string id;
        public string name;
        public string coverImagePath;
        public List<string> levelIDs;
    }

    class CustomColors
    {
        public SimpleColor colorA;
        public SimpleColor colorB;
    }

    class SwapLanguage
    {
        public bool swap;
        public string languageToSwapTo;
    }

    class Invocation
    {
        public string apkPath;
        public bool patchSignatureCheck;
        public bool sign;

        public Dictionary<string, string> levels;
        public List<LevelPack> packs;

        public CustomColors colors;
        public SwapLanguage swapLanguage;
        public string replacementLanguage;
        public Dictionary<string, string> replaceText;
        public List<string> soundEffectsFiles;
    }
    class Program
    {
        static string jsonApp2Name = "jsonApp2.exe";
        static void AddSongFolder(string topFolder, List<string> songFolders)
        {
            if (Directory.GetFiles(topFolder, "info.dat").Length > 0)
            {
                songFolders.Add(Path.GetFullPath(topFolder));
                return;
            }
            foreach (string s in Directory.GetDirectories(topFolder))
            {
                AddSongFolder(Path.GetFullPath(s), songFolders);
            }
        }
        static void Main(string[] args)
        {
            Invocation inv = new Invocation();
            //APK PATH
            Console.Write("Please enter the path to your APK: ");
            inv.apkPath = Console.ReadLine();
            if (!File.Exists(inv.apkPath))
            {
                throw new ApplicationException("Path: " + inv.apkPath + " does not exist!");
            }
            //SONGS
            Console.Write("Please input the path to all of your songs: ");
            string songs = Console.ReadLine();
            if (!Directory.Exists(songs))
            {
                throw new ApplicationException("Directory: " + songs + " does not exist!");
            }
            inv.levels = new Dictionary<string, string>();
            List<string> levels = new List<string>();
            List<string> songFolders = new List<string>();
            AddSongFolder(songs, songFolders);
            songFolders.ForEach(s =>
            {
                string levelID = Path.GetFileName(s);
                if (inv.levels.ContainsKey(levelID))
                {
                    Console.WriteLine("Already Contains Song: " + levelID);
                }
                inv.levels.Add(levelID, s);
                levels.Add(levelID);
                Console.WriteLine("Added song: " + levelID);
            });
            //CUSTOM IMAGE OR DEFAULT
            Console.Write("Please enter the path to the custom pack image you would like to use, or press enter to use the default: ");
            string customImage = Console.ReadLine();
            if (!File.Exists(customImage))
            {
                Console.WriteLine("Could not find custom image at path: " + customImage + " using default...");
                customImage = "DefaultCover.jpg";
            }
            //CREATE PACKS
            inv.packs = new List<LevelPack>
            {
                new LevelPack()
                {
                    id = "CustomSongs",
                    name = "Custom Songs",
                    coverImagePath = customImage,
                    levelIDs = levels
                }
            };
            //CUSTOM COLORS
            SimpleColor a = new SimpleColor();
            SimpleColor b = new SimpleColor();
            var _ = new SimpleColor[] { a, b };
            for (int i = 0; i < _.Length; i++)
            {
                SimpleColor color = _[i];
                foreach (var field in color.GetType().GetFields())
                {
                    Console.Write("Enter the " + field.Name + " value for color: " + (i == 0 ? "A" : "B") + " or leave blank to use default colors: ");
                    string val = Console.ReadLine();
                    try
                    {
                        field.SetValue(color, Convert.ToSingle(val));
                    } catch (FormatException)
                    {
                        Console.WriteLine("Using default color for color: " + (i == 0 ? "A" : "B"));
                        _[i] = null;
                        break;
                    }
                }
            }
            inv.colors = new CustomColors()
            {
                colorA = _[0],
                colorB = _[1]
            };
            //SWAP LANGUAGE
            Console.Write("Would you like to swap languages? (N/y): ");
            string yn = Console.ReadLine();
            if (!yn.ToLower().StartsWith("y"))
            {
                inv.swapLanguage = new SwapLanguage()
                {
                    swap = false,
                    languageToSwapTo = "ENGLISH"
                };
            } else
            {
                Console.Write("Please enter the language you would like to swap to: ");
                string l = Console.ReadLine();
                inv.swapLanguage = new SwapLanguage()
                {
                    swap = true,
                    languageToSwapTo = l
                };
            }
            //REPLACE TEXT
            inv.replaceText = new Dictionary<string, string>();
            inv.replacementLanguage = "ENGLISH";
            do
            {
                Console.Write("Enter the key of the text you would like to replace (or leave empty to continue): ");
                string key = Console.ReadLine();
                if (key.Length <= 1)
                {
                    break;
                }
                Console.Write("Enter the value of the text you would like to replace: ");
                inv.replaceText.Add(key, Console.ReadLine());
            } while (true);
            //REPLACE SOUNDS
            inv.soundEffectsFiles = new List<string>();
            do
            {
                Console.Write("Enter the path of a custom .ogg sound effect for cutting notes (or leave empty to use default cut sounds): ");
                string sound = Console.ReadLine();
                if (sound.Length <= 1)
                {
                    break;
                }
                inv.soundEffectsFiles.Add(sound);
            } while (true);

            inv.patchSignatureCheck = true;
            inv.sign = true;

            File.WriteAllText("temp.json", JsonConvert.SerializeObject(inv));

            Console.WriteLine("Completed!");
            Console.WriteLine("Run the following command (if this program runs into an error after attempting to execute this command): ");
            Console.WriteLine(jsonApp2Name + " < temp.json");
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();

            Process.Start(new ProcessStartInfo(jsonApp2Name, "< temp.json"));
        }
    }
}
