using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACONRP
{
    public class ACOHandler
    {
        public GenerationManager GenerationManager { get; set; }
        public SchedulingPeriod InputData { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<int[,]> CoverRequirements { get; set; }
        public Dictionary<string, int> GradeIndex = new Dictionary<string, int>();
        public Dictionary<string, int> ShiftPatternIndex = new Dictionary<string, int>();
        public Dictionary<string, List<int>> DayIndex = new Dictionary<string, List<int>>
         {
           {"Monday", new List<int>() },
           {"Tuesday", new List<int>() },
           {"Wednesday", new List<int>() },
           {"Thursday", new List<int>() },
           {"Friday", new List<int>() },
           {"Saturday", new List<int>() },
           {"Sunday", new List<int>() }
        };

        public Dictionary<string, int> SordedDays = new Dictionary<string, int>() {
            { "Monday", 0 }, {"Tuesday", 1 }, {"Wednesday", 2 }, {"Thursday", 3 }, {"Friday", 4 }, {"Saturday", 5 }, {"Sunday", 6 }
        };

        public double[] WParam { get; set; }
        public int NurseNumber { get; set; }


        public ACOHandler(SchedulingPeriod inputData)
        {
            InputData = inputData;
            StartDate = DateTime.Parse(InputData.StartDate);
            EndDate = DateTime.Parse(InputData.EndDate);

            NurseNumber = inputData.Employees.Employee.Count;
            GenerationManager = new GenerationManager(InputData);

            WParam = new double[inputData.Skills.Skill.Count];
            for (int i = 0; i < inputData.Skills.Skill.Count; i++)
            {
                GradeIndex.Add(inputData.Skills.Skill.ElementAt(i), i);
                WParam[i] = i + 1;
            }

            var orderedShifts = inputData.ShiftTypes.Shift.OrderBy(x => DateTime.Parse(x.StartTime)).ThenBy(x => DateTime.Parse(x.EndTime)).ToList();
            orderedShifts.ForEach(x => ShiftPatternIndex.Add(x.ID, orderedShifts.IndexOf(x)));

            for (int i = 0; i < EndDate.Subtract(StartDate).Days; i++)
            {
                DayIndex[StartDate.AddDays(i).DayOfWeek.ToString()].Add(i);
            }
        }

        public void ComputeStaticHeuristic(List<Node>[] nodes)
        {
            var contracts = InputData.Contracts.Contract;
            var patterns = InputData.Patterns.Pattern;
            var nurses = InputData.Employees.Employee;

            foreach (var nurse in nurses)
            {
                var unwantedPatterns = contracts.FirstOrDefault(contract => contract.ID == nurse.ContractID).UnwantedPatterns.Pattern;
                var unwantedContractPatterns = patterns.Where(pattern => unwantedPatterns.Contains(pattern.ID)).ToList();

                var unwantedShift = unwantedContractPatterns.Where(x => x.PatternEntries.PatternEntry.Any(y => y.ShiftType.Length == 1)).ToList();
                var unwantedDays = unwantedContractPatterns.Where(x => x.PatternEntries.PatternEntry.Any(y => y.ShiftType.Length > 1)).ToList();

                var nurseIndex = nurses.IndexOf(nurse);

                var costsUnwantedShift = HandleUnwantedShift(unwantedShift, nodes[nurseIndex]);
                var costsUnwantedDay = HandleUnwantedDays(unwantedDays, nodes[nurseIndex]);

                var costs = new List<double>();
                for (int i = 0; i < costsUnwantedDay.Count; i++)
                {
                    costs.Add(costsUnwantedShift.ElementAt(i) + costsUnwantedDay.ElementAt(i));
                }

                for (int i = 0; i < nodes[nurseIndex].Count; i++)
                {
                    //assegnazione sul nodo del valore euristico calcolato
                    //nursesPatterns[index].ElementAt(i) = costs.ElementAt(i);
                }
            }

        }

        private List<double> HandleUnwantedDays(List<Pattern> unwantedDays, List<Node> nodes)
        {
            List<double> costs = new List<double>();

            foreach (var node in nodes)
            {
                double totalCost = 0;
                foreach (var pattern in unwantedDays)
                {
                    var weight = double.Parse(pattern.Weight);

                    var occurences = CheckDays(pattern.PatternEntries.PatternEntry, node.ShiftPattern);
                    totalCost += weight * occurences;
                }
                costs.Add(totalCost);
            }

            return costs;
        }

        private int CheckDays(List<PatternEntry> patternEntries, bool[,] nursePattern)
        {
            int startWeekIndex = 0;
            DateTime startWeekDate = new DateTime(StartDate.Ticks);
            int daysUntilSunday = ((int)DayOfWeek.Sunday - (int)StartDate.DayOfWeek + 7) % 7;

            int endWeekIndex = daysUntilSunday + startWeekIndex;

            DateTime endWeekDate = startWeekDate.AddDays(daysUntilSunday);

            var days = new List<String>();
            patternEntries.ForEach(x => days.Add(x.Day));

            if (!CheckDaysOnInterval(startWeekDate, endWeekDate, days))
            {
                //moving to the next week
                startWeekDate = endWeekDate.AddDays(1);
                startWeekIndex = endWeekIndex + 1;
                endWeekIndex = startWeekIndex + 6;
                endWeekDate = startWeekDate.AddDays(6);
            }

            var patternOccurences = 0;
            do
            {
                foreach (var patternEntry in patternEntries)
                {
                    int? indexDay = DayIndex[patternEntry.Day].Where(x => x >= startWeekIndex && x <= endWeekIndex).FirstOrDefault();
                    if (!indexDay.HasValue)
                        break;

                    if (patternEntry.ShiftType == "Any" && !Utils.GetColumn<bool>(nursePattern, indexDay.Value).Contains(true))
                        break;

                    if (patternEntry.ShiftType == "None" && Utils.GetColumn<bool>(nursePattern, indexDay.Value).Contains(true))
                        break;

                    patternOccurences++;
                }

                startWeekDate = endWeekDate.AddDays(1);
                startWeekIndex = endWeekIndex + 1;
                endWeekIndex = startWeekIndex + 6;
                endWeekDate = startWeekDate.AddDays(6);
            } while (endWeekDate <= EndDate);

            return patternOccurences;
        }

        private int GetDaysTo(DateTime startDate, string day)
        {
            for (int i = 0; i < 7; i++)
            {
                if (startDate.AddDays(i).DayOfWeek.ToString() == day)
                    return i;
            }

            return 7;
        }



        private bool CheckDaysOnInterval(DateTime startDate, DateTime endDate, List<String> days)
        {
            foreach (var day in days)
            {
                bool dayFound = false;
                for (int i = 0; i < endDate.Subtract(startDate).Days + 1; i++)
                {
                    if (startDate.AddDays(i).DayOfWeek.ToString().ToLower() == day.ToLower())
                    {
                        dayFound = true;
                        break;
                    }
                }
                if (!dayFound)
                    return false;
            }

            return true;
        }

        public List<double> HandleUnwantedShift(List<Pattern> unwantedShift, List<Node> nodes)
        {
            List<double> costs = new List<double>();

            List<UnwantedShift> assignmentPosition = new List<UnwantedShift>();
            foreach (var node in nodes)
            {

                double totalCost = 0;

                foreach (var pattern in unwantedShift)
                {
                    UnwantedShift shift = new UnwantedShift(pattern, ShiftPatternIndex);
                    totalCost += shift.Check(node.ShiftPattern);
                }

                costs.Add(totalCost);
            }

            return costs;
        }
    }

    public class UnwantedShift
    {
        public List<int> AssignmentPosition { get; set; } = new List<int>();
        public bool IsTrue { get; set; }
        public bool IsCheked { get; set; }
        public double Weight { get; set; }

        public double Check(bool[,] nursePattern)
        {
            int patternOccurences = 0;
            for (int i = 0; i < nursePattern.GetLength(1); i++)
            {
                for (int j = 0; j < AssignmentPosition.Count && i + j < nursePattern.GetLength(1); j++)
                {
                    IsCheked = false;
                    if (!nursePattern[AssignmentPosition.ElementAt(j), i + j])
                    {
                        IsCheked = true;
                    }
                    else
                    {
                        IsCheked = j < AssignmentPosition.Count-1;
                        continue;
                    }

                    if (IsCheked)
                        break;
                }

                if (!IsCheked)
                {
                    patternOccurences++;
                    IsCheked = false;
                }
            }
            IsCheked = true;
            IsTrue = patternOccurences > 0;
            return (IsTrue) ? Weight * patternOccurences : 0;
        }

        public UnwantedShift(Pattern pattern, Dictionary<string, int> shiftPatternIndex)
        {
            Weight = double.Parse(pattern.Weight);

            var i = 0;
            foreach (var patternEntry in pattern.PatternEntries.PatternEntry)
            {
                var position = shiftPatternIndex[patternEntry.ShiftType];


                if (patternEntry.Day == "None")//TODO: This case is not contemplated in the instances but its handling could be required in the Check method
                    position *= -1;

                AssignmentPosition.Add(position);
                i++;
            }
        }
    }
}
