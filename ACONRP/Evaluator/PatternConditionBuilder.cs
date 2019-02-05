using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACONRP.Evaluator
{
    public class PatternConditionBuilder
    {
        public static List<Condition> BuildPatternCondition(int numUnits, int numDays, Contract contract, List<Pattern> patterns, Dictionary<string, int> shiftTypesDict, int numWeekends)
        {
            var unwantedPatterns = contract.UnwantedPatterns.Pattern;
            var unwantedContractPatterns = patterns.Where(pattern => unwantedPatterns.Contains(pattern.ID)).ToList();
            List<Condition> conditions = new List<Condition>(10);

            foreach (Pattern p in unwantedContractPatterns)
            {
                PatternEntry firstPE = p.PatternEntries.PatternEntry[0];
                PatternEntry secondPE = p.PatternEntries.PatternEntry[1];

                // ST - ?
                if (firstPE.ShiftType != "None" && firstPE.ShiftType != "Any")
                {
                    // ST-ST
                    if (secondPE.ShiftType != "None" && firstPE.ShiftType != "Any")
                    {
                        if (firstPE.Day == "Any")
                        {
                            conditions.AddRange(BuildSTSTST_Any(p, numUnits, numDays, shiftTypesDict, contract));
                        }
                        else
                        {
                            //conditions.addAll(buildSTSTST_Fixed(employee, p, sp));
                        }
                    }
                    // ST-W
                    else if (secondPE.ShiftType == "Any")
                    {
                        if (firstPE.Day == "Any") // this is any
                        {

                        }
                        else
                        {

                        }
                    }
                    // ST-F?
                    else if (secondPE.ShiftType == "None")
                    {
                        if (firstPE.Day == "Any") // this is any
                        {
                            //conditions.addAll(buildSTFF_Any(employee, p, sp));
                        }
                        else
                        {
                            //conditions.addAll(buildSTFF_Fixed(employee, p, sp));
                        }
                    }
                }
                // W - ?
                else if (firstPE.ShiftType == "Any")
                {
                    // W-ST
                    if (secondPE.ShiftType != "None" && secondPE.ShiftType != "Any")
                    {
                        if (firstPE.Day == "Any") // this is any
                        {

                        }
                        else
                        {

                        }
                    }
                    // W-W
                    else if (secondPE.ShiftType == "Any")
                    {
                        if (firstPE.Day == "Any") // this is any
                        {

                        }
                        else
                        {

                        }
                    }
                    // W-F?
                    else if (secondPE.ShiftType == "None")
                    {
                        if (firstPE.Day == "Any") // this is any
                        {
                            //conditions.addAll(buildWFF_Any(employee, p, sp));
                        }
                        else
                        {
                            //conditions.AddRange(buildWFF_Fixed(employee, p, sp));
                        }
                    }
                }
                // F - ?
                else if (firstPE.ShiftType == "None")
                {
                    // F-ST
                    if (secondPE.ShiftType != "Any" && secondPE.ShiftType != "None")
                    {
                        if (firstPE.Day == "Any") // this is any
                        {

                        }
                        else
                        {

                        }
                    }
                    // F-W
                    else if (secondPE.ShiftType == "Any")
                    {
                        if (firstPE.Day == "Any") // this is any
                        {
                            //conditions.addAll(buildFWW_Any(employee, p, sp));
                        }
                        else
                        {
                            conditions.AddRange(BuildFWW_Fixed(p, numUnits, numDays, shiftTypesDict, contract, numWeekends));
                        }
                    }
                    // F-F?
                    else if (secondPE.ShiftType == "None")
                    {
                        if (firstPE.Day == "Any") // this is any
                        {

                        }
                        else
                        {

                        }
                    }
                }
            }

            return conditions;
        }

        private static List<Condition> BuildSTSTST_Any(Pattern p, int numUnits, int numDays, Dictionary<string, int> shiftTypesDict, Contract contract)
        {
            PatternEntry first = p.PatternEntries.PatternEntry[0];
            int firstSt = -1;
            bool result = shiftTypesDict.TryGetValue(first.ShiftType, out firstSt);
            //ShiftType firstSt = sp.shiftTypes.get(first.ShiftType);

            int numSt = numUnits;
            int nDays = numDays;

            String patternStr = first.ShiftType;

            for (int i = 1; i != p.PatternEntries.PatternEntry.Count; i++)
            {
                patternStr += ("-" + p.PatternEntries.PatternEntry[i].ShiftType);
            }

            int nbNumbs = p.PatternEntries.PatternEntry.Count;

            List<Condition> conds = new List<Condition>(nbNumbs);

            for (int w = 0; w != nbNumbs; w++)
            {
                Condition c = new PatternCondition(numUnits, numDays, contract);

                // start day is w
                // next start is w + length of pattern

                int counter = 0;

                for (int j = 0; j != CalculateNrOfFills(nDays - w, nbNumbs); j++)
                {
                    int dayPos = w + (j * nbNumbs);
                    //if (firstSt == 1)
                    //{
                    //    firstSt = 2;
                    //}
                    //else if (firstSt == 2) firstSt = 1;
                    int pos = (dayPos * numSt) + firstSt;//firstSt viene conteggiato male

                    c.GetNumbering()[pos] = counter;

                    for (int i = 1; i != p.PatternEntries.PatternEntry.Count; i++)
                    {
                        int index = -1;
                        PatternEntry pe = p.PatternEntries.PatternEntry[i];
                        bool resulto = shiftTypesDict.TryGetValue(pe.ShiftType, out index);
                        //if (index == 1)
                        //{
                        //    index = 2;
                        //}
                        //else if (index == 2) index = 1;
                        pos = ((dayPos + i) * numSt) + index;
                        c.GetNumbering()[pos] = ++counter;

                        //if (j == 0) patternStr += ("-" + st.label);
                    }

                    counter += 2;
                }

                // set cons or between!!!!
                c.max_consecutive = p.PatternEntries.PatternEntry.Count - 1;
                c.cost_max_consecutive = Convert.ToInt16(p.Weight);
                c.max_consecutive_string = "Unwanted pattern: " + patternStr;

                conds.Add(c);
            }

            return conds;
        }

        public static List<Condition> BuildFWW_Fixed(Pattern p, int numUnits, int numDays, Dictionary<string, int> shiftTypesDict, Contract contract, int numWeekends)
        {
            List<Condition> conds = new List<Condition>(10);

            PatternEntry first = p.PatternEntries.PatternEntry[0];

            int numSt = numUnits;

            String patternStr = "";

            if (first.ShiftType == "Any")
            {
                patternStr += "Any";
            }
            else
            {
                patternStr += "None";
            }

            patternStr += "(" + first.Day + ")";

            for (int i = 1; i != p.PatternEntries.PatternEntry.Count; i++)
            {
                String pStr = "";
                if (p.PatternEntries.PatternEntry[i].ShiftType == "Any")
                {
                    pStr = "Any";
                }
                else
                {
                    pStr += "None";
                }

                pStr += "(" + p.PatternEntries.PatternEntry[i].Day + ")";

                patternStr += ("-" + pStr);
            }

            int dayPosFirst = -1;

            // be sure that correctly initialized
            //sp.CalculateFirstDays();

            if (first.Day == "Monday")
            {
                dayPosFirst = 3;
            }
            else if (first.Day == "Tuesday")
            {
                dayPosFirst = 4;
            }
            else if (first.Day == "Wednesday")
            {
                dayPosFirst = 5;
            }
            else if (first.Day == "Thursday")
            {
                dayPosFirst = 6;
            }
            else if (first.Day == "Friday")
            {
                dayPosFirst = 0;
            }
            else if (first.Day == "Saturday")
            {
                dayPosFirst = 1;
            }
            else if (first.Day == "Sunday")
            {
                dayPosFirst = 2;
            }

            for (int j = 0; j != numWeekends; j++)
            {
                // check if last index is still in planning period
                int dayPos = dayPosFirst + (j * 7);

                if (dayPos >= numDays) continue;

                Condition c = new PatternCondition(numUnits, numDays, contract);
                c.last_nr_history = 0;


                int pos = (dayPos * numSt);

                for (int k = 0; k != numSt; k++)
                {
                    pos = (dayPos * numSt) + k;
                    c.GetNumbering()[pos] = 3;
                }

                for (int i = 1; i != p.PatternEntries.PatternEntry.Count; i++)
                {
                    for (int k = 0; k != numSt; k++)
                    {
                        pos = ((dayPos + i) * numSt) + k;
                        c.GetNumbering()[pos] = 4;
                    }
                }

                c.max_between = 2;
                c.cost_max_between = Convert.ToInt16(p.Weight);
                c.max_between_string = "Unwanted pattern: " + patternStr;

                conds.Add(c);
            }
            return conds;
        }

        private static int CalculateNrOfFills(int nrOfDays, int patternLength)
        {
            return nrOfDays / patternLength;
        }
    }
}