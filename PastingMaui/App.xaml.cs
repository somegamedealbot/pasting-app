using PastingMaui.Data;

namespace PastingMaui;

public partial class App : Application
{
    //public static IBTScan BTDeviceScanner
    //{
    //	get; private set;
    //}

    //public App(IBTScan scanner)
    //{
    //	InitializeComponent();
    //	MainPage = new MainPage();
    //	BTDeviceScanner = scanner;
    //}

    public App()
    {
        InitializeComponent();
        MainPage = new MainPage();
    }
}
