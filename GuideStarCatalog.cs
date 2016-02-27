using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using nom.tam.fits;

namespace AstroSimulator
{
    public class GuideStarCatalog
    {
        // Class methods
        static public Fits OpenGSCFile(String filename)
        {
            // Read compressed files into memory because the CSharpFITS library does not
            // handle compressed files very well because they do not support deferred input
            if (filename.EndsWith(".gz"))
            {
                FileStream fileStream = new FileStream(filename, FileMode.Open);
                GZipStream gzStream = new GZipStream(fileStream, CompressionMode.Decompress);
                MemoryStream memStream = new MemoryStream();
                gzStream.CopyTo(memStream);
                memStream.Seek(0, SeekOrigin.Begin);
                return new nom.tam.fits.Fits(memStream);
            }
            else
                return new nom.tam.fits.Fits(filename);
        }

        // Instance data
        private List<GSCRegion> _regions;

        // Constructors
        public GuideStarCatalog(String baseDirectory)
        {
            BaseDirectory = baseDirectory;
        }

        // Properties
        public String BaseDirectory { get; private set; }
        public List<GSCRegion> Regions
        {
            get
            {
                if (_regions == null)
                {
                    _regions = new List<GSCRegion>();
                    Fits regionsFits = GuideStarCatalog.OpenGSCFile(String.Format("{0}\\TABLES\\REGIONS.TBL", BaseDirectory));
                    AsciiTableHDU hdu = regionsFits.GetHDU(1) as AsciiTableHDU;
                    AsciiTable table = hdu.GetData() as AsciiTable;
                    for (Int32 i = 0; i < table.NRows; ++i)
                        _regions.Add(new GSCRegion(BaseDirectory, (Object[])table.GetRow(i)));
                }
                return _regions;
            }
        }

        // Methods
        public List<GSCRegion> GetRegions(GSCArea area)
        {
            List<GSCRegion> overlappingRegions = new List<GSCRegion>();
            foreach (GSCRegion region in Regions)
            {
                if (region.Overlaps(area))
                    overlappingRegions.Add(region);
            }
            return overlappingRegions;
        }

        public List<GSCStar> GetStars(GSCArea area, Double minMagnitude = -10, Double maxMagitude = 20)
        {
            List<GSCStar> stars = new List<GSCStar>();
            foreach (GSCRegion region in GetRegions(area))
            {
                foreach (GSCStar star in region.Stars)
                {
                    if (area.Contains(star.RightAscension, star.Declination) && (star.Magnitude >= minMagnitude) && (star.Magnitude <= maxMagitude))
                        stars.Add(star);
                }
            }
            return stars;
        }
    }
}
