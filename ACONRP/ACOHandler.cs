using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACONRP
{
    public class ACOHandler
    {
        public GenerationManager GenerationManager { get; set; }
        public SchedulingPeriod InputData { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int[,] CoverRequirements { get; set; }
        public Dictionary<string, int> GradeIndex = new Dictionary<string, int>();
        public Dictionary<string, int> ShiftPatternIndex = new Dictionary<string, int>();
        public Dictionary<string, List<int>> DayIndex = new Dictionary<string, List<int>>
         {
           {"Monday", new List<int>() },
           {"Tuesday", new List<int>() },
           {"Wednesday", new List<int>() },
           {"Thursday", new List<int>() },
           {"Friday", new List<int>() },
           {"Saturday", new List<int>() },
           {"Sunday", new List<int>() }
        };

        /// <summary>
        /// Initializes the edges structure following the provided solution.
        /// In this phase the edge pheromone is set to the fitness value of the objective function using the provided solution
        /// </summary>
        /// <param name="mainSolution">Solution derived from the static heuristic info</param>
        /// <param name="edges">Empty list of edges</param>
        public void InitializeLocalPheromone(List<Node> mainSolution, List<Edge> edges)
        {
            var fitnessValue = ApplySolution(mainSolution).Item1;
            for (int i = 0; i < mainSolution.Count; i++)
            {
                if (mainSolution.Last() == mainSolution[i])
                    break;

                edges.Add(new Edge
                {
                    IndexNurseA = i,
                    IndexNodeA = mainSolution[i].Index,
                    IndexNurseB = i + 1,
                    IndexNodeB = mainSolution[i + 1].Index,
                    Pheromone = 1.0 / (fitnessValue + 1)
                });
            }
        }


        /// <summary>
        /// Returns a tuple where:
        ///     the first element is the sum of the uncovered requirement shifts
        ///     the second element is a matrix computed by the difference between the original
        ///     cover requirements and the provided solution
        /// </summary>
        /// <param name="mainSolution">A list of nodes that rappresents a complete solution</param>
        /// <returns></returns>
        public Tuple<int, int[,]> ApplySolution(List<Node> mainSolution)
        {
            int objectiveFunctionValue = 0;
            int[,] coverRequirements = (int[,])CoverRequirements.Clone(); ;

            //#if DEBUG
            //            Console.WriteLine("Cover requirements before applying the solution");
            //            for (int i = 0; i < coverRequirements.GetLength(0); i++)
            //            {
            //                for (int j = 0; j < coverRequirements.GetLength(1); j++)
            //                {
            //                    Console.Write($" {(coverRequirements[i, j])} ");
            //                }
            //                Console.Write("\n");
            //            }
            //            Console.Write("\n");
            //#endif

            foreach (Node node in mainSolution)
            {
                for (int i = 0; i < node.ShiftPattern.GetLength(0); i++)
                {
                    for (int j = 0; j < node.ShiftPattern.GetLength(1); j++)
                    {
                        int uncoveredShifts = coverRequirements[i, j] - ((node.ShiftPattern[i, j]) ? 1 : 0);
                        coverRequirements[i, j] = (uncoveredShifts < 0) ? 0 : uncoveredShifts;
                    }
                }
            }

            for (int i = 0; i < coverRequirements.GetLength(0); i++)
            {
                for (int j = 0; j < coverRequirements.GetLength(1); j++)
                {
                    objectiveFunctionValue += coverRequirements[i, j];
                }
            }

            //#if DEBUG
            //            Console.WriteLine("Cover requirements after applying the solution");
            //            for (int i = 0; i < coverRequirements.GetLength(0); i++)
            //            {
            //                for (int j = 0; j < coverRequirements.GetLength(1); j++)
            //                {
            //                    Console.Write($" {(coverRequirements[i, j])} ");
            //                }
            //                Console.Write("\n");
            //            }
            //#endif
            return new Tuple<int, int[,]>(objectiveFunctionValue, coverRequirements);
        }

        /// <summary>
        /// Returns a solution as a list that is made of the best nodes per single employee.
        /// e.i. the best node of a list of nodes is the one with the maximum static heuristic info
        /// </summary>
        /// <param name="nodes">Array of list of nodes where StaticHeuristicInfo has been initialized</param>
        /// <returns></returns>
        public List<Node> ExtractSolution(List<Node>[] nodes)
        {
            List<Node> solution = new List<Node>();

            for (int i = 0; i < nodes.Length; i++)
            {
                solution.Add(nodes[i].Aggregate((agg, next) => next.StaticHeuristicInfo > agg.StaticHeuristicInfo ? next : agg));
            }

            return solution;
        }

        public Dictionary<string, int> SordedDays = new Dictionary<string, int>() {
            { "Monday", 0 }, {"Tuesday", 1 }, {"Wednesday", 2 }, {"Thursday", 3 }, {"Friday", 4 }, {"Saturday", 5 }, {"Sunday", 6 }
        };

        public double[] WParam { get; set; }
        public int NurseNumber { get; set; }


        public ACOHandler(SchedulingPeriod inputData)
        {
            InputData = inputData;
            StartDate = DateTime.Parse(InputData.StartDate);
            EndDate = DateTime.Parse(InputData.EndDate);

            NurseNumber = inputData.Employees.Employee.Count;
            GenerationManager = new GenerationManager(InputData);

            WParam = new double[inputData.Skills.Skill.Count];
            for (int i = 0; i < inputData.Skills.Skill.Count; i++)
            {
                GradeIndex.Add(inputData.Skills.Skill.ElementAt(i), i);
                WParam[i] = i + 1;
            }

            var orderedShifts = inputData.ShiftTypes.Shift.OrderBy(x => DateTime.Parse(x.StartTime)).ThenBy(x => DateTime.Parse(x.EndTime)).ToList();
            orderedShifts.ForEach(x => ShiftPatternIndex.Add(x.ID, orderedShifts.IndexOf(x)));

            for (int i = 0; i < EndDate.Subtract(StartDate).Days + 1; i++)
            {
                DayIndex[StartDate.AddDays(i).DayOfWeek.ToString()].Add(i);
            }

            InitiazeCoverRequirements();
        }

        /// <summary>
        /// Initializes the matrix of cover requirements using InputData
        /// </summary>
        private void InitiazeCoverRequirements()
        {
            int periodInDays = EndDate.Subtract(StartDate).Days + 1;
            CoverRequirements = new int[InputData.ShiftTypes.Shift.Count, periodInDays];
            foreach (var coverDay in InputData.CoverRequirements.DayOfWeekCover)
            {
                foreach (var coverShift in coverDay.Cover)
                {
                    var shiftIndex = ShiftPatternIndex[coverShift.Shift];
                    var coverReqValue = int.Parse(coverShift.Preferred);
                    DayIndex[coverDay.Day].ForEach(index => CoverRequirements[shiftIndex, index] = coverReqValue);
                }
            }
        }

        public void ComputeStaticHeuristic(List<Node>[] nodes)
        {
            var contracts = InputData.Contracts.Contract;
            var patterns = InputData.Patterns.Pattern;
            var nurses = InputData.Employees.Employee;

            foreach (var nurse in nurses)
            {
                var unwantedPatterns = contracts.FirstOrDefault(contract => contract.ID == nurse.ContractID).UnwantedPatterns.Pattern;
                var unwantedContractPatterns = patterns.Where(pattern => unwantedPatterns.Contains(pattern.ID)).ToList();

                var unwantedShift = unwantedContractPatterns.Where(x => x.PatternEntries.PatternEntry.Any(y => y.ShiftType.Length == 1)).ToList();
                var unwantedDays = unwantedContractPatterns.Where(x => x.PatternEntries.PatternEntry.Any(y => y.ShiftType.Length > 1)).ToList();

                var nurseIndex = nurses.IndexOf(nurse);

                var costsUnwantedShift = HandleUnwantedShift(unwantedShift, nodes[nurseIndex]);
                var costsUnwantedDay = HandleUnwantedDays(unwantedDays, nodes[nurseIndex]);


                for (int i = 0; i < costsUnwantedDay.Count; i++)
                {
                    double cost = costsUnwantedShift.ElementAt(i) + costsUnwantedDay.ElementAt(i);
                    nodes[nurseIndex].ElementAt(i).StaticHeuristicInfo = 1.0 / (1.0 + cost);
                }
            }

        }

        private List<double> HandleUnwantedDays(List<Pattern> unwantedDays, List<Node> nodes)
        {
            List<double> costs = new List<double>();

            foreach (var node in nodes)
            {
                double totalCost = 0;
                foreach (var pattern in unwantedDays)
                {
                    var weight = double.Parse(pattern.Weight);

                    var occurences = CheckDays(pattern.PatternEntries.PatternEntry, node.ShiftPattern);
                    totalCost += weight * occurences;
                }
                costs.Add(totalCost);
            }

            return costs;
        }

        private int CheckDays(List<PatternEntry> patternEntries, bool[,] nursePattern)
        {
            int startWeekIndex = 0;
            DateTime startWeekDate = new DateTime(StartDate.Ticks);
            int daysUntilSunday = ((int)DayOfWeek.Sunday - (int)StartDate.DayOfWeek + 7) % 7;

            int endWeekIndex = daysUntilSunday + startWeekIndex;

            DateTime endWeekDate = startWeekDate.AddDays(daysUntilSunday);

            var days = new List<String>();
            patternEntries.ForEach(x => days.Add(x.Day));

            if (!CheckDaysOnInterval(startWeekDate, endWeekDate, days))
            {
                //moving to the next week
                startWeekDate = endWeekDate.AddDays(1);
                startWeekIndex = endWeekIndex + 1;
                endWeekIndex = startWeekIndex + 6;
                endWeekDate = startWeekDate.AddDays(6);
            }

            var patternOccurences = 0;
            do
            {
                var confirmedPattern = 0;
                foreach (var patternEntry in patternEntries)
                {
                    int? indexDay = DayIndex[patternEntry.Day].Where(x => x >= startWeekIndex && x <= endWeekIndex).FirstOrDefault();
                    if (!indexDay.HasValue)
                        break;

                    if (patternEntry.ShiftType == "Any" && !Utils.GetColumn<bool>(nursePattern, indexDay.Value).Contains(true))
                        break;

                    if (patternEntry.ShiftType == "None" && Utils.GetColumn<bool>(nursePattern, indexDay.Value).Contains(true))
                        break;

                    confirmedPattern++;
                }
                if (confirmedPattern == patternEntries.Count)
                    patternOccurences++;

                startWeekDate = endWeekDate.AddDays(1);
                startWeekIndex = endWeekIndex + 1;
                endWeekIndex = startWeekIndex + 6;
                endWeekDate = startWeekDate.AddDays(6);
            } while (endWeekDate <= EndDate);

            return patternOccurences;
        }

        private int GetDaysTo(DateTime startDate, string day)
        {
            for (int i = 0; i < 7; i++)
            {
                if (startDate.AddDays(i).DayOfWeek.ToString() == day)
                    return i;
            }

            return 7;
        }



        private bool CheckDaysOnInterval(DateTime startDate, DateTime endDate, List<String> days)
        {
            foreach (var day in days)
            {
                bool dayFound = false;
                for (int i = 0; i < endDate.Subtract(startDate).Days + 1; i++)
                {
                    if (startDate.AddDays(i).DayOfWeek.ToString().ToLower() == day.ToLower())
                    {
                        dayFound = true;
                        break;
                    }
                }
                if (!dayFound)
                    return false;
            }

            return true;
        }

        public List<double> HandleUnwantedShift(List<Pattern> unwantedShift, List<Node> nodes)
        {
            List<double> costs = new List<double>();

            List<UnwantedShift> assignmentPosition = new List<UnwantedShift>();
            foreach (var node in nodes)
            {

                double totalCost = 0;

                foreach (var pattern in unwantedShift)
                {
                    UnwantedShift shift = new UnwantedShift(pattern, ShiftPatternIndex);
                    totalCost += shift.Check(node.ShiftPattern);
                }

                costs.Add(totalCost);
            }

            return costs;
        }
    }

    public class UnwantedShift
    {
        public List<int> AssignmentPosition { get; set; } = new List<int>();
        public bool IsTrue { get; set; }
        public bool IsCheked { get; set; }
        public double Weight { get; set; }

        public double Check(bool[,] nursePattern)
        {
            int patternOccurences = 0;
            for (int i = 0; i < nursePattern.GetLength(1); i++)
            {
                for (int j = 0; j < AssignmentPosition.Count && i + j < nursePattern.GetLength(1); j++)
                {
                    IsCheked = false;
                    if (!nursePattern[AssignmentPosition.ElementAt(j), i + j])
                    {
                        IsCheked = true;
                    }
                    else
                    {
                        IsCheked = j < AssignmentPosition.Count - 1;
                        continue;
                    }

                    if (IsCheked)
                        break;
                }

                if (!IsCheked)
                {
                    patternOccurences++;
                    IsCheked = false;
                }
            }
            IsCheked = true;
            IsTrue = patternOccurences > 0;
            return (IsTrue) ? Weight * patternOccurences : 0;
        }

        public UnwantedShift(Pattern pattern, Dictionary<string, int> shiftPatternIndex)
        {
            Weight = double.Parse(pattern.Weight);

            var i = 0;
            foreach (var patternEntry in pattern.PatternEntries.PatternEntry)
            {
                var position = shiftPatternIndex[patternEntry.ShiftType];


                if (patternEntry.Day == "None")//TODO: This case is not contemplated in the instances but its handling could be required in the Check method
                    position *= -1;

                AssignmentPosition.Add(position);
                i++;
            }
        }
    }
}
