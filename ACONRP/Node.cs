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
        public bool[,] ShiftPatternMatrix { get; set; }
        public bool[] ShiftPatternArray { get; set; }
        public List<int> ShiftPatternSparse { get; set; }
        public double StaticHeuristicInfo { get; set; }
        public List<string> Violations { get; set; }
        /// <summary>
        /// Soft Constraint violation cost
        /// </summary>
        public int Cost { get; set; }
        /// <summary>
        /// Soft Constraint violation penalty
        /// </summary>
        public int Penalty { get; set; }
    }
}
