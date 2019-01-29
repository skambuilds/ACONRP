//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using ACONRP;

//namespace ACONRP.Evaluator
//{
//    public class RequestedDayOnCondition : Condition
//    {
//        public RequestedDayOnCondition(int numUnits, int numDays, Contract contract, List<DayOff> dayOffRequests)
//        {
//            this.name = "REQUESTED DAY ON";
//            this.BuildNumbering(numUnits, numDays, contract);
//        }

//        protected override void BuildNumbering(int numUnits, int numDays, Contract contract, List<DayOff> dayOffRequests)
//        {
//            this.numbering = new int[numUnits * numDays];

//            // INITIALISE PERTS
//            max_pert_nr = new int[numUnits * numDays];
//            max_pert_nr_nonevent = new int[numUnits * numDays];
//            min_pert_nr = new int[numUnits * numDays];
//            min_pert_nr_nonevent = new int[numUnits * numDays];
//            cost_max_pert_nr = new int[numUnits * numDays];
//            cost_min_pert_nr = new int[numUnits * numDays];

//            for (int i = 0; i != numUnits * numDays; i++)
//            {
//                this.numbering[i] = Condition.Undefined;
//                max_pert_nr[i] = -1;
//                min_pert_nr[i] = -1;
//            }

//            int counter = 0;
//            Date start = getEmployee().getSchedulingPeriod().startDate;

//            foreach ( rdo in dayOffRequests)
//            {
//                int day = DateTools.dateOffset(start, rdo.date) - 1;

//                for (int i = 0; i != numUnits; i++)
//                {
//                    int pos = (day * numUnits) + i;
//                    this.numbering[pos] = counter;
//                }

//                min_pert_nr[counter] = 0;
//                cost_min_pert_nr[counter] = rdo.weight;
//                counter += 1;
//            }


//            // INITIALISE COST OF CONDITION
//            if (this.getEmployee().daysOn.size() > 0)
//            {

//                min_pert_string = "Requested day off";

//                this.ENABLED = true;
//            }
//        }
//    }
//}