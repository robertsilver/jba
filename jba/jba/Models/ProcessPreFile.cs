using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace jba.Models
{
    public abstract class ProcessFile
    {
        private Regex whitespace = new Regex(@"\s+");

        public string ReplaceWhitespace(string input, string replacement)
        {
            return whitespace.Replace(input, replacement);
        }

        public abstract double[] ReadFile(string line);

        public abstract List<int> ReadDataLine(string line);

        public double[] ReadStandardHeader(string line, string headerItem, char splitValue = ',')
        {
            double[] headerValues = new double[2];
            var tempLine = ReplaceWhitespace(line, "");

            var posnOfHeaderItem = tempLine.IndexOf(headerItem);
            if (posnOfHeaderItem == -1)
                return headerValues;

            var posnOfEndBrack = tempLine.IndexOf("]", posnOfHeaderItem);
            var values = tempLine.Substring(posnOfHeaderItem + headerItem.Length, posnOfEndBrack - posnOfHeaderItem - headerItem.Length).Split(splitValue);

            headerValues[0] = Convert.ToDouble(values[0]);
            headerValues[1] = Convert.ToDouble(values[1]);

            return headerValues;
        }
    }
}