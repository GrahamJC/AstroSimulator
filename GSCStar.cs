using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AstroSimulator
{
    public class GSCStar
    {
        public GSCRegion Region { get; private set; }
        public Int32 ID { get; private set; }
        public Double RightAscension { get; private set; }
        public Double Declination { get; private set; }
        public Double PositionError { get; private set; }
        public Double Magnitude { get; private set; }
        public Double MagnitudeError { get; private set; }

        public GSCStar(GSCRegion region, Object[] fitsData)
        {
            // Save region
            Region = region;

            // Get details from FITS table
            ID = ((Int32[])fitsData[0])[0];
            RightAscension = ((Single[])fitsData[1])[0] / 15;
            Declination = ((Single[])fitsData[2])[0];
            PositionError = ((Single[])fitsData[3])[0];
            Magnitude = ((Single[])fitsData[4])[0];
            MagnitudeError = ((Single[])fitsData[5])[0];
        }
    }
}
