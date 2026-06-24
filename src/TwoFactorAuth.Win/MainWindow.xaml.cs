using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Threading;
using TwoFactorAuth.Core.Data;
using TwoFactorAuth.Core.Totp;

namespace TwoFactorAuth.Win;

public partial class MainWindow : Window
{
    private readonly JsonAccountStore _store;
    private readonly ObservableCollection<DispRow> _rows = [];
    private readonly DispatcherTimer _timer;

    public MainWindow()
    {
        InitializeComponent();
        string dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TwoFactorAuth");
        _store = new JsonAccountStore(Path.Combine(dir, "accounts.json"));
        AccountsView.ItemsSource = _rows;
        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
        _timer.Tick += (_, _) => TickRows();
        Loaded += (_, _) =>
        {
            ReloadFromStore();
            _timer.Start();
        };
        Closed += (_, _) => _timer.Stop();
    }

    private void ReloadFromStore()
    {
        _rows.Clear();
        foreach (AccountEntry a in _store.Load())
            _rows.Add(new DispRow(a));
        TickRows();
    }

    private void TickRows()
    {
        foreach (DispRow r in _rows)
            r.Refresh();
    }

    private void BtnManual_OnClick(object sender, RoutedEventArgs e)
    {
        var dlg = new ManualAddDialog { Owner = this };
        if (dlg.ShowDialog() != true)
            return;
        string iss = dlg.IssuerText.Trim();
        string sec = dlg.SecretText.Trim();
        if (string.IsNullOrWhiteSpace(sec))
        {
            MessageBox.Show("密钥不能为空。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        if (!SecretBytes.TryDecodeSecret(sec, out _, out string? err))
        {
            MessageBox.Show(SecretBytes.DescribeDecodeError(err), "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        string label = string.IsNullOrWhiteSpace(iss) ? "Manual" : iss;
        string storedSecret = SecretBytes.NormalizeSeparators(sec);
        _store.Add(new AccountEntry
        {
            Issuer = label,
            AccountName = label,
            SecretBase32 = storedSecret,
            Digits = 6,
            PeriodSeconds = 30
        });
        ReloadFromStore();
    }

    private void BtnPaste_OnClick(object sender, RoutedEventArgs e)
    {
        var dlg = new PasteUriDialog { Owner = this };
        if (dlg.ShowDialog() != true)
            return;
        string raw = dlg.UriText.Trim();
        if (!OtpAuthParser.TryParse(raw, out OtpAuthEntry? ent, out string? err) || ent is null)
        {
            MessageBox.Show(string.IsNullOrEmpty(err) ? "无法解析" : err, "解析失败", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        _store.Add(new AccountEntry
        {
            Issuer = ent.Issuer,
            AccountName = ent.AccountName,
            SecretBase32 = ent.SecretBase32,
            Digits = ent.Digits,
            PeriodSeconds = ent.PeriodSeconds
        });
        ReloadFromStore();
    }

    private void BtnDelete_OnClick(object sender, RoutedEventArgs e)
    {
        if (AccountsView.SelectedItem is not DispRow row)
        {
            MessageBox.Show("请先选中一行。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        if (MessageBox.Show("删除该账号的本地记录？", "确认", MessageBoxButton.OKCancel, MessageBoxImage.Question) != MessageBoxResult.OK)
            return;
        _store.Remove(row.Entry.Id);
        ReloadFromStore();
    }

    private sealed class DispRow : INotifyPropertyChanged
    {
        public DispRow(AccountEntry entry) => Entry = entry;

        public AccountEntry Entry { get; }

        public string Title => string.IsNullOrEmpty(Entry.Issuer)
            ? Entry.AccountName
            : $"{Entry.Issuer} · {Entry.AccountName}";

        private string _code = "";
        public string Code
        {
            get => _code;
            private set { _code = value; OnPropertyChanged(); }
        }

        private string _sec = "";
        public string Sec
        {
            get => _sec;
            private set { _sec = value; OnPropertyChanged(); }
        }

        public void Refresh()
        {
            Code = TotpGenerator.Generate(Entry.SecretBase32, Entry.Digits, Entry.PeriodSeconds);
            Sec = $"{TotpGenerator.SecondsRemaining(Entry.PeriodSeconds)} s";
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
