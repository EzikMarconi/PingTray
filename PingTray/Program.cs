using System.Net.NetworkInformation;
using PingTray.Properties;
using System.IO;
using System.Drawing;

namespace PingTray;

static class Program
{
    private static NotifyIcon? _trayIcon;
    private static readonly Ping _ping = new();
    private static AppSettings _settings = AppSettings.Load();

    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        // Загружаем иконки из массива байтов (в ресурсах они хранятся как byte[])
        var greenIcon = new Icon(new MemoryStream(Resources.green));
        var yellowIcon = new Icon(new MemoryStream(Resources.yellow));
        var redIcon = new Icon(new MemoryStream(Resources.red));

        _trayIcon = new NotifyIcon
        {
            Icon = greenIcon,
            Visible = true,
            Text = TrayTitle(_settings)
        };

        var contextMenu = new ContextMenuStrip();
        contextMenu.Items.Add("Настройки…", null, (_, _) => OpenSettings());
        contextMenu.Items.Add(new ToolStripSeparator());
        contextMenu.Items.Add("Выход", null, (_, _) => ExitApplication());
        _trayIcon.ContextMenuStrip = contextMenu;

        _ = PingLoopAsync(greenIcon, yellowIcon, redIcon);

        Application.Run();
    }

    private static string TrayTitle(AppSettings s) => $"Пинг {s.TargetHost}";

    private static void OpenSettings()
    {
        using var form = new SettingsForm(_settings);
        if (form.ShowDialog() == DialogResult.OK && form.ResultSettings != null)
            _settings = form.ResultSettings;
    }

    private static async Task PingLoopAsync(Icon greenIcon, Icon yellowIcon, Icon redIcon)
    {
        while (true)
        {
            var settings = _settings;
            try
            {
                PingReply reply = await _ping.SendPingAsync(settings.TargetHost, settings.PingTimeoutMs);

                if (reply.Status == IPStatus.Success)
                {
                    if (reply.RoundtripTime <= settings.YellowThresholdMs)
                        SetIcon(greenIcon, $"Пинг: {reply.RoundtripTime} мс");
                    else
                        SetIcon(yellowIcon, $"Медленно: {reply.RoundtripTime} мс");
                }
                else
                {
                    SetIcon(redIcon, $"Ошибка: {reply.Status}");
                }
            }
            catch
            {
                SetIcon(redIcon, "Нет сети / ошибка");
            }

            await Task.Delay(settings.PingIntervalMs);
        }
    }

    private static void SetIcon(Icon icon, string tooltip)
    {
        if (_trayIcon == null) return;
        _trayIcon.Icon = icon;
        _trayIcon.Text = tooltip;
    }

    private static void ExitApplication()
    {
        _trayIcon?.Dispose();
        _ping.Dispose();
        Application.ExitThread();
    }
}
