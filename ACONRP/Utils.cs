using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACONRP
{
    public static class Utils
    {
        public static T[] GetRow<T>(T[,] matrix, int row)
        {
            var columns = matrix.GetLength(1);
            var array = new T[columns];
            for (int i = 0; i < columns; ++i)
                array[i] = matrix[row, i];
            return array;
        }

        public static T[] GetColumn<T>(T[,] matrix, int column)
        {
            var rows = matrix.GetLength(0);
            var array = new T[rows];
            for (int i = 0; i < rows; ++i)
                array[i] = matrix[i, column];
            return array;
        }

        private static void NodesOrderInverter(List<Node>[] nodes)
        {
            List<Node>[] localNodes = (List<Node>[])nodes.Clone();

            int i = 0;
            for (int j = localNodes.Length - 1; j >= 0; j--)
            {
                nodes[i] = localNodes[j];
                i++;
            }
        }

        public static int DateOffset(DateTime start, DateTime date)
        {
            TimeSpan interval = ((Convert.ToDateTime(date) - Convert.ToDateTime(start)));
            int nrOfDays = (interval.Days) + 1;

            return nrOfDays;
        }
    }
}
