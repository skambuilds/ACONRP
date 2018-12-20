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

            //Chiamata procedura di generazione dell'insieme di nodi che rappresentano gli shift pattern validi
            List<Node>[] nodes = GenerationManager.PatternGenerationMethod();
            //Stampa dei nodi generati
            GenerationManager.PrintAllNodes(nodes);

            Console.ReadKey();
                      
        }

        private static List<Node> ACOAlgorithm()
        {
            //Inizio algoritmo
            //***1.
            List<Node> mainSolution = new List<Node>();
            List<Edge> edgeList = new List<Edge>();

            //***2.
            //Chiamata procedura di generazione dell'insieme di nodi che rappresentano gli shift pattern validi
            List<Node>[] nodesA = GenerationManager.PatternGenerationMethod();

            //***3 - Fabrizio B.
            ComputeStaticHeuristic(nodesA);

            //***4
            mainSolution = GetSolution(nodesA);

            //***5 - da agganciare Alessandro,  inserire metodo apposito per il calcolo della staticSolutionFitness
            //InitLocalPheromone(double staticSolutionFitness);

            do
            {
                List<Ant> ants = new List<Ant>(1000);
                foreach (Ant ant in ants)
                {
                    for (int i = 0; i < GenerationManager.numberOfNurses; i++)
                    {
                        double[] nurseHeuristic = MetodoCalcoloEuristica(nodesA[i]);
                        Node tempNode = MetodoSceltaDelNodo(nurseHeuristic, edgeList);
                        ant.Solution.Add(tempNode);
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
