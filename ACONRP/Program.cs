using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ACONRP
{
    public partial class Program
    {
        private const int NUM_ANTS = 30;
        private const int NO_IMPROVEMENT_LIMIT = 30;

        static void Main(string[] args)
        {            
            var inputData = InputData.GetObjectDataFromFile("Instances/Sprint/sprint02.xml");            
            ACOHandler handler = new ACOHandler(inputData);
            List<Node>[] nodes = handler.GenerationManager.GetShiftPatterns();
            
            //List<Node>[] nodes = NodeTest();
            List<Edge> edges = new List<Edge>();
            
            handler.ComputeStaticHeuristic(nodes);
            List<Node> mainSolution = handler.ExtractSolution(nodes);
            Fitness mainSolutionFitnessValue = handler.ApplySolution(mainSolution).Item1;
                    
            handler.InitializeStandardPheromone(mainSolutionFitnessValue);
            handler.ListOfEdgesUpdate(mainSolution, edges);

            int consecutiveNoImprovements = 0;
            Console.Write("Solution construction ");
            do
            {
                Console.Write(".");
                List<Ant> ants = Ant.GenerateAnts(NUM_ANTS, handler.CoverRequirements, handler.CoverRequirementsArray);
                foreach (var ant in ants)
                {
                    Console.Write(":");
                    for (int i = 0; i < handler.NurseNumber; i++)
                    {
                        //Compute the product between static heuristic and dynamic heuristic
                        var heuristicInformation = handler.ComputeHeuristicInfo(nodes[i], ant.CoverRequirementsArray);
                        var selectedIndex = handler.NodeSelection(heuristicInformation, nodes, edges, i);
                        var selectedNode = nodes[i].ElementAt(selectedIndex);
                        ant.Solution.Add(selectedNode);                        
                        ant.CoverRequirementsArray = heuristicInformation[selectedIndex].Item2;
                    }

                    Tuple<Fitness, int[]> antSolutionApplied = handler.ApplySolution(ant.Solution);
                    int[] antSolutionUpdatedCoverReq = antSolutionApplied.Item2;

                    //Edges update
                    handler.ListOfEdgesUpdate(ant.Solution, edges);

                    if (Fitness.FitnessCompare(antSolutionApplied.Item1, mainSolutionFitnessValue) == 1)
                    {                       
                        mainSolution = ant.Solution;
                        mainSolutionFitnessValue = antSolutionApplied.Item1;
                        consecutiveNoImprovements = 0;
                        Console.Write($" {antSolutionApplied.Item1.UncoveredShifts} + {antSolutionApplied.Item1.TotalOverShift} + {antSolutionApplied.Item1.TotalSolutionCost} ");
                    }
                    else
                    {
                        consecutiveNoImprovements++;
                    }
                    handler.LocalPheromoneUpdate(mainSolution, edges);
                }

                handler.GlobalPheromoneUpdate(mainSolution, mainSolutionFitnessValue, edges);

            } while (consecutiveNoImprovements < NO_IMPROVEMENT_LIMIT);

            handler.Evaluator.CalculateSolutionPenalty(mainSolution);
            handler.GenerationManager.PrintSolution(mainSolution, mainSolutionFitnessValue);

            Console.ReadKey();
        }        

        private static List<Node>[] NodeTest()
        {
            List<Node>[] nodes = new List<Node>[20] {
            new List<Node>(),
            new List<Node>(),
            new List<Node>(),
            new List<Node>(),
            new List<Node>(),
            new List<Node>(),
            new List<Node>(),
            new List<Node>(),
            new List<Node>(),
            new List<Node>(),
            new List<Node>(),
            new List<Node>(),
            new List<Node>(),
            new List<Node>(),
            new List<Node>(),
            new List<Node>(),
            new List<Node>(),
            new List<Node>(),
            new List<Node>(),
            new List<Node>(),
        };

            List<Node> nurse_0_nodes =
                new List<Node>()
                {
                    new Node(){
                        Index = 0, NurseId = 0, StaticHeuristicInfo = 0,
                        ShiftPatternMatrix = new bool[3,28]{
                            { false,    true,   true,   false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false },
                            { true,   false,  false,  true,   false,  false,  false,  false,  false,  false,  true,   true,   false,  false,  false,  false,  false,  false,  true,   true,   false,  false,  false,  false,  true,   false,  false,  false },
                            { false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  true }
                        }
                    }
                };

            List<Node> nurse_1_nodes =
               new List<Node>()
               {
                    new Node(){
                        Index = 0, NurseId = 1, StaticHeuristicInfo = 0,
                        ShiftPatternMatrix = new bool[3,28]{
                            { true, false,  false,  false,  false,  false,  true,   false,  false,  false,  true,   true,   false,  false,  false,  false,  false,  true,   false,  true,   false,  false,  false,  false,  false,  false,  false,  false },
                            { false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  true,   false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  true,   true,   false,  false },
                            { false,  false,  false,  false,  false,  true,   false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false }
                        }
                    }
                };

            List<Node> nurse_2_nodes =
               new List<Node>()
               {
                    new Node(){
                        Index = 0, NurseId = 2, StaticHeuristicInfo = 0,
                        ShiftPatternMatrix = new bool[3,28]{
                            { false,  false,  false,  true,   false,  false,  false,  false,  false,  false,  false,  true,   false,  true,   false,  false,  false,  false,  true,   true,   false,  false,  false,  false,  true,   false,  false,  false },
                            { false,  false,  false,  false,  true,   false,  false,  false,  false,  false,  false,  false,  true,   false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  true,   false,  false },
                            { false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  true,   false,  false,  false,  true,   false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false }
                        }
                    }
                };

            List<Node> nurse_3_nodes =
               new List<Node>()
               {
                    new Node(){
                        Index = 0, NurseId = 3, StaticHeuristicInfo = 0,
                        ShiftPatternMatrix = new bool[3,28]{
                            { false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  true,   true,   false,  false,  false,  false,  true,   true,   false,  false },
                            { false,  false,  false,  true,   true,   false,  false,  true,   false,  false,  false,  false,  false,  true,   false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false },
                            { false,  false,  false,  false,  false,  false,  true,   false,  false,  false,  false,  false,  true,   false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false }
                        }
                    }
                };
            List<Node> nurse_4_nodes =
               new List<Node>()
               {
                    new Node(){
                        Index = 0, NurseId = 4, StaticHeuristicInfo = 0,
                        ShiftPatternMatrix = new bool[3,28]{
                            { false,    false,  false,  false,  true,   false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  true,   false,  false,  false,  false,  true,   false,  false,  false,  false,  false },
                            { true,   false,  false,  false,  false,  true,   false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  true,   false,  false,  false,  true },
                            { false,  false,  false,  false,  false,  false,  false,  false,  true,   true,   false,  false,  false,  false,  false,  false,  true,   false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false }
                        }
                    }
                };
            List<Node> nurse_5_nodes =
               new List<Node>()
               {
                    new Node(){
                        Index = 0, NurseId = 5, StaticHeuristicInfo = 0,
                        ShiftPatternMatrix = new bool[3,28]{
                            { true, false,  false,  false,  false,  false,  false,  true,   false,  false,  false,  false,  false,  true,   false,  false,  false,  false,  false,  false,  false,  false,  true,   true,   false,  false,  false,  true },
                            { false,  true,   true,   false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  true,   false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false },
                            { true,   false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false }
                        }
                    }
                };
            List<Node> nurse_6_nodes =
               new List<Node>()
               {
                    new Node(){
                        Index = 0, NurseId = 6, StaticHeuristicInfo = 0,
                        ShiftPatternMatrix = new bool[3,28]{
                            { true, false,  false,  false,  false,  false,  true,   true,   false,  false,  true,   false,  false,  false,  false,  false,  false,  true,   false,  false,  false,  false,  false,  true,   false,  false,  false,  true },
                            {  false,  true,   true,   false,  false,  false,  false,  false,  false,  false,  false,  true,   false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false },
                            { false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  true,   false,  false,  false,  false,  false }
                         }
                    }
                };
            List<Node> nurse_7_nodes =
               new List<Node>()
               {
                    new Node(){
                        Index = 0, NurseId = 7, StaticHeuristicInfo = 0,
                        ShiftPatternMatrix = new bool[3,28]{
                            { false,    true,   true,   false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  true },
                            { true,   false,  false,  false,  false,  false,  false,  false,  false,  false,  true,   true,   false,  false,  false,  false,  false,  false,  true,   true,   false,  false,  false,  false,  false,  false,  false,  false },
                            { false,  false,  false,  true,   false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  true,   false,  false,  false,  false,  false,  true,   false,  false,  false,  false }
                        }
                    }
                };
            List<Node> nurse_8_nodes =
               new List<Node>()
               {
                    new Node(){
                        Index = 0, NurseId = 8, StaticHeuristicInfo = 0,
                        ShiftPatternMatrix = new bool[3,28]{
                            { false,    false,  false,  false,  true,   false,  false,  false,  true,   true,   false,  false,  false,  false,  false,  true,   true,   false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false },
                            { false,  false,  false,  false,  false,  true,   false,  true,   false,  false,  false,  false,  false,  false,  false,  false,  false,  true,   false,  false,  false,  false,  false,  false,  false,  false,  false,  true },
                            { false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  true,  false }
                        }
                    }
                };
            List<Node> nurse_9_nodes =
               new List<Node>()
               {
                    new Node(){
                        Index = 0, NurseId = 9, StaticHeuristicInfo = 0,
                        ShiftPatternMatrix = new bool[3,28]{
                            { false,    false,  false,  false,  false,  true,   false,  false,  true,   true,   false,  false,  false,  false,  false,  false,  true,   false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false },
                            { false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  true,   false,  false,  false,  false,  true,   true,   false,  false,  false,  true },
                            { false,  false,  false,  false,  true,   false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  true,   false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false }
                        }
                    }
                };
            List<Node> nurse_10_nodes =
               new List<Node>()
               {
                    new Node(){
                        Index = 0, NurseId = 10, StaticHeuristicInfo = 0,
                        ShiftPatternMatrix = new bool[3,28]{
                            { false,    false,  false,  false,  false,  false,  false,  false,  false,  false,  true,   true,   false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false },
                            { false,  false,  false,  true,   false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  true,   true,   false,  false,  false,  false,  true,   true,   false,  false },
                            { false,  true,   true,   false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false }
                        }
                    }
                };
            List<Node> nurse_11_nodes =
               new List<Node>()
               {
                    new Node(){
                        Index = 0, NurseId = 11, StaticHeuristicInfo = 0,
                        ShiftPatternMatrix = new bool[3,28]{
                            { false,    false,  false,  true,   true,   false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  true,   false,  false },
                            { false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  true,   false,  false,  false,  false,  false,  false,  false,  false,  false,  true,   false,  false,  false,  false,  false,  false,  false },
                            { false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  true,   false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  true,   false,  false,  false }
                        }
                    }
                };
            List<Node> nurse_12_nodes =
               new List<Node>()
               {
                    new Node(){
                        Index = 0, NurseId = 12, StaticHeuristicInfo = 0,
                        ShiftPatternMatrix = new bool[3,28]{
                            { false,    false,  false,  true,   false,  false,  false,  false,  false,  false,  false,  false,  true,   false,  false,  false,  false,  false,  true,   false,  false,  false,  false,  false,  true,   true,   false,  false },
                            { false,  false,  false,  false,  true,   false,  false,  false,  false,  false,  false,  false,  false,  true,   false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false },
                            { false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  true,   false,  false,  false,  false,  false,  false,  false,  false }
                                                    }
                    }
                };
            List<Node> nurse_13_nodes =
               new List<Node>()
               {
                    new Node(){
                        Index = 0, NurseId = 13, StaticHeuristicInfo = 0,
                        ShiftPatternMatrix = new bool[3,28]{
                            { false,    false,  false,  false,  false,  false,  true,   false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false },
                            { false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  true,   true,   true,   false,  false,  false,  true,   true,   false,  false,  false,  false,  true,   false },
                            { false,  false,  false,  false,  false,  false,  false,  true,   false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false }
                        }
                    }
                };
            List<Node> nurse_14_nodes =
               new List<Node>()
               {
                    new Node(){
                        Index = 0, NurseId = 14, StaticHeuristicInfo = 0,
                        ShiftPatternMatrix = new bool[3,28]{
                            { false,    false,  false,  false,  false,  true,   false,  true,   false,  false,  false,  false,  false,  false,  true,   false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false },
                            { false,  false,  false,  false,  false,  false,  true,   false,  false,  false,  false,  false,  false,  false,  false,  true,   true,   false,  false,  false,  true,   true,   false,  false,  false,  false,  false,  false },
                            { false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  true,   false,  false }
                        }
                    }
                };
            List<Node> nurse_15_nodes =
               new List<Node>()
               {
                    new Node(){
                        Index = 0, NurseId = 15, StaticHeuristicInfo = 0,
                        ShiftPatternMatrix = new bool[3,28]{
                            { false,    false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  true,   false,  false,  false,  false,  false,  false,  false,  true,   true,   false,  false,  false,  false,  false,  false },
                            { false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  true,   false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  true,   false },
                            { false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  true,   false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false }
                        }
                    }
                };
            List<Node> nurse_16_nodes =
               new List<Node>()
               {
                    new Node(){
                        Index = 0, NurseId = 16, StaticHeuristicInfo = 0,
                        ShiftPatternMatrix = new bool[3,28]{
                            { false,    false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  true,   true,   false,  true,   false,  false,  false,  false,  true,   false,  false,  false,  false,  false,  false,  false },
                            { false,  false,  false,  false,  false,  true,   true,   false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  true,   false,  false,  false,  false,  true,   false },
                            { false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false }
                        }
                    }
                };
            List<Node> nurse_17_nodes =
               new List<Node>()
               {
                    new Node(){
                        Index = 0, NurseId = 17, StaticHeuristicInfo = 0,
                        ShiftPatternMatrix = new bool[3,28]{
                            { false,    false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  true,   false,  false,  false,  false,  false,  false,  true,   false,  false,  false,  false,  true,   false },
                            { false,  false,  false,  false,  false,  false,  false,  true,   true,   true,   false,  false,  true,   true,   false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false },
                            { false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  true,   false,  false,  false,  false,  false,  false,  false }
                        }
                    }
                };
            List<Node> nurse_18_nodes =
               new List<Node>()
               {
                    new Node(){
                        Index = 0, NurseId = 18, StaticHeuristicInfo = 0,
                        ShiftPatternMatrix = new bool[3,28]{
                            { false,    false,  false,  false,  false,  true,   false,  false,  false,  false,  false,  false,  false,  false,  true,   false,  false,  false,  false,  false,  true,   true,   false,  false,  false,  false,  true,   false },
                            { false,  false,  false,  false,  false,  false,  true,   false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  true,   false,  false,  false,  false,  false },
                            { false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false }
                        }
                    }
                };
            List<Node> nurse_19_nodes =
               new List<Node>()
               {
                    new Node(){
                        Index = 0, NurseId = 19, StaticHeuristicInfo = 0,
                        ShiftPatternMatrix = new bool[3,28]{
                            { false,    false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  true,   false },
                            { false,  false,  false,  false,  false,  false,  false,  false,  true,   true,   false,  false,  false,  false,  false,  false,  false,  true,   false,  false,  false,  false,  false,  false,  false,  false,  false,  false },
                            { false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  false,  true,   false,  false,  true,   false,  false,  false,  false,  false,  false }
                        }
                    }
                };
            nodes[0] = nurse_0_nodes;
            nodes[1] = nurse_1_nodes;
            nodes[2] = nurse_2_nodes;
            nodes[3] = nurse_3_nodes;
            nodes[4] = nurse_4_nodes;
            nodes[5] = nurse_5_nodes;
            nodes[6] = nurse_6_nodes;
            nodes[7] = nurse_7_nodes;
            nodes[8] = nurse_8_nodes;
            nodes[9] = nurse_9_nodes;
            nodes[10] = nurse_10_nodes;
            nodes[11] = nurse_11_nodes;
            nodes[12] = nurse_12_nodes;
            nodes[13] = nurse_13_nodes;
            nodes[14] = nurse_14_nodes;
            nodes[15] = nurse_15_nodes;
            nodes[16] = nurse_16_nodes;
            nodes[17] = nurse_17_nodes;
            nodes[18] = nurse_18_nodes;
            nodes[19] = nurse_19_nodes;
            return nodes;
        }        
    }
}
