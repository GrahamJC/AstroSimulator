using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace AstroSimulator
{
    public class CameraBase : ObservableObject
    {
        private Int32 _focalLength;
        public Int32 FocalLength
        {
            get { lock (this) { return _focalLength; } }
            set
            {
                Boolean changed = false;
                lock (this)
                {
                    changed = (value != _focalLength);
                    if (changed)
                        _focalLength = value;
                }
                if (changed)
                    NotifyPropertyChanged("FocalLength");
            }
        }

        private Int32 _pixelsX;
        public Int32 PixelsX
        {
            get { lock (this) { return _pixelsX; } }
            set
            {
                Boolean changed = false;
                lock (this)
                {
                    changed = (value != _pixelsX);
                    if (changed)
                        _pixelsX = value;
                }
                if (changed)
                    NotifyPropertyChanged("PixelsX");
            }
        }

        private Int32 _pixelsY;
        public Int32 PixelsY
        {
            get { lock (this) { return _pixelsY; } }
            set
            {
                Boolean changed = false;
                lock (this)
                {
                    changed = (value != _pixelsY);
                    if (changed)
                        _pixelsY = value;
                }
                if (changed)
                    NotifyPropertyChanged("PixelsY");
            }
        }

        private Double _pixelSize;
        public Double PixelSize
        {
            get { lock (this) { return _pixelSize; } }
            set
            {
                Boolean changed = false;
                lock (this)
                {
                    changed = (value != _pixelSize);
                    if (changed)
                        _pixelSize = value;
                }
                if (changed)
                    NotifyPropertyChanged("PixelSize");
            }
        }

        private Int32 _rotationAngle;
        public Int32 RotationAngle
        {
            get { lock (this) { return _rotationAngle; } }
            set
            {
                Boolean changed = false;
                lock (this)
                {
                    changed = (value != _rotationAngle);
                    if (changed)
                        _rotationAngle = value;
                }
                if (changed)
                    NotifyPropertyChanged("RotationAngle");
            }
        }

        private Int32 _binning = 1;
        public Int32 Binning
        {
            get { lock (this) { return _binning; } }
            set
            {
                Boolean changed = false;
                lock (this)
                {
                    changed = (value != _binning);
                    if (changed)
                        _binning = value;
                }
                if (changed)
                    NotifyPropertyChanged("Binning");
            }
        }

        private Int32 _subFrameX = 0;
        public Int32 SubFrameX
        {
            get { lock (this) { return _subFrameX; } }
            set
            {
                Boolean changed = false;
                lock (this)
                {
                    changed = (value != _subFrameX);
                    if (changed)
                        _subFrameX = value;
                }
                if (changed)
                    NotifyPropertyChanged("SubFrameX");
            }
        }

        private Int32 _subFrameY = 0;
        public Int32 SubFrameY
        {
            get { lock (this) { return _subFrameY; } }
            set
            {
                Boolean changed = false;
                lock (this)
                {
                    changed = (value != _subFrameY);
                    if (changed)
                        _subFrameY = value;
                }
                if (changed)
                    NotifyPropertyChanged("SubFrameY");
            }
        }

        private Int32 _subFrameWidth = 0;
        public Int32 SubFrameWidth
        {
            get { lock (this) { return _subFrameWidth; } }
            set
            {
                Boolean changed = false;
                lock (this)
                {
                    changed = (value != _subFrameWidth);
                    if (changed)
                        _subFrameWidth = value;
                }
                if (changed)
                    NotifyPropertyChanged("SubFrameWidth");
            }
        }

        private Int32 _subFrameHeight = 0;
        public Int32 SubFrameHeight
        {
            get { lock (this) { return _subFrameHeight; } }
            set
            {
                Boolean changed = false;
                lock (this)
                {
                    changed = (value != _subFrameHeight);
                    if (changed)
                        _subFrameHeight = value;
                }
                if (changed)
                    NotifyPropertyChanged("SubFrameHeight");
            }
        }

        private Double _ccdTemperature = 10;
        public Double CcdTemperature
        {
            get { lock (this) { return _ccdTemperature; } }
            set
            {
                Boolean changed = false;
                lock (this)
                {
                    changed = (value != _ccdTemperature);
                    if (changed)
                        _ccdTemperature = value;
                }
                if (changed)
                    NotifyPropertyChanged("CcdTemperature");
            }
        }

        private Double _setCcdTemperature = -20;
        public Double SetCcdTemperature
        {
            get { lock (this) { return _setCcdTemperature; } }
            set
            {
                Boolean changed = false;
                lock (this)
                {
                    changed = (value != _setCcdTemperature);
                    if ( changed )
                    {
                        _setCcdTemperature = value;
                        if ( CoolerOn )
                            CcdTemperature = _setCcdTemperature;
                    }
                }
                if (changed)
                    NotifyPropertyChanged("SetCcdTemperature");
            }
        }

        private Boolean _coolerOn = false;
        public Boolean CoolerOn
        {
            get { lock (this) { return _coolerOn; } }
            set
            {
                Boolean changed = false;
                lock (this)
                {
                    changed = (value != _coolerOn);
                    if ( changed )
                    {
                        _coolerOn = value;
                        CcdTemperature = ( _coolerOn ? SetCcdTemperature : 10 );
                    }
                }
                if (changed)
                    NotifyPropertyChanged("CoolerOn");
            }
        }

        // Constructor
        protected CameraBase(Int32 focalLength, Int32 pixelsX, Int32 pixelsY, Double pixelSize, Int32 rotationAngle)
        {
            _focalLength = focalLength;
            _pixelsX = pixelsX;
            _pixelsY = pixelsY;
            _pixelSize = pixelSize;
            _rotationAngle = rotationAngle;
        }

        // Methods
        private Double[,] GeneratePSF(Double fwhm, Double arcsecPerPixel, Double defocus)
        {
            // Generate Gaussian point spread function
            Double c = fwhm / (2 * Math.Sqrt(2 * Math.Log(2))) * Math.Pow(10, (defocus / 100));
            Int32 psfSize = (Int32)Math.Round(20 * (fwhm / arcsecPerPixel) * Math.Pow(10, (defocus / 100)));
            if ((psfSize % 2) == 0)
                ++psfSize;
            Double[,] psf = new Double[psfSize, psfSize];
            if (psfSize == 1)
                psf[0, 0] = 1;
            else
            {
                Int32 psfCenter = (psfSize - 1) / 2;
                Double psfTotal = 0;
                for (Int32 x = 0; x < psfSize; ++x)
                {
                    for (Int32 y = 0; y < psfSize; ++y)
                    {
                        Double z = Math.Sqrt(((x - psfCenter) * (x - psfCenter)) + ((y - psfCenter) * (y - psfCenter)));
                        Double psfValue;
                        if (defocus == 0)
                            psfValue = Math.Exp(-(z * z) / (2 * c * c));
                        else
                        {
                            Double z1 = Math.Abs(z - (defocus / arcsecPerPixel / 2));
                            Double z2 = Math.Abs(z + (defocus / arcsecPerPixel / 2));
                            psfValue = Math.Exp(-(z1 * z1) / (2 * c * c)) + Math.Exp(-(z2 * z2) / (2 * c * c));
                        }
                        psf[x, y] = psfValue;
                        psfTotal += psfValue;
                    }
                }
                for (Int32 x = 0; x < psfSize; ++x)
                    for (Int32 y = 0; y < psfSize; ++y)
                        psf[x, y] /= psfTotal;
            }
            return psf;
        }

        private void AddToPixel(Int32[,] imageData, Int32 x, Int32 y, Double value)
        {
            // Check that coordinates are within limits before trying to adjust the value
            if ((value > 0) && (x >= 0) && (x <= imageData.GetUpperBound(0)) && (y >= 0) & (y <= imageData.GetUpperBound(1)))
            {
                Int32 pixelValue = imageData[x, y] + (Int32)Math.Round(value);
                if (pixelValue > UInt16.MaxValue)
                    pixelValue = UInt16.MaxValue;
                imageData[x, y] = pixelValue;
            }
        }

        private void DrawStar(Int32[,] imageData, Double x, Double y, Double intensity, Double[,] psf)
        {
            // Get base coordinates and offsets
            Int32 x0 = (Int32)Math.Round(x);
            Double dx = x - x0;
            Int32 y0 = (Int32)Math.Round(y);
            Double dy = y - y0;

            // Draw star using specified point spread function
            Int32 psfSize = psf.GetLength(0);
            Int32 psfCenter = (psfSize - 1) / 2;
            for (Int32 psfY = 0; psfY < psfSize; ++psfY)
            {
                Int32 pixelY = y0 + psfY - ((psfSize - 1) / 2);
                for (Int32 psfX = 0; psfX < psfSize; ++psfX)
                {
                    Int32 pixelX = x0 + psfX - ((psfSize - 1) / 2);
                    Double pixelIntensity = intensity * psf[psfX, psfY];
                    if (dy < 0)
                    {
                        if (dx < 0)
                            AddToPixel(imageData, pixelX - 1, pixelY - 1, pixelIntensity * Math.Abs(dx) * Math.Abs(dy));
                        AddToPixel(imageData, pixelX, pixelY - 1, pixelIntensity * (1 - Math.Abs(dx)) * Math.Abs(dy));
                        if (dx > 0)
                            AddToPixel(imageData, pixelX + 1, pixelY - 1, pixelIntensity * Math.Abs(dx) * Math.Abs(dy));
                    }
                    if (dx < 0)
                        AddToPixel(imageData, pixelX - 1, pixelY, pixelIntensity * Math.Abs(dx) * (1 - Math.Abs(dy)));
                    AddToPixel(imageData, pixelX    , pixelY    , pixelIntensity * (1 - Math.Abs(dx)) * (1 - Math.Abs(dy)));
                    if (dx > 0)
                        AddToPixel(imageData, pixelX + 1, pixelY, pixelIntensity * Math.Abs(dx) * (1 - Math.Abs(dy)));
                    if (dy > 0)
                    {
                        if (dx < 0)
                            AddToPixel(imageData, pixelX - 1, pixelY + 1, pixelIntensity * Math.Abs(dx) * Math.Abs(dy));
                        AddToPixel(imageData, pixelX, pixelY + 1, pixelIntensity * (1 - Math.Abs(dx)) * Math.Abs(dy));
                        if (dx > 0)
                            AddToPixel(imageData, pixelX + 1, pixelY + 1, pixelIntensity * Math.Abs(dx) * Math.Abs(dy));
                    }
                }
            }
        }

        public Int32[,] SimulateStarField(Double raCenter, Double decCenter, Int32 pixelsX, Int32 pixelsY, Double arcsecPerPixel, Int32 rotationAngle, Double duration, Int32 binning, Double fwhm, Double defocus)
        {
            // Initialize image data wth background flux
            Int32[,] imageData = new int[pixelsX, pixelsY];
            Int32 background = (Int32)Math.Round(duration * 10);
            for ( Int32 x = 0; x < pixelsX; ++x )
            {
                for ( Int32 y = 0; y < pixelsY; ++y )
                {
                    imageData[ x, y ] = background;
                }
            }

            // Generate point spread function used to simulate stars
            Double[,] psf = GeneratePSF(fwhm, arcsecPerPixel, defocus);

            // Calculate bounding rectangle which is twice the size of the CCD to allow for rotation
            Double halfWidth = (pixelsX * arcsecPerPixel / 3600) * (Math.PI / 180);
            Double halfHeight = (pixelsY * arcsecPerPixel / 3600) * (Math.PI / 180);
            GSCArea area = new GSCArea((raCenter + Math.Atan2(-halfWidth, Math.Cos(decCenter))) * (180 / Math.PI) / 15
                                      , (raCenter + Math.Atan2(halfWidth, Math.Cos(decCenter))) * (180 / Math.PI) / 15
                                      , Math.Asin((Math.Sin(decCenter) - (halfHeight * Math.Cos(decCenter))) / Math.Sqrt(1 + (halfHeight * halfHeight))) * (180 / Math.PI)
                                      , Math.Asin((Math.Sin(decCenter) + (halfHeight * Math.Cos(decCenter))) / Math.Sqrt(1 + (halfHeight * halfHeight))) * (180 / Math.PI));

            // Get stars and draw them on the image (RA/Dec need to be in radians
            // for trig functions used to calculate standard co-ordinates)
            List<GSCStar> stars = ( (App)Application.Current ).GSC.GetStars( area );
            foreach (GSCStar star in stars) //((App)Application.Current).GSC.GetStars(area))
            {
                // Project star onto standard coordinate system with origin at centre
                Double raStar = star.RightAscension * 15 * Math.PI / 180;
                Double decStar = star.Declination * Math.PI / 180;
                Double xStd = (Math.Cos(decStar) * Math.Sin(raStar - raCenter)) / (Math.Cos(decCenter) * Math.Cos(decStar) * Math.Cos(raStar - raCenter) + Math.Sin(decCenter) * Math.Sin(decStar));
                Double yStd = (Math.Sin(decCenter) * Math.Cos(decStar) * Math.Cos(raStar - raCenter) - Math.Cos(decCenter) * Math.Sin(decStar)) / (Math.Cos(decCenter) * Math.Cos(decStar) * Math.Cos(raStar - raCenter) + Math.Sin(decCenter) * Math.Sin(decStar));

                // Convert standard coordinates to pixels
                Double xPix = xStd * (180 / Math.PI) * 3600 / arcsecPerPixel;
                Double yPix = yStd * (180 / Math.PI) * 3600 / arcsecPerPixel;

                // Adjust for rotation angle
                if (rotationAngle > 0)
                {
                    Double rads = rotationAngle * (Math.PI / 180);
                    Double xPix2 = (xPix * Math.Cos(rads)) + (yPix * Math.Sin(rads));
                    Double yPix2 = (-xPix * Math.Sin(rads)) + (yPix * Math.Cos(rads));
                    xPix = xPix2;
                    yPix = yPix2;
                }

                // Calculate simulated intensity and draw star using PSF
                Double intensity = Math.Round(150000000 / Math.Pow(2.512, star.Magnitude)) * duration * binning * binning;
                DrawStar(imageData, xPix + (pixelsX / 2), yPix + (pixelsY / 2), intensity, psf);
            }

            // Return simulated image
            return imageData;
        }
    }
}
