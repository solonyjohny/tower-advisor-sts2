using HarmonyLib;
using Godot;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Screens;
using MegaCrit.Sts2.Core.Nodes.Rewards;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Models;
using DoradcaWiezy.UI;
using DoradcaWiezy.Util;

namespace DoradcaWiezy.Shops;

// Overlay doradcy łupów po walce
public static class RewardAdvisorOverlay
{
    private static CanvasLayer? _warstwa;

    public static void Pokaz(NRewardsScreen ekran)
    {
        if (!ModSettings.DoradcaDecku) return;
        Ukryj();

        try
        {
            // Zbierz wszystkie przyciski nagród
            var przyciski = ekran.GetChildren()
                .OfType<Control>()
                .SelectMany(c => c.GetChildren())
                .Concat(ekran.GetChildren())
                .OfType<NRewardButton>()
                .Where(b => b.Reward != null)
                .ToList();

            // Fallback: szukaj głębiej w drzewie
            if (przyciski.Count == 0)
                przyciski = ZnajdzPrzyciskiRekurencyjnie(ekran);

            if (przyciski.Count == 0) return;

            _warstwa = new CanvasLayer();
            _warstwa.Layer = 22;
            NGame.Instance?.AddChild(_warstwa);

            var (panel, zawartosc) = UIStyle.StworzPanel("OCENA ŁUPÓW", 270f);
            var deck = Combat.CombatStateTracker.PobierzDeck();

            foreach (var btn in przyciski)
            {
                var reward = btn.Reward;
                if (reward == null) continue;

                string ocena = "";
                string powod = "";
                Color kolor  = UIStyle.TekstGlowny;

                if (reward is RelicReward relicReward)
                {
                    var (relicNazwa, relicOpis) = PobierzDaneRelikwii(relicReward);
                    (ocena, powod, kolor) = OcenRelikwie(relicNazwa, relicOpis, deck);
                }
                else if (reward is CardReward)
                {
                    ocena = "→ Wybierz kartę";
                    kolor = UIStyle.TekstPorada;
                }
                else
                {
                    // Złoto lub inne
                    string opis = "";
                    try { opis = reward.Description.GetFormattedText(); } catch { }
                    ocena = string.IsNullOrEmpty(opis) ? "Nagroda" : opis;
                    kolor = UIStyle.TekstNaglowek;
                }

                var vbox = new VBoxContainer();
                vbox.AddThemeConstantOverride("separation", 1);
                vbox.AddChild(UIStyle.StworzEtykiete(ocena, kolor, UIStyle.RozmiarTekstuNormal));
                if (!string.IsNullOrEmpty(powod))
                    vbox.AddChild(UIStyle.StworzEtykiete("  " + powod, UIStyle.TekstSzary, UIStyle.RozmiarTekstuMaly));
                zawartosc.AddChild(vbox);
                zawartosc.AddChild(UIStyle.StworzSeparator());
            }

            panel.SetAnchorsPreset(Control.LayoutPreset.CenterLeft);
            panel.Position = new Vector2(10f, -80f);
            _warstwa.AddChild(panel);
        }
        catch (Exception ex) { ModLog.Error("RewardAdvisorOverlay.Pokaz", ex); }
    }

    private static List<NRewardButton> ZnajdzPrzyciskiRekurencyjnie(Node parent, int depth = 0)
    {
        var wynik = new List<NRewardButton>();
        if (depth > 5) return wynik;
        foreach (var child in parent.GetChildren())
        {
            if (child is NRewardButton btn && btn.Reward != null) wynik.Add(btn);
            wynik.AddRange(ZnajdzPrzyciskiRekurencyjnie(child, depth + 1));
        }
        return wynik;
    }

    private static (string nazwa, string opis) PobierzDaneRelikwii(RelicReward relicReward)
    {
        string nazwa = "";
        string opis  = "";
        try
        {
            var relic = Traverse.Create(relicReward).Field<RelicModel>("_relic").Value;
            if (relic != null)
            {
                try { nazwa = relic.Title.GetFormattedText(); } catch { }
                try { opis  = relic.Description.GetFormattedText(); } catch { }
            }
        }
        catch { }
        if (string.IsNullOrEmpty(nazwa))
            try { nazwa = relicReward.Description.GetFormattedText(); } catch { }
        return (nazwa, opis);
    }

    private static (string ocena, string powod, Color kolor) OcenRelikwie(
        string nazwa, string opis, List<CardModel> deck)
    {
        string combined = (nazwa + " " + opis).ToLower();

        // Analiza składu decku
        int liczbaDeck = deck.Count;
        int iloscAtakow = deck.Count(k => { try { return k.Type.ToString() == "Attack"; } catch { return false; } });
        float propAtakow = liczbaDeck > 0 ? (float)iloscAtakow / liczbaDeck : 0.5f;
        var (hp, maxHp, _) = Combat.CombatStateTracker.PobierzHPGracza();
        float propHP = maxHp > 0 ? (float)hp / maxHp : 1f;

        // Generowanie energii
        if (combined.Contains("energi") || combined.Contains("energy") || combined.Contains("mana"))
            return ($"★★★ {nazwa} – Weź! (energia)", "Energia = więcej kart zagranych", UIStyle.TekstPorada);

        // Siła / skalowanie ataków
        if (combined.Contains("siły") || combined.Contains("siłę") || combined.Contains("strength"))
        {
            if (propAtakow > 0.4f)
                return ($"★★★ {nazwa} – Weź! (synergia)", "Siła wzmacnia Twoje ataki", UIStyle.TekstPorada);
            return ($"★★  {nazwa} – Dobra", "Siła (mało ataków w decku)", UIStyle.TekstNaglowek);
        }

        // Leczenie / HP
        if (combined.Contains("lecz") || combined.Contains("hp") || combined.Contains("pż") || combined.Contains("życ"))
        {
            return propHP < 0.6f
                ? ($"★★★ {nazwa} – Weź! (HP)", "Mało HP – leczenie priorytet", UIStyle.TekstPorada)
                : ($"★★  {nazwa} – Dobra", "Leczenie zawsze przydatne", UIStyle.TekstNaglowek);
        }

        // Dobieranie kart
        if (combined.Contains("dobier") || combined.Contains("draw") || combined.Contains("kart"))
            return ($"★★  {nazwa} – Dobra", "Dobieranie kart napędza deck", UIStyle.TekstNaglowek);

        // Max HP
        if (combined.Contains("maks") || combined.Contains("max"))
            return ($"★★★ {nazwa} – Weź! (max HP)", "Max HP = większy margines błędu", UIStyle.TekstPorada);

        // Blok / Obrona
        if (combined.Contains("blok") || combined.Contains("block") || combined.Contains("obron"))
            return ($"★★  {nazwa} – Dobra", "Obrona zawsze przydatna", UIStyle.TekstNaglowek);

        // Złoto
        if (combined.Contains("złot") || combined.Contains("gold"))
            return ($"★    {nazwa} – Rozważ", "Złoto = zakupy u handlarza", UIStyle.TekstGlowny);

        // Domyślna ocena dla nieznanej relikwii
        return ($"?    {nazwa}", string.IsNullOrEmpty(opis) ? "Przeczytaj efekt" : opis[..Math.Min(60, opis.Length)], UIStyle.TekstSzary);
    }

    public static void Ukryj()
    {
        try { _warstwa?.QueueFree(); _warstwa = null; }
        catch { }
    }
}

// ======================================================================
// Patch – NRewardsScreen._Ready (ekran łupów po walce)
// ======================================================================
[HarmonyPatch(typeof(NRewardsScreen), "_Ready")]
public static class Patch_RewardsScreen
{
    static void Postfix(NRewardsScreen __instance)
    {
        try { RewardAdvisorOverlay.Pokaz(__instance); }
        catch (Exception ex) { ModLog.Error("Patch_RewardsScreen", ex); }
    }
}

[HarmonyPatch(typeof(NRewardsScreen), "_ExitTree")]
public static class Patch_RewardsScreenExit
{
    static void Postfix() => RewardAdvisorOverlay.Ukryj();
}
