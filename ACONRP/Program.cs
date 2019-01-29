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
            //var inputData = InputData.GetObjectDataFromFile("Instances/toy1.xml");
            var inputData = InputData.GetObjectDataFromFile("Instances/Sprint/sprint01.xml");
            ACOHandler handler = new ACOHandler(inputData);            
            List<Node>[] nodes = handler.GenerationManager.GetShiftPatterns();

            //NodeTest(nodes);

            List<Edge> edges = new List<Edge>();

            //Stampa dei nodi generati
            //handler.GenerationManager.PrintAllNodes(nodes);

            handler.ComputeStaticHeuristic(nodes);
            List<Node> mainSolution = handler.ExtractSolution(nodes);
            int mainSolutionFitnessValue = handler.ApplySolution(mainSolution).Item1;

            //handler.InitializeLocalPheromone(nodes, mainSolutionFitnessValue, edges);
            handler.InitializeStandardPheromone(mainSolutionFitnessValue);
            handler.ListOfEdgesUpdate(mainSolution, edges);

            int consecutiveNoImprovements = 0;
            Console.Write("Solution construction ");
            do
            {
                Console.Write(".");
                List<Ant> ants = Ant.GenerateAnts(500, handler.CoverRequirements);
                foreach (var ant in ants)
                {
                    for (int i = 0; i < handler.NurseNumber; i++)
                    {
                        var heuristicInformation = handler.ComputeHeuristicInfo(nodes[i], ant.CoverRequirements); //(statica*dinamica) 

                        var selectedIndex = handler.NodeSelection(heuristicInformation, nodes, edges, i);
                        var selectedNode = nodes[i].ElementAt(selectedIndex);

                        ant.Solution.Add(selectedNode);
                        ant.CoverRequirements = heuristicInformation[selectedIndex].Item2;
                    }

                    int antSolutionFitnessValue = handler.ApplySolution(ant.Solution).Item1;
                    int[,] antSolutionUpdatedCoverReq = handler.ApplySolution(ant.Solution).Item2;
                    //Aggiornamento degli edge
                    handler.ListOfEdgesUpdate(ant.Solution, edges);

                    if (mainSolutionFitnessValue > antSolutionFitnessValue)
                    {
                        //Console.WriteLine("Updated cover requirements after solution construction:");
                        //handler.PrintCoverRequirements(antSolutionUpdatedCoverReq);
                        mainSolution = ant.Solution;
                        mainSolutionFitnessValue = antSolutionFitnessValue;
                        consecutiveNoImprovements = 0;
                        Console.Write($" {mainSolutionFitnessValue} ");
                    }
                    else
                    {
                        consecutiveNoImprovements++;
                    }
                    handler.LocalPheromoneUpdate(mainSolution, edges);
                }

                handler.GlobalPheromoneUpdate(mainSolution, mainSolutionFitnessValue, edges);

            } while (consecutiveNoImprovements < 200);

            Console.WriteLine("\nThe ACO Algorithm has produced the following solution: ");
            //mainSolution.ForEach(x => Console.WriteLine($"Nurse {x.NurseId} - Node {x.Index}"));
            handler.GenerationManager.PrintSolutionNodes(mainSolution);
            Console.WriteLine($"\nThis solution had a total fitness value of {mainSolutionFitnessValue}");
            Console.ReadKey();

        }

        private static void NodeTest(List<Node>[] nodes)
        {
            List<Node> nurse_0_nodes =
                new List<Node>()
                {
                    new Node(){Index = 0, NurseId = 0, StaticHeuristicInfo = 0,
                        ShiftPattern = new bool[4,7]{
                            { false, true, false, false, false, false, true},
                            { false, false, true, true, false, true, false},
                            { false, false, false, false, false, false, false},
                            { false, false, false, false, false, false, false}
                        }
                    },
                    new Node(){Index = 1, NurseId = 0, StaticHeuristicInfo = 0,
                        ShiftPattern = new bool[4,7]{
                            { true, false, false, false, true, true, false},
                            { false, true, false, false, false, false, false},
                            { false, false, false, false, false, false, false},
                            { false, false, false, false, false, false, false}
                        }
                    },
                    new Node(){Index = 2, NurseId = 0, StaticHeuristicInfo = 0,
                        ShiftPattern = new bool[4,7]{
                            { false, true, false, false, true, false, false},
                            { false, false, false, true, false, true, false},
                            { true, false, false, false, false, false, false},
                            { false, false, false, false, false, false, false}
                        }
                    }

                };

            List<Node> nurse_1_nodes =
               new List<Node>()
               {
                    new Node(){Index = 0, NurseId = 1, StaticHeuristicInfo = 0,
                        ShiftPattern = new bool[4,7]{
                            { true, false, false, false, true, false, false},
                            { false, true, false, false, false, false, false},
                            { false, false, false, false, false, true, true},
                            { false, false, true, false, false, false, false}
                        }
                    },
                    new Node(){Index = 1, NurseId = 1, StaticHeuristicInfo = 0,
                        ShiftPattern = new bool[4,7]{
                            { false, false, true, false, false, false, false},
                            { false, true, false, true, false, false, false},
                            { false, false, false, false, false, true, true},
                            { false, false, false, false, false, false, false}
                        }
                    },
                    new Node(){Index = 2, NurseId = 1, StaticHeuristicInfo = 0,
                        ShiftPattern = new bool[4,7]{
                            { false, false, true, false, true, false, true},
                            { false, false, false, false, false, true, false},
                            { false, true, false, false, false, false, false},
                            { true, false, false, false, false, false, false}
                        }
                    }

               };

            List<Node> nurse_2_nodes =
               new List<Node>()
               {
                    new Node(){Index = 0, NurseId = 2, StaticHeuristicInfo = 0,
                        ShiftPattern = new bool[4,7]{
                            { false, false, false, false, true, false, false},
                            { false, false, false, true, false, true, true},
                            { true, true, false, false, false, false, false},
                            { false, false, false, false, false, false, false}
                        }
                    },
                    new Node(){Index = 1, NurseId = 2, StaticHeuristicInfo = 0,
                        ShiftPattern = new bool[4,7]{
                            { false, true, false, true, false, false, false},
                            { true, false, true, false, true, false, false},
                            { false, false, false, false, false, false, false},
                            { false, false, false, false, false, false, false}
                        }
                    },
                    new Node(){Index = 2, NurseId = 2, StaticHeuristicInfo = 0,
                        ShiftPattern = new bool[4,7]{
                            { false, false, false, true, true, true, false},
                            { false, true, false, false, false, false, false},
                            { false, false, false, false, false, false, false},
                            { true, false, false, false, false, false, false}
                        }
                    }
               };

            List<Node> nurse_3_nodes =
               new List<Node>()
               {
                    new Node(){Index = 0, NurseId = 3, StaticHeuristicInfo = 0,
                        ShiftPattern = new bool[4,7]{
                            { false, true, true, true, false, true, false},
                            { false, false, false, false, false, false, false},
                            { false, false, false, false, false, false, false},
                            { false, false, false, false, false, false, false}
                        }
                    },
                    new Node(){Index = 1, NurseId = 3, StaticHeuristicInfo = 0,
                        ShiftPattern = new bool[4,7]{
                            { false, true, false, true, false, true, false},
                            { true, false, true, false, false, false, true},
                            { false, false, false, false, false, false, false},
                            { false, false, false, false, false, false, false}
                        }
                    },
                    new Node(){Index = 2, NurseId = 3, StaticHeuristicInfo = 0,
                        ShiftPattern = new bool[4,7]{
                            { false, false, false, false, false, false, false},
                            { true, true, false, false, false, false, false},
                            { false, false, false, true, true, true, false},
                            { false, false, false, false, false, false, false}
                        }
                    }

               };
            nodes[0] = nurse_0_nodes;
            nodes[1] = nurse_1_nodes;
            nodes[2] = nurse_2_nodes;
            nodes[3] = nurse_3_nodes;
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
