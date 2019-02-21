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
        /// <summary>
        /// Dinamic heuristic exponential weight on eta_s * eta_d
        /// </summary>
        public const double PARAM_DELTA = 4;
        public const double PARAM_Q0 = 0.9;
        public const double PARAM_EPSILON = 0.1;
        public const double PARAM_RHO = 0.1;
        public const double PARAM_LAMBDA = 200.0;
        /// <summary>
        /// Over assignment penalty
        /// </summary>
        private const double PARAM_GAMMA = 1; //*

        public GenerationManager GenerationManager { get; set; }
        public Evalutador Evaluator { get; set; }
        public SchedulingPeriod InputData { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int[,] CoverRequirements { get; set; }
        public int[] CoverRequirementsArray { get; set; }
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

            var orderedShifts = inputData.ShiftTypes.Shift.ToList();
            orderedShifts.ForEach(x => ShiftPatternIndex.Add(x.ID, orderedShifts.IndexOf(x)));

            for (int i = 0; i < EndDate.Subtract(StartDate).Days + 1; i++)
            {
                DayIndex[StartDate.AddDays(i).DayOfWeek.ToString()].Add(i);
            }

            InitiazeCoverRequirements();
        }

        /// <summary>
        /// Performs the ant selection of the node after the calculation of the probability function and the pseudorandom proportional rule
        /// </summary>
        /// <param name="heuristicInformation"></param>
        /// <param name="nodes"></param>
        /// <param name="edges"></param>
        /// <param name="nurseDestinationIdex"></param>
        /// <returns></returns>
        public int NodeSelection(Tuple<double, int[]>[] heuristicInformation, List<Node>[] nodes, List<Edge> edges, int nurseDestinationIdex)
        {
            int nurseSourceIndex = nurseDestinationIdex - 1;

            int nodesOnSource = 1;
            if (nurseSourceIndex >= 0) nodesOnSource = nodes[nurseSourceIndex].Count;
            int nodesOnDestination = nodes[nurseDestinationIdex].Count;

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
            //Pseudorandom proportional rule
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
        /// <summary>
        /// Checks if the index belongs to the list of edges
        /// </summary>
        /// <param name="edgesFound"></param>
        /// <param name="i"></param>
        /// <returns></returns>
        private static bool CheckIndexNode(List<Edge> edgesFound, int i)
        {
            foreach (var edge in edgesFound)
            {
                if (edge.IndexNodeB == i) return true;
            }
            return false;
        }
        [Obsolete("Node selection old version - now outdated")]
        public int NodeSelectionOLD(Tuple<double, int[,]>[] heuristicInformation, List<Node>[] nodes, List<Edge> edges, int nurseDestinationIdex)
        {
            int nurseSourceIndex = nurseDestinationIdex - 1;
            int nodesOnDestination = nodes[nurseDestinationIdex].Count;

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

        internal void GlobalPheromoneUpdate(List<Node> mainSolution, Fitness mainSolutionFitnessValue, List<Edge> edges)
        {
            foreach (var node in mainSolution)
            {
                if (mainSolution.Last() == node)
                    break;

                var nextNode = mainSolution.First(n => n.NurseId == node.NurseId + 1);

                var solutionEdge = edges.First(ed => ed.IndexNurseA == node.NurseId && ed.IndexNurseB == nextNode.NurseId && ed.IndexNodeA == node.Index && ed.IndexNodeB == nextNode.Index);
                solutionEdge.Pheromone = (1.0 - PARAM_RHO) * solutionEdge.Pheromone + PARAM_RHO * (1 / (1 + mainSolutionFitnessValue.CompleteFitnessValue * PARAM_LAMBDA));

            }
        }
        /// <summary>
        /// Performs the pheromone update on the already existing edges or adds a new edge to the list
        /// </summary>
        /// <param name="antSolution"></param>
        /// <param name="edges"></param>
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

            //Simple pheromone evaporation update
            edges.Except(solutionEdgeList).ToList().ForEach(x => x.Pheromone = (1.0 - PARAM_EPSILON) * x.Pheromone);
            CurrentPheromone_0 = ((1.0 - PARAM_EPSILON) * CurrentPheromone_0);
        }

        /// <summary>
        /// Returns an arrray of couples where:
        ///     The first element denotes the value of the heuristic information of the node (static*dynamic)
        ///     The second element is the resulting cover requirements array after the application of the node ShiftPattern
        /// </summary>
        /// <param name="nodes"></param>
        /// <param name="coverRequirements"></param>
        /// <returns></returns>
        public Tuple<double, int[]>[] ComputeHeuristicInfo(List<Node> nodes, int[] coverRequirements)
        {
            var heuristicInfo = new Tuple<double, int[]>[nodes.Count];
            foreach (var node in nodes)
            {
                heuristicInfo[node.Index] = ApplySingleSolution(node, (int[])coverRequirements.Clone());
            }
            return heuristicInfo;
        }

        internal void InitializeStandardPheromone(Fitness fitness)
        {
            Pheromone_0 = 1.0 / (fitness.CompleteFitnessValue + 1.0);
            CurrentPheromone_0 = Pheromone_0;
        }

        public Tuple<double, int[]> ApplySingleSolution(Node node, int[] coverRequirements)
        {
            double coveredShifts = 0;
            double overAssignment = 0;

            foreach (int pos in node.ShiftPatternSparse)
            {
                int uncoverQuantity = coverRequirements[pos] - 1;
                if (uncoverQuantity >= 0) coveredShifts++;
                else overAssignment++;
                coverRequirements[pos] = (uncoverQuantity > 0) ? uncoverQuantity : 0;
            }

            double coefOverAssignment = overAssignment * PARAM_GAMMA;
            coveredShifts -= coefOverAssignment;
            //If the number of covered shifts is negative then there is some over assignments,
            //therefore the total value of covered shifts can't be more then 0.
            coveredShifts = (coveredShifts < 0) ? 0 : coveredShifts;
            return new Tuple<double, int[]>(node.StaticHeuristicInfo * Math.Pow(coveredShifts, PARAM_DELTA), coverRequirements);
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
        public Tuple<Fitness, int[]> ApplySolution(List<Node> mainSolution)
        {
            int uncoveredShifts = 0;
            int[] coverRequirements = (int[])CoverRequirementsArray.Clone();
            int totalSolutionCost = 0;
            //the nurse has been assign to a shift that do not require any nurse
            int totalOverShift = 0;

            foreach (Node node in mainSolution)
            {
                foreach (int pos in node.ShiftPatternSparse)
                {
                    int singleUncoveredShift = coverRequirements[pos] - 1;
                    totalOverShift += (singleUncoveredShift < 0) ? 1 : 0;
                    coverRequirements[pos] = (singleUncoveredShift < 0) ? 0 : singleUncoveredShift;
                }
                totalSolutionCost += node.Cost;
            }
            for (int i = 0; i < coverRequirements.Length; i++) uncoveredShifts += coverRequirements[i];


            Fitness fitnessSolution = new Fitness() { UncoveredShifts = uncoveredShifts, TotalOverShift = totalOverShift, TotalSolutionCost = totalSolutionCost };
            return new Tuple<Fitness, int[]>(fitnessSolution, coverRequirements);
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

        /// <summary>
        /// Initializes the matrix of cover requirements using InputData
        /// </summary>
        private void InitiazeCoverRequirements()
        {
            int periodInDays = EndDate.Subtract(StartDate).Days + 1;
            CoverRequirements = new int[InputData.ShiftTypes.Shift.Count, periodInDays];
            CoverRequirementsArray = new int[InputData.ShiftTypes.Shift.Count * periodInDays];

            foreach (var coverDay in InputData.CoverRequirements.DayOfWeekCover)
            {
                foreach (var coverShift in coverDay.Cover)
                {
                    var shiftIndex = ShiftPatternIndex[coverShift.Shift];
                    var coverReqValue = int.Parse(coverShift.Preferred);
                    DayIndex[coverDay.Day].ForEach(index => CoverRequirements[shiftIndex, index] = coverReqValue);
                }
            }
            //TODO: There is no more need of the cover requirements matrix, all the data can be stored in a single one dimension array
            MatrixToArrayConverter(CoverRequirements, CoverRequirementsArray);
        }

        private static void MatrixToArrayConverter(int[,] matrix, int[] array)
        {
            int k = 0;
            for (int i = 0; i != matrix.GetLength(1); i++)
            {
                for (int j = 0; j != matrix.GetLength(0); j++)
                {
                    array[k] = matrix[j, i];
                    k++;
                }
            }
        }
        /// <summary>
        /// Computes and saves on the node the static heuristic based on the violation cost computed during the shift pattern generation
        /// </summary>
        /// <param name="nodes"></param>
        public void ComputeStaticHeuristic(List<Node>[] nodes)
        {
            var contracts = InputData.Contracts.Contract;
            var patterns = InputData.Patterns.Pattern;
            var nurses = InputData.Employees.Employee;

            foreach (var nurse in nurses)
            {
                var nurseIndex = nurses.IndexOf(nurse);
                foreach (Node node in nodes[nurseIndex])
                    node.StaticHeuristicInfo = 1.0 / (1.0 + node.Cost);
            }
        }

    }
}
