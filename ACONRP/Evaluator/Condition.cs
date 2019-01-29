using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ACONRP.Evaluator
{
    public abstract class Condition
    {
        protected int[] numbering;
        protected string name;
        public int[] GetNumbering() { return numbering; }
        public string GetName() { return name; }
        public static readonly int Undefined = (int.MaxValue / 2);
        public bool HARD_CONSTRAINT;
        public bool ENABLED = false;      


        public int max_total = -1;
        public int min_total = -1;

        public int[] max_pert_nr;
        public int[] min_pert_nr;
        public int max_between = -1;
        public int min_between = -1;
        public int max_consecutive = -1;
        public int min_consecutive = -1;

        //costs
        public int cost_max_total;
        public int cost_min_total;

        public int[] cost_max_pert_nr;
        public int[] cost_min_pert_nr;
        public int cost_max_between;
        public int cost_min_between;
        public int cost_max_consecutive;
        public int cost_min_consecutive;

        //penalties
        public int penalty_max_total;
        public int penalty_min_total;

        public int penalty_max_pert_nr;
        public int penalty_min_pert_nr;
        public int penalty_max_between;
        public int penalty_min_between;
        public int penalty_max_consecutive;
        public int penalty_min_consecutive;

        public String count;

        // descriptions
        public String max_consecutive_string = "";
        public String min_consecutive_string = "";
        public String max_between_string = "";
        public String min_between_string = "";
        public String max_total_string = "";
        public String min_total_string = "";
        public String max_pert_string = "";
        public String min_pert_string = "";

        // future
        public int future_nr = Condition.Undefined;

        // HISTORY VARIABLES
        public int consecutive_history = -1;
        public int last_nr_history = Condition.Undefined;
        public int[] pert_nr_history = null;
        public int total_history = -1;

        // OVERALL NEEDED VARIABLES NONEVENT
        public int max_total_nonevent;
        public int min_total_nonevent;

        public int[] max_pert_nr_nonevent;
        public int[] min_pert_nr_nonevent;
        public int max_between_nonevent;
        public int min_between_nonevent;
        public int max_consecutive_nonevent;
        public int min_consecutive_nonevent;

        //costs
        public int cost_max_total_nonevent;
        public int cost_min_total_nonevent;

        public int cost_max_pert_nr_nonevent;
        public int cost_min_pert_nr_nonevent;
        public int cost_max_between_nonevent;
        public int cost_min_between_nonevent;
        public int cost_max_consecutive_nonevent;
        public int cost_min_consecutive_nonevent;

        //penalties
        public int penalty_max_total_nonevent;
        public int penalty_min_total_nonevent;

        public int penalty_max_pert_nr_nonevent;
        public int penalty_min_pert_nr_nonevent;
        public int penalty_max_between_nonevent;
        public int penalty_min_between_nonevent;
        public int penalty_max_consecutive_nonevent;
        public int penalty_min_consecutive_nonevent;

        public String count_nonevent;
        public int max_total_weight_nonevent;
        public int min_total_weight_nonevent;
        public int max_pert_weight_nonevent;
        public int min_pert_weight_nonevent;
        public int max_between_weight_nonevent;
        public int min_between_weight_nonevent;
        public int max_consecutive_weight_nonevent;
        public int min_consecutive_weight_nonevent;

        // descriptions
        public String max_consecutive_string_nonevent = "";
        public String min_consecutive_string_nonevent = "";
        public String max_between_string_nonevent = "";
        public String min_between_string_nonevent = "";
        public String max_total_string_nonevent = "";
        public String min_total_string_nonevent = "";
        public String max_pert_string_nonevent = "";
        public String min_pert_string_nonevent = "";

        public int GetCost()
        {
            return penalty_min_consecutive +
                    penalty_min_between +
                    penalty_max_consecutive +
                    penalty_max_between +
                    penalty_max_total +
                    penalty_min_total +
                    penalty_max_pert_nr +
                    penalty_min_pert_nr +

                    penalty_min_consecutive_nonevent +
                    penalty_min_between_nonevent +
                    penalty_max_consecutive_nonevent +
                    penalty_max_between_nonevent +
                    penalty_max_total_nonevent +
                    penalty_min_total_nonevent +
                    penalty_max_pert_nr_nonevent +
                    penalty_min_pert_nr_nonevent;
        }

        public void ResetCost()
        {
            //penalties
            penalty_max_total = 0;
            penalty_min_total = 0;

            penalty_max_pert_nr = 0;
            penalty_min_pert_nr = 0;
            penalty_max_between = 0;
            penalty_min_between = 0;
            penalty_max_consecutive = 0;
            penalty_min_consecutive = 0;


            penalty_max_total_nonevent = 0;
            penalty_min_total_nonevent = 0;

            penalty_max_pert_nr_nonevent = 0;
            penalty_min_pert_nr_nonevent = 0;
            penalty_max_between_nonevent = 0;
            penalty_min_between_nonevent = 0;
            penalty_max_consecutive_nonevent = 0;
            penalty_min_consecutive_nonevent = 0;
        }

        abstract protected void BuildNumbering(int numUnits, int numDays, Contract contract);
        abstract protected void BuildNumbering(int numUnits, int numDays, Contract contract, List<DayOff> dayOffRequests, DateTime startDate);
        abstract protected void BuildNumbering(int numUnits, int numDays, Contract contract, List<ShiftOff> shiftOffRequests, Dictionary<string, int> shiftTypesDict, DateTime startDate);



    }


}
