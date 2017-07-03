using ConnectedFieldServiceApp.Data;
using Emmellsoft.IoT.Rpi.SenseHat;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking.Connectivity;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace ConnectedFieldServiceApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        static bool _telemtryRunning = false;
        static bool _deviceClientTalking = false;
        static DeviceClient deviceClient;

        //-----------Data Storage For The Parameters
        Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
        Windows.Storage.StorageFolder localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;

        static string deviceName = "";
        static string iotHubUri = "";
        static string deviceKey = "";

        static double humidity;
        static double temperature;
        //static double gyro;
        static ISenseHat mySenseHat;

        public MainPage()
        {
            this.InitializeComponent();

            //Check for existing Device Values
            CheckDeviceValues();
            DeviceDetailsStack.Visibility = Visibility.Visible;
        }
        //----This function gets the stored values and sets them to the variable's and textbox's.
        private void CheckDeviceValues()
        {
            if (localSettings.Values["deviceName"] != null)
            {
                deviceName = localSettings.Values["deviceName"].ToString();
                deviceIDText.Text = deviceName;
            }
            else DeviceDetailsStack.Visibility = Visibility.Visible;

            if (localSettings.Values["iotHubUri"] != null)
            {
                iotHubUri = localSettings.Values["iotHubUri"].ToString();
                iotHubUriText.Text = iotHubUri;
            }
            else DeviceDetailsStack.Visibility = Visibility.Visible;

            if (localSettings.Values["deviceKey"] != null)
            {
                deviceKey = localSettings.Values["deviceKey"].ToString();
                deviceKeyText.Text = deviceKey;
            }
            else DeviceDetailsStack.Visibility = Visibility.Visible;
        }

        public void ContinueInitialize()
        {
            //get the sensehat registered
            InitializeSenseHat();

            //get IP Address, NOTE this could be put on a loop that would prevent the initialise
            // of the IoT hub that requires the connection....
            ipAddressText.Text = GetLocalIp();

            //get the connection to IoT hub started
            InitializeIoTDevice();


        }

        public async void InitializeIoTDevice()
        {
            try
            {
                deviceClient = DeviceClient.Create(iotHubUri, new DeviceAuthenticationWithRegistrySymmetricKey(deviceName, deviceKey));
                
            }
            catch (Exception ex)
            {
                //Most likely there is no internet.
                MessageDialog dialog = new MessageDialog("There was an error setting up the device client with IoT Hub", "Information");
                await dialog.ShowAsync();
            }

        }

        //Grabs the local IP Address (can be used to verify the network connnection is live
        private string GetLocalIp()
        {
            var icp = NetworkInformation.GetInternetConnectionProfile();

            if (icp?.NetworkAdapter == null) return null;
            var hostname =
                NetworkInformation.GetHostNames()
                    .SingleOrDefault(
                        hn =>
                            hn.IPInformation?.NetworkAdapter != null && hn.IPInformation.NetworkAdapter.NetworkAdapterId
                            == icp.NetworkAdapter.NetworkAdapterId);

            // the ip address
            return hostname?.CanonicalName;
        }

        //Sets up the SenseHat so that it can be used, also sets lights.
        public static async void InitializeSenseHat()
        {

            mySenseHat = await SenseHatFactory.GetSenseHat();
            mySenseHat.Display.Clear();
            //Sets the color to red (it goes green when the data is being collected
            mySenseHat.Display.Fill(Color.FromArgb(0, 255, 0, 0));
            mySenseHat.Display.Update();

        }

        //This function does the work of sending the documents to the IoT Hub...
        private async void SendDeviceToCloudMessagesAsync()
        {
            await deviceClient.OpenAsync();

            humidity = sliderHumidity.Value;
            temperature = sliderTemperature.Value;

            while (_telemtryRunning == true)
            {

                mySenseHat.Display.Clear();
                mySenseHat.Display.Fill(Color.FromArgb(0, 0, 255, 0));
                mySenseHat.Display.Update();

                var telemetryDataPoint = new
                {
                    deviceId = deviceName,
                    humidity,
                    temperature

                };
                var messageString = JsonConvert.SerializeObject(telemetryDataPoint);
                var message = new Message(Encoding.ASCII.GetBytes(messageString));

                await deviceClient.SendEventAsync(message);

                mySenseHat.Sensors.HumiditySensor.Update();
                temperature = (double)mySenseHat.Sensors.Temperature;
                humidity = (double)mySenseHat.Sensors.Humidity;

                TempGauge.Value = temperature;
                HumGauge.Value = humidity;

                Task.Delay(1000).Wait();
            }
            Start.IsOn = false;
        }

        //Set the toggle to off when device is stopping (used with Cloud to Device message)
        public void switchOff()
        {
            Start.IsOn = false;
        }

        //Button Start, which actually starts grabbing the sensor data.
        private void buttonStart(object sender, RoutedEventArgs e)
        {

            if (Start.IsOn) _telemtryRunning = true;
            else
            {
                mySenseHat.Display.Clear();
                mySenseHat.Display.Fill(Color.FromArgb(0, 255, 0, 0));
                mySenseHat.Display.Update();
                _telemtryRunning = false;
            }

            SendDeviceToCloudMessagesAsync();
        }

        private static async void InitializeDevice()
        {
            string deviceId = deviceName; //This line can be passed in if you want to register new.
            var data = new TemperatureInitData()
            {
                DeviceId = deviceId,
                DeviceProperties = new DeviceProperties()
                {
                    CreatedTime = DateTime.Now.ToUniversalTime(),
                    DeviceID = deviceId,
                    DeviceState = "normal",
                    FirmwareVersion = "1.8",
                    HubEnabledState = true,
                    InstalledRAM = "1024 MB",
                    Latitude = 51.461220,
                    Longitude = -0.925891,
                    Manufacturer = "Contoso",
                    ModelNumber = "v2",
                    Platform = "MyPlattform",
                    Process = "x86",
                    SerialNumber = "xxxxx",
                },
                CommandHistory = new List<DeviceCommandHistoryEntry>().ToArray(),
                Commands = CommandSetup().ToArray(),
                IsSimulatedDevice = false,
                ObjectType = "DeviceInfo",
                Version = "1.0"
            };

            var messageString = JsonConvert.SerializeObject(data);
            var message = new Message(Encoding.ASCII.GetBytes(messageString));
            await deviceClient.SendEventAsync(message);
        }

        private static List<DeviceCommand> CommandSetup()
        {

            var myCommand = new List<DeviceCommand>();

            myCommand.Add(new DeviceCommand { Name = "Stop" });
            myCommand.Add(new DeviceCommand { Name = "GetTemp" });

            return myCommand;
        }

        //This allows for the device to re-register
        private void UpdateDevice_Click(object sender, RoutedEventArgs e)
        {
            CheckDeviceValues();
            DeviceDetailsStack.Visibility = Visibility.Visible;
        }

        //Force a re-register of the device
        private void Resgister_Click(object sender, RoutedEventArgs e)
        {
            InitializeDevice();
        }


        //Runs after the initial check is done on the IoT Hub details
        private void deviceDetailsOK_Click(object sender, RoutedEventArgs e)
        {
            if (deviceIDText.Text != null)
            {
                localSettings.Values["deviceName"] = deviceIDText.Text;
                if (iotHubUriText != null)
                {
                    localSettings.Values["iotHubUri"] = iotHubUriText.Text;
                    if (deviceKeyText.Text != null)
                    {
                        localSettings.Values["deviceKey"] = deviceKeyText.Text;
                        DeviceDetailsStack.Visibility = Visibility.Collapsed;


                        CheckDeviceValues();
                        ////--------Put a verification of the parameters before the initialize
                        ContinueInitialize();
                    }
                }

            }
        }

    }
}
