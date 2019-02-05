using System;
using System.Collections.Generic;

namespace ACONRP.Evaluator
{
    public class PatternCondition : Condition
    {

        public PatternCondition(int numUnits, int numDays, Contract contract)
        {
            this.name = "Unwanted Pattern: ";
            this.BuildNumbering(numUnits, numDays, contract);
        }

        protected override void BuildNumbering(int numUnits, int numDays, Contract contract)
        {
            // fill all with undefined
            //int numSt = numUnits;
            //int numDays = this.getEmployee().getSchedulingPeriod().nrOfDaysInPeriod;
            //int numUnits = numSt * numDays;
            int numTimeUnits = numUnits * numDays;
            this.numbering = new int[numTimeUnits];

            for (int i = 0; i != numTimeUnits; i++)
            {
                this.numbering[i] = Condition.Undefined;
            }

            max_pert_nr = new int[numTimeUnits];
            max_pert_nr_nonevent = new int[numTimeUnits];
            min_pert_nr = new int[numTimeUnits];
            min_pert_nr_nonevent = new int[numTimeUnits];
            cost_max_pert_nr = new int[numTimeUnits];
            cost_min_pert_nr = new int[numTimeUnits];

            this.ENABLED = true;
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