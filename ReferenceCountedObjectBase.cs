using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace AstroSimulator
{
    public class ReferenceCountedObjectBase
    {
        public ReferenceCountedObjectBase()
        {
            ((App)Application.Current).IncrementObjectCount();
        }

        ~ReferenceCountedObjectBase()
        {
            ((App)Application.Current).DecrementObjectCount();
        }
    }
}
