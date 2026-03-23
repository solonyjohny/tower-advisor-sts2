using Godot;
using DoradcaWiezy.Util;

namespace DoradcaWiezy.UI;

// Centralny styl UI – wszystkie panele wyglądają spójnie
public static class UIStyle
{
    // Kolory motywu (złoto + ciemne tło jak w STS)
    public static Color TloPanel      => new(0.08f, 0.08f, 0.12f, ModSettings.Przezroczystosc);
    public static Color TloNaglowek   => new(0.15f, 0.12f, 0.05f, 0.98f);
    public static Color RamkaNormal   => new(0.6f,  0.5f,  0.2f,  1.0f);
    public static Color RamkaOstrzez  => new(0.9f,  0.3f,  0.1f,  1.0f);
    public static Color RamkaBezpiecz => new(0.2f,  0.7f,  0.3f,  1.0f);

    public static Color TekstGlowny   => new(0.95f, 0.90f, 0.75f, 1.0f); // kremowy
    public static Color TekstNaglowek => new(1.0f,  0.85f, 0.35f, 1.0f); // złoty
    public static Color TekstAtakRed  => new(1.0f,  0.35f, 0.35f, 1.0f); // czerwony – atak
    public static Color TekstBlok     => new(0.35f, 0.75f, 1.0f,  1.0f); // niebieski – obrona
    public static Color TekstPorada   => new(0.75f, 1.0f,  0.45f, 1.0f); // zielony – porada
    public static Color TekstOstrzez  => new(1.0f,  0.75f, 0.2f,  1.0f); // pomarańczowy
    public static Color TekstSzary    => new(0.55f, 0.55f, 0.55f, 1.0f);

    public const int RozmiarTekstuMaly   = 13;
    public const int RozmiarTekstuNormal = 15;
    public const int RozmiarTekstuDuzy   = 17;
    public const int RozmiarNaglowka     = 14;

    // Tworzy styl panelu (zaokrąglone rogi, ramka)
    public static StyleBoxFlat StworzTloPanel(Color? kolor = null, Color? ramka = null, float gruboscRamki = 1.5f)
    {
        var style = new StyleBoxFlat();
        style.BgColor = kolor ?? TloPanel;
        style.BorderColor = ramka ?? RamkaNormal;
        style.SetBorderWidthAll((int)gruboscRamki);
        style.SetCornerRadiusAll(6);
        style.SetContentMarginAll(8);
        return style;
    }

    // Tworzy styl nagłówka panelu
    public static StyleBoxFlat StworzTloNaglowek()
    {
        var style = new StyleBoxFlat();
        style.BgColor = TloNaglowek;
        style.SetCornerRadiusAll(4);
        style.SetContentMarginAll(5);
        return style;
    }

    // Tworzy etykietę z domyślnym stylem
    public static Label StworzEtykiete(string tekst, Color? kolor = null, int rozmiar = RozmiarTekstuNormal)
    {
        var label = new Label();
        label.Text = tekst;
        label.AddThemeColorOverride("font_color", kolor ?? TekstGlowny);
        label.AddThemeFontSizeOverride("font_size", rozmiar);
        label.AddThemeConstantOverride("outline_size", 3);
        label.AddThemeColorOverride("font_outline_color", new Color(0, 0, 0, 0.8f));
        return label;
    }

    // Tworzy etykietę nagłówka
    public static Label StworzNaglowek(string tekst)
    {
        var label = new Label();
        label.Text = tekst;
        label.AddThemeColorOverride("font_color", TekstNaglowek);
        label.AddThemeFontSizeOverride("font_size", RozmiarNaglowka);
        label.AddThemeConstantOverride("outline_size", 4);
        label.AddThemeColorOverride("font_outline_color", new Color(0, 0, 0, 1f));
        label.HorizontalAlignment = HorizontalAlignment.Center;
        return label;
    }

    // Tworzy separator
    public static HSeparator StworzSeparator()
    {
        var sep = new HSeparator();
        sep.AddThemeColorOverride("color", RamkaNormal);
        return sep;
    }

    // Buduje gotowy panel z nagłówkiem i VBoxem na zawartość
    public static (PanelContainer panel, VBoxContainer zawartosc) StworzPanel(
        string tytul, float szerokosc = 220f, Color? kolorRamki = null)
    {
        var panel = new PanelContainer();
        panel.AddThemeStyleboxOverride("panel", StworzTloPanel(ramka: kolorRamki));
        panel.CustomMinimumSize = new Vector2(szerokosc, 0);

        var vboxGlowny = new VBoxContainer();
        vboxGlowny.AddThemeConstantOverride("separation", 4);

        // Nagłówek
        var naglowekContainer = new PanelContainer();
        naglowekContainer.AddThemeStyleboxOverride("panel", StworzTloNaglowek());
        naglowekContainer.AddChild(StworzNaglowek(tytul));
        vboxGlowny.AddChild(naglowekContainer);

        var sep = StworzSeparator();
        vboxGlowny.AddChild(sep);

        // Kontener na zawartość
        var zawartosc = new VBoxContainer();
        zawartosc.AddThemeConstantOverride("separation", 3);
        vboxGlowny.AddChild(zawartosc);

        panel.AddChild(vboxGlowny);
        return (panel, zawartosc);
    }
}
