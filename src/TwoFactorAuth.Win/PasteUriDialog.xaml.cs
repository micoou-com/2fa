using System.Windows;
using TwoFactorAuth.Win.Localization;

namespace TwoFactorAuth.Win;

public partial class PasteUriDialog : Window
{
    public PasteUriDialog()
    {
        InitializeComponent();
        Title = UiLoc.T("dlg.paste.title");
        LblHint.Text = UiLoc.T("dlg.paste.hint");
        BtnOk.Content = UiLoc.T("dlg.manual.ok");
        BtnCancel.Content = UiLoc.T("dlg.manual.cancel");
        try
        {
            if (Clipboard.ContainsText())
                TxtUri.Text = Clipboard.GetText();
        }
        catch
        {
            // ignore clipboard errors
        }
    }

    public string UriText => TxtUri.Text;

    private void Ok_OnClick(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
    }
}
