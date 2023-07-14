using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PastingMaui.Shared;
// data model for devices
namespace PastingMaui.Data
{
    public interface IBTDevice /*: IDisposable*/
    {
        public static Func<Task> RefreshDevice;

        public static int count;

        public string Id { get; }

        public string Name { get; }

        public string Type { get; }

        public Task<ToastData> Connect(IClient scanner);
        // returns if failiure or success

        public void Disconnect(IClient client);

        public bool IsConnected();

        public bool Pair();
        

    }
}