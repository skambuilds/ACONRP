using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACONRP
{
    public class Edge
    {
        public int IndexNurseA { get; set; }
        public int IndexNurseB { get; set; }
        public int IndexNodeA { get; set; }
        public int IndexNodeB { get; set; }
        public double Pheromone { get; set; }
    }
}