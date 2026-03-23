using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Logging;
using DoradcaWiezy.Util;

namespace DoradcaWiezy;

// Punkt wejścia moda – [ModInitializer] wskazuje statyczną metodę Initialize()
[ModInitializer(nameof(Initialize))]
public static class Main
{
    public const string ModId      = "DoradcaWiezy";
    public const string ModVersion = "v0.1.0";

    public static Logger? Log { get; private set; }

    public static void Initialize()
    {
        // Awaryjny log diagnostyczny – czysty System.IO, zero Godot API
        // Pozwala ustalić czy Initialize() w ogóle jest wywoływane
        try
        {
            var bootLog = System.IO.Path.Combine(
                System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData),
                "SlayTheSpire2", "DoradcaWiezy_boot.log");
            System.IO.File.AppendAllText(bootLog, $"[{DateTime.Now:HH:mm:ss}] Initialize() START\n");
        }
        catch { }

        try
        {
            Log = new Logger(ModId, LogType.Generic);

            ModLog.Clear();
            ModLog.Info($"=== {L.ModName} {ModVersion} – ładowanie... ===");

            Util.ModSettings.Load();

            var harmony = new Harmony(ModId);

            // Patche stosujemy po jednym – błąd w jednym nie rozbija pozostałych
            ApplyPatch(harmony, typeof(Combat.Patch_WalkaStart));
            ApplyPatch(harmony, typeof(Combat.Patch_WalkaWygrana));
            ApplyPatch(harmony, typeof(Combat.Patch_WalkaOrzegrana));
            ApplyPatch(harmony, typeof(Combat.Patch_KoniecTuryGracza));
            ApplyPatch(harmony, typeof(Combat.Patch_OdswiezIntenty));
            ApplyPatch(harmony, typeof(Combat.Patch_IntentUpdateVisuals));
            ApplyPatch(harmony, typeof(Combat.Patch_IntentProcess));
            ApplyPatch(harmony, typeof(Combat.Patch_Input));
            ApplyPatch(harmony, typeof(Combat.Patch_ChooseCardScreen));
            ApplyPatch(harmony, typeof(Combat.Patch_ChooseCardScreenExit));
            ApplyPatch(harmony, typeof(DeckBuilding.Patch_CardReward));
            ApplyPatch(harmony, typeof(DeckBuilding.Patch_CardRewardExit));
            ApplyPatch(harmony, typeof(DeckBuilding.Patch_UpgradeScreen));
            ApplyPatch(harmony, typeof(DeckBuilding.Patch_UpgradeScreenExit));
            ApplyPatch(harmony, typeof(Map.Patch_MapScreen));
            ApplyPatch(harmony, typeof(Map.Patch_MapScreenClose));
            ApplyPatch(harmony, typeof(Events.Patch_EventScreen));
            ApplyPatch(harmony, typeof(Events.Patch_EventScreenExit));
            ApplyPatch(harmony, typeof(Shops.Patch_ShopOpen));
            ApplyPatch(harmony, typeof(Shops.Patch_ShopClose));
            ApplyPatch(harmony, typeof(Shops.Patch_ShopExit));
            ApplyPatch(harmony, typeof(Shops.Patch_RewardsScreen));
            ApplyPatch(harmony, typeof(Shops.Patch_RewardsScreenExit));

            int patched = harmony.GetPatchedMethods().Count();
            ModLog.Info($"Harmony: załadowano {patched} metod.");
            ModLog.Info($"{L.ModName} {ModVersion} – załadowano pomyślnie!");
            ModLog.Info($"F1 = przełącz HUD walki | F2 = ustawienia");

            // Potwierdź w awaryjnym logu
            try
            {
                var bootLog = System.IO.Path.Combine(
                    System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData),
                    "SlayTheSpire2", "DoradcaWiezy_boot.log");
                System.IO.File.AppendAllText(bootLog, $"[{DateTime.Now:HH:mm:ss}] Initialize() OK – {patched} patches\n");
            }
            catch { }
        }
        catch (Exception ex)
        {
            // Próba zapisu błędu przez ModLog
            try { ModLog.Error("Main.Initialize", ex); } catch { }

            // Ostateczny fallback – zapisz wyjątek bezpośrednio do pliku
            try
            {
                var crashLog = System.IO.Path.Combine(
                    System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData),
                    "SlayTheSpire2", "DoradcaWiezy_boot.log");
                System.IO.File.AppendAllText(crashLog,
                    $"[{DateTime.Now:HH:mm:ss}] CRASH: {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}\n");
            }
            catch { }
        }
    }

    private static void ApplyPatch(Harmony harmony, Type patchClass)
    {
        try
        {
            harmony.CreateClassProcessor(patchClass).Patch();
        }
        catch (Exception ex)
        {
            try { ModLog.Warn($"Patch {patchClass.Name} nieudany: {ex.Message}"); } catch { }
        }
    }
}
