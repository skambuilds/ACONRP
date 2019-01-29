using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ACONRP;

namespace ACONRP.Evaluator
{
    public class RequestedShiftOffCondition : Condition
    {
        public RequestedShiftOffCondition(int numUnits, int numDays, Contract contract, List<ShiftOff> shiftOffRequests, Dictionary<string, int> shiftTypesDict, DateTime startDate)
        {
            this.name = "REQUESTED SHIFT OFF";
            this.BuildNumbering(numUnits, numDays, contract, shiftOffRequests, shiftTypesDict, startDate);
        }

        protected override void BuildNumbering(int numUnits, int numDays, Contract contract, List<ShiftOff> shiftOffRequests, Dictionary<string, int> shiftTypesDict, DateTime startDate)
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

            foreach (ShiftOff rso in shiftOffRequests)
            {
                int unit = -1;
                int day = DateOffset(start, Convert.ToDateTime(rso.Date)) - 1;
                bool result = shiftTypesDict.TryGetValue(rso.ShiftTypeID, out unit);//TODO Check if bool result is correct
                


                int pos = (day * numUnits) + unit;
                this.numbering[pos] = counter;

                max_pert_nr[counter] = 0;


                cost_max_pert_nr[counter] = Convert.ToInt16(rso.Weight);
                max_pert_string = "Requested shift off";

                this.ENABLED = true;


                counter += 1;
            }
        }

        protected override void BuildNumbering(int numUnits, int numDays, Contract contract)
        {
            throw new NotImplementedException();
        }

        protected override void BuildNumbering(int numUnits, int numDays, Contract contract, List<DayOff> dayOffRequests, DateTime startDate)
        {
            throw new NotImplementedException();
        }

        public static int DateOffset(DateTime start, DateTime date)
        {
            TimeSpan interval = ((Convert.ToDateTime(date) - Convert.ToDateTime(start)));
            int nrOfDays = (interval.Days) + 1;

            return nrOfDays;
        }

    }
}