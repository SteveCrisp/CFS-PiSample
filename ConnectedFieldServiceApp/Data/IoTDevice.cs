using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectedFieldServiceApp.Data
{

    class IoTDevice
    {
        public class DeviceInformationData
        {
            public string DeviceId { get; set; }
            public DevicePropertiesData DeviceProperties { get; set; }
            public DeviceCommandData[] Commands { get; set; }
            public DeviceCommandHistoryEntryData[] CommandHistory { get; set; }
            public bool IsSimulatedDevice { get; set; }
            public string Version { get; set; }
            public string ObjectType { get; set; }
        }

        public class DevicePropertiesData
        {
            public string DeviceID { get; set; }
            public bool HubEnabledState { get; set; }
            public DateTime CreatedTime { get; set; }
            public string DeviceState { get; set; }
            public object UpdateTime { get; set; }
            public string Manufacturer { get; set; }
            public string ModelNumber { get; set; }
            public string SerialNumber { get; set; }
            public string FirmwareVersion { get; set; }
            public string Platform { get; set; }
            public string Process { get; set; }
            public string InstalledRAM { get; set; }
            public double Latitude { get; set; }
            public double Longitude { get; set; }
        }

        public class DeviceCommandData
        {
            public string Name { get; set; }
            public object Parameters { get; set; }
        }

        public class DeviceCommandHistoryEntryData { }


    }
}
