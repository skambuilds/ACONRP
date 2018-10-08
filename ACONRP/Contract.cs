using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACONRP
{
    public class Contract
    {
        int id;
        String description;
        bool singleassignmentperdaytrue;
        int weight1 = 1;
        int weight2 = 0;

        int maxnumassignments;
        int minnumassignments;

        int maxconsecutiveworkingdays;
        int minconsecutiveworkingdays;

        int maxconsecutivefreedays;
        int minconsecutivefreedays;

        int maxconsecutiveworkingweekends;
        int minconsecutiveworkingweekends;

        int maxworkingweekendsinfourweeks;
        String weekenddefinition;

        bool completeweekends;
        bool identicalshifttypesduringweekend;
        bool nonightshiftbeforefreeweekend;
        bool alternativeskillcategory;

        List<int> unwantedPatterns;

        public Contract(int id, String d)
        {
            description = d;
            this.id = id;
        }

        public void addContraints(String assigmentPD, String maxNA, String minNA, String maxWD, String minWD, String maxFD,
                String minFD, String maxCWW, String minCWW, String MWWIFW, String weekend, String compW,
                String ISTDW, String NNSBFW, String altSkill)
        {

            singleassignmentperdaytrue = bool.Parse(assigmentPD);
            maxnumassignments = int.Parse(maxNA);
            minnumassignments = int.Parse(minNA);

            maxconsecutiveworkingdays = int.Parse(maxWD);
            minconsecutiveworkingdays = int.Parse(minWD);

            maxconsecutivefreedays = int.Parse(maxFD);
            minconsecutivefreedays = int.Parse(minFD);

            maxconsecutiveworkingweekends = int.Parse(maxCWW); ;
            minconsecutiveworkingweekends = int.Parse(minCWW);

            maxworkingweekendsinfourweeks = int.Parse(MWWIFW);
            weekenddefinition = weekend;

            completeweekends = bool.Parse(compW);
            identicalshifttypesduringweekend = bool.Parse(ISTDW);
            nonightshiftbeforefreeweekend = bool.Parse(NNSBFW);
            alternativeskillcategory = bool.Parse(altSkill);
        }

        public void setUnwantedPatterns(List<int> unwantedPatterns)
        {
            this.unwantedPatterns = unwantedPatterns;
        }

        public String getDescription()
        {
            return description;
        }

        public int getMaxconsecutivefreedays()
        {
            return maxconsecutivefreedays;
        }

        public int getMaxconsecutiveworkingweekends()
        {
            return maxconsecutiveworkingweekends;
        }

        public int getMaxworkingweekendsinfourweeks()
        {
            return maxworkingweekendsinfourweeks;
        }

        public int getMaxnumassignments()
        {
            return maxnumassignments;
        }

        public int getId()
        {
            return id;
        }

        public int getMaxconsecutiveworkingdays()
        {
            return maxconsecutiveworkingdays;
        }

        public int getMinconsecutivefreedays()
        {
            return minconsecutivefreedays;
        }

        public int getMinconsecutiveworkingdays()
        {
            return minconsecutiveworkingdays;
        }

        public int getMinconsecutiveworkingweekends()
        {
            return minconsecutiveworkingweekends;
        }

        public int getMinnumassignments()
        {
            return minnumassignments;
        }

        public List<int> getUnwantedPatterns()
        {
            return unwantedPatterns;
        }

        public String getWeekenddefinition()
        {
            return weekenddefinition;
        }

        public int getWeight1()
        {
            return weight1;
        }

        public int getWeight2()
        {
            return weight2;
        }

        public override string ToString()
        {
            return "Contract ID : " + this.id + "\n" +
                         "description : " + this.description + "\n" +
                         "maxnumassignments : " + this.maxnumassignments + "\n" +
                         "minnumassignments : " + this.minnumassignments + "\n" +
                         "maxconsecutiveworkingdays : " + this.maxconsecutiveworkingdays + "\n" +
                         "minconsecutiveworkingdays : " + this.minconsecutiveworkingdays + "\n" +
                         "maxconsecutivefreedays : " + this.maxconsecutivefreedays + "\n" +
                         "minconsecutivefreedays : " + this.minconsecutivefreedays + "\n"
                         ;
        }
    }
}
