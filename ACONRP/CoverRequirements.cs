using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACONRP
{
    public class CoverRequirements
    {
        String day;
        List<Cover> cover;

        public CoverRequirements(String d, List<Cover> _cover)
        {
            day = d;
            cover = _cover;
        }

        public String getDay()
        {
            return day;
        }
        public List<Cover> getCoversList()
        {
            return cover;
        }

        public int getDailyCoverCount()
        {
            int dayNurseCover = 0;
            foreach (Cover c in cover)
            {
                dayNurseCover += c.getPrefNurses();
            }
            return dayNurseCover;

        }
        public override string ToString()
        {
            String outString = "Day : " + this.day + "\n";
            foreach (Cover c in this.cover)
            {
                outString += "Shift " + c.getShift().getType() + ", require " + c.getPrefNurses() + " nurses \n";
            }
            return outString += "\n";
        }


    }
}
