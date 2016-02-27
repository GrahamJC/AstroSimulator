using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AstroSimulator
{
    public class Utilities
    {
        // Class methods
        public static Coordinates GetEquatorial(Double altitude, Double azimuth, DateTime utc, Double latitude, Double longitude)
        {
            // All the following are in radians
            Double lst = GetLocalSiderealTime(utc, longitude) * Math.PI / 12;
            Double lat = latitude * Math.PI / 180;
            Double alt = altitude * Math.PI / 180;
            Double az = azimuth * Math.PI / 180;

            // Calculate RA/Dec and return coordinates
            Double dec = Math.Asin((Math.Sin(alt) * Math.Sin(lat)) + (Math.Cos(alt) * Math.Cos(lat) * Math.Cos(az)));
            Double H = Math.Atan2(-Math.Cos(alt) * Math.Cos(lat) * Math.Sin(az), Math.Sin(alt) - (Math.Sin(lat) * Math.Sin(dec)));
            Double ra = lst - H;
            while (ra < 0) ra += 24;
            while (ra > 24) ra -= 24;
            return new Coordinates(ra * 12 / Math.PI, dec * 180 / Math.PI);
        }

        public static Double GetJulianDate(DateTime utc)
        {
            // Get component parts of date
            Int32 year = utc.Year;
            Int32 month = utc.Month;
            Int32 day = utc.Day;
            Int32 hour = utc.Hour;
            Int32 minute = utc.Minute;
            Int32 second = utc.Second;

            // Calculate Julian day
            if (month <= 2)
            {
                month += 12;
                year -= 1;
            }
            Int32 A = (Int32)Math.Floor((Double)year / 100);
            Int32 B = 2 - A + (Int32)Math.Floor((Double)A / 4);
            Int32 C = (Int32)Math.Floor((Double)year * 365.25);
            Int32 D = (Int32)Math.Floor(30.6001 * ((Double)month + 1));
            Double julianDay = B + C + D + day + 1720994.5;

            // Add current time to get Julian date
            return julianDay + ((Double)hour / 24) + (Double)minute / 1440 + (Double)second / 86400;
        }

        public static Double GetGreenwichSiderealTime(DateTime utc)
        {
            // Get the Julian date at 0h and calculate Julian days since midday on 1/1/2000
            Double jd = GetJulianDate(utc.Date);
            Double S = jd - 2451545;

            // Calculate GST at 0h
            Double T = S / 36525;
            Double T0 = 6.697374558 + (2400.051336 * T) + (0.000025862 * T * T);

            // Add adjusted time component
            Double time = (Double)utc.Hour + ((Double)utc.Minute / 60) + ((Double)utc.Second / 3600);
            Double gst = T0 + (1.002737909 * time);

            // Adjust result to fall within range 0-24
            while (gst > 24) gst -= 24;
            while (gst < 0) gst += 24;
            return gst;
        }

        public static Double GetLocalSiderealTime(DateTime utc, Double longitude)
        {
            // Get Greenwich sideral time
            Double gst = GetGreenwichSiderealTime(utc);

            // Calculate local sidereal time and adjust result to fall within range 0-24
            Double lst = (gst + (longitude / 15));
            while (lst > 24) lst -= 24;
            while (lst < 0) lst += 24;
            return lst;
        }

        public static DateTime GetUniversalTime(DateTime date, Double gst)
        {
            // Get the Julian date at 0h and calculate Julian days since midday on 1/1/2000
            Double jd = GetJulianDate(date.Date);
            Double S = jd - 2451545;

            // Calculate universal time and adjust result to fall within range 0-24
            Double T = S / 36525;
            Double T0 = 6.697374558 + (2400.051336 * T) + (0.000025862 * T * T);
            while (T0 > 24) T0 -= 24;
            while (T0 < 0) T0 += 24;
            Double ut = gst - T0;
            while (ut > 24) ut -= 24;
            while (ut < 0) ut += 24;
            ut *= 0.9972695663;

            // Return date/time
            return new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, DateTimeKind.Utc).AddHours(ut);
        }

        public static Double GetHourAngle(Coordinates coordinates, DateTime utc, Double longitude)
        {
            // Calculate hour angle and adjust result to fall within range 0-24
            Double ha = GetLocalSiderealTime(utc, longitude) - coordinates.RightAscension;
            while (ha < -12) ha += 24;
            while (ha > 12) ha -= 24;
            return ha;
        }

        public static DateTime GetRiseTime(Coordinates coordinates, DateTime date, Double latitude, Double longitude)
        {
            // Convert angular values to radians
            Double ra = coordinates.RightAscension * Math.PI / 12;
            Double dec = coordinates.Declination * Math.PI / 180;
            Double lat = latitude * Math.PI / 180;
            Double v = ((Double)34 / 60) * Math.PI / 180;   // vertical shift due to atmospheric refration at horizon (34 arcmins)

            // Calculate hour angle when point crosses the horizon (return MaxValue if circumpolar and MinValue if never rises)
            Double cosH = -(Math.Sin(v) + (Math.Sin(lat) * Math.Sin(dec))) / (Math.Cos(lat) * Math.Cos(dec));
            if (cosH > 1)
                return DateTime.MinValue;
            else if (cosH < -1)
                return DateTime.MaxValue;
            Double H = Math.Acos(cosH) * 12 / Math.PI;

            // Convert hour angle to rise time
            Double lst = coordinates.RightAscension - H;
            Double gst = lst - (longitude / 15);
            return GetUniversalTime(date, gst);
        }

        public static DateTime GetTransitTime(Coordinates coordinates, DateTime date, Double longitude)
        {
            // Convert hour angle to rise time
            Double gst = coordinates.RightAscension - (longitude / 15);
            return GetUniversalTime(date, gst);
        }

        public static DateTime GetSetTime(Coordinates coordinates, DateTime date, Double latitude, Double longitude)
        {
            // Convert angular values to radians
            Double ra = coordinates.RightAscension * Math.PI / 12;
            Double dec = coordinates.Declination * Math.PI / 180;
            Double lat = latitude * Math.PI / 180;
            Double v = ((Double)34 / 60) * Math.PI / 180;   // vertical shift due to atmospheric refration at horizon (34 arcmins)

            // Calculate hour angle when point crosses the horizon (return MaxValue if circumpolar and MinValue if never rises)
            Double cosH = -(Math.Sin(v) + (Math.Sin(lat) * Math.Sin(dec))) / (Math.Cos(lat) * Math.Cos(dec));
            if (cosH > 1)
                return DateTime.MinValue;
            else if (cosH < -1)
                return DateTime.MaxValue;
            Double H = Math.Acos(cosH) * 12 / Math.PI;

            // Convert hour angle to rise time
            Double lst = coordinates.RightAscension + H;
            Double gst = lst - (longitude / 15);
            return GetUniversalTime(date, gst);
        }

        public static Coordinates GetSunPoisition(DateTime utc)
        {
            // Get angular motion since 2010 January 0.0 (assuming circular orbit)
            Int32 D = (utc.Date - new DateTime(2009, 12, 31)).Days;
            Double N = (360 / 365.242191) * D;

            // Adjust for eliptical orbit to get ecliptic longitude
            Double M = N + 279.557208 - 283.112438;
            while (M > 360) M -= 360;
            while (M < 0) M += 360;
            Double Ec = (360 / Math.PI) * 0.016705 * Math.Sin(M * Math.PI / 180);
            Double elng = N + Ec + 279.557208;
            while (elng > 360) elng -= 360;
            while (elng < 0) elng += 360;

            // Convert to equatorial coordinates (ecliptic latitude is zero)
            Double JD = GetJulianDate(utc.Date);
            Double MJD = JD - 2451545.0;
            Double T = MJD / 36525;
            Double DE = (46.815 * T) + (0.0006 * T * T) - (0.00181 * T * T * T);
            Double oblq = (23.439292 - (DE / 3600));
            elng = elng * Math.PI / 180;
            oblq = oblq * Math.PI / 180;
            Double ra = Math.Atan2(Math.Sin(elng) * Math.Cos(oblq), Math.Cos(elng)) * 12 / Math.PI;
            if (ra < 0) ra += 24;
            Double dec = Math.Asin(Math.Sin(oblq) * Math.Sin(elng)) * 180 / Math.PI;
            return new Coordinates(ra, dec);
        }

        public static DateTime GetSunrise(DateTime date, Double latitude, Double longitude)
        {
            return GetRiseTime(GetSunPoisition(date.AddHours(12)), date, latitude, longitude);
        }

        public static DateTime GetSunset(DateTime date, Double latitude, Double longitude)
        {
            return GetSetTime(GetSunPoisition(date.AddHours(12)), date, latitude, longitude);
        }

        public static Double GetAltitude(Coordinates coordinates, DateTime utc, Double latitude, Double longitude)
        {
            // All the following are in radians
            Double lst = GetLocalSiderealTime(utc, longitude) * Math.PI / 12;
            Double lat = latitude * Math.PI / 180;
            Double ra = coordinates.RightAscension * Math.PI / 12;
            Double dec = coordinates.Declination * Math.PI / 180;
            Double ha = lst - ra;
            Double alt = Math.Asin((Math.Sin(lat) * Math.Sin(dec)) + (Math.Cos(lat) * Math.Cos(dec) * Math.Cos(ha)));

            // Return altitude in degrees (-90 - 90)
            return alt * 180 / Math.PI;
        }

        public static Double GetAzimuth(Coordinates coordinates, DateTime utc, Double latitude, Double longitude)
        {
            // All the following are in radians
            Double lst = GetLocalSiderealTime(utc, longitude) * Math.PI / 12;
            Double lat = latitude * Math.PI / 180;
            Double ra = coordinates.RightAscension * Math.PI / 12;
            Double dec = coordinates.Declination * Math.PI / 180;
            Double ha = lst - ra;
            Double az = Math.Atan2(-Math.Sin(ha), (Math.Tan(dec) * Math.Cos(lat)) - (Math.Sin(lat) * Math.Cos(ha)));

            // Return azimuth in degrees (0 - 360)
            if (az < 0) az += 2 * Math.PI;
            return az * 180 / Math.PI;
        }
    }
}
