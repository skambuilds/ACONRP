using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACONRP.Evaluator
{
    public class MaxAndMinConsecutiveWorkingDaysCondition : Condition
    {
        public MaxAndMinConsecutiveWorkingDaysCondition(int numUnits, int numDays, Contract contract)
        {
            this.name = "MAX AND MIN CONS WORKING DAY";
            this.BuildNumbering(numUnits, numDays, contract);
        }

        
        protected override void BuildNumbering(int numUnits, int numDays, Contract contract)
        {



            this.numbering = new int[numUnits * numDays];

            for (int i = 0; i != numDays * numUnits; i++)
            {
                this.numbering[i] = (i / numUnits);
            }

            // INITIALISE PERTS
            max_pert_nr = new int[numUnits * numDays];
            max_pert_nr_nonevent = new int[numUnits * numDays];
            min_pert_nr = new int[numUnits * numDays];
            min_pert_nr_nonevent = new int[numUnits * numDays];
            cost_max_pert_nr = new int[numUnits * numDays];
            cost_min_pert_nr = new int[numUnits * numDays];

            // INITIALISE VALUES

            if ((contract.MaxConsecutiveWorkingDays.On) == "1")
            {
                max_consecutive = Convert.ToInt16(contract.MaxConsecutiveWorkingDays.Text);
                cost_max_consecutive = Convert.ToInt16(contract.MaxConsecutiveWorkingDays.Weight);
                max_consecutive_string = "Maximum number of consecutive working days";

                this.ENABLED = true;
            }

            if ((contract.MinConsecutiveWorkingDays.On) == "1")
            {
                min_consecutive = Convert.ToInt16(contract.MinConsecutiveWorkingDays.Text);
                cost_min_consecutive = Convert.ToInt16(contract.MinConsecutiveWorkingDays.Weight);
                min_consecutive_string = "Minimum number of consecutive working days";

                this.ENABLED = true;
            }
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