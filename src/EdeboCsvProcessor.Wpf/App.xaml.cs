using System.Configuration;
using System.Data;
using System.Windows;

namespace EdeboCsvProcessor.Wpf;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : System.Windows.Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        Velopack.VelopackApp.Build().Run();
        base.OnStartup(e);
    }
}
