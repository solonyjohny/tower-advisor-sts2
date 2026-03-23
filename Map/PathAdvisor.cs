using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Models;
using DoradcaWiezy.UI;
using DoradcaWiezy.Util;

namespace DoradcaWiezy.Map;

public static class PathAdvisor
{
    public class AnalizaSciezki
    {
        public string Rekomendacja   { get; set; } = "";
        public string DetailOpisStr  { get; set; } = "";
    }

    public static AnalizaSciezki? OstatniaAnaliza { get; private set; }

    public static AnalizaSciezki Analizuj()
    {
        var analiza = new AnalizaSciezki();

        try
        {
            var runState = Traverse.Create(RunManager.Instance)
                .Property<RunState>("State").Value;

            var (hp, maxHp, _) = Combat.CombatStateTracker.PobierzHPGracza();
            float propHP = maxHp > 0 ? (float)hp / maxHp : 1f;

            int zloto = PobierzZloto(runState);
            var rekom = new List<string>();

            if (propHP < 0.35f)    rekom.Add(L.SciezkaPorada_MaloHP);
            if (zloto > 150)       rekom.Add(L.SciezkaPorada_DuzoZlota);
            if (MoznaUlepszyc())   rekom.Add(L.SciezkaPorada_NeedUpgrade);
            rekom.Add(propHP > 0.65f ? L.SciezkaPorada_EliteOK : L.SciezkaPorada_EliteRyzyko);

            analiza.Rekomendacja  = rekom.FirstOrDefault() ?? "";
            analiza.DetailOpisStr = string.Join("\n", rekom);
        }
        catch (Exception ex) { ModLog.Error("PathAdvisor.Analizuj", ex); }

        OstatniaAnaliza = analiza;
        return analiza;
    }

    private static int PobierzZloto(RunState? state)
    {
        if (state == null) return 0;
        try { return Traverse.Create(state).Property<int>("Gold").Value; } catch { return 0; }
    }

    private static bool MoznaUlepszyc()
    {
        try
        {
            var deck = Combat.CombatStateTracker.PobierzDeck();
            return deck.Any(k => !k.IsUpgraded &&
                DeckBuilding.CardEvaluator.PobierzTyp(k) is "Attack" or "Skill");
        }
        catch { return true; }
    }
}

// ======================================================================
// Overlay panelu ścieżki
// ======================================================================
public static class PathOverlay
{
    private static CanvasLayer? _warstwa;

    public static void Pokaz()
    {
        if (!ModSettings.DoradcaSciezki) return;
        Ukryj();

        try
        {
            var analiza = PathAdvisor.Analizuj();

            _warstwa = new CanvasLayer();
            _warstwa.Layer = 18;
            NGame.Instance?.AddChild(_warstwa);

            var (panel, zawartosc) = UIStyle.StworzPanel(L.NaglowekSciezka, 240f);

            // HP gracza
            var (hp, maxHp, _) = Combat.CombatStateTracker.PobierzHPGracza();
            if (hp > 0)
            {
                var hpRow = new HBoxContainer();
                hpRow.AddChild(UIStyle.StworzEtykiete("HP: ", UIStyle.TekstSzary, UIStyle.RozmiarTekstuMaly));
                hpRow.AddChild(UIStyle.StworzEtykiete(L.HP(hp, maxHp),
                    hp < maxHp * 0.4f ? UIStyle.TekstAtakRed : UIStyle.TekstPorada,
                    UIStyle.RozmiarTekstuMaly));
                zawartosc.AddChild(hpRow);
                zawartosc.AddChild(UIStyle.StworzSeparator());
            }

            // Rekomendacje
            foreach (var linia in analiza.DetailOpisStr.Split('\n'))
            {
                if (string.IsNullOrWhiteSpace(linia)) continue;
                Color kolor = linia.StartsWith("⚠") ? UIStyle.TekstOstrzez
                            : linia.StartsWith("✓") ? UIStyle.TekstPorada
                            : UIStyle.TekstGlowny;
                var label = UIStyle.StworzEtykiete(linia, kolor, UIStyle.RozmiarTekstuMaly);
                label.AutowrapMode = TextServer.AutowrapMode.Word;
                zawartosc.AddChild(label);
            }

            panel.Position = new Vector2(10, 10);
            _warstwa.AddChild(panel);
        }
        catch (Exception ex) { ModLog.Error("PathOverlay.Pokaz", ex); }
    }

    public static void Ukryj()
    {
        try { _warstwa?.QueueFree(); _warstwa = null; }
        catch { }
    }
}
