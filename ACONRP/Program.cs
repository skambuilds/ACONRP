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
        static void Main(string[] args)
        {
            //var inputData = InputData.GetObjectDataFromFile("Instances/sprint_test.xml");
            var inputData = InputData.GetObjectDataFromFile("Instances/toy1.xml");
            //var inputData = InputData.GetObjectDataFromFile("Instances/Sprint/sprint01.xml");
            //var inputData = InputData.GetObjectDataFromFile("Instances/toy2.xml");
            ACOHandler handler = new ACOHandler(inputData);
            List<Node>[] nodes = handler.GenerationManager.GetShiftPatterns();


            //NodesOrderInverter(nodes);

            //List<Node>[] nodes = new List<Node>[20] {
            //    new List<Node>(),
            //    new List<Node>(),
            //    new List<Node>(),
            //    new List<Node>(),
            //    new List<Node>(),
            //    new List<Node>(),
            //    new List<Node>(),
            //    new List<Node>(),
            //    new List<Node>(),
            //    new List<Node>(),
            //    new List<Node>(),
            //    new List<Node>(),
            //    new List<Node>(),
            //    new List<Node>(),
            //    new List<Node>(),
            //    new List<Node>(),
            //    new List<Node>(),
            //    new List<Node>(),
            //    new List<Node>(),
            //    new List<Node>(),
            //};

            //NodeTest(nodes);
            //NodeTestToy1(nodes);

            List<Edge> edges = new List<Edge>();

            //Stampa dei nodi generati
            //handler.GenerationManager.PrintAllNodes(nodes);

            handler.ComputeStaticHeuristic(nodes);
            List<Node> mainSolution = handler.ExtractSolution(nodes);
            Fitness mainSolutionFitnessValue = handler.ApplySolution(mainSolution).Item1;

            //handler.InitializeLocalPheromone(nodes, mainSolutionFitnessValue, edges);
            handler.InitializeStandardPheromone(mainSolutionFitnessValue);
            handler.ListOfEdgesUpdate(mainSolution, edges);

            int consecutiveNoImprovements = 0;
            Console.Write("Solution construction ");
            do
            {
                Console.Write(".");
                List<Ant> ants = Ant.GenerateAnts(20, handler.CoverRequirements);
                foreach (var ant in ants)
                {
                    Console.Write(":");
                    for (int i = 0; i < handler.NurseNumber; i++)
                    {
                        var heuristicInformation = handler.ComputeHeuristicInfo(nodes[i], ant.CoverRequirements); //(statica*dinamica) 

                        var selectedIndex = handler.NodeSelection(heuristicInformation, nodes, edges, i);
                        var selectedNode = nodes[i].ElementAt(selectedIndex);

                        ant.Solution.Add(selectedNode);
                        ant.CoverRequirements = heuristicInformation[selectedIndex].Item2;
                    }

                    Tuple<Fitness, int[,]> antSolutionApplied = handler.ApplySolution(ant.Solution);
                         
                    int[,] antSolutionUpdatedCoverReq = antSolutionApplied.Item2;

                    //Aggiornamento degli edge
                    handler.ListOfEdgesUpdate(ant.Solution, edges);

                    if (Fitness.FitnessCompare(antSolutionApplied.Item1, mainSolutionFitnessValue) == 1)
                    {
                        //Console.WriteLine("Updated cover requirements after solution construction:");
                        //handler.PrintCoverRequirements(antSolutionUpdatedCoverReq);
                        mainSolution = ant.Solution;
                        mainSolutionFitnessValue = antSolutionApplied.Item1;
                        consecutiveNoImprovements = 0;
                        Console.Write($" {antSolutionApplied.Item1.UncoveredShifts} + {antSolutionApplied.Item1.TotalOvershift} + {antSolutionApplied.Item1.TotalSolutionCost} ");
                    }
                    else
                    {
                        consecutiveNoImprovements++;
                    }
                    handler.LocalPheromoneUpdate(mainSolution, edges);
                }

                handler.GlobalPheromoneUpdate(mainSolution, mainSolutionFitnessValue, edges);

            } while (consecutiveNoImprovements < 10);

            handler.Evaluator.CalculateSolutionPenalty(mainSolution);
            Console.WriteLine("\nThe ACO Algorithm has produced the following solution: ");          
            handler.GenerationManager.PrintSolutionNodes(mainSolution);
            Console.WriteLine($"\nThis solution had a total fitness value of {mainSolutionFitnessValue.CompleteFitnessValue}");
            Console.ReadKey();
        }
        
        private static void NodesOrderInverter(List<Node>[] nodes)
        {

            List<Node>[] localNodes = (List<Node>[])nodes.Clone();

            int i = 0;
            for (int j = localNodes.Length - 1; j >= 0; j--)
            {
                nodes[i] = localNodes[j];
                i++;
            }            
        }

        //private static void NodeTest(List<Node>[] nodes)
        //{
        //    List<Node> nurse_0_nodes =
        //        new List<Node>()
        //        {
        //            new Node(){Index = 0, NurseId = 0, StaticHeuristicInfo = 0,
        //                ShiftPattern = new bool[4,7]{
        //                    { false, true, false, false, false, false, true},
        //                    { false, false, true, true, false, true, false},
        //                    { false, false, false, false, false, false, false},
        //                    { false, false, false, false, false, false, false}
        //                }
        //            },
        //            new Node(){Index = 1, NurseId = 0, StaticHeuristicInfo = 0,
        //                ShiftPattern = new bool[4,7]{
        //                    { true, false, false, false, true, true, false},
        //                    { false, true, false, false, false, false, false},
        //                    { false, false, false, false, false, false, false},
        //                    { false, false, false, false, false, false, false}
        //                }
        //            },
        //            new Node(){Index = 2, NurseId = 0, StaticHeuristicInfo = 0,
        //                ShiftPattern = new bool[4,7]{
        //                    { false, true, false, false, true, false, false},
        //                    { false, false, false, true, false, true, false},
        //                    { true, false, false, false, false, false, false},
        //                    { false, false, false, false, false, false, false}
        //                }
        //            }

        //        };

        //    List<Node> nurse_1_nodes =
        //       new List<Node>()
        //       {
        //            new Node(){Index = 0, NurseId = 1, StaticHeuristicInfo = 0,
        //                ShiftPattern = new bool[4,7]{
        //                    { true, false, false, false, true, false, false},
        //                    { false, true, false, false, false, false, false},
        //                    { false, false, false, false, false, true, true},
        //                    { false, false, true, false, false, false, false}
        //                }
        //            },
        //            new Node(){Index = 1, NurseId = 1, StaticHeuristicInfo = 0,
        //                ShiftPattern = new bool[4,7]{
        //                    { false, false, true, false, false, false, false},
        //                    { false, true, false, true, false, false, false},
        //                    { false, false, false, false, false, true, true},
        //                    { false, false, false, false, false, false, false}
        //                }
        //            },
        //            new Node(){Index = 2, NurseId = 1, StaticHeuristicInfo = 0,
        //                ShiftPattern = new bool[4,7]{
        //                    { false, false, true, false, true, false, true},
        //                    { false, false, false, false, false, true, false},
        //                    { false, true, false, false, false, false, false},
        //                    { true, false, false, false, false, false, false}
        //                }
        //            }

        //       };

        //    List<Node> nurse_2_nodes =
        //       new List<Node>()
        //       {
        //            new Node(){Index = 0, NurseId = 2, StaticHeuristicInfo = 0,
        //                ShiftPattern = new bool[4,7]{
        //                    { false, false, false, false, true, false, false},
        //                    { false, false, false, true, false, true, true},
        //                    { true, true, false, false, false, false, false},
        //                    { false, false, false, false, false, false, false}
        //                }
        //            },
        //            new Node(){Index = 1, NurseId = 2, StaticHeuristicInfo = 0,
        //                ShiftPattern = new bool[4,7]{
        //                    { false, true, false, true, false, false, false},
        //                    { true, false, true, false, true, false, false},
        //                    { false, false, false, false, false, false, false},
        //                    { false, false, false, false, false, false, false}
        //                }
        //            },
        //            new Node(){Index = 2, NurseId = 2, StaticHeuristicInfo = 0,
        //                ShiftPattern = new bool[4,7]{
        //                    { false, false, false, true, true, true, false},
        //                    { false, true, false, false, false, false, false},
        //                    { false, false, false, false, false, false, false},
        //                    { true, false, false, false, false, false, false}
        //                }
        //            }
        //       };

        //    List<Node> nurse_3_nodes =
        //       new List<Node>()
        //       {
        //            new Node(){Index = 0, NurseId = 3, StaticHeuristicInfo = 0,
        //                ShiftPattern = new bool[4,7]{
        //                    { false, true, true, true, false, true, false},
        //                    { false, false, false, false, false, false, false},
        //                    { false, false, false, false, false, false, false},
        //                    { false, false, false, false, false, false, false}
        //                }
        //            },
        //            new Node(){Index = 1, NurseId = 3, StaticHeuristicInfo = 0,
        //                ShiftPattern = new bool[4,7]{
        //                    { false, true, false, true, false, true, false},
        //                    { true, false, true, false, false, false, true},
        //                    { false, false, false, false, false, false, false},
        //                    { false, false, false, false, false, false, false}
        //                }
        //            },
        //            new Node(){Index = 2, NurseId = 3, StaticHeuristicInfo = 0,
        //                ShiftPattern = new bool[4,7]{
        //                    { false, false, false, false, false, false, false},
        //                    { true, true, false, false, false, false, false},
        //                    { false, false, false, true, true, true, false},
        //                    { false, false, false, false, false, false, false}
        //                }
        //            }

        //       };
        //    nodes[0] = nurse_0_nodes;
        //    nodes[1] = nurse_1_nodes;
        //    nodes[2] = nurse_2_nodes;
        //    nodes[3] = nurse_3_nodes;
        //}

        private static void NodeTest(List<Node>[] nodes)
        {
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
        }

        private static void NodeTestToy1(List<Node>[] nodes)
        {
            List<Node> nurse_0_nodes =
          new List<Node>()
                 {
                     new Node(){Index = 0, NurseId = 0, StaticHeuristicInfo = 0,
                         ShiftPatternMatrix = new bool[3,7]{
                             { false, false, false, true, true, false, false},
                            { true, false, false, false, false, true, true},
                            { false, true, false, false, false, false, false}
                         }
                     },
          };

            List<Node> nurse_1_nodes =
        new List<Node>()
               {
                     new Node(){Index = 0, NurseId = 1, StaticHeuristicInfo = 0,
                         ShiftPatternMatrix = new bool[3,7]{
                             { false, false, false, false, true, false, false},
                            { true, true, true, false, false, true, true},
                            { false, false, true, false, false, false, false}
                         }
                     },
        };
            List<Node> nurse_2_nodes =
        new List<Node>()
               {
                     new Node(){Index = 0, NurseId = 2, StaticHeuristicInfo = 0,
                         ShiftPatternMatrix = new bool[3,7]{
                             { false, false, false, false, false, true, true},
                            { false, true, true, false, false, false, false},
                            { false, false, false, true, false, false, false}
                         }
                     },
        };
            List<Node> nurse_3_nodes =
        new List<Node>()
               {
                     new Node(){Index = 0, NurseId = 3, StaticHeuristicInfo = 0,
                         ShiftPatternMatrix = new bool[3,7]{
                             { true, true, false, false, false, true, true},
                            { false, false, false, true, true, false, false},
                            { false, false, false, false, true, false, false}
                         }
                     },
        };
            List<Node> nurse_4_nodes =
        new List<Node>()
               {
                     new Node(){Index = 0, NurseId = 4, StaticHeuristicInfo = 0,
                         ShiftPatternMatrix = new bool[3,7]{
                             { true, false, true, false, false, false, false},
                            { false, false, false, true, true, false, false},
                            { true, false, false, false, false, false, false}
                         }
                     },
        };
            List<Node> nurse_5_nodes =
        new List<Node>()
               {
                     new Node(){Index = 0, NurseId = 5, StaticHeuristicInfo = 0,
                         ShiftPatternMatrix = new bool[3,7]{
                             { false, true, true, true, false, false, false},
                            { false, false, false, false, false, false, false},
                            { false, false, false, false, false, true, true}
                         }
                     },
        };

            nodes[0] = nurse_0_nodes;
            nodes[1] = nurse_1_nodes;
            nodes[2] = nurse_2_nodes;
            nodes[3] = nurse_3_nodes;
            nodes[4] = nurse_4_nodes;
            nodes[5] = nurse_5_nodes;

        }

        private static List<Node> ACOAlgorithm()
        {
            //Inizio algoritmo
            //***1.
            List<Node> mainSolution = new List<Node>();
            List<Edge> edgeList = new List<Edge>();

            //***2.
            //Chiamata procedura di generazione dell'insieme di nodi che rappresentano gli shift pattern validi
            //List<Node>[] nodesA = GenerationManager.PatternGenerationMethod();

            //***3 - Fabrizio B.
            // ComputeStaticHeuristic(nodesA);

            //***4
            //  mainSolution = GetSolution(nodesA);

            //***5 - da agganciare Alessandro,  inserire metodo apposito per il calcolo della staticSolutionFitness
            //InitLocalPheromone(double staticSolutionFitness);

            do
            {
                List<Ant> ants = new List<Ant>(1000);
                foreach (Ant ant in ants)
                {
                    // for (int i = 0; i < GenerationManager.numberOfNurses; i++)
                    {
                        // double[] nurseHeuristic = MetodoCalcoloEuristica(nodesA[i]);
                        // Node tempNode = MetodoSceltaDelNodo(nurseHeuristic, edgeList);
                        // ant.Solution.Add(tempNode);
                    }
                    if (MetodoComparazioneSoluzioneMigliore(mainSolution, ant.Solution))
                    {
                        mainSolution = ant.Solution;
                        //***C - da agganciare Alessandro
                        //UpdateLocalPheromone(mainSolution, epsy);
                    }
                }
                //***II - da agganciare Alessandro, inserire metodo apposito per il calcolo della bsfSolutionFitness
                //UpdateGlobalPheromone(mainSolution, bsfSolutionFitness, rho);
            }
            while (VerificaVincoli());

            return mainSolution;
            //Fine algoritmo
        }

    }
}
