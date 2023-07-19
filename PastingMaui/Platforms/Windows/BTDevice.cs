using System.ComponentModel;
using Windows.Devices.Enumeration;
using PastingMaui.Data;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Rfcomm;
using PastingMaui.Shared;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace PastingMaui.Platforms
{
    internal class BTDevice : IBTDevice /*INotifyPropertyChanged*/
        // todo: rename BTDevice to DiscoveredDevice
    {

        // add static delegate for the event of a property 
        //public static PropertyChangedEventHandler UpdateUi = async (object device, PropertyChangedEventArgs args) => { 
        //    await RefreshDevice.Invoke();
        //};

        private DeviceInformation deviceInfo;

        private BluetoothDevice bluetoothDevice;

        private RfcommDeviceService pasteService;

        public DeviceInformationUpdate DeviceInfo
        {
            set
            {
                deviceInfo.Update(value);
            }
        }
        
        public BTDevice(DeviceInformation info) {
            deviceInfo = info;
            //PropertyChanged += UpdateUi;
            /*
             * Back up lambda method for updates where each object would have a different function
             * (async (obj, args) =>
            {
               await RefreshDevice.Invoke();
            });*/
        }

        public string Id
        {
            get {
                return deviceInfo.Id;
            } 
        }

        public string Name
        {
            get
            {
                return deviceInfo.Name;
            }
        }

        public string Type
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public static object RefreshDevice { get; private set; }

        public Boolean Equals(BTDevice device)
        {
            return device.Id.Equals(this.Id);
        }

        /*public void UpdateDeviceInformation(DeviceInformationUpdate device)
        {
            deviceInfo.Update(device);
        }*/

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Device Updated."));
        }

        public async Task<ToastData> Connect(IClient client)
        {
            if (deviceInfo == null)
            { // no device to be connected to, missing information
                return new ToastData("Failed to Connect", $"No such device {Name} to be connected to.", ToastType.Alert);
            }

            // add extra check here to see if the device is already connected

            DeviceAccessStatus accessStatus = DeviceAccessInformation.CreateFromId(Id).CurrentStatus;
            if (accessStatus == DeviceAccessStatus.DeniedByUser)
            {
                // The app doesn't have permission to access Bluetooth
                // To fix go into Settings -> Privacy -> Other Devices

                return new ToastData("Failed to Connect",
                    "The other device has denied access.",
                    ToastType.Alert);
            }
            else if (accessStatus == DeviceAccessStatus.DeniedBySystem)
            {
                return new ToastData("Failed to Connect", "Denied by System.", ToastType.Alert);
            }
            else if (accessStatus == DeviceAccessStatus.Unspecified)
            {
                return new ToastData("Failed to Connect", "Unspecified Device Access.", ToastType.Alert);
            }

            try
            {
                // Getting the Bluetooth Device by Id
                bluetoothDevice = await BluetoothDevice.FromIdAsync(Id);
            }
            catch (Exception ex)
            {
                // There was an error trying to get the Bluetooth Device
                return new ToastData("Failed to Connect", $"Something went wrong when trying to connect to {Name}.", ToastType.Alert);
            }

            if (bluetoothDevice == null)
            {
                // Accessing issues: user specifies that interactions with unpaired devices are not allowed
                return new ToastData("Failed to Connect", $"{Name} is unpaired and cannot be connected. Access Status: {accessStatus}", ToastType.Alert);
            }

            var deviceServices = await bluetoothDevice.GetRfcommServicesForIdAsync(RfcommServiceId.FromUuid(ServiceConfig.serviceUuid), BluetoothCacheMode.Uncached);
            if (deviceServices.Services.Count > 0)
            {
                pasteService = deviceServices.Services[0];
            }
            else
            {
                // does not contain service needed
                return new ToastData("Failed to Connect", $"{Name} does not support this app.", ToastType.Alert);
            }

            // check SDP profile to make sure device supports the App
            var attributes = await pasteService.GetSdpRawAttributesAsync();

            if (!attributes.ContainsKey(ServiceConfig.sdpServiceAttributeId)) { // questionable if this should even be here
                return new ToastData("Unable to connect", "Please make sure that the app is running on the other device", ToastType.Alert);
            }

            // alert success or failure using Toasts

            using (var attrReader = DataReader.FromBuffer(attributes[ServiceConfig.sdpServiceAttributeId]))
            {
                var attrType = attrReader.ReadByte();
                if (attrType != ServiceConfig.SdpServiceNameAttributeType)
                {
                    return new ToastData("Unable to connect", "Please make sure that the app is running on the other device", ToastType.Alert);
                }
            }
            
            var socket = new StreamSocket();
            //attrReader.UnicodeEncoding = WindowsStreams.UnicodeEncoding.Utf8;

            try
            {
                await socket.ConnectAsync(pasteService.ConnectionHostName, pasteService.ConnectionServiceName);
                PastingApp.app.SetConnectedDevice(this, socket);

                PastingApp.app.StopServicesOnConnect();

                PastingApp.app._toast_service.
                    AddToast("Connected", "Connected to device", ToastType.Alert);
            }
            catch (Exception e) when ((uint)e.HResult == 0x80070490) // service not found 
            {
                PastingApp.app.client.deviceScanner.RestartScan();
                return new ToastData("Unable to connect", "Please make sure that the app is running on the other device", ToastType.Alert);
            }
            catch (Exception e) when ((uint)e.HResult == 0x80072740) // already connected to another device using rfcomm
            {
                PastingApp.app.client.deviceScanner.RestartScan();
                return new ToastData("Unable to connect", "The other device is currently connected to another device", ToastType.Alert);
            }
            catch(Exception e)
            {
                return new ToastData("Unable to connect", "Unaccessible network", ToastType.Alert);
            }


            // recieving input
            // may or may not need to lock this object?


            return new ToastData ("Connected!", $"Successfully connected to {Name}", ToastType.Alert);

        }

        public void Disconnect()
        {
            if (pasteService != null)
            {
                pasteService.Dispose();
                pasteService = null;
            }
        }

        public bool IsConnected()
        {
            
            throw new NotImplementedException();
        }

        public bool Pair()
        {

            throw new NotImplementedException();
        }

        public void Disconnect(IClient client)
        {
            throw new NotImplementedException();
        }

    }
}
