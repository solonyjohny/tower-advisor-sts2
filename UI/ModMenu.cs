using Godot;
using DoradcaWiezy.Util;

namespace DoradcaWiezy.UI;

// Menu ustawień moda – otwierane przez F2
public static class ModMenu
{
    private static CanvasLayer? _warstwa;
    private static bool _widoczne;

    private static PanelContainer? _panel;

    public static void PrzelaczWidocznosc()
    {
        if (_widoczne) Zamknij();
        else Otworz();
    }

    public static void Otworz()
    {
        if (_widoczne) return;
        _widoczne = true;

        try
        {
            _warstwa = new CanvasLayer();
            _warstwa.Layer = 50; // ponad wszystkim
            MegaCrit.Sts2.Core.Nodes.NGame.Instance?.AddChild(_warstwa);

            // Tło przyciemnienia
            var overlay = new ColorRect();
            overlay.Color = new Color(0, 0, 0, 0.5f);
            overlay.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
            _warstwa.AddChild(overlay);

            // Główny panel
            _panel = new PanelContainer();
            _panel.CustomMinimumSize = new Vector2(320, 0);
            _panel.AddThemeStyleboxOverride("panel", UIStyle.StworzTloPanel(
                UIStyle.TloPanel with { A = 0.98f }, UIStyle.RamkaNormal, 2f));

            var vbox = new VBoxContainer();
            vbox.AddThemeConstantOverride("separation", 10);

            // === Nagłówek ===
            var naglowekRow = new HBoxContainer();
            naglowekRow.AddChild(UIStyle.StworzNaglowek(L.MenuTytul));
            vbox.AddChild(naglowekRow);
            vbox.AddChild(UIStyle.StworzSeparator());

            // === Checkboxy ===
            DodajCheckbox(vbox, L.MenuHUD,
                ModSettings.HudWalki,
                (val) => { ModSettings.HudWalki = val; });

            DodajCheckbox(vbox, L.MenuDoradcaDecku,
                ModSettings.DoradcaDecku,
                (val) => { ModSettings.DoradcaDecku = val; });

            DodajCheckbox(vbox, L.MenuDoradcaSciezki,
                ModSettings.DoradcaSciezki,
                (val) => { ModSettings.DoradcaSciezki = val; });

            DodajCheckbox(vbox, L.MenuDoradcaWydarzen,
                ModSettings.DoradcaWydarzen,
                (val) => { ModSettings.DoradcaWydarzen = val; });

            vbox.AddChild(UIStyle.StworzSeparator());

            // === Wybór języka ===
            vbox.AddChild(UIStyle.StworzEtykiete(L.MenuJezyk, UIStyle.TekstGlowny, UIStyle.RozmiarTekstuNormal));
            var jezykRow = new HBoxContainer();
            jezykRow.AddThemeConstantOverride("separation", 6);

            var btnPL = new Button();
            btnPL.Text = "🇵🇱 Polski";
            btnPL.TooltipText = "Interfejs po polsku";
            if (!ModSettings.JezykAngielski)
                btnPL.AddThemeColorOverride("font_color", new Color(0.3f, 1f, 0.3f));
            btnPL.Pressed += () =>
            {
                ModSettings.JezykAngielski = false;
                ModSettings.Save();
                Zamknij();
                Otworz();
            };

            var btnEN = new Button();
            btnEN.Text = "🇬🇧 English";
            btnEN.TooltipText = "English interface";
            if (ModSettings.JezykAngielski)
                btnEN.AddThemeColorOverride("font_color", new Color(0.3f, 1f, 0.3f));
            btnEN.Pressed += () =>
            {
                ModSettings.JezykAngielski = true;
                ModSettings.Save();
                Zamknij();
                Otworz();
            };

            jezykRow.AddChild(btnPL);
            jezykRow.AddChild(btnEN);
            vbox.AddChild(jezykRow);

            vbox.AddChild(UIStyle.StworzSeparator());

            // === Skróty klawiszowe ===
            vbox.AddChild(UIStyle.StworzEtykiete(L.MenuToggleHotkey, UIStyle.TekstSzary, UIStyle.RozmiarTekstuMaly));
            vbox.AddChild(UIStyle.StworzEtykiete(L.MenuMenuHotkey,   UIStyle.TekstSzary, UIStyle.RozmiarTekstuMaly));

            vbox.AddChild(UIStyle.StworzSeparator());

            // === Przycisk zamknij ===
            var btnZamknij = new Button();
            btnZamknij.Text = L.Zamknij;
            btnZamknij.AddThemeColorOverride("font_color", UIStyle.TekstNaglowek);
            btnZamknij.Pressed += () => { ModSettings.Save(); Zamknij(); };
            vbox.AddChild(btnZamknij);

            _panel.AddChild(vbox);

            // Wycentruj panel
            _panel.SetAnchorsPreset(Control.LayoutPreset.Center);
            _panel.Position = new Vector2(-160, -150);
            _warstwa.AddChild(_panel);
        }
        catch (Exception ex) { ModLog.Error("ModMenu.Otworz", ex); }
    }

    public static void Zamknij()
    {
        _widoczne = false;
        try { _warstwa?.QueueFree(); _warstwa = null; _panel = null; }
        catch { }
    }

    // Obsługa kliknięcia Escape
    public static bool ObsluzEscape(InputEventKey key)
    {
        if (_widoczne && key.Keycode == Key.Escape && key.Pressed)
        {
            ModSettings.Save();
            Zamknij();
            return true;
        }
        return false;
    }

    private static void DodajCheckbox(VBoxContainer parent, string etykieta, bool wartosc, Action<bool> onChange)
    {
        var row = new HBoxContainer();
        row.AddThemeConstantOverride("separation", 8);

        var chk = new CheckBox();
        chk.ButtonPressed = wartosc;
        chk.Toggled += (val) =>
        {
            onChange(val);
            ModSettings.Save();
        };

        row.AddChild(chk);
        row.AddChild(UIStyle.StworzEtykiete(etykieta, UIStyle.TekstGlowny, UIStyle.RozmiarTekstuNormal));
        parent.AddChild(row);
    }
}
