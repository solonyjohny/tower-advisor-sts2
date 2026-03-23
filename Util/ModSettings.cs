using Godot;
using System.Text.Json;

namespace DoradcaWiezy.Util;

public static class ModSettings
{
    // === Ustawienia ===
    public static bool HudWalki        { get; set; } = true;
    public static bool DoradcaDecku    { get; set; } = true;
    public static bool DoradcaSciezki  { get; set; } = true;
    public static bool DoradcaWydarzen { get; set; } = true;
    public static float Przezroczystosc { get; set; } = 0.92f;
    // false = Polski, true = English (na przyszłość)
    public static bool JezykAngielski  { get; set; } = false;

    private static string SettingsPath
    {
        get
        {
            try { return System.IO.Path.Combine(OS.GetUserDataDir(), "DoradcaWiezy_settings.json"); }
            catch
            {
                return System.IO.Path.Combine(
                    System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData),
                    "SlayTheSpire2", "DoradcaWiezy_settings.json");
            }
        }
    }

    public static void Load()
    {
        try
        {
            if (!System.IO.File.Exists(SettingsPath)) return;
            var json = System.IO.File.ReadAllText(SettingsPath);
            var dict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);
            if (dict == null) return;

            if (dict.TryGetValue("hud_walki",         out var v1)) HudWalki         = v1.GetBoolean();
            if (dict.TryGetValue("doradca_decku",     out var v2)) DoradcaDecku     = v2.GetBoolean();
            if (dict.TryGetValue("doradca_sciezki",   out var v3)) DoradcaSciezki   = v3.GetBoolean();
            if (dict.TryGetValue("doradca_wydarzen",  out var v4)) DoradcaWydarzen  = v4.GetBoolean();
            if (dict.TryGetValue("przezroczystosc",   out var v5)) Przezroczystosc  = v5.GetSingle();

            ModLog.Info("Ustawienia załadowane.");
        }
        catch (Exception ex) { ModLog.Error("ModSettings.Load", ex); }
    }

    public static void Save()
    {
        try
        {
            var dict = new Dictionary<string, object>
            {
                ["hud_walki"]         = HudWalki,
                ["doradca_decku"]     = DoradcaDecku,
                ["doradca_sciezki"]   = DoradcaSciezki,
                ["doradca_wydarzen"]  = DoradcaWydarzen,
                ["przezroczystosc"]   = Przezroczystosc,
            };
            System.IO.File.WriteAllText(SettingsPath, JsonSerializer.Serialize(dict,
                new JsonSerializerOptions { WriteIndented = true }));
        }
        catch (Exception ex) { ModLog.Error("ModSettings.Save", ex); }
    }
}
