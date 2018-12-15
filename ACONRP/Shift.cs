using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACONRP
{
    public class Shift
    {
        int id;
        String startTime;
        String endTime;
        String description;
        public String type;
        public List<String> requiredSkills = new List<String>();


        public Shift(String st, String et, String t, List<String> s, String desc)
        {
            startTime = st;
            endTime = et;
            type = t;
            requiredSkills = new List<string>(s);
            description = desc;
        }

        public List<String> getSkills()
        {
            return requiredSkills;
        }

        public String getType()
        {
            return type;
        }
        public override string ToString()
        {
            String @out = "Shift Type: " + this.type + "\n" +
                         "description : " + this.description + "\n" +
                         "start time: " + this.startTime + "\n" +
                         "end time: " + this.endTime + "\n" +
                         "skills : ";

            foreach (String s in this.requiredSkills)
            {
                @out += s + " ";
            }
            return @out;

        }

    }
}
