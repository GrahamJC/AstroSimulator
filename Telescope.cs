using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Threading;

using ASCOM.DeviceInterface;

namespace AstroSimulator
{
    public class Telescope : ObservableObject
    {
        // Properties
        public Double SiteLatitude
        {
            get { lock (this) { return ((App)Application.Current).SiteInformation.Latitude; } }
            set { lock (this) { ((App)Application.Current).SiteInformation.Latitude = value; } }
        }

        public Double SiteLongitude
        {
            get { lock (this) { return ((App)Application.Current).SiteInformation.Longitude; } }
            set { lock (this) { ((App)Application.Current).SiteInformation.Longitude = value; } }
        }

        public Double SiteElevation
        {
            get { lock (this) { return ((App)Application.Current).SiteInformation.Elevation; } }
            set { lock (this) { ((App)Application.Current).SiteInformation.Elevation = value; } }
        }

        private Double _pointingError = 0.5;
        public Double PointingError
        {
            get { lock (this) { return _pointingError; } }
            set
            {
                Boolean changed = false;
                lock (this)
                {
                    changed = (value != _pointingError);
                    if (changed)
                        _pointingError = value;
                }
                if (changed)
                    NotifyPropertyChanged("PointingError");
            }
        }

        public String Status
        {
            get
            {
                if (AtPark)
                    return "Parked";
                else if (IsTracking)
                    return "Tracking";
                else
                    return "Not tracking";
            }
        }

        private EquatorialCoordinateType _coordinateType = EquatorialCoordinateType.equLocalTopocentric;
        public EquatorialCoordinateType CoordinateType
        {
            get { lock (this) { return _coordinateType; } }
            set
            {
                Boolean changed = false;
                lock (this)
                {
                    changed = (value != _coordinateType);
                    if (changed)
                        _coordinateType = value;
                }
                if (changed)
                    NotifyPropertyChanged("CoordinateType");
            }
        }

        private Boolean _atPark = true;
        public Boolean AtPark
        {
            get { lock (this) { return _atPark; } }
            set
            {
                Boolean changed = false;
                lock (this)
                {
                    changed = (value != _atPark);
                    if (changed)
                    {
                        IsSlewing = false;
                        IsTracking = false;
                        _atPark = value;
                    }
                }
                if (changed)
                {
                    NotifyPropertyChanged("AtPark");
                    NotifyPropertyChanged("Status");
                }
            }
        }

        private Boolean _isSlewing = true;
        public Boolean IsSlewing
        {
            get { lock (this) { return _isSlewing; } }
            set
            {
                Boolean changed = false;
                lock (this)
                {
                    changed = (value != _isSlewing);
                    if (changed)
                        _isSlewing = value;
                }
                if (changed)
                {
                    NotifyPropertyChanged("IsSlewing");
                    NotifyPropertyChanged("Status");
                }
            }
        }

        private Boolean _isTracking = false;
        public Boolean IsTracking
        {
            get { lock (this) { return _isTracking; } }
            set
            {
                Boolean changed = false;
                if (!AtPark)
                {
                    lock (this)
                    {
                        changed = (value != _isTracking);
                        if (changed)
                        {
                            _isTracking = value;
                            if (_isTracking)
                            {
                                Coordinates equatorial = Utilities.GetEquatorial(_altitude, _azimuth, DateTime.UtcNow, SiteLatitude, SiteLongitude).ToJNow();
                                _rightAscension = equatorial.RightAscension;
                                _declination = equatorial.Declination;
                            }
                            else
                            {
                                _altitude = Utilities.GetAltitude(new Coordinates(Coordinates.CoordinatesType.JNow, _rightAscension, _declination), DateTime.UtcNow, SiteLatitude, SiteLongitude);
                                _azimuth = Utilities.GetAzimuth(new Coordinates(Coordinates.CoordinatesType.JNow, _rightAscension, _declination), DateTime.UtcNow, SiteLatitude, SiteLongitude);
                            }
                        }
                    }
                    if (changed)
                    {
                        NotifyPropertyChanged("IsTracking");
                        NotifyPropertyChanged("Status");
                    }
                }
            }
        }

        public Boolean IsPulseGuiding
        {
            get { lock (this) { return (DateTime.Now < _pulseGuideEnd); } }
        }

        private Double _rightAscension = 0;
        public Double RightAscension
        {
            get
            {
                lock (this)
                {
                    if (IsTracking)
                        return _rightAscension;
                    Coordinates equatorial = Utilities.GetEquatorial(_altitude, _azimuth, DateTime.UtcNow, SiteLatitude, SiteLongitude).ToJNow();
                    return equatorial.RightAscension;
                }
            }
            private set
            {
                if (IsTracking)
                {
                    Boolean changed = false; ;
                    lock (this)
                    {
                        changed = (value != _rightAscension);
                        if (changed)
                            _rightAscension = value;
                    }
                    if (changed)
                    {
                        NotifyPropertyChanged("RightAscension");
                        NotifyPropertyChanged("Altitude");
                        NotifyPropertyChanged("Azimuth");
                    }
                }
            }
        }

        public Double RightAscensionError
        {
            get { return (_pointingErrorRA + _periodicErrorRA) * 15 * 3600; }
        }

        public Double ActualRightAscension
        {
            get { return (RightAscension + _pointingErrorRA + _periodicErrorRA); }
        }

        private Double _declination = 90;
        public Double Declination
        {
            get
            {
                lock (this)
                {
                    if (IsTracking)
                        return _declination;
                    Coordinates equatorial = Utilities.GetEquatorial(_altitude, _azimuth, DateTime.UtcNow, SiteLatitude, SiteLongitude).ToJNow();
                    return equatorial.Declination;
                }
            }
            private set
            {
                if (IsTracking)
                {
                    Boolean changed = false;
                    lock (this)
                    {
                        changed = (value != _declination);
                        if (changed)
                            _declination = value;
                    }
                    if (changed)
                    {
                        NotifyPropertyChanged("Declination");
                        NotifyPropertyChanged("Altitude");
                        NotifyPropertyChanged("Azimuth");
                    }
                }
            }
        }

        public Double DeclinationError
        {
            get { return (_pointingErrorDec + _driftErrorDec) * 3600; }
        }

        public double ActualDeclination
        {
            get { return (Declination + _pointingErrorDec + _driftErrorDec); }
        }

        private Double _altitude = ((App)Application.Current).SiteInformation.Latitude;
        public Double Altitude
        {
            get
            {
                lock (this)
                {
                    if (!IsTracking)
                        return _altitude;
                    return Utilities.GetAltitude(new Coordinates(Coordinates.CoordinatesType.JNow, _rightAscension, _declination), DateTime.UtcNow, SiteLatitude, SiteLongitude);
                }
            }
            set
            {
                if (!IsTracking)
                {
                    Boolean changed = false;
                    lock (this)
                    {
                        changed = (value != _altitude);
                        if (changed)
                            _altitude = value;
                    }
                    if (changed)
                    {
                        NotifyPropertyChanged("Altitude");
                        NotifyPropertyChanged("RightAscension");
                        NotifyPropertyChanged("Declination");
                    }
                }
            }
        }

        private Double _azimuth = 0;
        public Double Azimuth
        {
            get
            {
                lock (this)
                {
                    if (!IsTracking)
                        return _azimuth;
                    return Utilities.GetAzimuth(new Coordinates(Coordinates.CoordinatesType.JNow, _rightAscension, _declination), DateTime.UtcNow, SiteLatitude, SiteLongitude);
                }
            }
            set
            {
                if (!IsTracking)
                {
                    Boolean changed = false;
                    lock (this)
                    {
                        changed = (value != _azimuth);
                        if (changed)
                            _azimuth = value;
                    }
                    if (changed)
                    {
                        NotifyPropertyChanged("Azimuth");
                        NotifyPropertyChanged("RightAscension");
                        NotifyPropertyChanged("Declination");
                    }
                }
            }
        }

        private PierSide _pierSide = PierSide.pierUnknown;
        public PierSide PierSide
        {
            get { lock (this) { return _pierSide; } }
            set
            {
                Boolean changed = false;
                lock (this)
                {
                    changed = (value != _pierSide);
                    if (changed)
                        _pierSide = value;
                }
                if (changed)
                    NotifyPropertyChanged("PierSide");
            }
        }

        private Double _guideRate = 50;
        public Double GuideRate
        {
            get { lock (this) { return _guideRate; } }
            set
            {
                Boolean changed = false;
                lock (this)
                {
                    changed = (value != _guideRate);
                    if (changed)
                        _guideRate = value;
                }
                if (changed)
                    NotifyPropertyChanged("GuideRate");
            }
        }

        private Double _peridicError = 20;
        public Double PeridicError
        {
            get { return _peridicError; }
            set
            {
                _peridicError = value;
                NotifyPropertyChanged("PeridicError");
            }
        }

        private Double _alignmentErrorAltitude = 30;
        public Double AlignmentErrorAltitude
        {
            get { return _alignmentErrorAltitude; }
            set
            {
                _alignmentErrorAltitude = value;
                NotifyPropertyChanged("AlignmentErrorAltitude");
            }
        }

        private Double _alignmentErrorAzimuth = 30;
        public Double AlignmentErrorAzimuth
        {
            get { return _alignmentErrorAzimuth; }
            set
            {
                _alignmentErrorAzimuth = value;
                NotifyPropertyChanged("AlignmentErrorAzimuth");
            }
        }

        public IAxisRates AxisRates(TelescopeAxes axis)
        {
            return _axisRates[(Int32)axis];
        }

        public Double LocalSiderealTime
        {
            get { return Utilities.GetLocalSiderealTime(DateTime.UtcNow, SiteLongitude); }
        }

        // Internal data
        private DispatcherTimer _refreshTimer;
        private static readonly AxisRates[] _axisRates;
        private double _pointingErrorRA = 0;
        private double _pointingErrorDec = 0;
        private Double _driftErrorDec = 0;
        private Double _periodicErrorRA = 0;
        private Int32 _periodicErrorCount = 0;
        private DateTime _pulseGuideEnd = DateTime.Now;

        // Construction/destruction
        static Telescope()
        {
            // Set axis rates
            _axisRates = new AxisRates[3];
            _axisRates[0] = new AxisRates(TelescopeAxes.axisPrimary);
            _axisRates[1] = new AxisRates(TelescopeAxes.axisSecondary);
            _axisRates[2] = new AxisRates(TelescopeAxes.axisTertiary);
        }

        public Telescope()
        {
            // Create timer to trigger position updates
            _refreshTimer = new DispatcherTimer();
            _refreshTimer.Interval = TimeSpan.FromSeconds(1);
            _refreshTimer.Tick += new EventHandler(RefreshTimer_Tick);
            _refreshTimer.Start();
        }

        ~Telescope()
        {
            if (_refreshTimer != null)
                _refreshTimer.Stop();
        }

        // Methods
        public void Park()
        {
            AtPark = true;
        }

        public void UnPark()
        {
            AtPark = false;
        }

        public void PulseGuide(GuideDirections direction, Double duration)
        {
            // Log pulse guide operation
            Application.Current.Dispatcher.Invoke((Action)delegate() { ((MainWindow)Application.Current.MainWindow).LogMessage("PulseGuide: {0}, {1}", direction, duration); });

            // Calculate RA/DEC adjustments (in degrees)
            Double raAdjustment = 0;
            Double decAdjustment = 0;
            Double degrees = (GuideRate / 100) * (360.0 / 24 / 60 / 60) * (duration / 1000);
            switch (direction)
            {
                case GuideDirections.guideEast:
                    raAdjustment = (degrees / 15);
                    break;
                case GuideDirections.guideWest:
                    raAdjustment = -(degrees / 15);
                    break;
                case GuideDirections.guideNorth:
                    decAdjustment = ((PierSide == PierSide.pierWest) ? degrees : -degrees);
                    break;
                case GuideDirections.guideSouth:
                    decAdjustment = ((PierSide == PierSide.pierEast) ? degrees : -degrees);
                    break;
            }

            // Adjust position
            if (IsTracking)
            {
                RightAscension += raAdjustment;
                Declination += decAdjustment;
            }
            else
            {
                Coordinates equatorial = Utilities.GetEquatorial(Altitude, Azimuth, DateTime.Now, SiteLatitude, SiteLongitude);
                equatorial.Add(raAdjustment, decAdjustment);
                Altitude = Utilities.GetAltitude(equatorial, DateTime.Now, SiteLatitude, SiteLongitude);
                Azimuth = Utilities.GetAzimuth(equatorial, DateTime.Now, SiteLatitude, SiteLongitude);
            }

            // Save pulse guide end time
            _pulseGuideEnd = DateTime.Now.AddMilliseconds(duration);
        }

        public void SlewToCoordinates(double rightAscension, double declination)
        {
            // Flag slewing
            IsSlewing = true;

            // Calculate distance being slewed (in degrees)
            Double slewDistance = Math.Sqrt(Math.Pow((rightAscension - RightAscension) * 15, 2) + Math.Pow((declination - Declination), 2));

            // Set new co-ordinates
            RightAscension = rightAscension;
            Declination = declination;
            Coordinates coordinates = new Coordinates(Coordinates.CoordinatesType.JNow, rightAscension, declination);
            Altitude = Utilities.GetAltitude(coordinates, DateTime.UtcNow, SiteLatitude, SiteLongitude);
            Azimuth = Utilities.GetAzimuth(coordinates, DateTime.UtcNow, SiteLatitude, SiteLongitude);

            // Simulate pointing error
            if (PointingError > 0)
            {
                // If the slew is more than twice the pointing error treat it as a slew to a new
                // position and generate a random pointing error; smaller slews are treated as
                // corrections and the pointing error is left unchanged
                if (slewDistance >= 2 * (PointingError / 60))
                {
                    // Random pointing error up to defined maximum
                    Random random = new Random();
                    _pointingErrorRA = (random.NextDouble() - 0.5) * 2 * PointingError / 60 / 15;
                    _pointingErrorDec = (random.NextDouble() - 0.5) * 2 * PointingError / 60;
                }
            }
            else
            {
                _pointingErrorRA = 0;
                _pointingErrorDec = 0;
            }

            // Set pier side
            Double ha = Utilities.GetHourAngle(coordinates, DateTime.UtcNow, SiteLongitude);
            PierSide = ((ha < 0) ? PierSide.pierWest : PierSide.pierEast);

            // Slew complete - reset drift error
            _driftErrorDec = 0;
            IsSlewing = false;
        }

        public void SyncToCoordinates(double rightAscension, double declination)
        {
            // Set new co-ordinates
            RightAscension = rightAscension;
            Declination = declination;
            Altitude = Utilities.GetAltitude(new Coordinates(Coordinates.CoordinatesType.JNow, rightAscension, declination), DateTime.UtcNow, SiteLatitude, SiteLongitude);
            Azimuth = Utilities.GetAzimuth(new Coordinates(Coordinates.CoordinatesType.JNow, rightAscension, declination), DateTime.UtcNow, SiteLatitude, SiteLongitude);

            // Reset pointing and drift errors
            _pointingErrorRA = 0;
            _pointingErrorDec = 0;
            _driftErrorDec = 0;
        }

        // Event handlers
        private void RefreshTimer_Tick(Object sender, EventArgs e)
        {
            // Generate poroperty change notifications to update UI
            NotifyPropertyChanged("LocalSiderealTime");
            if (IsTracking)
            {
                NotifyPropertyChanged("Altitude");
                NotifyPropertyChanged("Azimuth");

                // Calculate declination drift rate (in arcsec/sec) based on the polar alignment error and
                // current declination (sideral tracking rate is 15 arcsec/sec)
                // a) Positive azimuth error (to the East) causes declination to increase on meridian
                // b) Positive altitude error (too high) causes declination to decrease pointing East and increase pointing West
                Double driftRateDec = (-(Math.Tan((AlignmentErrorAzimuth / 60) * (Math.PI / 180)) * Math.Cos(Azimuth * Math.PI / 180))
                                       - (Math.Tan((AlignmentErrorAltitude / 60) * (Math.PI / 180)) * Math.Sin(Azimuth * Math.PI / 180)))
                                      * 15 * Math.Cos(Declination);

                // Update drift and periodic errors (timer is once per second)
                _driftErrorDec += (driftRateDec / 3600);
                if (++_periodicErrorCount == 480)
                    _periodicErrorCount = 0;
                _periodicErrorRA = Math.Sin(2 * Math.PI * ((Double)_periodicErrorCount / 480)) * PeridicError / 2 / 3600 / 15;
                NotifyPropertyChanged("RightAscensionError");
                NotifyPropertyChanged("DeclinationError");
            }
            else
            {
                NotifyPropertyChanged("RightAscension");
                NotifyPropertyChanged("Declination");
            }
        }
    }
}
