using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace AstroSimulator
{
    public class Guider : CameraBase
    {
        // Construction
        public Guider()
            : base(900, 659, 494, 7.4, 0)
        {
        }

        // Methods
        public Int32[,] ExposeImage(Double duration)
        {
            // Get parameters required for image simulation
            Boolean isSubFrame = ((SubFrameWidth > 0) && (SubFrameHeight > 0));
            Int32 pixelsX = (isSubFrame ? SubFrameWidth : PixelsX / Binning);
            Int32 pixelsY = (isSubFrame ? SubFrameHeight : PixelsY / Binning);
            Double arcsecPerPixel = (206.625 / FocalLength) * PixelSize * Binning;

            // Get target coordinates from telescope and convert to J2000
            Telescope telescope = ((App)Application.Current).Telescope;
            Coordinates target = new Coordinates(Coordinates.CoordinatesType.JNow, telescope.ActualRightAscension, telescope.ActualDeclination).ToJ2000();

            // Get rotation angle and adjust for side of pier
            Int32 rotationAngle = RotationAngle;
            if (telescope.PierSide == ASCOM.DeviceInterface.PierSide.pierEast)
                rotationAngle = (rotationAngle + 180) % 360;

            // Calculate co-ordinates of image center allowing for random variation due to seeing
            Double seeing = ((App)Application.Current).SiteInformation.Seeing;
            Random random = new Random();
            Double centerRA = ((target.RightAscension * 15) + ((random.NextDouble() - 0.5) * seeing / 3600)) * (Math.PI / 180);
            Double centerDec = (target.Declination + ((random.NextDouble() - 0.5) * seeing / 3600)) * (Math.PI / 180);
            if (isSubFrame)
            {
                // Calculate pixel offset of center of subframe from center of CCD
                Double pixelOffsetX = SubFrameX + (SubFrameWidth / 2) - (PixelsX / Binning / 2);
                Double pixelOffsetY = SubFrameY + (SubFrameHeight / 2) - (PixelsY / Binning / 2);

                // Allow for rotation angle and convert to radians
                Double rads = rotationAngle * (Math.PI / 180);
                Double offsetX = ((pixelOffsetX * Math.Cos(rads)) + (pixelOffsetY * Math.Sin(rads))) * (arcsecPerPixel / 3600) * (Math.PI / 180);
                Double offsetY = ((-pixelOffsetX * Math.Sin(rads)) + (pixelOffsetY * Math.Cos(rads))) * (arcsecPerPixel / 3600) * (Math.PI / 180);

                // Adjust image center coordinates
                centerRA = centerRA + Math.Atan2(offsetX, Math.Cos(centerDec));
                centerDec = Math.Asin((Math.Sin(centerDec) - (offsetY * Math.Cos(centerDec))) / Math.Sqrt(1 + (offsetY * offsetY)));
            }

            // Simulate starfield
            return SimulateStarField(centerRA, centerDec, pixelsX, pixelsY, arcsecPerPixel, rotationAngle, duration, Binning, 2, 0);
        }
    }
}
