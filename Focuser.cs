using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace AstroSimulator
{
    public class Focuser : ObservableObject
    {
        // Properties
        private Int32 _position = 5000;
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
                if (changed)
                    NotifyPropertyChanged("Position");
            }
        }

        public Double Defocus
        {
            get { return Math.Abs(_position - 5000) / 10; }
        }

        // Construction
        public Focuser()
        {
        }

        // Methods
        public void Move(Int32 value)
        {
            Position = value;
        }
    }
}
