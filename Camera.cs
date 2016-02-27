using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace AstroSimulator
{
    public class Camera : CameraBase
    {
        // Construction
        public Camera()
            : base(800, 3362, 2504, 5.4, 0)
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

            // Get coordinates (including any errors) from telescope and convert to J2000
            Telescope telescope = ((App)Application.Current).Telescope;
            Coordinates target = ( telescope.CoordinateType == ASCOM.DeviceInterface.EquatorialCoordinateType.equLocalTopocentric )
                                    ? new Coordinates(Coordinates.CoordinatesType.JNow, telescope.ActualRightAscension, telescope.ActualDeclination).ToJ2000()
                                    : new Coordinates(Coordinates.CoordinatesType.J2000, telescope.ActualRightAscension, telescope.ActualDeclination);

            // Get rotation angle (in radians) and adjust for side of pier
            Int32 rotationAngle = RotationAngle;
            if (telescope.PierSide == ASCOM.DeviceInterface.PierSide.pierEast)
                rotationAngle = (rotationAngle + 180) % 360;

            // Calculate co-ordinates (in radians) of image center allowing for random variation due to seeing
            Double seeing = ((App)Application.Current).SiteInformation.Seeing;
            Random random = new Random();
            Double centerRA = ((target.RightAscension * 15) + ((random.NextDouble() - 0.5) * seeing / 3600)) * (Math.PI / 180);
            Double centerDec = (target.Declination + ((random.NextDouble() - 0.5) * seeing / 3600)) * (Math.PI / 180);
            if (isSubFrame)
            {
                // Calculate pixel offset of center of subframe from center of CCD
                Double offsetXPix = SubFrameX + (SubFrameWidth / 2) - (PixelsX / Binning / 2);
                Double offsetYPix = SubFrameY + (SubFrameHeight / 2) - (PixelsY / Binning / 2);

                // Adjust for camera rotation (movement of the subframe center is in the opposite direction
                // so the rotation angle is reversed)
                if (rotationAngle != 0)
                {
                    Double rotationRads = -rotationAngle * Math.PI / 180;
                    Double offsetXPix2 = (offsetXPix * Math.Cos(rotationRads)) + (offsetYPix * Math.Sin(rotationRads));
                    Double offsetYPix2 = (-offsetXPix * Math.Sin(rotationRads)) + (offsetYPix * Math.Cos(rotationRads));
                    offsetXPix = offsetXPix2;
                    offsetYPix = offsetYPix2;
                }

                // Convert pixel coordinates to radians
                Double offsetXRad = offsetXPix * (arcsecPerPixel / 3600) * (Math.PI / 180);
                Double offsetYRad = offsetYPix * (arcsecPerPixel / 3600) * (Math.PI / 180);

                // Adjust image center coordinates
                centerRA = centerRA + Math.Atan2(offsetXRad, (Math.Cos(centerDec) - offsetYRad * Math.Sin(centerDec)));
                centerDec = Math.Asin((Math.Sin(centerDec) - (offsetYRad * Math.Cos(centerDec))) / Math.Sqrt(1 + (offsetXRad * offsetXRad) + (offsetYRad * offsetYRad)));
            }

            // Simulate starfield
            Double defocus = ((App)Application.Current).Focuser.Defocus;
            return SimulateStarField(centerRA, centerDec, pixelsX, pixelsY, arcsecPerPixel, rotationAngle, duration, Binning, 2, defocus);
        }
    }
}
