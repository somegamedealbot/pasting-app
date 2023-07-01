using Android;
using Android.App;
using Android.Bluetooth;
using Android.Content;
using Android.Content.PM;
using AndroidX.Core.Content;
using PastingMaui.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application = Android.App.Application;

namespace PastingMaui.Platforms
{
    internal class PastingApp : IPasting
    {

        Client appClient;
        Server appServer;

        public PastingApp()
        {

        }

        public PastingApp(Context context)
        {
            // ask for permission here
            //Intent clientIntent = new(Application.Context, typeof(Client));
            //Application.Context.StartActivity(clientIntent);
            //appServer = new Server(context);
            // https://github.com/android/connectivity-samples/issues/263#issuecomment-1100650576
            // use for requsting bluetooth permission
        }

        IServer IPasting.server {
            get
            {
                return appServer;
            }
        }
        IClient IPasting.client {
            get
            {
                return appClient;
            }
        }

        public void StartClient()
        {
            appClient = new Client();


            //Intent clientIntent = new(Application.Context, typeof(Client));
            
            //// prevents multiple instances of the client and required for starting in non-activity class
            //clientIntent.AddFlags(ActivityFlags.SingleTop | ActivityFlags.NewTask);
            
            //Application.Context.StartActivity(clientIntent);
        }

        public void StartServer()
        {

        }
    }
}
