using MegaCrit.Sts2.Core.Models;
using DoradcaWiezy.Util;

namespace DoradcaWiezy.DeckBuilding;

public static class DeckAnalyzer
{
    // === Typ decku + porada strategiczna ===
    public static (string archetyp, string strategia) AnalizujArchetype(List<CardModel> deck)
    {
        if (deck.Count == 0) return (L.ArchBrakKart, L.ArchBrakKartDesc);

        int ataki   = deck.Count(k => CardEvaluator.PobierzTyp(k) == "Attack");
        int moce    = deck.Count(k => CardEvaluator.PobierzTyp(k) == "Power");
        int total   = deck.Count;
        float propAtk = (float)ataki / total;
        float propMoc = (float)moce / total;

        bool maEnergie    = deck.Any(k => CardEvaluator.CzyMaEnergie(k));
        bool maSile       = deck.Any(k => CardEvaluator.CzyMaSile(k));
        int iloscDobier   = deck.Count(k => CardEvaluator.CzyMaDobieranie(k));
        int iloscWyczerp  = deck.Count(k => CardEvaluator.CzyMaWyczerpanie(k));
        int podstas       = LiczPodstawowe(deck);

        bool maCorruption      = deck.Any(k => CardEvaluator.PobierzIdKarty(k) == "Corruption");
        bool maExhaustSynergię = deck.Any(k => CardEvaluator.PobierzIdKarty(k) is "DarkEmbrace" or "FeelNoPain");
        bool maApparition      = deck.Any(k => CardEvaluator.PobierzIdKarty(k) == "Apparition");
        bool maNostalgia       = deck.Any(k => CardEvaluator.PobierzIdKarty(k) == "Nostalgia");

        if (maCorruption && maExhaustSynergię)
            return (L.ArchExhaust, L.ArchExhaustDesc);

        if (maApparition && maNostalgia)
            return (L.ArchApparition, L.ArchApparitionDesc);

        if (podstas >= 7)
            return (L.ArchStartowy, L.ArchStartowyDesc);

        if (maEnergie && iloscDobier >= 2)
            return (L.ArchCombo, L.ArchComboDesc);

        if (maSile && propAtk > 0.45f)
            return (L.ArchSilowy, L.ArchSilowyDesc);

        if (iloscWyczerp >= 3)
            return (L.ArchWyczerpywania, L.ArchWyczerpowaniaDesc);

        if (propMoc > 0.20f || moce >= 3)
            return (L.ArchMocy, L.ArchMocyDesc);

        if (propAtk > 0.65f)
            return (L.ArchAgresywny, L.ArchAgresywnyDesc);

        return (L.ArchMieszany, L.ArchMieszanyDesc);
    }

    // === Top karty do ulepszenia (posortowane po priorytecie) ===
    public static List<(CardModel karta, string powod)> PriorytetUlepszenia(List<CardModel> deck)
    {
        var wynik = new List<(CardModel karta, string powod, int pkt)>();

        foreach (var k in deck.Where(k => !k.IsUpgraded))
        {
            string typ = CardEvaluator.PobierzTyp(k);
            if (typ is "Curse" or "Status") continue;

            int pkt    = 0;
            string pow = "";

            bool maEnergię  = CardEvaluator.CzyMaEnergie(k);
            bool maDobier   = CardEvaluator.CzyMaDobieranie(k);
            bool maSilę     = CardEvaluator.CzyMaSile(k);
            int koszt       = CardEvaluator.PobierzKoszt(k);
            int dmg         = CardEvaluator.PobierzWartosc(k, "Damage");
            int blk         = CardEvaluator.PobierzWartosc(k, "Block");

            if (maEnergię)             { pkt = 70; pow = L.UP_Energia; }
            else if (maDobier)         { pkt = 60; pow = L.UP_Dobieranie; }
            else if (maSilę)           { pkt = 55; pow = L.UP_Sila; }
            else if (typ == "Power")   { pkt = 50; pow = L.UP_Moc; }
            else if (koszt == 0 && (dmg > 0 || blk > 0))
                                       { pkt = 45; pow = L.UP_Darmowa; }
            else if (koszt == 1 && dmg >= 9)
                                       { pkt = 40; pow = L.UP_TaniAtak; }
            else if (blk >= 8)         { pkt = 35; pow = L.UP_SolidnaOb; }
            else if (dmg >= 12)        { pkt = 30; pow = L.UP_WysokieObr; }
            else                       { pkt =  5; pow = L.UP_Ogolne; }

            wynik.Add((k, pow, pkt));
        }

        return wynik
            .OrderByDescending(x => x.pkt)
            .Select(x => (x.karta, x.powod))
            .ToList();
    }

    // === Karty do rozważenia usunięcia ===
    public static List<(CardModel karta, string powod)> ZalecaniaUsunieciaKart(List<CardModel> deck)
    {
        var wynik = new List<(CardModel karta, string powod, int pkt)>();

        int iloscAtk = 0; int iloscDef = 0;
        foreach (var k in deck)
        {
            string n = CardEvaluator.PobierzNazwe(k).ToLower();
            if (n.Contains("strike") || n.Contains("uderzeni")) iloscAtk++;
            if (n.Contains("defend") || n == "obrona")          iloscDef++;
        }

        foreach (var k in deck)
        {
            string typ   = CardEvaluator.PobierzTyp(k);
            string nazwa = CardEvaluator.PobierzNazwe(k).ToLower();
            int koszt    = CardEvaluator.PobierzKoszt(k);
            int dmg      = CardEvaluator.PobierzWartosc(k, "Damage");
            int blk      = CardEvaluator.PobierzWartosc(k, "Block");

            if (typ == "Curse")
            { wynik.Add((k, L.RM_Przeklenstwo, 100)); continue; }
            if (typ == "Status")
            { wynik.Add((k, L.RM_Status, 80)); continue; }

            bool toAtk = nazwa.Contains("strike") || nazwa.Contains("uderzeni");
            bool toDef = nazwa.Contains("defend") || nazwa == "obrona";

            if (toAtk && !k.IsUpgraded && iloscAtk > 3)
            {
                wynik.Add((k, L.RM_ZaduzoUderzen(iloscAtk, 3), 60));
                iloscAtk--; continue;
            }
            if (toDef && !k.IsUpgraded && iloscDef > 3)
            {
                wynik.Add((k, L.RM_ZaduzoObrona(iloscDef, 3), 55));
                iloscDef--; continue;
            }

            // Drogie i słabe (2+ energii, mało efektu, bez specjalnych właściwości)
            if (!k.IsUpgraded && koszt >= 2 && dmg < 6 && blk < 6
                && !CardEvaluator.CzyMaDobieranie(k)
                && !CardEvaluator.CzyMaEnergie(k)
                && !CardEvaluator.CzyMaSile(k)
                && typ != "Power")
            { wynik.Add((k, L.RM_SlabaDroga, 20)); }
        }

        return wynik
            .OrderByDescending(x => x.pkt)
            .Select(x => (x.karta, x.powod))
            .ToList();
    }

    private static int LiczPodstawowe(List<CardModel> deck)
    {
        int count = 0;
        foreach (var k in deck)
        {
            string n = CardEvaluator.PobierzNazwe(k).ToLower();
            if ((n.Contains("strike") || n.Contains("uderzeni") ||
                 n.Contains("defend") || n == "obrona") && !k.IsUpgraded)
                count++;
        }
        return count;
    }
}
