using HarmonyLib;
using Godot;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Screens.CardSelection;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using DoradcaWiezy.UI;
using DoradcaWiezy.Util;

namespace DoradcaWiezy.DeckBuilding;

// ======================================================================
// Overlay na ekranie NAGRODY (wybór karty po walce)
// ======================================================================
public static class DeckAdvisorOverlay
{
    private static CanvasLayer? _warstwa;

    public static void Pokaz(List<CardModel> proponowane, List<CardModel> deck)
    {
        if (!ModSettings.DoradcaDecku) return;
        Ukryj();

        try
        {
            _warstwa = new CanvasLayer();
            _warstwa.Layer = 20;
            NGame.Instance?.AddChild(_warstwa);

            var (panel, zawartosc) = UIStyle.StworzPanel(L.OcenaKarty, 260f);

            // Typ decku (1 linia kontekstu)
            var (archetyp, _) = DeckAnalyzer.AnalizujArchetype(deck);
            zawartosc.AddChild(UIStyle.StworzEtykiete(
                $"Deck: {archetyp}", UIStyle.TekstSzary, UIStyle.RozmiarTekstuMaly));
            zawartosc.AddChild(UIStyle.StworzSeparator());

            // Ocena każdej proponowanej karty
            foreach (var karta in proponowane)
            {
                var wynik = CardEvaluator.OcenKarte(karta, deck);
                Color kolorOceny = wynik.Ocena switch
                {
                    OcenaPoziom.Doskonala => UIStyle.TekstPorada,
                    OcenaPoziom.Dobra     => UIStyle.TekstNaglowek,
                    OcenaPoziom.Srednia   => UIStyle.TekstGlowny,
                    _                     => UIStyle.TekstSzary
                };

                var vbox = new VBoxContainer();
                vbox.AddThemeConstantOverride("separation", 2);
                vbox.AddChild(UIStyle.StworzEtykiete(wynik.OcenaStr, kolorOceny, UIStyle.RozmiarTekstuNormal));
                if (!string.IsNullOrEmpty(wynik.Powod))
                    vbox.AddChild(UIStyle.StworzEtykiete($"  {wynik.Powod}", UIStyle.TekstSzary, UIStyle.RozmiarTekstuMaly));

                zawartosc.AddChild(vbox);
                zawartosc.AddChild(UIStyle.StworzSeparator());
            }

            // Sugestia ulepszenia (footer)
            var doUlepszenia = DeckAnalyzer.PriorytetUlepszenia(deck).FirstOrDefault();
            if (doUlepszenia != default)
            {
                zawartosc.AddChild(UIStyle.StworzEtykiete(
                    $"Ulepsz: {CardEvaluator.PobierzNazwe(doUlepszenia.karta)}",
                    UIStyle.TekstOstrzez, UIStyle.RozmiarTekstuMaly));
            }

            panel.SetAnchorsPreset(Control.LayoutPreset.CenterRight);
            panel.Position = new Vector2(-270f, -100f);
            _warstwa.AddChild(panel);
        }
        catch (Exception ex) { ModLog.Error("DeckAdvisorOverlay.Pokaz", ex); }
    }

    public static void Ukryj()
    {
        try { _warstwa?.QueueFree(); _warstwa = null; }
        catch { }
    }
}

// ======================================================================
// Overlay na ekranie ULEPSZANIA KART (Smith / Ognisko)
// ======================================================================
public static class UpgradeAdvisorOverlay
{
    private static CanvasLayer? _warstwa;

    public static void Pokaz(List<CardModel> deck)
    {
        if (!ModSettings.DoradcaDecku) return;
        Ukryj();
        if (deck.Count == 0) return;

        try
        {
            _warstwa = new CanvasLayer();
            _warstwa.Layer = 20;
            NGame.Instance?.AddChild(_warstwa);

            var (panel, zawartosc) = UIStyle.StworzPanel(L.NaglowekUlepszenia, 320f);

            // Archetype + strategia
            var (archetyp, strategia) = DeckAnalyzer.AnalizujArchetype(deck);
            zawartosc.AddChild(UIStyle.StworzEtykiete(archetyp, UIStyle.TekstNaglowek, UIStyle.RozmiarTekstuMaly));
            zawartosc.AddChild(UIStyle.StworzEtykiete(strategia, UIStyle.TekstSzary, UIStyle.RozmiarTekstuMaly));
            zawartosc.AddChild(UIStyle.StworzSeparator());

            // Top 4 karty do ulepszenia
            var lista = DeckAnalyzer.PriorytetUlepszenia(deck);
            if (lista.Count == 0)
            {
                zawartosc.AddChild(UIStyle.StworzEtykiete(
                    L.WszystkieUlepszone, UIStyle.TekstPorada, UIStyle.RozmiarTekstuMaly));
            }
            else
            {
                for (int i = 0; i < Math.Min(4, lista.Count); i++)
                {
                    var (karta, powod) = lista[i];
                    string nazwa = CardEvaluator.PobierzNazwe(karta);
                    string dispNazwa = nazwa.Length > 22 ? nazwa[..21] + "." : nazwa;
                    string dispPowod = powod.Length > 32 ? powod[..31] + "." : powod;
                    Color kolor  = i == 0 ? UIStyle.TekstPorada : UIStyle.TekstNaglowek;

                    var vbox = new VBoxContainer();
                    vbox.AddThemeConstantOverride("separation", 1);
                    vbox.AddChild(UIStyle.StworzEtykiete($"{i + 1}. {dispNazwa}", kolor, UIStyle.RozmiarTekstuNormal));
                    vbox.AddChild(UIStyle.StworzEtykiete($"   {dispPowod}", UIStyle.TekstSzary, UIStyle.RozmiarTekstuMaly));
                    zawartosc.AddChild(vbox);
                }
            }

            // Karty do usunięcia
            var doUsuniecia = DeckAnalyzer.ZalecaniaUsunieciaKart(deck);
            if (doUsuniecia.Count > 0)
            {
                zawartosc.AddChild(UIStyle.StworzSeparator());
                zawartosc.AddChild(UIStyle.StworzEtykiete(
                    L.RozwazUsuniecie, UIStyle.TekstOstrzez, UIStyle.RozmiarTekstuMaly));
                foreach (var (karta, powod) in doUsuniecia.Take(3))
                {
                    string nazwa = CardEvaluator.PobierzNazwe(karta);
                    string dispN = nazwa.Length > 18 ? nazwa[..17] + "." : nazwa;
                    zawartosc.AddChild(UIStyle.StworzEtykiete(
                        $"✗ {dispN} – {powod}", UIStyle.TekstSzary, UIStyle.RozmiarTekstuMaly));
                }
            }

            panel.SetAnchorsPreset(Control.LayoutPreset.CenterRight);
            panel.Position = new Vector2(-328f, -160f);
            _warstwa.AddChild(panel);
        }
        catch (Exception ex) { ModLog.Error("UpgradeAdvisorOverlay.Pokaz", ex); }
    }

    public static void Ukryj()
    {
        try { _warstwa?.QueueFree(); _warstwa = null; }
        catch { }
    }
}

// ======================================================================
// Patch – NCardRewardSelectionScreen (wybór karty po walce)
// ======================================================================
[HarmonyPatch(typeof(NCardRewardSelectionScreen), "_Ready")]
public static class Patch_CardReward
{
    static void Prefix() => DeckAdvisorOverlay.Ukryj();

    static void Postfix(NCardRewardSelectionScreen __instance)
    {
        try
        {
            if (!ModSettings.DoradcaDecku) return;

            var options = Traverse.Create(__instance)
                .Field<IReadOnlyList<CardCreationResult>>("_options")
                .Value;

            if (options == null || options.Count == 0) return;

            var karty = options.Select(o => o.Card).ToList();
            var deck  = Combat.CombatStateTracker.PobierzDeck();
            DeckAdvisorOverlay.Pokaz(karty, deck);
        }
        catch (Exception ex) { ModLog.Error("Patch_CardReward.Postfix", ex); }
    }
}

[HarmonyPatch(typeof(NCardRewardSelectionScreen), "_ExitTree")]
public static class Patch_CardRewardExit
{
    static void Postfix() => DeckAdvisorOverlay.Ukryj();
}

// ======================================================================
// Patch – NDeckUpgradeSelectScreen (ulepszanie kart przy ognisku/kowalu)
// ======================================================================
[HarmonyPatch(typeof(NDeckUpgradeSelectScreen), "_Ready")]
public static class Patch_UpgradeScreen
{
    static void Postfix(NDeckUpgradeSelectScreen __instance)
    {
        try
        {
            var deck = Combat.CombatStateTracker.PobierzDeck();
            UpgradeAdvisorOverlay.Pokaz(deck);

            // TreeExited odpala zawsze gdy węzeł opuszcza drzewo sceny,
            // nawet gdy _ExitTree nie jest nadpisane w tej klasie
            __instance.TreeExited += UpgradeAdvisorOverlay.Ukryj;
        }
        catch (Exception ex) { ModLog.Error("Patch_UpgradeScreen.Postfix", ex); }
    }
}

// Patch _ExitTree zostawiamy jako fallback (może zadziałać w przyszłych wersjach gry)
[HarmonyPatch(typeof(NDeckUpgradeSelectScreen), "_ExitTree")]
public static class Patch_UpgradeScreenExit
{
    static void Postfix() => UpgradeAdvisorOverlay.Ukryj();
}
