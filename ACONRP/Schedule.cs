using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACONRP.Instances
{
    public class Schedule
    {

        public const int SOFTCONTRAINTS = 15;
        private Random rand;

        String start = "";
        String end = "";

        List<String> skillsTypes = new List<String>();
        List<Shift> shiftsTypes = new List<Shift>();
        List<Contract> contractsTypes = new List<Contract>();
        List<UnwantedPattern> dayPatterns = new List<UnwantedPattern>();
        List<UnwantedPattern> shiftPatterns = new List<UnwantedPattern>();

        List<Nurse> nurses = new List<Nurse>();
        // nurse requirements for specific day or shift
        List<CoverRequirements> weeklyCoverRequirements = new List<CoverRequirements>();

        AllocationVector solution = null;
        // schedule variables	
        public int nursesCount = 0;
        // number of different shifts
        public int shiftTypeCount = 0;
        // scheduling period in days (example 0-28)
        public int period = 0;
        // total shifts type in period 
        public int allocationCount = 0;

        public Schedule(String _start, String _end)
        {
            start = _start;
            end = _end;
        }

        public void initialize()
        {
            nursesCount = nurses.Count;
            shiftTypeCount = shiftsTypes.Count;
            //period = 7;
            period = covertDateToInt(start, end);
            allocationCount = getAllocCount();

            // create Random generator
            int currentTime = DateTime.Now.Millisecond;
            rand = new Random(currentTime);

        }

        public int covertDateToInt(String sDate, String eDate)
        {
            //Date df = new SimpleDateFormat("yyyy-MM-dd");
            DateTime startDate;
            DateTime endDate;
            try
            {
                startDate = df.parse(sDate);
                endDate = df.parse(eDate);
                int days = (int)Math.Abs((startDate.getTime() - endDate.getTime()) / 86400000);
                return days + 1;
            }
            catch (Exception e)
            {

            }

            return -1;
        }

        public String createDate(int dateIndex)
        {
            if (dateIndex < 9)
                return "2010-01-0" + (dateIndex + 1);
            else
                return "2010-01-" + (dateIndex + 1);
        }

        public void AddSkill(String s)
        {
            skillsTypes.Add(s);
        }

        public void AddNurse(Nurse n)
        {
            nurses.Add(n);
        }

        public void AddShift(Shift s)
        {
            shiftsTypes.Add(s);
        }

        public void AddContract(Contract c)
        {
            contractsTypes.Add(c);
        }

        public void AddCoverRequirements(List<CoverRequirements> _week)
        {
            weeklyCoverRequirements = _week;
        }

        public Contract getContract(int contractID)
        {
            return contractsTypes.FirstOrDefault(x=> x.getId() == contractID);
        }

        public Shift getShift(String type)
        {
            foreach (Shift s in shiftsTypes)
            {
                if (s.getType().Equals(type))
                {
                    return s;
                }
            }
            return null;
        }

        public Nurse getNurse(int index)
        {
            return nurses.ElementAt(index);
        }

        public void AddNurseFreeDay(int nurseId, String day)
        {
            nurses.First(x => x.id == nurseId).addFreeDay(covertDateToInt(start, day));
        }

        public void AddNurseFreeShift(int nurseId, String day, String st)
        {
            nurses.First(x => x.id == nurseId).addFreeShift(covertDateToInt(start, day), st);
        }

        /**
         * Returns total nurses count for all period.
         * */
        public int getAllocCount()
        {
            int ac = 0;
            //get count for period selected by xml Cover requirements
            foreach (CoverRequirements cr in weeklyCoverRequirements)
            {
                ac += cr.getDailyCoverCount();
            }
            // count full period interval
            int i = period / weeklyCoverRequirements.Count; // prediod/7 || 28/7 = 4 
            ac *= i;
            return ac;
        }

        /**
         * 
         * @param day
         * @return Allocation count at all days day
         */
        public int getAllocCount(String day)
        {
            int ac = 0;
            //get count for period selected by xml Cover requirements
            foreach (CoverRequirements cr in weeklyCoverRequirements)
            {
                if (cr.getDay().Equals(day))
                {
                    ac += cr.getDailyCoverCount();
                    break;
                }
            }
            return ac;
        }

        /**
         * 
         * @param i
         * @return Allocation count at day i
         */
        public int getAllocCount(int i)
        {
            int ac = 0;
            //get count for day i
            CoverRequirements cr = weeklyCoverRequirements.ElementAt(i);
            ac += cr.getDailyCoverCount();

            return ac;
        }

        public List<Cover> getCoversList(int i)
        {
            //get covers for day i
            CoverRequirements cr = weeklyCoverRequirements.ElementAt(i);
            return cr.getCoversList();
        }

        public int getRandNum(int min, int max)
        {
            int result = rand.Next((max - min) + 1) + min;
            return result;
        }

        public double getRandNum()
        {
            double result = rand.NextDouble();
            return result;
        }

        public void AddShiftPattern(UnwantedPattern p)
        {
            shiftPatterns.Add(p);
        }

        public List<UnwantedPattern> getShiftPattern()
        {
            return shiftPatterns;
        }

        public void AddDayPattern(UnwantedPattern p)
        {
            dayPatterns.Add(p);
        }

        public List<UnwantedPattern> getDayPattern()
        {
            return dayPatterns;
        }

        public void setSolution(AllocationVector solution)
        {
            this.solution = solution;
        }

        public AllocationVector getSolution()
        {
            return solution;
        }
    }
}
}
