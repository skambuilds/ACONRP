using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACONRP
{
    public class GenerationManager
    {

        private const string inputXmlFile = "Instances/toy1.xml";
        //private const string inputXmlFile = "Instances/Sprint/sprint01.xml"; TODO: Check and solve the problem with the tag "DayOffRequests"
        public SchedulingPeriod InputData { get; set; }
        /// <summary>
        /// Number of nurses
        /// </summary>
        public int numberOfNurses { get; set; }
        /// <summary>
        /// Number of shift types
        /// </summary>
        private int numShiftTypes = 4;
        /// <summary>
        /// Number of days to perform the shift assignment
        /// </summary>
        private int numOfDays = 7;
        /// <summary>
        /// Maximum number of assignment in the considered time period
        /// </summary>
        private int maxNumAssnt = 5;
        /// <summary>
        /// Minimum number of assignment in the considered time period
        /// </summary>
        private int minNumAssnt = 5;
        /// <summary>
        /// Maximum number of consecutive working days which can be assigned
        /// </summary>
        private int maxConsWorkDays = 2;
        /// <summary>
        /// Minimum number of consecutive working days which can be assigned
        /// </summary>
        private int minConsWorkDays = 2;
        /// <summary>
        /// Number of desired shift patterns for each possible assignment value
        /// </summary>
        private int shiftsPerNumAssnt = 1;
        /// <summary>
        /// Indicates if the single assignment per day option is active
        /// </summary>
        private bool singleAssntPerDay = false;
        /// <summary>
        /// Indicates if the generation procedure considers a circular period of time
        /// </summary>
        private bool circularTimePeriod = false;

        /// <summary>
        /// Constractor for the pattern generation function
        /// </summary>
        /// <returns>A lists of nodes array which contains for each nurse their set of nodes</returns>
        public GenerationManager(SchedulingPeriod inputData)
        {
            GenericInputDataInitializer(inputData);
        }


        public List<Node>[] PatternGenerationMethod()
        {
            List<Node>[] nodesPerNurse = new List<Node>[numberOfNurses];
            for (int i = 0; i < numberOfNurses; i++)
            {
                ContractDataInitializer(i);
                //Random generator initialization
                Random rnd = new Random(i);
                nodesPerNurse[i] = ShiftPatternsGenerator(rnd, i);
            }
            return nodesPerNurse;
        }
        /// <summary>
        /// Generic input data initializer procedure via xml file reading
        /// </summary>
        private void GenericInputDataInitializer(SchedulingPeriod inputData)
        {
            numberOfNurses = inputData.Employees.Employee.Count;
            numShiftTypes = inputData.ShiftTypes.Shift.Count;
            DateTime startDate = Convert.ToDateTime(inputData.StartDate);
            DateTime endDate = Convert.ToDateTime(inputData.EndDate);
            numOfDays = endDate.Day - startDate.Day + 1;
            InputData = inputData;
        }
        /// <summary>
        /// Employee's contract data initializer procedure
        /// </summary>
        /// <param name="employeeId">Employee Identifier</param>
        private void ContractDataInitializer(int employeeId)
        {
            //Retrieving the employee's contract identifier
            int contractId = Convert.ToInt16(InputData.Employees.Employee[employeeId].ContractID);
            maxNumAssnt = Convert.ToInt16(InputData.Contracts.Contract[contractId].MaxNumAssignments.Text);
            minNumAssnt = Convert.ToInt16(InputData.Contracts.Contract[contractId].MinNumAssignments.Text);
            maxConsWorkDays = Convert.ToInt16(InputData.Contracts.Contract[contractId].MaxConsecutiveWorkingDays.Text);
            minConsWorkDays = Convert.ToInt16(InputData.Contracts.Contract[contractId].MinConsecutiveWorkingDays.Text);
            singleAssntPerDay = Convert.ToBoolean(InputData.Contracts.Contract[contractId].SingleAssignmentPerDay.Text);
        }

        /// <summary>
        /// Random shift pattern generation function
        /// </summary>
        /// <param name="rnd">Random generator</param>
        /// <param name="nurseId">Nurse identifier which have to be assigned to the node</param>
        /// <returns>Nodes list which contains the feasible shift patterns</returns>
        private List<Node> ShiftPatternsGenerator(Random rnd, int nurseId)
        {
            //Total number of existing shifts
            int totalNumOfShifts = numShiftTypes * numOfDays;
            //Nodes list which contains the feasible shift patterns
            List<Node> nodesSet = new List<Node>();
            //Indexes list used for managing the random generation activity
            List<int> indexesList = new List<int>();
            //List of active indexes which have not been randomly extracted
            List<int> activeIndexes = new List<int>();
            //List of removed indexes which have been randomly extracted
            List<int> actualRemovedElements = new List<int>();
            //Counter of created nodes used as node index
            int nodeIndex = 0;

            //Loop for each desired assignment value
            for (int i = minNumAssnt; i <= maxNumAssnt; i++)
            {
                //Create the amount of needed shift patterns
                for (int j = 0; j < shiftsPerNumAssnt; j++)
                {
                    Node node = new Node();
                    //Boolean array which will contain the randomly generated shift pattern
                    bool[] shiftPattern = new bool[totalNumOfShifts];
                    //Starting value for the comparison with the values in the active indexes list
                    int baseComparisonValue = 0;
                    indexesList.Clear();
                    activeIndexes.Clear();
                    actualRemovedElements.Clear();

                    IndexesListInitializer(indexesList, totalNumOfShifts, 0);
                    IndexesListInitializer(activeIndexes, totalNumOfShifts, 0);

                    //Perform a number of iteration to reach the number of assignment specified in the first for loop
                    for (int k = 0; k < i; k++)
                    {
                        //Check the maximum consecutive working days value and remove from the indexes list the days indexes which can determine a violation
                        RemoveConsecutiveDays(ref baseComparisonValue, indexesList, actualRemovedElements, activeIndexes);

                        //Error checker circularTimePeriod = true 
                        //if (k == 4 && indexesList.Count > 0)
                        //{
                        //    Console.WriteLine("Errore! Attenzione!");
                        //    PrintSingleShiftPattern(shiftPattern, numShiftTypes, k);
                        //}

                        List<int> removedElements = RandomShiftAssigner(rnd, indexesList, shiftPattern);
                        //Add and remove the randomly extracted elements from the respective control lists
                        RemoveElementsToTotalList(removedElements, activeIndexes);
                        AddElementsToTotalList(removedElements, actualRemovedElements);
                    }
                    bool[,] shiftPatternMatrix = ArrayToMatrixConverter(shiftPattern);
                    //Check if the last generated pattern is already in the node list
                    if (!(listContainsPattern(nodesSet, shiftPatternMatrix)))
                    {
                        //PrintSingleShiftPattern(shiftPattern, j);
                        //PrintSingleShiftPattern(shiftPatternMatrix, j);
                        node.Index = nodeIndex;
                        node.NurseId = nurseId;
                        node.ShiftPattern = shiftPatternMatrix;
                        node.StaticHeuristicInfo = 0.00;
                        nodesSet.Add(node);
                        nodeIndex++;
                    }
                    //If the last generated pattern is already in the node list "j" must be decreased
                    else j--;
                }

            }
            return nodesSet;
        }
        /// <summary>
        /// Shift pattern array to shift pattern matrix converter
        /// </summary>
        /// <param name="shiftPattern">Shift pattern array which contains the generated shift</param>
        /// <returns>A shift pattern matrix with 'n' rows which are the shift types and 'm' columns which are the day's positions</returns>
        private bool[,] ArrayToMatrixConverter(bool[] shiftPattern)
        {
            bool[,] shiftPatternMatrix = new bool[numShiftTypes, numOfDays];
            //Total number of shifts in the array
            int totalNumOfShifts = numShiftTypes * numOfDays;
            int rowIndex = 0;
            int columnIndex = 0;
            for (int i = 0; i < totalNumOfShifts; i++)
            {
                if (rowIndex == numShiftTypes)
                {
                    rowIndex = 0;
                    columnIndex++;
                }
                if (shiftPattern[i])
                    shiftPatternMatrix[rowIndex, columnIndex] = true;
                rowIndex++;
            }
            return shiftPatternMatrix;
        }
        /// <summary>
        /// Shift pattern print procedure
        /// </summary>
        /// <param name="shiftPattern">Boolean array which contains the randomly generated shift pattern</param>
        /// <param name="iterazione">Number of the iteration where the shift pattern has been generated</param>
        private void PrintSingleShiftPattern(bool[] shiftPattern, int iterazione)
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
        /// Shift pattern print override procedure for shift pattern matrix
        /// </summary>
        /// <param name="shiftPattern">Boolean matrix which contains the randomly generated shift pattern</param>
        /// <param name="iterazione">Number of the iteration where the shift pattern has been generated</param>
        private void PrintSingleShiftPattern(bool[,] shiftPatternMatrix, int iterazione)
        {
            int count = 0;
            Console.Write($"Iterazione {iterazione}:\n");

            for (int i = 0; i < numShiftTypes; i++)
            {
                for (int j = 0; j < numOfDays; j++)
                {
                    if (shiftPatternMatrix[i, j]) count++;
                    Console.Write((shiftPatternMatrix[i, j]) ? " 1" : " 0");
                }
                Console.Write("\n");
            }
            Console.Write($" {count}\n");
        }
        /// <summary>
        /// Print procedure for whole set of created nodes
        /// </summary>
        /// <param name="nodesPerNurse">Array which contains the corresponding set of nodes for each nurse</param>
        public void PrintAllNodes(List<Node>[] nodesPerNurse)
        {
            for (int i = 0; i < numberOfNurses; i++)
            {
                int j = 0;
                Console.Write($"\nStampa Shift Patterns Infermiere {i}:\n\n");
                foreach (Node node in nodesPerNurse[i])
                {
                    Console.Write($"Id nodo: {node.Index}\n");
                    PrintSingleShiftPattern(node.ShiftPattern, j);
                    j++;
                }
            }
        }
        /// <summary>
        /// Add to a list procedure
        /// </summary>
        /// <param name="removedElements">Elements to add</param>
        /// <param name="actualRemovedElements">Target list where add the elements</param>
        private void AddElementsToTotalList(List<int> removedElements, List<int> actualRemovedElements)
        {
            foreach (int element in removedElements)
            {
                actualRemovedElements.Add(element);
            }
        }
        /// <summary>
        /// Remove from a list procedure
        /// </summary>
        /// <param name="removedElements">Elements to remove</param>
        /// <param name="actualRemovedElements">Target list where remove the elements</param>
        private void RemoveElementsToTotalList(List<int> removedElements, List<int> actualRemovedElements)
        {
            foreach (int element in removedElements)
            {
                actualRemovedElements.Remove(element);
            }
        }
        /// <summary>
        /// Removal procedure of the indexes which can determine a violation from the list used for managing the random generation activity
        /// </summary>
        /// <param name="comparisonValue">Starting comparison value</param>
        /// <param name="indexesList">Indexes list used for managing the random generation activity</param>
        /// <param name="removedIndexesList">List of removed indexes which have been randomly extracted</param>
        /// <param name="activeIndexesList">List of active indexes which have not been randomly extracted</param>
        private void RemoveConsecutiveDays(ref int comparisonValue, List<int> indexesList, List<int> removedIndexesList, List<int> activeIndexesList)
        {
            //The analisys take place only if the list of indexes is not empty
            if (!(indexesList.Count == 0))
            {
                //Limit value used to anticipate a possible violation
                int limitViolation = numShiftTypes * maxConsWorkDays;
                //Number of location to check
                int numLocationsToCheck = numShiftTypes * numOfDays;
                //Difference counter
                int numberOfDifferences = 0;
                //Distance counter between identified differences
                int differencesDistance = 0;

                //For each location to check
                for (int i = 0; i < numLocationsToCheck; i++)
                {
                    //Identification of the last location in the active indexes list
                    int lastLocation = activeIndexesList.Count - 1;
                    //If there are elements to check and compare in the active indexes list
                    if (lastLocation >= i)
                    {
                        //Extract the element at the "i" location for the comparative analisys
                        int analyzedValue = activeIndexesList.ElementAt(i);
                        if (analyzedValue > comparisonValue)
                        {
                            //*******1. Difference found ********
                            int differenceValue = analyzedValue - comparisonValue;

                            //1.1 The difference found is equal to the limit violation value, there are "n" consecutive days extracted
                            if (differenceValue == limitViolation)
                            {
                                removeNextElements(analyzedValue, indexesList);
                                if (i != 0)
                                {
                                    int valueToRemove = activeIndexesList.ElementAt(i - 1);
                                    removePreviousElements(valueToRemove, indexesList);
                                }
                                //If the difference found is at the beginning of the list then last elements will be removed
                                else if (circularTimePeriod)
                                {
                                    int valueToRemove = numLocationsToCheck - 1;
                                    removePreviousElements(valueToRemove, indexesList);
                                }
                                //Counters reset
                                numberOfDifferences = 0;
                                differencesDistance = 0;
                            }
                            //1.2 The difference found is not equal to the limit violation value, but it must be considered anyway
                            else
                            {
                                //Multiple differences must be count, if differnceValue/numShiftTypes = n then the accumulator needs to be increased by "n"
                                numberOfDifferences += differenceValue / numShiftTypes;
                                if (numberOfDifferences == maxConsWorkDays && differencesDistance == numShiftTypes)
                                {
                                    //Removal of the elements located between two series of extracted values, they are the previous elements of the current analyzed value
                                    int valueToRemove = activeIndexesList.ElementAt(i - 1);
                                    removePreviousElements(valueToRemove, indexesList);
                                    //The difference counter must be decrease by 1 because another difference can be found later
                                    numberOfDifferences--;
                                    differencesDistance = 0;
                                }
                                else if (differencesDistance > numShiftTypes)
                                {
                                    //The difference counter must be decrease by 1 because another difference can be found later
                                    numberOfDifferences--;
                                    differencesDistance = 0;
                                }
                            }
                            //The comparison value is update with the different found value
                            comparisonValue = analyzedValue;
                            //The comparative analisys restart from the location where the last different value has been found
                            i = activeIndexesList.IndexOf(analyzedValue);
                            //The "i" index is decrease because in the for loop there is an increase
                            i--;
                        }
                        else
                        //*******2. Difference not found ********
                        {
                            //If previously a difference was found the distance counter has to be increase
                            if (numberOfDifferences > 0) differencesDistance++;
                            comparisonValue++;
                        }
                    }
                    else
                    {
                        //*******3. Indexes list finished ********
                        //Last value expected in the list
                        int lastValueExpected = numLocationsToCheck - 1;
                        //Last value found in the list
                        int lastValueFound = activeIndexesList.ElementAt(i - 1);
                        //Gap found at the end of the list
                        int finalGap = lastValueExpected - lastValueFound;

                        //3.1 No gap found at the end of the list
                        if (finalGap == 0)
                        {
                            //If there are previous found differences
                            if (numberOfDifferences > 0 && differencesDistance == numShiftTypes)
                            {
                                //Identification of the first shift in the first day of the time interval
                                int firstValueInList = -1;
                                //Inspection of a period of time wide enough to contain a violation
                                int numValuesToCheck = numShiftTypes * (maxConsWorkDays - numberOfDifferences);
                                //Checks if there are gaps in the analyzed period of time
                                if (CheckRemovedNextElements(firstValueInList, removedIndexesList, numValuesToCheck) && circularTimePeriod)
                                {
                                    //Removal of the last elements in the list, they can lead to a violation
                                    removePreviousElements(lastValueFound, indexesList);
                                }
                            }
                        }
                        //3.2 The last gap is equal to the limit violation value
                        else if (finalGap == limitViolation)
                        {
                            //Removal of the previous elements of the last value found
                            removePreviousElements(lastValueFound, indexesList);
                            //Identification of the first shift in the first day of the time interval
                            int firstValueInList = 0;
                            //Removal of the next elements of the first value in the list
                            if (circularTimePeriod) removeNextElements(firstValueInList, indexesList);
                        }
                        //3.3 The last gap is not equal to the limit violation value, but the found differences must be considered anyway
                        else
                        {
                            numberOfDifferences += finalGap / numShiftTypes;
                            //Removal of the elements located between two series of extracted values, they are the previous elements of the current analyzed value
                            if (numberOfDifferences == maxConsWorkDays && differencesDistance == numShiftTypes) removePreviousElements(lastValueFound, indexesList);
                            if (numberOfDifferences == maxConsWorkDays) numberOfDifferences--;
                            //Analysis of the elements at the beginning of the time interval
                            if (circularTimePeriod) CheckAndRemoveInitialElements(removedIndexesList, numberOfDifferences, lastValueFound, indexesList);
                        }
                        break;
                    }
                }
                comparisonValue = 0;
            }
        }
        /// <summary>
        /// Analisys and removal of the elements which are located at the start of the time period, it is used if the generation procedure considers a circular period of time
        /// </summary>
        /// <param name="removedElementsList">List of removed indexes which have been randomly extracted</param>
        /// <param name="numberOfDifferences">Number of found differences, meant as the missing days which have been extracted</param>
        /// <param name="lastValueFound">Starting element for the removal</param>
        /// <param name="indexesList">Indexes list used for managing the random generation activity</param>
        /// <returns>Boolan value which indicates if the removal has been executed</returns>
        private bool CheckAndRemoveInitialElements(List<int> removedElementsList, int numberOfDifferences, int lastValueFound, List<int> indexesList)
        {
            //Identification of the first shift in the first day of the time interval
            int firstValueInList = 0;
            int timeInteval = (numShiftTypes * (maxConsWorkDays - numberOfDifferences)) + numShiftTypes;
            int numToCheck = maxConsWorkDays - numberOfDifferences;
            int diffIndex = 0;
            bool consecutive = true;

            bool removePrevFromDiff = false;
            bool removePrevFromLast = false;
            if (CheckRemovedInTimeInteval(firstValueInList, removedElementsList, timeInteval, numToCheck, ref diffIndex, ref consecutive))
            {
                //Removal of the previous elements of the last difference found
                removePrevFromDiff = removePreviousElements(diffIndex, indexesList);

                //If the missing indexes found are consecutive then the previous elements of the last value in the list will be removed
                if (consecutive) removePrevFromLast = removePreviousElements(lastValueFound, indexesList);
            }
            return (removePrevFromDiff || removePrevFromLast);
        }
        /// <summary>
        /// Check if there are removed elements in a well-defined time interval, this is necessary for the circular period of time implementation
        /// </summary>
        /// <param name="startingValue">Starting element for the check activity<</param>
        /// <param name="removedElements">List of removed indexes which have been randomly extracted</param>
        /// <param name="timeInterval">Time interval chosen for the check activity</param>
        /// <param name="numToSearch">Needed number of random extraction to identify</param>
        /// <param name="diffIndex">Index of the last identified difference, this is necessary for the successive removal procedure</param>
        /// <param name="consecutive">Denotes the identified random extraction typology which can be consecutive or nonconsecutive, this is necessary for the successive removal procedure</param>
        /// <returns>Boolean value which indicates the check result</returns>
        private bool CheckRemovedInTimeInteval(int startingValue, List<int> removedElements, int timeInterval, int numToSearch, ref int diffIndex, ref bool consecutive)
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
        /// Check if the removed elements list contains all the previous elements of an input value
        /// </summary>
        /// <param name="startingValue">Starting element for the check activity</param>
        /// <param name="removedElements">List of removed indexes which have been randomly extracted</param>
        /// <param name="numValuesToCheck">Number of values to check</param>
        /// <returns>Boolean value which indicates the check result</returns>
        private bool CheckRemovedPreviousElements(int startingValue, List<int> removedElements, int numValuesToCheck)
        {
            for (int j = 1; j <= numValuesToCheck; j++)
            {
                if (!(removedElements.Contains(startingValue - j))) return false;
            }
            return true;
        }
        /// <summary>
        /// Check if the removed elements list contains all the next elements of an input value
        /// </summary>
        /// <param name="startingValue">Starting element for the check activity</param>
        /// <param name="removedElements">List of removed indexes which have been randomly extracted</param>
        /// <param name="numValuesToCheck">Number of values to check</param>
        /// <returns>Boolean value which indicates the check result</returns>
        private bool CheckRemovedNextElements(int startingValue, List<int> removedElements, int numValuesToCheck)
        {
            for (int j = 1; j <= numValuesToCheck; j++)
            {
                if (!(removedElements.Contains(startingValue + j))) return false;
            }
            return true;
        }
        /// <summary>
        /// Removal of the previous elements of an input value
        /// </summary>
        /// <param name="valueToRemove">Starting element for the removal</param>
        /// <param name="indexesList">Indexes list used for managing the random generation activity</param>
        /// <returns>Boolean value which indicates the execution of the removal</returns>
        private bool removePreviousElements(int valueToRemove, List<int> indexesList)
        {
            for (int j = 0; j < numShiftTypes; j++)
            {
                if (indexesList.Contains(valueToRemove - j)) indexesList.Remove(valueToRemove - j);
                else return false;
            }
            return true;
        }
        /// <summary>
        /// Removal of the next elements of an input value
        /// </summary>
        /// <param name="valueToRemove">Starting element for the removal</param>
        /// <param name="indexesList">Indexes list used for managing the random generation activity</param>
        /// <returns>Boolean value which indicates the execution of the removal</returns>
        private bool removeNextElements(int valueToRemove, List<int> indexesList)
        {
            for (int j = 0; j < numShiftTypes; j++)
            {
                if (indexesList.Contains(valueToRemove + j)) indexesList.Remove(valueToRemove + j);
                else return false;

            }
            return true;
        }
        /// <summary>
        /// Check if a new shift pattern is already in the set of nodes generated so far
        /// </summary>
        /// <param name="nodesSet">Current set of nodes</param>
        /// <param name="pattern">New generated pattern</param>
        /// <returns>Boolean value which indicates if the new shift pattern is already in the set of nodes</returns>
        private bool listContainsPattern(List<Node> nodesSet, bool[,] pattern)
        {
            foreach (Node node in nodesSet)
            {
                if (PatternsAreEqual(node.ShiftPattern, pattern))
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// Shift pattern comparison function
        /// </summary>
        /// <param name="pattern1">Boolean array to make the comparison</param>
        /// <param name="pattern2">Boolean array to make the comparison</param>
        /// <returns>Boolean value which indicates the comparison result</returns>
        private bool PatternsAreEqual(bool[,] pattern1, bool[,] pattern2)
        {
            if (pattern1.GetLength(0) != pattern2.GetLength(0) || pattern1.GetLength(1) != pattern2.GetLength(1))
                return false;

            for (int i = 0; i < pattern1.GetLength(0); i++)
            {
                for (int j = 0; j < pattern1.GetLength(1); j++)
                {
                    if (pattern1[i, j] != pattern2[i, j])
                        return false;
                }
            }
            return true;
        }
        /// <summary>
        /// Binomila coefficent function
        /// </summary>
        /// <param name="N"></param>
        /// <param name="K"></param>
        /// <returns>Risultato del calcalo</returns>
        private decimal binomialCoefficentCalc(int N, int K)
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
        /// List of indexes initialization procedure
        /// </summary>
        /// <param name="arrayListIndexes">List of indexes that needs to be initialized</param>
        /// <param name="numberPositions">Number of positions to index</param>
        /// <param name="offset">Index offset for translation operation</param>
        private void IndexesListInitializer(List<int> arrayListIndexes, int numberPositions, int offset)
        {
            for (int i = 0; i < numberPositions; i++)
            {
                arrayListIndexes.Add(i + offset);
            }

        }
        /// <summary>
        /// Random shift assignment function
        /// </summary>
        /// <param name="rnd">Random generator</param>
        /// <param name="indexesList">Indexes list used for managing the random generation activity</param>
        /// <param name="baseShiftPattern">Boolean array which contains the randomly generated shift pattern</param>       
        /// <returns>List of removed indexes which have been randomly generated</returns>
        private List<int> RandomShiftAssigner(Random rnd, List<int> indexesList, bool[] baseShiftPattern)
        {
            List<int> removedIndexes = new List<int>();
            if (indexesList.Count > 0)
            {
                int randomValue = rnd.Next(indexesList.Count);
                int shiftPatterIndex = Convert.ToInt16(indexesList[randomValue]);
                if (singleAssntPerDay)
                {
                    removedIndexes = GetIndexesToRemove(shiftPatterIndex);
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
        /// Identification of the shifts which belong to the same day of a randomly extracted shift and need to be removed
        /// </summary>
        /// <param name="shiftPatternIndex">Randomly extracted index</param>
        /// <param name="numShiftTypes">Number of shift types</param>
        /// <returns>List of the identified indexes</returns>
        private List<int> GetIndexesToRemove(int shiftPatternIndex)
        {
            List<int> indexesToRemove = new List<int>();
            //Modulus between the extracted index value and the number of shift types
            int modResult = shiftPatternIndex % numShiftTypes;

            if (!(modResult > numShiftTypes))
            {
                for (int i = 0; i < numShiftTypes; i++)
                {
                    //Add to the list the adjacent indexes of the randomly extracted index
                    indexesToRemove.Add((i - modResult) + shiftPatternIndex);
                }
            }
            else
            {
                Console.WriteLine($"Attention please! The modulus result is greater then the specified number of shift types. Review your input data.");
            }
            return indexesToRemove;
        }
    }

}
