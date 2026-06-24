using System.Windows;

namespace TwoFactorAuth.Win;

public partial class ManualAddDialog : Window
{
    public ManualAddDialog()
    {
        InitializeComponent();
    }

    public string IssuerText => TxtIssuer.Text;
    public string SecretText => TxtSecret.Text;

    private void Ok_OnClick(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
    }
}
