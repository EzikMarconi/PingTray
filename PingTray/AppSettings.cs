using System.Text.Json;

namespace PingTray;

internal sealed class AppSettings
{
    public const string DefaultTargetHost = "77.88.55.242";
    public const int DefaultYellowThresholdMs = 150;
    public const int DefaultPingTimeoutMs = 3000;
    public const int DefaultPingIntervalMs = 500;

    public string TargetHost { get; set; } = DefaultTargetHost;

    public int YellowThresholdMs { get; set; } = DefaultYellowThresholdMs;

    public int PingTimeoutMs { get; set; } = DefaultPingTimeoutMs;

    public int PingIntervalMs { get; set; } = DefaultPingIntervalMs;

    private static string SettingsPath =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PingTray", "settings.json");

    public static AppSettings Load()
    {
        try
        {
            var path = SettingsPath;
            if (!File.Exists(path))
                return new AppSettings();

            var json = File.ReadAllText(path);
            var loaded = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions);
            return Normalize(loaded ?? new AppSettings());
        }
        catch
        {
            return new AppSettings();
        }
    }

    public void Save()
    {
        var dir = Path.GetDirectoryName(SettingsPath);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);

        var normalized = Normalize(this);
        TargetHost = normalized.TargetHost;
        YellowThresholdMs = normalized.YellowThresholdMs;
        PingTimeoutMs = normalized.PingTimeoutMs;
        PingIntervalMs = normalized.PingIntervalMs;

        File.WriteAllText(SettingsPath, JsonSerializer.Serialize(this, JsonOptions));
    }

    private static AppSettings Normalize(AppSettings s)
    {
        var host = (s.TargetHost ?? string.Empty).Trim();
        if (host.Length == 0)
            host = DefaultTargetHost;

        var yellow = Math.Clamp(s.YellowThresholdMs, 1, 60_000);
        var timeout = Math.Clamp(s.PingTimeoutMs, 100, 120_000);
        var interval = Math.Clamp(s.PingIntervalMs, 50, 60_000);

        return new AppSettings
        {
            TargetHost = host,
            YellowThresholdMs = yellow,
            PingTimeoutMs = timeout,
            PingIntervalMs = interval
        };
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
}
