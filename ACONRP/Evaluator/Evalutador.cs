using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ACONRP;


namespace ACONRP.Evaluator
{
    public class Evalutador
    {
        public ScheduleEvaluator scheduleEvaluator;
        private List<Condition> conditions;        
        SchedulingPeriod inputDataLocal;
        int numUnits;
        int numDays;
        public Dictionary<String, int> shiftTypesDict = new Dictionary<string, int>();
        int numWeekends;
        int firstSaturday;


        public Evalutador(SchedulingPeriod inputData)
        {
            //Data Initialization
            inputDataLocal = inputData;
            //var orderedShifts = inputDataLocal.ShiftTypes.Shift.OrderBy(x => DateTime.Parse(x.StartTime)).ThenBy(x => DateTime.Parse(x.EndTime)).ToList();
            var orderedShifts = inputDataLocal.ShiftTypes.Shift.ToList();
            orderedShifts.ForEach(shift => shiftTypesDict.Add(shift.ID, orderedShifts.IndexOf(shift)));
            numUnits = inputDataLocal.ShiftTypes.Shift.Count;
            DateTime startDate = Convert.ToDateTime(inputDataLocal.StartDate);
            DateTime endDate = Convert.ToDateTime(inputDataLocal.EndDate);
            numDays = endDate.Day - startDate.Day + 1;
            conditions = new List<Condition>();            
            firstSaturday = GetFirstSaturday(startDate, endDate);
            numWeekends = GetNumWeekends(firstSaturday, startDate, endDate);
        }
        /// <summary>
        /// Evalutador overloaded constractor
        /// </summary>
        /// <param name="inputData"></param>
        /// <param name="nurseId"></param>
        public Evalutador(SchedulingPeriod inputData, int nurseId) : this(inputData)
        {
            scheduleEvaluator = new ScheduleEvaluator(inputDataLocal);
            conditions.Clear();
            BuildConditionsList(nurseId);
        }

        private int GetNumWeekends(int firstSaturday, DateTime startDate, DateTime endDate)
        {
            var numWeeksInPeriod = 0;
            int saturdayIndex = firstSaturday;
            if (saturdayIndex < 0) return numWeeksInPeriod;

            while (saturdayIndex < (endDate.Subtract(startDate).Days))
            {
                saturdayIndex += 7;
                numWeeksInPeriod++;
            }
            return numWeeksInPeriod;
        }

        private int GetFirstSaturday(DateTime startDate, DateTime endDate)
        {
            for (int i = 0; i < endDate.Subtract(startDate).Days; i++)
            {
                if (startDate.AddDays(i).DayOfWeek.ToString() == "Saturday") return i;

            }
            throw new Exception($"No Saturday found between {startDate} - {endDate}");
        }

        private void BuildConditionsList(int employeeId)
        {
            int contractId = Convert.ToInt16(inputDataLocal.Employees.Employee[employeeId].ContractID);
            Contract contract = inputDataLocal.Contracts.Contract[contractId];
            var patterns = inputDataLocal.Patterns.Pattern;
            List<DayOff> dayOffRequests = inputDataLocal.DayOffRequests.DayOff.Where(day => day.EmployeeID == employeeId.ToString()).ToList();
            List<ShiftOff> shiftOffRequests = inputDataLocal.ShiftOffRequests.ShiftOff.Where(shift => shift.EmployeeID == employeeId.ToString()).ToList();
            DateTime startDate = Convert.ToDateTime(inputDataLocal.StartDate);
            
            Condition c;
            c = new MaxAndMinConsecutiveWorkingDaysCondition(numUnits, numDays, contract);
            conditions.Add(c);

            c = new MaxAndMinConsecutiveFreeDaysCondition(numUnits, numDays, contract);
            conditions.Add(c);

            c = new MaxAndMinNumAssignments(numUnits, numDays, contract);
            conditions.Add(c);

            c = new SingleAssignmentPerDay(numUnits, numDays, contract);// Equals to MaxNumAssignmentPerDays soft constraint condition
            conditions.Add(c);

            c = new IdenticalShiftTypesInWeekend(numUnits, numDays, contract, firstSaturday, numWeekends);
            conditions.Add(c);

            c = new CompleteWeekendsCondition(numUnits, numDays, contract, firstSaturday, numWeekends);
            conditions.Add(c);

            c = new MaxAndMinConsecutiveWorkingWeekendsCondition(numUnits, numDays, contract, firstSaturday, numWeekends);
            conditions.Add(c);

            c = new RequestedDayOffCondition(numUnits, numDays, contract, dayOffRequests, startDate);
            conditions.Add(c);

            c = new RequestedShiftOffCondition(numUnits, numDays, contract, shiftOffRequests, shiftTypesDict, startDate);
            conditions.Add(c);

            // ADD UNWANTED PATTERNS
            List<Condition> conds = PatternConditionBuilder.BuildPatternCondition(numUnits, numDays, contract, patterns, shiftTypesDict, numWeekends);
            conditions.AddRange(conds);            

            //c = new RequestedShiftOnCondition(numUnits, numDays, contract);
            //conditions.Add(c);

            //c = new AlternativeSkillCondition(numUnits, numDays, contract);
            //conditions.Add(c);           
        }        
        /// <summary>
        /// Calculate penalty and cost of a single node
        /// </summary>
        /// <param name="node">Node to evaluate</param>
        public void CalculateNodePenalty(Node node)
        {
            int nodeCost = scheduleEvaluator.GetCost(node, conditions);
            node.Cost = nodeCost;
        }
        public void CalculateSolutionPenalty(List<Node> nodesToEvaluate)
        {
            bool enableViolTracking = true;
            if (nodesToEvaluate == null) throw new NullReferenceException();
            scheduleEvaluator = new ScheduleEvaluator(inputDataLocal);
            foreach (Node node in nodesToEvaluate)
            {
                conditions.Clear();
                BuildConditionsList(node.NurseId);
                node.Violations = new List<string>();
                int nodeCost = scheduleEvaluator.GetCost(node, conditions, enableViolTracking);
            }
        }
        private void PrintSingleShiftPattern(Node nodeToEvaluate)
        {
            {
                //Swap from Matrix data structure to simple array data structure
                bool[] rooster = new bool[nodeToEvaluate.ShiftPatternMatrix.GetLength(0) * nodeToEvaluate.ShiftPatternMatrix.GetLength(1)];
                int k = 0;
                bool[,] tempMatrix = nodeToEvaluate.ShiftPatternMatrix;
                for (int i = 0; i != tempMatrix.GetLength(1); i++)
                {
                    for (int y = 0; y != tempMatrix.GetLength(0); y++)
                    {
                        rooster[k] = tempMatrix[y, i];
                        k++;
                    }

                }
                int j = 1;
                int count = 0;
                foreach (bool element in rooster)
                {
                    Console.Write((element) ? "1" : "0");
                    if (element) count++;
                    if (j == nodeToEvaluate.ShiftPatternMatrix.GetLength(0))
                    {
                        Console.Write("|");
                        j = 0;
                    }
                    else
                    {
                        Console.Write("-");
                    }
                    j++;
                }
                Console.Write($" {count}\n");
            }
        }

    }


}
