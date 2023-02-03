using HarmonyLib;
using Kingmaker.Localization.Shared;
using Kingmaker.Localization;
using System;
using UnityModManagerNet;
using System.IO;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using MiniJSON;
using System.Text;
using System.Collections.Generic;
using MonoMod.Utils;
using UnityEngine;
using System.Linq;
using System.Diagnostics;
using System.Security.Cryptography;

namespace RealTimeCn2Zh
{
    public static class Main
    {
        public static void Load(UnityModManager.ModEntry modEntry)
        {
            PatchLocalizedString.Path = modEntry.Path;
            PatchLocalizedString.GameVersion = modEntry.GameVersion.ToString();
            new Harmony("RealTimeCn2Zh").PatchAll();
        }

    }

    [HarmonyPatch(typeof(PatchLocalizedString))]
    public class PatchLocalizedString
    {
        public static string Path { get; internal set; }
        public static string GameVersion { get; internal set; }
        [HarmonyPrefix, HarmonyPatch(typeof(LocalizationManager), "LoadPack", new Type[] { typeof(string), typeof(Locale) })]
        public static void PatchLoadPack(ref string packPath, Locale locale)
        {
            if (locale == Locale.zhCN)
            {
                var sb = new StringBuilder();
                try
                {
                    string hash;
                    using (var sha1 = SHA1.Create())
                    using (var fs = new FileStream(packPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        hash = BitConverter.ToString(sha1.ComputeHash(fs)).Replace("-", "");
                    }
                    var folder = System.IO.Path.GetDirectoryName(packPath);
                    var file = System.IO.Path.Combine(folder, $"zhTw.{hash}.json");
                    if (!File.Exists(file))
                    {
                        sb.AppendLine($"packPath:{packPath}");
                        sb.AppendLine($"file:{file}");
                        sb.AppendLine($"opencc:{System.IO.Path.Combine(Path, "opencc")}");
                        var p = new Process()
                        {
                            StartInfo = new ProcessStartInfo
                            {
                                WorkingDirectory = System.IO.Path.Combine(Path, "opencc"),
                                FileName = "opencc.exe",
                                Arguments = $"-i \"{packPath}\" -o \"{file}\" -c .\\config\\s2t.json",
                                WindowStyle = ProcessWindowStyle.Hidden
                            }
                        };
                        p.Start();
                        p.WaitForExit();
                    }
                    packPath = file;
                }
                catch (Exception ex)
                {
                    sb.AppendLine(ex.ToString());
                    File.WriteAllText("error.log", sb.ToString());
                }
            }
        }
    }
}
