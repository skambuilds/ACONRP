using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACONRP
{
    public class Ant
    {
        private int id;
        public List<Node> Solution { get; set; }
        public int[,] CoverRequirements { get; set; }

        public static List<Ant> GenerateAnts(int numberOfAnts, int[,] initialCoverRequirements)
        {
            var listOfAnts = new List<Ant>();
            for (int i = 0; i < numberOfAnts; i++)
            {
                listOfAnts.Add(new Ant() { id = i, Solution = new List<Node>(), CoverRequirements = (int[,]) initialCoverRequirements.Clone() });
            }

            return listOfAnts;
        }
    }
}
