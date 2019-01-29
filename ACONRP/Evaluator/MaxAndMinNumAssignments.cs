using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACONRP.Evaluator
{
    public class MaxAndMinNumAssignments : Condition
    {
        public MaxAndMinNumAssignments(int numUnits, int numDays, Contract contract)
        {
            this.name = "MAX AND MIN USAGE";
            this.BuildNumbering(numUnits, numDays, contract);
        }


        protected override void BuildNumbering(int numUnits, int numDays, Contract contract)
        {
            this.numbering = new int[numUnits * numDays];

            for (int i = 0; i != numUnits * numDays; i++)
            {
                this.numbering[i] = i;
            }

            // INITIALISE COST OF CONDITION

            if ((contract.MaxNumAssignments.On) == "1")
            {
                max_total = Convert.ToInt16(contract.MaxNumAssignments.Text);
                cost_max_total = Convert.ToInt16(contract.MaxNumAssignments.Weight);

                max_total_string = "Maximum number of assignments";

                this.ENABLED = true;
            }

            if ((contract.MinNumAssignments.On) == "1")
            {
                min_total = Convert.ToInt16(contract.MinNumAssignments.Text);
                cost_min_total = Convert.ToInt16(contract.MaxNumAssignments.Weight);

                min_total_string = "Minimum number of assignments";

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