using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ACONRP.Evaluator;

namespace ACONRP
{
    public class Node
    {
        public int NurseId { get; set; }
        public int Index { get; set; }
        public bool[,] ShiftPattern { get; set; }
        public double StaticHeuristicInfo { get; set; }
        public int Penalty { get; set; } //Soft Constraint violation penalty
    }
}
