﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectedFieldServiceApp.Data
{
    class SenseHatData
    {
        public double? Humidity { get; set; }
        public double? Pressure { get; set; }
        public double? Temperature { get; set; }
        public string Location { get; set; }
    }
}
