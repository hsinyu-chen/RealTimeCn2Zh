using HarmonyLib;
using Kingmaker.Localization.Shared;
using Kingmaker.Localization;
using System;
using UnityModManagerNet;
using System.IO;
using System.Collections.Concurrent;

namespace RealTimeCn2Zh
{
    public static class Main
    {
        public static void Load()
        {
            new Harmony("RealTimeCn2Zh").PatchAll();
        }

    }
    [HarmonyPatch(typeof(PatchLocalizedString))]
    public class PatchLocalizedString
    {
        readonly static OpenCC.NET.OpenChineseConverter converter = new OpenCC.NET.OpenChineseConverter("Mods\\RealTimeCn2Zh\\opencc-dictionary.json");
        [HarmonyPostfix, HarmonyPatch(typeof(LocalizedString), "LoadString")]
        public static void PatchLoadString(Locale locale, ref string __result)
        {
            if (locale == Locale.zhCN)
            {
                try
                {
                    if (!string.IsNullOrWhiteSpace(__result))
                    {
                        __result = converter.ToTraditionalFromSimplified(__result);
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
