using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACONRP.Evaluator
{
    public class CompleteWeekendsCondition : Condition
    {

        public CompleteWeekendsCondition(int numUnits, int numDays, Contract contract, int firstSaturday, int numWeekends)
        {
            this.name = "COMPLETE WEEKENDS";
            this.firstSaturday = firstSaturday;
            this.numWeekends = numWeekends;
            this.BuildNumbering(numUnits, numDays, contract);
        }

        protected override void BuildNumbering(int numUnits, int numDays, Contract contract)
        {
            int begin = firstSaturday * numUnits;

            int numDaysInWeekend = 0;
            if (contract.WeekendDefinition == "SaturdaySunday")
            {
                numDaysInWeekend = 2;
            }
            else if (contract.WeekendDefinition == "FridaySaturdaySunday")
            {
                begin = begin - (numUnits);
                numDaysInWeekend = 3;
            }
            else if (contract.WeekendDefinition == "FridaySaturdaySundayMonday")
            {
                begin = begin - (numUnits);
                numDaysInWeekend = 4;
            }
            else if (contract.WeekendDefinition == "SaturdaySundayMonday")
            {
                numDaysInWeekend = 3;
            }

            this.numbering = new int[numUnits * numDays];

            // initialize with Undefined...
            for (int i = 0; i != numUnits * numDays; i++)
            {
                this.numbering[i] = Condition.Undefined;
            }

            int counter = 0;
            for (int i = 0; i != numWeekends; i++)
            {
                for (int j = 0; j != numDaysInWeekend; j++)
                {
                    for (int k = 0; k != numUnits; k++)
                    {
                        int pos = (begin + (i * 7 * numUnits)) + (j * numUnits) + k;
                        numbering[pos] = counter;
                    }

                    counter++;
                }

                counter += 2;
            }
            // INITIALISE COST OF CONDITION
            if ((contract.CompleteWeekends.Text) == "true")
            {
                
                min_consecutive = numDaysInWeekend;
                cost_min_consecutive = Convert.ToInt16(contract.CompleteWeekends.Weight); 

                min_consecutive_string = "Incomplete working weekends";

                this.ENABLED = true;
            }
            // INITIALISE PERTS
            max_pert_nr = new int[numUnits * numDays];
            max_pert_nr_nonevent = new int[numUnits * numDays];
            min_pert_nr = new int[numUnits * numDays];
            min_pert_nr_nonevent = new int[numUnits * numDays];
            cost_max_pert_nr = new int[numUnits * numDays];
            cost_min_pert_nr = new int[numUnits * numDays];
        }

        protected override void BuildNumbering(int numUnits, int numDays, Contract contract, List<DayOff> dayOffRequests, DateTime startDate)
        {
            throw new NotImplementedException();
        }
        protected override void BuildNumbering(int numUnits, int numDays, Contract contract, List<ShiftOff> shiftOffRequests, Dictionary<string, int> shiftTypesDict, DateTime startDate)
        {
            throw new NotImplementedException();
        }
    }
}