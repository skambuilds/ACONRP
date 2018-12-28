using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ACONRP
{
    public class InputData
    {
        public static SchedulingPeriod GetObjectDataFromText(string stringData)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(SchedulingPeriod));
            SchedulingPeriod result = null;
            using (TextReader reader = new StringReader(stringData))
            {
                result = (SchedulingPeriod)serializer.Deserialize(reader);
            }

            return result;
        }
        public static SchedulingPeriod GetObjectDataFromFile(string path)
        {
            return GetObjectDataFromText(File.ReadAllText(path));
        }

    }

    [XmlRoot(ElementName = "Skills")]
    public class Skills
    {
        [XmlElement(ElementName = "Skill")]
        public List<string> Skill { get; set; }
    }

    [XmlRoot(ElementName = "Shift")]
    public class Shift
    {
        [XmlElement(ElementName = "StartTime")]
        public string StartTime { get; set; }
        [XmlElement(ElementName = "EndTime")]
        public string EndTime { get; set; }
        [XmlElement(ElementName = "Description")]
        public string Description { get; set; }
        [XmlElement(ElementName = "Skills")]
        public Skills Skills { get; set; }
        [XmlAttribute(AttributeName = "ID")]
        public string ID { get; set; }
    }

    [XmlRoot(ElementName = "ShiftTypes")]
    public class ShiftTypes
    {
        [XmlElement(ElementName = "Shift")]
        public List<Shift> Shift { get; set; }
    }

    [XmlRoot(ElementName = "PatternEntry")]
    public class PatternEntry
    {
        [XmlElement(ElementName = "ShiftType")]
        public string ShiftType { get; set; }
        [XmlElement(ElementName = "Day")]
        public string Day { get; set; }
        [XmlAttribute(AttributeName = "index")]
        public string Index { get; set; }
    }

    [XmlRoot(ElementName = "PatternEntries")]
    public class PatternEntries
    {
        [XmlElement(ElementName = "PatternEntry")]
        public List<PatternEntry> PatternEntry { get; set; }
    }

    [XmlRoot(ElementName = "Pattern")]
    public class Pattern
    {
        [XmlElement(ElementName = "PatternEntries")]
        public PatternEntries PatternEntries { get; set; }
        [XmlAttribute(AttributeName = "ID")]
        public string ID { get; set; }
        [XmlAttribute(AttributeName = "weight")]
        public string Weight { get; set; }
    }

    [XmlRoot(ElementName = "Patterns")]
    public class Patterns
    {
        [XmlElement(ElementName = "Pattern")]
        public List<Pattern> Pattern { get; set; }
    }

    [XmlRoot(ElementName = "SingleAssignmentPerDay")]
    public class SingleAssignmentPerDay
    {
        [XmlAttribute(AttributeName = "weight")]
        public string Weight { get; set; }
        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "MaxNumAssignments")]
    public class MaxNumAssignments
    {
        [XmlAttribute(AttributeName = "on")]
        public string On { get; set; }
        [XmlAttribute(AttributeName = "weight")]
        public string Weight { get; set; }
        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "MinNumAssignments")]
    public class MinNumAssignments
    {
        [XmlAttribute(AttributeName = "on")]
        public string On { get; set; }
        [XmlAttribute(AttributeName = "weight")]
        public string Weight { get; set; }
        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "MaxConsecutiveWorkingDays")]
    public class MaxConsecutiveWorkingDays
    {
        [XmlAttribute(AttributeName = "on")]
        public string On { get; set; }
        [XmlAttribute(AttributeName = "weight")]
        public string Weight { get; set; }
        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "MinConsecutiveWorkingDays")]
    public class MinConsecutiveWorkingDays
    {
        [XmlAttribute(AttributeName = "on")]
        public string On { get; set; }
        [XmlAttribute(AttributeName = "weight")]
        public string Weight { get; set; }
        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "MaxConsecutiveFreeDays")]
    public class MaxConsecutiveFreeDays
    {
        [XmlAttribute(AttributeName = "on")]
        public string On { get; set; }
        [XmlAttribute(AttributeName = "weight")]
        public string Weight { get; set; }
        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "MinConsecutiveFreeDays")]
    public class MinConsecutiveFreeDays
    {
        [XmlAttribute(AttributeName = "on")]
        public string On { get; set; }
        [XmlAttribute(AttributeName = "weight")]
        public string Weight { get; set; }
        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "MaxConsecutiveWorkingWeekends")]
    public class MaxConsecutiveWorkingWeekends
    {
        [XmlAttribute(AttributeName = "on")]
        public string On { get; set; }
        [XmlAttribute(AttributeName = "weight")]
        public string Weight { get; set; }
        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "MinConsecutiveWorkingWeekends")]
    public class MinConsecutiveWorkingWeekends
    {
        [XmlAttribute(AttributeName = "on")]
        public string On { get; set; }
        [XmlAttribute(AttributeName = "weight")]
        public string Weight { get; set; }
        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "MaxWorkingWeekendsInFourWeeks")]
    public class MaxWorkingWeekendsInFourWeeks
    {
        [XmlAttribute(AttributeName = "on")]
        public string On { get; set; }
        [XmlAttribute(AttributeName = "weight")]
        public string Weight { get; set; }
        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "CompleteWeekends")]
    public class CompleteWeekends
    {
        [XmlAttribute(AttributeName = "weight")]
        public string Weight { get; set; }
        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "IdenticalShiftTypesDuringWeekend")]
    public class IdenticalShiftTypesDuringWeekend
    {
        [XmlAttribute(AttributeName = "weight")]
        public string Weight { get; set; }
        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "NoNightShiftBeforeFreeWeekend")]
    public class NoNightShiftBeforeFreeWeekend
    {
        [XmlAttribute(AttributeName = "weight")]
        public string Weight { get; set; }
        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "AlternativeSkillCategory")]
    public class AlternativeSkillCategory
    {
        [XmlAttribute(AttributeName = "weight")]
        public string Weight { get; set; }
        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "UnwantedPatterns")]
    public class UnwantedPatterns
    {
        [XmlElement(ElementName = "Pattern")]
        public List<string> Pattern { get; set; }
    }

    [XmlRoot(ElementName = "Contract")]
    public class Contract
    {
        [XmlElement(ElementName = "Description")]
        public string Description { get; set; }
        [XmlElement(ElementName = "SingleAssignmentPerDay")]
        public SingleAssignmentPerDay SingleAssignmentPerDay { get; set; }
        [XmlElement(ElementName = "MaxNumAssignments")]
        public MaxNumAssignments MaxNumAssignments { get; set; }
        [XmlElement(ElementName = "MinNumAssignments")]
        public MinNumAssignments MinNumAssignments { get; set; }
        [XmlElement(ElementName = "MaxConsecutiveWorkingDays")]
        public MaxConsecutiveWorkingDays MaxConsecutiveWorkingDays { get; set; }
        [XmlElement(ElementName = "MinConsecutiveWorkingDays")]
        public MinConsecutiveWorkingDays MinConsecutiveWorkingDays { get; set; }
        [XmlElement(ElementName = "MaxConsecutiveFreeDays")]
        public MaxConsecutiveFreeDays MaxConsecutiveFreeDays { get; set; }
        [XmlElement(ElementName = "MinConsecutiveFreeDays")]
        public MinConsecutiveFreeDays MinConsecutiveFreeDays { get; set; }
        [XmlElement(ElementName = "MaxConsecutiveWorkingWeekends")]
        public MaxConsecutiveWorkingWeekends MaxConsecutiveWorkingWeekends { get; set; }
        [XmlElement(ElementName = "MinConsecutiveWorkingWeekends")]
        public MinConsecutiveWorkingWeekends MinConsecutiveWorkingWeekends { get; set; }
        [XmlElement(ElementName = "MaxWorkingWeekendsInFourWeeks")]
        public MaxWorkingWeekendsInFourWeeks MaxWorkingWeekendsInFourWeeks { get; set; }
        [XmlElement(ElementName = "WeekendDefinition")]
        public string WeekendDefinition { get; set; }
        [XmlElement(ElementName = "CompleteWeekends")]
        public CompleteWeekends CompleteWeekends { get; set; }
        [XmlElement(ElementName = "IdenticalShiftTypesDuringWeekend")]
        public IdenticalShiftTypesDuringWeekend IdenticalShiftTypesDuringWeekend { get; set; }
        [XmlElement(ElementName = "NoNightShiftBeforeFreeWeekend")]
        public NoNightShiftBeforeFreeWeekend NoNightShiftBeforeFreeWeekend { get; set; }
        [XmlElement(ElementName = "AlternativeSkillCategory")]
        public AlternativeSkillCategory AlternativeSkillCategory { get; set; }
        [XmlElement(ElementName = "UnwantedPatterns")]
        public UnwantedPatterns UnwantedPatterns { get; set; }
        [XmlAttribute(AttributeName = "ID")]
        public string ID { get; set; }
    }

    [XmlRoot(ElementName = "Contracts")]
    public class Contracts
    {
        [XmlElement(ElementName = "Contract")]
        public List<Contract> Contract { get; set; }
    }

    [XmlRoot(ElementName = "Employee")]
    public class Employee
    {
        [XmlElement(ElementName = "ContractID")]
        public string ContractID { get; set; }
        [XmlElement(ElementName = "Name")]
        public string Name { get; set; }
        [XmlElement(ElementName = "Skills")]
        public Skills Skills { get; set; }
        [XmlAttribute(AttributeName = "ID")]
        public string ID { get; set; }
    }

    [XmlRoot(ElementName = "Employees")]
    public class Employees
    {
        [XmlElement(ElementName = "Employee")]
        public List<Employee> Employee { get; set; }
    }

    [XmlRoot(ElementName = "Cover")]
    public class Cover
    {
        [XmlElement(ElementName = "Skill")]
        public string Skill { get; set; }
        [XmlElement(ElementName = "Shift")]
        public string Shift { get; set; }
        [XmlElement(ElementName = "Preferred")]
        public string Preferred { get; set; }
    }

    [XmlRoot(ElementName = "DayOfWeekCover")]
    public class DayOfWeekCover
    {
        [XmlElement(ElementName = "Day")]
        public string Day { get; set; }
        [XmlElement(ElementName = "Cover")]
        public List<Cover> Cover { get; set; }
    }

    [XmlRoot(ElementName = "CoverRequirements")]
    public class CoverRequirements
    {
        [XmlElement(ElementName = "DayOfWeekCover")]
        public List<DayOfWeekCover> DayOfWeekCover { get; set; }
    }

    [XmlRoot(ElementName = "DayOff")]
    public class DayOff
    {
        [XmlElement(ElementName = "EmployeeID")]
        public string EmployeeID { get; set; }
        [XmlElement(ElementName = "Date")]
        public string Date { get; set; }
        [XmlAttribute(AttributeName = "weight")]
        public string Weight { get; set; }
    }

    [XmlRoot(ElementName = "DayOffRequests")]
    public class DayOffRequests
    {
        [XmlElement(ElementName = "DayOff")]
        public List<DayOff> DayOff { get; set; }
    }

    [XmlRoot(ElementName = "ShiftOff")]
    public class ShiftOff
    {
        [XmlElement(ElementName = "ShiftTypeID")]
        public string ShiftTypeID { get; set; }
        [XmlElement(ElementName = "EmployeeID")]
        public string EmployeeID { get; set; }
        [XmlElement(ElementName = "Date")]
        public string Date { get; set; }
        [XmlAttribute(AttributeName = "weight")]
        public string Weight { get; set; }
    }

    [XmlRoot(ElementName = "ShiftOffRequests")]
    public class ShiftOffRequests
    {
        [XmlElement(ElementName = "ShiftOff")]
        public List<ShiftOff> ShiftOff { get; set; }
    }

    [XmlRoot(ElementName = "SchedulingPeriod")]
    public class SchedulingPeriod
    {
        [XmlElement(ElementName = "StartDate")]
        public string StartDate { get; set; }
        [XmlElement(ElementName = "EndDate")]
        public string EndDate { get; set; }
        [XmlElement(ElementName = "Skills")]
        public Skills Skills { get; set; }
        [XmlElement(ElementName = "ShiftTypes")]
        public ShiftTypes ShiftTypes { get; set; }
        [XmlElement(ElementName = "Patterns")]
        public Patterns Patterns { get; set; }
        [XmlElement(ElementName = "Contracts")]
        public Contracts Contracts { get; set; }
        [XmlElement(ElementName = "Employees")]
        public Employees Employees { get; set; }
        [XmlElement(ElementName = "CoverRequirements")]
        public CoverRequirements CoverRequirements { get; set; }
        [XmlElement(ElementName = "DayOffRequests")]
        public DayOffRequests DayOffRequests { get; set; }
        [XmlElement(ElementName = "ShiftOffRequests")]
        public ShiftOffRequests ShiftOffRequests { get; set; }
        [XmlAttribute(AttributeName = "ID")]
        public string ID { get; set; }
        [XmlAttribute(AttributeName = "xsi", Namespace = "http://www.w3.org/2000/xmlns/")]
        public string Xsi { get; set; }
        [XmlAttribute(AttributeName = "noNamespaceSchemaLocation", Namespace = "http://www.w3.org/2001/XMLSchema-instance")]
        public string NoNamespaceSchemaLocation { get; set; }
    }


}

