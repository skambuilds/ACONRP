using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACONRP.Instances
{
    public class UnwantedPattern
    {
        public List<Pattern> patterns { get; set; }

        public UnwantedPattern()
        {
            patterns = new List<Pattern>();
        }

        public void addPattern(Pattern p)
        {
            patterns.Add(p);
        }

        public List<Pattern> getPatterns()
        {
            return patterns;
        }
    }

    public class Pattern
    {
        String shiftType = "";
        String day = "";

        public Pattern(String _st, String _day)
        {
            shiftType = _st;
            day = _day;
        }
        public String getShiftType()
        {
            return shiftType;
        }
        public String getDay()
        {
            return day;
        }
        public void setDay(String day)
        {
            this.day = day;
        }
        public void setShiftType(String shiftType)
        {
            this.shiftType = shiftType;
        }

        public override string ToString()
        {
            return "[ " + this.day + ", " + this.shiftType + " ]";
        }
    }

    
}
