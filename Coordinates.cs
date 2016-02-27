using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AstroSimulator
{
    public class Coordinates
    {
        // Enumerations
        public enum CoordinatesType { J2000, JNow }

        // Properties
        public CoordinatesType Type { get; private set; }
        public Double RightAscension { get; private set; }
        public Double Declination { get; private set; }

        // Construction
        public Coordinates(CoordinatesType type, Double rightAscension, Double declination)
        {
            Type = type;
            RightAscension = rightAscension;
            Declination = declination;
        }

        public Coordinates(Double rightAscension, Double declination)
            : this(CoordinatesType.J2000, rightAscension, declination)
        {
        }

        public Coordinates()
            : this(0, 0)
        {
        }

        // Methods
        public Coordinates ToJ2000()
        {
            ASCOM.Astrometry.Transform.Transform transform = new ASCOM.Astrometry.Transform.Transform();
            switch (Type)
            {
                case CoordinatesType.J2000:
                    transform.SetJ2000(RightAscension, Declination);
                    break;
                case CoordinatesType.JNow:
                    transform.SetApparent(RightAscension, Declination);
                    break;
            }
            return new Coordinates(CoordinatesType.J2000, transform.RAJ2000, transform.DecJ2000);
        }

        public Coordinates ToJNow()
        {
            ASCOM.Astrometry.Transform.Transform transform = new ASCOM.Astrometry.Transform.Transform();
            switch (Type)
            {
                case CoordinatesType.J2000:
                    transform.SetJ2000(RightAscension, Declination);
                    break;
                case CoordinatesType.JNow:
                    transform.SetApparent(RightAscension, Declination);
                    break;
            }
            return new Coordinates(CoordinatesType.JNow, transform.RAApparent, transform.DECApparent);
        }

        public Coordinates Add(Double rightAscension, Double declination)
        {
            return new Coordinates(Type, RightAscension + rightAscension, Declination + declination);
        }

        public Coordinates Subtract(Double rightAscension, Double declination)
        {
            return new Coordinates(Type, RightAscension - rightAscension, Declination - declination);
        }
    }
}
