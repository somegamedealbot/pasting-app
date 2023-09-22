using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PastingMaui.Data;
using PastingMaui.Platforms;
using System.Collections.ObjectModel;
using PastingMaui.Shared;
using System.Runtime.CompilerServices;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Storage;

namespace PastingMaui;

public static class MauiProgram
{

	//static ObservableCollection<IBTDevice> discoveredBTDevices = new();

	//public static ObservableCollection<IBTDevice> BTDevices
	//{
	//	get { return discoveredBTDevices; }
	//	set { discoveredBTDevices = value; }
	//}

    //public static IBTScan BTDeviceScanner
    //{
    //    get; private set;
    //}

    public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
			});

		builder.Services.AddMauiBlazorWebView();
        //builder.Services.AddSingleton<IClient, Client>();
        //builder.Services.AddSingleton<IServer, Server>();
        builder.Services.AddSingleton<IToastService, ToastService>();
        builder.Services.AddSingleton<IPasting, PastingApp>();
		builder.Services.AddSingleton<IPasteManager, PasteManager>();

		/*builder.Services.AddSingleton<ToastData()>;*/
        /*builder.Services.AddTransient<IBTScan, BTScanner>();*/

#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
		builder.Logging.AddDebug();
#endif

		builder.Services.AddSingleton<WeatherForecastService>();

		return builder.Build();
	}

//    public static MauiApp CreateMauiApp(Context context)
//    {
//        var builder = MauiApp.CreateBuilder();
//        builder
//            .UseMauiApp<App>()
//            .ConfigureFonts(fonts =>
//            {
//                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
//            });

//        builder.Services.AddMauiBlazorWebView();
//        //builder.Services.AddSingleton<IClient, Client>();
//        //builder.Services.AddSingleton<IServer, Server>();
//        builder.Services.AddSingleton<IPasting, PastingApp>(new Func<IServiceProvider, PastingApp>((provider) =>
//        {
//            return new PastingApp(context);
//        }));
//        builder.Services.AddSingleton<ToastService>();
//        /*builder.Services.AddSingleton<ToastData()>;*/
//        /*builder.Services.AddTransient<IBTScan, BTScanner>();*/

//#if DEBUG
//        builder.Services.AddBlazorWebViewDeveloperTools();
//        builder.Logging.AddDebug();
//#endif

//        builder.Services.AddSingleton<WeatherForecastService>();

//        return builder.Build();
//    }

}
