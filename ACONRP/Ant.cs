using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACONRP
{
    public class Ant
    {        
        public List<Node> Solution { get; set; }
        public int[,] CoverRequirements { get; set; }
        public int[] CoverRequirementsArray { get; set; }

        public static List<Ant> GenerateAnts(int numberOfAnts, int[,] initialCoverRequirements, int[] initialCoverReqArray)
        {
            var listOfAnts = new List<Ant>();
            for (int i = 0; i < numberOfAnts; i++)
            {
                listOfAnts.Add(new Ant() {Solution = new List<Node>(), CoverRequirements = (int[,]) initialCoverRequirements.Clone(), CoverRequirementsArray = (int[]) initialCoverReqArray.Clone() });
            }

            return listOfAnts;
        }
    }
}
