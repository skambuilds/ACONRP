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
        public List<Violation> violations;
        SchedulingPeriod inputDataLocal;
        int numUnits;
        int numDays;
        public Dictionary<String, int> shiftTypesDict = new Dictionary<string, int>();


        public Evalutador(SchedulingPeriod inputData)
        {
            //Data Initialization
            inputDataLocal = inputData;
            numUnits = inputDataLocal.ShiftTypes.Shift.Count;
            DateTime startDate = Convert.ToDateTime(inputDataLocal.StartDate);
            DateTime endDate = Convert.ToDateTime(inputDataLocal.EndDate);
            numDays = endDate.Day - startDate.Day + 1;
            conditions = new List<Condition>();
            violations = new List<Violation>();
        }

        private void BuildConditionsList(int employeeId)
        {
            int contractId = Convert.ToInt16(inputDataLocal.Employees.Employee[employeeId].ContractID);
            Contract contract = inputDataLocal.Contracts.Contract[contractId];
            List<DayOff> dayOffRequests = inputDataLocal.DayOffRequests.DayOff;
            List<ShiftOff> shiftOffRequests = inputDataLocal.ShiftOffRequests.ShiftOff;
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

            c = new IdenticalShiftTypesInWeekend(numUnits, numDays, contract);
            conditions.Add(c);

            c = new CompleteWeekendsCondition(numUnits, numDays, contract);
            conditions.Add(c);

            c = new MaxAndMinConsecutiveWorkingWeekendsCondition(numUnits, numDays, contract);
            conditions.Add(c);

            c = new RequestedDayOffCondition(numUnits, numDays, contract, dayOffRequests, startDate);
            conditions.Add(c);

            int i = 0;
            foreach(Shift shiftElement  in inputDataLocal.ShiftTypes.Shift)//TODO automatic shift identification based on start time and end time.
            {   
                if (shiftElement.ID == "E") { shiftTypesDict.Add(shiftElement.ID, 0); }
                if (shiftElement.ID == "D") { shiftTypesDict.Add(shiftElement.ID, 1); }
                if (shiftElement.ID == "L") { shiftTypesDict.Add(shiftElement.ID, 2); }
                if (shiftElement.ID == "N") { shiftTypesDict.Add(shiftElement.ID, 3); }
                //shiftTypesDict.Add(shiftElement.ID, i);
                //i++;
            }
            c = new RequestedShiftOffCondition(numUnits, numDays, contract, shiftOffRequests, shiftTypesDict, startDate);
            conditions.Add(c);
            shiftTypesDict.Clear();
            

            //c = new RequestedShiftOnCondition(numUnits, numDays, contract);
            //conditions.Add(c);

            //c = new AlternativeSkillCondition(numUnits, numDays, contract);
            //conditions.Add(c);

            // ADD UNWANTED PATTERNS
            //ArrayList<Condition> conds = PatternConditionBuilder.buildPatternCondition(getEmployee(), getEmployee().getSchedulingPeriod());
            //conditions.addAll(conds);

            // ADD UNWANTED SHIFTS

            //ShiftType night = getEmployee().getSchedulingPeriod().shiftTypes.get(("N"));
            //conds = TwoFreeDaysAfterANightShiftConditionBuilder.buildCondition(getEmployee(), getEmployee().getSchedulingPeriod(), night);
            //conditions.addAll(conds);
        }


        public int CalculatePenalty(List<Node> nodesToEvaluate)
        {
            int cost = 0;
            scheduleEvaluator = new ScheduleEvaluator(inputDataLocal);
            if (nodesToEvaluate == null) throw new IndexOutOfRangeException();
            BuildConditionsList(nodesToEvaluate[0].NurseId);
            int median = 0;
            foreach (Node nodeToEvaluate in nodesToEvaluate)
            {
                int temp = 0;
                temp = scheduleEvaluator.GetCost(nodeToEvaluate, conditions, violations);
                cost += temp;
                median++;
            }
            //DEBUG: Prints total cost of soft constraint violation per node.
            //foreach (Violation viol in violations)
            //{
            //    string g = viol.ToString();
            //    Console.WriteLine(g);
            //}

            Console.WriteLine("Soft Constraint ShiftPatterns Violation for nurse: " + nodesToEvaluate[0].NurseId + " = " + cost + " , Average Cost = " + (cost/median));
            return cost;
        }

    }


}
