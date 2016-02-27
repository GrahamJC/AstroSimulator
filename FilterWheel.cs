using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace AstroSimulator
{
    public class FilterWheel : ObservableObject
    {
        // Class data
        public readonly String[] FilterNames = { "Luminance", "Red", "Green", "Blue", "HAlpha", "SII", "OIII" };
        public readonly Int32[] FocusOffsets = { 0, 0, 0, 0, 0, 0, 0 };

        // Properties
        private Int32 _position = 3;
        public Int32 Position
        {
            get { lock (this) { return _position; } }
            set
            {
                Boolean changed = false;
                lock (this)
                {
                    changed = (value != _position);
                    if (changed)
                        _position = value;
                }
                if ( changed )
                {
                    NotifyPropertyChanged( "Position" );
                    NotifyPropertyChanged( "Status" );
                }
            }
        }

        public String Status
        {
            get
            {
                return String.Format( "{0} ({1})", FilterNames[ Position ], FocusOffsets[ Position ] );
            }
        }

        // Construction
        public FilterWheel()
        {
        }

        // Methods
    }
}
