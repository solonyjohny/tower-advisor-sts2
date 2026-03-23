using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Entities.Cards;
using DoradcaWiezy.Util;

namespace DoradcaWiezy.DeckBuilding;

public enum OcenaPoziom { Doskonala, Dobra, Srednia, Slaba }

public class WynikOceny
{
    public CardModel Karta      { get; set; } = null!;
    public OcenaPoziom Ocena   { get; set; }
    public string OcenaStr     { get; set; } = "";
    public string Powod        { get; set; } = "";
    public int PunktyDebug     { get; set; }
}

public static class CardEvaluator
{
    public static WynikOceny OcenKarte(CardModel karta, List<CardModel> deck)
    {
        int punkty = 0;
        var powody = new List<string>();

        string rarityStr  = PobierzRarosc(karta);
        int koszt         = PobierzKoszt(karta);
        int obrazenia     = PobierzWartosc(karta, "Damage");
        int obrona        = PobierzWartosc(karta, "Block");
        bool maDobieranie = CzyMaDobieranie(karta);
        bool maEnergie    = CzyMaEnergie(karta);
        bool maSile       = CzyMaSile(karta);
        bool maWyczerpanie = CzyMaWyczerpanie(karta);
        string typStr     = PobierzTyp(karta);

        // === Rzadkość ===
        punkty += rarityStr switch
        {
            "Rare"     => 40,
            "Uncommon" => 20,
            _          => 5
        };
        if (rarityStr == "Rare") powody.Add(L.CE_Rzadka);

        // === Typ – Power (Moc) to zawsze cenny trwały efekt ===
        if (typStr == "Power")
        {
            punkty += 35;
            powody.Add(L.CE_MocTrwaly);
        }

        // === Koszt 0 – zawsze graj za darmo ===
        if (koszt == 0)
        {
            punkty += 45;
            powody.Add(L.CE_Bezplatna);
        }

        // === Dobieranie kart – napędza deck ===
        if (maDobieranie)
        {
            punkty += 30;
            powody.Add(L.CE_Dobieranie);
        }

        // === Generowanie energii – S-tier ===
        if (maEnergie)
        {
            punkty += 45;
            powody.Add(L.CE_Energia);
        }

        // === Siła – skaluje wszystkie ataki ===
        if (maSile)
        {
            punkty += 25;
            powody.Add(L.CE_Sila);
        }

        // === Wyczerpanie – często synergii ===
        if (maWyczerpanie) punkty += 8;

        // === Efektywność obrażeń na energię ===
        if (koszt > 0 && obrazenia > 0)
        {
            float ratio = (float)obrazenia / koszt;
            if (ratio >= 18)      { punkty += 30; powody.Add(L.CE_AtkWybitny); }
            else if (ratio >= 12) { punkty += 20; powody.Add(L.CE_AtkMocny); }
            else if (ratio >= 8)  { punkty += 10; }
        }
        else if (koszt == 0 && obrazenia > 0)
        {
            punkty += 20;
            powody.Add(L.CE_DarmowyAtk);
        }

        // === Obrona ===
        if (obrona >= 12) { punkty += 15; powody.Add(L.CE_SolidnaObrona); }
        else if (obrona >= 8) { punkty += 8; }
        else if (obrona > 0 && koszt == 0) { punkty += 10; }

        // === Analiza składu decku ===
        int liczbaDeck = deck.Count;
        if (liczbaDeck > 0)
        {
            int iloscAtakow    = deck.Count(k => PobierzTyp(k) == "Attack");
            int iloscMocy      = deck.Count(k => PobierzTyp(k) == "Power");
            float propAtakow   = (float)iloscAtakow / liczbaDeck;
            float propMocy     = (float)iloscMocy / liczbaDeck;

            // Brak ataków → ataki są pilne
            if (typStr == "Attack" && propAtakow < 0.35f)
            { punkty += 25; powody.Add(L.CE_BrakAtakow); }

            // Brak obrony → karty obrony cenne
            if (obrona > 0 && propAtakow > 0.70f)
            { punkty += 20; powody.Add(L.CE_BrakObrony); }

            // Dużo ataków + karta Siły → synergia
            if (maSile && propAtakow > 0.45f)
            { punkty += 15; powody.Add(L.CE_SynergiaAtaki); }

            // Moc wzmacnia deck bogaty w ataki
            if (typStr == "Power" && propAtakow > 0.45f)
            { punkty += 10; powody.Add(L.CE_WzmacniaAtaki); }

            // Mały deck = dobry czas na karty jakościowe
            if (liczbaDeck < 12)
            { punkty += 10; powody.Add(L.CE_MalyDeck); }
            else if (liczbaDeck > 25)
            { punkty -= 15; powody.Add(L.CE_DuzyDeck); }
            if (liczbaDeck > 35) punkty -= 10;

            // === Exhaust Engine – synergie z Corruption ===
            string idKarty = PobierzIdKarty(karta);
            bool deckMaCorruption      = deck.Any(k => PobierzIdKarty(k) == "Corruption");
            bool deckMaExhaustSynergię = deck.Any(k => PobierzIdKarty(k) is "DarkEmbrace" or "FeelNoPain");

            if ((idKarty is "DarkEmbrace" or "FeelNoPain") && deckMaCorruption)
            {
                punkty += 50;
                powody.Insert(0, L.CE_ExhaustSynergy);
            }
            else if (idKarty == "Corruption" && deckMaExhaustSynergię)
            {
                punkty += 50;
                powody.Insert(0, L.CE_ExhaustAktywuje);
            }
            else if (typStr == "Skill" && deckMaCorruption && idKarty != "Corruption")
            {
                punkty += 20;
                powody.Add(L.CE_CorruptionSkill);
            }
        }

        // === Przekleństwo / Status ===
        if (typStr is "Curse" or "Status")
        {
            punkty = -100;
            powody.Clear();
            powody.Add(L.CE_Przeklenstwo);
        }

        // === Próg oceny ===
        OcenaPoziom ocena = punkty switch
        {
            >= 70 => OcenaPoziom.Doskonala,
            >= 50 => OcenaPoziom.Dobra,
            >= 30 => OcenaPoziom.Srednia,
            _     => OcenaPoziom.Slaba
        };

        // Nazwa karty w ocenie (żeby wiedzieć której to dotyczy)
        string nazwaKarty = PobierzNazwe(karta);

        bool eng = ModSettings.JezykAngielski;
        string ocenaStr = ocena switch
        {
            OcenaPoziom.Doskonala => eng ? $"★★★ {nazwaKarty} – Take!" : $"★★★ {nazwaKarty} – Weź!",
            OcenaPoziom.Dobra     => eng ? $"★★  {nazwaKarty} – Good"  : $"★★  {nazwaKarty} – Dobra",
            OcenaPoziom.Srednia   => eng ? $"★    {nazwaKarty} – Skip?" : $"★    {nazwaKarty} – Pomiń?",
            _                     => eng ? $"✗    {nazwaKarty} – Skip"  : $"✗    {nazwaKarty} – Pomiń"
        };

        return new WynikOceny
        {
            Karta       = karta,
            Ocena       = ocena,
            OcenaStr    = ocenaStr,
            Powod       = powody.FirstOrDefault() ?? "",
            PunktyDebug = punkty
        };
    }

    public static CardModel? SugerujUlepszenie(List<CardModel> deck)
    {
        return deck
            .Where(k => !k.IsUpgraded && PobierzTyp(k) is "Attack" or "Skill")
            .OrderByDescending(k =>
                PobierzWartosc(k, "Damage") * 2 + PobierzWartosc(k, "Block") * 2 +
                (PobierzKoszt(k) == 1 ? 20 : 0) +
                (CzyMaDobieranie(k) ? 15 : 0) +
                (CzyMaSile(k) ? 25 : 0))
            .FirstOrDefault();
    }

    // ======================== Pomocniki ========================

    public static string PobierzIdKarty(CardModel k)
    {
        try { return k.GetType().Name; }
        catch { return ""; }
    }

    public static string PobierzNazwe(CardModel k)
    {
        try { return k.Title; }
        catch { return "Karta"; }
    }

    public static string PobierzRarosc(CardModel k)
    {
        try { return k.Rarity.ToString(); }
        catch { return "Common"; }
    }

    public static string PobierzTyp(CardModel k)
    {
        try { return k.Type.ToString(); }
        catch { return "Unknown"; }
    }

    public static int PobierzKoszt(CardModel k)
    {
        try { return k.EnergyCost.Canonical; }
        catch { return 1; }
    }

    public static int PobierzWartosc(CardModel k, string klucz)
    {
        try
        {
            if (k.DynamicVars.TryGetValue(klucz, out var val))
                return (int)val.BaseValue;
        }
        catch { }
        return 0;
    }

    public static bool CzyMaDobieranie(CardModel k)
    {
        try
        {
            return k.DynamicVars.ContainsKey("Draw") ||
                   k.DynamicVars.ContainsKey("CardDraw") ||
                   k.DynamicVars.ContainsKey("Cards");
        }
        catch { return false; }
    }

    public static bool CzyMaEnergie(CardModel k)
    {
        try
        {
            return k.DynamicVars.ContainsKey("Energy") ||
                   k.DynamicVars.ContainsKey("EnergyGain") ||
                   k.DynamicVars.ContainsKey("Recharge") ||
                   k.DynamicVars.ContainsKey("MaxEnergy");
        }
        catch { return false; }
    }

    public static bool CzyMaSile(CardModel k)
    {
        try
        {
            return k.DynamicVars.ContainsKey("Strength") ||
                   k.DynamicVars.ContainsKey("StrengthGain") ||
                   k.DynamicVars.ContainsKey("StrengthMultiplier");
        }
        catch { return false; }
    }

    public static bool CzyMaWyczerpanie(CardModel k)
    {
        try
        {
            return k.DynamicVars.ContainsKey("Exhaust") ||
                   k.DynamicVars.ContainsKey("ExhaustCount");
        }
        catch { return false; }
    }
}
