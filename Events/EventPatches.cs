using HarmonyLib;
using Godot;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Events;
using DoradcaWiezy.UI;
using DoradcaWiezy.Util;

namespace DoradcaWiezy.Events;

// Overlay doradcy wydarzeń
public static class EventAdvisorOverlay
{
    private static CanvasLayer? _warstwa;

    public static void Pokaz(string nazwaWydarzenia, List<(string tytul, string opis)> opcje)
    {
        if (!ModSettings.DoradcaWydarzen) return;
        Ukryj();

        try
        {
            _warstwa = new CanvasLayer();
            _warstwa.Layer = 18;
            NGame.Instance?.AddChild(_warstwa);

            var (panel, zawartosc) = UIStyle.StworzPanel(L.NaglowekWydarzenia, 260f);

            if (!string.IsNullOrEmpty(nazwaWydarzenia))
                zawartosc.AddChild(UIStyle.StworzEtykiete(nazwaWydarzenia,
                    UIStyle.TekstNaglowek, UIStyle.RozmiarTekstuMaly));

            zawartosc.AddChild(UIStyle.StworzSeparator());

            for (int i = 0; i < opcje.Count; i++)
            {
                var (tytul, opis) = opcje[i];
                string ocena = OcenOpcje(tytul, opis);
                Color kolor  = ocena.StartsWith("✓") ? UIStyle.TekstPorada
                             : ocena.StartsWith("⚠") ? UIStyle.TekstOstrzez
                             : UIStyle.TekstSzary;

                var vbox = new VBoxContainer();
                vbox.AddThemeConstantOverride("separation", 1);
                var wiersz = new HBoxContainer();
                wiersz.AddChild(UIStyle.StworzEtykiete($"{i + 1}. ", UIStyle.TekstSzary, UIStyle.RozmiarTekstuMaly));
                wiersz.AddChild(UIStyle.StworzEtykiete(tytul, UIStyle.TekstGlowny, UIStyle.RozmiarTekstuMaly));
                vbox.AddChild(wiersz);
                vbox.AddChild(UIStyle.StworzEtykiete("   " + ocena, kolor, UIStyle.RozmiarTekstuMaly));
                zawartosc.AddChild(vbox);
            }

            panel.SetAnchorsPreset(Control.LayoutPreset.CenterLeft);
            panel.Position = new Vector2(10f, -100f);
            _warstwa.AddChild(panel);
        }
        catch (Exception ex) { ModLog.Error("EventAdvisorOverlay.Pokaz", ex); }
    }

    private static string OcenOpcje(string tytul, string opis)
    {
        // Łączymy tytuł i opis do analizy (obsługujemy PL i EN)
        string op = (tytul + " " + opis).ToLower();

        var (hp, maxHp, _) = Combat.CombatStateTracker.PobierzHPGracza();
        float propHP = maxHp > 0 ? (float)hp / maxHp : 1f;
        bool malHP = propHP < 0.5f;

        // === S-tier: Max HP ===
        if (op.Contains("maks. pż") || op.Contains("maksymaln") || op.Contains("max hp") || op.Contains("max pż") ||
            op.Contains("maximum hp") || op.Contains("max health"))
            return L.Zalecana + " (max HP!)";

        // === S-tier: Efekt co turę (relic/passive) ===
        bool efektCoTure = op.Contains("beginning of") || op.Contains("start of") || op.Contains("at the start") ||
                           op.Contains("each turn") || op.Contains("every turn") || op.Contains("na początku tury") ||
                           op.Contains("co turę") || op.Contains("każdą turę");
        bool korzystnyEfekt = op.Contains("attack") || op.Contains("card") || op.Contains("energy") ||
                              op.Contains("ataku") || op.Contains("kart") || op.Contains("energi") ||
                              op.Contains("free") || op.Contains("0 ") || op.Contains("draw");
        if (efektCoTure && korzystnyEfekt)
            return L.Zalecana + " (co turę darmowy efekt – S-tier!)";

        // === S-tier: Ulepsza wiele kart na raz ===
        bool ulepsza = op.Contains("upgrade") || op.Contains("ulepsz");
        bool wieloKart = op.Contains(" 4") || op.Contains(" 5") || op.Contains(" 3 ") ||
                         op.Contains("random") || op.Contains("losow") || op.Contains("all ");
        if (ulepsza && wieloKart)
            return L.Zalecana + " (ulepsza wiele kart – rewelacja!)";

        // === Leczenie / HP ===
        if (op.Contains("pż") || op.Contains("hp") || op.Contains("lecz") || op.Contains("heal") ||
            op.Contains("odzysku") || op.Contains("odzyskaj") || op.Contains("życi") || op.Contains("uzdrow") ||
            op.Contains("restore") || op.Contains("recover") || op.Contains("health"))
            return malHP ? L.Zalecana + " (HP – pilne!)" : L.Zalecana + " (odzysk HP)";

        // === Relikwia / Artefakt ===
        if (op.Contains("relikwi") || op.Contains("relic") || op.Contains("artefakt"))
            return L.Zalecana + " (relikwia)";

        // === Enchant / Instinct / specjalne mechaniki ===
        if (op.Contains("enchant") || op.Contains("instinct") || op.Contains("innate") || op.Contains("ethereal"))
            return L.Zalecana + " (enchant kart)";

        // === Warunkowy bonus (whenever/when you kill) ===
        bool warunkowy = op.Contains("whenever") || op.Contains("when you") || op.Contains("kiedy");
        if (warunkowy && (ulepsza || op.Contains("card") || op.Contains("gold") || op.Contains("kart")))
            return L.Zalecana + " (efekt warunkowy)";

        // === Ulepszenie (pojedyncze) ===
        if (ulepsza || op.Contains("smith"))
            return L.Zalecana + " (ulepszenie)";

        // === Karta ===
        if (op.Contains("kart") || op.Contains("card") || op.Contains("tali"))
            return L.Zalecana + " (karta)";

        // === Usunięcie karty ===
        if (op.Contains("usuń") || op.Contains("remove") || op.Contains("wyrzuć") || op.Contains("purge") ||
            op.Contains("exhaust a") || op.Contains("lose a"))
            return L.Zalecana + " (usuń słabą kartę!)";

        // === Złoto ===
        if (op.Contains("złot") || op.Contains("gold") || op.Contains("dukat"))
            return propHP > 0.6f ? L.Zalecana + " (złoto)" : "Rozważ (lepiej HP)";

        // === Negatywne / niebezpieczne ===
        if (op.Contains("przeklę") || op.Contains("curse"))
            return "⚠ Unikaj (przekleństwo)";
        if (op.Contains("strac") || op.Contains("tracisz") || op.Contains("lose") || op.Contains("obrażeni"))
            return malHP ? "⚠ Ryzykowne (mało HP)" : "Ryzyko";
        if (op.Contains("śmierć") || op.Contains("death") || op.Contains("zgin"))
            return "⚠ Bardzo ryzykowne!";

        // === Bezpieczne wyjście ===
        if (op.Contains("omiń") || op.Contains("skip") || op.Contains("odejdź") ||
            op.Contains("cofn") || op.Contains("wyjdź") || op.Contains("zostaw") || op.Contains("leave"))
            return "Bezpieczne (pomiń)";

        return L.ZalecanaBrak;
    }

    public static void Ukryj()
    {
        try { _warstwa?.QueueFree(); _warstwa = null; }
        catch { }
    }
}

// ======================================================================
// Patch bezpośredni – NEventRoom._Ready
// ======================================================================
[HarmonyPatch(typeof(NEventRoom), "_Ready")]
public static class Patch_EventScreen
{
    static void Prefix()
    {
        EventAdvisorOverlay.Ukryj();
        DeckBuilding.UpgradeAdvisorOverlay.Ukryj();
    }

    static void Postfix(NEventRoom __instance)
    {
        try
        {
            if (!ModSettings.DoradcaWydarzen) return;

            // Pobierz model eventu przez Traverse (pole prywatne _event)
            var eventModel = Traverse.Create(__instance)
                .Field<EventModel>("_event")
                .Value;

            if (eventModel == null) return;

            string nazwa = "";
            try { nazwa = eventModel.Title.GetFormattedText(); } catch { }

            var opcje = new List<(string, string)>();
            try
            {
                foreach (var opt in eventModel.CurrentOptions)
                {
                    string tytul = "";
                    string opis  = "";
                    try { tytul = opt.Title.GetFormattedText(); } catch { }
                    try { opis  = opt.Description.GetFormattedText(); } catch { }
                    opcje.Add((tytul, opis));
                }
            }
            catch { }

            EventAdvisorOverlay.Pokaz(nazwa, opcje);
        }
        catch (Exception ex) { ModLog.Error("Patch_EventScreen.Postfix", ex); }
    }
}

// Ukryj overlay gdy pokój eventu się zamknie
[HarmonyPatch(typeof(NEventRoom), "_ExitTree")]
public static class Patch_EventScreenExit
{
    static void Postfix()
    {
        EventAdvisorOverlay.Ukryj();
        DeckBuilding.UpgradeAdvisorOverlay.Ukryj();
    }
}
