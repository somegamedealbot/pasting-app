using Android;
using Android.App;
using Android.Bluetooth;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using Kotlin.Contracts;
using PastingMaui.Data;
using PastingMaui.Platforms;

namespace PastingMaui;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{

    private readonly IPasting _app;
    private readonly int BLUETOOTH_REQUEST_CODE = 218;

    public Context mainContext;

    public static Activity GetMainActivity()
    {
        if (mainActivity == null){
            return null;
        }
        return mainActivity;
    }

    public static Activity mainActivity
    {
        get; private set;
    }

    protected override void OnCreate(Bundle savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        mainActivity = this;
        IPasting _app = MauiApplication.Current.Services.GetService<IPasting>();
        // do stuff here to activate the services
        Intent intent = new Intent(this, typeof(MainActivity));
        mainContext = Platform.AppContext;  // or MauiApplication.Context ???
    }

    public bool CheckBluetoothPermission()
    {
        return ContextCompat.CheckSelfPermission(MauiApplication.Context, Manifest.Permission.BluetoothConnect).Equals(Permission.Granted);
    }

    public void CheckAndReqPermission()
    {
        if (!CheckBluetoothPermission())
        {
            var perms = new string[] {
                    Manifest.Permission.Bluetooth,
                    Manifest.Permission.BluetoothAdmin
                };

            ActivityCompat.RequestPermissions(Platform.CurrentActivity, perms, BLUETOOTH_REQUEST_CODE);
        }
        else
        {
            //scanner.ScanDevices();
            Intent intent = new Intent(this, Java.Lang.Class.FromType(typeof(BTScanner)));
            StartActivityForResult(intent, BTScanner.REQUEST_CODE);
        }
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
                // start the scanning
                
            }
        }
    }

    protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
    {
        base.OnActivityResult(requestCode, resultCode, data);
        if (Intent.Action.Equals(BluetoothAdapter.ActionRequestEnable))
        {
            if (resultCode.Equals(Result.Ok))
            {
                if (requestCode == BTScanner.ENABLE_BLUETOOTH_REQ_CODE)
                {
                    Intent intent = new Intent(this, typeof(BTScanner));
                    intent.SetAction(BTScanner.EnableBluetoothResult);
                    intent.PutExtra("enabled", true);
                    StartService(intent);
                }
                // restart the specific service
            }
        }
    }

}
