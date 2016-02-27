using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using nom.tam.fits;

namespace AstroSimulator
{
    public class GSCRegion
    {
        // Class data
        private class RegionInfo
        {
            public Int32 RegionNoMin { get; private set; }
            public Int32 RegionNoMax { get; private set; }
            public String Directory { get; private set; }

            public RegionInfo(Int32 regionNoMin, Int32 regionNoMax, String directory)
            {
                RegionNoMin = regionNoMin;
                RegionNoMax = regionNoMax;
                Directory = directory;
            }
        }

        static private readonly RegionInfo[] _RegionInfos = {
            new RegionInfo(0001, 0593, "N0000"),
            new RegionInfo(0594, 1177, "N0730"),
            new RegionInfo(1178, 1728, "N1500"),
            new RegionInfo(1729, 2258, "N2230"),
            new RegionInfo(2259, 2780, "N3000"),
            new RegionInfo(2781, 3245, "N3730"),
            new RegionInfo(3246, 3651, "N4500"),
            new RegionInfo(3652, 4013, "N5230"),
            new RegionInfo(4014, 4293, "N6000"),
            new RegionInfo(4294, 4491, "N6730"),
            new RegionInfo(4492, 4614, "N7500"),
            new RegionInfo(4615, 4662, "N8230"),
            new RegionInfo(4663, 5259, "S0000"),
            new RegionInfo(5260, 5837, "S0730"),
            new RegionInfo(5838, 6411, "S1500"),
            new RegionInfo(6412, 6988, "S2230"),
            new RegionInfo(6989, 7522, "S3000"),
            new RegionInfo(7523, 8021, "S3730"),
            new RegionInfo(8022, 8463, "S4500"),
            new RegionInfo(8464, 8839, "S5230"),
            new RegionInfo(8840, 9133, "S6000"),
            new RegionInfo(9134, 9345, "S6730"),
            new RegionInfo(9346, 9489, "S7500"),
            new RegionInfo(9490, 9537, "S8230")
        };

        // Instance data
        private String _regionDirectory;
        private List<GSCStar> _stars;

        // Constructors
        public GSCRegion(String baseDirectory, Object[] fitsData)
        {
            // Get details from FITS table
            RegionNo = ((Int32[])fitsData[0])[0];
            RightAscensionMin = ((Int32[])fitsData[1])[0] + (((Int32[])fitsData[2])[0] / 60.0) + (((Single[])fitsData[3])[0] / 3600);
            RightAscensionMax = ((Int32[])fitsData[4])[0] + (((Int32[])fitsData[5])[0] / 60.0) + (((Single[])fitsData[6])[0] / 3600);
            DeclinationMin = ((Int32[])fitsData[8])[0] + (((Single[])fitsData[9])[0] / 60);
            if (((String[])fitsData[7])[0] == "-")
                DeclinationMin = -DeclinationMin;
            DeclinationMax = ((Int32[])fitsData[11])[0] + (((Single[])fitsData[12])[0] / 60);
            if (((String[])fitsData[10])[0] == "-")
                DeclinationMax = -DeclinationMax;

            // Check that declination min/max are the right way around (the GSC tables have them reversed for
            // the southern hemisphere)
            if (DeclinationMin > DeclinationMax)
            {
                Double temp = DeclinationMin;
                DeclinationMin = DeclinationMax;
                DeclinationMax = temp;
            }

            // Lookup other details of the region
            foreach (RegionInfo regionInfo in _RegionInfos)
            {
                if ((RegionNo >= regionInfo.RegionNoMin) && (RegionNo <= regionInfo.RegionNoMax))
                {
                    _regionDirectory = String.Format("{0}\\gsc\\{1}", baseDirectory, regionInfo.Directory);
                    break;
                }
            }
        }

        // Properties
        public Int32 RegionNo { get; private set; }
        public Double RightAscensionMin { get; private set; }
        public Double RightAscensionMax { get; private set; }
        public Double DeclinationMin { get; private set; }
        public Double DeclinationMax { get; private set; }
        public List<GSCStar> Stars
        {
            get
            {
                // If we haven't already got the star data read it from the GSC file
                if (_stars == null)
                {
                    // 
                    _stars = new List<GSCStar>();
                    Fits starsFits = GuideStarCatalog.OpenGSCFile(String.Format("{0}\\{1:0000}.gsc.gz", _regionDirectory, RegionNo));
                    AsciiTableHDU hdu = starsFits.GetHDU(1) as AsciiTableHDU;
                    AsciiTable table = hdu.GetData() as AsciiTable;
                    for (Int32 i = 0; i < table.NRows; ++i)
                        _stars.Add(new GSCStar(this, (Object[])table.GetRow(i)));
                }
                return _stars;
            }
        }

        // Methods
        public Boolean Overlaps(GSCArea area)
        {
            return area.Overlaps(new GSCArea(RightAscensionMin, RightAscensionMax, DeclinationMin, DeclinationMax));
        }
    }
}
