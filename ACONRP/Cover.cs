using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACONRP
{
    public class Cover
    {
        Shift shift;
        int nursesCount = 0;
        public Cover(Shift _shift, int nCount)
        {
            shift = _shift;
            nursesCount = nCount;
        }

        public int getPrefNurses()
        {
            return nursesCount;
        }

        public Shift getShift()
        {
            return shift;
        }
    }
}
