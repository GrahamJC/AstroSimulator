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
    [Guid("06538e5a-f7be-461b-bc3f-85f429d0e45d")]
    [ServedClassName("AstroSimulator Focuser")]
    [ProgId("AstroSimulator.Focuser")]
    [ClassInterface(ClassInterfaceType.None)]
    [ComVisible(true)]
    public class FocuserDriver : ReferenceCountedObjectBase, IFocuserV2
    {
        // Instance data
        private Focuser _focuser;

        // Construction
        public FocuserDriver()
        {
        }

        // IFocuserV2
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

        public void Halt()
        {
        }

        public void Move(int value)
        {
            if (_focuser != null)
                _focuser.Move(value);
        }

        public void SetupDialog()
        {
        }

        public bool Connected
        {
            get { return (_focuser != null); }
            set
            {
                ((App)Application.Current).LogMessage("Focuser: {0}", (value ? "Connected" : "Disconnected" ));
                if (value && (_focuser == null))
                    _focuser = ((App)Application.Current).Focuser;
                else if (!value && (_focuser != null))
                    _focuser = null;
            }
        }

        public string Description
        {
            get { return "AstroSimulator focuser"; }
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

        public short InterfaceVersion
        {
            get { return 2; }
        }

        public string Name
        {
            get { return "AstroSimulator.Focuser"; }
        }

        public ArrayList SupportedActions
        {
            get { return new ArrayList(); }
        }

        public bool Absolute
        {
            get { return true; }
        }

        public bool IsMoving
        {
            get { return false; }
        }

        // use the V2 connected property
        public bool Link
        {
            get { return this.Connected; }
            set { this.Connected = value; }
        }

        public int MaxIncrement
        {
            get { return 10000; }
        }

        public int MaxStep
        {
            get { return 10000; }
        }

        public int Position
        {
            get { return _focuser.Position; }
        }

        public double StepSize
        {
            get { throw new System.NotImplementedException(); }
        }

        public bool TempComp
        {
            get { return false; }
            set { throw new System.NotImplementedException(); }
        }

        public bool TempCompAvailable
        {
            get { return false; }
        }

        public double Temperature
        {
            get { throw new System.NotImplementedException(); }
        }
    }
}
