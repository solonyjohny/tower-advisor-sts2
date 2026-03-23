using HarmonyLib;
using Godot;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.Combat;
using DoradcaWiezy.Util;

namespace DoradcaWiezy.Combat;

// ======================================================================
// Walka rozpoczęta (Reset ma parametr bool graceful)
// ======================================================================
[HarmonyPatch(typeof(CombatManager), nameof(CombatManager.Reset))]
public static class Patch_WalkaStart
{
    static void Postfix(bool graceful)
    {
        try
        {
            CombatStateTracker.Resetuj();
            CombatHUD.Zniszcz();
            CombatHUD.Stworz();
            CombatStateTracker.CzyWWalce = true;
            DeckBuilding.UpgradeAdvisorOverlay.Ukryj();
            DeckBuilding.DeckAdvisorOverlay.Ukryj();
            ModLog.Info($"Walka rozpoczęta (graceful={graceful}).");
        }
        catch (Exception ex) { ModLog.Error("Patch_WalkaStart", ex); }
    }
}

// ======================================================================
// Walka zakończona – wygrana (async Task)
// ======================================================================
[HarmonyPatch(typeof(CombatManager), nameof(CombatManager.EndCombatInternal))]
public static class Patch_WalkaWygrana
{
    static void Prefix()
    {
        try { CombatHUD.Ukryj(); CombatStateTracker.CzyWWalce = false; }
        catch (Exception ex) { ModLog.Error("Patch_WalkaWygrana", ex); }
    }
}

// ======================================================================
// Walka – przegrana
// ======================================================================
[HarmonyPatch(typeof(CombatManager), nameof(CombatManager.LoseCombat))]
public static class Patch_WalkaOrzegrana
{
    static void Prefix()
    {
        try { CombatHUD.Ukryj(); CombatStateTracker.CzyWWalce = false; }
        catch (Exception ex) { ModLog.Error("Patch_WalkaPrzegrana", ex); }
    }
}

// ======================================================================
// Gracz gotowy do zakończenia tury
// ======================================================================
[HarmonyPatch(typeof(CombatManager), nameof(CombatManager.SetReadyToEndTurn))]
public static class Patch_KoniecTuryGracza
{
    static void Postfix(Player player, bool canBackOut)
    {
        try { CombatHUD.Aktualizuj(); }
        catch (Exception ex) { ModLog.Error("Patch_KoniecTury", ex); }
    }
}

// ======================================================================
// Odświeżenie intentów – NCreature.RefreshIntents (async)
// ======================================================================
[HarmonyPatch(typeof(NCreature), nameof(NCreature.RefreshIntents))]
public static class Patch_OdswiezIntenty
{
    static void Postfix()
    {
        try { CombatHUD.Aktualizuj(); }
        catch (Exception ex) { ModLog.Error("Patch_OdswiezIntenty", ex); }
    }
}

// ======================================================================
// Aktualizacja wizualna intentu – zbieramy dane o obrażeniach
// NIntent ma private fields: _intent, _owner, _targets
// ======================================================================
[HarmonyPatch(typeof(NIntent), "UpdateVisuals")]
public static class Patch_IntentUpdateVisuals
{
    static void Postfix(
        AbstractIntent? ____intent,
        Creature? ____owner,
        System.Collections.Generic.IEnumerable<Creature>? ____targets)
    {
        try
        {
            if (____owner == null || ____intent == null) return;

            var dane = new IntentData
            {
                Owner   = ____owner,
                TypPL   = IntentTranslator.Tlumacz(____intent),
                JestAtakiem = ____intent is AttackIntent,
            };

            if (____intent is AttackIntent attackIntent && ____targets != null)
            {
                try
                {
                    var targetsList = ____targets.ToList();
                    dane.ObrazeniaJednorazowe = attackIntent.GetSingleDamage(targetsList, ____owner);
                    dane.ObrazeniaLaczne      = attackIntent.GetTotalDamage(targetsList, ____owner);
                    dane.LiczbaUderzen = dane.ObrazeniaJednorazowe > 0
                        ? Math.Max(1, dane.ObrazeniaLaczne / dane.ObrazeniaJednorazowe)
                        : 1;
                }
                catch
                {
                    // Fallback przez Traverse
                    try { dane.ObrazeniaLaczne = Traverse.Create(____intent).Property<int>("Damage").Value; }
                    catch { }
                }
            }

            CombatStateTracker.OdswiezIntencje(____owner, dane);
            CombatHUD.Aktualizuj();
        }
        catch (Exception ex) { ModLog.Error("Patch_IntentUpdateVisuals", ex); }
    }
}

// ======================================================================
// Cykliczna aktualizacja przez _Process
// ======================================================================
[HarmonyPatch(typeof(NIntent), "_Process")]
public static class Patch_IntentProcess
{
    private static ulong _ostatniaTick;

    static void Postfix()
    {
        try
        {
            ulong teraz = Time.GetTicksMsec();
            if (teraz - _ostatniaTick < 500) return;
            _ostatniaTick = teraz;
            CombatHUD.Aktualizuj();
        }
        catch { }
    }
}

// ======================================================================
// Obsługa klawiatury – F1 = toggle HUD, F2 = ustawienia
// ======================================================================
[HarmonyPatch(typeof(NGame), "_Input")]
public static class Patch_Input
{
    static void Postfix(InputEvent inputEvent)
    {
        try
        {
            if (inputEvent is not InputEventKey keyEvent) return;
            if (!keyEvent.Pressed || keyEvent.IsEcho()) return;

            switch (keyEvent.Keycode)
            {
                case Key.F1:
                    CombatHUD.PrzelaczWidocznosc();
                    break;
                case Key.F2:
                    UI.ModMenu.PrzelaczWidocznosc();
                    break;
            }
        }
        catch (Exception ex) { ModLog.Error("Patch_Input", ex); }
    }
}
