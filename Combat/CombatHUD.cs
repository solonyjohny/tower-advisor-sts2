using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Entities.Creatures;
using DoradcaWiezy.Util;
using DoradcaWiezy.DeckBuilding;

namespace DoradcaWiezy.Combat;

// HUD walki – jeden RichTextLabel, tylko aktualizacja .Text (brak dynamicznych węzłów)
public static class CombatHUD
{
    private static CanvasLayer?    _layer;
    private static PanelContainer? _panel;
    private static RichTextLabel?  _rtl;
    private static bool _widoczny   = true;
    private static ulong _lastTick  = 0;
    private const  ulong INTERVAL   = 180; // ms

    // ====================== TWORZENIE ======================

    public static void Stworz()
    {
        Zniszcz();
        if (!ModSettings.HudWalki) return;

        try
        {
            _layer       = new CanvasLayer { Layer = 25 };
            NGame.Instance?.AddChild(_layer);

            _panel = new PanelContainer();
            var style = new StyleBoxFlat();
            style.BgColor      = new Color(0.03f, 0.03f, 0.07f, 0.88f);
            style.BorderColor  = new Color(0.60f, 0.50f, 0.20f, 1.0f);
            style.SetBorderWidthAll(1);
            style.SetCornerRadiusAll(5);
            style.SetContentMarginAll(10f);
            _panel.AddThemeStyleboxOverride("panel", style);
            _panel.Position = new Vector2(8, 8);

            _rtl = new RichTextLabel();
            _rtl.BbcodeEnabled = true;
            _rtl.AutowrapMode  = TextServer.AutowrapMode.Off;
            _rtl.FitContent    = true;
            _rtl.ScrollActive  = false;
            _rtl.CustomMinimumSize = new Vector2(330, 0);
            _rtl.AddThemeFontSizeOverride("normal_font_size", 13);
            _rtl.AddThemeFontSizeOverride("bold_font_size",   13);

            _panel.AddChild(_rtl);
            _layer.AddChild(_panel);

            ModLog.Info("HUD walki utworzony.");
        }
        catch (Exception ex) { ModLog.Error("CombatHUD.Stworz", ex); }
    }

    // ====================== AKTUALIZACJA ======================

    public static void Aktualizuj()
    {
        if (!ModSettings.HudWalki) return;
        if (_layer == null) Stworz();
        if (_rtl == null || !_widoczny) return;

        ulong now = Time.GetTicksMsec();
        if (now - _lastTick < INTERVAL) return;
        _lastTick = now;

        try
        {
            var sb = new System.Text.StringBuilder();

            // Gracz
            var (hp, maxHp, blok) = CombatStateTracker.PobierzHPGracza();
            if (hp > 0)
            {
                string hpKol = hp < maxHp * 0.35f ? "#ff6060" : "#f5e68c";
                sb.Append($"[color={hpKol}]{L.HudTy}: {hp}/{maxHp} HP[/color]");
                if (blok > 0) sb.Append($"  [color=#66aaff]🛡{blok}[/color]");
                sb.AppendLine();

                // Debuff gracza
                var buffyGracza = CombatStateTracker.PobierzBuffyGracza();
                var debuffOpisy = new List<string>();
                if (buffyGracza.Contains("WeakPower"))       debuffOpisy.Add(L.DebuffSlaby);
                if (buffyGracza.Contains("VulnerablePower")) debuffOpisy.Add(L.DebuffPodatny);
                if (buffyGracza.Contains("FrailPower"))      debuffOpisy.Add(L.DebuffWatly);
                if (buffyGracza.Contains("ConstrictPower"))  debuffOpisy.Add(L.DebuffDuszon);
                if (buffyGracza.Contains("PoisonPower"))     debuffOpisy.Add(L.DebuffTrucizna);
                if (debuffOpisy.Count > 0)
                    sb.AppendLine($"[color=#ff8855]{L.HudDebuffPrefix}{string.Join(", ", debuffOpisy)}[/color]");

                if (buffyGracza.Contains("CorruptionPower"))
                    sb.AppendLine($"[color=#aa44ff]{L.HudCorruption}[/color]");

                // Statystyki decku
                var deck = CombatStateTracker.PobierzDeck();
                if (deck.Count > 0)
                    sb.AppendLine("[color=#777777]" + ZbudujStatsDeck(deck) + "[/color]");
            }

            // Wrogowie
            foreach (var (_, intent) in CombatStateTracker.IntencjeWrogow)
            {
                if (intent.Owner == null) continue;
                var (wHp, wMaxHp, wBlok) = CombatStateTracker.PobierzHPKreature(intent.Owner);
                string wNazwa = PobierzNazweWroga(intent.Owner);
                string line   = $"{wNazwa}: {wHp}/{wMaxHp} HP";
                if (wBlok > 0) line += $" 🛡{wBlok}";

                if (intent.JestAtakiem && intent.ObrazeniaLaczne > 0)
                {
                    line += $"  [color=#ff8888]ATK:{intent.ObrazeniaLaczne}[/color]";
                    if (intent.LiczbaUderzen > 1)
                        line += $"[color=#ffaa88]×{intent.LiczbaUderzen}[/color]";
                }
                else
                {
                    // Skrócony opis intencji (usuń emoji + skróć)
                    string typKrotki = intent.TypPL
                        .Replace("⚔ ", "").Replace("💪 ", "").Replace("🛡 ", "")
                        .Replace("☠ ", "").Replace("💤 ", "").Replace("👾 ", "");
                    if (typKrotki.Length > 20) typKrotki = typKrotki[..18] + ".";
                    line += $"  [color=#aaddaa]{typKrotki}[/color]";
                }
                sb.AppendLine(line);
            }

            // Specjalne buffy wrogów – ostrzeżenia
            foreach (var (_, sint) in CombatStateTracker.IntencjeWrogow)
            {
                if (sint.Owner == null) continue;
                string wn = PobierzNazweWroga(sint.Owner);

                if (CombatStateTracker.CzyMaSliskosc(sint.Owner))
                    sb.AppendLine($"[color=#88ffff]{L.HudWrogSliskosc(wn)}[/color]");

                if (CombatStateTracker.CzyMaIntangible(sint.Owner))
                    sb.AppendLine($"[color=#88ffff]{L.HudWrogNiematerialny(wn)}[/color]");

                int limit = CombatStateTracker.PobierzLimitDmg(sint.Owner);
                if (limit < int.MaxValue)
                    sb.AppendLine($"[color=#ffaa44]{L.HudWrogPancerz(wn, limit)}[/color]");
            }

            // Separator
            sb.AppendLine("[color=#333355]─────────────────────────────────[/color]");

            // Porada walki
            var best = HandAdvisor.AnalizujRuch();

            if (best.CzyLethal)
                sb.AppendLine($"[color=#55ff55][b]{L.HudLethal}[/b][/color]");
            else if (best.CzySurvive)
                sb.AppendLine($"[color=#ff4444][b]{L.HudSurvive}[/b][/color]");

            if (!string.IsNullOrEmpty(best.KartaNazwa))
            {
                string kol = best.CzyLethal ? "#55ff55" : best.CzySurvive ? "#ff6666" : "#f5e68c";
                sb.AppendLine($"[b][color={kol}]{L.HudZagraj}{best.KartaNazwa}[/color][/b]");

                if (!string.IsNullOrEmpty(best.CelNazwa))
                    sb.AppendLine($"[color=#ff9999]{L.HudCel}{best.CelNazwa}[/color]");

                if (!string.IsNullOrEmpty(best.Powod))
                    sb.AppendLine($"[color=#ffcc44]   {best.Powod}[/color]");

                if (!string.IsNullOrEmpty(best.Sekwencja))
                    sb.AppendLine($"[color=#888888]   {best.Sekwencja}[/color]");
            }
            else if (hp > 0)
            {
                sb.AppendLine($"[color=#888888]{L.HudZagrajDowolna}[/color]");
            }

            _rtl.Text = sb.ToString().TrimEnd('\n', '\r');
        }
        catch (Exception ex) { ModLog.Error("CombatHUD.Aktualizuj", ex); }
    }

    // ====================== POMOCNIKI ======================

    private static string ZbudujStatsDeck(List<MegaCrit.Sts2.Core.Models.CardModel> deck)
    {
        int atk = 0, skl = 0, pow = 0;
        float costSum = 0; int costCnt = 0;
        foreach (var k in deck)
        {
            string typ = CardEvaluator.PobierzTyp(k);
            if      (typ == "Attack") atk++;
            else if (typ == "Skill")  skl++;
            else if (typ == "Power")  pow++;
            int c = CardEvaluator.PobierzKoszt(k);
            if (c >= 0) { costSum += c; costCnt++; }
        }
        string avg = costCnt > 0 ? (costSum / costCnt).ToString("0.0") : "?";
        return L.StatsDeck(deck.Count, atk, skl, pow, avg);
    }

    // public bo używa HandAdvisor
    public static string PobierzNazweWroga(Creature c)
    {
        // Próba 1: property Name
        try
        {
            string? n = Traverse.Create(c).Property<string>("Name").Value;
            if (!string.IsNullOrEmpty(n)) return n;
        }
        catch { }
        // Próba 2: Model.Title.GetFormattedText()
        try
        {
            object? model = Traverse.Create(c).Property("Model").GetValue();
            if (model != null)
            {
                object? title = Traverse.Create(model).Property("Title").GetValue();
                if (title != null)
                {
                    var gft = title.GetType().GetMethod("GetFormattedText");
                    if (gft != null) return gft.Invoke(title, null)?.ToString() ?? L.HudWrog;
                    return title.ToString() ?? L.HudWrog;
                }
            }
        }
        catch { }
        return L.HudWrog;
    }

    // ====================== STEROWANIE ======================

    public static void PrzelaczWidocznosc()
    {
        _widoczny = !_widoczny;
        if (_layer != null) _layer.Visible = _widoczny;
        ModLog.Info($"HUD walki: {(_widoczny ? "Włączony" : "Wyłączony")}");
    }

    public static void Ukryj()
    {
        try { if (_layer != null) _layer.Visible = false; } catch { }
    }

    public static void Pokaz()
    {
        try { if (_layer != null && _widoczny) _layer.Visible = true; } catch { }
    }

    public static void Zniszcz()
    {
        try { _layer?.QueueFree(); } catch { }
        _layer = null; _panel = null; _rtl = null;
    }
}
