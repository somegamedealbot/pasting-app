using PastingMaui.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PastingMaui.Platforms
{
    internal class PastingApp : Application, IPasting
    {

        Client appClient;
        Server appServer;

        public PastingApp()
        {
            appClient = new Client();
            appServer = new Server();
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
    }
}
