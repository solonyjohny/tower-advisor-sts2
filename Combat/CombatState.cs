using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using HarmonyLib;

namespace DoradcaWiezy.Combat;

// Nasz własny tracker stanu walki (oddzielny od klas gry)
public static class CombatStateTracker
{
    // Dane wrogów: hashCode stworzenia -> dane intentu
    public static Dictionary<int, IntentData> IntencjeWrogow { get; } = new();

    // Łączne nadchodzące obrażenia w tej turze
    public static int LaczneNadchodzaceObrazenia { get; private set; }

    // Czy jesteśmy w walce
    public static bool CzyWWalce { get; set; }

    // Aktualizuj po odświeżeniu intentów
    public static void OdswiezIntencje(Creature owner, IntentData dane)
    {
        int id = System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(owner);
        IntencjeWrogow[id] = dane;
        PrzeliczObrazenia();
    }

    public static void UsunKreature(Creature owner)
    {
        IntencjeWrogow.Remove(System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(owner));
        PrzeliczObrazenia();
    }

    public static void Resetuj()
    {
        IntencjeWrogow.Clear();
        LaczneNadchodzaceObrazenia = 0;
        CzyWWalce = false;
    }

    private static void PrzeliczObrazenia()
    {
        LaczneNadchodzaceObrazenia = IntencjeWrogow.Values
            .Where(d => d.JestAtakiem)
            .Sum(d => d.ObrazeniaLaczne);
    }

    // Pobierz HP gracza
    public static (int hp, int maxHp, int blok) PobierzHPGracza()
    {
        try
        {
            var state = CombatManager.Instance.DebugOnlyGetState();
            if (state == null) return (0, 0, 0);
            var players = state.Players;
            if (players == null || players.Count == 0) return (0, 0, 0);
            var creature = players[0].Creature;
            return (creature.CurrentHp, creature.MaxHp, creature.Block);
        }
        catch { return (0, 0, 0); }
    }

    public static (int hp, int maxHp, int blok) PobierzHPKreature(Creature c)
    {
        try { return (c.CurrentHp, c.MaxHp, c.Block); }
        catch { return (0, 0, 0); }
    }

    // Próba odczytu kolekcji powersów stworzenia
    private static System.Collections.IEnumerable? PobierzPowers(Creature c)
    {
        try
        {
            object? p = Traverse.Create(c).Property("Powers").GetValue()
                     ?? Traverse.Create(c).Field("_powers").GetValue()
                     ?? Traverse.Create(c).Field("powers").GetValue();
            return p as System.Collections.IEnumerable;
        }
        catch { return null; }
    }

    // Czy stworzenie ma buff Śliskość (Slippery) – następny atak zada tylko 1 dmg
    public static bool CzyMaSliskosc(Creature c)
    {
        try
        {
            var powers = PobierzPowers(c);
            if (powers == null) return false;
            foreach (var power in powers)
            {
                string n = power.GetType().Name;
                if (n.Contains("Slippery") || n.Contains("Slimy") || n.Contains("Slip"))
                    return true;
            }
        }
        catch { }
        return false;
    }

    // Czy stworzenie ma IntangiblePower (każdy atak zadaje 1 dmg)
    public static bool CzyMaIntangible(Creature c)
    {
        try
        {
            var powers = PobierzPowers(c);
            if (powers == null) return false;
            foreach (var power in powers)
                if (power.GetType().Name == "IntangiblePower") return true;
        }
        catch { }
        return false;
    }

    // Zwraca limit obrażeń na atak (HardenedShellPower = "Wytrzyma prawie wszystko")
    // Jeśli brak limitu → int.MaxValue
    public static int PobierzLimitDmg(Creature c)
    {
        try
        {
            var powers = PobierzPowers(c);
            if (powers == null) return int.MaxValue;
            foreach (var power in powers)
            {
                string n = power.GetType().Name;
                if (n == "HardenedShellPower" || n == "BufferPower")
                {
                    // Próba odczytu wartości limitu z Amount/Value/Cap
                    foreach (var key in new[] { "Amount", "Value", "Cap", "Limit", "DamageCap" })
                    {
                        try
                        {
                            var val = Traverse.Create(power).Property(key).GetValue()
                                   ?? Traverse.Create(power).Field(key).GetValue();
                            if (val is int iv && iv > 0) return iv;
                        }
                        catch { }
                    }
                    return 9; // Fallback dla HardenedShell
                }
            }
        }
        catch { }
        return int.MaxValue;
    }

    // Zbierz aktywne buffy/debuff gracza jako listę nazw klas
    public static List<string> PobierzBuffyGracza()
    {
        var wynik = new List<string>();
        try
        {
            var state = CombatManager.Instance.DebugOnlyGetState();
            if (state?.Players == null || state.Players.Count == 0) return wynik;
            var creature = state.Players[0].Creature;
            var powers = PobierzPowers(creature);
            if (powers == null) return wynik;
            foreach (var power in powers)
                wynik.Add(power.GetType().Name);
        }
        catch { }
        return wynik;
    }

    // Pobierz aktualną energię gracza (0 jeśli brak)
    public static int PobierzEnergieGracza()
    {
        try
        {
            var state = CombatManager.Instance.DebugOnlyGetState();
            if (state?.Players == null || state.Players.Count == 0) return 99;
            var p = state.Players[0];

            // Próba 1-4: różne nazwy właściwości / pól
            foreach (var name in new[] { "CurrentEnergy", "Energy", "CurrentMana", "Mana" })
            {
                try
                {
                    var v = Traverse.Create(p).Property(name).GetValue();
                    if (v is int iv) return iv;
                }
                catch { }
            }
            foreach (var name in new[] { "_currentEnergy", "_energy", "_mana" })
            {
                try
                {
                    var v = Traverse.Create(p).Field(name).GetValue();
                    if (v is int iv) return iv;
                }
                catch { }
            }
            // Próba 5: Creature
            try
            {
                var v = Traverse.Create(p.Creature).Property("Energy").GetValue();
                if (v is int iv) return iv;
            }
            catch { }
        }
        catch { }
        return 99; // fallback: nie filtruj kart
    }

    // Pobierz karty w ręce
    public static List<CardModel> PobierzReke()
    {
        try
        {
            var state = CombatManager.Instance.DebugOnlyGetState();
            if (state == null) return new();
            var players = state.Players;
            if (players == null || players.Count == 0) return new();
            var hand = CardPile.Get(PileType.Hand, players[0]);
            return hand?.Cards?.ToList() ?? new();
        }
        catch { return new(); }
    }

    // Pobierz deck gracza (poza walką)
    public static List<CardModel> PobierzDeck()
    {
        try
        {
            var state = CombatManager.Instance.DebugOnlyGetState();
            if (state != null)
            {
                var players = state.Players;
                if (players != null && players.Count > 0)
                    return players[0].Deck.Cards.ToList();
            }
            // Fallback przez RunManager
            var runState = Traverse.Create(MegaCrit.Sts2.Core.Runs.RunManager.Instance)
                .Property<MegaCrit.Sts2.Core.Runs.RunState>("State").Value;
            if (runState?.Players != null && runState.Players.Count > 0)
                return runState.Players[0].Deck.Cards.ToList();
        }
        catch { }
        return new();
    }
}
