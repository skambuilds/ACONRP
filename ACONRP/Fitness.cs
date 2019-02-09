using System;

namespace ACONRP
{
    public class Fitness
    {
        public int UncoveredShifts { get; set; }
        public int TotalOverShift { get; set; }
        public int TotalSolutionCost { get; set; }

        public int CompleteFitnessValue
        {
            get
            {
                return UncoveredShifts + TotalOverShift + TotalSolutionCost;
            }
        }
        /// <summary>
        /// Returns an integer value for the comparison
        /// 1: if the first is better
        /// -1: if the second is better
        /// 0: if they are equal
        /// </summary>
        /// <param name="fitnessOne"></param>
        /// <param name="fitnessTwo"></param>
        /// <returns></returns>
        public static int FitnessCompare(Fitness fitnessOne, Fitness fitnessTwo)
        {
            if (fitnessOne.UncoveredShifts > fitnessTwo.UncoveredShifts) // fitnessTwo is better in this case
            {
                return -1;
            }
            else if (fitnessOne.UncoveredShifts < fitnessTwo.UncoveredShifts)
            {
                return 1;
            }
            if (fitnessOne.TotalOverShift > fitnessTwo.TotalOverShift)
            {
                return -1;
            }
            else if (fitnessOne.TotalOverShift < fitnessTwo.TotalOverShift)
            {
                return 1;
            }
            if (fitnessOne.TotalSolutionCost > fitnessTwo.TotalSolutionCost)
            {
                return -1;
            }
            else if (fitnessOne.TotalSolutionCost < fitnessTwo.TotalSolutionCost)
            {
                return 1;
            }

            return 0;
        }

    }
}