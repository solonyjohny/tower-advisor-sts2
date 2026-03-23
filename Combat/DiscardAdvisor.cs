using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Screens.CardSelection;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using DoradcaWiezy.UI;
using DoradcaWiezy.Util;
using DoradcaWiezy.DeckBuilding;

namespace DoradcaWiezy.Combat;

// ======================================================================
// Overlay na ekranie "Choose a card" (odrzucanie, wyczerpywanie itp.)
// ======================================================================
public static class DiscardAdvisorOverlay
{
    private static CanvasLayer? _warstwa;

    public static void Pokaz(List<CardModel> karty)
    {
        if (!ModSettings.HudWalki) return;
        Ukryj();
        if (karty.Count == 0) return;

        try
        {
            _warstwa = new CanvasLayer { Layer = 26 };
            NGame.Instance?.AddChild(_warstwa);

            var (panel, zawartosc) = UIStyle.StworzPanel("KTÓRĄ KARTĘ?", 255f);

            bool wrogiAtakuja = CombatStateTracker.LaczneNadchodzaceObrazenia > 0;

            // Posortuj: najwyższy pkt = najlepsza do odrzucenia
            var ranked = karty
                .Select(k => (karta: k, pkt: OcenDoOdrzucenia(k, wrogiAtakuja)))
                .OrderByDescending(x => x.pkt)
                .ToList();

            // Top 3 rekomendacje
            for (int i = 0; i < Math.Min(3, ranked.Count); i++)
            {
                var (karta, _) = ranked[i];
                string nazwa  = CardEvaluator.PobierzNazwe(karta);
                string powod  = ZbudujPowod(karta, wrogiAtakuja, i == 0);
                Color  kolor  = i == 0 ? UIStyle.TekstPorada : UIStyle.TekstSzary;

                var vbox = new VBoxContainer();
                vbox.AddThemeConstantOverride("separation", 1);
                vbox.AddChild(UIStyle.StworzEtykiete(
                    $"{i + 1}. {nazwa}", kolor, UIStyle.RozmiarTekstuNormal));
                if (!string.IsNullOrEmpty(powod))
                    vbox.AddChild(UIStyle.StworzEtykiete(
                        $"   {powod}", UIStyle.TekstSzary, UIStyle.RozmiarTekstuMaly));
                zawartosc.AddChild(vbox);

                if (i < 2) zawartosc.AddChild(UIStyle.StworzSeparator());
            }

            panel.SetAnchorsPreset(Control.LayoutPreset.CenterRight);
            panel.Position = new Vector2(-265f, -100f);
            _warstwa.AddChild(panel);
        }
        catch (Exception ex) { ModLog.Error("DiscardAdvisorOverlay.Pokaz", ex); }
    }

    // Wyższy wynik = bardziej opłaca się odrzucić tę kartę
    private static int OcenDoOdrzucenia(CardModel k, bool wrogiAtakuja)
    {
        string typ  = CardEvaluator.PobierzTyp(k);
        int koszt   = CardEvaluator.PobierzKoszt(k);
        int dmg     = CardEvaluator.PobierzWartosc(k, "Damage");
        int blk     = CardEvaluator.PobierzWartosc(k, "Block");
        bool maEnergię = CardEvaluator.CzyMaEnergie(k);
        bool maDobier  = CardEvaluator.CzyMaDobieranie(k);
        bool maSilę    = CardEvaluator.CzyMaSile(k);
        bool upgraded  = k.IsUpgraded;

        // Zawsze odrzuć przekleństwo/status
        if (typ == "Curse")  return 200;
        if (typ == "Status") return 150;

        int pkt = 0;

        // Wartościowe karty – zatrzymaj (obniżaj pkt)
        if (maEnergię)        pkt -= 60;
        if (maDobier)         pkt -= 45;
        if (maSilę)           pkt -= 35;
        if (typ == "Power")   pkt -= 30;
        if (koszt == 0 && (dmg > 0 || blk > 0)) pkt -= 28;

        // Blok gdy atak nadchodzi – zatrzymaj
        if (wrogiAtakuja && blk > 0)
        {
            int nad = CombatStateTracker.LaczneNadchodzaceObrazenia;
            pkt -= 20 + (blk >= nad ? 15 : 0);
        }

        // Duże obrażenia – zatrzymaj
        if (dmg >= 15) pkt -= 15;
        else if (dmg >= 8) pkt -= 8;

        // Ulepszona karta – wartościowsza
        if (upgraded) pkt -= 10;

        // Droga i słaba – odrzuć
        if (koszt >= 2 && dmg < 5 && blk < 5 && !maDobier && !maEnergię && !maSilę)
            pkt += 25;

        // Podstawowe nieulepszane karty
        string nazwa = CardEvaluator.PobierzNazwe(k).ToLower();
        if ((nazwa.Contains("strike") || nazwa.Contains("uderzeni") ||
             nazwa.Contains("defend") || nazwa == "obrona") && !upgraded)
            pkt += 15;

        return pkt;
    }

    private static string ZbudujPowod(CardModel k, bool wrogiAtakuja, bool czyNajgorsza)
    {
        string typ  = CardEvaluator.PobierzTyp(k);
        int blk     = CardEvaluator.PobierzWartosc(k, "Block");
        bool maEnergię = CardEvaluator.CzyMaEnergie(k);
        bool maDobier  = CardEvaluator.CzyMaDobieranie(k);

        if (typ == "Curse")  return "Przekleństwo – zawsze odrzuć!";
        if (typ == "Status") return "Status – zawsze odrzuć!";

        if (!czyNajgorsza) return "";

        if (wrogiAtakuja && blk >= CombatStateTracker.LaczneNadchodzaceObrazenia)
            return "Uwaga! Ta karta blokuje atak – rozważ dokładnie";
        if (maEnergię) return "Uwaga! Generuje energię – wartościowa";
        if (maDobier)  return "Uwaga! Dobiera karty – wartościowa";

        return "Najsłabsza w bieżącej ręce";
    }

    public static void Ukryj()
    {
        try { _warstwa?.QueueFree(); _warstwa = null; }
        catch { }
    }
}

// ======================================================================
// Patche – NChooseACardSelectionScreen
// ======================================================================
[HarmonyPatch(typeof(NChooseACardSelectionScreen), "_Ready")]
public static class Patch_ChooseCardScreen
{
    static void Postfix(NChooseACardSelectionScreen __instance)
    {
        try
        {
            var cards = Traverse.Create(__instance)
                .Field<IReadOnlyList<CardModel>>("_cards")
                .Value;

            if (cards == null || cards.Count == 0) return;
            DiscardAdvisorOverlay.Pokaz(cards.ToList());
        }
        catch (Exception ex) { ModLog.Error("Patch_ChooseCardScreen", ex); }
    }
}

[HarmonyPatch(typeof(NChooseACardSelectionScreen), "_ExitTree")]
public static class Patch_ChooseCardScreenExit
{
    static void Postfix() => DiscardAdvisorOverlay.Ukryj();
}
