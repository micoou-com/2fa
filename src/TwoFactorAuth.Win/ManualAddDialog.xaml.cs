using System.Windows;
using System.Windows.Documents;
using TwoFactorAuth.Core.Totp;
using TwoFactorAuth.Win.Localization;

namespace TwoFactorAuth.Win;

public partial class ManualAddDialog : Window
{
    private bool _secretVisible;

    public ManualAddDialog()
    {
        InitializeComponent();
        ApplyLocalization();
    }

    public string IssuerText => TxtIssuer.Text;
    public string SecretText => _secretVisible ? TxtSecret.Text : PwdSecret.Password;

    private void ApplyLocalization()
    {
        Title = UiLoc.T("dlg.manual.title");
        LblIssuer.Text = UiLoc.T("dlg.manual.issuerLabel");
        LblSecret.Inlines.Clear();
        LblSecret.Inlines.Add(new Run(UiLoc.T("dlg.manual.secretLabel")));
        LblSecret.Inlines.Add(new LineBreak());
        LblSecret.Inlines.Add(new Run(UiLoc.T("dlg.manual.secretNote"))
        {
            Foreground = System.Windows.Media.Brushes.Gray,
            FontSize = 11
        });
        BtnToggleSecret.Content = UiLoc.T("dlg.manual.show");
        BtnToggleSecret.ToolTip = UiLoc.T("dlg.manual.toggleTip");
        BtnOk.Content = UiLoc.T("dlg.manual.ok");
        BtnCancel.Content = UiLoc.T("dlg.manual.cancel");
    }

    private void Ok_OnClick(object sender, RoutedEventArgs e)
    {
        string sec = SecretText.Trim();
        if (!SecretBytes.TryDecodeSecret(sec, out _, out string? err))
        {
            MessageBox.Show(SecretBytes.DescribeDecodeError(err), UiLoc.T("msg.secretFormatTitle"),
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        DialogResult = true;
    }

    private void BtnToggleSecret_OnClick(object sender, RoutedEventArgs e)
    {
        _secretVisible = !_secretVisible;
        if (_secretVisible)
        {
            TxtSecret.Text = PwdSecret.Password;
            PwdSecret.Visibility = Visibility.Collapsed;
            TxtSecret.Visibility = Visibility.Visible;
            BtnToggleSecret.Content = UiLoc.T("dlg.manual.hide");
        }
        else
        {
            PwdSecret.Password = TxtSecret.Text;
            TxtSecret.Visibility = Visibility.Collapsed;
            PwdSecret.Visibility = Visibility.Visible;
            BtnToggleSecret.Content = UiLoc.T("dlg.manual.show");
        }
    }
}
