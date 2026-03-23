namespace DoradcaWiezy.Util;

// Wszystkie teksty interfejsu – przełączane między PL a EN przez ModSettings.JezykAngielski
public static class L
{
    private static bool EN => ModSettings.JezykAngielski;

    // === Ogólne ===
    public static string ModName    => "Doradca Wieży";
    public static string Wlaczone   => EN ? "Enabled"    : "Włączone";
    public static string Wylaczone  => EN ? "Disabled"   : "Wyłączone";
    public static string Zamknij    => EN ? "Close [ESC]": "Zamknij [ESC]";
    public static string Ustawienia => EN ? "Settings"   : "Ustawienia";

    // === Menu ustawień ===
    public static string MenuTytul           => EN ? "Tower Advisor – Settings"  : "Doradca Wieży – Ustawienia";
    public static string MenuHUD             => EN ? "Combat HUD"                : "HUD walki";
    public static string MenuDoradcaDecku    => EN ? "Deck Advisor"              : "Doradca decku";
    public static string MenuDoradcaSciezki  => EN ? "Path Advisor"              : "Doradca ścieżki";
    public static string MenuDoradcaWydarzen => EN ? "Event Advisor"             : "Doradca wydarzeń";
    public static string MenuPrzezroczystosc => EN ? "Panel transparency"        : "Przezroczystość paneli";
    public static string MenuToggleHotkey    => EN ? "Toggle HUD: [F1]"          : "Przełącz HUD: [F1]";
    public static string MenuMenuHotkey      => EN ? "Settings: [F2]"            : "Ustawienia: [F2]";
    public static string MenuZapisano        => EN ? "Settings saved"            : "Zapisano ustawienia";
    public static string MenuJezyk           => EN ? "Language / Język:"         : "Język / Language:";

    // === HUD walki – etykiety ===
    public static string HudTy             => EN ? "YOU"      : "TY";
    public static string HudWrog           => EN ? "Enemy"    : "Wróg";
    public static string HudLethal         => EN ? "★ YOU CAN KILL THIS TURN — ATTACK!"  : "★ MOŻESZ ZABIĆ TEJ TURY — ATAKUJ!";
    public static string HudSurvive        => EN ? "! DIE WITHOUT BLOCK — BLOCK FIRST!"  : "! ZGINIE BEZ BLOKU — BLOKUJ NAJPIERW!";
    public static string HudZagraj         => EN ? "► PLAY: "                            : "► ZAGRAJ: ";
    public static string HudCel            => EN ? "   TARGET: "                         : "   CEL: ";
    public static string HudZagrajDowolna  => EN ? "Play any card"                       : "Zagraj dowolną kartę";
    public static string HudCorruption     => EN ? "⚗ CORRUPTION: Skills cost 0 (exhaust)!" : "⚗ CORRUPTION: Skille za darmo (exhaust)!";
    public static string HudWrogSliskosc(string n)     => EN ? $"⚗ {n}: SLIPPERY — next attack: 1 dmg!"   : $"⚗ {n}: ŚLISKOŚĆ — następny atak: 1 dmg!";
    public static string HudWrogNiematerialny(string n) => EN ? $"👻 {n}: INTANGIBLE — every attack: 1 dmg!" : $"👻 {n}: NIEMATERIALNY — każdy atak: 1 dmg!";
    public static string HudWrogPancerz(string n, int l) => EN ? $"🛡 {n}: ARMOR — max {l} dmg/attack!"    : $"🛡 {n}: PANCERZ — maks. {l} dmg/atak!";

    // === HUD walki – debuff gracza ===
    public static string DebuffSlaby    => EN ? "Weak(-25%dmg)"       : "Słaby(-25%dmg)";
    public static string DebuffPodatny  => EN ? "Vulnerable(+50%dmg)" : "Podatny(+50%dmg)";
    public static string DebuffWatly    => EN ? "Frail(-25%blk)"      : "Wątły(-25%blok)";
    public static string DebuffDuszon   => EN ? "Constrict"           : "Duszon";
    public static string DebuffTrucizna => EN ? "Poison"              : "Trucizna";
    public static string HudDebuffPrefix => EN ? "⚠ YOU: "            : "⚠ TY: ";

    // === HUD walki – statystyki decku ===
    public static string StatsDeck(int total, int atk, int skl, int pow, string avg)
        => EN ? $"Deck:{total}  Atk:{atk} Skl:{skl} Pow:{pow}  Avg:{avg}"
              : $"Deck:{total}  Atk:{atk} Skl:{skl} Pow:{pow}  Śr.:{avg}";

    // === Poradnik walki (HandAdvisor) ===
    public static string PoradaLethal       => EN ? "You can kill the enemy this turn!"                    : "Możesz dobić wroga tej tury!";
    public static string PoradaBlok(int blk, int total) => EN ? $"Blocks {Math.Min(blk,total)} of {total} damage" : $"Zablokuje {Math.Min(blk,total)} z {total} obrażeń";
    public static string PoradaZrecznosc    => EN ? "Play before blocks — Dexterity boosts every Defense!" : "Zagraj przed blokiem — Zręczność wzmocni każdą Obronę!";
    public static string PoradaSila         => EN ? "Play before attacks — Strength boosts every attack!"  : "Zagraj przed atakami — Siła wzmocni każdy atak!";
    public static string PoradaEnergia      => EN ? "Generates energy – play first!"                       : "Generuje energię – zagraj na początku!";
    public static string PoradaWzmocnienie  => EN ? "Enemy is buffing — attack before effects stack!"      : "Wróg się wzmacnia — atakuj zanim efekty narosną!";
    public static string PoradaDebuff       => EN ? "Enemy wants to debuff — deal damage now!"             : "Wróg chce osłabić — zadaj obrażenia teraz!";
    public static string PoradaNieAtakuje   => EN ? "Enemy not attacking — don't waste energy on block!"   : "Wróg nie atakuje — nie marnuj energii na blok!";
    public static string PoradaDobieranie   => EN ? "Draw cards – more options in hand"                    : "Dobierz karty – więcej opcji w ręce";
    public static string PoradaBudujeSize   => EN ? "Builds Strength – boosts next attacks"                : "Buduje Siłę – wzmocni kolejne ataki";
    public static string PoradaDarmowa      => EN ? "Free card – no energy cost"                           : "Darmowa karta – bez kosztu energii";
    public static string PoradaDuzeObr(int dmg) => EN ? $"High damage ({dmg}) – efficient"                : $"Duże obrażenia ({dmg}) – opłacalne";
    public static string Sekwencja          => EN ? "Seq: " : "Kol: ";

    // === Ocena kart w HandAdvisor ===
    public static string OcenaBlokuje(int blk, int nadch) => EN ? $"Blocks {Math.Min(blk,nadch)} dmg"       : $"Blokuje {Math.Min(blk,nadch)} obrażeń";
    public static string OcenaWrogWzmacnia  => EN ? "Stop enemy buff!"                                      : "Przerwij wzmocnienie!";
    public static string OcenaWrogNieAtk    => EN ? "Enemy not attacking — attack!"                         : "Wróg nie atakuje — atakuj!";
    public static string OcenaBlokZbedny    => EN ? "Block wasted – enemy not attacking"                    : "Blok zbędny – wróg nie atakuje";
    public static string OcenaMocWrogNieAtk => EN ? "Enemy not attacking — good time for Power"             : "Wróg nie atakuje — dobry moment na Moc";
    public static string OcenaDarmowa       => EN ? "Free card"                                             : "Darmowa";
    public static string OcenaGenerujeEn    => EN ? "Generates energy"                                      : "Generuje energię";
    public static string OcenaDobieranie    => EN ? "Card draw"                                             : "Dobieranie";
    public static string OcenaBudujeSize    => EN ? "Builds Strength"                                       : "Buduje Siłę";
    public static string OcenaZagrajNaKoncu => EN ? "Play last"                                             : "Zagraj na końcu";

    // === Intencje wrogów ===
    public static string NaglowekWrogowie  => EN ? "ENEMIES"    : "WROGOWIE";
    public static string NaglowekReka      => EN ? "YOUR HAND"  : "TWOJA RĘKA";
    public static string NaglowekPorada    => EN ? "ADVICE"     : "PORADA";
    public static string Tura              => EN ? "Turn"       : "Tura";
    public static string TwojaTura         => EN ? "Your turn"  : "Twoja tura";
    public static string TuraWroga         => EN ? "Enemy turn" : "Tura wroga";
    public static string IntencjaAtak        => EN ? "Attack"   : "Atak";
    public static string IntencjaWzmocnienie => EN ? "Buff"     : "Wzmocnienie";
    public static string IntencjaOslabienie  => EN ? "Debuff"   : "Osłabienie";
    public static string IntencjaObrona      => EN ? "Defend"   : "Obrona";
    public static string IntencjaPrzywolanie => EN ? "Summon"   : "Przywołanie";
    public static string IntencjaUnknown     => EN ? "Unknown"  : "Nieznane";
    public static string IntencjaUcieczka    => EN ? "Escape"   : "Ucieczka";
    public static string IntencjaSen         => EN ? "Wait"     : "Oczekiwanie";
    public static string Razem               => EN ? "Total"    : "Łącznie";
    public static string RazyCios            => EN ? "× {0} hits": "× {0} ciosów";

    // === Statusy ===
    public static string StatusTruciznaPrefiks => EN ? "Poison"      : "Trucizna";
    public static string StatusWrazliwy        => EN ? "Vulnerable"  : "Podatny";
    public static string StatusSlaby           => EN ? "Weak"        : "Słaby";
    public static string StatusZablokowany     => EN ? "Blocked"     : "Zablokowany";

    // === Sugestie kart ===
    public static string ZagrajNajpierw     => EN ? "Play first"   : "Zagraj najpierw";
    public static string KartaBloku         => EN ? "block card"   : "karta obrony";
    public static string KartaAtaku         => EN ? "attack card"  : "karta ataku";
    public static string KartaMagii         => EN ? "skill card"   : "karta umiejętności";
    public static string KartaMocy          => EN ? "power card"   : "karta mocy";
    public static string KartaBezplatna     => EN ? "free card"    : "bezpłatna";
    public static string KartaCiagniecia    => EN ? "card draw"    : "dobieranie";
    public static string KartaZla           => EN ? "curse card"   : "karta przekleństwa";
    public static string ObroneNaPierwszosc => EN ? "⚔ Enemy attacks – play defense first!" : "⚔ Wróg atakuje – zagraj ochronę najpierw!";
    public static string ZagrajDarmowe      => EN ? "⚡ Play free cards first!"               : "⚡ Zagraj darmowe karty na początku!";
    public static string ZagrajCiagniecia   => EN ? "🃏 Draw cards to see more options!"      : "🃏 Dobierz karty żeby zobaczyć więcej opcji!";

    // === Obliczenia obrażeń ===
    public static string Obrazenia       => EN ? "Damage"                : "Obrażenia";
    public static string Ochrona         => EN ? "Protection"            : "Ochrona";
    public static string PrzewObrazenia  => EN ? "Expected dmg"          : "Przewidywane obrażenia";
    public static string ZagrożenieHP    => EN ? "⚠ HP threat!"          : "⚠ Zagrożenie HP!";
    public static string BezpiecznaObrona => EN ? "✓ Sufficient protection" : "✓ Wystarczająca ochrona";
    public static string ObrazeniaWroga(int dmg) => EN ? $"Enemy will deal: {dmg} dmg" : $"Wróg zada: {dmg} obrażeń";
    public static string TwojaOchrona(int blk)   => EN ? $"Your protection: {blk}"     : $"Twoja ochrona: {blk}";

    // === Doradca decku – ocena kart (CardEvaluator) ===
    public static string OcenaKarty      => EN ? "Card rating"          : "Ocena karty";
    public static string DoskonalaKarta  => EN ? "★★★ Excellent pick!"  : "★★★ Doskonały wybór!";
    public static string DobraKarta      => EN ? "★★  Good card"        : "★★  Dobra karta";
    public static string SrednaKarta     => EN ? "★    Average"         : "★    Przeciętna";
    public static string SlabaKarta      => EN ? "✗    Skip it"         : "✗    Słaby wybór";
    // CardEvaluator powody
    public static string CE_Rzadka        => EN ? "Rare card"                       : "Rzadka karta";
    public static string CE_MocTrwaly     => EN ? "Power – permanent effect"        : "Moc – efekt trwały";
    public static string CE_Bezplatna     => EN ? "Free – always efficient"         : "Bezpłatna – zawsze opłacalna";
    public static string CE_Dobieranie    => EN ? "Card draw – fuels deck"          : "Dobiera karty – napędza deck";
    public static string CE_Energia       => EN ? "Generates energy!"               : "Generuje energię!";
    public static string CE_Sila          => EN ? "Gives Strength (scales attacks)" : "Daje Siłę (skaluje ataki)";
    public static string CE_AtkWybitny    => EN ? "Excellent attack (18+ dmg/energy)": "Wybitny atak (18+ dmg/energię)";
    public static string CE_AtkMocny      => EN ? "Strong attack (12+ dmg/energy)" : "Mocny atak (12+ dmg/energię)";
    public static string CE_DarmowyAtk    => EN ? "Free attack"                     : "Darmowy atak";
    public static string CE_SolidnaObrona => EN ? "Solid defense"                   : "Solidna obrona";
    public static string CE_BrakAtakow    => EN ? "Deck urgently needs attacks"     : "Deck pilnie potrzebuje ataków";
    public static string CE_BrakObrony    => EN ? "Deck needs defense"              : "Deck potrzebuje obrony";
    public static string CE_SynergiaAtaki => EN ? "Synergy with attacks in deck"    : "Synergia z atakami w decku";
    public static string CE_WzmacniaAtaki => EN ? "Boosts attack-heavy deck"        : "Wzmacnia deck nastawiony na ataki";
    public static string CE_MalyDeck      => EN ? "Small deck – good time for quality cards" : "Mały deck – dobry czas na wartościowe karty";
    public static string CE_DuzyDeck      => EN ? "Deck already large"              : "Deck już duży";
    public static string CE_Przeklenstwo  => EN ? "Curse – do not take!"            : "Przekleństwo – nie brać!";
    public static string CE_ExhaustSynergy  => EN ? "★ Synergy with Corruption (exhaust engine)!" : "★ Synergia z Corruption (exhaust engine)!";
    public static string CE_ExhaustAktywuje => EN ? "★ Activates exhaust engine!"  : "★ Aktywuje exhaust engine!";
    public static string CE_CorruptionSkill => EN ? "Skill costs 0 with Corruption" : "Skill kosztuje 0 z Corruption";

    // === Doradca ulepszeń ===
    public static string NaglowekUlepszenia         => EN ? "WHAT TO UPGRADE?"              : "CO ULEPSZYĆ?";
    public static string UlepszPierwsza(string name) => EN ? $"Upgrade: {name}"             : $"Ulepsz: {name}";
    public static string UlepszPrzycisk(string name) => EN ? $"Upgrade: {name}"             : $"Ulepsz: {name}";
    public static string WszystkieUlepszone          => EN ? "All key cards upgraded!"       : "Wszystkie kluczowe karty ulepszone!";
    public static string RozwazUsuniecie             => EN ? "CONSIDER REMOVING:"            : "ROZWAŻ USUNIĘCIE:";
    public static string PowodNajczesciej   => EN ? "Most often played"           : "Najczęściej używana";
    public static string PowodKluczowa      => EN ? "Key for strategy"            : "Kluczowa dla strategii";
    public static string PowodDuzyZysk      => EN ? "High upgrade value"          : "Duży zysk z ulepszenia";
    // DeckAnalyzer – upgrade priority reasons
    public static string UP_Energia    => EN ? "Energy – upgrade priority!"       : "Energia – ulepsz priorytetowo!";
    public static string UP_Dobieranie => EN ? "Card draw – turbocharges deck"    : "Dobieranie – turbonapędza deck";
    public static string UP_Sila       => EN ? "Strength – more power for attacks": "Siła – więcej mocy dla ataków";
    public static string UP_Moc        => EN ? "Power – permanent, every round"   : "Moc – trwały efekt, każda runda";
    public static string UP_Darmowa    => EN ? "Free – gain without energy cost"  : "Darmowa – zysk bez kosztu energii";
    public static string UP_TaniAtak   => EN ? "Cheap strong attack – more damage": "Tani mocny atak – więcej obrażeń";
    public static string UP_SolidnaOb  => EN ? "Solid defense – more block"       : "Solidna obrona – więcej bloku";
    public static string UP_WysokieObr => EN ? "High damage"                      : "Wysokie obrażenia";
    public static string UP_Ogolne     => EN ? "Upgrade improves effect"          : "Ulepszenie poprawi efekt";
    // DeckAnalyzer – removal reasons
    public static string RM_Przeklenstwo                  => EN ? "Curse – remove immediately!"  : "Przekleństwo – usuń natychmiast!";
    public static string RM_Status                        => EN ? "Status – remove when possible": "Status – usuń przy okazji";
    public static string RM_ZaduzoUderzen(int n, int max) => EN ? $"You have {n}× Strike – keep max {max}" : $"Masz {n}× Uderzenie – zatrzymaj max {max}";
    public static string RM_ZaduzoObrona(int n, int max)  => EN ? $"You have {n}× Defend – keep max {max}" : $"Masz {n}× Obrona – zatrzymaj max {max}";
    public static string RM_SlabaDroga                    => EN ? "Expensive and weak card"       : "Kosztowna i słaba karta";

    // === Archetypy decku (DeckAnalyzer) ===
    public static string ArchExhaust         => EN ? "Exhaust Engine"     : "Exhaust Engine";
    public static string ArchExhaustDesc     => EN ? "Corruption → Skills cost 0 (exhaust). DarkEmbrace/FeelNoPain give bonuses per exhaust."
                                                    : "Corruption → Skille za 0 (exhaust). DarkEmbrace/FeelNoPain dają premie za każdy exhaust.";
    public static string ArchApparition      => EN ? "Apparition Stall"   : "Apparition Stall";
    public static string ArchApparitionDesc  => EN ? "Apparition + Nostalgia – stall, stack block. Apparition returns every act!"
                                                    : "Zjawa + Nostalgia – opóźniaj, kumuluj blok. Zjawa wraca każdy akt!";
    public static string ArchStartowy        => EN ? "Starter Deck"       : "Deck Startowy";
    public static string ArchStartowyDesc    => EN ? "Too many basic cards. Priority: remove Strike/Defend, take stronger cards."
                                                    : "Za dużo podstawowych kart. Priorytet: usuń Strike/Defend, weź mocniejsze karty.";
    public static string ArchCombo           => EN ? "Combo Deck"         : "Deck Combo";
    public static string ArchComboDesc       => EN ? "Strategy: generate energy and draw cards. Look for: Apotheosis, cycle cards."
                                                    : "Strategia: generuj energię i dobieraj karty. Szukaj: Apokalipsy, kart cyku.";
    public static string ArchSilowy          => EN ? "Strength Deck"      : "Deck Siłowy";
    public static string ArchSilowyDesc      => EN ? "Strategy: build Strength, then attack. Key: 1-2 Strength cards + strong attacks."
                                                    : "Strategia: zbuduj Siłę, potem atakuj. Kluczowe: 1-2 karty Siły + mocne ataki.";
    public static string ArchWyczerpywania   => EN ? "Exhaust Deck"       : "Deck Wyczerpywania";
    public static string ArchWyczerpowaniaDesc => EN ? "Strategy: purge weak cards, keep only the strong. Find synergies."
                                                     : "Strategia: wyczyść deck ze słabych kart, zostaw tylko mocne. Szukaj synergii.";
    public static string ArchMocy            => EN ? "Power Deck"         : "Deck Mocy (Power)";
    public static string ArchMocyDesc        => EN ? "Strategy: play Powers fast, survive 2-3 rounds, then dominate."
                                                    : "Strategia: wystaw Moce szybko, przeżyj 2-3 rundy, potem dominuj.";
    public static string ArchAgresywny       => EN ? "Aggro Deck"         : "Deck Agresywny";
    public static string ArchAgresywnyDesc   => EN ? "Strategy: attack fast. Watch out for bosses – add more block/defense."
                                                    : "Strategia: atakuj szybko. Uwaga na szefów – dodaj więcej bloku/obrony.";
    public static string ArchMieszany        => EN ? "Mixed Deck"         : "Deck Mieszany";
    public static string ArchMieszanyDesc    => EN ? "Decide on a style: Strength, Combo or Aggro. Collect cards for your chosen strategy."
                                                    : "Zdecyduj na styl: Siła, Combo lub Agresja. Zbieraj karty do wybranej strategii.";
    public static string ArchBrakKart        => EN ? "No cards"           : "Brak kart";
    public static string ArchBrakKartDesc    => EN ? "Collect cards"      : "Zbierz karty";

    // === Doradca ścieżki ===
    public static string NaglowekSciezka           => EN ? "PATH ANALYSIS"               : "ANALIZA ŚCIEŻKI";
    public static string SciezkaWalki              => EN ? "Fights"                      : "Walki";
    public static string SciezkaElity              => EN ? "Elites"                      : "Elity";
    public static string SciezkaSklepu             => EN ? "Shops"                       : "Sklepy";
    public static string SciezkaOdpoczynku         => EN ? "Rests"                       : "Odpoczynki";
    public static string SciezkaWydarzenia         => EN ? "Events"                      : "Wydarzenia";
    public static string SciezkaJef                => EN ? "Boss"                        : "Szef";
    public static string SciezkaRekomenacja        => EN ? "Recommendation"              : "Rekomendacja";
    public static string SciezkaPorada_MaloHP      => EN ? "Low HP – find a rest site"   : "Mało HP – szukaj miejsca odpoczynku";
    public static string SciezkaPorada_DuzoZlota   => EN ? "Lots of gold – visit shop"   : "Dużo złota – idź do sklepu";
    public static string SciezkaPorada_NeedUpgrade => EN ? "Deck needs upgrades – find campfire" : "Deck wymaga ulepszeń – szukaj ogniska";
    public static string SciezkaPorada_EliteOK     => EN ? "✓ Ready for elite"           : "✓ Jesteś gotowy na elitę";
    public static string SciezkaPorada_EliteRyzyko => EN ? "⚠ Elite may be risky"        : "⚠ Elita może być ryzykowna";

    // === Doradca wydarzeń ===
    public static string NaglowekWydarzenia => EN ? "EVENT ADVISOR"           : "DORADCA WYDARZEŃ";
    public static string ZalecanaBrak       => EN ? "No clear recommendation" : "Brak wyraźnej rekomendacji";
    public static string Zalecana           => EN ? "✓ Recommended"          : "✓ Zalecane";

    // === HP / Statystyki ===
    public static string HP(int cur, int max)      => $"{cur}/{max} HP";
    public static string Blok(int blk)             => blk > 0 ? $"🛡{blk}" : "";
    public static string Energia(int cur, int max) => $"⚡{cur}/{max}";
}
