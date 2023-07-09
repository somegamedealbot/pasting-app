using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using PastingMaui.Data;
using Windows.Foundation;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Rfcomm;
using PastingMaui.Shared;
using Windows.Networking.Sockets;
using System.IO;
using Windows.Devices.PointOfService.Provider;
using WindowsStreams = Windows.Storage.Streams;
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

        private StreamSocket socket;

        private DataWriter dataWriter;
        private DataReader dataReader;

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

        /*public override string Name
        {
            return deviceInfo.Name;
        }

        public override string Kind()
        {
            throw new NotImplementedException();
        }*/

        public Boolean Equals(BTDevice device)
        {
            return device.Id.Equals(this.Id);
        }

        /*public void UpdateDeviceInformation(DeviceInformationUpdate device)
        {
            deviceInfo.Update(device);
        }*/

        /*public string Kind()
        {
            return deviceInfo.Kind;
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

            // services supported by the device
            var deviceServices = await bluetoothDevice.GetRfcommServicesForIdAsync(RfcommServiceId.FromUuid(ServiceConfig.serviceUuid));

            if (deviceServices.Services.Count > 0) {
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

            var attrReader = DataReader.FromBuffer(attributes[ServiceConfig.sdpServiceAttributeId]);
            var attrType = attrReader.ReadByte();
            
            if (attrType != ServiceConfig.SdpServiceNameAttributeType)
            {
                return new ToastData("Unable to connect", "Please make sure that the app is running on the other device", ToastType.Alert);
            }

            socket = new StreamSocket();
            attrReader.UnicodeEncoding = WindowsStreams.UnicodeEncoding.Utf8;

            try
            {
                await socket.ConnectAsync(pasteService.ConnectionHostName, pasteService.ConnectionServiceName);
                StreamReader reader = new StreamReader(socket.OutputStream.AsStreamForWrite());
                dataWriter = new DataWriter(socket.OutputStream);
                dataReader = new DataReader(socket.InputStream);
                PastingApp.app.client.deviceScanner.StopScan();

                //client.SetConnectedDevice(this);
                ReadLoop(dataReader, PastingApp.app.client.deviceScanner);

            }
            catch (Exception e) when ((uint)e.HResult == 0x80070490) // service not found 
            {
                return new ToastData("Unable to connect", "Please make sure that the app is running on the other device", ToastType.Alert);
            }
            catch (Exception e) when ((uint)e.HResult == 0x80072740) // already connected to another device using rfcomm
            {
                return new ToastData("Unable to connect", "The other device is currently connected to another device", ToastType.Alert);
            }


            // recieving input
            // may or may not need to lock this object?


            return new ToastData ("Connected!", $"Successfully connected to {Name}", ToastType.Alert);

        }

        public async void ReadLoop(DataReader reader, IBTScan scanner)
        {

            // now connected to the client

            // display socket statistics using socket.Information

            while (true)
            {
                IBuffer buffer;
                bool disconnected = false;
                int blockSize = 4096;
                int totalReadCount = 0;

                try
                {
                    int type = dataReader.ReadByte(); // type of info

                    if (type > 2)
                    {
                        // notify user bad packet
                    }

                    // 32 bit integer
                    uint dataSize = dataReader.ReadUInt32();

                    var readCount = await dataReader.LoadAsync((uint)blockSize);

                    if (readCount != dataSize)
                    {
                        // data was cut off, or connection was lost

                        // cut connection
                        // Disconnect Here?
                        disconnected = true;

                        // notify user that connection was broken
                    }
                    else
                    {
                        buffer = dataReader.ReadBuffer(readCount);
                        // deal with data here
                    }


                    // save file to folder location
                }
                catch (Exception ex)
                {
                    // handle exception here 
                    if (socket == null)
                    {
                        if ((uint)ex.HResult == 0x80072745) // disconnect by remote device
                        {

                        }
                        else
                        {

                        }
                    }
                }

                if (disconnected)
                {
                    Disconnect();
                    return;
                    // notify user about the disconnection
                }
            }
        }

        public void Disconnect()
        {
            if (dataWriter != null)
            {
                dataWriter.DetachStream();
                dataWriter = null;
            }

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

        //public Task<ToastData> Connect(IClient scanner)
        //{
        //    throw new NotImplementedException();
        //}
    }
}
