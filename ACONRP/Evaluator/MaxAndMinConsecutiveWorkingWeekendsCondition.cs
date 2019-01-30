using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACONRP.Evaluator
{
    public class MaxAndMinConsecutiveWorkingWeekendsCondition : Condition
    {
        public MaxAndMinConsecutiveWorkingWeekendsCondition(int numUnits, int numDays, Contract contract, int firstSaturday, int numWeekends)
        {
            this.name = "CONSECUTIVE WORKING WEEKENDS";
            this.firstSaturday = firstSaturday;
            this.numWeekends = numWeekends;
            this.BuildNumbering(numUnits, numDays, contract);
        }

        protected override void BuildNumbering(int numUnits, int numDays, Contract contract)
        {
            int begin = firstSaturday * numUnits;
            if (firstSaturday == 0)
            {
                begin = (firstSaturday + 7) * numUnits;
            }

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

            // initialize with Undefined.
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

                }

                counter += 1;
            }

            // INITIALISE COST OF CONDITION
            if ((contract.MinConsecutiveWorkingWeekends.On) == "1")
            {
                min_consecutive = Convert.ToInt16(contract.MinConsecutiveWorkingWeekends.Text);
                cost_min_consecutive = Convert.ToInt16(contract.MinConsecutiveWorkingWeekends.Weight);

                min_consecutive_string = "Minimum consecutive working weekends";

                this.ENABLED = true;
            }

            if ((contract.MaxConsecutiveWorkingWeekends.On) == "1")
            {
                max_consecutive = Convert.ToInt16(contract.MaxConsecutiveWorkingWeekends.Text);
                cost_max_consecutive = Convert.ToInt16(contract.MaxConsecutiveWorkingWeekends.Weight);

                max_consecutive_string = "Maximum consecutive working weekends";

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