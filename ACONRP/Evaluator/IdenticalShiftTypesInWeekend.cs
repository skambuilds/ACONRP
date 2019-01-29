using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACONRP.Evaluator
{
    public class IdenticalShiftTypesInWeekend : Condition
    {
        public IdenticalShiftTypesInWeekend(int numUnits, int numDays, Contract contract)
        {
            this.name = "IDENTICAL_WEEKENDS";
            this.BuildNumbering(numUnits, numDays, contract);
        }

        protected override void BuildNumbering(int numUnits, int numDays, Contract contract)
        {
            // INITIALISE PERTS
            max_pert_nr = new int[numUnits * numDays];
            max_pert_nr_nonevent = new int[numUnits * numDays];
            min_pert_nr = new int[numUnits * numDays];
            min_pert_nr_nonevent = new int[numUnits * numDays];
            cost_max_pert_nr = new int[numUnits * numDays];
            cost_min_pert_nr = new int[numUnits * numDays];

            int numWeekends = 4;//TODO automatic numWeekends and firstSaturday recognition
            int firstSaturday = 1;
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

            // initialize with Undefined...
            for (int i = 0; i != numUnits * numDays; i++)
            {
                this.numbering[i] = Condition.Undefined;
                max_pert_nr[i] = -1;
                min_pert_nr[i] = -1;
            }



            // INITIALISE COST OF CONDITION
            if (Convert.ToBoolean(contract.IdenticalShiftTypesDuringWeekend.Text))
            {

                int getal = 1;

                for (int i = 0; i != numWeekends; i++)
                {
                    for (int k = 0; k != numUnits; k++)
                    {
                        for (int j = 0; j != numDaysInWeekend; j++)
                        {
                            int pos = ((i * 7 * numUnits) + begin) + (j * numUnits) + k;

                            try
                            {
                                numbering[pos] = getal;
                            }
                            catch (Exception e)
                            {
                            }
                        }

                        min_pert_nr[getal] = numDaysInWeekend;
                        cost_min_pert_nr[getal] = Convert.ToInt16(contract.IdenticalShiftTypesDuringWeekend.Weight);

                        getal++;
                    }

                    getal += (numDaysInWeekend + 1);
                }
                min_pert_string = "Identical shifttypes during weekend";

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