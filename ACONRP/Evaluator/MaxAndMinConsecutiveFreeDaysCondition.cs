using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACONRP.Evaluator
{
    public class MaxAndMinConsecutiveFreeDaysCondition : Condition
    {
        public MaxAndMinConsecutiveFreeDaysCondition(int numUnits, int numDays, Contract contract)
        {
            this.name = "MAX AND MIN CONS FREE DAY";
            this.BuildNumbering(numUnits, numDays, contract);
        }

        protected override void BuildNumbering(int numUnits, int numDays, Contract contract)
        {
            this.numbering = new int[numUnits * numDays];
            for (int i = 0; i != numDays * numUnits; i++)
            {
                this.numbering[i] = (i / numUnits);
            }

            this.last_nr_history = -1;
            this.future_nr = numDays;

            // INITIALISE PERTS
            max_pert_nr = new int[numUnits * numDays];
            max_pert_nr_nonevent = new int[numUnits * numDays];
            min_pert_nr = new int[numUnits * numDays];
            min_pert_nr_nonevent = new int[numUnits * numDays];
            cost_max_pert_nr = new int[numUnits * numDays];
            cost_min_pert_nr = new int[numUnits * numDays];

            // INITIALISE VALUES

            if ((contract.MaxConsecutiveFreeDays.On) == "1")
            {
                max_between = Convert.ToInt16(contract.MaxConsecutiveFreeDays.Text);
                cost_max_between = Convert.ToInt16(contract.MaxConsecutiveFreeDays.Weight);
                max_between_string = "Maximum number of consecutive free days";

                this.ENABLED = true;
            }

            if ((contract.MinConsecutiveFreeDays.On) == "1")
            {
                min_between = Convert.ToInt16(contract.MinConsecutiveFreeDays.On);
                cost_min_between = Convert.ToInt16(contract.MinConsecutiveFreeDays.On);
                min_between_string = "Minimum number of consecutive free days";

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