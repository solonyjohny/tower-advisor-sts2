using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Entities.Creatures;
using DoradcaWiezy.Util;
using DoradcaWiezy.DeckBuilding;

namespace DoradcaWiezy.Combat;

// Wynik analizy: co zagrać tej tury
public class BestPlay
{
    public string KartaNazwa { get; set; } = "";
    public string CelNazwa   { get; set; } = "";
    public string Powod      { get; set; } = "";
    public string Sekwencja  { get; set; } = "";
    public bool   CzyLethal  { get; set; }
    public bool   CzySurvive { get; set; }
}

// Analizuje rękę i sugeruje kolejność zagrywania kart
public static class HandAdvisor
{
    public class OcenaKarty
    {
        public CardModel Karta       { get; set; } = null!;
        public int       Priorytet   { get; set; }
        public string    Powod       { get; set; } = "";
        public string    WartoscStr  { get; set; } = "";
        public bool      CzyPolecana { get; set; }
    }

    // ====================== KONTEKST WALKI ======================

    // Czy wróg ma intent inny niż atak (wzmocnienie, obrona, itp.)
    private static bool WrogiWzmacniaSie() =>
        CombatStateTracker.IntencjeWrogow.Values
            .Any(i => !i.JestAtakiem && i.TypPL.Contains("Wzmocnienie"));

    private static bool WrogiUzywaDebuffu() =>
        CombatStateTracker.IntencjeWrogow.Values
            .Any(i => !i.JestAtakiem && i.TypPL.Contains("Osłabienie"));

    private static bool WrogiNieAtakuje() =>
        CombatStateTracker.IntencjeWrogow.Count > 0 &&
        CombatStateTracker.LaczneNadchodzaceObrazenia == 0;

    // ====================== GŁÓWNA ANALIZA ======================

    public static BestPlay AnalizujRuch()
    {
        var wynik = new BestPlay();
        var reka = CombatStateTracker.PobierzReke();
        if (reka.Count == 0) return wynik;

        var (hp, maxHp, blok) = CombatStateTracker.PobierzHPGracza();
        int energia      = CombatStateTracker.PobierzEnergieGracza();
        int nadchodzace  = CombatStateTracker.LaczneNadchodzaceObrazenia;
        bool wrogiAtakuja = nadchodzace > 0;
        bool wrogiWzmacnia = WrogiWzmacniaSie();
        bool wrogiDebuffuje = WrogiUzywaDebuffu();
        bool wrogiNieAtakuje = WrogiNieAtakuje();

        // SURVIVE: obrażenia po bloku >= HP
        if (wrogiAtakuja && hp > 0)
        {
            int efektywne = Math.Max(0, nadchodzace - blok);
            if (efektywne >= hp) wynik.CzySurvive = true;
        }

        // LETHAL: ile dmg możemy zadać z uwzględnieniem budżetu energii i limitów obrony
        var celLethal  = WezNajslabszegoWroga(); // cel = min (HP + blok)
        bool sliskosc   = celLethal != null && CombatStateTracker.CzyMaSliskosc(celLethal);
        bool intangible = celLethal != null && CombatStateTracker.CzyMaIntangible(celLethal);
        int  limitDmg   = celLethal != null ? CombatStateTracker.PobierzLimitDmg(celLethal) : int.MaxValue;
        bool maPancerz  = limitDmg < int.MaxValue;

        int celHP = 0, celBlok = 0;
        if (celLethal != null)
        {
            var (cHP, _, cBlok) = CombatStateTracker.PobierzHPKreature(celLethal);
            celHP = cHP; celBlok = cBlok;
        }

        if (celHP > 0 && !sliskosc && !intangible)
        {
            int potDmg = 0, budzet = energia;
            foreach (var k in reka
                .Where(k => PobierzTypStr(k) == "Attack")
                .OrderBy(k => PobierzKoszt(k)))
            {
                int koszt = Math.Max(0, PobierzKoszt(k));
                if (budzet < koszt) continue;
                budzet -= koszt;
                int dmg = PobierzWartosc(k, "Damage");
                potDmg += maPancerz ? Math.Min(dmg, limitDmg) : dmg;
            }
            // Musi przebić blok + HP (blok pochłania obrażenia przed HP)
            if (potDmg >= celHP + celBlok) wynik.CzyLethal = true;
        }

        // Priorytet kart z pełnym kontekstem
        var analiza = AnalizujReke(wrogiAtakuja, wrogiWzmacnia, wrogiNieAtakuje);
        if (analiza.Count == 0) return wynik;

        // Filtruj: tylko karty na które aktualnie stać (koszt <= energia lub X-cost)
        bool maCorruptionPower = CombatStateTracker.PobierzBuffyGracza().Contains("CorruptionPower");
        analiza = analiza
            .Where(o =>
            {
                int k = PobierzKoszt(o.Karta);
                if (k <= energia || k < 0) return true;
                // Corruption: Skills kosztują 0 (exhaust)
                if (maCorruptionPower && PobierzTypStr(o.Karta) == "Skill") return true;
                return false;
            })
            .ToList();
        if (analiza.Count == 0) return wynik; // brak energii – nic nie sugeruj

        // Synergiczna kolejność: Moc wzmacniająca inne karty idzie PIERWSZA
        analiza = PrzesortujSynergicznie(analiza);

        // Wybierz kartę: SURVIVE bez LETHAL → preferuj blok
        OcenaKarty best;
        if (wynik.CzySurvive && !wynik.CzyLethal)
            best = analiza.FirstOrDefault(o => PobierzWartosc(o.Karta, "Block") > 0) ?? analiza[0];
        else
            best = analiza[0];

        wynik.KartaNazwa = CardEvaluator.PobierzNazwe(best.Karta);

        // Cel: atak → najsłabszy wróg
        string bestTyp = PobierzTypStr(best.Karta);
        if (bestTyp == "Attack")
        {
            var target = WezNajslabszegoWroga();
            if (target != null)
            {
                var (tHp, _, _) = CombatStateTracker.PobierzHPKreature(target);
                wynik.CelNazwa  = $"{CombatHUD.PobierzNazweWroga(target)} ({tHp} HP)";
            }
        }

        // Powód – kontekstowy
        int dmgKarty   = PobierzWartosc(best.Karta, "Damage");
        int blkKarty   = PobierzWartosc(best.Karta, "Block");
        bool maEnergię  = CardEvaluator.CzyMaEnergie(best.Karta);
        bool maDobier   = CzyMaDobieranie(best.Karta);
        bool maSilę     = CardEvaluator.CzyMaSile(best.Karta);

        bool maZrecznosc = CzyMaZrecznosc(best.Karta);

        if (wynik.CzyLethal && bestTyp == "Attack")
            wynik.Powod = L.PoradaLethal;
        else if (wynik.CzySurvive && blkKarty > 0)
            wynik.Powod = L.PoradaBlok(blkKarty, nadchodzace);
        else if (maZrecznosc && analiza.Any(o => PobierzWartosc(o.Karta, "Block") > 0))
            wynik.Powod = L.PoradaZrecznosc;
        else if (maSilę && analiza.Any(o => PobierzWartosc(o.Karta, "Damage") > 0))
            wynik.Powod = L.PoradaSila;
        else if (maEnergię)
            wynik.Powod = L.PoradaEnergia;
        else if (wrogiWzmacnia && bestTyp == "Attack")
            wynik.Powod = L.PoradaWzmocnienie;
        else if (wrogiDebuffuje && bestTyp == "Attack")
            wynik.Powod = L.PoradaDebuff;
        else if (wrogiNieAtakuje && bestTyp == "Attack")
            wynik.Powod = L.PoradaNieAtakuje;
        else if (maDobier)
            wynik.Powod = L.PoradaDobieranie;
        else if (maSilę)
            wynik.Powod = L.PoradaBudujeSize;
        else if (PobierzKoszt(best.Karta) == 0)
            wynik.Powod = L.PoradaDarmowa;
        else if (dmgKarty >= 12)
            wynik.Powod = L.PoradaDuzeObr(dmgKarty);

        // Sekwencja
        wynik.Sekwencja = ZbudujSekwencje(analiza);

        return wynik;
    }

    // Analiza ręki – lista posortowana po priorytecie
    public static List<OcenaKarty> AnalizujReke()
    {
        var reka = CombatStateTracker.PobierzReke();
        if (reka.Count == 0) return new();

        bool wrogiAtakuja    = CombatStateTracker.LaczneNadchodzaceObrazenia > 0;
        bool wrogiWzmacnia   = WrogiWzmacniaSie();
        bool wrogiNieAtakuje = WrogiNieAtakuje();
        return reka
            .Select(k => OcenKarte(k, wrogiAtakuja, wrogiWzmacnia, wrogiNieAtakuje))
            .OrderByDescending(o => o.Priorytet)
            .ToList();
    }

    // Wewnętrzna wersja z pełnym kontekstem
    private static List<OcenaKarty> AnalizujReke(bool wrogiAtakuja, bool wrogiWzmacnia, bool wrogiNieAtakuje)
    {
        var reka = CombatStateTracker.PobierzReke();
        if (reka.Count == 0) return new();
        return reka
            .Select(k => OcenKarte(k, wrogiAtakuja, wrogiWzmacnia, wrogiNieAtakuje))
            .OrderByDescending(o => o.Priorytet)
            .ToList();
    }

    // Prosta porada (fallback)
    public static string GenerujPorade()
    {
        try
        {
            bool wrogiAtakuja = CombatStateTracker.LaczneNadchodzaceObrazenia > 0;
            var analiza = AnalizujReke();
            if (analiza.Count == 0) return "";

            if (wrogiAtakuja)
            {
                var obrona = analiza.FirstOrDefault(o => PobierzWartosc(o.Karta, "Block") > 0);
                if (obrona != null && obrona.Priorytet > 70) return "⚔ Wróg atakuje – zagraj ochronę najpierw!";
            }
            if (analiza.Any(o => PobierzKoszt(o.Karta) == 0)) return "⚡ Zagraj darmowe karty na początku!";
            if (analiza.Any(o => CzyMaDobieranie(o.Karta)))    return "🃏 Dobierz karty żeby zobaczyć więcej opcji!";
            return "";
        }
        catch { return ""; }
    }

    // ====================== OCENA KARTY ======================

    private static OcenaKarty OcenKarte(CardModel karta, bool wrogiAtakuja,
                                         bool wrogiWzmacnia, bool wrogiNieAtakuje)
    {
        var ocena = new OcenaKarty { Karta = karta };

        int koszt      = PobierzKoszt(karta);
        int obrazenia  = PobierzWartosc(karta, "Damage");
        int obrona     = PobierzWartosc(karta, "Block");
        bool maDobier  = CzyMaDobieranie(karta);
        bool maEnergię = CardEvaluator.CzyMaEnergie(karta);
        bool maSilę    = CardEvaluator.CzyMaSile(karta);
        string typStr  = PobierzTypStr(karta);

        // Opis wartości
        var wartosci = new List<string>();
        if (obrazenia  > 0) wartosci.Add($"⚔{obrazenia}");
        if (obrona     > 0) wartosci.Add($"🛡{obrona}");
        if (maDobier)       wartosci.Add("🃏dobierz");
        if (maEnergię)      wartosci.Add("⚡energię");
        if (maSilę)         wartosci.Add("💪siłę");
        if (koszt == 0)     wartosci.Add("darmowa");
        ocena.WartoscStr = wartosci.Count > 0 ? string.Join(" ", wartosci) : typStr;

        // Priorytet bazowy
        ocena.Priorytet = 50;
        var powody = new List<string>();

        // Curse/Status – zagraj na końcu lub wcale
        if (typStr is "Curse" or "Status")
        {
            ocena.Priorytet = -100;
            ocena.Powod = L.OcenaZagrajNaKoncu;
            return ocena;
        }

        // ── Karty specjalne – zawsze wysokie ──
        if (maEnergię)  { ocena.Priorytet += 55; powody.Add(L.OcenaGenerujeEn); }
        if (maDobier)   { ocena.Priorytet += 40; powody.Add(L.OcenaDobieranie); }
        if (maSilę)     { ocena.Priorytet += 35; powody.Add(L.OcenaBudujeSize); }
        if (koszt == 0) { ocena.Priorytet += 38; powody.Add(L.OcenaDarmowa); }

        // ── BLOK: dobry tylko gdy atak nadchodzi ──
        if (wrogiAtakuja && obrona > 0)
        {
            int nadchodzace = CombatStateTracker.LaczneNadchodzaceObrazenia;
            ocena.Priorytet += 35 + (obrona >= nadchodzace ? 20 : 0);
            powody.Add(L.OcenaBlokuje(obrona, nadchodzace));
        }
        else if (wrogiNieAtakuje && obrona > 0 && obrazenia == 0)
        {
            ocena.Priorytet -= 32;
            powody.Add(L.OcenaBlokZbedny);
        }

        // ── ATAK: premiowany gdy wróg nie atakuje lub się wzmacnia ──
        if (obrazenia > 0)
        {
            if (wrogiNieAtakuje)
            {
                ocena.Priorytet += wrogiWzmacnia ? 25 : 18;
                powody.Add(wrogiWzmacnia ? L.OcenaWrogWzmacnia : L.OcenaWrogNieAtk);
            }
            if (obrazenia >= 15) ocena.Priorytet += 15;
            else if (obrazenia >= 8) ocena.Priorytet += 8;
        }

        // ── Moc gdy wróg nie atakuje – dobry moment ──
        if (typStr == "Power" && wrogiNieAtakuje)
        {
            ocena.Priorytet += 15;
            powody.Add(L.OcenaMocWrogNieAtk);
        }

        ocena.CzyPolecana = ocena.Priorytet > 70;
        ocena.Powod = powody.FirstOrDefault() ?? "";
        return ocena;
    }

    // ====================== POMOCNIKI ======================

    private static Creature? WezNajslabszegoWroga()
    {
        Creature? target = null;
        int minEffHP = int.MaxValue;
        foreach (var (_, intent) in CombatStateTracker.IntencjeWrogow)
        {
            if (intent.Owner == null) continue;
            var (wHp, _, wBlok) = CombatStateTracker.PobierzHPKreature(intent.Owner);
            // Efektywne HP = HP + blok: wróg z bloku jest trudniejszy do zabicia tej tury
            int effHP = wHp + wBlok;
            if (wHp > 0 && effHP < minEffHP) { minEffHP = effHP; target = intent.Owner; }
        }
        return target;
    }

    private static int WezMinHPWroga()
    {
        int min = int.MaxValue;
        foreach (var (_, intent) in CombatStateTracker.IntencjeWrogow)
        {
            if (intent.Owner == null) continue;
            var (wHp, _, _) = CombatStateTracker.PobierzHPKreature(intent.Owner);
            if (wHp > 0 && wHp < min) min = wHp;
        }
        return min == int.MaxValue ? 0 : min;
    }

    private static string ZbudujSekwencje(List<OcenaKarty> lista)
    {
        if (lista.Count <= 1) return "";
        var sb = new System.Text.StringBuilder(L.Sekwencja);
        int n = Math.Min(3, lista.Count);
        for (int i = 0; i < n; i++)
        {
            if (i > 0) sb.Append(" → ");
            string nazwa = CardEvaluator.PobierzNazwe(lista[i].Karta);
            sb.Append(nazwa.Length > 11 ? nazwa[..10] + "." : nazwa);
        }
        return sb.ToString();
    }

    // ====================== STATYCZNE HELPERY KART ======================

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

    public static bool CzyMaZrecznosc(CardModel k)
    {
        try
        {
            return k.DynamicVars.ContainsKey("Dexterity") ||
                   k.DynamicVars.ContainsKey("Dex");
        }
        catch { return false; }
    }

    // Reorder: Power cards that buff subsequent cards go first in sequence
    private static List<OcenaKarty> PrzesortujSynergicznie(List<OcenaKarty> lista)
    {
        if (lista.Count <= 1) return lista;
        var result = new List<OcenaKarty>(lista);

        int blokow  = result.Count(o => PobierzWartosc(o.Karta, "Block") > 0);
        int atakow  = result.Count(o => PobierzWartosc(o.Karta, "Damage") > 0);

        // Zręczność (Dexterity) Power → przed kartami bloku (opłacalne gdy ≥1 karta bloku)
        if (blokow >= 1)
        {
            var dex = result.FirstOrDefault(o =>
                PobierzTypStr(o.Karta) == "Power" && CzyMaZrecznosc(o.Karta));
            if (dex != null) { result.Remove(dex); result.Insert(0, dex); }
        }

        // Siła (Strength) Power → przed atakami (opłacalne gdy ≥2 ataki)
        if (atakow >= 2)
        {
            var str = result.FirstOrDefault(o =>
                PobierzTypStr(o.Karta) == "Power" && CardEvaluator.CzyMaSile(o.Karta));
            if (str != null && PobierzTypStr(str.Karta) == "Power")
            { result.Remove(str); result.Insert(0, str); }
        }

        return result;
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

    public static string PobierzTypStr(CardModel k)
    {
        try { return k.Type.ToString(); }
        catch { return "Unknown"; }
    }
}
