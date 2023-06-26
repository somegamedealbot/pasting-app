﻿using PastingMaui.Platforms;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PastingMaui.Data
{
    public interface IClient
    {
        public IBTScan scanner
        {
            get;
        }

        public ObservableCollection<IBTDevice> discovered_devices
        {
            get;
        }

    }
}