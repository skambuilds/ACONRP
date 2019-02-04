using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ACONRP.Evaluator;

namespace ACONRP
{
    public class ACOHandler
    {
        public const double PARAM_ALPHA = 1.0;
        public const double PARAM_BETA = 2.0;
        public const double PARAM_Q0 = 0.9;
        public const double PARAM_EPSILON = 0.1;
        public const double PARAM_RHO = 0.1;
        public const double PARAM_LAMBDA = 200.0;
        /// <summary>
        /// Over assignment penalty
        /// </summary>
        private const double PARAM_GAMMA = 1.5; 

        public GenerationManager GenerationManager { get; set; }
        public Evalutador Evaluator { get; set; }
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
        private double Pheromone_0 { get; set; }
        private double CurrentPheromone_0 { get; set; }


        public int NodeSelection(Tuple<double, int[,]>[] heuristicInformation, List<Node>[] nodes, List<Edge> edges, int nurseDestinationIdex)
        {
            int nurseSourceIndex = nurseDestinationIdex - 1;

            int nodesOnSource = 1;
            if (nurseSourceIndex >= 0) nodesOnSource = nodes[nurseSourceIndex].Count;
            int nodesOnDestination = nodes[nurseDestinationIdex].Count;

            //proability p(i,j)

            var probability = new double[nodesOnDestination];

            //Number of edges calculation in order to calculate the probability function
            int numberOfEdges = nodesOnSource * nodesOnDestination;

            List<Edge> edgesFound = edges.Where(ed => ed.IndexNurseA == nurseSourceIndex && ed.IndexNurseB == nurseDestinationIdex).ToList();

            var overallProbability = edges.Where(ed => ed.IndexNurseA == nurseSourceIndex && ed.IndexNurseB == nurseDestinationIdex)
                                            .Sum(x => Math.Pow(x.Pheromone, PARAM_ALPHA) * Math.Pow(heuristicInformation[x.IndexNodeB].Item1, PARAM_BETA));

            for (int i = 0; i < nodesOnDestination; i++)
            {
                if (CheckIndexNode(edgesFound, i))
                {
                    continue;
                }
                else overallProbability += Math.Pow(CurrentPheromone_0, PARAM_ALPHA) * Math.Pow(heuristicInformation[i].Item1, PARAM_BETA);
            }

            foreach (var edge in edges.Where(ed => ed.IndexNurseA == nurseSourceIndex && ed.IndexNurseB == nurseDestinationIdex))
            {
                probability[edge.IndexNodeB] = (Math.Pow(edge.Pheromone, PARAM_ALPHA) * Math.Pow(heuristicInformation[edge.IndexNodeB].Item1, PARAM_BETA)) / overallProbability;
            }

            for (int i = 0; i < nodesOnDestination; i++)
            {
                if (CheckIndexNode(edgesFound, i)) continue;
                else probability[i] += (Math.Pow(CurrentPheromone_0, PARAM_ALPHA) * Math.Pow(heuristicInformation[i].Item1, PARAM_BETA)) / overallProbability;
            }

            Random rnd = new Random();
            //pseudorandom proportional rule
            if (rnd.NextDouble() < PARAM_Q0)
            {
                double max = probability.Cast<double>().Max();
                return Array.IndexOf(probability, max);
            }
            else
            {
                return rnd.Next(0, probability.Count());
            }
        }

        private static bool CheckIndexNode(List<Edge> edgesFound, int i)
        {
            foreach (var edge in edgesFound)
            {
                if (edge.IndexNodeB == i) return true;
            }
            return false;
        }

        public int NodeSelectionOLD(Tuple<double, int[,]>[] heuristicInformation, List<Node>[] nodes, List<Edge> edges, int nurseDestinationIdex)
        {
            int nurseSourceIndex = nurseDestinationIdex - 1;
            int nodesOnDestination = nodes[nurseDestinationIdex].Count;

            //proability p(i,j)
            var probability = new double[nodesOnDestination];

            var overallProbability = edges.Where(ed => ed.IndexNurseA == nurseSourceIndex && ed.IndexNurseB == nurseDestinationIdex)
                                            .Sum(x => Math.Pow(x.Pheromone, PARAM_ALPHA) * Math.Pow(heuristicInformation[x.IndexNodeB].Item1, PARAM_BETA));

            foreach (var edge in edges.Where(ed => ed.IndexNurseA == nurseSourceIndex && ed.IndexNurseB == nurseDestinationIdex))
            {
                probability[edge.IndexNodeB] = (Math.Pow(edge.Pheromone, PARAM_ALPHA) * Math.Pow(heuristicInformation[edge.IndexNodeB].Item1, PARAM_BETA)) / overallProbability;
            }

            Random rnd = new Random();
            //pseudorandom proportional rule
            if (rnd.NextDouble() < PARAM_Q0)
            {
                double max = probability.Cast<double>().Max();
                return Array.IndexOf(probability, max);
            }
            else
            {
                return rnd.Next(0, probability.Count());
            }
        }

        internal void GlobalPheromoneUpdate(List<Node> mainSolution, int mainSolutionFitnessValue, List<Edge> edges)
        {
            foreach (var node in mainSolution)
            {
                if (mainSolution.Last() == node)
                    break;

                var nextNode = mainSolution.First(n => n.NurseId == node.NurseId + 1);

                var solutionEdge = edges.First(ed => ed.IndexNurseA == node.NurseId && ed.IndexNurseB == nextNode.NurseId && ed.IndexNodeA == node.Index && ed.IndexNodeB == nextNode.Index);
                solutionEdge.Pheromone = (1.0 - PARAM_RHO) * solutionEdge.Pheromone + PARAM_RHO * (1 / (1 + mainSolutionFitnessValue * PARAM_LAMBDA));

            }
        }

        internal void ListOfEdgesUpdate(List<Node> antSolution, List<Edge> edges)
        {

            List<Edge> firstEdge = edges.Where(ed => ed.IndexNurseA == -1 && ed.IndexNurseB == antSolution[0].NurseId && ed.IndexNodeA == 0 && ed.IndexNodeB == antSolution[0].Index).ToList();
            if (firstEdge.Count > 0)
            {
                foreach (Edge ed in firstEdge)
                {
                    ed.Pheromone = (1.0 - PARAM_EPSILON) * ed.Pheromone + PARAM_EPSILON * Pheromone_0;
                }
            }
            else
            {
                edges.Add(new Edge()
                {
                    IndexNodeA = 0,
                    IndexNurseA = -1,
                    IndexNurseB = antSolution[0].NurseId,
                    IndexNodeB = antSolution[0].Index,
                    Pheromone = Pheromone_0
                });
            }

            foreach (var node in antSolution)
            {
                if (antSolution.Last() == node)
                    break;

                var nextNode = antSolution.First(n => n.NurseId == node.NurseId + 1);

                List<Edge> solutionEdge = edges.Where(ed => ed.IndexNurseA == node.NurseId && ed.IndexNurseB == nextNode.NurseId && ed.IndexNodeA == node.Index && ed.IndexNodeB == nextNode.Index).ToList();
                if (solutionEdge.Count > 0)
                {
                    foreach (Edge ed in solutionEdge)
                    {
                        ed.Pheromone = (1.0 - PARAM_EPSILON) * ed.Pheromone + PARAM_EPSILON * Pheromone_0;
                    }
                }
                else
                {
                    edges.Add(new Edge()
                    {
                        IndexNodeA = node.Index,
                        IndexNurseA = node.NurseId,
                        IndexNurseB = nextNode.NurseId,
                        IndexNodeB = nextNode.Index,
                        Pheromone = Pheromone_0
                    });
                }
            }
        }
        /// <summary>
        /// Local pheromone update is applied on the edges that belongs to the mainSolution
        /// </summary>
        /// <param name="mainSolution"></param>
        /// <param name="edges"></param>
        internal void LocalPheromoneUpdate(List<Node> mainSolution, List<Edge> edges)
        {
            var solutionEdgeList = new List<Edge>();
            foreach (var node in mainSolution)
            {
                if (mainSolution.Last() == node)
                    break;

                var nextNode = mainSolution.First(n => n.NurseId == node.NurseId + 1);

                var solutionEdge = edges.First(ed => ed.IndexNurseA == node.NurseId && ed.IndexNurseB == nextNode.NurseId && ed.IndexNodeA == node.Index && ed.IndexNodeB == nextNode.Index);
                solutionEdge.Pheromone = (1.0 - PARAM_EPSILON) * solutionEdge.Pheromone + PARAM_EPSILON * Pheromone_0;
                solutionEdgeList.Add(solutionEdge);

            }

            //simple pheromone evaporation update
            edges.Except(solutionEdgeList).ToList().ForEach(x => x.Pheromone = (1.0 - PARAM_EPSILON) * x.Pheromone);
            CurrentPheromone_0 = ((1.0 - PARAM_EPSILON) * CurrentPheromone_0);
        }
        internal void LocalPheromoneUpdateOLD(List<Node> mainSolution, List<Edge> edges)
        {
            var solutionEdgeList = new List<Edge>();
            foreach (var node in mainSolution)
            {
                if (mainSolution.Last() == node)
                    break;

                var nextNode = mainSolution.First(n => n.NurseId == node.NurseId + 1);

                var solutionEdge = edges.First(ed => ed.IndexNurseA == node.NurseId && ed.IndexNurseB == nextNode.NurseId && ed.IndexNodeA == node.Index && ed.IndexNodeB == nextNode.Index);
                solutionEdge.Pheromone = (1.0 - PARAM_EPSILON) * solutionEdge.Pheromone + PARAM_EPSILON * Pheromone_0;
                solutionEdgeList.Add(solutionEdge);
            }

            //simple pheromone evaporation update
            edges.Except(solutionEdgeList).ToList().ForEach(x => x.Pheromone = (1.0 - PARAM_EPSILON) * x.Pheromone);
        }

        /// <summary>
        /// Returns an arrray of couples where:
        ///     The first element denotes the value of the heuristic information of the node (static*dynamic)
        ///     The second element is the resulting cover requirements matrix after the application of the node ShiftPattern
        /// </summary>
        /// <param name="nodes"></param>
        /// <param name="coverRequirements"></param>
        /// <returns></returns>
        public Tuple<double, int[,]>[] ComputeHeuristicInfo(List<Node> nodes, int[,] coverRequirements)
        {
            var heuristicInfo = new Tuple<double, int[,]>[nodes.Count];
            foreach (var node in nodes)
            {
                heuristicInfo[node.Index] = ApplySingleSolution(node, (int[,])coverRequirements.Clone());
            }

            return heuristicInfo;
        }

        internal void InitializeStandardPheromone(int fitnessValue)
        {
            Pheromone_0 = 1.0 / (fitnessValue + 1.0);
            CurrentPheromone_0 = Pheromone_0;
        }

        internal void InitializeLocalPheromone(List<Node>[] nodes, int fitnessValue, List<Edge> edges)
        {
            Pheromone_0 = 1.0 / (fitnessValue + 1.0);

            //All the edges between the v0 node and the very first node of the solution are added here
            for (int i = 0; i < nodes[0].Count; i++)
            {
                edges.Add(new Edge()
                {
                    IndexNodeA = 0,
                    IndexNurseA = -1,
                    IndexNurseB = 0,
                    IndexNodeB = i,
                    Pheromone = Pheromone_0
                });
            }

            for (int k = 0; k < nodes.Count() - 1; k++)
            {
                int indexNurseA = k;
                int indexNurseB = k + 1;

                for (int i = 0; i < nodes[indexNurseA].Count; i++)
                {
                    for (int j = 0; j < nodes[indexNurseB].Count; j++)
                    {
                        edges.Add(new Edge()
                        {
                            IndexNodeA = i,
                            IndexNodeB = j,
                            IndexNurseA = indexNurseA,
                            IndexNurseB = indexNurseB,
                            Pheromone = Pheromone_0
                        });
                    }
                }
            }
        }

        ///// <summary>
        ///// Initializes the edges structure following the provided solution.
        ///// In this phase the edge pheromone is set to the fitness value of the objective function using the provided solution
        ///// </summary>
        ///// <param name="mainSolution">Solution derived from the static heuristic info</param>
        ///// <param name="edges">Empty list of edges</param>
        //public void InitializeLocalPheromone(List<Node> mainSolution, List<Edge> edges)
        //{
        //    var fitnessValue = ApplySolution(mainSolution).Item1;

        //    //Special Edges that link the imaginary v0 node (ants node) the the first node of the solution

        //    edges.Add(
        //        new Edge()
        //        {
        //            IndexNodeA = 0,
        //            IndexNurseA = -1,
        //            IndexNurseB = 0,
        //            IndexNodeB = mainSolution.First(x => x.NurseId == 0).Index,
        //            Pheromone = 1.0 / (fitnessValue + 1)
        //        });

        //    for (int i = 0; i < mainSolution.Count; i++)
        //    {
        //        if (mainSolution.Last() == mainSolution[i])
        //            break;

        //        edges.Add(new Edge
        //        {
        //            IndexNurseA = i,
        //            IndexNodeA = mainSolution[i].Index,
        //            IndexNurseB = i + 1,
        //            IndexNodeB = mainSolution[i + 1].Index,
        //            Pheromone = 1.0 / (fitnessValue + 1)
        //        });
        //    }
        //}

        public Tuple<double, int[,]> ApplySingleSolution(Node node, int[,] coverRequirements)
        {
            int[,] coverReqUpdated = new int[coverRequirements.GetLength(0), coverRequirements.GetLength(1)];
            //Console.WriteLine("Starting cover requirements matrix:\n");
            //PrintCoverRequirements(coverRequirements);
            double coveredShifts = 0;
            double overAssignment = 0;
            for (int i = 0; i < node.ShiftPattern.GetLength(0); i++)
            {
                for (int j = 0; j < node.ShiftPattern.GetLength(1); j++)
                {
                    int uncoverQuantity = coverRequirements[i, j] - ((node.ShiftPattern[i, j]) ? 1 : 0);
                    if (uncoverQuantity >= 0)
                    {
                        coveredShifts += coverRequirements[i, j] - uncoverQuantity;
                    }
                    else
                    {
                        //If an over assignment has been found then the value of coveredShift will be decrease properly
                        //coveredShifts += coverRequirements[i, j] - (Math.Abs(coverRequirements[i, j] - uncoverQuantity));
                        overAssignment++;
                    }
                    //coveredShifts += Math.Abs(uncoverQuantity);
                    coverReqUpdated[i, j] = (uncoverQuantity > 0) ? uncoverQuantity : 0;
                }
            }
            //If the number of covered shifts is negative then some over assignments have been identified, therefore the total value can't be more then 0.
            double coefOverAssignment = overAssignment * PARAM_GAMMA;
            coveredShifts -= coefOverAssignment;
            coveredShifts = (coveredShifts < 0) ? 0 : coveredShifts;
            //Console.WriteLine("Updated cover requirements matrix:\n");
            //PrintCoverRequirements(coverReqUpdated);
            return new Tuple<double, int[,]>(node.StaticHeuristicInfo * coveredShifts, coverReqUpdated);
        }
        public void PrintCoverRequirements(int[,] coverRequirements)
        {
            for (int i = 0; i < coverRequirements.GetLength(0); i++)
            {
                for (int j = 0; j < coverRequirements.GetLength(1); j++)
                {
                    Console.Write($" {coverRequirements[i, j]}");
                }
                Console.Write("\n");
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
        public Tuple<int, int, int[,]> ApplySolution(List<Node> mainSolution)
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

            int totalOvershift = 0; //the nurse has been assign to a shift that do not require any nurse
            //Console.WriteLine("Starting cover requirements matrix:\n");
            //PrintCoverRequirements(coverRequirements);
            foreach (Node node in mainSolution)
            {
                for (int i = 0; i < node.ShiftPattern.GetLength(0); i++)
                {
                    for (int j = 0; j < node.ShiftPattern.GetLength(1); j++)
                    {
                        int uncoveredShifts = coverRequirements[i, j] - ((node.ShiftPattern[i, j]) ? 1 : 0);
                        totalOvershift += (uncoveredShifts < 0) ? 1 : 0;
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

            //Console.WriteLine("Updated cover requirements matrix:\n");
            //PrintCoverRequirements(coverRequirements);

            return new Tuple<int, int, int[,]>(objectiveFunctionValue + totalOvershift, totalOvershift, coverRequirements);
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
            Evaluator = new Evalutador(InputData);

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

                var costSoftConst = Evaluator.CalculatePenalty(nodes[nurseIndex]);

                for (int i = 0; i < costsUnwantedDay.Count; i++)
                {
                    double cost = costsUnwantedShift.ElementAt(i) + costsUnwantedDay.ElementAt(i) + costSoftConst.ElementAt(i);
                    Console.WriteLine($"------------------------>\t COSTO : {costsUnwantedShift.ElementAt(i)} + {costsUnwantedDay.ElementAt(i)} + {costSoftConst.ElementAt(i)} = {cost}");
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
