using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AstroSimulator
{
    public class GSCArea
    {
        // Properties
        public Double RightAscensionMin { get; private set; }
        public Double RightAscensionMax { get; private set; }
        public Double DeclinationMin { get; private set; }
        public Double DeclinationMax { get; private set; }

        // Constructors
        public GSCArea(Double rightAscensionMin, Double rightAscensionMax, Double declinationMin, Double declinationMax)
        {
            RightAscensionMin = rightAscensionMin;
            RightAscensionMax = rightAscensionMax;
            DeclinationMin = declinationMin;
            DeclinationMax = declinationMax;
        }

        // Methods
        public Boolean Contains(Double rightAscension, Double declination)
        {
            if (DeclinationMin > DeclinationMax)
                return false;
            else if (RightAscensionMin > RightAscensionMax)
                return ((rightAscension >= RightAscensionMin) && (rightAscension < 24) ||
                        (rightAscension >= 0) && (rightAscension <= RightAscensionMax)) &&
                       (declination >= DeclinationMin) && (declination <= DeclinationMax);
            else
                return (rightAscension >= RightAscensionMin) && (rightAscension <= RightAscensionMax) &&
                       (declination >= DeclinationMin) && (declination <= DeclinationMax);
        }

        public Boolean Overlaps(GSCArea area)
        {
            // Areas overlap if a vertex of one is contained within the other
            return Contains(area.RightAscensionMin, area.DeclinationMin) ||
                   Contains(area.RightAscensionMin, area.DeclinationMax) ||
                   Contains(area.RightAscensionMax, area.DeclinationMin) ||
                   Contains(area.RightAscensionMax, area.DeclinationMax) ||
                   area.Contains(RightAscensionMin, DeclinationMin) ||
                   area.Contains(RightAscensionMin, DeclinationMax) ||
                   area.Contains(RightAscensionMax, DeclinationMin) ||
                   area.Contains(RightAscensionMax, DeclinationMax);
        }
    }
}
