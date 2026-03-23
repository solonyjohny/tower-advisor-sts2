using Godot;

namespace DoradcaWiezy.Util;

public static class ModLog
{
    // Leniwa inicjalizacja – NIE wywołujemy OS.GetUserDataDir() przy ładowaniu klasy
    private static string? _logPath;
    private static string LogPath
    {
        get
        {
            if (_logPath != null) return _logPath;
            try
            {
                _logPath = System.IO.Path.Combine(OS.GetUserDataDir(), "DoradcaWiezy.log");
            }
            catch
            {
                // Fallback gdy Godot API jeszcze niedostępne
                _logPath = System.IO.Path.Combine(
                    System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData),
                    "SlayTheSpire2", "DoradcaWiezy.log");
            }
            return _logPath;
        }
    }

    public static void Info(string msg)
    {
        var line = $"[{DateTime.Now:HH:mm:ss}] [INFO] {msg}";
        System.IO.File.AppendAllText(LogPath, line + "\n");
        Main.Log?.Info(msg);
    }

    public static void Warn(string msg)
    {
        var line = $"[{DateTime.Now:HH:mm:ss}] [WARN] {msg}";
        System.IO.File.AppendAllText(LogPath, line + "\n");
        Main.Log?.Warn(msg);
    }

    public static void Error(string context, Exception ex)
    {
        var line = $"[{DateTime.Now:HH:mm:ss}] [ERR] {context}: {ex.Message}";
        System.IO.File.AppendAllText(LogPath, line + "\n");
        System.IO.File.AppendAllText(LogPath, ex.StackTrace + "\n");
        Main.Log?.Error(context + ": " + ex.Message);
    }

    public static void Clear()
    {
        try { System.IO.File.WriteAllText(LogPath, $"=== {L.ModName} Log - {DateTime.Now} ===\n"); }
        catch { }
    }
}
