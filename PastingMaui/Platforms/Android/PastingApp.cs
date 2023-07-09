using Android;
using Android.App;
using Android.Bluetooth;
using Android.Content;
using Android.Content.PM;
using AndroidX.Core.Content;
using PastingMaui.Data;
using PastingMaui.Shared;
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

        public Client appClient;
        private ToastService _toast_service;
        public Server appServer;
        public static PastingApp app
        {
            get; private set;
        }

        public PastingApp()
        {
            app = this;
            appClient = new Client();
            // appServer = new Server();
        }

        //public PastingApp(ToastService service)
        //{
        //    _toast_service = service;
        //    app = this;
        //    appClient = new Client();
        //    // appServer = new Server();
        //}

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


            //Intent clientIntent = new(Application.Context, typeof(Client));
            
            //// prevents multiple instances of the client and required for starting in non-activity class
            //clientIntent.AddFlags(ActivityFlags.SingleTop | ActivityFlags.NewTask);
            
            //Application.Context.StartActivity(clientIntent);
        }

        public void StartServer()
        {

        }

        public static PastingApp getApp()
        {
            return app ?? null;
        }

        public void StartScanningDevices()
        {
            appClient.ScanDevices();
        }
    }
}
