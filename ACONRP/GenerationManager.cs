﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using ACONRP.Evaluator;

namespace ACONRP
{
    public class GenerationManager
    {
        #region Members Definitions

        public SchedulingPeriod InputData { get; set; }
        public Evalutador Evaluator { get; set; }
        /// <summary>
        /// Number of nurses
        /// </summary>
        private int numberOfNurses { get; set; }
        /// <summary>
        /// Number of shift types
        /// </summary>
        private int numShiftTypes = 4;
        DateTime startDate = DateTime.MinValue;
        DateTime endDate = DateTime.MinValue;
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
        private int minNumAssnt = 3;
        /// <summary>
        /// Maximum number of consecutive working days which can be assigned
        /// </summary>
        private int maxConsWorkDays = 2;
        /// <summary>
        /// Minimum number of consecutive working days which can be assigned
        /// </summary>
        private int minConsWorkDays = 2;
        /// <summary>
        /// Maximum number of shift patterns for each possible assignment value
        /// </summary>
        private const decimal maximumLimit = 5000;
        /// <summary>
        /// Indicates if the single assignment per day option is active
        /// </summary>
        private bool singleAssntPerDay = false;
        /// <summary>
        /// Indicates if the generation procedure considers a circular period of time
        /// </summary>
        private bool circularTimePeriod = false;
        /// <summary>
        /// Eneable regeneration of shif patterns
        /// </summary>
        private const bool regenerate = false;
        /// <summary>
        /// Enable maximum and minimum number of assigment violation, the value is the amount of violation - put 0 to disable
        /// </summary>
        private const int assntViolation = 6;
        /// <summary>
        /// Enable the maximum number of consecutive working days violation, 
        /// if set to 2 an half of the generated shift patterns will contain the violation - put 1 to disable
        /// </summary>
        private const int violationEvent = 4;
        /// <summary>
        /// Amount of the maximum number of consecutive working days violation
        /// </summary>
        private const int maxConsViolation = 3;

        //Shift patterns files location
        private string shiftDirectoryName = "ShiftPatterns/";
        private string istanceSubDirectory = String.Empty;
        private string shiftsFileName = "ShiftPatternsNurse";
        private string txtFileExt = ".txt";

        private string solutionsDirectoryName = "Solutions/";
        private string solutionFileName = "Solution";


        #endregion

        /// <summary>
        /// Constractor for the pattern generation function
        /// </summary>
        /// <returns>A lists of nodes array which contains for each nurse their set of nodes</returns>
        public GenerationManager(SchedulingPeriod schedulingData)
        {
            GenericInputDataInitializer(schedulingData);
        }
        public List<Node>[] GetShiftPatterns()
        {
            if (regenerate == false && CheckShiftPatternFilesExistance())
                return PatternLoadingMethod();
            else
            {
                if (regenerate) Console.WriteLine($"Regeneration activeted");

                //Chiamata procedura di generazione dell'insieme di nodi che rappresentano gli shift pattern validi
                return PatternGenerationMethod();
            }
        }
        /// <summary>
        /// Check the existance of the all shift pattern files
        /// </summary>
        /// <returns>Boolean value</returns>
        public bool CheckShiftPatternFilesExistance()
        {
            for (int i = 0; i < numberOfNurses; i++)
            {
                if (System.IO.File.Exists($@"{GetShiftsFileName(i)}"))
                {
                    FileInfo f = new FileInfo($@"{GetShiftsFileName(i)}");
                    if (f.Length == 0)
                    {
                        Console.WriteLine($"The {GetShiftsFileName(i)} file is empty, the shift pattern generation will be executed");
                        return false;
                    }
                }
                else {
                    Console.WriteLine($"The {GetShiftsFileName(i)} does not exist, the shift pattern generation will be executed");
                    return false;
                }
            }
            return true;
        }
        private string GetShiftsFileName(int id)
        {
            string fileName = $"{shiftDirectoryName}{istanceSubDirectory}{shiftsFileName}{id}{txtFileExt}";
            return fileName;
        }
        private string GetShiftsPath()
        {
            string path = $"{shiftDirectoryName}{istanceSubDirectory}";
            return path;
        }
        private string GetSolutionsPath()
        {
            string path = $"{solutionsDirectoryName}{istanceSubDirectory}";
            return path;
        }
        private List<Node>[] PatternGenerationMethod()
        {
            List<Node>[] nodesPerNurse = new List<Node>[numberOfNurses];
            for (int i = 0; i < numberOfNurses; i++)
            {
                ContractDataInitializer(i);
                //Random generator initialization
                Random rnd = new Random(i);
                Console.Write($"Generation for nurse {i}  ");
                var watch = System.Diagnostics.Stopwatch.StartNew();
                nodesPerNurse[i] = ShiftPatternsGenerator(rnd, i);
                watch.Stop();
                var elapsedMs = watch.ElapsedMilliseconds;
                Console.Write($"completed in {elapsedMs}ms with {nodesPerNurse[i].Count} shift patterns\n");
            }
            return nodesPerNurse;
        }
        /// <summary>
        /// Read the shift patterns from .txt files
        /// </summary>
        /// <returns>A lists of nodes array which contains for each nurse their set of nodes</returns>
        private List<Node>[] PatternLoadingMethod()
        {
            List<Node>[] nodesPerNurse = new List<Node>[numberOfNurses];
            for (int i = 0; i < numberOfNurses; i++)
            {
                List<Node> nodesSet = new List<Node>();
                int lineCounter = 0;
                string line = string.Empty;
                try
                {
                    System.IO.StreamReader file = new System.IO.StreamReader($@"{GetShiftsFileName(i)}");
                    while ((line = file.ReadLine()) != null)
                    {
                        Node node = NodeInitializer(i, lineCounter, line);
                        nodesSet.Add(node);
                        lineCounter++;
                    }
                    file.Close();
                    nodesPerNurse[i] = nodesSet;
                    Console.Write($"Nurse {i} loading completed with {nodesPerNurse[i].Count} shift patterns\n");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error has occurred during the {GetShiftsFileName(i)} file reading:");
                    Console.WriteLine(ex.Message);
                }
            }
            Console.WriteLine($"The shift patterns have been loaded correctly from .txt files");
            return nodesPerNurse;
        }

        private Node NodeInitializer(int i, int lineCounter, string line)
        {
            Node node = new Node();
            string[] lineString = line.Split(';');
            int endingPosition = lineString.Length - 1;
            List<int> shiftPatternSparse = ShiftPatternSparseLoader(lineString, endingPosition);
            bool[] shiftPatternBool = ShiftPatternConverter(shiftPatternSparse);
            node.Index = lineCounter;
            node.NurseId = i;
            node.ShiftPatternArray = shiftPatternBool;
            node.ShiftPatternSparse = shiftPatternSparse;
            node.Cost = NodeCostLoader(lineString, endingPosition);
            node.StaticHeuristicInfo = 0.00;
            return node;
        }

        private int NodeCostLoader(string[] lineString, int endingPosition)
        {
            int cost = 0;
            if (!(Int32.TryParse(lineString[endingPosition], out cost))) throw new InvalidCastException();
            return cost;
        }

        /// <summary>
        /// Converts a shift pattern in sparse form into a shift pattern array in boolean form
        /// </summary>
        /// <param name="shiftPattern">Shift pattern sparse</param>
        /// <returns>Shift pattern boolean array</returns>
        private bool[] ShiftPatternConverter(List<int> shiftPattern)
        {
            int totalNumOfShifts = numShiftTypes * numOfDays;
            bool[] shiftPatternBool = new bool[totalNumOfShifts];
            for (int j = 0; j < totalNumOfShifts; j++)
            {
                shiftPatternBool[j] = ((shiftPattern.Contains(j)) ? true : false);
            }
            return shiftPatternBool;
        }
        private List<int> ShiftPatternSparseLoader(string[] shiftPattern, int endingPosition)
        {
            List<int> shiftPatternSparse = new List<int>();
            for (int j = 0; j < endingPosition; j++)
            {
                int value = 0;
                if (!(Int32.TryParse(shiftPattern.ElementAt(j), out value))) throw new InvalidCastException();
                shiftPatternSparse.Add(value);
            }

            return shiftPatternSparse;
        }

        /// <summary>
        /// Generic input data initializer procedure via xml file reading
        /// </summary>
        private void GenericInputDataInitializer(SchedulingPeriod schedulingData)
        {
            InputData = schedulingData;
            istanceSubDirectory = $"{InputData.ID}/";
            numberOfNurses = InputData.Employees.Employee.Count;
            numShiftTypes = InputData.ShiftTypes.Shift.Count;
            startDate = Convert.ToDateTime(InputData.StartDate);
            endDate = Convert.ToDateTime(InputData.EndDate);
            numOfDays = endDate.Day - startDate.Day + 1;
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
            //List of the neighborhoods of the removed indexes which have been randomly extracted
            List<int> actualRemovedElements = new List<int>();
            //Counter of created nodes used as node index
            int nodeIndex = 0;
            //Create an evaluator object for the node penalty evaluation
            Evaluator = new Evalutador(InputData, nurseId);

            CheckAndCreateDirectory(GetShiftsPath());
            using (System.IO.StreamWriter file = new System.IO.StreamWriter($@"{GetShiftsFileName(nurseId)}", false))
            {
                //Flags for character animation
                bool dashFlag = true;
                bool backFlag = true;

                //file.WriteLine($"NurseId: {nurseId}");
                int minLimit;
                int maxLimit;
                if (minNumAssnt - assntViolation < 1) minLimit = 1;
                else minLimit = minNumAssnt - assntViolation;
                maxLimit = maxNumAssnt + assntViolation;

                //Loop for each desired assignment value
                for (int i = minLimit; i <= maxLimit; i++)
                {
                    decimal iterationLimit = maximumLimit;
                    int alreadyExistCounter = 0;
                    //Enable violation of the max consecutive working days constraint
                    bool enableViolation = false;

                    //Create the amount of needed shift patterns
                    int j = 0;
                    do
                    {
                        Node node = new Node();
                        //Boolean array which will contain the randomly generated shift pattern
                        bool[] shiftPattern = new bool[totalNumOfShifts];
                        //List of the single indexes which have been randomly extracted
                        List<int> shiftPatternSparse = new List<int>();
                        //Starting value for the comparison with the values in the active indexes list
                        int baseComparisonValue = 0;
                        indexesList.Clear();
                        activeIndexes.Clear();
                        actualRemovedElements.Clear();

                        IndexesListInitializer(indexesList, totalNumOfShifts, 0);
                        IndexesListInitializer(activeIndexes, totalNumOfShifts, 0);

                        RequestedDayOffRemover(indexesList, nurseId);
                        decimal binCoefValue = BinomialCoefficentCalc(indexesList.Count, i);
                        if (binCoefValue < maximumLimit) iterationLimit = binCoefValue;

                        //Perform a number of iteration to reach the number of assignment specified in the first for loop
                        for (int k = 0; k < i; k++)
                        {
                            //Check the maximum consecutive working days value and remove from the indexes list the days indexes which can determine a violation
                            RemoveConsecutiveDays(ref baseComparisonValue, indexesList, actualRemovedElements, activeIndexes, enableViolation);
                            
                            List<int> removedElements = RandomShiftAssigner(rnd, indexesList, shiftPattern, shiftPatternSparse);
                            //Add and remove the randomly extracted elements from the respective control lists
                            RemoveElementsToTotalList(removedElements, activeIndexes);
                            AddElementsToTotalList(removedElements, actualRemovedElements);
                        }

                        //Check if the last generated pattern is already in the node list
                        if (!(listContainsPattern(nodesSet, shiftPatternSparse)))
                        {
                            node.Index = nodeIndex;
                            node.NurseId = nurseId;
                            node.ShiftPatternArray = shiftPattern;
                            node.ShiftPatternSparse = shiftPatternSparse;
                            node.StaticHeuristicInfo = 0.00;
                            //Node penalty calculation
                            Evaluator.CalculateNodePenalty(node);

                            //File writing for shift pattern and relative cost saving
                            foreach (int position in node.ShiftPatternSparse)
                            {
                                file.Write($"{position};");
                            }
                            file.Write($"{node.Cost}\n");

                            nodesSet.Add(node);
                            nodeIndex++;

                            BarAnimation(ref dashFlag, ref backFlag);

                            decimal maximumUnsuccessfulIter = iterationLimit / 2;
                            if (alreadyExistCounter >= (maximumUnsuccessfulIter))
                            {
                                iterationLimit += alreadyExistCounter;                                
                            }
                            alreadyExistCounter = 0;
                            j++;
                        }
                        else
                        {
                            alreadyExistCounter++;
                        }
                        int violationNum = (int)(iterationLimit - (iterationLimit / violationEvent));
                        if (j > 0 && j == violationNum)
                        {
                            enableViolation = true;
                        }
                    }
                    while (alreadyExistCounter < iterationLimit && j < iterationLimit);
                    Console.Write("\b- ");
                }
            }
            return nodesSet;
        }
        /// <summary>
        /// Removes the indexes of the requested days off 
        /// </summary>
        /// <param name="indexesList"></param>
        /// <param name="nurseId"></param>
        private void RequestedDayOffRemover(List<int> indexesList, int nurseId)
        {
            List<DayOff> dayOffRequests = InputData.DayOffRequests.DayOff.Where(day => day.EmployeeID == nurseId.ToString()).ToList();
            foreach (DayOff rdo in dayOffRequests)
            {
                int day = Utils.DateOffset(startDate, Convert.ToDateTime(rdo.Date)) - 1;
                int index = day * numShiftTypes;
                List<int> removedIndexes = new List<int>();
                removedIndexes = GetIndexesToRemove(index);
                foreach (int element in removedIndexes) indexesList.Remove(element);
            }
        }

        private static void BarAnimation(ref bool dashFlag, ref bool backFlag)
        {
            Console.Write("\b");
            if (dashFlag)
            {
                Console.Write("-");
                dashFlag = false;
            }
            else {
                if (backFlag)
                {
                    Console.Write("\\");
                    backFlag = false;
                }
                else
                {
                    Console.Write("/");
                    backFlag = true;
                }
                dashFlag = true;
            }
        }

        private void CheckAndCreateDirectory(string pathToCheck)
        {
            if (!(Directory.Exists(pathToCheck)))
            {
                Directory.CreateDirectory(pathToCheck);
            }
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
        private bool[,] ArrayToMatrixConverter(char[] shiftPattern)
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
                if (shiftPattern[i].Equals('1'))
                    shiftPatternMatrix[rowIndex, columnIndex] = true;
                rowIndex++;
            }
            return shiftPatternMatrix;
        }
        /// <summary>
        /// Print shift patterns of the solution
        /// </summary>
        /// <param name="solutionNodes">List of solution nodes</param>
        public void PrintSolution(List<Node> solutionNodes, Fitness solutionFitness)
        {
            CheckAndCreateDirectory(GetSolutionsPath());
            using (System.IO.StreamWriter fileStream = new System.IO.StreamWriter($@"{GetSolutionFileName()}", false))
            {
                WriteOnFileAndConsole("\nThe ACO Algorithm has produced the following solution: ", fileStream);
                foreach (Node node in solutionNodes)
                {
                    WriteOnFileAndConsole($"\nNurse {node.NurseId} - ", fileStream);
                    PrintSingleShiftPattern(node.ShiftPatternSparse, node.Index, fileStream);
                    WriteOnFileAndConsole($"Violations:\n", fileStream);
                    foreach (string viol in node.Violations) WriteOnFileAndConsole($"{viol}\n", fileStream);
                    WriteOnFileAndConsole($"Total cost: {node.Cost}\n", fileStream);
                }
                WriteOnFileAndConsole($"\nThis solution had a total fitness value of: {solutionFitness.UncoveredShifts} + {solutionFitness.TotalOverShift} + {solutionFitness.TotalSolutionCost} = {solutionFitness.CompleteFitnessValue}", fileStream);
            }
        }
        private void WriteOnFileAndConsole(string stringLine, StreamWriter file)
        {
            Console.Write(stringLine);
            file.Write(stringLine);
        }

        private string GetSolutionFileName()
        {
            string dateNow = DateTime.Now.ToLongTimeString().Replace(':', '-');
            string fileName = $"{solutionsDirectoryName}{istanceSubDirectory}{solutionFileName}{dateNow}{txtFileExt}";
            return fileName;
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
        /// Shift pattern print procedure overload for shift pattern matrix
        /// </summary>
        /// <param name="shiftPattern">Boolean matrix which contains the randomly generated shift pattern</param>
        /// <param name="iterazione">Number of the iteration where the shift pattern has been generated</param>
        private void PrintSingleShiftPattern(bool[,] shiftPatternMatrix, int iterazione)
        {
            int count = 0;
            Console.Write($"Node {iterazione}:\n");
            PrintDaysNumAndName();
            for (int i = 0; i < numShiftTypes; i++)
            {
                for (int j = 0; j < numOfDays; j++)
                {
                    if (shiftPatternMatrix[i, j]) count++;
                    Console.Write((shiftPatternMatrix[i, j]) ? "1\t" : "0\t");
                }
                Console.Write("\n");
            }
            Console.Write($"Number of assigned shifts: {count}\n");
        }
        /// <summary>
        /// Shift pattern print procedure overload for shift pattern matrix with file writing
        /// </summary>
        /// <param name="shiftPattern">Boolean array which contains the randomly generated shift pattern</param>
        /// <param name="iterazione">Number of the iteration where the shift pattern has been generated</param>
        /// <param name="fileStream">Strem writer for the file writing</param>
        private void PrintSingleShiftPattern(bool[,] shiftPatternMatrix, int iterazione, StreamWriter fileStream)
        {
            int count = 0;
            WriteOnFileAndConsole($"Node {iterazione}:\n", fileStream);
            PrintDaysNumAndName(fileStream);
            for (int i = 0; i < numShiftTypes; i++)
            {
                for (int j = 0; j < numOfDays; j++)
                {
                    if (shiftPatternMatrix[i, j]) count++;
                    WriteOnFileAndConsole((shiftPatternMatrix[i, j]) ? "1\t" : "0\t", fileStream);
                }
                WriteOnFileAndConsole("\n", fileStream);
            }
            WriteOnFileAndConsole($"Number of assigned shifts: {count}\n", fileStream);
        }
        private void PrintSingleShiftPattern(List<int> shiftPatternSparse, int iterazione, StreamWriter fileStream)
        {
            int count = 0;
            WriteOnFileAndConsole($"Node {iterazione}:\n", fileStream);
            PrintDaysNumAndName(fileStream);
            for (int j = 0; j < numShiftTypes; j++)
            {
                for (int i = 0; i < numOfDays; i++)
                {
                    int valueToSearch = (i * numShiftTypes) + j;
                    if (shiftPatternSparse.Contains(valueToSearch))
                    {
                        WriteOnFileAndConsole("1\t", fileStream);
                        count++;
                    }
                    else WriteOnFileAndConsole("0\t", fileStream);

                }
                WriteOnFileAndConsole("\n", fileStream);
            }
            WriteOnFileAndConsole($"Number of assigned shifts: {count}\n", fileStream);
        }
        /// <summary>
        /// Print the number and the name of the days in the analyzed period
        /// </summary>
        private void PrintDaysNumAndName()
        {
            DateTime startDay = Convert.ToDateTime(InputData.StartDate);
            for (int i = 1; i <= numOfDays; i++)
            {
                Console.Write($"{i}\t");
            }
            Console.Write($"\n\n");
            for (int j = 0; j < numOfDays; j++)
            {
                Console.Write($"{startDay.DayOfWeek.ToString().First()}\t");
                startDay = startDay.AddDays(1);
            }
            Console.Write($"\n");
        }
        /// <summary>
        /// Print the number and the name of the days in the analyzed period 
        /// </summary>
        private void PrintDaysNumAndName(StreamWriter fileStream)
        {
            DateTime startDay = Convert.ToDateTime(InputData.StartDate);
            for (int i = 1; i <= numOfDays; i++)
            {
                WriteOnFileAndConsole($"{i}\t", fileStream);
            }
            WriteOnFileAndConsole($"\n\n", fileStream);
            for (int j = 0; j < numOfDays; j++)
            {
                WriteOnFileAndConsole($"{startDay.DayOfWeek.ToString().First()}\t", fileStream);
                startDay = startDay.AddDays(1);
            }
            WriteOnFileAndConsole($"\n", fileStream);
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
                    PrintSingleShiftPattern(node.ShiftPatternMatrix, j);
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
        private void RemoveConsecutiveDays(ref int comparisonValue, List<int> indexesList, List<int> removedIndexesList, List<int> activeIndexesList, bool enableViolation)
        {
            //The analisys take place only if the list of indexes is not empty
            if (!(indexesList.Count == 0))
            {
                //Limit value used to anticipate a possible violation
                int limitViolation;
                if (enableViolation) limitViolation = numShiftTypes * (maxConsWorkDays + maxConsViolation);
                else limitViolation = numShiftTypes * maxConsWorkDays;
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
        private bool listContainsPattern(List<Node> nodesSet, List<int> pattern)
        {
            foreach (Node node in nodesSet)
            {
                if (PatternsAreEqual(node.ShiftPatternSparse, pattern))
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
        private bool PatternsAreEqual(List<int> pattern1, List<int> pattern2)
        {
            if (pattern1.Count != pattern2.Count)
                return false;

            for (int i = 0; i < pattern1.Count; i++)
            {
                if (pattern1.ElementAt(i) != pattern2.ElementAt(i))
                    return false;
            }
            return true;
        }
        /// <summary>
        /// Binomila coefficent function
        /// </summary>
        /// <param name="N"></param>
        /// <param name="K"></param>
        /// <returns>Risultato del calcalo</returns>
        private decimal BinomialCoefficentCalc(int N, int K)
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
        private List<int> RandomShiftAssigner(Random rnd, List<int> indexesList, bool[] baseShiftPattern, List<int> shiftPatternSparse)
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
                shiftPatternSparse.Add(shiftPatterIndex);
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
