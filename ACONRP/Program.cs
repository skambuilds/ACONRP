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
    class Program
    {
        static void Main(string[] args)
        {
                                   
            //Chiamata procedura di generazione dell'insieme di nodi che rappresentano gli shift pattern validi
            List<Node>[] nodes = GenerationManager.PatternGenerationMethod();
            //Stampa dei nodi generati
            GenerationManager.PrintAllNodes(nodes);

            Console.ReadKey();
        }               
    }
}
