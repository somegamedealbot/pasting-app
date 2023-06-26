//using Android.OS;
//using Android.Runtime;
//using Java.Interop;
//using PastingMaui.Data;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace PastingMaui.Platforms.Android
//{
//    internal class DeviceList : IParcelable
//    {
//        private bool disposedValue;

//        public List<IBTDevice> devices
//        {
//            get; private set;
//        }

//        public nint Handle => throw new NotImplementedException();

//        public int JniIdentityHashCode => throw new NotImplementedException();

//        public JniObjectReference PeerReference => throw new NotImplementedException();

//        public JniPeerMembers JniPeerMembers => throw new NotImplementedException();

//        public JniManagedPeerStates JniManagedPeerState => throw new NotImplementedException();

//        public DeviceList() { 
//            devices = new List<IBTDevice>();
//        }

//        protected DeviceList(Parcel parcel)
//        {
//            parcel.ReadTypedList(devices,  );
//        }

//        public void AddDevice(IBTDevice device)
//        {
//            devices.Add(device);
//        }

//        public void RemoveDevice(IBTDevice device)
//        {
//            devices.Remove(device);
//        }


//        public int DescribeContents()
//        {
//            return 0;
//        }

//        public void Disposed()
//        {
//            throw new NotImplementedException();
//        }

//        public void DisposeUnlessReferenced()
//        {
//            throw new NotImplementedException();
//        }

//        public void Finalized()
//        {
//            throw new NotImplementedException();
//        }

//        public void SetJniIdentityHashCode(int value)
//        {
//            throw new NotImplementedException();
//        }

//        public void SetJniManagedPeerState(JniManagedPeerStates value)
//        {
//            throw new NotImplementedException();
//        }

//        public void SetPeerReference(JniObjectReference reference)
//        {
//            throw new NotImplementedException();
//        }

//        public void UnregisterFromRuntime()
//        {
//            throw new NotImplementedException();
//        }

//        public void WriteToParcel(Parcel dest, [GeneratedEnum] ParcelableWriteFlags flags)
//        {
//            dest.WriteList(devices);
//        }

//        protected virtual void Dispose(bool disposing)
//        {
//            if (!disposedValue)
//            {
//                if (disposing)
//                {
//                    // TODO: dispose managed state (managed objects)
//                }

//                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
//                // TODO: set large fields to null
//                disposedValue = true;
//            }
//        }

//        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
//        // ~DeviceList()
//        // {
//        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
//        //     Dispose(disposing: false);
//        // }

//        public void Dispose()
//        {
//            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
//            Dispose(disposing: true);
//            GC.SuppressFinalize(this);
//        }
//    }
//}
