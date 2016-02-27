using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using ASCOM;
using ASCOM.Utilities;
using ASCOM.DeviceInterface;

namespace AstroSimulator
{
    [Guid("01B7B1C3-37DC-4A69-8181-AAE04B72D673")]
    [ServedClassName("AstroSimulator Telescope")]
    [ProgId("AstroSimulator.Telesope")]
    [ClassInterface(ClassInterfaceType.None)]
    [ComVisible(true)]
    public class TelescopeDriver : ReferenceCountedObjectBase, ITelescopeV3
    {
        // Instaance data
        private Telescope _telescope;
        private static double _targetRA = 0;
        private static double _targetDec = 0;

        // Construction
        public TelescopeDriver()
        {
        }

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

        public void AbortSlew()
        {
        }

        public IAxisRates AxisRates(TelescopeAxes axis)
        {
            return _telescope.AxisRates(axis);
        }

        public bool CanMoveAxis(TelescopeAxes axis)
        {
            return false;
        }

        public PierSide DestinationSideOfPier(double rightAscension, double declination)
        {
            Double ha = Utilities.GetHourAngle(new Coordinates(Coordinates.CoordinatesType.JNow, rightAscension, declination), DateTime.UtcNow, SiteLongitude);
            return ((ha < 0) ? PierSide.pierWest : PierSide.pierEast);
        }

        public void FindHome()
        {
            throw new System.NotImplementedException();
        }

        public void MoveAxis(TelescopeAxes axis, double rate)
        {
            throw new System.NotImplementedException();
        }

        public void Park()
        {
            _telescope.Park();
        }

        public void PulseGuide(GuideDirections direction, int duration)
        {
            _telescope.PulseGuide(direction, duration);
        }

        public void SetPark()
        {
        }

        public void SlewToAltAz(double azimuth, double altitude)
        {
            Coordinates equatorial = Utilities.GetEquatorial(altitude, azimuth, DateTime.UtcNow, SiteLatitude, SiteLongitude).ToJNow();
            SlewToCoordinates(equatorial.RightAscension, equatorial.Declination);
        }

        public void SlewToAltAzAsync(double azimuth, double altitude)
        {
            SlewToAltAz(azimuth, altitude);
        }

        public void SlewToCoordinates(double rightAscension, double declination)
        {
            _telescope.SlewToCoordinates(rightAscension, declination);
        }

        public void SlewToCoordinatesAsync(double rightAscension, double declination)
        {
            SlewToCoordinates(rightAscension, declination);
        }

        public void SlewToTarget()
        {
            SlewToCoordinates(_targetRA, _targetDec);
        }

        public void SlewToTargetAsync()
        {
            SlewToTarget();
        }

        public void SyncToAltAz(double azimuth, double altitude)
        {
            Coordinates equatorial = Utilities.GetEquatorial(altitude, azimuth, DateTime.UtcNow, SiteLatitude, SiteLongitude).ToJNow();
            SyncToCoordinates(equatorial.RightAscension, equatorial.Declination);
        }

        public void SyncToCoordinates(double rightAscension, double declination)
        {
            _telescope.SyncToCoordinates(rightAscension, declination);
        }

        public void SyncToTarget()
        {
            SyncToCoordinates(_targetRA, _targetDec);
        }

        public void Unpark()
        {
            _telescope.UnPark();
        }

        public bool Connected
        {
            get { return (_telescope != null); }
            set
            {
                ((App)Application.Current).LogMessage("Telescope: {0}", (value ? "Connected" : "Disconnected" ));
                if (value && (_telescope == null))
                    _telescope = ((App)Application.Current).Telescope;
                else if (!value && (_telescope != null))
                    _telescope = null;
            }
        }

        public string Description
        {
            get { return "AstroSimulator telescope"; }
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
            get { return 3; }
        }

        public string Name
        {
            get { return "AstroSimulator.Telescope"; }
        }

        public ArrayList SupportedActions
        {
            get { return new ArrayList(); }
        }

        public AlignmentModes AlignmentMode
        {
            get { return AlignmentModes.algGermanPolar; }
        }

        public double Altitude
        {
            get { return _telescope.Altitude; }
        }

        public double ApertureArea
        {
            get { return 0; }
        }

        public double ApertureDiameter
        {
            get { return 0; }
        }

        public bool AtHome
        {
            get { throw new System.NotImplementedException(); }
        }

        public bool AtPark
        {
            get { return _telescope.AtPark; }
        }

        public double Azimuth
        {
            get { return _telescope.Azimuth; }
        }

        public bool CanFindHome
        {
            get { return false; }
        }

        public bool CanPark
        {
            get { return true; }
        }

        public bool CanPulseGuide
        {
            get { return true; }
        }

        public bool CanSetDeclinationRate
        {
            get { return false; }
        }

        public bool CanSetGuideRates
        {
            get { return true; }
        }

        public bool CanSetPark
        {
            get { return true; }
        }

        public bool CanSetPierSide
        {
            get { return false; }
        }

        public bool CanSetRightAscensionRate
        {
            get { return false; }
        }

        public bool CanSetTracking
        {
            get { return true; }
        }

        public bool CanSlew
        {
            get { return true; }
        }

        public bool CanSlewAltAz
        {
            get { return true; }
        }

        public bool CanSlewAltAzAsync
        {
            get { return true; }
        }

        public bool CanSlewAsync
        {
            get { return true; }
        }

        public bool CanSync
        {
            get { return true; }
        }

        public bool CanSyncAltAz
        {
            get { return true; }
        }

        public bool CanUnpark
        {
            get { return true; }
        }

        public double Declination
        {
            get { return _telescope.Declination; }
        }

        public double DeclinationRate
        {
            get { throw new System.NotImplementedException(); }
            set { throw new System.NotImplementedException(); }
        }

        public bool DoesRefraction
        {
            get { throw new System.NotImplementedException(); }
            set { throw new System.NotImplementedException(); }
        }

        public EquatorialCoordinateType EquatorialSystem
        {
            get { return _telescope.CoordinateType; }
        }

        public double FocalLength
        {
            get { return 0; }
        }

        public double GuideRateDeclination
        {
            get { return (_telescope.GuideRate / 100) * (360.0 / 24 / 60 / 60); }
            set { _telescope.GuideRate = value * 100 / (360.0 / 24 / 60 / 60); }
        }

        public double GuideRateRightAscension
        {
            get { return (_telescope.GuideRate / 100) * (360.0 / 24 / 60 / 60); }
            set { _telescope.GuideRate = value * 100 / (360.0 / 24 / 60 / 60); }
        }

        public bool IsPulseGuiding
        {
            get { return _telescope.IsPulseGuiding; }
        }

        public double RightAscension
        {
            get { return _telescope.RightAscension; }
        }

        public double RightAscensionRate
        {
            get { throw new System.NotImplementedException(); }
            set { throw new System.NotImplementedException(); }
        }

        public PierSide SideOfPier
        {
            get { return _telescope.PierSide; }
            set { throw new System.NotImplementedException(); }
        }

        public double SiderealTime
        {
            get { return Utilities.GetLocalSiderealTime(DateTime.UtcNow, SiteLongitude); }
        }

        public double SiteElevation
        {
            get { return _telescope.SiteElevation; }
            set { _telescope.SiteElevation = value; }
        }

        public double SiteLatitude
        {
            get { return _telescope.SiteLatitude; }
            set { _telescope.SiteLatitude = value; }
        }

        public double SiteLongitude
        {
            get { return _telescope.SiteLongitude; }
            set { _telescope.SiteLongitude = value; }
        }

        public bool Slewing
        {
            get { return false; }
        }

        public short SlewSettleTime
        {
            get { return 0; }
            set {  }
        }

        public double TargetDeclination
        {
            get { return _targetDec; }
            set { _targetDec = value; }
        }

        public double TargetRightAscension
        {
            get { return _targetRA; }
            set { _targetRA = value; }
        }

        public bool Tracking
        {
            get { return _telescope.IsTracking; }
            set { _telescope.IsTracking = value; }
        }

        public DriveRates TrackingRate
        {
            get { return DriveRates.driveSidereal; }
            set { throw new System.NotImplementedException(); }
        }

        public ITrackingRates TrackingRates
        {
            get { return new TrackingRates(); }
        }

        public DateTime UTCDate
        {
            get { return DateTime.UtcNow; }
            set { throw new System.NotImplementedException(); }
        }
    }

    #region Rate
    //
    // The Rate class implements IRate, and is used to hold values
    // for AxisRates. You do not need to change this class.
    //
    // The Guid attribute sets the CLSID for ASCOM.Telescope.Rate
    // The ClassInterface/None addribute prevents an empty interface called
    // _Rate from being created and used as the [default] interface
    //
    [Guid("3ec5dc86-db99-4a3d-abba-c944facfc0d5")]
    [ClassInterface(ClassInterfaceType.None)]
    [ComVisible(true)]
    public class Rate : IRate
    {
        private double _maximum = 0;
        private double _minimum = 0;

        //
        // Default constructor - Internal prevents public creation
        // of instances. These are values for AxisRates.
        //
        internal Rate(double minimum, double maximum)
        {
            _maximum = maximum;
            _minimum = minimum;
        }

        #region Implementation of IRate

        public void Dispose()
        {
            throw new System.NotImplementedException();
        }

        public double Maximum
        {
            get { throw new System.NotImplementedException(); }
            set { throw new System.NotImplementedException(); }
        }

        public double Minimum
        {
            get { throw new System.NotImplementedException(); }
            set { throw new System.NotImplementedException(); }
        }

        #endregion
    }
    #endregion

    #region AxisRates
    //
    // AxisRates is a strongly-typed collection that must be enumerable by
    // both COM and .NET. The IAxisRates and IEnumerable interfaces provide
    // this polymorphism. 
    //
    // The Guid attribute sets the CLSID for ASCOM.Telescope.AxisRates
    // The ClassInterface/None addribute prevents an empty interface called
    // _AxisRates from being created and used as the [default] interface
    //
    [Guid("3ad50815-f302-4c65-bcaf-0fd4fc2876e1")]
    [ClassInterface(ClassInterfaceType.None)]
    [ComVisible(true)]
    public class AxisRates : IAxisRates, IEnumerable
    {
        private TelescopeAxes _axis;
        private readonly Rate[] _rates;

        //
        // Constructor - Internal prevents public creation
        // of instances. Returned by Telescope.AxisRates.
        //
        internal AxisRates(TelescopeAxes axis)
        {
            _axis = axis;
            //
            // This collection must hold zero or more Rate objects describing the 
            // rates of motion ranges for the Telescope.MoveAxis() method
            // that are supported by your driver. It is OK to leave this 
            // array empty, indicating that MoveAxis() is not supported.
            //
            // Note that we are constructing a rate array for the axis passed
            // to the constructor. Thus we switch() below, and each case should 
            // initialize the array for the rate for the selected axis.
            //
            switch (axis)
            {
                case TelescopeAxes.axisPrimary:
                    // TODO Initialize this array with any Primary axis rates that your driver may provide
                    // Example: m_Rates = new Rate[] { new Rate(10.5, 30.2), new Rate(54.0, 43.6) }
                    _rates = new Rate[0];
                    break;
                case TelescopeAxes.axisSecondary:
                    // TODO Initialize this array with any Secondary axis rates that your driver may provide
                    _rates = new Rate[0];
                    break;
                case TelescopeAxes.axisTertiary:
                    // TODO Initialize this array with any Tertiary axis rates that your driver may provide
                    _rates = new Rate[0];
                    break;
            }
        }

        #region IAxisRates Members

        public int Count
        {
            get { return _rates.Length; }
        }

        public void Dispose()
        {
            throw new System.NotImplementedException();
        }

        public IEnumerator GetEnumerator()
        {
            return _rates.GetEnumerator();
        }

        public IRate this[int index]
        {
            get { return _rates[index - 1]; }	// 1-based
        }

        #endregion



    }
    #endregion

    #region TrackingRates
    //
    // TrackingRates is a strongly-typed collection that must be enumerable by
    // both COM and .NET. The ITrackingRates and IEnumerable interfaces provide
    // this polymorphism. 
    //
    // The Guid attribute sets the CLSID for ASCOM.Telescope.TrackingRates
    // The ClassInterface/None addribute prevents an empty interface called
    // _TrackingRates from being created and used as the [default] interface
    //
    [Guid("84814d05-8921-4bc8-9c54-40597ee1293a")]
    [ClassInterface(ClassInterfaceType.None)]
    [ComVisible(true)]
    public class TrackingRates : ITrackingRates, IEnumerable
    {
        private readonly DriveRates[] _trackingRates;

        //
        // Default constructor - Internal prevents public creation
        // of instances. Returned by Telescope.AxisRates.
        //
        internal TrackingRates()
        {
            //
            // This array must hold ONE or more DriveRates values, indicating
            // the tracking rates supported by your telescope. The one value
            // (tracking rate) that MUST be supported is driveSidereal!
            //
            _trackingRates = new[] { DriveRates.driveSidereal };
            // TODO Initialize this array with any additional tracking rates that your driver may provide
        }

        #region ITrackingRates Members

        public int Count
        {
            get { return _trackingRates.Length; }
        }

        public IEnumerator GetEnumerator()
        {
            return _trackingRates.GetEnumerator();
        }

        public void Dispose()
        {
            throw new System.NotImplementedException();
        }

        public DriveRates this[int index]
        {
            get { return _trackingRates[index - 1]; }	// 1-based
        }

        #endregion

    }
    #endregion
}
