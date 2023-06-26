using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using AndroidX.Core.App;
using PastingMaui.Data;
using PastingMaui.Platforms;

namespace PastingMaui;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{

    private readonly IPasting _app;
    private readonly int BLUETOOTH_REQUEST_CODE = 218;

    protected override void OnCreate(Bundle savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        IPasting _app = MauiApplication.Current.Services.GetService<IPasting>();
        // do stuff here to activate the services
        Intent intent = new Intent(this, typeof(MainActivity));


    }

    public override void OnRequestPermissionsResult(int code, string[] permissions, Permission[] grantResults)
    {
        base.OnRequestPermissionsResult(code, permissions, grantResults);
        if (code == BLUETOOTH_REQUEST_CODE)
        {
            if (grantResults.Length != 0 && grantResults[0].Equals(Permission.Granted))
            {
#if DEBUG
                Console.WriteLine("Bluetooth Permission Granted");
#endif
            }
        }
    }


}
