using HarmonyLib;
using Godot;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Screens.Shops;
using MegaCrit.Sts2.Core.Entities.Merchant;
using MegaCrit.Sts2.Core.Models;
using DoradcaWiezy.DeckBuilding;
using DoradcaWiezy.UI;
using DoradcaWiezy.Util;

namespace DoradcaWiezy.Shops;

// Overlay doradcy sklepu
public static class ShopAdvisorOverlay
{
    private static CanvasLayer? _warstwa;

    public static void Pokaz(NMerchantInventory sklep)
    {
        if (!ModSettings.DoradcaDecku) return;
        Ukryj();

        try
        {
            var inventory = sklep.Inventory;
            if (inventory == null) return;

            // Tylko karty które są dostępne (nie kupione)
            var kartyWSklepie = inventory.CardEntries
                .Where(e => e.IsStocked && e.CreationResult != null)
                .ToList();

            if (kartyWSklepie.Count == 0) return;

            _warstwa = new CanvasLayer();
            _warstwa.Layer = 20;
            NGame.Instance?.AddChild(_warstwa);

            var (panel, zawartosc) = UIStyle.StworzPanel("DORADCA SKLEPU", 270f);

            var deck = Combat.CombatStateTracker.PobierzDeck();

            foreach (var entry in kartyWSklepie)
            {
                var karta = entry.CreationResult!.Card;
                var wynik = CardEvaluator.OcenKarte(karta, deck);

                Color kolorOceny = wynik.Ocena switch
                {
                    OcenaPoziom.Doskonala => UIStyle.TekstPorada,
                    OcenaPoziom.Dobra     => UIStyle.TekstNaglowek,
                    OcenaPoziom.Srednia   => UIStyle.TekstGlowny,
                    _                     => UIStyle.TekstSzary
                };

                var vbox = new VBoxContainer();
                vbox.AddThemeConstantOverride("separation", 1);

                // Ocena + nazwa
                vbox.AddChild(UIStyle.StworzEtykiete(wynik.OcenaStr, kolorOceny, UIStyle.RozmiarTekstuNormal));

                // Cena + czy stać
                Color kolorCeny = entry.EnoughGold ? UIStyle.TekstGlowny : UIStyle.TekstSzary;
                string opisCeny = entry.EnoughGold
                    ? $"  {entry.Cost} złota" + (entry.IsOnSale ? " (SALE!)" : "")
                    : $"  {entry.Cost} złota (za drogo)";
                vbox.AddChild(UIStyle.StworzEtykiete(opisCeny, kolorCeny, UIStyle.RozmiarTekstuMaly));

                if (!string.IsNullOrEmpty(wynik.Powod))
                    vbox.AddChild(UIStyle.StworzEtykiete($"  {wynik.Powod}", UIStyle.TekstSzary, UIStyle.RozmiarTekstuMaly));

                zawartosc.AddChild(vbox);
                zawartosc.AddChild(UIStyle.StworzSeparator());
            }

            panel.SetAnchorsPreset(Control.LayoutPreset.CenterRight);
            panel.Position = new Vector2(-280f, -150f);
            _warstwa.AddChild(panel);
        }
        catch (Exception ex) { ModLog.Error("ShopAdvisorOverlay.Pokaz", ex); }
    }

    public static void Ukryj()
    {
        try { _warstwa?.QueueFree(); _warstwa = null; }
        catch { }
    }
}

// ======================================================================
// Patch – NMerchantInventory.Open / Close
// ======================================================================
[HarmonyPatch(typeof(NMerchantInventory), "Open")]
public static class Patch_ShopOpen
{
    static void Postfix(NMerchantInventory __instance)
    {
        try { ShopAdvisorOverlay.Pokaz(__instance); }
        catch (Exception ex) { ModLog.Error("Patch_ShopOpen", ex); }
    }
}

[HarmonyPatch(typeof(NMerchantInventory), "Close")]
public static class Patch_ShopClose
{
    static void Postfix() => ShopAdvisorOverlay.Ukryj();
}

[HarmonyPatch(typeof(NMerchantInventory), "_ExitTree")]
public static class Patch_ShopExit
{
    static void Postfix() => ShopAdvisorOverlay.Ukryj();
}
