using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PastingMaui.Data
{
    public interface IPasting
    {

        public IServer server
        {
            get;
        }


        public IClient client
        {
            get;
        }

        public void StartServer();

        public void StartClient();

    }
}
