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
            InputData.GetObjectDataFromFile("Instances/toy1.xml");

            //DataSet dataSet = new DataSet();
            //dataSet.ReadXmlSchema("Instances/competition.xsd");
            //var doc = new XmlDataDocument(dataSet);
            //doc.Load("Instances/toy1.xml");


            List<Couple> distributionList = getShiftDistributionStruct();
            List<bool[]> shiftPatterns = shiftPatternsGenerator(distributionList);

            printShiftPatterns(shiftPatterns);
            Console.ReadKey();
        }

        private static List<Couple> getShiftDistributionStruct()
        {
            return BuildTableShiftFromNurseNum(NURSE_NUMBER);
        }

        private static List<bool[]> shiftPatternsGenerator(List<Couple> distributionList)
        {
            List<bool[]> shiftPatternsList = new List<bool[]>();
            int numberOfWorkDays = 7;
            int firstInterations = 0;
            int secondInterations = 0;

            int firstIterCount = 0;
            int secondIterCount = 0;
            Random rnd = new Random();

            //int[] daysIndexes = new int[7];
            ArrayList daysIndexes = new ArrayList();
            ArrayList nighsIndexes = new ArrayList();

            bool[] baseShiftPattern = new bool[14];
            bool[] firstShiftPattern = new bool[14];
            bool[] completeShiftPattern = new bool[14];

            foreach (Couple dist in distributionList)
            {

                int numOfDays = days(dist);
                int numOfNights = nights(dist);
                decimal binomialCoefficentDays = binomialCoefficentCalc(numberOfWorkDays, numOfDays);
                decimal binomialCoefficentNights = binomialCoefficentCalc(numberOfWorkDays, numOfNights);

                Console.WriteLine($"binomialCoefficent days = {binomialCoefficentDays}");
                Console.WriteLine($"binomialCoefficent nights = {binomialCoefficentNights}");

                bool daysFirst = false;
                if (binomialCoefficentDays > binomialCoefficentNights)
                {
                    firstInterations = Convert.ToInt16(binomialCoefficentDays) / 2;
                    secondInterations = Convert.ToInt16(binomialCoefficentNights) / 2;
                    daysFirst = true;
                }
                else
                {
                    firstInterations = Convert.ToInt16(binomialCoefficentNights) / 2;
                    secondInterations = Convert.ToInt16(binomialCoefficentDays) / 2;
                }

                firstIterCount = 0;
                secondIterCount = 0;

                while (firstIterCount < firstInterations)
                {

                    shiftPatternInitializer(ref baseShiftPattern);

                    daysIndexes.Clear();
                    nighsIndexes.Clear();
                    arrayListIndexesInitializer(ref daysIndexes, numberOfWorkDays, 0);
                    arrayListIndexesInitializer(ref nighsIndexes, numberOfWorkDays, 7);

                    if (daysFirst)
                    {
                        firstShiftPattern = randomDaysNightsAssigner(rnd, numOfDays, daysIndexes, baseShiftPattern);
                    }
                    else
                    {
                        firstShiftPattern = randomDaysNightsAssigner(rnd, numOfNights, nighsIndexes, baseShiftPattern);
                    }

                    int insuccess = 0;
                    while (secondIterCount < secondInterations && insuccess < 5)
                    {


                        daysIndexes.Clear();
                        nighsIndexes.Clear();
                        arrayListIndexesInitializer(ref daysIndexes, numberOfWorkDays, 0);
                        arrayListIndexesInitializer(ref nighsIndexes, numberOfWorkDays, 7);


                        if (daysFirst)
                        {
                            completeShiftPattern = randomDaysNightsAssigner(rnd, numOfNights, nighsIndexes, firstShiftPattern);
                        }
                        else
                        {
                            completeShiftPattern = randomDaysNightsAssigner(rnd, numOfDays, daysIndexes, firstShiftPattern);
                        }

                        if (!(listContainsPattern(shiftPatternsList, completeShiftPattern)))
                        {
                            shiftPatternsList.Add(completeShiftPattern);
                            secondIterCount += 1;
                            insuccess = 0; //reset insuccess
                        }
                        else
                        {
                            insuccess += 1;
                        }

                    }

                    if (secondInterations == 0)
                    {

                        if (!(listContainsPattern(shiftPatternsList, firstShiftPattern)))
                        {
                            shiftPatternsList.Add(firstShiftPattern);
                            firstIterCount += 1;

                        }

                    }
                    else
                    {
                        firstIterCount += 1;
                    }

                    secondIterCount = 0;


                }


            }

            return shiftPatternsList;
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

        private static void arrayListIndexesInitializer(ref ArrayList arrayListIndexes, int numberOfWorkDays, int offset)
        {
            for (int i = 0; i < numberOfWorkDays; i++)
            {
                arrayListIndexes.Add(i + offset);
            }

        }

        private static bool[] randomDaysNightsAssigner(Random rnd, int numOfShifts, ArrayList indexesList, bool[] baseShiftPattern)
        {

            bool[] resultShiftPattern = new bool[14];
            baseShiftPattern.CopyTo(resultShiftPattern, 0);

            for (int i = 0; i < numOfShifts; i++)
            {   
                int randomValue = rnd.Next(indexesList.Count);
                int shiftPatterIndex = Convert.ToInt16(indexesList[randomValue]);
                resultShiftPattern[shiftPatterIndex] = true;

                indexesList.RemoveAt(randomValue);

            }

            return resultShiftPattern;
        }

        private static void printShiftPatterns(List<bool[]> shiftPatterns)
        {
            int i = 0;
            foreach (bool[] shiftPatt in shiftPatterns)

            {
                string joined = string.Join(",", shiftPatt);
                Console.WriteLine($" {i}: {joined}");
                i++;
            }

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
