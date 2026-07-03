using System.Configuration;
using System.Data;
using System.Windows;

namespace TwoFactorAuth.Win;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        CultureSetup.Apply();
        base.OnStartup(e);
    }
}
