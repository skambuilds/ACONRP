//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace ACONRP.Evaluator
//{
//    public class RequestedShiftOnCondition : Condition
//    {
//        public RequestedShiftOnCondition(int numUnits, int numDays, Contract contract)
//        {
//            this.name = "REQUESTED SHIFT ON";
//            this.BuildNumbering(numUnits, numDays, contract);
//        }

//        protected override void BuildNumbering(int numUnits, int numDays, Contract contract)
//        {
//            this.numbering = new int[numUnits * numDays];

//            for (int i = 0; i != numUnits * numDays; i++)
//            {
//                this.numbering[i] = Condition.Undefined;
//                max_pert_nr[i] = -1;
//                min_pert_nr[i] = -1;
//            }

//            // INITIALISE PERTS
//            max_pert_nr = new int[numUnits * numDays];
//            max_pert_nr_nonevent = new int[numUnits * numDays];
//            min_pert_nr = new int[numUnits * numDays];
//            min_pert_nr_nonevent = new int[numUnits * numDays];
//            cost_max_pert_nr = new int[numUnits * numDays];
//            cost_min_pert_nr = new int[numUnits * numDays];

//            int counter = 0;
//            Date start = getEmployee().getSchedulingPeriod().startDate;

//            for (RequestedShiftOn rso : getEmployee().shiftsOn)
//            {
//                int day = DateTools.dateOffset(start, rso.date) - 1;
//                int unit = rso.shiftType.index;

//                int pos = (day * numUnits) + unit;
//                this.numbering[pos] = counter;

//                min_pert_nr[counter] = 0;
//                cost_min_pert_nr[counter] = rso.weight;
//                counter += 1;
//            }


//            // INITIALISE COST OF CONDITION
//            //cost_max_total = this.Employee.Schedule.SchedulingPeriod.PenaltyValues.MaxNumAssignmentsPenalty;

//            if (this.getEmployee().shiftsOn.size() > 0)
//            {

//                min_pert_string = "Requested shift on";

//                this.ENABLED = true;
//            }
//        }
//    }
//}