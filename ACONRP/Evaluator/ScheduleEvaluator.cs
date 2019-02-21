using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACONRP.Evaluator
{
    public class ScheduleEvaluator
    {
        private int origPenalty = 0;
        private int[] constraintViolationsOnDay;
        private int numberOfConditions;
        private int nrOfDaysInPeriod;
        int nrShiftTypes;
        int numTimeUnits;
        private int penalty;
        private int[] lastNr;
        private int[] lastNrPos;
        private int[] total;
        private int[] consecutive;
        private int[][] pertNr;
        private int[] rightSpot;
        private int[] currentNr;
        private int[] lastNrIni;
        private int[] consecutiveIni;
        private bool[] shiftPatternEvent;
        SchedulingPeriod sp;

        //Constructor
        public ScheduleEvaluator(SchedulingPeriod inputData)
        {
            sp = inputData;
            nrShiftTypes = sp.ShiftTypes.Shift.Count;
            DateTime startDate = Convert.ToDateTime(sp.StartDate);
            DateTime endDate = Convert.ToDateTime(sp.EndDate);
            nrOfDaysInPeriod = endDate.Day - startDate.Day + 1;
            numTimeUnits = nrOfDaysInPeriod * nrShiftTypes;
        }

        //Initialization
        public void InitialiseEvaluation(Node nodeToEvaluate, List<Condition> conditions)
        {
            numberOfConditions = conditions.Count;
            origPenalty = nodeToEvaluate.Penalty;
            penalty = 0;
            if (currentNr == null) currentNr = new int[numberOfConditions];
            if (lastNr == null) lastNr = new int[numberOfConditions];
            if (lastNrPos == null) lastNrPos = new int[numberOfConditions];
            if (lastNrIni == null) lastNrIni = new int[numberOfConditions];
            if (consecutive == null) consecutive = new int[numberOfConditions];
            if (consecutiveIni == null) consecutiveIni = new int[numberOfConditions];
            if (total == null) total = new int[numberOfConditions];
            if (rightSpot == null) rightSpot = new int[numberOfConditions];
            if (pertNr == null) pertNr = new int[numberOfConditions][];
            if (shiftPatternEvent == null) shiftPatternEvent = new bool[numberOfConditions];
            constraintViolationsOnDay = new int[nrOfDaysInPeriod];

            for (int iConditions = 0; iConditions != numberOfConditions; iConditions++)
            {
                Condition curCondition = conditions[iConditions];

                curCondition.ResetCost();

                consecutive[iConditions] = 1;
                lastNr[iConditions] = curCondition.last_nr_history;
                shiftPatternEvent[iConditions] = false;
                pertNr[iConditions] = new int[curCondition.max_pert_nr.Length];
                total[iConditions] = 0;
            }
        }


        //Intermediate Evaluation
        public void IntermediateEvaluation(Node nodeToEvaluate, List<Condition> conditions, bool trackViolations)
        {
            //Swap from Matrix data structure to simple array data structure
            bool[] rooster = new bool[numTimeUnits];

            if (nodeToEvaluate.ShiftPatternArray != null) rooster = nodeToEvaluate.ShiftPatternArray;
            else MatrixToArrayConverter(nodeToEvaluate.ShiftPatternMatrix, rooster);

            for (int iConditions = 0; iConditions != numberOfConditions; iConditions++)
            {
                for (int iRooster = 0; iRooster != rooster.Length; iRooster++)
                {
                    if (rooster[iRooster])
                    {
                        int nr = conditions[iConditions].GetNumbering()[iRooster]; //Returns the numbering of the iRooster day associated to the current condition 
                        if (nr != Condition.Undefined)
                        {
                            shiftPatternEvent[iConditions] = true;
                            total[iConditions] = total[iConditions] + 1;
                            if (nr == lastNr[iConditions] + 1)
                            {
                                consecutive[iConditions] += 1;
                            }
                            else if (nr > lastNr[iConditions] + 1)
                            {
                                // 4 CASES
                                Condition curCondition = conditions[iConditions];
                                if (consecutive[iConditions] > 0 && consecutive[iConditions] < curCondition.min_consecutive)
                                {
                                    int cost = curCondition.cost_min_consecutive;
                                    if (cost > 0)
                                    {
                                        int val = curCondition.min_consecutive - consecutive[iConditions];
                                        int pen = (cost * val);
                                        curCondition.penalty_min_consecutive += pen;
                                        Violation violation = new Violation(nodeToEvaluate.NurseId, nodeToEvaluate.Index, "MIN_CONSECUTIVE", val, curCondition, lastNrPos[iConditions], pen);
                                        if (trackViolations) nodeToEvaluate.Violations.Add(violation.ToString());
                                        int day = iRooster / nrShiftTypes;
                                        constraintViolationsOnDay[day] += pen;
                                        penalty += pen;
                                    }
                                }
                                if (consecutive[iConditions] > curCondition.max_consecutive)
                                {
                                    int cost = curCondition.cost_max_consecutive;
                                    if (cost > 0)
                                    {
                                        int val = consecutive[iConditions] - curCondition.max_consecutive;
                                        int pen = (cost * val);
                                        curCondition.penalty_max_consecutive += pen;
                                        // create and add violation
                                        Violation violation = new Violation(nodeToEvaluate.NurseId, nodeToEvaluate.Index, "MAX_CONSECUTIVE", val, curCondition, lastNrPos[iConditions], pen);
                                        if (trackViolations) nodeToEvaluate.Violations.Add(violation.ToString());
                                        int day = iRooster / nrShiftTypes;
                                        this.constraintViolationsOnDay[day] += pen;
                                        this.penalty += pen;
                                    }
                                }
                                if (nr - lastNr[iConditions] - 1 < curCondition.min_between)
                                {
                                    int cost = curCondition.cost_min_between;
                                    if (cost > 0)
                                    {
                                        int val = curCondition.min_between - (nr - lastNr[iConditions] - 1);
                                        int pen = (cost * val);
                                        curCondition.penalty_min_between += pen;
                                        // create and add violation
                                        Violation violation = new Violation(nodeToEvaluate.NurseId, nodeToEvaluate.Index, "MIN_BETWEEN", val, curCondition, lastNrPos[iConditions], pen);
                                        if (trackViolations) nodeToEvaluate.Violations.Add(violation.ToString());
                                        int day = iRooster / nrShiftTypes;
                                        this.constraintViolationsOnDay[day] += pen;
                                        this.penalty += pen;
                                    }
                                }
                                if (nr - lastNr[iConditions] - 1 > curCondition.max_between)
                                {
                                    int cost = curCondition.cost_max_between;
                                    if (cost > 0)
                                    {
                                        int val = (nr - lastNr[iConditions] - 1) - curCondition.max_between;
                                        int pen = (cost * val);
                                        curCondition.penalty_max_between += pen;
                                        Violation violation = new Violation(nodeToEvaluate.NurseId, nodeToEvaluate.Index, "MAX_BETWEEN", val, curCondition, iRooster, pen);
                                        if (trackViolations) nodeToEvaluate.Violations.Add(violation.ToString());
                                        int day = iRooster / nrShiftTypes;
                                        this.constraintViolationsOnDay[day] += pen;
                                        this.penalty += pen;
                                    }
                                }
                                consecutive[iConditions] = 1;
                            }
                            pertNr[iConditions][nr] += 1;
                            lastNr[iConditions] = nr;
                            lastNrPos[iConditions] = iRooster;
                        }
                    }
                }
            }

        }

        private static void MatrixToArrayConverter(bool[,] shiftPatternMatrix, bool[] rooster)
        {
            int k = 0;
            for (int i = 0; i != shiftPatternMatrix.GetLength(1); i++)
            {
                for (int j = 0; j != shiftPatternMatrix.GetLength(0); j++)
                {
                    rooster[k] = shiftPatternMatrix[j, i];
                    k++;
                }
            }
        }


        //Final Evaluation
        public void FinalEvaluation(Node nodeToEvaluate, List<Condition> conditions, bool trackViolations)
        {
            for (int iConditions = 0; iConditions != numberOfConditions; iConditions++)
            {
                int lastPosition = numTimeUnits - 1;
                Condition curCondition = conditions[iConditions];
                if (total[iConditions] > curCondition.max_total)
                {
                    int cost = curCondition.cost_max_total;
                    if (cost > 0)
                    {
                        int val = total[iConditions] - curCondition.max_total;
                        int pen = (cost * val);
                        curCondition.penalty_max_total += pen;
                        // create and add violation
                        Violation violation = new Violation(nodeToEvaluate.NurseId, nodeToEvaluate.Index, "MAX_TOTAL", val, curCondition, lastPosition, pen);
                        if (trackViolations) nodeToEvaluate.Violations.Add(violation.ToString());
                        int day = nrOfDaysInPeriod - 1;//?same value every time?                                 
                        this.constraintViolationsOnDay[day] += pen;
                        this.penalty += pen;
                    }
                }
                if (total[iConditions] < curCondition.min_total)
                {
                    int cost = curCondition.cost_min_total;
                    if (cost > 0)
                    {
                        int val = curCondition.min_total - total[iConditions];
                        int pen = (cost * val);
                        curCondition.penalty_min_total += pen;
                        // create and add violation
                        Violation violation = new Violation(nodeToEvaluate.NurseId, nodeToEvaluate.Index, "MIN_TOTAL", val, curCondition, lastPosition, pen);
                        if (trackViolations) nodeToEvaluate.Violations.Add(violation.ToString());
                        int day = nrOfDaysInPeriod - 1;
                        this.constraintViolationsOnDay[day] += pen;
                        this.penalty += pen;
                    }
                }
                if (curCondition.future_nr != Condition.Undefined)
                {
                    if (lastNr[iConditions] == curCondition.future_nr - 1)
                    {
                        consecutive[iConditions] += 1;
                    }
                }
                if (shiftPatternEvent[iConditions])
                {
                    if (consecutive[iConditions] > curCondition.max_consecutive)
                    {
                        int cost = curCondition.cost_max_consecutive;
                        if (cost > 0)
                        {
                            int val = consecutive[iConditions] - curCondition.max_consecutive;
                            int pen = (cost * val);
                            curCondition.penalty_max_consecutive += pen;
                            // create and add violation
                            Violation violation = new Violation(nodeToEvaluate.NurseId, nodeToEvaluate.Index, "MAX_CONSECUTIVE", val, curCondition, lastPosition, pen);
                            if (trackViolations) nodeToEvaluate.Violations.Add(violation.ToString());
                            int day = nrOfDaysInPeriod - 1;
                            this.constraintViolationsOnDay[day] += pen;
                            this.penalty += pen;
                        }
                    }
                }
                if (lastNr[iConditions] != Condition.Undefined && consecutive[iConditions] < curCondition.min_consecutive)
                {
                    int cost = curCondition.cost_min_consecutive;
                    if (cost > 0)
                    {
                        int val = curCondition.min_consecutive - consecutive[iConditions];
                        int pen = (cost * val);
                        curCondition.penalty_min_consecutive += pen;
                        // create and add violation
                        Violation violation = new Violation(nodeToEvaluate.NurseId, nodeToEvaluate.Index, "MIN_CONSECUTIVE", val, curCondition, lastPosition, pen);
                        if (trackViolations) nodeToEvaluate.Violations.Add(violation.ToString());
                        int day = nrOfDaysInPeriod - 1;
                        this.constraintViolationsOnDay[day] += pen;
                        this.penalty += pen;
                    }
                }
                // max_between
                if (lastNr[iConditions] != Condition.Undefined && curCondition.future_nr != Condition.Undefined)
                {
                    int cost = curCondition.cost_max_between;

                    if (cost > 0)
                    {
                        int val = curCondition.future_nr - lastNr[iConditions] - 1;

                        if (val > curCondition.max_between)
                        {
                            val = val - curCondition.max_between;

                            int pen = (cost * val);
                            curCondition.penalty_max_between += pen;

                            // create and add violation
                            Violation violation = new Violation(nodeToEvaluate.NurseId, nodeToEvaluate.Index, "MAX_BETWEEN", val, curCondition, lastPosition, pen);
                            if (trackViolations) nodeToEvaluate.Violations.Add(violation.ToString());
                            int day = nrOfDaysInPeriod - 1;
                            this.constraintViolationsOnDay[day] += pen;
                            this.penalty += pen;
                        }
                    }
                }
                // min_between
                if (lastNr[iConditions] != Condition.Undefined && curCondition.future_nr != Condition.Undefined)
                {
                    int cost = curCondition.cost_min_between;

                    if (cost > 0)
                    {
                        int val = curCondition.future_nr - lastNr[iConditions] - 1;

                        // if zero then no between!
                        if (val > 0 && val < curCondition.min_between)
                        {
                            val = curCondition.min_between - val;

                            int pen = (cost * val);
                            curCondition.penalty_min_between += pen;

                            // create and add violation
                            Violation violation = new Violation(nodeToEvaluate.NurseId, nodeToEvaluate.Index, "MIN_BETWEEN", val, curCondition, lastPosition, pen);
                            if (trackViolations) nodeToEvaluate.Violations.Add(violation.ToString());
                            int day = nrOfDaysInPeriod - 1;
                            this.constraintViolationsOnDay[day] += pen;
                            this.penalty += pen;
                        }
                    }
                }
                for (int iNum = 0; iNum != curCondition.max_pert_nr.Length; iNum++)
                {
                    if (pertNr[iConditions][iNum] != 0 && pertNr[iConditions][iNum] > curCondition.max_pert_nr[iNum])
                    {
                        int cost = curCondition.cost_max_pert_nr[iNum];

                        if (cost > 0)
                        {
                            int val = pertNr[iConditions][iNum] - curCondition.max_pert_nr[iNum];
                            int pen = (cost * val);
                            curCondition.penalty_max_pert_nr += pen;

                            // create and add violation
                            Violation violation = new Violation(nodeToEvaluate.NurseId, nodeToEvaluate.Index, "MAX_PERT", val, curCondition, iNum, pen);
                            if (trackViolations) nodeToEvaluate.Violations.Add(violation.ToString());
                            int day = nrOfDaysInPeriod - 1;
                            this.constraintViolationsOnDay[day] += pen;
                            this.penalty += pen;
                        }
                    }

                    if (pertNr[iConditions][iNum] != 0 && pertNr[iConditions][iNum] < curCondition.min_pert_nr[iNum])
                    {
                        int cost = curCondition.cost_min_pert_nr[iNum];

                        if (cost > 0)
                        {
                            int val = curCondition.min_pert_nr[iNum] - pertNr[iConditions][iNum];
                            int pen = (cost * val);
                            curCondition.penalty_min_pert_nr += pen;

                            // create and add violation
                            Violation violation = new Violation(nodeToEvaluate.NurseId, nodeToEvaluate.Index, "MIN_PERT", val, curCondition, iNum, pen);
                            if (trackViolations) nodeToEvaluate.Violations.Add(violation.ToString());
                            int day = nrOfDaysInPeriod - 1;
                            this.constraintViolationsOnDay[day] += pen;
                            this.penalty += pen;
                        }
                    }
                }
                nodeToEvaluate.Penalty += (this.penalty - this.origPenalty);

            }

        }

        public int GetCost(Node nodeToEvaluate, List<Condition> conditions, bool trackViolations = false)
        {
            InitialiseEvaluation(nodeToEvaluate, conditions);
            IntermediateEvaluation(nodeToEvaluate, conditions, trackViolations);
            FinalEvaluation(nodeToEvaluate, conditions, trackViolations);

            int temp = 0;
            foreach (Condition c in conditions)
            {
                int cCost = c.GetCost();
                temp += cCost;
            }
            return temp;
        }

    }
}
