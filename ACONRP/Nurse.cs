using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACONRP
{
    public class Nurse
    {
        public int id;
        public Contract contract;
        public Shift shift;
        List<String> skills;
        List<int> freeDayRequirements = new List<int>();
        List<FreeShift> freeShiftRequirements = new List<FreeShift>();
        String name;

        public class FreeShift
        {
            int date;
            String shiftType;

            public FreeShift(int d, String st)
            {
                date = d;
                shiftType = st;
            }

            public int getDate()
            {
                return date;
            }

            public String getShiftType()
            {
                return shiftType;
            }

            public bool Contains(int date, String shift)
            {
                return date == this.date && shift.Equals(this.shiftType) ? true : false;
            }

            public override string ToString()
            {
                return "(" + date + ", " + shiftType + ")";
            }

            public override bool Equals(object obj)
            {
                bool isEqual = false;

                if (obj != null && obj.GetType() == typeof(FreeShift))
                {
                    isEqual = (this.date == ((FreeShift)obj).date && shiftType.Equals(((FreeShift)obj).shiftType));
                }

                return isEqual;
            }



            public override int GetHashCode()
            {
                return base.GetHashCode();
            }


        }

        public Nurse(int _id, Contract c, List<String> _skills)
        {
            this.id = _id;
            contract = c;
            skills = _skills;

        }

        public void addFreeDay(int day)
        {
            freeDayRequirements.Add(day);
        }

        public void addFreeShift(int d, String st)
        {
            freeShiftRequirements.Add(new FreeShift(d, st));
        }

        public Contract getContract()
        {
            return contract;
        }

        public List<int> getFreeDayRequirements()
        {
            return freeDayRequirements;
        }
        public List<FreeShift> getFreeShiftRequirements()
        {
            return freeShiftRequirements;
        }


        public String toString()
        {
            String outString = "Nurse Id: " + this.id + "\n" + "name : " + this.name + "\n" + "contract id: " + this.contract.getId() + "\n" + "skills : ";

            foreach (String s in this.skills)
            {
                outString += s + " ";
            }
            outString += "\n Free days: ";
            foreach (int d in this.freeDayRequirements)
            {
                outString += d + " ";
            }
            outString += "\n Free shifts: ";
            foreach (FreeShift fs in this.freeShiftRequirements)
            {
                outString += fs.getDate() + "-" + fs.getShiftType() + ", ";
            }

            return outString += "\n";
        }
    }
}
