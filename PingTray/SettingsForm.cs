namespace PingTray;

internal sealed class SettingsForm : Form
{
    private readonly TextBox _host = new();
    private readonly NumericUpDown _yellowMs = new();
    private readonly NumericUpDown _timeoutMs = new();
    private readonly NumericUpDown _intervalMs = new();
    private readonly Button _ok = new();
    private readonly Button _cancel = new();

    public AppSettings? ResultSettings { get; private set; }

    public SettingsForm(AppSettings current)
    {
        Text = "Настройки PingTray";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterScreen;
        AutoScaleMode = AutoScaleMode.Font;
        MinimumSize = new Size(480, 0);
        ClientSize = new Size(520, 230);
        Padding = new Padding(12);

        var table = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 2,
            RowCount = 4
        };
        // Первая колонка по ширине самой длинной подписи — иначе русский текст обрезается и ломает строку «Интервал…».
        table.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        for (var i = 0; i < 4; i++)
            table.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        void AddRow(int row, string labelText, Control editor)
        {
            var label = new Label
            {
                Text = labelText,
                AutoSize = true,
                Anchor = AnchorStyles.Left | AnchorStyles.Top,
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = new Padding(0, 0, 12, 4)
            };
            editor.Dock = DockStyle.Fill;
            editor.Margin = new Padding(0, 0, 0, 4);
            table.Controls.Add(label, 0, row);
            table.Controls.Add(editor, 1, row);
        }

        _host.Text = current.TargetHost;

        _yellowMs.Minimum = 1;
        _yellowMs.Maximum = 60_000;
        _yellowMs.Value = current.YellowThresholdMs;

        _timeoutMs.Minimum = 100;
        _timeoutMs.Maximum = 120_000;
        _timeoutMs.Increment = 100;
        _timeoutMs.Value = current.PingTimeoutMs;

        _intervalMs.Minimum = 50;
        _intervalMs.Maximum = 60_000;
        _intervalMs.Increment = 50;
        _intervalMs.Value = current.PingIntervalMs;

        AddRow(0, "Целевой хост (IP или имя):", _host);
        AddRow(1, "Порог «жёлтой» зоны, мс:", _yellowMs);
        AddRow(2, "Таймаут ping, мс:", _timeoutMs);
        AddRow(3, "Интервал между ping, мс:", _intervalMs);

        const int buttonHeight = 36;
        const int buttonWidth = 104;
        _ok.Text = "OK";
        _ok.DialogResult = DialogResult.OK;
        _ok.AutoSize = false;
        _ok.Size = new Size(buttonWidth, buttonHeight);
        _cancel.Text = "Отмена";
        _cancel.DialogResult = DialogResult.Cancel;
        _cancel.AutoSize = false;
        _cancel.Size = new Size(buttonWidth, buttonHeight);

        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false,
            AutoSize = false,
            Padding = new Padding(0, 10, 0, 2)
        };
        buttons.Controls.Add(_ok);
        buttons.Controls.Add(_cancel);

        var bottom = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = buttonHeight + buttons.Padding.Vertical + 4,
            Padding = new Padding(0)
        };
        bottom.Controls.Add(buttons);

        Controls.Add(bottom);
        Controls.Add(table);

        AcceptButton = _ok;
        CancelButton = _cancel;

        Shown += (_, _) => _host.Select();
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (DialogResult == DialogResult.OK)
        {
            var next = new AppSettings
            {
                TargetHost = _host.Text.Trim(),
                YellowThresholdMs = (int)_yellowMs.Value,
                PingTimeoutMs = (int)_timeoutMs.Value,
                PingIntervalMs = (int)_intervalMs.Value
            };
            next.Save();
            ResultSettings = next;
        }

        base.OnFormClosing(e);
    }
}
