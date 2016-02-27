using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Globalization;
using System.Windows;
using ASCOM;
using ASCOM.Utilities;
using ASCOM.DeviceInterface;

namespace AstroSimulator
{
    [Guid("087C3BDC-79A6-4C19-83B3-997E9BB15ECA")]
    [ServedClassName("AstroSimulator Filter Wheel")]
    [ProgId("AstroSimulator.FilterWheel")]
    [ClassInterface(ClassInterfaceType.None)]
    [ComVisible(true)]
    public class FilterWheelDriver : ReferenceCountedObjectBase, IFilterWheelV2
    {
        // Instance data
        private FilterWheel _filterWheel;

        // Construction
        public FilterWheelDriver()
        {
        }

        // IFilterWheelV2
        public string Action(string actionName, string actionParameters)
        {
            throw new ASCOM.MethodNotImplementedException("Action");
        }

        public void CommandBlind(string command, bool raw)
        {
            throw new ASCOM.MethodNotImplementedException("CommandBlind");
        }

        public bool CommandBool(string command, bool raw)
        {
            throw new ASCOM.MethodNotImplementedException("CommandBool");
        }

        public string CommandString(string command, bool raw)
        {
            throw new ASCOM.MethodNotImplementedException("CommandString");
        }

        public void Dispose()
        {
        }

        public void SetupDialog()
        {
        }

        public bool Connected
        {
            get { return (_filterWheel != null); }
            set
            {
                ((App)Application.Current).LogMessage("FilterWheel: {0}", (value ? "Connected" : "Disconnected" ));
                if (value && (_filterWheel == null))
                    _filterWheel = ((App)Application.Current).FilterWheel;
                else if (!value && (_filterWheel != null))
                    _filterWheel = null;
            }
        }

        public string Description
        {
            get { return "AstroSimulator filter wheel"; }
        }

        public string DriverInfo
        {
            get { return this.Description; }
        }

        public string DriverVersion
        {
            get
            {
                Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                return String.Format(CultureInfo.InvariantCulture, "{0}.{1}", version.Major, version.Minor);
            }
        }

        public Int32[] FocusOffsets
        {
            get { return _filterWheel.FocusOffsets; }
        }

        public short InterfaceVersion
        {
            get { return 2; }
        }

        public string Name
        {
            get { return "AstroSimulator.FilterWheel"; }
        }

        public String[] Names
        {
            get { return _filterWheel.FilterNames; }
        }

        public Int16 Position
        {
            get { return (Int16)_filterWheel.Position; }
            set { _filterWheel.Position = (Int16)value; }
        }

        public ArrayList SupportedActions
        {
            get { return new ArrayList(); }
        }
    }
}
