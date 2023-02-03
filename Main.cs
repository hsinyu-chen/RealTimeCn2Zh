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

namespace RealTimeCn2Zh
{
    public static class Main
    {
        public static void Load(UnityModManager.ModEntry modEntry)
        {
            PatchLocalizedString.Path = modEntry.Path;
            new Harmony("RealTimeCn2Zh").PatchAll();
        }

    }
    [HarmonyPatch(typeof(PatchLocalizedString))]
    public class PatchLocalizedString
    {
        readonly static ConcurrentDictionary<string, string> _cache = new ConcurrentDictionary<string, string>();
        static OpenCC.NET.OpenChineseConverter converter;
        public static string Path { get; internal set; }
        static Task saving;
        [HarmonyPostfix, HarmonyPatch(typeof(LocalizedString), "LoadString")]
        public static void PatchLoadString(Locale locale, ref string __result)
        {
            if (locale == Locale.zhCN)
            {
                if (converter == null)
                {
                    lock (_cache)
                    {
                        if (converter == null)
                        {
                            converter = new OpenCC.NET.OpenChineseConverter(System.IO.Path.Combine(Path, "opencc-dictionary.json"));
                            var cache = System.IO.Path.Combine(Path, "cache.json");
                            if (File.Exists(cache))
                            {
                                var json = File.ReadAllText(cache, Encoding.UTF8);
                                var data = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                                _cache.AddRange<string, string>(data);
                            }
                        }
                    }
                }
                try
                {
                    if (!string.IsNullOrWhiteSpace(__result))
                    {
                        __result = _cache.GetOrAdd(__result, key =>
                        {
                            if (saving == null)
                            {
                                lock (_cache)
                                {
                                    if (saving == null)
                                    {
                                        saving = Task.Factory.StartNew(async () =>
                                        {
                                            await Task.Delay(3000);
                                            File.WriteAllText(System.IO.Path.Combine(Path, "cache.json"), Newtonsoft.Json.JsonConvert.SerializeObject(_cache));
                                            saving = null;
                                        }, TaskCreationOptions.LongRunning);
                                    }
                                }
                            }
                            return converter.ToTraditionalFromSimplified(key);
                        });
                    }
                }
                catch (Exception ex)
                {
                    File.AppendAllText("RealTimeCn2Zh.log", ex.ToString());
                }
            }
        }
    }
}
