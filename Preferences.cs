using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Threading;

using ASCOM.DeviceInterface;

namespace AstroSimulator
{
    public class Preferences : ObservableObject
    {
        // Properties
        private String _gscPath = Properties.Settings.Default.GSCPath;
        public String GSCPath
        {
            get { return _gscPath; }
            set
            {
                _gscPath = value;
                NotifyPropertyChanged("GSCPath");
            }
        }

        // Construction/destruction
        public Preferences()
        {
        }

        // Methods
    }
}
