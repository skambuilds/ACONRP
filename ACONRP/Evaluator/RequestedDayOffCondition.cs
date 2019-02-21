using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ACONRP;

namespace ACONRP.Evaluator
{
    public class RequestedDayOffCondition : Condition
    {
        public RequestedDayOffCondition(int numUnits, int numDays, Contract contract, List<DayOff> dayOffRequests, DateTime startDate)
        {
            this.name = "REQUESTED DAY OFF";
            this.BuildNumbering(numUnits, numDays, contract, dayOffRequests, startDate);
        }

        protected override void BuildNumbering(int numUnits, int numDays, Contract contract, List<DayOff> dayOffRequests, DateTime startDate)
        {
            this.numbering = new int[numUnits * numDays];

            // INITIALISE PERTS
            max_pert_nr = new int[numUnits * numDays];
            max_pert_nr_nonevent = new int[numUnits * numDays];
            min_pert_nr = new int[numUnits * numDays];
            min_pert_nr_nonevent = new int[numUnits * numDays];
            cost_max_pert_nr = new int[numUnits * numDays];
            cost_min_pert_nr = new int[numUnits * numDays];

            for (int i = 0; i != numUnits * numDays; i++)
            {
                this.numbering[i] = Condition.Undefined;
                max_pert_nr[i] = -1;
                min_pert_nr[i] = -1;
            }

            int counter = 0;
            DateTime start = startDate;

            foreach (DayOff rdo in dayOffRequests)
            {
                int day = Utils.DateOffset(start, Convert.ToDateTime(rdo.Date)) - 1;

                for (int i = 0; i != numUnits; i++)
                {
                    int pos = (day * numUnits) + i;
                    this.numbering[pos] = counter;
                }

                max_pert_nr[counter] = 0;

                cost_max_pert_nr[counter] = Convert.ToInt16(rdo.Weight);
                max_pert_string = "Requested day off";

                this.ENABLED = true;

                counter += 1;
            }
        }

        protected override void BuildNumbering(int numUnits, int numDays, Contract contract)
        {
            throw new NotImplementedException();
        }

        protected override void BuildNumbering(int numUnits, int numDays, Contract contract, List<ShiftOff> shiftOffRequests, Dictionary<string, int> shiftTypesDict, DateTime startDate)
        {
            throw new NotImplementedException();
        }

       
    }
}