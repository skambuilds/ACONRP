using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ACONRP.Evaluator
{
    public class SingleAssignmentPerDay : Condition
    {
        public SingleAssignmentPerDay(int numUnits, int numDays, Contract contract)
        {
            this.name = "SINGLE ASSIGNMENT PER DAY";
            this.BuildNumbering(numUnits, numDays, contract);
            this.HARD_CONSTRAINT = true;
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

            int counter = 0;
            for (int i = 0; i != numDays; i++)
            {
                for (int j = 0; j != numUnits; j++)
                {
                    this.numbering[(i * numUnits) + j] = counter;
                }

                max_pert_nr[counter] = 1;
                cost_max_pert_nr[counter] = Convert.ToInt16(contract.SingleAssignmentPerDay.Weight); 
                counter++;
            }

            // INITIALISE VALUES
            if (Convert.ToBoolean(contract.SingleAssignmentPerDay.Text))
            {

                max_pert_string = "Single assignment per day (pert)";

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