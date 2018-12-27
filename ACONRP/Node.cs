using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACONRP
{
    public class Node
    {
        private int nurseId;
        private int index;
        private bool[,] shiftPattern;
        private double staticHeuristicInfo;

        public int NurseId
        {
            get
            {
                return nurseId;
            }
            set
            {
                nurseId = value;
            }
        }
        public int Index
        {
            get
            {
                return index;
            }
            set
            {
                index = value;
            }
        }
        public bool[,] ShiftPattern
        {
            get
            {
                return shiftPattern;
            }
            set
            {
                shiftPattern = value;
            }
        }
        public double StaticHeuristicInfo
        {
            get
            {
                return staticHeuristicInfo;
            }
            set
            {
                staticHeuristicInfo = value;
            }

        }
    }
}
