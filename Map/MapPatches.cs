using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;
using DoradcaWiezy.Util;

namespace DoradcaWiezy.Map;

// ======================================================================
// Patch bezpośredni – NMapScreen._Ready
// ======================================================================
[HarmonyPatch(typeof(NMapScreen), "_Ready")]
public static class Patch_MapScreen
{
    static void Postfix()
    {
        try
        {
            PathOverlay.Pokaz();
            DeckBuilding.UpgradeAdvisorOverlay.Ukryj();
        }
        catch (Exception ex) { ModLog.Error("Patch_MapScreen.Postfix", ex); }
    }
}

[HarmonyPatch(typeof(NMapScreen), "Close")]
public static class Patch_MapScreenClose
{
    static void Postfix()
    {
        try { PathOverlay.Ukryj(); }
        catch { }
    }
}
