using System.Windows;

namespace TwoFactorAuth.Win;

public partial class PasteUriDialog : Window
{
    public PasteUriDialog()
    {
        InitializeComponent();
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
