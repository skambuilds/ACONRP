using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACONRP
{
    public class AllocationVector :IComparable<Object>
    {

        Schedule schedule;
        ArrayList<Allocation> x;
        int fx;
        int[] softContraintsVolation;

        public AllocationVector(Schedule schedule)
        {
            this.schedule = schedule;
            softContraintsVolation = new int[Schedule.SOFTCONTRAINTS];
            Arrays.fill(softContraintsVolation, new Integer(0));
            x = new ArrayList<Allocation>();
            fx = 0;
        }

        public void addAllocation(int _n, int _d, String _s)
        {
            x.add(new Allocation(_n, _d, _s));
        }

        public void addAllocation(Allocation a)
        {
            x.add(a);
        }

        public void setFxWeight(int _fx)
        {
            fx = _fx;
        }

        public ArrayList<Allocation> getX()
        {
            return x;
        }

        public int getFxWeight()
        {
            return fx;
        }

        public void clear()
        {
            this.fx = 0;
            this.x.clear();
        }

        @Override
    public String toString()
        {
            String out = "[ ";
            for (int j = 0; j < x.size(); j++)
            {
			
			out += x.get(j).toString() + " , ";
                if (j > 0 && j % 5 == 4)
                {
				out += "\n";
                }
            }
		
		out += " | " + fx + " ]";
            return out;
        }

        @Override
    public int compareTo(Object o)
        {
            if (o instanceof AllocationVector) {
                AllocationVector a = ((AllocationVector)o);
                if (this.fx == a.fx)
                {
                    return 0;
                }
                else if (this.fx > a.fx)
                {
                    return 1;
                }
                else
                {
                    return -1;
                }
            }
            return 0;
        }

        /**
         * Creates one feasible rooster.
         */
        public void createFeasubleRooster()
        {
            //get vector of feasible solutions (AllocationVector)
            int period = schedule.period / 7; // week = 7
            for (int i = 0; i < period; i++)
            {
                for (int j = 0; j < 7; j++)
                {
                    ArrayList<Cover> coverList = schedule.getCoversList(j);
                    int nurseId = 0;
                    int[] randN = randNurses(schedule.getAllocCount(j), schedule.nursesCount);
                    for (int k = 0; k < coverList.size(); k++)
                    {
                        Cover c = coverList.get(k);
                        for (int l = 0; l < c.getPrefNurses(); l++)
                        {
                            int day = 7 * i + j; // week = 7 * period + day of week 
                            Allocation a = new Allocation(randN[nurseId], day, c.getShift().getType());
                            x.add(a);
                            nurseId++;
                        }
                    }
                }
            }
            return;
        }

        /**
         * 
         * @param allocationPosition
         * @param day
         * @param shift
         * @return feasible allocation for shift s, on day d. 
         */
        public Allocation getNextFeasibleAllocation(int allocationPosition, int day, String shift)
        {
            ArrayList<Integer> possibleNurses = new ArrayList<>();
            possibleNurses = initArray(possibleNurses, schedule.nursesCount);

            // iterate actual rooster x, and find all nurses scheduled at day "day"
            for (Allocation a : x)
            {
                if (day == a.d)
                {
                    possibleNurses.remove(Integer.valueOf(a.n));
                }
            }
            // get random nurse from array of possible nurses
            int randNurse = schedule.getRandNum(0, possibleNurses.size() - 1);
            int nurseId = possibleNurses.get(randNurse);
            // return new allocation
            return new Allocation(nurseId, day, shift);
        }

        /**
         * Check hard constraint H2 (one nurse has assigned only one shift per day).
         * H1 is assumed as feasible.
         * @return true/false
         */
        public boolean checkFeasibility()
        {
            Collections.sort(x);
            ArrayList<Integer> assignedNurses = new ArrayList<>();

            int day = 0;
            for (int i = 0; i < x.size(); i++)
            {
                Allocation a = x.get(i);
                // new day starts, clear assigned nurses
                if (a.d != day)
                {
                    assignedNurses.clear();
                    day = a.d;
                }
                //check if nurse is already assigned at some day.  
                if (assignedNurses.contains(a.n))
                {
                    System.out.println("Incorrcet rooster, nurse " + a.n + " in already assigned at day " + day);
                    return false;
                }
                else
                {
                    assignedNurses.add(a.n);
                }
            }
            return true;
        }

        /**
         * Check if new allocation can be add into rooster
         * @return true/false
         */
        public boolean willBeFeasible(Allocation alloc)
        {
            // sort
            Collections.sort(x);
            for (int i = 0; i < x.size(); i++)
            {
                Allocation a = x.get(i);
                // find same nurse assigned at same day
                if (a.d == alloc.d && alloc.n == a.n)
                {
                    return false;
                }
            }

            return true;
        }

        /**
         * For all Soft constraint count Gs(x). From Gs(x) count total evaluate function fx.
         * @param x : feasible rooster
         * @return fx value
         */
        protected int evaluateRooster()
        {
            // check S1,S2 constraint	
            getMaxMinAssignViolations();
            // check S3,S4 constraint
            getMaxMinConseqDayViolations();
            // check S5,S6 constraint
            getMaxMinFreeDayVolations();
            // check S7
            completeWeekends();
            // check S8
            identicalWeekends();
            // check S9
            twoFreeDaysAfterNight();
            // check S10, S12
            dayOffShiftOff();
            // S15
            unwantedPatterns();

            // this patterns don t need to be implemented, they are not tested in data sets
            // S11 S13 -- day on, shift on
            // S14 -- alternative skills 

            for (int i = 0; i < softContraintsVolation.length; i++)
            {
                //System.out.println("Total S" + i + " violation " + softContraintsVolation[i]);
                fx += softContraintsVolation[i];
            }
            return 0;
        }

        /**
         * Count soft constraints S1, S2.
         */
        private void getMaxMinAssignViolations()
        {
            // count Max,Min number of assigments
            // loop all nurses		
            for (int i = 0; i < schedule.nursesCount; i++)
            {
                Nurse n = schedule.getNurse(i);
                int maxAssigmentns = n.getContract().getMaxnumassignments();
                int minAssigmentns = n.getContract().getMinnumassignments();

                int scheduledAssigments = 0;

                //Count nurse assigned shifts
                for (Allocation a: x)
                {
                    if (a.n == n.id)
                    {
                        scheduledAssigments++;
                    }
                }
                // total cost for S1 constraint
                softContraintsVolation[0] += (n.getContract().getWeight1() * Math.max(scheduledAssigments - maxAssigmentns, 0));
                // total cost for S2 constraint
                softContraintsVolation[1] += (n.getContract().getWeight2() * Math.max(minAssigmentns - scheduledAssigments, 0));
            }
        }
        /**
         * Count soft constraints S3,S4. 
         */
        private void getMaxMinConseqDayViolations()
        {
            int[] consWorkDayOld = new int[schedule.nursesCount];
            Arrays.fill(consWorkDayOld, new Integer(0));
            int[] consWorkDayNew = new int[schedule.nursesCount];
            Arrays.fill(consWorkDayNew, new Integer(0));

            // vector of nurses total consecutive day 
            int[] consWorkTotal = new int[schedule.nursesCount];
            Arrays.fill(consWorkTotal, new Integer(0));
            // vector of nurses total consecutive day 
            int[] minConsWorkTotal = new int[schedule.nursesCount];
            Arrays.fill(minConsWorkTotal, new Integer(0));

            int day = 0;
            for (int i = 0; i < x.size(); i++)
            {
                Allocation a = x.get(i);

                if (a.d != day)
                {
                    day = a.d;
                    // at start of the day find minimal consecutive day violation
                    for (int j = 0; j < minConsWorkTotal.length; j++)
                    {
                        int mincwd = schedule.getNurse(j).getContract().getMinconsecutiveworkingdays();
                        if ((consWorkDayNew[j] == 0) && (consWorkDayOld[j] < mincwd))
                        {
                            minConsWorkTotal[j] += consWorkDayOld[j];
                        }
                    }
                    //count consecutive work days
                    consWorkDayOld = consWorkDayNew.clone();
                    Arrays.fill(consWorkDayNew, new Integer(0));
                }

                // add next working dayt to nurse a.n
                consWorkDayNew[a.n] = consWorkDayOld[a.n] + 1;
                int maxcwd = schedule.getNurse(a.n).getContract().getMaxconsecutiveworkingdays();
                if (consWorkDayNew[a.n] > maxcwd)
                {
                    consWorkTotal[a.n]++;
                }
            }

            //count total soft constraints
            for (int j = 0; j < minConsWorkTotal.length; j++)
            {
                softContraintsVolation[2] += consWorkTotal[j];
                softContraintsVolation[3] += minConsWorkTotal[j];
            }
        }

        /**
         * Count soft constraints S5, S6.
         */
        private void getMaxMinFreeDayVolations()
        {
            int[] freeWorkDaysOld = new int[schedule.nursesCount];
            Arrays.fill(freeWorkDaysOld, new Integer(0));
            int[] freeWorkingDaysNew = new int[schedule.nursesCount];
            Arrays.fill(freeWorkingDaysNew, new Integer(0));

            // vector of nurses with maximal and minimal free days in row
            int[] maxFreeDays = new int[schedule.nursesCount];
            Arrays.fill(maxFreeDays, new Integer(0));
            // vector of nurses total consecutive day 
            int[] minFreeDays = new int[schedule.nursesCount];
            Arrays.fill(minFreeDays, new Integer(0));

            ArrayList<Integer> freeNurses = new ArrayList<>();
            freeNurses = initArray(freeNurses, schedule.nursesCount);

            int day = 0;
            // loop feasible rooster represented like Allocation vector x
            for (int i = 0; i < x.size(); i++)
            {
                Allocation a = x.get(i);
                // at start of new day starts
                if (a.d != day)
                {
                    day = a.d;
                    // iterate all nurses that does not work previous day
                    for (int j = 0; j < freeNurses.size(); j++)
                    {
                        int nurse = freeNurses.get(j);
                        freeWorkingDaysNew[nurse] = freeWorkDaysOld[nurse] + 1;

                        // count maximum consecutive free days
                        int maxcfd = schedule.getNurse(nurse).getContract().getMaxconsecutivefreedays();
                        if (freeWorkingDaysNew[nurse] > maxcfd)
                        {
                            maxFreeDays[nurse]++;
                        }
                    }
                    //for all nurses
                    for (int j = 0; j < minFreeDays.length; j++)
                    {
                        // count minimum consecutive free days
                        int mincfd = schedule.getNurse(j).getContract().getMinconsecutivefreedays();
                        if ((freeWorkingDaysNew[j] == 0) && (freeWorkDaysOld[j] < mincfd))
                        {
                            minFreeDays[j] += freeWorkDaysOld[j];
                        }
                    }
                    // reinitialize nurses array
                    freeNurses = initArray(freeNurses, schedule.nursesCount);
                    // copy new array into old
                    freeWorkDaysOld = freeWorkingDaysNew.clone();
                    Arrays.fill(freeWorkingDaysNew, new Integer(0));
                }
                //remove nurse if has assigned shift
                freeNurses.remove(Integer.valueOf(a.n));
            }
            // count S6, S6 violation for all nurses
            for (int j = 0; j < maxFreeDays.length; j++)
            {
                softContraintsVolation[4] += maxFreeDays[j];
                softContraintsVolation[5] += minFreeDays[j];
            }
        }

        /**
         * Count soft constraints S7.
         */
        private void completeWeekends()
        {
            int[] workWeekends = new int[schedule.nursesCount];
            Arrays.fill(workWeekends, new Integer(0));
            // vector of penalty for nurses, when nurse work only one day of weekend 
            int[] completeWeekendsPenalty = new int[schedule.nursesCount];
            Arrays.fill(completeWeekendsPenalty, new Integer(0));

            for (int i = 0; i < x.size(); i++)
            {
                Allocation a = x.get(i);
                // find only Saturday or Sunday in rooster 
                if (a.d % 7 == 5 || a.d % 7 == 6)
                {
                    workWeekends[a.n] += 1;
                }
                else
                {
                    for (int j = 0; j < workWeekends.length; j++)
                    {
                        if (workWeekends[j] == 1)
                        {
                            completeWeekendsPenalty[j] += 1;
                        }
                    }
                    //new week started, set workWeekends to 0
                    Arrays.fill(workWeekends, new Integer(0));
                }
            }
            // count last weekend during scheduling period
            for (int j = 0; j < workWeekends.length; j++)
            {
                if (workWeekends[j] == 1)
                {
                    completeWeekendsPenalty[j] += 1;
                }
                softContraintsVolation[7] += completeWeekendsPenalty[j];
            }

        }

        /**
         * Count soft constraints S8.
         */
        private void identicalWeekends()
        {
            String[] workSaturady = new String[schedule.nursesCount];
            Arrays.fill(workSaturady, new String(""));
            String[] workSunday = new String[schedule.nursesCount];
            Arrays.fill(workSunday, new String(""));

            // vector of penalty for nurses, when nurse work only one day of weekend 
            int[] identicaleWeekendsPenalty = new int[schedule.nursesCount];
            Arrays.fill(identicaleWeekendsPenalty, new Integer(0));

            for (int i = 0; i < x.size(); i++)
            {
                Allocation a = x.get(i);
                // find only Saturday
                if (a.d % 7 == 5 || a.d % 7 == 6)
                {
                    workSaturady[a.n] = a.s;
                }
                // find only Sunday 
                if (a.d % 7 == 6)
                {
                    workSunday[a.n] = a.s;
                }
                // start of the new week
                if (a.d % 7 == 0)
                {
                    for (int j = 0; j < workSaturady.length; j++)
                    {
                        if (!(workSaturady[j].equals(workSunday[j])))
                        {
                            identicaleWeekendsPenalty[j] += 1;
                        }
                        workSaturady[j] = "";
                        workSunday[j] = "";
                    }
                }
            }
            // count total S8 violation penalty
            for (int j = 0; j < identicaleWeekendsPenalty.length; j++)
            {
                softContraintsVolation[8] += identicaleWeekendsPenalty[j];
            }
        }

        /**
         * Count soft constraints S9.
         */
        private void twoFreeDaysAfterNight()
        {
            int[] twoFreeDays = new int[schedule.nursesCount];
            Arrays.fill(twoFreeDays, new Integer(0));

            // vector of nurses with total working days after night shift 
            int[] workDayAfterNight = new int[schedule.nursesCount];
            Arrays.fill(workDayAfterNight, new Integer(0));

            // vector of nurses with total working days after night shift 
            int[] workDayAfterNightTotal = new int[schedule.nursesCount];
            Arrays.fill(workDayAfterNightTotal, new Integer(0));

            int day = 0;
            for (int i = 0; i < x.size(); i++)
            {
                Allocation a = x.get(i);
                // if day changes decrease remaining days
                if (a.d != day)
                {
                    day = a.d;
                    for (int j = 0; j < twoFreeDays.length; j++)
                    {
                        if (workDayAfterNight[j] > 0)
                        {
                            twoFreeDays[j] = 0;
                            workDayAfterNight[j] = 0;
                            workDayAfterNightTotal[j] += 1;
                        }
                        twoFreeDays[j] = Math.max(twoFreeDays[j] - 1, 0);
                    }
                }
                // check if nurse need free day
                if ((int)twoFreeDays[a.n] > 0)
                {
                    // increase S9 nurse violation
                    workDayAfterNight[a.n]++;
                }
                // if nurse a.n has assigned night shift
                if (a.s.equals("N"))
                {
                    // set signal that nurse need 2 free days
                    twoFreeDays[a.n] = 3;
                }
            }

            for (int j = 0; j < workDayAfterNightTotal.length; j++)
            {
                softContraintsVolation[9] += workDayAfterNightTotal[j];
            }
        }
        /**
         * Count soft constraints S15.
         */
        private void unwantedPatterns()
        {
            int[] uwPatternCounter = new int[schedule.nursesCount];
            // vector of nurses with total working days after night shift 
            int[] uwPatternVolation = new int[schedule.nursesCount];
            // vector of nurses with total working days after night shift 
            int[] uwPatternTotalVolation = new int[schedule.nursesCount];
            Arrays.fill(uwPatternTotalVolation, new Integer(0));
            //for day unwanted patterns
            for (int i = 0; i < schedule.getShiftPattern().size(); i++)
            {

                UnwantedPattern shiftPattern = schedule.getShiftPattern().get(0);
                int patternLen = shiftPattern.getPatterns().size();
                Arrays.fill(uwPatternCounter, new Integer(0));
                Arrays.fill(uwPatternVolation, new Integer(0));

                Pattern pattern;
                int day = 0;
                for (int j = 0; j < x.size(); j++)
                {
                    Allocation a = x.get(j);
                    if (a.d != day)
                    {
                        day = a.d;
                        for (int k = 0; k < uwPatternCounter.length; k++)
                        {
                            if (uwPatternVolation[k] > 0)
                            {
                                uwPatternCounter[k] = 0;
                                uwPatternVolation[k] = 0;
                                uwPatternTotalVolation[k] += 1;
                            }
                            uwPatternCounter[k] = Math.max(uwPatternCounter[k] - 1, 0);
                        }
                    }
                    // check unwanted pattern volation
                    if ((int)uwPatternCounter[a.n] > 0)
                    {
                        // increase S15 shift nurse violation
                        pattern = shiftPattern.getPatterns().get(uwPatternCounter[a.n]);
                        if (a.s.equals(pattern.getShiftType()))
                        {
                            uwPatternVolation[a.n]++;
                        }
                    }
                    // if nurse a.n has assigned unwanted shift
                    pattern = shiftPattern.getPatterns().get(0);
                    if (a.s.equals(pattern.getShiftType()))
                    {
                        // set signal that nurse need 2 free days
                        uwPatternCounter[a.n] = patternLen;
                    }
                }
                for (int j = 0; j < uwPatternTotalVolation.length; j++)
                {
                    softContraintsVolation[14] += uwPatternTotalVolation[j];
                }
            }
        }

        /**
         * Count soft constraints S10,12.
         */
        private void dayOffShiftOff()
        {
            int[] daysOffVolations = new int[schedule.nursesCount];
            Arrays.fill(daysOffVolations, new Integer(0));
            int[] shiftsOffVolations = new int[schedule.nursesCount];
            Arrays.fill(shiftsOffVolations, new Integer(0));

            Nurse tmpNurse;
            for (int i = 0; i < x.size(); i++)
            {
                Allocation a = x.get(i);

                tmpNurse = schedule.getNurse(a.n);
                if (tmpNurse.getFreeDayRequirements().contains(a.d))
                {
                    daysOffVolations[a.n]++;
                }
                FreeShift fs = tmpNurse.new FreeShift(a.d, a.s);
                if (tmpNurse.getFreeShiftRequirements().contains(fs))
                {
                    shiftsOffVolations[a.n]++;
                }
            }

            for (int j = 0; j < daysOffVolations.length; j++)
            {
                softContraintsVolation[10] += daysOffVolations[j];
                softContraintsVolation[12] += shiftsOffVolations[j];
            }
        }

        /**
         * Initialize array
         * @param list
         * @param size
         * @return
         */
        private ArrayList<Integer> initArray(ArrayList<Integer> list, int size)
        {

            list.clear();
            for (int i = 0; i < size; i++)
            {
                list.add(i);
            }
            return list;
        }

        /**
         * 
         * @param count of nurses
         * @param max number of nurses
         * @return array with nurses id
         */
        private int[] randNurses(int count, int max)
        {
            ArrayList<Integer> fullList = new ArrayList<>();
            for (int i = 0; i < max; ++i)
            {
                fullList.add(i);
            }

            int[] newfiled = new int[count];
            for (int i = 0; i < count; ++i)
            {
                int r = (int)(Math.random() * fullList.size());
                newfiled[i] = fullList.remove(r);
            }
            return newfiled;
        }

        /**
         * Select random nurse from list.
         * @param avaliableNurses : list of available 
         * @return nurse id
         */
        public int randNurse(ArrayList<Integer> avaliableNurses)
        {
            int index;
            index = schedule.getRandNum(0, avaliableNurses.size() - 1);
            return avaliableNurses.get(index);
        }

        /**
         * 
         * @param day: selected day
         * @return : list of nurses available for day. 
         */
        ArrayList<Integer> getAvaliableNurseList(int day)
        {
            ArrayList<Integer> avaliableNurses = new ArrayList<>();
            avaliableNurses = initArray(avaliableNurses, schedule.nursesCount);

            for (int i = 0; i < x.size(); i++)
            {
                Allocation a = x.get(i);
                // find working nurses at required day 
                if (a.d == day)
                {
                    avaliableNurses.remove(Integer.valueOf(a.n));
                }
            }
            return avaliableNurses;
        }
    }
}
