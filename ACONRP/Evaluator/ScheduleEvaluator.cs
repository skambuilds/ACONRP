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
        private int[] last_nr;
        private int[] last_nr_pos;
        private int[] total;
        private int[] consecutive;
        private int[][] pert_nr;
        private int[] de_juiste_plaats;
        private int[] current_nr;
        private int[] last_nr_ini;
        private int[] consecutive_ini;
        private bool[] evento;
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
            if (current_nr == null) current_nr = new int[numberOfConditions];
            if (last_nr == null) last_nr = new int[numberOfConditions];
            if (last_nr_pos == null) last_nr_pos = new int[numberOfConditions];
            if (last_nr_ini == null) last_nr_ini = new int[numberOfConditions];
            if (consecutive == null) consecutive = new int[numberOfConditions];
            if (consecutive_ini == null) consecutive_ini = new int[numberOfConditions];
            if (total == null) total = new int[numberOfConditions];
            if (de_juiste_plaats == null) de_juiste_plaats = new int[numberOfConditions];
            if (pert_nr == null) pert_nr = new int[numberOfConditions][];
            if (evento == null) evento = new bool[numberOfConditions];
            constraintViolationsOnDay = new int[nrOfDaysInPeriod];

            for (int iConditions = 0; iConditions != numberOfConditions; iConditions++)
            {
                Condition curCondition = conditions[iConditions];

                curCondition.ResetCost();

                consecutive[iConditions] = 1;
                last_nr[iConditions] = curCondition.last_nr_history;
                evento[iConditions] = false;
                pert_nr[iConditions] = new int[curCondition.max_pert_nr.Length];
                total[iConditions] = 0;
            }
        }


        //Intermediate Evaluation
        public void IntermediateEvaluation(Node nodeToEvaluate, List<Condition> conditions, List<Violation> violations)
        {
            //Swap from Matrix data structure to simple array data structure
            bool[] rooster = new bool[numTimeUnits];
            int k = 0;
            bool[,] tempMatrix = nodeToEvaluate.ShiftPattern;
            for (int i = 0; i != tempMatrix.GetLength(1); i++)
            {
                for (int j = 0; j != tempMatrix.GetLength(0); j++)
                {
                    rooster[k] = tempMatrix[j, i];
                    k++;
                }

            }
            for (int iConditions = 0; iConditions != numberOfConditions; iConditions++)
            {
                for (int iRooster = 0; iRooster != rooster.Length; iRooster++)
                {
                    if (rooster[iRooster])
                    {
                        int nr = conditions[iConditions].GetNumbering()[iRooster]; //Returns the numbering of the iRooster day associated to the current condition 
                        if (nr != Condition.Undefined)
                        {
                            evento[iConditions] = true;
                            total[iConditions] = total[iConditions] + 1;
                            if (nr == last_nr[iConditions] + 1)
                            {
                                consecutive[iConditions] += 1;
                            }
                            else if (nr > last_nr[iConditions] + 1)
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
                                        Violation violation = new Violation(nodeToEvaluate.NurseId, nodeToEvaluate.Index, "MIN_CONSECUTIVE", val, curCondition, last_nr_pos[iConditions], pen);
                                        violations.Add(violation);
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
                                        Violation violation = new Violation(nodeToEvaluate.NurseId, nodeToEvaluate.Index, "MAX_CONSECUTIVE", val, curCondition, last_nr_pos[iConditions], pen);
                                        violations.Add(violation);
                                        int day = iRooster / nrShiftTypes;
                                        this.constraintViolationsOnDay[day] += pen;
                                        this.penalty += pen;
                                    }
                                }
                                if (nr - last_nr[iConditions] - 1 < curCondition.min_between)
                                {
                                    int cost = curCondition.cost_min_between;
                                    if (cost > 0)
                                    {
                                        int val = curCondition.min_between - (nr - last_nr[iConditions] - 1);
                                        int pen = (cost * val);
                                        curCondition.penalty_min_between += pen;
                                        // create and add violation
                                        Violation violation = new Violation(nodeToEvaluate.NurseId, nodeToEvaluate.Index, "MIN_BETWEEN", val, curCondition, last_nr_pos[iConditions], pen);
                                        violations.Add(violation);
                                        int day = iRooster / nrShiftTypes;
                                        this.constraintViolationsOnDay[day] += pen;
                                        this.penalty += pen;
                                    }
                                }
                                if (nr - last_nr[iConditions] - 1 > curCondition.max_between)
                                {
                                    int cost = curCondition.cost_max_between;
                                    if (cost > 0)
                                    {
                                        int val = (nr - last_nr[iConditions] - 1) - curCondition.max_between;
                                        int pen = (cost * val);
                                        curCondition.penalty_max_between += pen;
                                        Violation violation = new Violation(nodeToEvaluate.NurseId, nodeToEvaluate.Index, "MAX_BETWEEN", val, curCondition, last_nr_pos[iConditions], pen);                                    //Violation v = new Violation(ConstraintType.MAX_BETWEEN, val, curCondition, last_nr_pos[iConditions], this.getSchedule(), pen);
                                        violations.Add(violation);
                                        int day = iRooster / nrShiftTypes;
                                        this.constraintViolationsOnDay[day] += pen;
                                        this.penalty += pen;
                                    }
                                }
                                consecutive[iConditions] = 1;
                            }//end ELSE
                            pert_nr[iConditions][nr] += 1;
                            last_nr[iConditions] = nr;
                            last_nr_pos[iConditions] = iRooster;
                        } // ENDIF (nr != Undefined)
                    }
                }//ENDIF if rooster[]
            }

        }


        //Final Evaluation
        public void FinalEvaluation(Node nodeToEvaluate, List<Condition> conditions, List<Violation> violations)
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
                        Violation violation = new Violation(nodeToEvaluate.NurseId, nodeToEvaluate.Index, "MAX_TOTAL", val, curCondition, last_nr_pos[iConditions], pen);
                        violations.Add(violation);
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
                        Violation violation = new Violation(nodeToEvaluate.NurseId, nodeToEvaluate.Index, "MIN_TOTAL", val, curCondition, last_nr_pos[iConditions], pen);
                        violations.Add(violation);
                        int day = nrOfDaysInPeriod - 1;	                                    
                        this.constraintViolationsOnDay[day] += pen;
                        this.penalty += pen;
                    }
                }
                if (curCondition.future_nr != Condition.Undefined)
                {
                    if (last_nr[iConditions] == curCondition.future_nr - 1)
                    {
                        consecutive[iConditions] += 1;
                    }
                }
                if (evento[iConditions])
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
                            Violation violation = new Violation(nodeToEvaluate.NurseId, nodeToEvaluate.Index, "MAX_CONSECUTIVE", val, curCondition, last_nr_pos[iConditions], pen);
                            violations.Add(violation);
                            int day = nrOfDaysInPeriod - 1;	                                    
                            this.constraintViolationsOnDay[day] += pen;
                            this.penalty += pen;
                        }
                    }
                }
                if (last_nr[iConditions] != Condition.Undefined && consecutive[iConditions] < curCondition.min_consecutive)
                {
                    int cost = curCondition.cost_min_consecutive;
                    if (cost > 0)
                    {
                        int val = curCondition.min_consecutive - consecutive[iConditions];
                        int pen = (cost * val);
                        curCondition.penalty_min_consecutive += pen;
                        // create and add violation
                        Violation violation = new Violation(nodeToEvaluate.NurseId, nodeToEvaluate.Index, "MIN_CONSECUTIVE", val, curCondition, last_nr_pos[iConditions], pen);
                        violations.Add(violation);
                        int day = nrOfDaysInPeriod - 1;	                                    
                        this.constraintViolationsOnDay[day] += pen;
                        this.penalty += pen;
                    }
                }
                // max_between
                if (last_nr[iConditions] != Condition.Undefined && curCondition.future_nr != Condition.Undefined)
                {
                    int cost = curCondition.cost_max_between;

                    if (cost > 0)
                    {
                        int val = curCondition.future_nr - last_nr[iConditions] - 1;

                        if (val > curCondition.max_between)
                        {
                            val = val - curCondition.max_between;

                            int pen = (cost * val);
                            curCondition.penalty_max_between += pen;

                            // create and add violation
                            Violation violation = new Violation(nodeToEvaluate.NurseId, nodeToEvaluate.Index, "MAX_BETWEEN", val, curCondition, last_nr_pos[iConditions], pen);
                            violations.Add(violation);
                            int day = nrOfDaysInPeriod - 1;	                                    
                            this.constraintViolationsOnDay[day] += pen;
                            this.penalty += pen;
                        }
                    }
                }
                // min_between
                if (last_nr[iConditions] != Condition.Undefined && curCondition.future_nr != Condition.Undefined)
                {
                    int cost = curCondition.cost_min_between;

                    if (cost > 0)
                    {
                        int val = curCondition.future_nr - last_nr[iConditions] - 1;

                        // if zero then no between!
                        if (val > 0 && val < curCondition.min_between)
                        {
                            val = curCondition.min_between - val;

                            int pen = (cost * val);
                            curCondition.penalty_min_between += pen;

                            // create and add violation
                            Violation violation = new Violation(nodeToEvaluate.NurseId, nodeToEvaluate.Index, "MIN_BETWEEN", val, curCondition, last_nr_pos[iConditions], pen);
                            violations.Add(violation);
                            int day = nrOfDaysInPeriod - 1;	                                    
                            this.constraintViolationsOnDay[day] += pen;
                            this.penalty += pen;
                        }
                    }
                }
                for (int iNum = 0; iNum != curCondition.max_pert_nr.Length; iNum++)
                {
                    if (pert_nr[iConditions][iNum] != 0 && pert_nr[iConditions][iNum] > curCondition.max_pert_nr[iNum])
                    {
                        int cost = curCondition.cost_max_pert_nr[iNum];

                        if (cost > 0)
                        {
                            int val = pert_nr[iConditions][iNum] - curCondition.max_pert_nr[iNum];
                            int pen = (cost * val);
                            curCondition.penalty_max_pert_nr += pen;

                            // create and add violation
                            Violation violation = new Violation(nodeToEvaluate.NurseId, nodeToEvaluate.Index, "MAX_PERT", val, curCondition, last_nr_pos[iConditions], pen);
                            violations.Add(violation);
                            int day = nrOfDaysInPeriod - 1;                                    
                            this.constraintViolationsOnDay[day] += pen;
                            this.penalty += pen;
                        }
                    }

                    if (pert_nr[iConditions][iNum] != 0 && pert_nr[iConditions][iNum] < curCondition.min_pert_nr[iNum])
                    {
                        int cost = curCondition.cost_min_pert_nr[iNum];

                        if (cost > 0)
                        {
                            int val = curCondition.min_pert_nr[iNum] - pert_nr[iConditions][iNum];
                            int pen = (cost * val);
                            curCondition.penalty_min_pert_nr += pen;

                            // create and add violation
                            Violation violation = new Violation(nodeToEvaluate.NurseId, nodeToEvaluate.Index, "MIN_PERT", val, curCondition, last_nr_pos[iConditions], pen);
                            violations.Add(violation);
                            int day = nrOfDaysInPeriod - 1;                                 
                            this.constraintViolationsOnDay[day] += pen;
                            this.penalty += pen;
                        }
                    }
                }
                nodeToEvaluate.Penalty += (this.penalty - this.origPenalty);

            }

        }

        public int GetCost(Node nodeToEvaluate, List<Condition> conditions, List<Violation> violations)
        {
            InitialiseEvaluation(nodeToEvaluate, conditions);
            IntermediateEvaluation(nodeToEvaluate, conditions, violations);
            FinalEvaluation(nodeToEvaluate, conditions, violations);

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
