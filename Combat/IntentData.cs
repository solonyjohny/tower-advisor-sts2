using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;

namespace DoradcaWiezy.Combat;

// Przechowuje przetworzone dane o intencji wroga
public class IntentData
{
    public Creature? Owner       { get; set; }
    public string    TypPL       { get; set; } = "?";
    public bool      JestAtakiem { get; set; }
    public int       ObrazeniaJednorazowe { get; set; }
    public int       ObrazeniaLaczne { get; set; }
    public int       LiczbaUderzen { get; set; }
    public string    OpisKrotki   { get; set; } = "";

    // Czy bieżący intent grozi poważnymi obrażeniami (>50% HP gracza)
    public bool CzyNiebezpieczny { get; set; }
}

// Słownik tłumaczeń typów intentów
public static class IntentTranslator
{
    public static string Tlumacz(AbstractIntent? intent)
    {
        if (intent == null) return "?";
        return intent.GetType().Name switch
        {
            "AttackIntent"         => "⚔ Atak",
            "SingleAttackIntent"   => "⚔ Atak",
            "MultiAttackIntent"    => "⚔ Atak (wielokrotny)",
            "AttackDebuffIntent"   => "⚔ Atak + osłabienie",
            "AttackDefendIntent"   => "⚔ Atak + obrona",
            "AttackBuffIntent"     => "⚔ Atak + wzmocnienie",
            "BuffIntent"           => "💪 Wzmocnienie",
            "DebuffIntent"         => "☠ Osłabienie",
            "CardDebuffIntent"     => "☠ Osłabienie kart",
            "DefendIntent"         => "🛡 Obrona",
            "DefendBuffIntent"     => "🛡 Obrona + wzmocnienie",
            "DefendDebuffIntent"   => "🛡 Obrona + osłabienie",
            "HealIntent"           => "💚 Leczenie",
            "StatusIntent"         => "📋 Status",
            "SummonIntent"         => "👾 Przywołanie",
            "EscapeIntent"         => "🏃 Ucieczka",
            "SleepIntent"          => "💤 Oczekiwanie",
            "StunIntent"           => "💫 Ogłuszenie",
            "DeathBlowIntent"      => "💀 Cios śmiertelny",
            "HiddenIntent"         => "❓ Ukryte",
            "MagicIntent"          => "✨ Magia",
            "UnknownIntent"        => "❓ Nieznane",
            _                      => TlumaczZNazwy(intent.GetType().Name)
        };
    }

    private static string TlumaczZNazwy(string name)
    {
        if (name.Contains("Attack")) return "⚔ Atak";
        if (name.Contains("Buff"))   return "💪 Wzmocnienie";
        if (name.Contains("Defend")) return "🛡 Obrona";
        if (name.Contains("Debuff")) return "☠ Osłabienie";
        return "❓ " + name.Replace("Intent", "");
    }
}
