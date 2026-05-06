using System.Net.NetworkInformation;
using PingTray.Properties;
using System.IO;
using System.Drawing;

namespace PingTray;

static class Program
{
    private static NotifyIcon? _trayIcon;
    private static readonly Ping _ping = new();
    private static readonly string _target = "77.88.55.242";

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
            Text = "Пинг 77.88.55.242"
        };

        // Контекстное меню для выхода
        var contextMenu = new ContextMenuStrip();
        contextMenu.Items.Add("Выход", null, (s, e) => ExitApplication());
        _trayIcon.ContextMenuStrip = contextMenu;

        // Запускаем бесконечный цикл пингов
        _ = PingLoopAsync(greenIcon, yellowIcon, redIcon);

        Application.Run();
    }

    private static async Task PingLoopAsync(Icon greenIcon, Icon yellowIcon, Icon redIcon)
    {
        while (true)
        {
            try
            {
                // Отправляем пинг, без using
                PingReply reply = await _ping.SendPingAsync(_target, 3000);

                if (reply.Status == IPStatus.Success)
                {
                    if (reply.RoundtripTime <= 150)
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

            // Небольшая пауза перед следующим пингом
            await Task.Delay(500);
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