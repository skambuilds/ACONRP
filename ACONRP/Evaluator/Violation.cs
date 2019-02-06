using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACONRP.Evaluator
{
    public class Violation
    {
        private string ConstraintType { get; set;}
        private int Value { get; set; }
        private Condition condition;
        private int ViolationIndex { get; set; }
        private int Cost { get; set; }
        public int NodeNurseId { get; set; }
        public int NodeIndex { get; set; }

        public Violation(int nodeNurseId, int nodeIndex, string constraintType, int value, Condition condition, int violationIndex, int cost)
        {
            this.NodeNurseId = nodeNurseId;
            this.NodeIndex = nodeIndex;
            this.ConstraintType = constraintType;
            this.Value = value;
            this.condition = condition;
            this.ViolationIndex = violationIndex;
            this.Cost = cost;
        }

        public override String ToString()
        {
            String s = "";

            switch (this.ConstraintType)
            {
                case "MAX_CONSECUTIVE":
                    s = s + this.condition.max_consecutive_string;
                    break;
                case "MIN_CONSECUTIVE":
                    s = s + this.condition.min_consecutive_string;
                    break;
                case "MAX_BETWEEN":
                    s = s + this.condition.max_between_string;
                    break;
                case "MIN_BETWEEN":
                    s = s + this.condition.min_between_string;
                    break;
                case "MAX_PERT":
                    s = s + this.condition.max_pert_string;
                    break;
                case "MIN_PERT":
                    s = s + this.condition.min_pert_string;
                    break;
                case "MAX_TOTAL":
                    s = s + this.condition.max_total_string;
                    break;
                case "MIN_TOTAL":
                    s = s + this.condition.min_total_string;
                    break;
            }

            s = s + ", Node Nurse ID = " + NodeNurseId + " , Node Index = " + NodeIndex + " , Violation in excess = " + Value + " , pos = " + ViolationIndex + ", cost = " + Cost;            

            return s;
        }
    }
}