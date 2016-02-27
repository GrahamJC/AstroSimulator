using System;
using System.Collections;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using ASCOM;
using ASCOM.DeviceInterface;
using ASCOM.Utilities;

namespace AstroSimulator
{
    [Guid("C622D6F1-DD65-41ED-A40D-FD429C8F4C8E")]
    [ServedClassName("AstroSimulator Guider")]
    [ProgId("AstroSimulator.Guider")]
    [ClassInterface(ClassInterfaceType.None)]
    [ComVisible(true)]
    public class GuiderDriver : ReferenceCountedObjectBase, ICameraV2
    {
        private Guider _guider;

        private double _exposureDuration = 0;
        private DateTime _exposureStartTime = DateTime.MinValue;
        private object _imageArray;

        //
        // Constructor - Must be public for COM registration!
        //
        public GuiderDriver()
        {
        }

        #region Implementation of ICameraV2

        public void SetupDialog()
        {
        }

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

        public void PulseGuide(GuideDirections direction, int duration)
        {
            throw new MethodNotImplementedException("PulseGuide");
        }

        public void StartExposure(double duration, bool light)
        {
            // Save exposure details
            _exposureDuration = duration;
            _exposureStartTime = DateTime.Now;

            // Simulate image
            _imageArray = _guider.ExposeImage(duration);
        }

        public void StopExposure()
        {
        }

        public void AbortExposure()
        {
        }

        public bool Connected
        {
            get { return (_guider != null); }
            set
            {
                ((App)Application.Current).LogMessage("Guider: {0}", (value ? "Connected" : "Disconnected" ));
                if (value && (_guider == null))
                    _guider = ((App)Application.Current).Guider;
                else if (!value && (_guider != null))
                    _guider = null;
            }
        }

        public string Description
        {
            get { return "AstroSimulator guider"; }
        }

        public string DriverInfo
        {
            get { return Description; }
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
            get { return "AstroSimulator.Guider"; }
        }

        public ArrayList SupportedActions
        {
            get { return new ArrayList(); }
        }

        public short BinX
        {
            get { return (short)_guider.Binning; }
            set { _guider.Binning = value; ; }
        }

        public short BinY
        {
            get { return (short)_guider.Binning; }
            set { _guider.Binning = value; ; }
        }

        public CameraStates CameraState
        {
            get
            {
                if ((_exposureStartTime != DateTime.MinValue) && (DateTime.Now < _exposureStartTime.AddSeconds(_exposureDuration)))
                    return CameraStates.cameraExposing;
                return CameraStates.cameraIdle;
            }
        }

        public int CameraXSize
        {
            get { return _guider.PixelsX; }
        }

        public int CameraYSize
        {
            get { return _guider.PixelsY; }
        }

        public bool CanAbortExposure
        {
            get { return true; }
        }

        public bool CanAsymmetricBin
        {
            get { return false; }
        }

        public bool CanGetCoolerPower
        {
            get { return false; }
        }

        public bool CanPulseGuide
        {
            get { return false; }
        }

        public bool CanSetCCDTemperature
        {
            get { return false; }
        }

        public bool CanStopExposure
        {
            get { return true; }
        }

        public double CCDTemperature
        {
            get { return 0; }
        }

        public bool CoolerOn
        {
            get { return false; }
            set {  }
        }

        public double CoolerPower
        {
            get { return 0; }
        }

        public double ElectronsPerADU
        {
            get { return 0.5; }
        }

        public double FullWellCapacity
        {
            get { return 50000; }
        }

        public bool HasShutter
        {
            get { return false; }
        }

        public double HeatSinkTemperature
        {
            get { return 0; }
        }

        public object ImageArray
        {
            get { return _imageArray; }
        }

        public object ImageArrayVariant
        {
            get { throw new PropertyNotImplementedException("ImageArrayVariant", false); }
        }

        public bool ImageReady
        {
            get { return true; }
        }

        public bool IsPulseGuiding
        {
            get { return false; }
        }

        public double LastExposureDuration
        {
            get { return _exposureDuration; }
        }

        public string LastExposureStartTime
        {
            get { return _exposureStartTime.ToString("yyyy-MM-ddTHH:mm:ss"); }
        }

        public int MaxADU
        {
            get { return 65536; }
        }

        public short MaxBinX
        {
            get { return 4; }
        }

        public short MaxBinY
        {
            get { return 4; }
        }

        public int NumX
        {
            get { return _guider.SubFrameWidth; }
            set { _guider.SubFrameWidth = value; }
        }

        public int NumY
        {
            get { return _guider.SubFrameHeight; }
            set { _guider.SubFrameHeight = value; }
        }

        public double PixelSizeX
        {
            get { return _guider.PixelSize; }
        }

        public double PixelSizeY
        {
            get { return _guider.PixelSize; }
        }

        public double SetCCDTemperature
        {
            get { return 0; }
            set {  }
        }

        public int StartX
        {
            get { return _guider.SubFrameX; }
            set { _guider.SubFrameX = value; }
        }

        public int StartY
        {
            get { return _guider.SubFrameY; }
            set { _guider.SubFrameY = value; }
        }

        public short BayerOffsetX
        {
            get { return 0; }
        }

        public short BayerOffsetY
        {
            get { return 0; }
        }

        public bool CanFastReadout
        {
            get { return true; }
        }

        public double ExposureMax
        {
            get { return 3600; }
        }

        public double ExposureMin
        {
            get { return 0.1; }
        }

        public double ExposureResolution
        {
            get { return 0.1; }
        }

        public bool FastReadout
        {
            get { return false; }
            set {  }
        }

        public short Gain
        {
            get { return 0; }
            set { }
        }

        public short GainMax
        {
            get { return 0; }
        }

        public short GainMin
        {
            get { return 0; }
        }

        public ArrayList Gains
        {
            get { return new ArrayList(); }
        }

        public short PercentCompleted
        {
            get { return 0; }
        }

        public short ReadoutMode
        {
            get { return 0; }
            set { }
        }

        public ArrayList ReadoutModes
        {
            get { return new ArrayList(); }
        }

        public string SensorName
        {
            get { return "Simulator"; }
        }

        public SensorType SensorType
        {
            get { return SensorType.Monochrome; }
        }

        #endregion
    }
}
