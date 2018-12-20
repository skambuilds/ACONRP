using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACONRP
{
    class GenerationManager
    {

        private const string inputXmlFile = "Instances/toy1.xml";

        //Impostazione rapida dei dati di input ai fini di debug - ATTENZIONE! Da sostituire con i dati presenti nel data object
        public static int numberOfNurses { get; set; }
        private static int numShiftTypes = 4;
        private static int numOfDays = 7;
        private static int maxNumAssnt = 5;
        private static int minNumAssnt = 5;
        private static int maxConsWorkDays = 2;
        private static int minConsWorkDays = 2;
        private static int shiftsPerNumAssnt = 10;
        private static bool singleAssntPerDay = false;
        private static bool circularTimePeriod = false;

        

        /// <summary>
        /// Procedura per l'avvio della funzione di generazione pattern e popolamento nodi
        /// </summary>
        /// <returns>Un array di liste di nodi che contiene per ogni infermiere l'insieme dei nodi ad esso associato</returns>
        public static List<Node>[] PatternGenerationMethod()
        {
            //Inizializzatore dati di input
            InputDataInitializer();

            List<Node>[] nodesPerNurse = new List<Node>[numberOfNurses];
            for (int i = 0; i < numberOfNurses; i++)
            {
                //Inizializzazione generatore randomico
                Random rnd = new Random(i);
                nodesPerNurse[i] = ShiftPatternsGenerator(rnd, i, numShiftTypes, numOfDays, maxNumAssnt, minNumAssnt, maxConsWorkDays, shiftsPerNumAssnt, singleAssntPerDay, circularTimePeriod);
            }
            return nodesPerNurse;
        }
        /// <summary>
        /// Procedura di inizializzazione dei dati di input tramite lettura file xml
        /// </summary>
        private static void InputDataInitializer()
        {
            numberOfNurses = InputData.GetObjectDataFromFile(inputXmlFile).Employees.Employee.Count;
            numShiftTypes = InputData.GetObjectDataFromFile(inputXmlFile).ShiftTypes.Shift.Count;
            DateTime startDate = Convert.ToDateTime(InputData.GetObjectDataFromFile(inputXmlFile).StartDate);
            DateTime endDate = Convert.ToDateTime(InputData.GetObjectDataFromFile(inputXmlFile).EndDate);
            numOfDays = endDate.Day - startDate.Day + 1;
            //TODO: Impostare una array per memorizzare le caratteristiche del contratto
            maxNumAssnt = Convert.ToInt16(InputData.GetObjectDataFromFile(inputXmlFile).Contracts.Contract[0].MaxNumAssignments.Text);
            minNumAssnt = Convert.ToInt16(InputData.GetObjectDataFromFile(inputXmlFile).Contracts.Contract[0].MinNumAssignments.Text);
            maxConsWorkDays = Convert.ToInt16(InputData.GetObjectDataFromFile(inputXmlFile).Contracts.Contract[0].MaxConsecutiveWorkingDays.Text);
            minConsWorkDays = Convert.ToInt16(InputData.GetObjectDataFromFile(inputXmlFile).Contracts.Contract[0].MinConsecutiveWorkingDays.Text);
            singleAssntPerDay = Convert.ToBoolean(InputData.GetObjectDataFromFile(inputXmlFile).Contracts.Contract[0].SingleAssignmentPerDay.Text);
        }

        /// <summary>
        /// Funzione di creazione della lista degli shift pattern validi
        /// </summary>
        /// <param name="rnd">Generatore randomico</param>
        /// <param name="nurseId">Identificativo infermiere da assegnare al nodo</param>
        /// <param name="numShiftTypes">Numero di tipi di shift</param>
        /// <param name="numOfDays">Numero di giorni in cui assegnare i turni</param>
        /// <param name="maxNumAssnt">Numero massimo di assegnamenti nell'intervallo temporale considerato</param>
        /// <param name="minNumAssnt">Numero minimo di assegnamenti nell'intervallo temporale considerato</param>
        /// <param name="maxConsWorkDays">Numero massimo di giorni consecutivi di lavoro che è possibile assegnare</param>
        /// <param name="shiftsPerNumAssnt">Numero di shift pattern desiderati per ogni valore di assegnamento</param>
        /// <param name="singleAssntPerDay">Determina se vi può essere un solo turno assegnato al giorno</param>
        /// <param name="circularTimePeriod">Determina se l'algoritmo si attiene ai vincoli oppure produce qualche shift pattern non valido</param>
        /// <returns>Lista contenente un insieme di shift pattern, tutti diversi</returns>
        private static List<Node> ShiftPatternsGenerator(Random rnd, int nurseId, int numShiftTypes, int numOfDays, int maxNumAssnt, int minNumAssnt, int maxConsWorkDays, int shiftsPerNumAssnt, bool singleAssntPerDay = true, bool circularTimePeriod = true)
        {
            //Numero totale degli shift presenti
            int totalNumOfShifts = numShiftTypes * numOfDays;
            //Lista contenente gli shift pattern validi
            List<Node> nodesSet = new List<Node>();
            //Lista di indici per la gestione della generazione randomica
            List<int> indexesList = new List<int>();
            //Lista contenente gli indici attivi in quanto non estratti randomicamente
            List<int> activeIndexes = new List<int>();
            //Lista contenente gli indici rimossi in quanto estratti randomicamente
            List<int> actualRemovedElements = new List<int>();
           
            //Ciclo dal numero minimo al numero massimo di assegnamenti da effettuare
            for (int i = minNumAssnt; i <= maxNumAssnt; i++)
            {
                //Creo il numero richiesto di shift pattern
                for (int j = 0; j < shiftsPerNumAssnt; j++)
                {
                    Node node = new Node();
                    //Array di booleani che rappresenta il singolo shift pattern generato
                    bool[] shiftPattern = new bool[totalNumOfShifts];
                    //Valore di partenza per la comparazione con gli indici nella lista degli indici attivi
                    int baseComparisonValue = 0;
                    indexesList.Clear();
                    activeIndexes.Clear();
                    actualRemovedElements.Clear();

                    //Procedure di inizializzazioe della liste
                    IndexesListInitializer(indexesList, totalNumOfShifts, 0);
                    IndexesListInitializer(activeIndexes, totalNumOfShifts, 0);

                    //Itero fino a raggiungere il numero di assegnamenti specificato nel primo ciclo esterno
                    for (int k = 0; k < i; k++)
                    {
                        //Effettuo il controllo sul numero massimo di giorni consecutivi di lavoro e rimuovo della lista degli indici i giorni che possono determinare una violazione
                        RemoveConsecutiveDays(ref baseComparisonValue, indexesList, numShiftTypes, maxConsWorkDays, numOfDays, actualRemovedElements, activeIndexes, circularTimePeriod);

                        //Controllo di errore per circularTimePeriod = true 
                        //if (k == 4 && indexesList.Count > 0)
                        //{
                        //    Console.WriteLine("Errore! Attenzione!");
                        //    PrintSingleShiftPattern(shiftPattern, numShiftTypes, k);
                        //}

                        //Funzione di assegnamento randomico di un turno
                        List<int> removedElements = RandomShiftAssigner(rnd, indexesList, shiftPattern, numShiftTypes, singleAssntPerDay);
                        //Procedure di rimozione e aggiunta degli elemnti alle liste di controllo
                        RemoveElementsToTotalList(removedElements, activeIndexes);
                        AddElementsToTotalList(removedElements, actualRemovedElements);
                    }
                    //Controllo che il pattern appena generato non sia già presente nella lista
                    if (!(listContainsPattern(nodesSet, shiftPattern)))
                    {
                        //PrintSingleShiftPattern(shiftPattern, numShiftTypes, j);
                        node.Index = j;
                        node.NurseId = nurseId;
                        node.ShiftPattern = shiftPattern;
                        node.StaticHeuristicInfo = 0.00;
                        nodesSet.Add(node);
                    }
                    //Decremento j in quanto lo shiftpattern era già stato generato
                    else j--;
                }

            }
            return nodesSet;
        }
        /// <summary>
        /// Procedura per la stampa di un singolo shift pattern generato
        /// </summary>
        /// <param name="shiftPattern">Array di booleani che rappresenta uno shift pattern</param>
        /// <param name="numShiftTypes">Numero di tipi di shift</param>
        /// <param name="iterazione">Iterazione in cui è stato generato lo shift pattern</param>
        private static void PrintSingleShiftPattern(bool[] shiftPattern, int numShiftTypes, int iterazione)
        {
            int j = 1;
            int count = 0;
            Console.Write($"Iterazione {iterazione}:\n");
            foreach (bool element in shiftPattern)
            {
                Console.Write((element) ? "1" : "0");
                if (element) count++;
                if (j == numShiftTypes)
                {
                    Console.Write("|");
                    j = 0;
                }
                else
                {
                    Console.Write("-");
                }
                j++;
            }
            Console.Write($" {count}\n");
        }
        /// <summary>
        /// Procedura per la stampa di tutti gli shift pattern contenuti nei nodi
        /// </summary>
        /// <param name="nodesPerNurse">Array contenente per ogni infermiere l'insieme di nodi ad esso associato</param>
        public static void PrintAllNodes(List<Node>[] nodesPerNurse)
        {
            for (int i = 0; i < numberOfNurses; i++)
            {
                int j = 0;
                Console.Write($"\nStampa Shift Patterns Infermiere {i}:\n\n");
                foreach (Node node in nodesPerNurse[i])
                {
                    PrintSingleShiftPattern(node.ShiftPattern, numShiftTypes, j);
                    j++;
                }
            }
        }
        /// <summary>
        /// Procedura per l'aggiunta di un insieme di elementi ad una lista
        /// </summary>
        /// <param name="removedElements">Elementi da aggiungere</param>
        /// <param name="actualRemovedElements">Lista in cui dovranno essere aggiunti</param>
        private static void AddElementsToTotalList(List<int> removedElements, List<int> actualRemovedElements)
        {
            foreach (int element in removedElements)
            {
                actualRemovedElements.Add(element);
            }
        }
        /// <summary>
        /// Procedura per la rimozione di un insieme di elementi da una lista
        /// </summary>
        /// <param name="removedElements">Elementi da rimuovere</param>
        /// <param name="actualRemovedElements">Lista da cui dovranno essere rimossi</param>
        private static void RemoveElementsToTotalList(List<int> removedElements, List<int> actualRemovedElements)
        {
            foreach (int element in removedElements)
            {
                actualRemovedElements.Remove(element);
            }
        }
        /// <summary>
        /// Procedura di rimozione dalla lista degli indici utilizzati per l'estrazione randomica dei valori che possono determinare una violazione
        /// </summary>
        /// <param name="comparisonValue">Valore di comparazione di partenza</param>
        /// <param name="indexesList">Lista di indici utilizzata per la generazione randomica</param>
        /// <param name="numShiftTypes">Numero di tipi di shift</param>
        /// <param name="maxConsWorkDays">Numero massimo di giorni consecutivi di lavoro che è possibile assegnare</param>
        /// <param name="numOfDays">Numero di giorni in cui assegnare i turni</param>
        /// <param name="removedIndexesList">Lista contenente gli indici rimossi in quanto estratti randomicamente</param>
        /// <param name="activeIndexesList">Lista contenente gli indici attivi in quanto non estratti randomicamente</param>
        /// <param name="circularTimePeriod">Determina se l'algoritmo si attiene ai vincoli oppure evita di rimuovere valori che determinano una violazione</param>
        private static void RemoveConsecutiveDays(ref int comparisonValue, List<int> indexesList, int numShiftTypes, int maxConsWorkDays, int numOfDays, List<int> removedIndexesList, List<int> activeIndexesList, bool circularTimePeriod)
        {
            //L'analisi viene effettuata solo se la lista degli indici contiene elementi
            if (!(indexesList.Count == 0))
            {
                //Valore limite con il quale si anticipa una violazione, pari al numero di tipi di shift per il numero massimo di giorni consecutivi di lavoro
                int limitViolation = numShiftTypes * maxConsWorkDays;
                //Numero di locazioni da controllare, pari al numero di tipi shift per il numero di giorni in cui assegnare i turni
                int numLocationsToCheck = numShiftTypes * numOfDays;
                //Contatore differenze individuate durante controllo
                int numberOfDifferences = 0;
                //Contatore per individuare la distanza tra le differenze individuate
                int differencesDistance = 0;

                //Ciclo tutte le locazioni da verificare
                for (int i = 0; i < numLocationsToCheck; i++)
                {
                    //Identifico l'ultima locazione della lista degli indici attivi
                    int lastLocation = activeIndexesList.Count - 1;
                    //Finchè sono presenti elementi da confrontare nella lista degli indici attivi
                    if (lastLocation >= i)
                    {
                        //Estrapolo elemento da analizzare alla locazione i
                        int analyzedValue = activeIndexesList.ElementAt(i);
                        if (analyzedValue > comparisonValue)
                        {
                            //*******1. Differenza trovata ********
                            int differenceValue = analyzedValue - comparisonValue;

                            //1.1 La differenza trovata è al limite della violazione, n giorni consecutivi estratti
                            if (differenceValue == limitViolation)
                            {
                                //rimuovo successivi
                                removeNextElements(analyzedValue, indexesList, numShiftTypes);
                                //rimuovo precedenti
                                if (i != 0)
                                {
                                    int valueToRemove = activeIndexesList.ElementAt(i - 1);
                                    removePreviousElements(valueToRemove, indexesList, numShiftTypes);
                                }
                                //Se mi trovo all'inizio della lista rimuovo gli elementi finali
                                else if (circularTimePeriod)
                                {
                                    int valueToRemove = numLocationsToCheck - 1;
                                    removePreviousElements(valueToRemove, indexesList, numShiftTypes);
                                }
                                //Resetto i contatori
                                numberOfDifferences = 0;
                                differencesDistance = 0;
                            }
                            //1.2 La differenza trovata non è al limite della violazione ma deve essere comunque conteggiata
                            else
                            {
                                //Conteggio anche le differenze multiple, se differnceValue/numShiftTypes = 2 allora l'accumulatore va incrementato di 2
                                numberOfDifferences += differenceValue / numShiftTypes;
                                if (numberOfDifferences == maxConsWorkDays && differencesDistance == numShiftTypes)
                                {
                                    //Rimuovo gli elementi che si trovano tra le due serie di valori estratti, saranno quelli che precedono il valore analizzato
                                    int valueToRemove = activeIndexesList.ElementAt(i - 1);
                                    removePreviousElements(valueToRemove, indexesList, numShiftTypes);
                                    //Decremento il valore del contatore delle differenze di 1 perchè potrei trovarne una successiva da analizzare
                                    numberOfDifferences--;
                                    differencesDistance = 0;
                                }
                                else if (differencesDistance > numShiftTypes)
                                {
                                    //Decremento il valore del contatore delle differenze di 1 perchè potrei trovarne una successiva da analizzare
                                    numberOfDifferences--;
                                    differencesDistance = 0;
                                }
                            }
                            //Aggiorno il valore di confronto al valore differente individuato
                            comparisonValue = analyzedValue;
                            //Riparto a confrontare dalla locazione in cui ho trovato un valore discordante
                            i = activeIndexesList.IndexOf(analyzedValue);
                            //Decremento 'i' in quanto successivamente verrà incrementato tramite il for
                            i--;
                        }
                        else
                        //*******2. Differenza non trovata ********
                        {
                            //Se precedentemente ho individuato una differenza avvio il conteggio della distanza che la separa dalla successiva
                            if (numberOfDifferences > 0) differencesDistance++;
                            //Incremento il valore di confronto
                            comparisonValue++;
                        }
                    }
                    else
                    {
                        //*******3. Lista indici terminata ********
                        //Ultimo valore atteso nella lista
                        int lastValueAttended = numLocationsToCheck - 1;
                        //Ultimo valore presente effettivamente nella lista
                        int lastValueFound = activeIndexesList.ElementAt(i - 1);
                        //Eliminazione individuata alla fine della lista
                        int finalGap = lastValueAttended - lastValueFound;

                        //3.1 Nessuna eliminazione incontrata nella parte finale della lista
                        if (finalGap == 0)
                        {
                            //Se ho individuato in precedenza delle differenze
                            if (numberOfDifferences > 0 && differencesDistance == numShiftTypes)
                            {
                                //Individuo il primo turno del primo giorno dell'intervallo temporale
                                int firstValueInList = -1;
                                //Controllo un orizzonte temporale ampio quanto la possibile violazione
                                int numValuesToCheck = numShiftTypes * (maxConsWorkDays - numberOfDifferences);
                                //Verifico se vi sono state eliminazioni nell'orizzonte temporale sospetto
                                if (CheckRemovedNextElements(firstValueInList, removedIndexesList, numValuesToCheck) && circularTimePeriod)
                                {
                                    //Rimuovo gli ultimi elementi presenti nella lista, in quanto comporterebbero una violazione
                                    removePreviousElements(lastValueFound, indexesList, numShiftTypes);
                                }
                            }
                        }
                        //3.2 L'ultima eliminazione eguaglia il limite della violazione
                        else if (finalGap == limitViolation)
                        {
                            //Rimuovo gli elementi precedenti all'ultimo valore trovato
                            removePreviousElements(lastValueFound, indexesList, numShiftTypes);
                            //Individuo il primo turno del primo giorno dell'intervallo temporale
                            int firstValueInList = 0;
                            //Rimuovo gli elementi successivi all'ultimo valore trovato
                            if (circularTimePeriod) removeNextElements(firstValueInList, indexesList, numShiftTypes);
                        }
                        //3.3 L'ultima eliminazione non eguaglia il limite della violazione, devo comunque considare le differenze incotrate
                        else
                        {
                            numberOfDifferences += finalGap / numShiftTypes;
                            //Rimuovo gli elementi che si trovano tra le due serie di valori estratti, saranno quelli che precedono il valore analizzato
                            if (numberOfDifferences == maxConsWorkDays && differencesDistance == numShiftTypes) removePreviousElements(lastValueFound, indexesList, numShiftTypes);
                            if (numberOfDifferences == maxConsWorkDays) numberOfDifferences--;
                            //Analizzo gli elementi all'inizio dell'intervallo temporale
                            CheckAndRemoveInitialElements(removedIndexesList, numShiftTypes, maxConsWorkDays, numberOfDifferences, lastValueFound, indexesList, circularTimePeriod);
                        }
                        break; //Termino il ciclo
                    }
                }
                comparisonValue = 0;
            }
        }
        /// <summary>
        /// Funzione per l'analisi e la rimozione degli elementi che si trovano all'inizio dell'intervallo temporale, utile nel caso di un periodo temporale inteso in modo circolare
        /// </summary>
        /// <param name="removedElementsList">Lista contenente gli indici rimossi in quanto estratti randomicamente</param>
        /// <param name="numShiftTypes">Numero di tipi di shift</param>
        /// <param name="maxConsWorkDays">Numero massimo di giorni consecutivi di lavoro che è possibile assegnare</param>
        /// <param name="numberOfDifferences">Numero di differenze trovate, intese come giorni mancanti in quanto estratti</param>
        /// <param name="lastValueFound">Elemento di partenza da cui iniziare la rimozione</param>
        /// <param name="indexesList">Lista di indici utilizzata per la generazione randomica</param>
        /// <param name="circularTimePeriod">Indica se è attiva o meno un periodo temporale circolare</param>
        /// <returns>Restituisce un valore booleano che denota l'avvenuta rimozione o meno degli elementi</returns>
        private static bool CheckAndRemoveInitialElements(List<int> removedElementsList, int numShiftTypes, int maxConsWorkDays, int numberOfDifferences, int lastValueFound, List<int> indexesList, bool circularTimePeriod)
        {
            //Individuo il primo turno del primo giorno dell'intervallo temporale
            int firstValueInList = 0;
            int timeInteval = (numShiftTypes * (maxConsWorkDays - numberOfDifferences)) + numShiftTypes;
            int numToCheck = maxConsWorkDays - numberOfDifferences;
            int diffIndex = 0;
            bool consecutive = true;

            bool removePrevFromDiff = false;
            bool removePrevFromLast = false;
            if (CheckRemovedInTimeInteval(firstValueInList, removedElementsList, timeInteval, numShiftTypes, numToCheck, ref diffIndex, ref consecutive))
            {
                //Rimuovo gli elementi precedenti all'ultima differenza trovata
                if (circularTimePeriod) removePrevFromDiff = removePreviousElements(diffIndex, indexesList, numShiftTypes);

                //Se le associzioni trovate sono consecutive rimuovo gli elementi precedenti all'ultimo valore presente nella lista
                if (consecutive && circularTimePeriod) removePrevFromLast = removePreviousElements(lastValueFound, indexesList, numShiftTypes);

            }

            return (removePrevFromDiff || removePrevFromLast);
        }
        /// <summary>
        /// Funzione di controllo degli elementi rimossi per estrazione randomica all'interno di un intervallo temporale definito - implementazione lista circolare
        /// </summary>
        /// <param name="startingValue">Valore di partenza da cui effettuare il controllo</param>
        /// <param name="removedElements">Lista elementi rimossi per estrazione randomica</param>
        /// <param name="timeInterval">Intervallo temporale in cui applicare il confronto</param>
        /// <param name="numShiftTypes">Numero di tipi di shift</param>
        /// <param name="numToSearch">Numero di estarzioni randomiche da individuare</param>
        /// <param name="diffIndex">Indice dell'ultima differenza riscontrata, necessario per le successive procedure di rimozione</param>
        /// <param name="consecutive">Denota la tipologia di estrazioni randomiche individuate, consecutive o non consecutive, necessario per le successive procedure di rimozione</param>
        /// <returns>Restituisce un valore booleano che identifica se le estrazioni randomica sono state individuate</returns>
        private static bool CheckRemovedInTimeInteval(int startingValue, List<int> removedElements, int timeInterval, int numShiftTypes, int numToSearch, ref int diffIndex, ref bool consecutive)
        {

            int equalsCounter = 0;
            int diffCounter = 0;

            for (int i = 0; i < timeInterval; i++)
            {
                if (removedElements.Contains(startingValue + i))
                {
                    equalsCounter++;
                    if (diffCounter > 0) consecutive = false;
                }
                else
                {
                    diffIndex = i;
                    diffCounter++;
                }
            }

            int numOfEquals = equalsCounter / numShiftTypes;
            int numOfDiffers = diffCounter / numShiftTypes;

            if (numOfEquals == numToSearch && numOfDiffers == 1) return true;
            else return false;

        }
        /// <summary>
        /// Verifica che tutti gli elementi che precedono il valore da controllare siano presenti nella lista degli elementi rimossi
        /// </summary>
        /// <param name="startingValue">Valore di partenza da cui avviare il controllo</param>
        /// <param name="removedElements">Lista degli elementi rimossi in quanto estratti randomicamente</param>
        /// <param name="numValuesToCheck">Numero di valori da controllare</param>
        /// <returns>Restiutisce l'esito del controllo tramite booleano</returns>
        private static bool CheckRemovedPreviousElements(int startingValue, List<int> removedElements, int numValuesToCheck)
        {
            for (int j = 1; j <= numValuesToCheck; j++)
            {
                if (!(removedElements.Contains(startingValue - j))) return false;
            }
            return true;
        }
        /// <summary>
        /// Verifica che tutti gli elementi che susseguono il valore da controllare siano presenti nella lista degli elementi rimossi
        /// </summary>
        /// <param name="startingValue">Valore di partenza da cui avviare il controllo</param>
        /// <param name="removedElements">Lista degli elementi rimossi in quanto estratti randomicamente</param>
        /// <param name="numValuesToCheck">Numero di valori da controllare</param>
        /// <returns>Restiutisce l'esito del controllo tramite booleano</returns>
        private static bool CheckRemovedNextElements(int startingValue, List<int> removedElements, int numValuesToCheck)
        {
            for (int j = 1; j <= numValuesToCheck; j++)
            {
                if (!(removedElements.Contains(startingValue + j))) return false;
            }
            return true;
        }
        /// <summary>
        /// Rimuove gli elementi precedenti ad un valore di input
        /// </summary>
        /// <param name="valueToRemove">Elemento di partenza da cui iniziare la rimozione</param>
        /// <param name="indexesList">Lista di indici utilizzata per la generazione randomica</param>
        /// <param name="numShiftTypes">Numero di tipi di shift</param>
        /// <returns>Valore booleano che denota l'esecuzione o meno della rimozione</returns>
        private static bool removePreviousElements(int valueToRemove, List<int> indexesList, int numShiftTypes)
        {
            for (int j = 0; j < numShiftTypes; j++)
            {
                if (indexesList.Contains(valueToRemove - j)) indexesList.Remove(valueToRemove - j);
                else return false;
            }
            return true;
        }
        /// <summary>
        /// Rimuove gli elementi successivi ad un valore di input
        /// </summary>
        /// <param name="valueToRemove">Elemento di partenza da cui iniziare la rimozione</param>
        /// <param name="indexesList">Lista di indici utilizzata per la generazione randomica</param>
        /// <param name="numShiftTypes">Numero di tipi di shift</param>
        /// <returns>Valore booleano che denota l'esecuzione o meno della rimozione</returns>
        private static bool removeNextElements(int valueToRemove, List<int> indexesList, int numShiftTypes)
        {
            for (int j = 0; j < numShiftTypes; j++)
            {
                if (indexesList.Contains(valueToRemove + j)) indexesList.Remove(valueToRemove + j);
                else return false;

            }
            return true;
        }
        /// <summary>
        /// Funzione che verifica se l'insieme dei nodi contiene già il nuovo pattern generato
        /// </summary>
        /// <param name="nodesSet">Insieme dei nodi attuale</param>
        /// <param name="pattern">Nuovo pattern generato</param>
        /// <returns>Restituisce un valore booleano che indica se il pattern è già contenuto o meno nell'insieme</returns>
        private static bool listContainsPattern(List<Node> nodesSet, bool[] pattern)
        {

            foreach (Node node in nodesSet)
            {
                if (patternsAreEqual(node.ShiftPattern, pattern))
                {
                    return true;
                }
            }

            return false;
        }
        /// <summary>
        /// Funzione di confronto tra pattern
        /// </summary>
        /// <param name="pattern1">Array di booleani da confrontare</param>
        /// <param name="pattern2">Array di booleani da confrontare</param>
        /// <returns>Restituisce un valore booleano che denota l'esito del confronto</returns>
        private static bool patternsAreEqual(bool[] pattern1, bool[] pattern2)
        {
            return pattern1.SequenceEqual(pattern2);
        }
        /// <summary>
        /// Funzione per il calcolo del coefficente binomiale
        /// </summary>
        /// <param name="N"></param>
        /// <param name="K"></param>
        /// <returns>Risultato del calcalo</returns>
        private static decimal binomialCoefficentCalc(int N, int K)
        {
            decimal result = 1;
            for (int i = 1; i <= K; i++)
            {
                result *= N - (K - i);
                result /= i;
            }
            return result;
        }
        /// <summary>
        /// Procedura di inizializzazione di una lista di indici
        /// </summary>
        /// <param name="arrayListIndexes">Lista di indici da inizializzare</param>
        /// <param name="numberPositions">Numero di posizioni da indicizzare</param>
        /// <param name="offset">Offset per traslazione indici</param>
        private static void IndexesListInitializer(List<int> arrayListIndexes, int numberPositions, int offset)
        {
            for (int i = 0; i < numberPositions; i++)
            {
                arrayListIndexes.Add(i + offset);
            }

        }
        /// <summary>
        /// Funzione per l'assegnamento randomico di un turno
        /// </summary>
        /// <param name="rnd">Generatore randomico</param>
        /// <param name="indexesList">Lista di indici utilizzata per la generazione randomica</param>
        /// <param name="baseShiftPattern">Shift pattern in cui viene memorizzato il turno generato</param>
        /// <param name="numShiftTypes">Numero di tipi di shift</param>
        /// <param name="singleAssntPerDay">Opzione che regola se in un giorno può essere assegnato un solo turno</param>
        /// <returns>Restituisce una lista con gli indici rimossi in quanto estratti randomicamente</returns>
        private static List<int> RandomShiftAssigner(Random rnd, List<int> indexesList, bool[] baseShiftPattern, int numShiftTypes, bool singleAssntPerDay)
        {
            List<int> removedIndexes = new List<int>();
            if (indexesList.Count > 0)
            {
                int randomValue = rnd.Next(indexesList.Count);
                int shiftPatterIndex = Convert.ToInt16(indexesList[randomValue]);
                if (singleAssntPerDay)
                {
                    removedIndexes = GetIndexesToRemove(shiftPatterIndex, numShiftTypes);
                    foreach (int element in removedIndexes) indexesList.Remove(element);
                }
                else
                {
                    removedIndexes.Add(shiftPatterIndex);
                    indexesList.RemoveAt(randomValue);
                }
                baseShiftPattern[shiftPatterIndex] = true;
            }
            return removedIndexes;
        }
        /// <summary>
        /// Funzione per indentificare i turni da rimuovere in quanto appartenenti allo stesso giorno in cui è avvenuta l'estrazione randomica
        /// </summary>
        /// <param name="shiftPatternIndex">Indice estratto randomicamente</param>
        /// <param name="numShiftTypes">Numero di tipi di shift</param>
        /// <returns></returns>
        private static List<int> GetIndexesToRemove(int shiftPatternIndex, int numShiftTypes)
        {
            List<int> indexesToRemove = new List<int>();
            //Calcolo il modulo tra il valore dell'indice estratto e il numero di tipi di shift
            int modResult = shiftPatternIndex % numShiftTypes;

            if (!(modResult > numShiftTypes))
            {
                for (int i = 0; i < numShiftTypes; i++)
                {
                    //Aggiungo alla lista gli indici adiacenti all'indice estratto randomicamente
                    indexesToRemove.Add((i - modResult) + shiftPatternIndex);
                }
            }
            else
            {
                Console.WriteLine($"Attenzione il risultato dell'operazione modulo è maggiore del numero dei tipi di turni inserito. Effettuare una verifica.");
            }
            return indexesToRemove;
        }
    }

}
