using PastingMaui.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PastingMaui.Platforms
{
    internal class PastingApp : IPasting
    {

        Client appClient;
        Server appServer;

        public static PastingApp app
        {
            get; private set;
        }

        public PastingApp()
        {
            app = this;
            appClient = new Client();
            appServer = new Server();
            StartServer();
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

        public Client client
        {
            get
            {
                return client;
            }
        }

        public Server server
        {
            get
            {
                return server;
            }
        }

        public void StartClient()
        {
            appClient.ScanDevices();
        }

        public void StartScanningDevices()
        {
            appClient.ScanDevices();
        }

        public void StartServer()
        {
            Task.Run(() => appServer.InitServer()).Wait();
        }
    }
}
