using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Threading;

using ASCOM.DeviceInterface;

namespace AstroSimulator
{
    public class SiteInformation : ObservableObject
    {
        // Properties
        private Double _latitude = 51;
        public Double Latitude
        {
            get { return _latitude; }
            set
            {
                _latitude = value;
                NotifyPropertyChanged("Latitude");
            }
        }

        private Double _longitude = -4;
        public Double Longitude
        {
            get { return _longitude; }
            set
            {
                _longitude = value;
                NotifyPropertyChanged("Longitude");
            }
        }

        private Double _elevation = 0;
        public Double Elevation
        {
            get { return _elevation; }
            set
            {
                _elevation = value;
                NotifyPropertyChanged("Elevation");
            }
        }

        private Double _seeing = 2;
        public Double Seeing
        {
            get { return _seeing; }
            set
            {
                _seeing = value;
                NotifyPropertyChanged("Seeing");
            }
        }

        // Construction/destruction
        public SiteInformation()
        {
        }

        // Methods
    }
}
