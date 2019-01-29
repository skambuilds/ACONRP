using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACONRP.Evaluator
{
    public class AlternativeSkillCondition : Condition
    {
        public AlternativeSkillCondition(int numUnits, int numDays, Contract contract)
        {
            this.name = "ALTERNATIVE SKILL CONDITION";
            this.BuildNumbering(numUnits, numDays, contract);
        }

        protected override void BuildNumbering(int numUnits, int numDays, Contract contract)
        {
            //this.numbering = new int[numUnits * numDays];

            //// INITIALISE PERTS
            //max_pert_nr = new int[numUnits * numDays];
            //max_pert_nr_nonevent = new int[numUnits * numDays];
            //min_pert_nr = new int[numUnits * numDays];
            //min_pert_nr_nonevent = new int[numUnits * numDays];
            //cost_max_pert_nr = new int[numUnits * numDays];
            //cost_min_pert_nr = new int[numUnits * numDays];

            //for (int i = 0; i != numUnits * numDays; i++)
            //{
            //    this.numbering[i] = Condition.Undefined;
            //}

            //// INITIALISE COST OF CONDITION
            //SchedulingPeriod sp = this.getEmployee().getSchedulingPeriod();

            //int nr = 0;
            ////assicurarsi che funzioni text
            //if (Convert.ToBoolean(contract.AlternativeSkillCategory.Text))
            //{
            //    for (int i = 0; i != numUnits * numDays; i++)
            //    {
            //        int stIndex = i % numUnits;
            //        ShiftType st = sp.getShiftTypeByIndex(stIndex);

            //        // + CHANGE EMPLOYEE SKILL CONTENTS

            //        bool hasSkill = true;
            //        for (Skill skill : st.skills.values())
            //        {
            //            if (!this.getEmployee().hasSkill(skill))
            //            {
            //                hasSkill = false;
            //            }
            //        }

            //        if (!hasSkill)
            //        {
            //            this.numbering[i] = nr;
            //            cost_max_pert_nr[nr] = Convert.ToInt16(contract.AlternativeSkillCategory.Weight);
            //            max_pert_nr[nr++] = 0;
            //        }
            //    }

            //    max_pert_string = "Alternative skill";

            //    this.ENABLED = true;

            }

        protected override void BuildNumbering(int numUnits, int numDays, Contract contract, List<DayOff> dayOffRequests, DateTime startDate)
        {
            throw new NotImplementedException();
        }

        protected override void BuildNumbering(int numUnits, int numDays, Contract contract, List<ShiftOff> shiftOffRequests, Dictionary<string, int> shiftTypesDict, DateTime startDate)
        {
            throw new NotImplementedException();
        }
    }
}