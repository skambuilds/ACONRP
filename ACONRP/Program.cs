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
        public const int NURSE_NUMBER = 5;

        static void Main(string[] args)
        {
            //Creazione data object a partire dal file xml
            InputData.GetObjectDataFromFile("Instances/toy1.xml");
            //Impostazione rapida dei dati di input ai fini di debug - da sostituire con i dati presenti nel data object
            int numShiftTypes = 4;
            int numOfDays = 8;
            int maxNumAssnt = 5;
            int minNumAssnt = 5;
            int maxConsWorkDays = 2;
            int neededShiftPatters = 3000;
            bool singleAssntPerDay = true;
            bool doItExactly = true;

            //Chiamata funzione di creazione della lista degli shift pattern validi
            List<bool[]> shiftPatterns = ShiftPatternsGenerator(numShiftTypes, numOfDays, maxNumAssnt, minNumAssnt, maxConsWorkDays, neededShiftPatters, singleAssntPerDay, doItExactly);
            //printShiftPatterns(shiftPatterns, numShiftTypes);

            Console.ReadKey();
        }
        /// <summary>
        /// Funzione di creazione della lista degli shift pattern validi
        /// </summary>
        /// <param name="numShiftTypes">Numero di tipi di shift</param>
        /// <param name="numOfDays">Numero di giorni in cui assegnare i turni</param>
        /// <param name="maxNumAssnt">Numero massimo di assegnamenti nell'intervallo temporale considerato</param>
        /// <param name="minNumAssnt">Numero minimo di assegnamenti nell'intervallo temporale considerato</param>
        /// <param name="maxConsWorkDays">Numero massimo di giorni consecutivi di lavoro che è possibile assegnare</param>
        /// <param name="neededShiftPatters">Numero di shift pattern desiderati per ogni valore di assegnamento</param>
        /// <param name="singleAssntPerDay">Determina se vi può essere un solo turno assegnato al giorno</param>
        /// <param name="doItExactly">Determina se l'algoritmo si attiene ai vincoli oppure produce qualche shift pattern non valido</param>
        /// <returns>Lista contenente un insieme di shift pattern, tutti diversi</returns>
        private static List<bool[]> ShiftPatternsGenerator(int numShiftTypes, int numOfDays, int maxNumAssnt, int minNumAssnt, int maxConsWorkDays, int neededShiftPatters, bool singleAssntPerDay = true, bool doItExactly = true)
        {
            //Numero totale degli shift presenti
            int totalNumOfShifts = numShiftTypes * numOfDays;
            //Lista contenente gli shift pattern validi
            List<bool[]> shiftPatternsList = new List<bool[]>();
            //Lista di indici per la gestione della generazione randomica
            List<int> indexesList = new List<int>();
            //Lista contenente gli indici attivi in quanto non estratti randomicamente
            List<int> activeIndexes = new List<int>();
            //Lista contenente gli indici rimossi in quanto estratti randomicamente
            List<int> actualRemovedElements = new List<int>();
            //Inizializzazione generatore randomico
            Random rnd = new Random();

            //Ciclo dal numero minimo al numero massimo di assegnamenti da effettuare
            for (int i = minNumAssnt; i <= maxNumAssnt; i++)
            {
                //Creo il numero richiesto di shift pattern
                for (int j = 0; j < neededShiftPatters; j++)
                {
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
                        RemoveConsecutiveDays(ref baseComparisonValue, indexesList, numShiftTypes, maxConsWorkDays, numOfDays, actualRemovedElements, activeIndexes, doItExactly);
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
                    if (!(listContainsPattern(shiftPatternsList, shiftPattern)))
                    {
                        PrintSingleShiftPattern(shiftPattern, numShiftTypes, j);
                        shiftPatternsList.Add(shiftPattern);
                    }
                    //Decremento j in quanto lo shiftpattern era già stato generato
                    else j--;
                }
            }
            return shiftPatternsList;
        }

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

        private static void AddElementsToTotalList(List<int> removedElements, List<int> actualRemovedElements)
        {
            foreach (int element in removedElements)
            {
                actualRemovedElements.Add(element);
            }
        }

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
        /// <param name="doItExactly">Determina se l'algoritmo si attiene ai vincoli oppure evita di rimuovere valori che determinano una violazione</param>
        private static void RemoveConsecutiveDays(ref int comparisonValue, List<int> indexesList, int numShiftTypes, int maxConsWorkDays, int numOfDays, List<int> removedIndexesList, List<int> activeIndexesList, bool doItExactly)
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
                                bool resultRemoveNext = false;
                                bool resultRemovePrev = false;

                                //rimuovo successivi
                                resultRemoveNext = removeNextElements(analyzedValue, indexesList, numShiftTypes);
                                //rimuovo precedenti
                                if (i != 0)
                                {
                                    int valueToRemove = activeIndexesList.ElementAt(i - 1);
                                    resultRemovePrev = removePreviousElements(valueToRemove, indexesList, numShiftTypes);
                                }
                                //Se mi trovo all'inizio della lista rimuovo gli elementi finali
                                else
                                {
                                    int valueToRemove = numLocationsToCheck - 1;
                                    resultRemovePrev = removePreviousElements(valueToRemove, indexesList, numShiftTypes);
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
                                if (CheckRemovedNextElements(firstValueInList, removedIndexesList, numValuesToCheck))
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
                            removeNextElements(firstValueInList, indexesList, numShiftTypes);
                        }
                        //3.3 L'ultima eliminazione non eguaglia il limite della violazione, devo comunque considare le differenze incotrate
                        else
                        {
                            if (CheckRemovedNextElements(lastValueFound, removedIndexesList, finalGap))
                            {
                                numberOfDifferences += finalGap / numShiftTypes;
                                //Rimuovo gli elementi che si trovano tra le due serie di valori estratti, saranno quelli che precedono il valore analizzato
                                if (numberOfDifferences == maxConsWorkDays && differencesDistance == numShiftTypes) removePreviousElements(lastValueFound, indexesList, numShiftTypes);
                                if (numberOfDifferences == maxConsWorkDays) numberOfDifferences--;
                                //Analizzo gli elementi all'inizio dell'intervallo temporale
                                CheckAndRemoveInitialElements(removedIndexesList, numShiftTypes, maxConsWorkDays, numberOfDifferences, lastValueFound, indexesList, doItExactly);
                            }
                        }
                        break; //Termino il ciclo
                    }
                }
                comparisonValue = 0;
            }
        }
        private static bool CheckAndRemoveInitialElements(List<int> removedElementsList, int numShiftTypes, int maxConsWorkDays, int numberOfDifferences, int lastValueFound, List<int> indexesList, bool doItExactly)
        {
            //individuo il primo turno del primo giorno dell'intervallo temporale
            int firstValueInList = 0;
            int timeInteval = (numShiftTypes * (maxConsWorkDays - numberOfDifferences)) + numShiftTypes;
            int numToCheck = maxConsWorkDays - numberOfDifferences;
            int diffIndex = 0;
            bool consecutive = true;

            bool removePrevFromDiff = false;
            bool removePrevFromLast = false;
            if (CheckRemovedInTimeInteval(firstValueInList, removedElementsList, timeInteval, numShiftTypes, numToCheck, ref diffIndex, ref consecutive))
            {
                //rimuovo gli elementi precedenti all'ultima differenza trovata
                removePrevFromDiff = removePreviousElements(diffIndex, indexesList, numShiftTypes);

                //se le associzioni trovate sono consecutive rimuovo gli elementi precedenti all'ultimo valore presente nella lista
                if (consecutive && doItExactly) removePrevFromLast = removePreviousElements(lastValueFound, indexesList, numShiftTypes);

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

        private static bool removePreviousElements(int valueToRemove, List<int> indexesList, int numShiftTypes)
        {
            for (int j = 0; j < numShiftTypes; j++)
            {
                if (indexesList.Contains(valueToRemove - j)) indexesList.Remove(valueToRemove - j);
                else return false;
            }
            return true;
        }

        private static bool removeNextElements(int valueToRemove, List<int> indexesList, int numShiftTypes)
        {
            for (int j = 0; j < numShiftTypes; j++)
            {
                if (indexesList.Contains(valueToRemove + j)) indexesList.Remove(valueToRemove + j);
                else return false;

            }
            return true;
        }

        private static bool listContainsPattern(List<bool[]> patternList, bool[] pattern)
        {

            foreach (bool[] element in patternList)
            {
                if (patternsAreEqual(element, pattern))
                {
                    return true;
                }
            }

            return false;
        }


        private static bool patternsAreEqual(bool[] pattern1, bool[] pattern2)
        {
            return pattern1.SequenceEqual(pattern2);
        }

        private static int days(Couple daysNightDist)
        {
            return daysNightDist.First;
        }

        private static int nights(Couple daysNightDist)
        {
            return daysNightDist.Second;
        }

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

        private static void shiftPatternInitializer(ref bool[] shiftPattern)
        {
            for (int i = 0; i < shiftPattern.Length; i++)
            {
                shiftPattern[i] = false;
            }

        }

        private static void IndexesListInitializer(List<int> arrayListIndexes, int numberPositions, int offset)
        {
            for (int i = 0; i < numberPositions; i++)
            {
                arrayListIndexes.Add(i + offset);
            }

        }

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

        private static List<int> GetIndexesToRemove(int shiftPatternIndex, int numShiftTypes)
        {

            List<int> indexesToRemove = new List<int>();
            int modResult = shiftPatternIndex % numShiftTypes;

            if (!(modResult > numShiftTypes))
            {
                for (int i = 0; i < numShiftTypes; i++)
                {
                    indexesToRemove.Add((i - modResult) + shiftPatternIndex);
                }
            }
            else
            {
                Console.WriteLine($"Attenzione il risultato dell'operazione modulo è maggiore del numero dei tipi di turni inserito. Effettuare una verifica.");
            }

            return indexesToRemove;
        }


        private static void printShiftPatterns(List<bool[]> shiftPatterns, int numShiftTypes)
        {
            int i = 0;
            foreach (bool[] shiftPatt in shiftPatterns)

            {
                int j = 1;
                foreach (bool element in shiftPatt)
                {
                    Console.Write((element) ? "1" : "0");
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
                //string joined = string.Join(",", shiftPatt);
                //Console.WriteLine($" {i}: {joined}");
                Console.Write("\n\n");
                i++;
            }

        }
        private static List<Couple> getShiftDistributionStruct()
        {
            return BuildTableShiftFromNurseNum(NURSE_NUMBER);
        }
        private static List<Couple> BuildTableShiftFromNurseNum(int numberOfNurse)
        {
            int dayNurses = Convert.ToInt32(numberOfNurse * (1.0 / 3.0));
            int nightNurses = numberOfNurse - dayNurses;

            int dayPerNurse = 7 / dayNurses;
            int restDayPerNurse = 7 % dayNurses;

            int nightPerNurse = 7 / nightNurses;
            int restNightPerNurse = 7 % nightNurses;

            List<Couple> dayTable = new List<Couple>();
            for (int i = 0; i < dayNurses; i++)
            {
                dayTable.Add(new Couple(dayPerNurse, 0));
            }

            for (int i = 0; i < restDayPerNurse; i = (i + 1) % dayTable.Count())
            {
                dayTable[i].First += 1;
            }

            List<Couple> nightTable = new List<Couple>();
            for (int i = 0; i < nightNurses; i++)
            {
                nightTable.Add(new Couple(0, nightPerNurse));
            }

            for (int i = 0; i < restNightPerNurse; i = (i + 1) % nightTable.Count())
            {
                nightTable[i].Second += 1;
            }

            return ShuffleList(dayTable.Concat(nightTable).Distinct().ToList()).Distinct().ToList();
        }

        private static List<Couple> ShuffleList(List<Couple> list)
        {
            var newEntryList = new List<Couple>();
            foreach (var entry in list)
            {
                newEntryList.Add(new Couple(entry.First + 1, entry.Second + 0));
                newEntryList.Add(new Couple(entry.First + 0, entry.Second + 1));

                for (int i = entry.First; i > 0; i--)
                {
                    newEntryList.Add(new Couple(i - 1, entry.First - (i - 1)));
                }

                for (int i = entry.Second; i > 0; i--)
                {
                    newEntryList.Add(new Couple(entry.Second - (i - 1), i - 1));

                }
            }

            return list.Concat(newEntryList).Distinct().ToList();
        }

    }

    public class Couple
    {
        public int First { get; set; }
        public int Second { get; set; }

        public Couple(int first, int second)
        {
            First = first;
            Second = second;
        }

        // override object.Equals
        public override bool Equals(object obj)
        {
            Couple f = obj as Couple;
            if (f == null)
                return false;

            return f.First.Equals(this.First) && f.Second.Equals(this.Second);
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {

            return this.First + this.Second;

        }
    }
}



/* Struttura dati che contiene la distribuzione giorni e notti:
               Lista di coppie
               
               Funzione che restituisce una struttura dati che contiene la distrubuzione dei giorni e delle notti da assegnare
               
               Dichiaro la lista shiftPatterns che dovrà contenere tutti gli shift pattern
               Si effettua un ciclo su questa struttura e posizione per posizione si generano gli array rappresentanti gli shift pattern

                Leggo il numero di giorni da assegnare
                Calcolo il coefficiente binomiale
                Divido a metà il risultato ottenuto e lo assegno alla variabile 'iterations'
                
                Dichiaro un contatore per le iterazioni denominato 'iterCount'

                Finchè iterCount < iterations:

                    Generazione dell'array 'shiftPattern1' che rappresenta il primo shift pattern generato
                    Generazione di un array 'dayIndexes' contenente i valori che rappresentano i giorni della settimana da 0 a 6
                    Per ogni giorno da assegnare:
                        Generazione di un intero da 0 a 6
                        Utilizzo l'intero generato come indice per esplorare l'array 'dayIndexes' e prelevare il valore che fa riferimento al giorno
                        Nell'array 'shiftPattern1' imposto il boolean presente all'indice uguale al valore prelevato a True
                        Rimuovo dall'array 'dayIndexes' il valore prelevato
                        Continuo con il giorno successivo
                    Terminati i giorni da assegnare:

                Terminate le notti da assegnare:

                Leggo il numero di notti da assegnare
                Calcolo il coefficiente binomiale
                Divido a metà il risultato ottenuto e lo assegno alla variabile 'iterations'

                    Generazione di un array 'nightIndexes' contenente i valori che rappresentano le notti della settimana da 7 a 13
                    Per ogni notte da assegnare:
                        Generazione di un intero da 0 a 6
                        Utilizzo l'intero generato come indice per esplorare l'array 'nightIndexes' e prelevare il valore che fa riferimento al giorno
                        Nell'array 'shiftPattern1' imposto il boolean presente all'indice uguale al valore prelevato a True
                        Rimuovo dall'array 'nightIndexes' il valore prelevato
                        Continuo con la notte successiva
                    Terminate le notti da assegnare:

                    Se non è già presente inserisco nella lista 'shiftPatterns' l'array 'shiftPattern1'
                    Proseguo con la generazione del successivo 'shiftPattern2'

                Terminate le iterazioni
                Proseguo con la generazione degli shift pattern che rispettano la successiva distrubuzione giorni notti

              Esplorate tutte le possibili distribuzioni ho ottenuto la lista con tutti gli shift pattern
              
            */
